using System.Text.Json;
using System.Text.Json.Serialization;
using Sporim.Api.Auth;
using Sporim.Api.Contracts;
using Sporim.Api.State;
using Xunit;

namespace Sporim.Api.Tests;

public sealed class MockApiIntegrationTests
{
    private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void JsonSerialization_EnumsAndDtos_MatchOpenApiContract()
    {
        var store = new MockStore();
        var catalog = store.GetEventCatalog(
            search: null,
            categoryRaw: "pop_culture",
            onlyActive: true,
            statusRaw: null,
            page: 1,
            pageSize: 20,
            currentUserId: MockStore.VanyaUserId).Value!;

        var json = JsonSerializer.Serialize(catalog, WebJsonOptions);
        Assert.Contains("\"category\":\"pop_culture\"", json);
        Assert.Contains("\"status\":\"open\"", json);
        Assert.Contains("\"shortCode\":\"k9x2mP8q\"", json);

        var error = new ApiError(
            ErrorCode.EventClosed,
            "Приём прогнозов на это событие уже закрыт.");
        var errorJson = JsonSerializer.Serialize(error, WebJsonOptions);
        Assert.Contains("\"code\":\"EVENT_CLOSED\"", errorJson);
        Assert.DoesNotContain("fieldErrors", errorJson);
    }

    [Fact]
    public void Auth_EmailRequestAndVerify_ReturnsTokenAndCreatesWelcomeBonusForNewUser()
    {
        var store = new MockStore();
        var tokenService = new MockTokenService();

        var requestCodeResult = store.RequestEmailCode("new_student@sporim.ru");
        Assert.True(requestCodeResult.IsSuccess);
        Assert.Equal(60, requestCodeResult.Value!.CooldownSeconds);

        var verifyInvalidResult = store.VerifyEmailCode("new_student@sporim.ru", "000000");
        Assert.False(verifyInvalidResult.IsSuccess);
        Assert.Equal(400, verifyInvalidResult.StatusCode);
        Assert.Equal(ErrorCode.ValidationError, verifyInvalidResult.Error!.Code);

        var verifyResult = store.VerifyEmailCode("new_student@sporim.ru", "482910");
        Assert.True(verifyResult.IsSuccess);
        var user = verifyResult.Value!;
        Assert.Equal(1000, user.Balance);
        Assert.Equal(Role.User, user.Role);

        var jwt = tokenService.IssueToken(user);
        var validatedId = tokenService.TryValidateAndGetUserId($"Bearer {jwt}", store);
        Assert.Equal(user.Id, validatedId);

        var wallet = store.GetWallet(user.Id).Value!;
        Assert.Equal(1000, wallet.Balance);
        Assert.Single(wallet.Transactions);
        Assert.Equal(LedgerReason.WelcomeBonus, wallet.Transactions[0].Reason);
        Assert.Equal(1000, wallet.Transactions[0].AmountDelta);
    }

    [Fact]
    public void Events_GetCatalogAndDetails_ComputesLazyDeadlineAndPopulatesMyPrediction()
    {
        var store = new MockStore();

        // Guest catalog request
        var guestCatalog = store.GetEventCatalog(
            search: null,
            categoryRaw: null,
            onlyActive: false,
            statusRaw: null,
            page: 1,
            pageSize: 20,
            currentUserId: null);

        Assert.True(guestCatalog.IsSuccess);
        Assert.All(guestCatalog.Value!.Items, item => Assert.Null(item.MyPrediction));

        // Streamers Battle event has acceptUntil in the past -> lazy status must be awaiting_result
        var streamersCard = Assert.Single(
            guestCatalog.Value.Items,
            e => e.Id == MockStore.StreamersBattleEventId);
        Assert.Equal(EventStatus.AwaitingResult, streamersCard.Status);

        // Authenticated catalog request for Vanya populates myPrediction on GTA VI event
        var vanyaCatalog = store.GetEventCatalog(
            search: "GTA",
            categoryRaw: "pop_culture",
            onlyActive: true,
            statusRaw: null,
            page: 1,
            pageSize: 20,
            currentUserId: MockStore.VanyaUserId);

        Assert.True(vanyaCatalog.IsSuccess);
        var gtaCard = Assert.Single(vanyaCatalog.Value!.Items);
        Assert.Equal(MockStore.GtaTrailerEventId, gtaCard.Id);
        Assert.NotNull(gtaCard.MyPrediction);
        Assert.Equal("k9x2mP8q", gtaCard.MyPrediction!.ShortCode);
    }

