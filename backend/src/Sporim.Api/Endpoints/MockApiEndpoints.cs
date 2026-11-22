using Microsoft.AspNetCore.Mvc;
using Sporim.Api.Auth;
using Sporim.Api.Contracts;
using Sporim.Api.State;

namespace Sporim.Api.Endpoints;

public static class MockApiEndpoints
{
    public static IEndpointRouteBuilder MapMockApiEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1");

        MapAuthEndpoints(api);
        MapEventsEndpoints(api);
        MapPredictionsEndpoints(api);
        MapProofsEndpoints(api);
        MapProfileAndWalletEndpoints(api);
        MapAdminEndpoints(api);
        MapDevMockControlEndpoints(api);

        app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }))
            .WithTags("Health");

        return app;
    }

    private static void MapAuthEndpoints(RouteGroupBuilder api)
    {
        var auth = api.MapGroup("/auth").WithTags("Auth");

        auth.MapPost("/oauth/{provider}", (
            string provider,
            [FromBody] OAuthLoginRequest? request,
            MockStore store,
            MockTokenService tokenService) =>
        {
            if (request is null)
            {
                return ToErrorResult(
                    400,
                    ErrorCode.ValidationError,
                    "Тело запроса обязательно.");
            }

            var result = store.AuthenticateOAuth(provider, request.ExternalAuthToken);
            if (!result.IsSuccess)
            {
                return ToStoreResult(result);
            }

            var (user, isNewUser) = result.Value;
            var token = tokenService.IssueToken(user);
            var profile = store.GetUserProfileById(user.Id).Value!;

            return Results.Json(
                new OAuthLoginResponse(
                    AccessToken: token,
                    IsNewUser: isNewUser,
                    User: profile),
                statusCode: 200);
        })
        .WithSummary("Вход в 1 клик через внешний провайдер (Telegram / VK)");

        auth.MapPost("/email/request-code", (
            [FromBody] EmailRequestCodeRequest? request,
            MockStore store) =>
        {
            if (request is null)
            {
                return ToErrorResult(400, ErrorCode.ValidationError, "Тело запроса обязательно.");
            }

            var result = store.RequestEmailCode(request.Email);
            return ToStoreResult(result);
        })
        .WithSummary("Запрос одноразового кода на почту (сценарий AuthForm из макетов)");

        auth.MapPost("/email/verify-code", (
            [FromBody] EmailVerifyCodeRequest? request,
            MockStore store,
            MockTokenService tokenService) =>
        {
            if (request is null)
            {
                return ToErrorResult(400, ErrorCode.ValidationError, "Тело запроса обязательно.");
            }

            var result = store.VerifyEmailCode(request.Email, request.Code);
            if (!result.IsSuccess)
            {
                return ToStoreResult(result);
            }

            var user = result.Value!;
            var token = tokenService.IssueToken(user);
            var profile = store.GetUserProfileById(user.Id).Value!;

            return Results.Json(
                new EmailVerifyCodeResponse(
                    AccessToken: token,
                    User: profile),
                statusCode: 200);
        })
        .WithSummary("Подтверждение одноразового кода из письма и получение JWT");
    }

    private static void MapEventsEndpoints(RouteGroupBuilder api)
    {
        var events = api.MapGroup("/events").WithTags("Events");

        events.MapGet("", (
            HttpContext httpContext,
            [FromQuery] string? search,
            [FromQuery] string? category,
            [FromQuery] bool? onlyActive,
            [FromQuery] string? status,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            MockStore store,
            MockTokenService tokenService) =>
        {
            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            var result = store.GetEventCatalog(
                search: search,
                categoryRaw: category,
                onlyActive: onlyActive ?? false,
                statusRaw: status,
                page: page ?? 1,
                pageSize: pageSize ?? 20,
                currentUserId: currentUserId);

            return ToStoreResult(result);
        })
        .WithSummary("Загрузить ленту событий с учётом поиска, категории и фильтра активности");

        events.MapGet("/{eventId}", (
            string eventId,
            HttpContext httpContext,
            MockStore store,
            MockTokenService tokenService) =>
        {
            if (!Guid.TryParse(eventId, out var parsedEventId))
            {
                return ToErrorResult(404, ErrorCode.EventNotFound, "Событие не найдено.");
            }

            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            var result = store.GetEventDetails(parsedEventId, currentUserId);
            return ToStoreResult(result);
        })
        .WithSummary("Загрузить полную карточку события с исходами, дедлайном, статусом и фактом");
    }

    private static void MapPredictionsEndpoints(RouteGroupBuilder api)
    {
        api.MapPost("/events/{eventId}/predictions", (
            string eventId,
            HttpContext httpContext,
            [FromBody] CreatePredictionRequest? request,
            MockStore store,
            MockTokenService tokenService) =>
        {
            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            if (!currentUserId.HasValue)
            {
                return ToErrorResult(
                    401,
                    ErrorCode.Unauthorized,
                    "Войдите, чтобы сделать прогноз.");
            }

            if (!Guid.TryParse(eventId, out var parsedEventId))
            {
                return ToErrorResult(404, ErrorCode.EventNotFound, "Событие не найдено.");
            }

            if (request is null)
            {
                return ToErrorResult(400, ErrorCode.ValidationError, "Тело запроса прогноза обязательно.");
            }

            var idempotencyKeyHeader = httpContext.Request.Headers["Idempotency-Key"].FirstOrDefault();
            var result = store.CreatePrediction(
                eventId: parsedEventId,
                userId: currentUserId.Value,
                idempotencyKeyRaw: idempotencyKeyHeader,
                request: request);

            return ToStoreResult(result);
        })
        .WithTags("Predictions")
        .WithSummary("Отправить подтверждённый прогноз на событие (Задача 1, сценарии А1–А6)");

        api.MapGet("/predictions/my", (
            HttpContext httpContext,
            [FromQuery] string? status,
            [FromQuery] string? category,
            MockStore store,
            MockTokenService tokenService) =>
        {
            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            if (!currentUserId.HasValue)
            {
                return ToErrorResult(
                    401,
                    ErrorCode.Unauthorized,
                    "Требуется авторизация для просмотра портфеля прогнозов.");
            }

            var result = store.GetMyPredictions(currentUserId.Value, status, category);
            return ToStoreResult(result);
        })
        .WithTags("Predictions")
        .WithSummary("Получить историю прогнозов текущего игрока (Портфель / Мои прогнозы)");
    }

    private static void MapProofsEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/proofs/{shortCode}", (
            string shortCode,
            MockStore store) =>
        {
            var result = store.GetPublicProof(shortCode);
            return ToStoreResult(result);
        })
        .WithTags("Proofs")
        .WithSummary("Получить данные публичной пруф-страницы прогноза (/p/{shortCode})");
    }

    private static void MapProfileAndWalletEndpoints(RouteGroupBuilder api)
    {
        api.MapGet("/me", (
            HttpContext httpContext,
            MockStore store,
            MockTokenService tokenService) =>
        {
            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            if (!currentUserId.HasValue)
            {
                return ToErrorResult(
                    401,
                    ErrorCode.Unauthorized,
                    "Требуется авторизация.");
            }

            var result = store.GetUserProfileById(currentUserId.Value);
            return ToStoreResult(result);
        })
        .WithTags("Profile & Wallet")
        .WithSummary("Получить профиль текущего пользователя, баланс и сводную статистику");

        api.MapGet("/wallet", (
            HttpContext httpContext,
            MockStore store,
            MockTokenService tokenService) =>
        {
            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            if (!currentUserId.HasValue)
            {
                return ToErrorResult(
                    401,
                    ErrorCode.Unauthorized,
                    "Требуется авторизация для доступа к кошельку.");
            }

            var result = store.GetWallet(currentUserId.Value);
            return ToStoreResult(result);
        })
        .WithTags("Profile & Wallet")
        .WithSummary("Получить баланс игровых баллов и историю операций (Ledger)");

        api.MapGet("/users/{username}", (
            string username,
            [FromQuery] bool? portfolioMode,
            MockStore store) =>
        {
            var result = store.GetUserProfileByUsername(username, portfolioMode ?? false);
            return ToStoreResult(result);
        })
        .WithTags("Profile & Wallet")
        .WithSummary("Публичный профиль игрока и его трек-рекорд (винрейт, Brier Score, история)");

        api.MapGet("/leaderboard", (
            [FromQuery] string? category,
            MockStore store) =>
        {
            var result = store.GetLeaderboard(category);
            return ToStoreResult(result);
        })
        .WithTags("Profile & Wallet")
        .WithSummary("Рейтинг игроков (общий и дружеский лидерборд по винрейту и очкам)");
    }

    private static void MapAdminEndpoints(RouteGroupBuilder api)
    {
        var admin = api.MapGroup("/admin/events").WithTags("Admin & Moderation");

        admin.MapGet("", (
            HttpContext httpContext,
            [FromQuery] string? status,
            MockStore store,
            MockTokenService tokenService) =>
        {
            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            if (!currentUserId.HasValue)
            {
                return ToErrorResult(401, ErrorCode.Unauthorized, "Требуется авторизация.");
            }

            var result = store.GetAdminEvents(currentUserId.Value, status);
            return ToStoreResult(result);
        })
        .WithSummary("[CRUD: Read] Получить список черновиков и событий на модерации");

        admin.MapPost("", (
            HttpContext httpContext,
            [FromBody] DraftEventRequest? request,
            MockStore store,
            MockTokenService tokenService) =>
        {
            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            if (!currentUserId.HasValue)
            {
                return ToErrorResult(401, ErrorCode.Unauthorized, "Требуется авторизация.");
            }

            if (request is null)
            {
                return ToErrorResult(400, ErrorCode.ValidationError, "Тело запроса черновика обязательно.");
            }

            var result = store.CreateDraftEvent(currentUserId.Value, request);
            return ToStoreResult(result);
        })
        .WithSummary("[CRUD: Create] Создать новый черновик события в разделе модератора");

        admin.MapPatch("/{eventId}", (
            string eventId,
            HttpContext httpContext,
            [FromBody] PatchDraftEventRequest? request,
            MockStore store,
            MockTokenService tokenService) =>
        {
            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            if (!currentUserId.HasValue)
            {
                return ToErrorResult(401, ErrorCode.Unauthorized, "Требуется авторизация.");
            }

            if (!Guid.TryParse(eventId, out var parsedEventId))
            {
                return ToErrorResult(404, ErrorCode.EventNotFound, "Событие не найдено.");
            }

            if (request is null)
            {
                return ToErrorResult(400, ErrorCode.ValidationError, "Тело запроса обязательно.");
            }

            var result = store.PatchDraftEvent(currentUserId.Value, parsedEventId, request);
            return ToStoreResult(result);
        })
        .WithSummary("[CRUD: Update] Изменить черновик события или опубликовать его");

        admin.MapDelete("/{eventId}", (
            string eventId,
            HttpContext httpContext,
            MockStore store,
            MockTokenService tokenService) =>
        {
            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            if (!currentUserId.HasValue)
            {
                return ToErrorResult(401, ErrorCode.Unauthorized, "Требуется авторизация.");
            }

            if (!Guid.TryParse(eventId, out var parsedEventId))
            {
                return ToErrorResult(404, ErrorCode.EventNotFound, "Событие не найдено.");
            }

            var result = store.DeleteDraftEvent(currentUserId.Value, parsedEventId);
            if (result.IsSuccess && result.StatusCode == 204)
            {
                return Results.NoContent();
            }

            return ToStoreResult(result);
        })
        .WithSummary("[CRUD: Delete] Удалить черновик события");

        admin.MapPost("/{eventId}/resolve", (
            string eventId,
            HttpContext httpContext,
            [FromBody] ResolveEventRequest? request,
            MockStore store,
            MockTokenService tokenService) =>
        {
            var currentUserId = ResolveOptionalUserId(httpContext, tokenService, store);
            if (!currentUserId.HasValue)
            {
                return ToErrorResult(401, ErrorCode.Unauthorized, "Требуется авторизация.");
            }

            if (!Guid.TryParse(eventId, out var parsedEventId))
            {
                return ToErrorResult(404, ErrorCode.EventNotFound, "Событие не найдено.");
            }

            if (request is null)
            {
                return ToErrorResult(400, ErrorCode.ValidationError, "Тело запроса резолюции обязательно.");
            }

            var result = store.ResolveEvent(currentUserId.Value, parsedEventId, request);
            return ToStoreResult(result);
        })
        .WithSummary("Зафиксировать официальный исход события, описание факта и источник (Settlement)");
    }

    private static void MapDevMockControlEndpoints(RouteGroupBuilder api)
    {
        var mock = api.MapGroup("/mock").WithTags("Mock Controls (Dev)");

        mock.MapPost("/reset", (MockStore store) =>
        {
            store.ResetToSeed();
            return Results.Ok(new { message = "Состояние Mock API сброшено к начальным Seed-данным." });
        })
        .WithSummary("Сбросить состояние In-Memory стора к начальным данным (для демонстрации на защите)");

        mock.MapPost("/events/{eventId}/expire-deadline", (string eventId, MockStore store) =>
        {
            if (!Guid.TryParse(eventId, out var parsedEventId))
            {
                return ToErrorResult(404, ErrorCode.EventNotFound, "Событие не найдено.");
            }

            var result = store.ExpireEventDeadlineForDemo(parsedEventId);
            return ToStoreResult(result);
        })
        .WithSummary("Принудительно закрыть приём прогнозов по дедлайну acceptUntil (для демонстрации сценария А4)");
    }

    private static Guid? ResolveOptionalUserId(
        HttpContext httpContext,
        MockTokenService tokenService,
        MockStore store)
    {
        var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
        return tokenService.TryValidateAndGetUserId(authHeader, store);
    }

    private static IResult ToStoreResult<T>(StoreResult<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Json(result.Value, statusCode: result.StatusCode);
        }

        return Results.Json(result.Error, statusCode: result.StatusCode);
    }

    private static IResult ToErrorResult(
        int statusCode,
        ErrorCode code,
        string message,
        Dictionary<string, string[]>? fieldErrors = null)
    {
        return Results.Json(new ApiError(code, message, fieldErrors), statusCode: statusCode);
    }
}
