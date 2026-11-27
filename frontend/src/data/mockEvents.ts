import type { EventDetails } from '../types'

export const mockEvents: EventDetails[] = [
  {
    id: '4b6b9157-878f-4b67-b09f-eab7abf0c001',
    slug: 'spirit-vs-falcons-major-final',
    category: 'esports',
    title: 'Победит ли Team Spirit в финале Major?',
    descriptionSnippet:
      'Финальный матч международного турнира. Серия пройдёт в формате best of 5',
    description:
      'Team Spirit встретится с Team Falcons в финале международного турнира. Победителем станет команда, которая первой выиграет три карты',
    resolutionRules:
      'Исход «Да» фиксируется, если Team Spirit побеждает в серии. Учитывается официальный результат организатора турнира',
    status: 'open',
    eventDate: '2026-12-02T16:00:00Z',
    acceptUntil: '2026-12-02T15:30:00Z',
    participantsCount: 2847,
    outcomes: [
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5161001',
        code: 'YES',
        title: 'Да, Team Spirit победит',
        sortOrder: 1,
        consensusPercent: 62,
      },
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5161002',
        code: 'NO',
        title: 'Нет, победит Team Falcons',
        sortOrder: 2,
        consensusPercent: 38,
      },
    ],
    myPrediction: null,
    resolution: null,
  },
  {
    id: '4b6b9157-878f-4b67-b09f-eab7abf0c002',
    slug: 'gta-vi-trailer-2026',
    category: 'pop_culture',
    title: 'Выйдет ли новый трейлер GTA VI до конца 2026 года?',
    descriptionSnippet:
      'Официальный ролик Rockstar Games длительностью от одной минуты',
    description:
      'Прогноз касается выхода нового официального трейлера GTA VI на ресурсах Rockstar Games до конца 2026 года',
    resolutionRules:
      'Исход «Да» фиксируется, если до 31 декабря 2026 года включительно Rockstar Games публикует новый официальный трейлер длительностью не менее одной минуты',
    status: 'open',
    eventDate: '2026-12-31T20:59:59Z',
    acceptUntil: '2026-12-20T20:59:59Z',
    participantsCount: 5194,
    outcomes: [
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5162001',
        code: 'YES',
        title: 'Да, трейлер выйдет',
        sortOrder: 1,
        consensusPercent: 71,
      },
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5162002',
        code: 'NO',
        title: 'Нет, трейлера не будет',
        sortOrder: 2,
        consensusPercent: 29,
      },
    ],
    myPrediction: null,
    resolution: null,
  },
  {
    id: '4b6b9157-878f-4b67-b09f-eab7abf0c003',
    slug: 'central-bank-rate-december',
    category: 'macro',
    title: 'Опустит ли ЦБ ключевую ставку ниже 15%?',
    descriptionSnippet:
      'Решение Банка России на последнем заседании в декабре',
    description:
      'Участники прогнозируют значение ключевой ставки Банка России после заключительного заседания совета директоров в декабре',
    resolutionRules:
      'Исход «Да» фиксируется, если опубликованное Банком России значение ключевой ставки составит менее 15%',
    status: 'open',
    eventDate: '2026-12-18T10:30:00Z',
    acceptUntil: '2026-12-18T10:00:00Z',
    participantsCount: 1284,
    outcomes: [
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5163001',
        code: 'YES',
        title: 'Да, ставка будет ниже 15%',
        sortOrder: 1,
        consensusPercent: 44,
      },
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5163002',
        code: 'NO',
        title: 'Нет, ставка будет 15% или выше',
        sortOrder: 2,
        consensusPercent: 56,
      },
    ],
    myPrediction: {
      id: 'a0ddf679-3f14-43b0-950f-97eed7103001',
      shortCode: 'ratePavel',
      proofUrl: '/p/ratePavel',
      outcomeId: 'f177fdad-0b4e-48bb-aee1-b2d1e5163001',
      outcomeTitle: 'Да, ставка будет ниже 15%',
      stakeAmount: 250,
      createdAt: '2026-11-18T09:47:00Z',
      isWin: null,
    },
    resolution: null,
  },
  {
    id: '4b6b9157-878f-4b67-b09f-eab7abf0c004',
    slug: 'streamer-million-subscribers',
    category: 'streamers',
    title: 'Наберёт ли канал 1 миллион подписчиков до Нового года?',
    descriptionSnippet:
      'Счётчик подписчиков должен достичь отметки до 31 декабря',
    description:
      'Прогноз считается исполненным после того, как публичный счётчик канала впервые покажет один миллион подписчиков',
    resolutionRules:
      'Исход определяется по публичному счётчику канала на 31 декабря 2026 года в 23:59 по московскому времени',
    status: 'awaiting_result',
    eventDate: '2026-11-20T20:59:59Z',
    acceptUntil: '2026-11-18T20:59:59Z',
    participantsCount: 936,
    outcomes: [
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5164001',
        code: 'YES',
        title: 'Да, наберёт',
        sortOrder: 1,
        consensusPercent: 53,
      },
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5164002',
        code: 'NO',
        title: 'Нет, не наберёт',
        sortOrder: 2,
        consensusPercent: 47,
      },
    ],
    myPrediction: null,
    resolution: null,
  },
  {
    id: '4b6b9157-878f-4b67-b09f-eab7abf0c005',
    slug: 'urfu-hackathon-students',
    category: 'student_ugc',
    title: 'Примут ли участие в хакатоне более 300 студентов?',
    descriptionSnippet:
      'Количество зарегистрированных участников студенческого хакатона УрФУ',
    description:
      'Прогноз на итоговое число подтверждённых участников студенческого хакатона УрФУ',
    resolutionRules:
      'Исход «Да» фиксируется при наличии в итоговом протоколе не менее 301 подтверждённого участника',
    status: 'resolved',
    eventDate: '2026-11-12T12:00:00Z',
    acceptUntil: '2026-11-10T12:00:00Z',
    participantsCount: 642,
    outcomes: [
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5165001',
        code: 'YES',
        title: 'Да, более 300',
        sortOrder: 1,
        consensusPercent: 67,
      },
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5165002',
        code: 'NO',
        title: 'Нет, 300 или меньше',
        sortOrder: 2,
        consensusPercent: 33,
      },
    ],
    myPrediction: null,
    resolution: {
      winningOutcomeId: 'f177fdad-0b4e-48bb-aee1-b2d1e5165001',
      factSummary: 'В итоговом протоколе зарегистрировано 347 участников',
      sourceUrl: 'https://urfu.ru',
      resolvedAt: '2026-11-12T15:10:00Z',
    },
  },
  {
    id: '4b6b9157-878f-4b67-b09f-eab7abf0c006',
    slug: 'dota-patch-before-december',
    category: 'esports',
    title: 'Выйдет ли крупное обновление Dota 2 до 1 декабря?',
    descriptionSnippet:
      'Учитывается обновление с изменениями баланса героев и предметов',
    description:
      'Прогноз на выпуск крупного игрового обновления Dota 2, которое содержит изменения баланса героев и предметов',
    resolutionRules:
      'Исход «Да» фиксируется после публикации Valve официального списка изменений до указанной даты',
    status: 'open',
    eventDate: '2026-12-01T20:59:59Z',
    acceptUntil: '2026-11-30T20:59:59Z',
    participantsCount: 3108,
    outcomes: [
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5166001',
        code: 'YES',
        title: 'Да, обновление выйдет',
        sortOrder: 1,
        consensusPercent: 58,
      },
      {
        id: 'f177fdad-0b4e-48bb-aee1-b2d1e5166002',
        code: 'NO',
        title: 'Нет, обновления не будет',
        sortOrder: 2,
        consensusPercent: 42,
      },
    ],
    myPrediction: null,
    resolution: null,
  },
]