    [Fact]
    public void Predictions_CreatePrediction_HandlesIdempotencyAndPreventsDoubleDeduction_ScenariosA5AndA6()
    {
        var store = new MockStore();
        var idempotencyKey = Guid.NewGuid().ToString();
        var initialBalance = store.GetWallet(MockStore.VanyaUserId).Value!.Balance;

        var request = new CreatePredictionRequest(
            OutcomeId: MockStore.GrandFinalYesOutcomeId,
            Stake: 250,
            Thesis: "Команда А не проиграла ни одной карты в верхней сетке.",
            EstimatedProbability: 70);

        // First submission -> 201 Created, deducts 250 points
        var firstAttempt = store.CreatePrediction(
            eventId: MockStore.GrandFinalEventId,
            userId: MockStore.VanyaUserId,
            idempotencyKeyRaw: idempotencyKey,
            request: request);

        Assert.True(firstAttempt.IsSuccess);
        Assert.Equal(201, firstAttempt.StatusCode);
        Assert.False(firstAttempt.Value!.IsDuplicateReplay);
        Assert.Equal(initialBalance - 250, firstAttempt.Value.UpdatedBalance);
        var createdTimestamp = firstAttempt.Value.Prediction.CreatedAt;
        var createdShortCode = firstAttempt.Value.Prediction.ShortCode;

        // Scenario A5: retry with the SAME Idempotency-Key after simulated network drop -> 200 OK, no second deduction
        var retryAttempt = store.CreatePrediction(
            eventId: MockStore.GrandFinalEventId,
            userId: MockStore.VanyaUserId,
            idempotencyKeyRaw: idempotencyKey,
            request: request);

        Assert.True(retryAttempt.IsSuccess);
        Assert.Equal(200, retryAttempt.StatusCode);
        Assert.True(retryAttempt.Value!.IsDuplicateReplay);
        Assert.Equal(initialBalance - 250, retryAttempt.Value.UpdatedBalance);
        Assert.Equal(createdTimestamp, retryAttempt.Value.Prediction.CreatedAt);
        Assert.Equal(createdShortCode, retryAttempt.Value.Prediction.ShortCode);

        // Scenario A6: second attempt with a DIFFERENT Idempotency-Key on the same event -> 200 OK, returns existing prediction
        var secondKeyAttempt = store.CreatePrediction(
            eventId: MockStore.GrandFinalEventId,
            userId: MockStore.VanyaUserId,
            idempotencyKeyRaw: Guid.NewGuid().ToString(),
            request: new CreatePredictionRequest(MockStore.GrandFinalNoOutcomeId, 500));

        Assert.True(secondKeyAttempt.IsSuccess);
        Assert.Equal(200, secondKeyAttempt.StatusCode);
        Assert.True(secondKeyAttempt.Value!.IsDuplicateReplay);
        Assert.Equal(initialBalance - 250, secondKeyAttempt.Value.UpdatedBalance);
        Assert.Equal(MockStore.GrandFinalYesOutcomeId, secondKeyAttempt.Value.Prediction.OutcomeId);
    }

    [Fact]
    public void Predictions_CreatePrediction_Returns409EventClosedWhenDeadlinePassed_ScenarioA4()
    {
        var store = new MockStore();

        // Expire deadline on GrandFinalEventId
        store.ExpireEventDeadlineForDemo(MockStore.GrandFinalEventId);

        var result = store.CreatePrediction(
            eventId: MockStore.GrandFinalEventId,
            userId: MockStore.VanyaUserId,
            idempotencyKeyRaw: Guid.NewGuid().ToString(),
            request: new CreatePredictionRequest(MockStore.GrandFinalYesOutcomeId, 100));

        Assert.False(result.IsSuccess);
        Assert.Equal(409, result.StatusCode);
        Assert.Equal(ErrorCode.EventClosed, result.Error!.Code);
    }

    [Fact]
    public void Predictions_CreatePrediction_Returns400InsufficientBalanceWhenStakeExceedsBalance()
    {
        var store = new MockStore();

        var result = store.CreatePrediction(
            eventId: MockStore.GrandFinalEventId,
            userId: MockStore.VanyaUserId,
            idempotencyKeyRaw: Guid.NewGuid().ToString(),
            request: new CreatePredictionRequest(MockStore.GrandFinalYesOutcomeId, 999_999));

        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal(ErrorCode.InsufficientBalance, result.Error!.Code);
        Assert.True(result.Error.FieldErrors!.ContainsKey("stake"));
    }

    [Fact]
    public void Proofs_GetPublicProof_ReturnsProofDataOr404WhenNotFound()
    {
        var store = new MockStore();

        var proofResult = store.GetPublicProof("k9x2mP8q");
        Assert.True(proofResult.IsSuccess);
        Assert.Equal("vanya_analyst", proofResult.Value!.Author.Username);
        Assert.Equal(MockStore.GtaTrailerEventId, proofResult.Value.Event.Id);

        var missingResult = store.GetPublicProof("unknown99");
        Assert.False(missingResult.IsSuccess);
        Assert.Equal(404, missingResult.StatusCode);
        Assert.Equal(ErrorCode.ProofNotFound, missingResult.Error!.Code);
    }

