using Sporim.Api.Contracts;

namespace Sporim.Api.State;

public sealed class UserEntity
{
    public required Guid Id { get; init; }
    public required string Email { get; set; }
    public string? ExternalAuthId { get; set; }
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public required Role Role { get; set; }
    public required long Balance { get; set; }
    public required DateTimeOffset WalletUpdatedAt { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }

    public int BaseTotalPredictions { get; set; }
    public int BaseResolvedPredictions { get; set; }
    public int BaseWinsCount { get; set; }
    public double? BaseBrierScore { get; set; }
    public int BaseCurrentStreak { get; set; }
    public int BaseMaxStreak { get; set; }
}

public sealed class OutcomeEntity
{
    public required Guid Id { get; init; }
    public required string Code { get; set; }
    public required string Title { get; set; }
    public required int SortOrder { get; set; }
    public int InitialVotes { get; set; }
}

public sealed class EventEntity
{
    public required Guid Id { get; init; }
    public required string Slug { get; set; }
    public required Guid CreatorId { get; init; }
    public required EventCategory Category { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string ResolutionRules { get; set; }
    public required EventStatus Status { get; set; }
    public required DateTimeOffset EventDate { get; set; }
    public required DateTimeOffset AcceptUntil { get; set; }
    public required int BaseParticipantsCount { get; set; }
    public required List<OutcomeEntity> Outcomes { get; set; }
    public ResolutionDto? Resolution { get; set; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class PredictionEntity
{
    public required Guid Id { get; init; }
    public required string ShortCode { get; init; }
    public required Guid UserId { get; init; }
    public required Guid EventId { get; init; }
    public required Guid OutcomeId { get; init; }
    public required long StakeAmount { get; init; }
    public int? EstimatedProbability { get; init; }
    public string? ThesisText { get; init; }
    public required Guid IdempotencyKey { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public string? OgImageUrl { get; set; }
    public bool? IsWin { get; set; }
    public long? PayoutAmount { get; set; }
}

public sealed class LedgerEntryEntity
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required long AmountDelta { get; init; }
    public required LedgerReason Reason { get; init; }
    public required LedgerReferenceType ReferenceType { get; init; }
    public required Guid ReferenceId { get; init; }
    public required string Description { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
