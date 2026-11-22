using System.Text.Json.Serialization;

namespace Sporim.Api.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<Role>))]
public enum Role
{
    [JsonStringEnumMemberName("user")]
    User,

    [JsonStringEnumMemberName("moderator")]
    Moderator,

    [JsonStringEnumMemberName("admin")]
    Admin
}

[JsonConverter(typeof(JsonStringEnumConverter<EventCategory>))]
public enum EventCategory
{
    [JsonStringEnumMemberName("esports")]
    Esports,

    [JsonStringEnumMemberName("pop_culture")]
    PopCulture,

    [JsonStringEnumMemberName("streamers")]
    Streamers,

    [JsonStringEnumMemberName("macro")]
    Macro,

    [JsonStringEnumMemberName("student_ugc")]
    StudentUgc
}

[JsonConverter(typeof(JsonStringEnumConverter<EventStatus>))]
public enum EventStatus
{
    [JsonStringEnumMemberName("draft")]
    Draft,

    [JsonStringEnumMemberName("on_moderation")]
    OnModeration,

    [JsonStringEnumMemberName("open")]
    Open,

    [JsonStringEnumMemberName("awaiting_result")]
    AwaitingResult,

    [JsonStringEnumMemberName("resolved")]
    Resolved,

    [JsonStringEnumMemberName("rejected")]
    Rejected,

    [JsonStringEnumMemberName("canceled")]
    Canceled
}

[JsonConverter(typeof(JsonStringEnumConverter<LedgerReason>))]
public enum LedgerReason
{
    [JsonStringEnumMemberName("welcome_bonus")]
    WelcomeBonus,

    [JsonStringEnumMemberName("prediction_stake")]
    PredictionStake,

    [JsonStringEnumMemberName("prediction_payout")]
    PredictionPayout,

    [JsonStringEnumMemberName("refund")]
    Refund
}

[JsonConverter(typeof(JsonStringEnumConverter<LedgerReferenceType>))]
public enum LedgerReferenceType
{
    [JsonStringEnumMemberName("event")]
    Event,

    [JsonStringEnumMemberName("prediction")]
    Prediction,

    [JsonStringEnumMemberName("system")]
    System
}

[JsonConverter(typeof(JsonStringEnumConverter<ErrorCode>))]
public enum ErrorCode
{
    [JsonStringEnumMemberName("VALIDATION_ERROR")]
    ValidationError,

    [JsonStringEnumMemberName("UNAUTHORIZED")]
    Unauthorized,

    [JsonStringEnumMemberName("FORBIDDEN")]
    Forbidden,

    [JsonStringEnumMemberName("EVENT_NOT_FOUND")]
    EventNotFound,

    [JsonStringEnumMemberName("PROOF_NOT_FOUND")]
    ProofNotFound,

    [JsonStringEnumMemberName("EVENT_CLOSED")]
    EventClosed,

    [JsonStringEnumMemberName("INSUFFICIENT_BALANCE")]
    InsufficientBalance,

    [JsonStringEnumMemberName("INVALID_DRAFT_STATE")]
    InvalidDraftState,

    [JsonStringEnumMemberName("INTERNAL_ERROR")]
    InternalError
}

public static class EnumParser
{
    public static bool TryParseCategory(string? raw, out EventCategory category)
    {
        switch (raw?.Trim().ToLowerInvariant())
        {
            case "esports":
                category = EventCategory.Esports;
                return true;
            case "pop_culture":
            case "popculture":
                category = EventCategory.PopCulture;
                return true;
            case "streamers":
                category = EventCategory.Streamers;
                return true;
            case "macro":
                category = EventCategory.Macro;
                return true;
            case "student_ugc":
            case "studentugc":
                category = EventCategory.StudentUgc;
                return true;
            default:
                category = default;
                return false;
        }
    }

    public static bool TryParseStatus(string? raw, out EventStatus status)
    {
        switch (raw?.Trim().ToLowerInvariant())
        {
            case "draft":
                status = EventStatus.Draft;
                return true;
            case "on_moderation":
            case "onmoderation":
                status = EventStatus.OnModeration;
                return true;
            case "open":
                status = EventStatus.Open;
                return true;
            case "awaiting_result":
            case "awaitingresult":
                status = EventStatus.AwaitingResult;
                return true;
            case "resolved":
                status = EventStatus.Resolved;
                return true;
            case "rejected":
                status = EventStatus.Rejected;
                return true;
            case "canceled":
                status = EventStatus.Canceled;
                return true;
            default:
                status = default;
                return false;
        }
    }
}
