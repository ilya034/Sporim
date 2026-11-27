import type { EventCategory, EventStatus } from '../types'

export const categoryLabels: Record<EventCategory, string> = {
  esports: 'Киберспорт',
  pop_culture: 'Поп-культура',
  streamers: 'Стримеры',
  macro: 'Экономика',
  student_ugc: 'Студенческое',
}

export const statusLabels: Record<EventStatus, string> = {
  draft: 'Черновик',
  on_moderation: 'На модерации',
  open: 'Приём открыт',
  awaiting_result: 'Ожидается результат',
  resolved: 'Завершено',
  rejected: 'Отклонено',
  canceled: 'Отменено',
}

export function formatDate(value: string, includeTime = true) {
  return new Intl.DateTimeFormat('ru-RU', {
    day: 'numeric',
    month: 'long',
    year: 'numeric',
    ...(includeTime ? { hour: '2-digit', minute: '2-digit' } : {}),
  }).format(new Date(value))
}

export function formatCompactNumber(value: number) {
  return new Intl.NumberFormat('ru-RU', {
    notation: value >= 1000 ? 'compact' : 'standard',
    maximumFractionDigits: 1,
  }).format(value)
}
