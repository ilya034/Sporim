using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Sporim.Api.Contracts;

namespace Sporim.Api.State;

public sealed record StoreResult<T>(
    bool IsSuccess,
    int StatusCode,
    T? Value = default,
    ApiError? Error = null)
{
    public static StoreResult<T> Ok(T value, int statusCode = 200) =>
        new(true, statusCode, value, null);

    public static StoreResult<T> Fail(
        int statusCode,
        ErrorCode code,
        string message,
        Dictionary<string, string[]>? fieldErrors = null) =>
        new(false, statusCode, default, new ApiError(code, message, fieldErrors));
}

public sealed class MockStore
{
    public static readonly Guid VanyaUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ArtemUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid DenisUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid ModeratorUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid AdminUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    public static readonly Guid GrandFinalEventId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid GtaTrailerEventId = Guid.Parse("22222222-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid StreamersBattleEventId = Guid.Parse("33333333-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid CentralBankRateEventId = Guid.Parse("44444444-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid HackathonEventId = Guid.Parse("55555555-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid DraftCs2MajorEventId = Guid.Parse("66666666-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid UgcExamEventId = Guid.Parse("77777777-e5f6-7890-abcd-ef1234567890");

    public static readonly Guid GrandFinalYesOutcomeId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    public static readonly Guid GrandFinalNoOutcomeId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678902");

    private const long WelcomeBonusAmount = 1000;
    private const string Base62Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private readonly Lock _sync = new();
    private readonly Dictionary<Guid, UserEntity> _users = [];
    private readonly Dictionary<Guid, EventEntity> _events = [];
    private readonly List<PredictionEntity> _predictions = [];
    private readonly List<LedgerEntryEntity> _ledger = [];
    private readonly Dictionary<string, string> _pendingEmailCodes = new(StringComparer.OrdinalIgnoreCase);

    public MockStore()
    {
        ResetToSeed();
    }

    public void ResetToSeed()
    {
        lock (_sync)
        {
            _users.Clear();
            _events.Clear();
            _predictions.Clear();
            _ledger.Clear();
            _pendingEmailCodes.Clear();

            var now = DateTimeOffset.UtcNow;

            var vanya = new UserEntity
            {
                Id = VanyaUserId,
                Email = "vanya@sporim.ru",
                ExternalAuthId = "tg:vanya_analyst",
                Username = "vanya_analyst",
                DisplayName = "Ваня",
                AvatarUrl = "https://api.dicebear.com/9.x/bottts/svg?seed=vanya",
                Role = Role.User,
                Balance = 1250,
                WalletUpdatedAt = now.AddHours(-2),
                CreatedAt = now.AddDays(-30),
                BaseTotalPredictions = 21,
                BaseResolvedPredictions = 18,
                BaseWinsCount = 12,
                BaseBrierScore = 0.1845,
                BaseCurrentStreak = 3,
                BaseMaxStreak = 7
            };

            var artem = new UserEntity
            {
                Id = ArtemUserId,
                Email = "artem@sporim.ru",
                ExternalAuthId = "tg:artem_pop",
                Username = "artem_pop",
                DisplayName = "Артём",
                AvatarUrl = "https://api.dicebear.com/9.x/bottts/svg?seed=artem",
                Role = Role.User,
                Balance = 900,
                WalletUpdatedAt = now.AddHours(-5),
                CreatedAt = now.AddDays(-24),
                BaseTotalPredictions = 18,
                BaseResolvedPredictions = 15,
                BaseWinsCount = 9,
                BaseBrierScore = 0.2310,
                BaseCurrentStreak = 2,
                BaseMaxStreak = 5
            };

            var denis = new UserEntity
            {
                Id = DenisUserId,
                Email = "denis@sporim.ru",
                ExternalAuthId = "tg:denis_quant",
                Username = "denis_quant",
                DisplayName = "Денис",
                AvatarUrl = "https://api.dicebear.com/9.x/bottts/svg?seed=denis",
                Role = Role.User,
                Balance = 2100,
                WalletUpdatedAt = now.AddHours(-1),
                CreatedAt = now.AddDays(-40),
                BaseTotalPredictions = 32,
                BaseResolvedPredictions = 28,
                BaseWinsCount = 21,
                BaseBrierScore = 0.1420,
                BaseCurrentStreak = 6,
                BaseMaxStreak = 9
            };

            var moderator = new UserEntity
            {
                Id = ModeratorUserId,
                Email = "moderator@sporim.ru",
                ExternalAuthId = "tg:moderator_max",
                Username = "moderator_max",
                DisplayName = "Максим (Модератор)",
                AvatarUrl = "https://api.dicebear.com/9.x/bottts/svg?seed=moderator",
                Role = Role.Moderator,
                Balance = 1000,
                WalletUpdatedAt = now.AddDays(-10),
                CreatedAt = now.AddDays(-45),
                BaseTotalPredictions = 5,
                BaseResolvedPredictions = 4,
                BaseWinsCount = 3,
                BaseBrierScore = 0.1900,
                BaseCurrentStreak = 1,
                BaseMaxStreak = 3
            };

            var admin = new UserEntity
            {
                Id = AdminUserId,
                Email = "admin@sporim.ru",
                ExternalAuthId = "tg:admin_root",
                Username = "admin_root",
                DisplayName = "Администратор СПОРИМ",
                AvatarUrl = "https://api.dicebear.com/9.x/bottts/svg?seed=admin",
                Role = Role.Admin,
                Balance = 5000,
                WalletUpdatedAt = now.AddDays(-50),
                CreatedAt = now.AddDays(-60),
                BaseTotalPredictions = 10,
                BaseResolvedPredictions = 10,
                BaseWinsCount = 8,
                BaseBrierScore = 0.1500,
                BaseCurrentStreak = 4,
                BaseMaxStreak = 6
            };

            foreach (var u in new[] { vanya, artem, denis, moderator, admin })
            {
                _users[u.Id] = u;
            }

            // Event 1: Open event WITHOUT prediction from vanya_analyst (Card State 1: Приём открыт)
            var event1 = new EventEntity
            {
                Id = GrandFinalEventId,
                Slug = "team-a-vs-team-b-grand-final",
                CreatorId = ModeratorUserId,
                Category = EventCategory.Esports,
                Title = "Победит ли команда А в финале турнира?",
                Description = "Финальный матч международного турнира по киберспорту. Команда A vs Команда B. Формат — до 3 побед (Bo5).",
                ResolutionRules = "Исход «Да» фиксируется, если Команда А одержит победу в серии (3:0, 3:1 или 3:2) согласно официальному протоколу турнира.",
                Status = EventStatus.Open,
                EventDate = now.AddDays(5).AddHours(4),
                AcceptUntil = now.AddDays(5).AddHours(2),
                BaseParticipantsCount = 142,
                CreatedAt = now.AddDays(-3),
                Outcomes =
                [
                    new OutcomeEntity
                    {
                        Id = GrandFinalYesOutcomeId,
                        Code = "YES",
                        Title = "Да — Команда А победит",
                        SortOrder = 1,
                        InitialVotes = 92
                    },
                    new OutcomeEntity
                    {
                        Id = GrandFinalNoOutcomeId,
                        Code = "NO",
                        Title = "Нет — Команда А не победит",
                        SortOrder = 2,
                        InitialVotes = 50
                    }
                ]
            };

            // Event 2: Open event WITH existing prediction from vanya_analyst (Card State 2: Прогноз сохранён, proof k9x2mP8q)
            var gtaYesId = Guid.Parse("22222222-f6a7-8901-bcde-f12345678901");
            var gtaNoId = Guid.Parse("22222222-f6a7-8901-bcde-f12345678902");
            var event2 = new EventEntity
            {
                Id = GtaTrailerEventId,
                Slug = "gta-vi-second-trailer-nov-2026",
                CreatorId = ModeratorUserId,
                Category = EventCategory.PopCulture,
                Title = "Выйдет ли второй трейлер GTA VI до конца ноября 2026?",
                Description = "Спор о дате публикации второго официального трейлера GTA VI на официальном канале Rockstar Games.",
                ResolutionRules = "Исход «Да» фиксируется при появлении официального ролика на YouTube-канале Rockstar Games до указанной даты события.",
                Status = EventStatus.Open,
                EventDate = now.AddDays(14),
                AcceptUntil = now.AddDays(10),
                BaseParticipantsCount = 219,
                CreatedAt = now.AddDays(-5),
                Outcomes =
                [
                    new OutcomeEntity
                    {
                        Id = gtaYesId,
                        Code = "YES",
                        Title = "Да — Трейлер выйдет",
                        SortOrder = 1,
                        InitialVotes = 150
                    },
                    new OutcomeEntity
                    {
                        Id = gtaNoId,
                        Code = "NO",
                        Title = "Нет — Релиз трейлера перенесут",
                        SortOrder = 2,
                        InitialVotes = 69
                    }
                ]
            };

            // Event 3: Status in DB is Open, but AcceptUntil has already passed -> Lazy evaluation returns awaiting_result (Card State 3: Ожидается результат)
            var streamersYesId = Guid.Parse("33333333-f6a7-8901-bcde-f12345678901");
            var streamersNoId = Guid.Parse("33333333-f6a7-8901-bcde-f12345678902");
            var event3 = new EventEntity
            {
                Id = StreamersBattleEventId,
                Slug = "streamers-battle-winner-season-9",
                CreatorId = ModeratorUserId,
                Category = EventCategory.Streamers,
                Title = "Наберёт ли финал Streamers Battle более 250 тысяч зрителей в пике?",
                Description = "Решающий гранд-финал стримерского турнира на Twitch. Учитывается суммарный пиковый онлайн официальных кастеров и участников.",
                ResolutionRules = "Исход «Да» фиксируется, если по данным StreamsCharts пиковый онлайн гранд-финала превысит 250 000 зрителей.",
                Status = EventStatus.Open,
                EventDate = now.AddHours(3),
                AcceptUntil = now.AddMinutes(-30),
                BaseParticipantsCount = 87,
                CreatedAt = now.AddDays(-2),
                Outcomes =
                [
                    new OutcomeEntity
                    {
                        Id = streamersYesId,
                        Code = "YES",
                        Title = "Да — Пробьёт 250к зрителей",
                        SortOrder = 1,
                        InitialVotes = 54
                    },
                    new OutcomeEntity
                    {
                        Id = streamersNoId,
                        Code = "NO",
                        Title = "Нет — Будет меньше 250к",
                        SortOrder = 2,
                        InitialVotes = 33
                    }
                ]
            };

            // Event 4: Resolved event (Card State 4: Завершено)
            var cbYesId = Guid.Parse("44444444-f6a7-8901-bcde-f12345678901");
            var cbNoId = Guid.Parse("44444444-f6a7-8901-bcde-f12345678902");
            var event4 = new EventEntity
            {
                Id = CentralBankRateEventId,
                Slug = "cb-key-rate-decision-oct-2026",
                CreatorId = ModeratorUserId,
                Category = EventCategory.Macro,
                Title = "Сохранит ли ЦБ РФ ключевую ставку без изменений на заседании?",
                Description = "Заседание Совета директоров Банка России по денежно-кредитной политике и уровню ключевой ставки.",
                ResolutionRules = "Исход «Да» фиксируется, если в официальном пресс-релизе ЦБ РФ ключевая ставка оставлена без изменений.",
                Status = EventStatus.Resolved,
                EventDate = now.AddDays(-4),
                AcceptUntil = now.AddDays(-5),
                BaseParticipantsCount = 311,
                CreatedAt = now.AddDays(-12),
                Outcomes =
                [
                    new OutcomeEntity
                    {
                        Id = cbYesId,
                        Code = "YES",
                        Title = "Да — Ставка сохранится",
                        SortOrder = 1,
                        InitialVotes = 205
                    },
                    new OutcomeEntity
                    {
                        Id = cbNoId,
                        Code = "NO",
                        Title = "Нет — Ставку изменят",
                        SortOrder = 2,
                        InitialVotes = 106
                    }
                ],
                Resolution = new ResolutionDto(
                    WinningOutcomeId: cbYesId,
                    FactSummary: "Банк России принял решение сохранить ключевую ставку на прежнем уровне согласно официальному пресс-релизу.",
                    SourceUrl: "https://cbr.ru/press/",
                    ResolvedAt: now.AddDays(-4).AddHours(1))
            };

            // Event 5: Student UGC open event
            var hackYesId = Guid.Parse("55555555-f6a7-8901-bcde-f12345678901");
            var hackNoId = Guid.Parse("55555555-f6a7-8901-bcde-f12345678902");
            var event5 = new EventEntity
            {
                Id = HackathonEventId,
                Slug = "campus-hackathon-winner-2026",
                CreatorId = ArtemUserId,
                Category = EventCategory.StudentUgc,
                Title = "Успеет ли команда сдать прототип до дедлайна студенческого хакатона?",
                Description = "Локальный спор участников весеннего студенческого хакатона о готовности рабочего демо к питчингу.",
                ResolutionRules = "Исход «Да» фиксируется, если репозиторий с рабочим билдом отправлен в форму жюри до закрытия приёма.",
                Status = EventStatus.Open,
                EventDate = now.AddDays(3),
                AcceptUntil = now.AddDays(2),
                BaseParticipantsCount = 34,
                CreatedAt = now.AddDays(-1),
                Outcomes =
                [
                    new OutcomeEntity
                    {
                        Id = hackYesId,
                        Code = "YES",
                        Title = "Да — Сдадут вовремя",
                        SortOrder = 1,
                        InitialVotes = 24
                    },
                    new OutcomeEntity
                    {
                        Id = hackNoId,
                        Code = "NO",
                        Title = "Нет — Дедлайн сорвётся",
                        SortOrder = 2,
                        InitialVotes = 10
                    }
                ]
            };

            // Event 6: Draft event for Moderator CRUD
            var draftYesId = Guid.Parse("66666666-f6a7-8901-bcde-f12345678901");
            var draftNoId = Guid.Parse("66666666-f6a7-8901-bcde-f12345678902");
            var event6 = new EventEntity
            {
                Id = DraftCs2MajorEventId,
                Slug = "draft-new-cs2-major-pickem",
                CreatorId = ModeratorUserId,
                Category = EventCategory.Esports,
                Title = "Пройдёт ли Team Spirit в плей-офф мейджора со счётом 3-0?",
                Description = "Черновик спора на стадию Elimination Stage предстоящего турнира по CS2.",
                ResolutionRules = "Исход «Да» фиксируется при выходе команды в плей-офф без поражений (3-0).",
                Status = EventStatus.Draft,
                EventDate = now.AddDays(20),
                AcceptUntil = now.AddDays(18),
                BaseParticipantsCount = 0,
                CreatedAt = now.AddHours(-6),
                Outcomes =
                [
                    new OutcomeEntity
                    {
                        Id = draftYesId,
                        Code = "YES",
                        Title = "Да — Пройдут 3-0",
                        SortOrder = 1,
                        InitialVotes = 0
                    },
                    new OutcomeEntity
                    {
                        Id = draftNoId,
                        Code = "NO",
                        Title = "Нет — Будет минимум 1 поражение",
                        SortOrder = 2,
                        InitialVotes = 0
                    }
                ]
            };

            // Event 7: OnModeration UGC event for Moderator panel
            var ugcYesId = Guid.Parse("77777777-f6a7-8901-bcde-f12345678901");
            var ugcNoId = Guid.Parse("77777777-f6a7-8901-bcde-f12345678902");
            var event7 = new EventEntity
            {
                Id = UgcExamEventId,
                Slug = "ugc-exam-automats-winter",
                CreatorId = ArtemUserId,
                Category = EventCategory.StudentUgc,
                Title = "Будет ли больше 10 автоматов по матанализу на потоке в сессию?",
                Description = "Предложенный студентами спор о количестве автоматов по итогам семестрового рейтинга.",
                ResolutionRules = "Исход «Да» фиксируется по официальной ведомости БРС кафедры в день зачёта.",
                Status = EventStatus.OnModeration,
                EventDate = now.AddDays(25),
                AcceptUntil = now.AddDays(22),
                BaseParticipantsCount = 0,
                CreatedAt = now.AddHours(-3),
                Outcomes =
                [
                    new OutcomeEntity
                    {
                        Id = ugcYesId,
                        Code = "YES",
                        Title = "Да — Больше 10 автоматов",
                        SortOrder = 1,
                        InitialVotes = 0
                    },
                    new OutcomeEntity
                    {
                        Id = ugcNoId,
                        Code = "NO",
                        Title = "Нет — 10 или меньше",
                        SortOrder = 2,
                        InitialVotes = 0
                    }
                ]
            };

            foreach (var ev in new[] { event1, event2, event3, event4, event5, event6, event7 })
            {
                _events[ev.Id] = ev;
            }

            // Seeded predictions for vanya_analyst
            var predGta = new PredictionEntity
            {
                Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
                ShortCode = "k9x2mP8q",
                UserId = VanyaUserId,
                EventId = GtaTrailerEventId,
                OutcomeId = gtaYesId,
                StakeAmount = 250,
                EstimatedProbability = 75,
                ThesisText = "У Команды А сильнее драфты и Rockstar традиционно публикует трейлеры перед квартальным отчётом Take-Two.",
                IdempotencyKey = Guid.Parse("10000000-0000-4000-8000-000000000001"),
                CreatedAt = now.AddDays(-2),
                OgImageUrl = "https://cdn.sporim.ru/og/k9x2mP8q.png",
                IsWin = null,
                PayoutAmount = null
            };

            var predStreamers = new PredictionEntity
            {
                Id = Guid.Parse("d4e5f6a7-b8c9-0123-def0-234567890123"),
                ShortCode = "m4p9xL2v",
                UserId = VanyaUserId,
                EventId = StreamersBattleEventId,
                OutcomeId = streamersYesId,
                StakeAmount = 150,
                EstimatedProbability = 65,
                ThesisText = "В финале играют два самых популярных состава сезона, пик легко превысит 250 тысяч.",
                IdempotencyKey = Guid.Parse("10000000-0000-4000-8000-000000000002"),
                CreatedAt = now.AddHours(-5),
                OgImageUrl = "https://cdn.sporim.ru/og/m4p9xL2v.png",
                IsWin = null,
                PayoutAmount = null
            };

            var predCb = new PredictionEntity
            {
                Id = Guid.Parse("e5f6a7b8-c9d0-1234-ef01-345678901234"),
                ShortCode = "w7n3qR5z",
                UserId = VanyaUserId,
                EventId = CentralBankRateEventId,
                OutcomeId = cbYesId,
                StakeAmount = 200,
                EstimatedProbability = 80,
                ThesisText = "Инфляционные ожидания стабилизировались, консенсус макроаналитиков — сохранение ставки.",
                IdempotencyKey = Guid.Parse("10000000-0000-4000-8000-000000000003"),
                CreatedAt = now.AddDays(-7),
                OgImageUrl = "https://cdn.sporim.ru/og/w7n3qR5z.png",
                IsWin = true,
                PayoutAmount = 420
            };

            // Seeded macro prediction for denis_quant
            var predDenisMacro = new PredictionEntity
            {
                Id = Guid.Parse("f6a7b8c9-d0e1-2345-f012-456789012345"),
                ShortCode = "q1z8kT4n",
                UserId = DenisUserId,
                EventId = CentralBankRateEventId,
                OutcomeId = cbYesId,
                StakeAmount = 500,
                EstimatedProbability = 85,
                ThesisText = "Кривая ОФЗ уже заложила сохранение текущей ставки с вероятностью выше 80%.",
                IdempotencyKey = Guid.Parse("10000000-0000-4000-8000-000000000004"),
                CreatedAt = now.AddDays(-8),
                OgImageUrl = "https://cdn.sporim.ru/og/q1z8kT4n.png",
                IsWin = true,
                PayoutAmount = 900
            };

            _predictions.AddRange([predGta, predStreamers, predCb, predDenisMacro]);

            // Seeded ledger transactions for vanya_analyst
            _ledger.AddRange(
            [
                new LedgerEntryEntity
                {
                    Id = Guid.Parse("90000000-0000-4000-8000-000000000001"),
                    UserId = VanyaUserId,
                    AmountDelta = 1000,
                    Reason = LedgerReason.WelcomeBonus,
                    ReferenceType = LedgerReferenceType.System,
                    ReferenceId = VanyaUserId,
                    Description = "Стартовый бонус при регистрации в «СПОРИМ»",
                    CreatedAt = now.AddDays(-30)
                },
                new LedgerEntryEntity
                {
                    Id = Guid.Parse("90000000-0000-4000-8000-000000000002"),
                    UserId = VanyaUserId,
                    AmountDelta = -200,
                    Reason = LedgerReason.PredictionStake,
                    ReferenceType = LedgerReferenceType.Prediction,
                    ReferenceId = predCb.Id,
                    Description = $"Прогноз на событие: {event4.Title}",
                    CreatedAt = predCb.CreatedAt
                },
                new LedgerEntryEntity
                {
                    Id = Guid.Parse("90000000-0000-4000-8000-000000000003"),
                    UserId = VanyaUserId,
                    AmountDelta = 420,
                    Reason = LedgerReason.PredictionPayout,
                    ReferenceType = LedgerReferenceType.Event,
                    ReferenceId = event4.Id,
                    Description = $"Выплата за точный прогноз: {event4.Title}",
                    CreatedAt = now.AddDays(-4).AddHours(1)
                },
                new LedgerEntryEntity
                {
                    Id = Guid.Parse("90000000-0000-4000-8000-000000000004"),
                    UserId = VanyaUserId,
                    AmountDelta = -250,
                    Reason = LedgerReason.PredictionStake,
                    ReferenceType = LedgerReferenceType.Prediction,
                    ReferenceId = predGta.Id,
                    Description = $"Прогноз на событие: {event2.Title}",
                    CreatedAt = predGta.CreatedAt
                },
                new LedgerEntryEntity
                {
                    Id = Guid.Parse("90000000-0000-4000-8000-000000000005"),
                    UserId = VanyaUserId,
                    AmountDelta = -150,
                    Reason = LedgerReason.PredictionStake,
                    ReferenceType = LedgerReferenceType.Prediction,
                    ReferenceId = predStreamers.Id,
                    Description = $"Прогноз на событие: {event3.Title}",
                    CreatedAt = predStreamers.CreatedAt
                }
            ]);
        }
    }

    public bool UserExists(Guid userId)
    {
        lock (_sync)
        {
            return _users.ContainsKey(userId);
        }
    }

    public UserEntity? GetUserEntity(Guid userId)
    {
        lock (_sync)
        {
            return _users.GetValueOrDefault(userId);
        }
    }

    public StoreResult<(UserEntity User, bool IsNewUser)> AuthenticateOAuth(string provider, string externalAuthToken)
    {
        var normalizedProvider = provider.Trim().ToLowerInvariant();
        if (normalizedProvider is not ("telegram" or "vk"))
        {
            return StoreResult<(UserEntity, bool)>.Fail(
                400,
                ErrorCode.ValidationError,
                "Поддерживаются только провайдеры telegram и vk.",
                new Dictionary<string, string[]> { ["provider"] = ["Недопустимый OAuth-провайдер."] });
        }

        if (string.IsNullOrWhiteSpace(externalAuthToken))
        {
            return StoreResult<(UserEntity, bool)>.Fail(
                400,
                ErrorCode.ValidationError,
                "Не передан externalAuthToken.",
                new Dictionary<string, string[]> { ["externalAuthToken"] = ["Поле externalAuthToken обязательно."] });
        }

        lock (_sync)
        {
            var tokenKey = externalAuthToken.Trim();
            var existing = _users.Values.FirstOrDefault(u =>
                string.Equals(u.ExternalAuthId, $"{normalizedProvider}:{tokenKey}", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.ExternalAuthId, tokenKey, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.Username, tokenKey, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                return StoreResult<(UserEntity, bool)>.Ok((existing, false));
            }

            var now = DateTimeOffset.UtcNow;
            var cleanSuffix = Regex.Replace(tokenKey, "[^a-zA-Z0-9_]", "");
            if (string.IsNullOrWhiteSpace(cleanSuffix))
            {
                cleanSuffix = Guid.NewGuid().ToString("N")[..6];
            }
            else if (cleanSuffix.Length > 16)
            {
                cleanSuffix = cleanSuffix[..16];
            }

            var username = EnsureUniqueUsername($"{normalizedProvider}_{cleanSuffix}".ToLowerInvariant());
            var newUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = $"{username}@oauth.sporim.ru",
                ExternalAuthId = $"{normalizedProvider}:{tokenKey}",
                Username = username,
                DisplayName = $"Игрок {cleanSuffix}",
                AvatarUrl = $"https://api.dicebear.com/9.x/bottts/svg?seed={username}",
                Role = Role.User,
                Balance = WelcomeBonusAmount,
                WalletUpdatedAt = now,
                CreatedAt = now
            };

            _users[newUser.Id] = newUser;
            _ledger.Add(new LedgerEntryEntity
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                AmountDelta = WelcomeBonusAmount,
                Reason = LedgerReason.WelcomeBonus,
                ReferenceType = LedgerReferenceType.System,
                ReferenceId = newUser.Id,
                Description = "Стартовый бонус при первом входе через " + normalizedProvider.ToUpperInvariant(),
                CreatedAt = now
            });

            return StoreResult<(UserEntity, bool)>.Ok((newUser, true));
        }
    }

    public StoreResult<EmailRequestCodeResponse> RequestEmailCode(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            return StoreResult<EmailRequestCodeResponse>.Fail(
                400,
                ErrorCode.ValidationError,
                "Проверьте адрес электронной почты.",
                new Dictionary<string, string[]> { ["email"] = ["Введите корректный адрес электронной почты."] });
        }

        lock (_sync)
        {
            _pendingEmailCodes[email.Trim()] = "482910";
            return StoreResult<EmailRequestCodeResponse>.Ok(new EmailRequestCodeResponse(CooldownSeconds: 60));
        }
    }

    public StoreResult<UserEntity> VerifyEmailCode(string? email, string? code)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@', StringComparison.Ordinal))
        {
            return StoreResult<UserEntity>.Fail(
                400,
                ErrorCode.ValidationError,
                "Проверьте адрес электронной почты.",
                new Dictionary<string, string[]> { ["email"] = ["Введите корректный адрес электронной почты."] });
        }

        var trimmedCode = code?.Trim() ?? string.Empty;
        // Code "000000" or wrong length triggers 400 error to test AuthForm invalid code handling
        if (trimmedCode.Length < 4 || string.Equals(trimmedCode, "000000", StringComparison.Ordinal))
        {
            return StoreResult<UserEntity>.Fail(
                400,
                ErrorCode.ValidationError,
                "Неверный или истёкший одноразовый код.",
                new Dictionary<string, string[]> { ["code"] = ["Неверный или истёкший код подтверждения."] });
        }

        lock (_sync)
        {
            var normalizedEmail = email.Trim();
            var existing = _users.Values.FirstOrDefault(u =>
                string.Equals(u.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                return StoreResult<UserEntity>.Ok(existing);
            }

            var now = DateTimeOffset.UtcNow;
            var localPart = normalizedEmail.Split('@')[0];
            var baseUsername = Regex.Replace(localPart, "[^a-zA-Z0-9_]", "_").ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(baseUsername))
            {
                baseUsername = "player";
            }

            var username = EnsureUniqueUsername(baseUsername);
            var role = normalizedEmail.StartsWith("mod", StringComparison.OrdinalIgnoreCase)
                ? Role.Moderator
                : normalizedEmail.StartsWith("admin", StringComparison.OrdinalIgnoreCase)
                    ? Role.Admin
                    : Role.User;

            var newUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = normalizedEmail,
                Username = username,
                DisplayName = localPart,
                AvatarUrl = $"https://api.dicebear.com/9.x/bottts/svg?seed={username}",
                Role = role,
                Balance = WelcomeBonusAmount,
                WalletUpdatedAt = now,
                CreatedAt = now
            };

            _users[newUser.Id] = newUser;
            _ledger.Add(new LedgerEntryEntity
            {
                Id = Guid.NewGuid(),
                UserId = newUser.Id,
                AmountDelta = WelcomeBonusAmount,
                Reason = LedgerReason.WelcomeBonus,
                ReferenceType = LedgerReferenceType.System,
                ReferenceId = newUser.Id,
                Description = "Стартовый бонус при регистрации в «СПОРИМ»",
                CreatedAt = now
            });

            return StoreResult<UserEntity>.Ok(newUser);
        }
    }

    public StoreResult<EventCatalogResponse> GetEventCatalog(
        string? search,
        string? categoryRaw,
        bool onlyActive,
        string? statusRaw,
        int page,
        int pageSize,
        Guid? currentUserId)
    {
        EventCategory? categoryFilter = null;
        if (!string.IsNullOrWhiteSpace(categoryRaw))
        {
            if (!EnumParser.TryParseCategory(categoryRaw, out var parsedCategory))
            {
                return StoreResult<EventCatalogResponse>.Fail(
                    400,
                    ErrorCode.ValidationError,
                    $"Неизвестная категория: {categoryRaw}",
                    new Dictionary<string, string[]> { ["category"] = ["Недопустимое значение категории."] });
            }

            categoryFilter = parsedCategory;
        }

        EventStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(statusRaw))
        {
            if (!EnumParser.TryParseStatus(statusRaw, out var parsedStatus))
            {
                return StoreResult<EventCatalogResponse>.Fail(
                    400,
                    ErrorCode.ValidationError,
                    $"Неизвестный статус: {statusRaw}",
                    new Dictionary<string, string[]> { ["status"] = ["Недопустимое значение статуса."] });
            }

            statusFilter = parsedStatus;
        }

        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, 50);

        lock (_sync)
        {
            var now = DateTimeOffset.UtcNow;

            // Public catalog only shows published lifecycle events (open, awaiting_result, resolved)
            var query = _events.Values
                .Select(ev => (Entity: ev, EffectiveStatus: GetEffectiveStatus(ev, now)))
                .Where(x => x.EffectiveStatus is EventStatus.Open or EventStatus.AwaitingResult or EventStatus.Resolved);

            if (categoryFilter.HasValue)
            {
                query = query.Where(x => x.Entity.Category == categoryFilter.Value);
            }

            if (onlyActive)
            {
                query = query.Where(x => x.EffectiveStatus == EventStatus.Open && x.Entity.AcceptUntil > now);
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(x => x.EffectiveStatus == statusFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(x =>
                    x.Entity.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Entity.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    x.Entity.Slug.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            var ordered = query
                .OrderBy(x => x.EffectiveStatus == EventStatus.Open ? 0 : x.EffectiveStatus == EventStatus.AwaitingResult ? 1 : 2)
                .ThenBy(x => x.Entity.AcceptUntil)
                .ToList();

            var total = ordered.Count;
            var items = ordered
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .Select(x => ToEventCardItemDto(x.Entity, x.EffectiveStatus, currentUserId))
                .ToList();

            return StoreResult<EventCatalogResponse>.Ok(
                new EventCatalogResponse(items, total, normalizedPage, normalizedPageSize));
        }
    }

    public StoreResult<EventDetailsDto> GetEventDetails(Guid eventId, Guid? currentUserId)
    {
        lock (_sync)
        {
            if (!_events.TryGetValue(eventId, out var ev))
            {
                return StoreResult<EventDetailsDto>.Fail(
                    404,
                    ErrorCode.EventNotFound,
                    "Событие не найдено.");
            }

            var effectiveStatus = GetEffectiveStatus(ev, DateTimeOffset.UtcNow);
            return StoreResult<EventDetailsDto>.Ok(ToEventDetailsDto(ev, effectiveStatus, currentUserId));
        }
    }

    public StoreResult<CreatePredictionResponse> CreatePrediction(
        Guid eventId,
        Guid userId,
        string? idempotencyKeyRaw,
        CreatePredictionRequest request)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKeyRaw) || !Guid.TryParse(idempotencyKeyRaw, out var idempotencyKey))
        {
            return StoreResult<CreatePredictionResponse>.Fail(
                400,
                ErrorCode.ValidationError,
                "Заголовок Idempotency-Key должен быть валидным UUID.",
                new Dictionary<string, string[]>
                {
                    ["Idempotency-Key"] = ["Передайте корректный UUID в заголовке Idempotency-Key."]
                });
        }

        lock (_sync)
        {
            if (!_users.TryGetValue(userId, out var user))
            {
                return StoreResult<CreatePredictionResponse>.Fail(
                    401,
                    ErrorCode.Unauthorized,
                    "Пользователь не авторизован.");
            }

            if (!_events.TryGetValue(eventId, out var ev))
            {
                return StoreResult<CreatePredictionResponse>.Fail(
                    404,
                    ErrorCode.EventNotFound,
                    "Событие не найдено.");
            }

            // Scenarios A5 and A6: check existing prediction by Idempotency-Key OR (userId, eventId) first!
            var existingByUserAndEvent = _predictions.FirstOrDefault(p =>
                p.UserId == userId && p.EventId == eventId);
            var existingByKey = _predictions.FirstOrDefault(p =>
                p.IdempotencyKey == idempotencyKey && p.UserId == userId && p.EventId == eventId);

            var existingPrediction = existingByKey ?? existingByUserAndEvent;
            if (existingPrediction is not null)
            {
                var summary = ToUserPredictionSummaryDto(existingPrediction, ev);
                return StoreResult<CreatePredictionResponse>.Ok(
                    new CreatePredictionResponse(
                        Prediction: summary,
                        UpdatedBalance: user.Balance,
                        IsDuplicateReplay: true),
                    statusCode: 200);
            }

            // Scenario A4: hybrid deadline check (status != open or clock_timestamp() >= accept_until)
            var now = DateTimeOffset.UtcNow;
            var effectiveStatus = GetEffectiveStatus(ev, now);
            if (effectiveStatus != EventStatus.Open)
            {
                return StoreResult<CreatePredictionResponse>.Fail(
                    409,
                    ErrorCode.EventClosed,
                    "Приём прогнозов на это событие уже закрыт.");
            }

            // Field validations
            var fieldErrors = new Dictionary<string, string[]>();
            var outcome = ev.Outcomes.FirstOrDefault(o => o.Id == request.OutcomeId);
            if (outcome is null)
            {
                fieldErrors["outcomeId"] = ["Выбранный вариант исхода не принадлежит данному событию."];
            }

            if (request.Stake < 0)
            {
                fieldErrors["stake"] = ["Введите неотрицательное целое число баллов."];
            }

            if (request.Thesis is { Length: > 500 })
            {
                fieldErrors["thesis"] = ["Тезис не должен превышать 500 символов."];
            }

            if (request.EstimatedProbability is < 1 or > 99)
            {
                fieldErrors["estimatedProbability"] = ["Оценка вероятности должна быть в диапазоне от 1 до 99%."];
            }

            if (fieldErrors.Count > 0)
            {
                return StoreResult<CreatePredictionResponse>.Fail(
                    400,
                    ErrorCode.ValidationError,
                    "Проверьте корректность заполнения полей прогноза.",
                    fieldErrors);
            }

            if (user.Balance < request.Stake)
            {
                return StoreResult<CreatePredictionResponse>.Fail(
                    400,
                    ErrorCode.InsufficientBalance,
                    "Недостаточно игровых баллов на балансе для выбранной ставки.",
                    new Dictionary<string, string[]>
                    {
                        ["stake"] = ["Недостаточно баллов на балансе"]
                    });
            }

            // Atomic balance deduction, ledger entry, and prediction creation
            user.Balance -= request.Stake;
            user.WalletUpdatedAt = now;

            var shortCode = GenerateUniqueShortCode();
            var prediction = new PredictionEntity
            {
                Id = Guid.NewGuid(),
                ShortCode = shortCode,
                UserId = user.Id,
                EventId = ev.Id,
                OutcomeId = outcome!.Id,
                StakeAmount = request.Stake,
                EstimatedProbability = request.EstimatedProbability,
                ThesisText = string.IsNullOrWhiteSpace(request.Thesis) ? null : request.Thesis.Trim(),
                IdempotencyKey = idempotencyKey,
                CreatedAt = now,
                OgImageUrl = $"https://cdn.sporim.ru/og/{shortCode}.png",
                IsWin = null,
                PayoutAmount = null
            };

            _predictions.Add(prediction);

            if (request.Stake > 0)
            {
                _ledger.Add(new LedgerEntryEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    AmountDelta = -request.Stake,
                    Reason = LedgerReason.PredictionStake,
                    ReferenceType = LedgerReferenceType.Prediction,
                    ReferenceId = prediction.Id,
                    Description = $"Прогноз на событие: {ev.Title}",
                    CreatedAt = now
                });
            }

            var createdDto = ToUserPredictionSummaryDto(prediction, ev);
            return StoreResult<CreatePredictionResponse>.Ok(
                new CreatePredictionResponse(
                    Prediction: createdDto,
                    UpdatedBalance: user.Balance,
                    IsDuplicateReplay: false),
                statusCode: 201);
        }
    }

    public StoreResult<List<MyPredictionItemDto>> GetMyPredictions(
        Guid userId,
        string? statusRaw,
        string? categoryRaw)
    {
        EventStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(statusRaw))
        {
            if (!EnumParser.TryParseStatus(statusRaw, out var parsedStatus))
            {
                return StoreResult<List<MyPredictionItemDto>>.Fail(
                    400,
                    ErrorCode.ValidationError,
                    $"Недопустимый статус: {statusRaw}");
            }

            statusFilter = parsedStatus;
        }

        EventCategory? categoryFilter = null;
        if (!string.IsNullOrWhiteSpace(categoryRaw))
        {
            if (!EnumParser.TryParseCategory(categoryRaw, out var parsedCategory))
            {
                return StoreResult<List<MyPredictionItemDto>>.Fail(
                    400,
                    ErrorCode.ValidationError,
                    $"Недопустимая категория: {categoryRaw}");
            }

            categoryFilter = parsedCategory;
        }

        lock (_sync)
        {
            var now = DateTimeOffset.UtcNow;
            var items = new List<MyPredictionItemDto>();

            foreach (var pred in _predictions
                         .Where(p => p.UserId == userId)
                         .OrderByDescending(p => p.CreatedAt))
            {
                if (!_events.TryGetValue(pred.EventId, out var ev))
                {
                    continue;
                }

                var effectiveStatus = GetEffectiveStatus(ev, now);
                if (statusFilter.HasValue && effectiveStatus != statusFilter.Value)
                {
                    continue;
                }

                if (categoryFilter.HasValue && ev.Category != categoryFilter.Value)
                {
                    continue;
                }

                items.Add(new MyPredictionItemDto(
                    Prediction: ToUserPredictionSummaryDto(pred, ev),
                    EventId: ev.Id,
                    EventTitle: ev.Title,
                    EventSlug: ev.Slug,
                    EventCategory: ev.Category,
                    EventStatus: effectiveStatus,
                    EventDate: ev.EventDate,
                    Resolution: ev.Resolution));
            }

            return StoreResult<List<MyPredictionItemDto>>.Ok(items);
        }
    }

    public StoreResult<PublicProofDto> GetPublicProof(string shortCode)
    {
        lock (_sync)
        {
            var pred = _predictions.FirstOrDefault(p =>
                string.Equals(p.ShortCode, shortCode, StringComparison.Ordinal));

            if (pred is null ||
                !_events.TryGetValue(pred.EventId, out var ev) ||
                !_users.TryGetValue(pred.UserId, out var author))
            {
                return StoreResult<PublicProofDto>.Fail(
                    404,
                    ErrorCode.ProofNotFound,
                    "Пруф-ссылка не найдена.");
            }

            var effectiveStatus = GetEffectiveStatus(ev, DateTimeOffset.UtcNow);
            var authorStats = ComputeUserStats(author, portfolioMode: false);
            var outcomeTitle = ev.Outcomes.FirstOrDefault(o => o.Id == pred.OutcomeId)?.Title ?? "Исход";

            var dto = new PublicProofDto(
                ShortCode: pred.ShortCode,
                Author: new ProofAuthorDto(
                    Username: author.Username,
                    DisplayName: author.DisplayName,
                    AvatarUrl: author.AvatarUrl,
                    RankCode: authorStats.RankCode,
                    WinRate: authorStats.WinRate),
                Event: new ProofEventDto(
                    Id: ev.Id,
                    Slug: ev.Slug,
                    Title: ev.Title,
                    Category: ev.Category,
                    Status: effectiveStatus,
                    EventDate: ev.EventDate),
                OutcomeTitle: outcomeTitle,
                StakeAmount: pred.StakeAmount,
                EstimatedProbability: pred.EstimatedProbability,
                ThesisText: pred.ThesisText,
                CreatedAt: pred.CreatedAt,
                OgImageUrl: pred.OgImageUrl,
                Resolution: ev.Resolution,
                IsWin: pred.IsWin);

            return StoreResult<PublicProofDto>.Ok(dto);
        }
    }

    public StoreResult<UserProfileDto> GetUserProfileById(Guid userId)
    {
        lock (_sync)
        {
            if (!_users.TryGetValue(userId, out var user))
            {
                return StoreResult<UserProfileDto>.Fail(
                    401,
                    ErrorCode.Unauthorized,
                    "Пользователь не найден.");
            }

            return StoreResult<UserProfileDto>.Ok(ToUserProfileDto(user, portfolioMode: false));
        }
    }

    public StoreResult<UserProfileDto> GetUserProfileByUsername(string username, bool portfolioMode)
    {
        lock (_sync)
        {
            var user = _users.Values.FirstOrDefault(u =>
                string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (user is null)
            {
                return StoreResult<UserProfileDto>.Fail(
                    404,
                    ErrorCode.ValidationError,
                    $"Пользователь «{username}» не найден.");
            }

            return StoreResult<UserProfileDto>.Ok(ToUserProfileDto(user, portfolioMode));
        }
    }

    public StoreResult<WalletSummaryDto> GetWallet(Guid userId)
    {
        lock (_sync)
        {
            if (!_users.TryGetValue(userId, out var user))
            {
                return StoreResult<WalletSummaryDto>.Fail(
                    401,
                    ErrorCode.Unauthorized,
                    "Пользователь не авторизован.");
            }

            var transactions = _ledger
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new WalletTransactionDto(
                    Id: t.Id,
                    AmountDelta: t.AmountDelta,
                    Reason: t.Reason,
                    ReferenceType: t.ReferenceType,
                    ReferenceId: t.ReferenceId,
                    Description: t.Description,
                    CreatedAt: t.CreatedAt))
                .ToList();

            return StoreResult<WalletSummaryDto>.Ok(new WalletSummaryDto(
                Balance: user.Balance,
                UpdatedAt: user.WalletUpdatedAt,
                Transactions: transactions));
        }
    }

    public StoreResult<List<LeaderboardItemDto>> GetLeaderboard(string? categoryRaw)
    {
        EventCategory? categoryFilter = null;
        if (!string.IsNullOrWhiteSpace(categoryRaw))
        {
            if (!EnumParser.TryParseCategory(categoryRaw, out var parsedCategory))
            {
                return StoreResult<List<LeaderboardItemDto>>.Fail(
                    400,
                    ErrorCode.ValidationError,
                    $"Неизвестная категория: {categoryRaw}");
            }

            categoryFilter = parsedCategory;
        }

        lock (_sync)
        {
            var entries = _users.Values
                .Where(u => u.Role == Role.User)
                .Select(u =>
                {
                    var stats = ComputeUserStats(u, portfolioMode: categoryFilter == EventCategory.Macro);
                    return new
                    {
                        User = u,
                        Stats = stats
                    };
                })
                .OrderByDescending(x => x.Stats.WinRate)
                .ThenByDescending(x => x.Stats.WinsCount)
                .ThenByDescending(x => x.User.Balance)
                .Select((x, index) => new LeaderboardItemDto(
                    RankPosition: index + 1,
                    Username: x.User.Username,
                    DisplayName: x.User.DisplayName,
                    AvatarUrl: x.User.AvatarUrl,
                    WinRate: x.Stats.WinRate,
                    WinsCount: x.Stats.WinsCount,
                    TotalResolved: x.Stats.ResolvedPredictions,
                    RankCode: x.Stats.RankCode))
                .ToList();

            return StoreResult<List<LeaderboardItemDto>>.Ok(entries);
        }
    }

    public StoreResult<List<EventDetailsDto>> GetAdminEvents(Guid currentUserId, string? statusRaw)
    {
        lock (_sync)
        {
            var authCheck = EnsureModeratorOrAdmin(currentUserId);
            if (authCheck is not null)
            {
                return StoreResult<List<EventDetailsDto>>.Fail(
                    authCheck.Value.StatusCode,
                    authCheck.Value.Error.Code,
                    authCheck.Value.Error.Message);
            }

            EventStatus? statusFilter = null;
            if (!string.IsNullOrWhiteSpace(statusRaw))
            {
                if (!EnumParser.TryParseStatus(statusRaw, out var parsedStatus))
                {
                    return StoreResult<List<EventDetailsDto>>.Fail(
                        400,
                        ErrorCode.ValidationError,
                        $"Недопустимый статус: {statusRaw}");
                }

                statusFilter = parsedStatus;
            }

            var now = DateTimeOffset.UtcNow;
            var items = _events.Values
                .Select(ev => (Entity: ev, EffectiveStatus: GetEffectiveStatus(ev, now)))
                .Where(x => !statusFilter.HasValue || x.EffectiveStatus == statusFilter.Value)
                .OrderByDescending(x => x.Entity.CreatedAt)
                .Select(x => ToEventDetailsDto(x.Entity, x.EffectiveStatus, currentUserId))
                .ToList();

            return StoreResult<List<EventDetailsDto>>.Ok(items);
        }
    }

    public StoreResult<EventDetailsDto> CreateDraftEvent(Guid currentUserId, DraftEventRequest request)
    {
        lock (_sync)
        {
            var authCheck = EnsureModeratorOrAdmin(currentUserId);
            if (authCheck is not null)
            {
                return StoreResult<EventDetailsDto>.Fail(
                    authCheck.Value.StatusCode,
                    authCheck.Value.Error.Code,
                    authCheck.Value.Error.Message);
            }

            var fieldErrors = ValidateDraftFields(
                request.Title,
                request.Description,
                request.ResolutionRules,
                request.EventDate,
                request.AcceptUntil,
                request.Outcomes);

            if (fieldErrors.Count > 0)
            {
                return StoreResult<EventDetailsDto>.Fail(
                    400,
                    ErrorCode.ValidationError,
                    "Ошибка валидации полей черновика события.",
                    fieldErrors);
            }

            var slug = string.IsNullOrWhiteSpace(request.Slug)
                ? GenerateUniqueSlug(request.Title)
                : EnsureUniqueSlug(request.Slug.Trim());

            var now = DateTimeOffset.UtcNow;
            var entity = new EventEntity
            {
                Id = Guid.NewGuid(),
                Slug = slug,
                CreatorId = currentUserId,
                Category = request.Category,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                ResolutionRules = request.ResolutionRules.Trim(),
                Status = EventStatus.Draft,
                EventDate = request.EventDate,
                AcceptUntil = request.AcceptUntil,
                BaseParticipantsCount = 0,
                CreatedAt = now,
                Outcomes = request.Outcomes
                    .OrderBy(o => o.SortOrder)
                    .Select(o => new OutcomeEntity
                    {
                        Id = Guid.NewGuid(),
                        Code = o.Code.Trim(),
                        Title = o.Title.Trim(),
                        SortOrder = o.SortOrder,
                        InitialVotes = 0
                    })
                    .ToList()
            };

            _events[entity.Id] = entity;
            return StoreResult<EventDetailsDto>.Ok(
                ToEventDetailsDto(entity, entity.Status, currentUserId),
                statusCode: 201);
        }
    }

    public StoreResult<EventDetailsDto> PatchDraftEvent(
        Guid currentUserId,
        Guid eventId,
        PatchDraftEventRequest request)
    {
        lock (_sync)
        {
            var authCheck = EnsureModeratorOrAdmin(currentUserId);
            if (authCheck is not null)
            {
                return StoreResult<EventDetailsDto>.Fail(
                    authCheck.Value.StatusCode,
                    authCheck.Value.Error.Code,
                    authCheck.Value.Error.Message);
            }

            if (!_events.TryGetValue(eventId, out var ev))
            {
                return StoreResult<EventDetailsDto>.Fail(
                    404,
                    ErrorCode.EventNotFound,
                    "Событие не найдено.");
            }

            if (ev.Status is not (EventStatus.Draft or EventStatus.OnModeration))
            {
                return StoreResult<EventDetailsDto>.Fail(
                    409,
                    ErrorCode.InvalidDraftState,
                    "Невозможно изменить уже опубликованное или завершённое событие.");
            }

            var newTitle = request.Title ?? ev.Title;
            var newDescription = request.Description ?? ev.Description;
            var newRules = request.ResolutionRules ?? ev.ResolutionRules;
            var newEventDate = request.EventDate ?? ev.EventDate;
            var newAcceptUntil = request.AcceptUntil ?? ev.AcceptUntil;
            var newOutcomes = request.Outcomes ?? ev.Outcomes
                .Select(o => new DraftOutcomeInput(o.Code, o.Title, o.SortOrder))
                .ToList();

            var fieldErrors = ValidateDraftFields(
                newTitle,
                newDescription,
                newRules,
                newEventDate,
                newAcceptUntil,
                newOutcomes);

            if (request.Status.HasValue && request.Status.Value is not (EventStatus.Draft or EventStatus.Open))
            {
                fieldErrors["status"] = ["Через PATCH черновик можно перевести только в статус draft или open."];
            }

            if (fieldErrors.Count > 0)
            {
                return StoreResult<EventDetailsDto>.Fail(
                    400,
                    ErrorCode.ValidationError,
                    "Ошибка валидации обновляемых полей черновика.",
                    fieldErrors);
            }

            ev.Title = newTitle.Trim();
            ev.Description = newDescription.Trim();
            ev.ResolutionRules = newRules.Trim();
            ev.EventDate = newEventDate;
            ev.AcceptUntil = newAcceptUntil;

            if (request.Category.HasValue)
            {
                ev.Category = request.Category.Value;
            }

            if (request.Outcomes is not null)
            {
                ev.Outcomes = request.Outcomes
                    .OrderBy(o => o.SortOrder)
                    .Select(o => new OutcomeEntity
                    {
                        Id = Guid.NewGuid(),
                        Code = o.Code.Trim(),
                        Title = o.Title.Trim(),
                        SortOrder = o.SortOrder,
                        InitialVotes = 0
                    })
                    .ToList();
            }

            if (request.Status.HasValue)
            {
                ev.Status = request.Status.Value;
            }

            var effectiveStatus = GetEffectiveStatus(ev, DateTimeOffset.UtcNow);
            return StoreResult<EventDetailsDto>.Ok(ToEventDetailsDto(ev, effectiveStatus, currentUserId));
        }
    }

    public StoreResult<bool> DeleteDraftEvent(Guid currentUserId, Guid eventId)
    {
        lock (_sync)
        {
            var authCheck = EnsureModeratorOrAdmin(currentUserId);
            if (authCheck is not null)
            {
                return StoreResult<bool>.Fail(
                    authCheck.Value.StatusCode,
                    authCheck.Value.Error.Code,
                    authCheck.Value.Error.Message);
            }

            if (!_events.TryGetValue(eventId, out var ev))
            {
                return StoreResult<bool>.Fail(
                    404,
                    ErrorCode.EventNotFound,
                    "Событие не найдено.");
            }

            var hasPredictions = _predictions.Any(p => p.EventId == eventId);
            if (ev.Status != EventStatus.Draft || hasPredictions)
            {
                return StoreResult<bool>.Fail(
                    409,
                    ErrorCode.InvalidDraftState,
                    "Запрещено удалять опубликованное событие или черновик с существующими прогнозами.");
            }

            _events.Remove(eventId);
            return StoreResult<bool>.Ok(true, statusCode: 204);
        }
    }

    public StoreResult<EventDetailsDto> ResolveEvent(
        Guid currentUserId,
        Guid eventId,
        ResolveEventRequest request)
    {
        lock (_sync)
        {
            var authCheck = EnsureModeratorOrAdmin(currentUserId);
            if (authCheck is not null)
            {
                return StoreResult<EventDetailsDto>.Fail(
                    authCheck.Value.StatusCode,
                    authCheck.Value.Error.Code,
                    authCheck.Value.Error.Message);
            }

            if (!_events.TryGetValue(eventId, out var ev))
            {
                return StoreResult<EventDetailsDto>.Fail(
                    404,
                    ErrorCode.EventNotFound,
                    "Событие не найдено.");
            }

            var now = DateTimeOffset.UtcNow;
            var effectiveStatus = GetEffectiveStatus(ev, now);
            if (effectiveStatus is not (EventStatus.Open or EventStatus.AwaitingResult))
            {
                return StoreResult<EventDetailsDto>.Fail(
                    409,
                    ErrorCode.InvalidDraftState,
                    "Событие уже рассчитано или ещё не было опубликовано.");
            }

            var fieldErrors = new Dictionary<string, string[]>();
            if (ev.Outcomes.All(o => o.Id != request.WinningOutcomeId))
            {
                fieldErrors["winningOutcomeId"] = ["Указанный победный исход не найден в этом событии."];
            }

            if (string.IsNullOrWhiteSpace(request.FactSummary) || request.FactSummary.Trim().Length < 5)
            {
                fieldErrors["factSummary"] = ["Описание свершившегося факта должно содержать минимум 5 символов."];
            }

            if (fieldErrors.Count > 0)
            {
                return StoreResult<EventDetailsDto>.Fail(
                    400,
                    ErrorCode.ValidationError,
                    "Проверьте корректность полей резолюции события.",
                    fieldErrors);
            }

            ev.Status = EventStatus.Resolved;
            ev.Resolution = new ResolutionDto(
                WinningOutcomeId: request.WinningOutcomeId,
                FactSummary: request.FactSummary.Trim(),
                SourceUrl: string.IsNullOrWhiteSpace(request.SourceUrl) ? null : request.SourceUrl.Trim(),
                ResolvedAt: now);

            // Settlement pipeline: calculate payouts, update predictions and credit user wallets/ledgers
            foreach (var pred in _predictions.Where(p => p.EventId == ev.Id))
            {
                var isWin = pred.OutcomeId == request.WinningOutcomeId;
                pred.IsWin = isWin;

                if (isWin)
                {
                    var payout = pred.StakeAmount > 0
                        ? (long)Math.Round(pred.StakeAmount * 1.8m, MidpointRounding.AwayFromZero)
                        : 0L;
                    pred.PayoutAmount = payout;

                    if (payout > 0 && _users.TryGetValue(pred.UserId, out var winner))
                    {
                        winner.Balance += payout;
                        winner.WalletUpdatedAt = now;

                        _ledger.Add(new LedgerEntryEntity
                        {
                            Id = Guid.NewGuid(),
                            UserId = winner.Id,
                            AmountDelta = payout,
                            Reason = LedgerReason.PredictionPayout,
                            ReferenceType = LedgerReferenceType.Event,
                            ReferenceId = ev.Id,
                            Description = $"Выплата за точный прогноз: {ev.Title}",
                            CreatedAt = now
                        });
                    }
                }
                else
                {
                    pred.PayoutAmount = 0L;
                }
            }

            return StoreResult<EventDetailsDto>.Ok(ToEventDetailsDto(ev, EventStatus.Resolved, currentUserId));
        }
    }

    public StoreResult<EventDetailsDto> ExpireEventDeadlineForDemo(Guid eventId)
    {
        lock (_sync)
        {
            if (!_events.TryGetValue(eventId, out var ev))
            {
                return StoreResult<EventDetailsDto>.Fail(
                    404,
                    ErrorCode.EventNotFound,
                    "Событие не найдено.");
            }

            var now = DateTimeOffset.UtcNow;
            ev.AcceptUntil = now.AddSeconds(-5);
            var effectiveStatus = GetEffectiveStatus(ev, now);
            return StoreResult<EventDetailsDto>.Ok(ToEventDetailsDto(ev, effectiveStatus, null));
        }
    }

    private (int StatusCode, ApiError Error)? EnsureModeratorOrAdmin(Guid userId)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            return (401, new ApiError(ErrorCode.Unauthorized, "Требуется авторизация."));
        }

        if (user.Role is not (Role.Moderator or Role.Admin))
        {
            return (403, new ApiError(ErrorCode.Forbidden, "Доступ разрешён только модераторам и администраторам."));
        }

        return null;
    }

    private static Dictionary<string, string[]> ValidateDraftFields(
        string? title,
        string? description,
        string? resolutionRules,
        DateTimeOffset eventDate,
        DateTimeOffset acceptUntil,
        List<DraftOutcomeInput>? outcomes)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 10 || title.Trim().Length > 255)
        {
            errors["title"] = ["Название события должно содержать от 10 до 255 символов."];
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            errors["description"] = ["Описание события обязательно."];
        }

        if (string.IsNullOrWhiteSpace(resolutionRules))
        {
            errors["resolutionRules"] = ["Правила расчёта исхода обязательны."];
        }

        if (acceptUntil > eventDate)
        {
            errors["acceptUntil"] = ["Срок окончания приёма прогнозов (acceptUntil) должен быть меньше или равен дате события (eventDate)."];
        }

        if (outcomes is null || outcomes.Count < 2)
        {
            errors["outcomes"] = ["Необходимо указать минимум 2 варианта исхода."];
        }
        else if (outcomes.Any(o => string.IsNullOrWhiteSpace(o.Code) || string.IsNullOrWhiteSpace(o.Title)))
        {
            errors["outcomes"] = ["У каждого исхода должны быть заполнены код и название."];
        }

        return errors;
    }

    private static EventStatus GetEffectiveStatus(EventEntity ev, DateTimeOffset now)
    {
        if (ev.Status == EventStatus.Open && now >= ev.AcceptUntil)
        {
            return EventStatus.AwaitingResult;
        }

        return ev.Status;
    }

    private EventCardItemDto ToEventCardItemDto(EventEntity ev, EventStatus effectiveStatus, Guid? currentUserId)
    {
        var (outcomes, participantsCount) = ComputeOutcomesAndParticipants(ev);
        var myPrediction = GetUserPredictionSummary(ev, currentUserId);
        var snippet = ev.Description.Length <= 140
            ? ev.Description
            : ev.Description[..137] + "...";

        return new EventCardItemDto(
            Id: ev.Id,
            Slug: ev.Slug,
            Category: ev.Category,
            Title: ev.Title,
            DescriptionSnippet: snippet,
            Status: effectiveStatus,
            EventDate: ev.EventDate,
            AcceptUntil: ev.AcceptUntil,
            ParticipantsCount: participantsCount,
            Outcomes: outcomes,
            MyPrediction: myPrediction);
    }

    private EventDetailsDto ToEventDetailsDto(EventEntity ev, EventStatus effectiveStatus, Guid? currentUserId)
    {
        var card = ToEventCardItemDto(ev, effectiveStatus, currentUserId);
        return new EventDetailsDto(
            Id: card.Id,
            Slug: card.Slug,
            Category: card.Category,
            Title: card.Title,
            DescriptionSnippet: card.DescriptionSnippet,
            Status: card.Status,
            EventDate: card.EventDate,
            AcceptUntil: card.AcceptUntil,
            ParticipantsCount: card.ParticipantsCount,
            Outcomes: card.Outcomes,
            MyPrediction: card.MyPrediction,
            Description: ev.Description,
            ResolutionRules: ev.ResolutionRules,
            Resolution: ev.Resolution);
    }

    private (List<OutcomeDto> Outcomes, int ParticipantsCount) ComputeOutcomesAndParticipants(EventEntity ev)
    {
        var eventPredictions = _predictions.Where(p => p.EventId == ev.Id).ToList();
        var voteCounts = new Dictionary<Guid, int>();

        foreach (var outcome in ev.Outcomes)
        {
            var dynamicVotes = eventPredictions.Count(p => p.OutcomeId == outcome.Id);
            voteCounts[outcome.Id] = outcome.InitialVotes + dynamicVotes;
        }

        var totalVotes = voteCounts.Values.Sum();
        var outcomeDtos = ev.Outcomes
            .OrderBy(o => o.SortOrder)
            .Select(o =>
            {
                double? consensus = totalVotes > 0
                    ? Math.Round(voteCounts[o.Id] * 100.0 / totalVotes, 1)
                    : null;

                return new OutcomeDto(
                    Id: o.Id,
                    Code: o.Code,
                    Title: o.Title,
                    SortOrder: o.SortOrder,
                    ConsensusPercent: consensus);
            })
            .ToList();

        return (outcomeDtos, totalVotes);
    }

    private UserPredictionSummaryDto? GetUserPredictionSummary(EventEntity ev, Guid? currentUserId)
    {
        if (!currentUserId.HasValue)
        {
            return null;
        }

        var pred = _predictions.FirstOrDefault(p =>
            p.EventId == ev.Id && p.UserId == currentUserId.Value);

        return pred is null ? null : ToUserPredictionSummaryDto(pred, ev);
    }

    private static UserPredictionSummaryDto ToUserPredictionSummaryDto(PredictionEntity pred, EventEntity ev)
    {
        var outcomeTitle = ev.Outcomes.FirstOrDefault(o => o.Id == pred.OutcomeId)?.Title ?? "Исход";
        return new UserPredictionSummaryDto(
            Id: pred.Id,
            ShortCode: pred.ShortCode,
            ProofUrl: $"/p/{pred.ShortCode}",
            OutcomeId: pred.OutcomeId,
            OutcomeTitle: outcomeTitle,
            StakeAmount: pred.StakeAmount,
            EstimatedProbability: pred.EstimatedProbability,
            ThesisText: pred.ThesisText,
            CreatedAt: pred.CreatedAt,
            IsWin: pred.IsWin,
            PayoutAmount: pred.PayoutAmount);
    }

    private UserProfileDto ToUserProfileDto(UserEntity user, bool portfolioMode)
    {
        var stats = ComputeUserStats(user, portfolioMode);
        return new UserProfileDto(
            Id: user.Id,
            Username: user.Username,
            DisplayName: user.DisplayName,
            AvatarUrl: user.AvatarUrl,
            Role: user.Role,
            Balance: user.Balance,
            Stats: stats,
            CreatedAt: user.CreatedAt);
    }

    private UserStatsDto ComputeUserStats(UserEntity user, bool portfolioMode)
    {
        var userPredictions = _predictions
            .Where(p => p.UserId == user.Id)
            .Where(p => !portfolioMode || (_events.TryGetValue(p.EventId, out var ev) && ev.Category == EventCategory.Macro))
            .OrderBy(p => p.CreatedAt)
            .ToList();

        var dynamicTotal = userPredictions.Count;
        var resolvedList = userPredictions.Where(p => p.IsWin.HasValue).ToList();
        var dynamicResolved = resolvedList.Count;
        var dynamicWins = resolvedList.Count(p => p.IsWin == true);

        var totalPredictions = portfolioMode ? dynamicTotal : user.BaseTotalPredictions + dynamicTotal;
        var resolvedPredictions = portfolioMode ? dynamicResolved : user.BaseResolvedPredictions + dynamicResolved;
        var winsCount = portfolioMode ? dynamicWins : user.BaseWinsCount + dynamicWins;

        var winRate = resolvedPredictions > 0
            ? Math.Round(winsCount * 100.0 / resolvedPredictions, 2)
            : 0.0;

        // Calculate Brier Score if probabilities were provided
        var brierPredictions = resolvedList.Where(p => p.EstimatedProbability.HasValue).ToList();
        double? brierScore = user.BaseBrierScore;
        if (brierPredictions.Count > 0)
        {
            var avgDynamicBrier = brierPredictions.Average(p =>
            {
                var prob = p.EstimatedProbability!.Value / 100.0;
                var outcome = p.IsWin == true ? 1.0 : 0.0;
                return Math.Pow(prob - outcome, 2);
            });

            brierScore = user.BaseBrierScore.HasValue && !portfolioMode
                ? Math.Round((user.BaseBrierScore.Value + avgDynamicBrier) / 2.0, 4)
                : Math.Round(avgDynamicBrier, 4);
        }

        var currentStreak = portfolioMode ? 0 : user.BaseCurrentStreak;
        var maxStreak = portfolioMode ? 0 : user.BaseMaxStreak;
        foreach (var resolved in resolvedList)
        {
            if (resolved.IsWin == true)
            {
                currentStreak++;
                if (currentStreak > maxStreak)
                {
                    maxStreak = currentStreak;
                }
            }
            else
            {
                currentStreak = 0;
            }
        }

        var rankCode = resolvedPredictions switch
        {
            >= 25 when winRate >= 70.0 => "oracle",
            >= 15 when winRate >= 60.0 => "strategist",
            >= 5 => "analyst",
            _ => "novice"
        };

        return new UserStatsDto(
            TotalPredictions: totalPredictions,
            ResolvedPredictions: resolvedPredictions,
            WinsCount: winsCount,
            WinRate: winRate,
            BrierScore: brierScore,
            CurrentStreak: currentStreak,
            MaxStreak: maxStreak,
            RankCode: rankCode);
    }

    private string GenerateUniqueShortCode()
    {
        Span<byte> randomBytes = stackalloc byte[8];
        while (true)
        {
            Random.Shared.NextBytes(randomBytes);
            var sb = new StringBuilder(8);
            for (var i = 0; i < 8; i++)
            {
                sb.Append(Base62Alphabet[randomBytes[i] % Base62Alphabet.Length]);
            }

            var candidate = sb.ToString();
            if (_predictions.All(p => !string.Equals(p.ShortCode, candidate, StringComparison.Ordinal)))
            {
                return candidate;
            }
        }
    }

    private string EnsureUniqueUsername(string baseUsername)
    {
        var candidate = baseUsername;
        var counter = 1;
        while (_users.Values.Any(u => string.Equals(u.Username, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"{baseUsername}_{counter++}";
        }

        return candidate;
    }

    private string GenerateUniqueSlug(string title)
    {
        var slugBase = TransliterateToSlug(title);
        if (string.IsNullOrWhiteSpace(slugBase))
        {
            slugBase = "event-" + Guid.NewGuid().ToString("N")[..6];
        }

        return EnsureUniqueSlug(slugBase);
    }

    private string EnsureUniqueSlug(string baseSlug)
    {
        var candidate = baseSlug.Length > 140 ? baseSlug[..140].Trim('-') : baseSlug;
        var counter = 2;
        while (_events.Values.Any(e => string.Equals(e.Slug, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"{baseSlug}-{counter++}";
        }

        return candidate;
    }

    private static string TransliterateToSlug(string input)
    {
        var map = new Dictionary<char, string>
        {
            ['а'] = "a",
            ['б'] = "b",
            ['в'] = "v",
            ['г'] = "g",
            ['д'] = "d",
            ['е'] = "e",
            ['ё'] = "yo",
            ['ж'] = "zh",
            ['з'] = "z",
            ['и'] = "i",
            ['й'] = "y",
            ['к'] = "k",
            ['л'] = "l",
            ['м'] = "m",
            ['н'] = "n",
            ['о'] = "o",
            ['п'] = "p",
            ['р'] = "r",
            ['с'] = "s",
            ['т'] = "t",
            ['у'] = "u",
            ['ф'] = "f",
            ['х'] = "kh",
            ['ц'] = "ts",
            ['ч'] = "ch",
            ['ш'] = "sh",
            ['щ'] = "shch",
            ['ъ'] = "",
            ['ы'] = "y",
            ['ь'] = "",
            ['э'] = "e",
            ['ю'] = "yu",
            ['я'] = "ya"
        };

        var sb = new StringBuilder();
        foreach (var ch in input.ToLower(CultureInfo.InvariantCulture))
        {
            if (map.TryGetValue(ch, out var mapped))
            {
                sb.Append(mapped);
            }
            else if (ch is (>= 'a' and <= 'z') or (>= '0' and <= '9'))
            {
                sb.Append(ch);
            }
            else if (sb.Length > 0 && sb[^1] != '-')
            {
                sb.Append('-');
            }
        }

        return sb.ToString().Trim('-');
    }
}
