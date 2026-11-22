import { mockEvents } from '../data/mockEvents'
import type {
  EventCatalogResponse,
  EventDetails,
  EventFilters,
} from '../types'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, '') ?? 'http://localhost:5000'
const USE_MOCK_API = import.meta.env.VITE_USE_MOCK_API !== 'false'

const mockDelay = () => new Promise((resolve) => window.setTimeout(resolve, 350))

export async function getEvents(
  filters: EventFilters,
): Promise<EventCatalogResponse> {
  if (USE_MOCK_API) {
    await mockDelay()
    const normalizedSearch = filters.search.trim().toLowerCase()
    const items = mockEvents.filter((event) => {
      const matchesSearch =
        !normalizedSearch ||
        event.title.toLowerCase().includes(normalizedSearch) ||
        event.descriptionSnippet.toLowerCase().includes(normalizedSearch)
      const matchesCategory =
        filters.category === 'all' || event.category === filters.category
      const matchesActive = !filters.onlyActive || event.status === 'open'

      return matchesSearch && matchesCategory && matchesActive
    })

    return {
      items,
      total: items.length,
      page: 1,
      pageSize: 20,
    }
  }

  const params = new URLSearchParams()
  if (filters.search.trim()) params.set('search', filters.search.trim())
  if (filters.category !== 'all') params.set('category', filters.category)
  if (filters.onlyActive) params.set('onlyActive', 'true')

  const response = await fetch(`${API_BASE_URL}/api/v1/events?${params}`)
  if (!response.ok) {
    throw new Error('Не удалось загрузить события')
  }

  return response.json() as Promise<EventCatalogResponse>
}

export async function getEvent(eventId: string): Promise<EventDetails> {
  if (USE_MOCK_API) {
    await mockDelay()
    const event = mockEvents.find((item) => item.id === eventId)
    if (!event) throw new Error('Событие не найдено')
    return event
  }

  const response = await fetch(`${API_BASE_URL}/api/v1/events/${eventId}`)
  if (response.status === 404) throw new Error('Событие не найдено')
  if (!response.ok) throw new Error('Не удалось загрузить событие')
  return response.json() as Promise<EventDetails>
}
