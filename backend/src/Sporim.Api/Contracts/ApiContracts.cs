namespace Sporim.Api.Contracts;

public sealed record ApiError(
    ErrorCode Code,
    string Message,
    Dictionary<string, string[]>? FieldErrors = null);

public sealed record OAuthLoginRequest(string ExternalAuthToken);

public sealed record OAuthLoginResponse(
    string AccessToken,
    bool IsNewUser,
    UserProfileDto User);

public sealed record EmailRequestCodeRequest(string Email);

public sealed record EmailRequestCodeResponse(int CooldownSeconds);

public sealed record EmailVerifyCodeRequest(string Email, string Code);

public sealed record EmailVerifyCodeResponse(
    string AccessToken,
    UserProfileDto User);

public sealed record OutcomeDto(
    Guid Id,
    string Code,
    string Title,
    int SortOrder,
    double? ConsensusPercent);

public sealed record ResolutionDto(
    Guid WinningOutcomeId,
    string FactSummary,
    string? SourceUrl,
    DateTimeOffset ResolvedAt);

public sealed record UserPredictionSummaryDto(
    Guid Id,
    string ShortCode,
    string ProofUrl,
    Guid OutcomeId,
    string OutcomeTitle,
    long StakeAmount,
    int? EstimatedProbability,
    string? ThesisText,
    DateTimeOffset CreatedAt,
    bool? IsWin,
    long? PayoutAmount);

public record EventCardItemDto(
    Guid Id,
    string Slug,
    EventCategory Category,
    string Title,
    string DescriptionSnippet,
    EventStatus Status,
    DateTimeOffset EventDate,
    DateTimeOffset AcceptUntil,
    int ParticipantsCount,
    List<OutcomeDto> Outcomes,
    UserPredictionSummaryDto? MyPrediction);

public sealed record EventDetailsDto(
    Guid Id,
    string Slug,
    EventCategory Category,
    string Title,
    string DescriptionSnippet,
    EventStatus Status,
    DateTimeOffset EventDate,
    DateTimeOffset AcceptUntil,
    int ParticipantsCount,
    List<OutcomeDto> Outcomes,
    UserPredictionSummaryDto? MyPrediction,
    string Description,
    string ResolutionRules,
    ResolutionDto? Resolution) : EventCardItemDto(
        Id,
        Slug,
        Category,
        Title,
        DescriptionSnippet,
        Status,
        EventDate,
        AcceptUntil,
        ParticipantsCount,
        Outcomes,
        MyPrediction);

public sealed record EventCatalogResponse(
    List<EventCardItemDto> Items,
    int Total,
    int Page,
    int PageSize);

public sealed record CreatePredictionRequest(
    Guid OutcomeId,
    long Stake,
    string? Thesis = null,
    int? EstimatedProbability = null);

public sealed record CreatePredictionResponse(
    UserPredictionSummaryDto Prediction,
    long UpdatedBalance,
    bool IsDuplicateReplay);

public sealed record MyPredictionItemDto(
    UserPredictionSummaryDto Prediction,
    Guid EventId,
    string EventTitle,
    string EventSlug,
    EventCategory EventCategory,
    EventStatus EventStatus,
    DateTimeOffset EventDate,
    ResolutionDto? Resolution);

public sealed record ProofAuthorDto(
    string Username,
    string DisplayName,
    string? AvatarUrl,
    string RankCode,
    double WinRate);

public sealed record ProofEventDto(
    Guid Id,
    string Slug,
    string Title,
    EventCategory Category,
    EventStatus Status,
    DateTimeOffset EventDate);

public sealed record PublicProofDto(
    string ShortCode,
    ProofAuthorDto Author,
    ProofEventDto Event,
    string OutcomeTitle,
    long StakeAmount,
    int? EstimatedProbability,
    string? ThesisText,
    DateTimeOffset CreatedAt,
    string? OgImageUrl,
    ResolutionDto? Resolution,
    bool? IsWin);

public sealed record WalletTransactionDto(
    Guid Id,
    long AmountDelta,
    LedgerReason Reason,
    LedgerReferenceType ReferenceType,
    Guid ReferenceId,
    string? Description,
    DateTimeOffset CreatedAt);

public sealed record WalletSummaryDto(
    long Balance,
    DateTimeOffset UpdatedAt,
    List<WalletTransactionDto> Transactions);

public sealed record UserStatsDto(
    int TotalPredictions,
    int ResolvedPredictions,
    int WinsCount,
    double WinRate,
    double? BrierScore,
    int CurrentStreak,
    int MaxStreak,
    string RankCode);

public sealed record UserProfileDto(
    Guid Id,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    Role Role,
    long Balance,
    UserStatsDto Stats,
    DateTimeOffset CreatedAt);

public sealed record LeaderboardItemDto(
    int RankPosition,
    string Username,
    string DisplayName,
    string? AvatarUrl,
    double WinRate,
    int WinsCount,
    int TotalResolved,
    string RankCode);

public sealed record DraftOutcomeInput(
    string Code,
    string Title,
    int SortOrder);

public sealed record DraftEventRequest(
    string Title,
    string? Slug,
    EventCategory Category,
    string Description,
    string ResolutionRules,
    DateTimeOffset EventDate,
    DateTimeOffset AcceptUntil,
    List<DraftOutcomeInput> Outcomes);

public sealed record PatchDraftEventRequest(
    string? Title = null,
    EventCategory? Category = null,
    string? Description = null,
    string? ResolutionRules = null,
    DateTimeOffset? EventDate = null,
    DateTimeOffset? AcceptUntil = null,
    EventStatus? Status = null,
    List<DraftOutcomeInput>? Outcomes = null);

public sealed record ResolveEventRequest(
    Guid WinningOutcomeId,
    string FactSummary,
    string? SourceUrl = null);