    [Fact]
    public void Admin_DraftCrudAndSettlement_EnforcesPermissionsAndCalculatesPayouts()
    {
        var store = new MockStore();
        var now = DateTimeOffset.UtcNow;

        // Regular user cannot create draft -> 403 FORBIDDEN
        var forbiddenResult = store.CreateDraftEvent(
            MockStore.VanyaUserId,
            new DraftEventRequest(
                Title: "Новый тестовый спор пользователя",
                Slug: null,
                Category: EventCategory.Esports,
                Description: "Описание спора",
                ResolutionRules: "Правила расчёта",
                EventDate: now.AddDays(5),
                AcceptUntil: now.AddDays(4),
                Outcomes:
                [
                    new DraftOutcomeInput("YES", "Да", 1),
                    new DraftOutcomeInput("NO", "Нет", 2)
                ]));

        Assert.False(forbiddenResult.IsSuccess);
        Assert.Equal(403, forbiddenResult.StatusCode);
        Assert.Equal(ErrorCode.Forbidden, forbiddenResult.Error!.Code);

        // Moderator creates draft with acceptUntil > eventDate -> 400 VALIDATION_ERROR
        var invalidDatesResult = store.CreateDraftEvent(
            MockStore.ModeratorUserId,
            new DraftEventRequest(
                Title: "Некорректные даты приёма прогнозов",
                Slug: null,
                Category: EventCategory.Esports,
                Description: "Описание спора",
                ResolutionRules: "Правила расчёта",
                EventDate: now.AddDays(2),
                AcceptUntil: now.AddDays(5),
                Outcomes:
                [
                    new DraftOutcomeInput("YES", "Да", 1),
                    new DraftOutcomeInput("NO", "Нет", 2)
                ]));

        Assert.False(invalidDatesResult.IsSuccess);
        Assert.Equal(400, invalidDatesResult.StatusCode);
        Assert.Equal(ErrorCode.ValidationError, invalidDatesResult.Error!.Code);

        // Moderator creates valid draft -> 201 Created
        var createDraftResult = store.CreateDraftEvent(
            MockStore.ModeratorUserId,
            new DraftEventRequest(
                Title: "Победит ли Team Spirit на следующем турнире?",
                Slug: "team-spirit-next-tournament",
                Category: EventCategory.Esports,
                Description: "Описание турнира",
                ResolutionRules: "По официальному протоколу",
                EventDate: now.AddDays(5),
                AcceptUntil: now.AddDays(4),
                Outcomes:
                [
                    new DraftOutcomeInput("YES", "Да — Победит", 1),
                    new DraftOutcomeInput("NO", "Нет — Не победит", 2)
                ]));

        Assert.True(createDraftResult.IsSuccess);
        Assert.Equal(201, createDraftResult.StatusCode);
        var draftId = createDraftResult.Value!.Id;
        Assert.Equal(EventStatus.Draft, createDraftResult.Value.Status);

        // Moderator publishes draft via PATCH -> status becomes Open
        var publishResult = store.PatchDraftEvent(
            MockStore.ModeratorUserId,
            draftId,
            new PatchDraftEventRequest(Status: EventStatus.Open));

        Assert.True(publishResult.IsSuccess);
        Assert.Equal(EventStatus.Open, publishResult.Value!.Status);

        // Deleting an already published event is rejected -> 409 INVALID_DRAFT_STATE
        var deletePublishedResult = store.DeleteDraftEvent(MockStore.ModeratorUserId, draftId);
        Assert.False(deletePublishedResult.IsSuccess);
        Assert.Equal(409, deletePublishedResult.StatusCode);
        Assert.Equal(ErrorCode.InvalidDraftState, deletePublishedResult.Error!.Code);

        // Vanya places a prediction on the newly published event
        var winningOutcomeId = publishResult.Value.Outcomes[0].Id;
        var predResult = store.CreatePrediction(
            draftId,
            MockStore.VanyaUserId,
            Guid.NewGuid().ToString(),
            new CreatePredictionRequest(winningOutcomeId, 100, "Верю в драконов!", 80));

        Assert.True(predResult.IsSuccess);
        var balanceAfterStake = predResult.Value!.UpdatedBalance;

        // Moderator resolves event -> Vanya receives payout (100 * 1.8 = 180)
        var resolveResult = store.ResolveEvent(
            MockStore.ModeratorUserId,
            draftId,
            new ResolveEventRequest(
                WinningOutcomeId: winningOutcomeId,
                FactSummary: "Team Spirit одержала победу в гранд-финале со счётом 3:0.",
                SourceUrl: "https://liquipedia.net/counterstrike"));

        Assert.True(resolveResult.IsSuccess);
        Assert.Equal(EventStatus.Resolved, resolveResult.Value!.Status);
        Assert.NotNull(resolveResult.Value.Resolution);

        var walletAfterResolve = store.GetWallet(MockStore.VanyaUserId).Value!;
        Assert.Equal(balanceAfterStake + 180, walletAfterResolve.Balance);
        Assert.Contains(walletAfterResolve.Transactions, t =>
            t.Reason == LedgerReason.PredictionPayout &&
            t.ReferenceId == draftId &&
            t.AmountDelta == 180);
    }
}
