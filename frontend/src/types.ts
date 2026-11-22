export type EventCategory =
  | 'esports'
  | 'pop_culture'
  | 'streamers'
  | 'macro'
  | 'student_ugc'

export type EventStatus =
  | 'draft'
  | 'on_moderation'
  | 'open'
  | 'awaiting_result'
  | 'resolved'
  | 'rejected'
  | 'canceled'

export interface Outcome {
  id: string
  code: string
  title: string
  sortOrder: number
  consensusPercent: number | null
}

export interface UserPredictionSummary {
  id: string
  shortCode: string
  proofUrl: string
  outcomeId: string
  outcomeTitle: string
  stakeAmount: number
  createdAt: string
  isWin: boolean | null
}

export interface EventCardItem {
  id: string
  slug: string
  category: EventCategory
  title: string
  descriptionSnippet: string
  status: EventStatus
  eventDate: string
  acceptUntil: string
  participantsCount: number
  outcomes: Outcome[]
  myPrediction: UserPredictionSummary | null
}

export interface EventDetails extends EventCardItem {
  description: string
  resolutionRules: string
  resolution: {
    winningOutcomeId: string
    factSummary: string
    sourceUrl: string | null
    resolvedAt: string
  } | null
}

export interface EventCatalogResponse {
  items: EventCardItem[]
  total: number
  page: number
  pageSize: number
}

export interface EventFilters {
  search: string
  category: EventCategory | 'all'
  onlyActive: boolean
}
