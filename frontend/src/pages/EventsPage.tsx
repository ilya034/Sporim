import { useEffect, useState } from 'react'
import { getEvents } from '../api/events'
import { EventCard } from '../components/EventCard'
import { EventCardSkeleton } from '../components/EventCardSkeleton'
import { FilterBar } from '../components/FilterBar'
import type { EventCardItem, EventFilters } from '../types'

const initialFilters: EventFilters = {
  search: '',
  category: 'all',
  onlyActive: false,
}

export function EventsPage() {
  const [filters, setFilters] = useState(initialFilters)
  const [events, setEvents] = useState<EventCardItem[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [requestKey, setRequestKey] = useState(0)

  useEffect(() => {
    let ignore = false

    async function loadEvents() {
      setIsLoading(true)
      setError(null)
      try {
        const response = await getEvents(filters)
        if (!ignore) setEvents(response.items)
      } catch (requestError) {
        if (!ignore) {
          setError(
            requestError instanceof Error
              ? requestError.message
              : 'Не удалось загрузить события',
          )
        }
      } finally {
        if (!ignore) setIsLoading(false)
      }
    }

    void loadEvents()
    return () => {
      ignore = true
    }
  }, [filters, requestKey])

  return (
    <>
      <section className="hero">
        <div className="page-container hero-inner">
          <div className="hero-copy">
            <span className="eyebrow">Проверь свою интуицию</span>
            <h1>Делай прогнозы.<br />Доказывай, что был прав</h1>
            <p>
              Выбирай исход события, фиксируй мнение и делись пруфом с
              точным временем прогноза
            </p>
          </div>
          <div className="hero-proof" aria-hidden="true">
            <div className="proof-card proof-card-back" />
            <div className="proof-card proof-card-front">
              <span className="proof-label">ПРОГНОЗ ЗАФИКСИРОВАН</span>
              <strong>Team Spirit победит</strong>
              <div className="proof-score">
                <span>Да</span>
                <b>62%</b>
              </div>
              <span className="proof-time">21.11.2026 · 18:42</span>
            </div>
          </div>
        </div>
      </section>

      <section className="page-container catalog-section">
        <div className="section-heading">
          <div>
            <span className="eyebrow">Актуальные темы</span>
            <h2>События</h2>
          </div>
          {!isLoading && !error && (
            <span className="result-count">Найдено: {events.length}</span>
          )}
        </div>

        <FilterBar filters={filters} onChange={setFilters} />

        {isLoading && (
          <div className="events-grid" aria-label="Загрузка событий">
            {Array.from({ length: 6 }, (_, index) => (
              <EventCardSkeleton key={index} />
            ))}
          </div>
        )}

        {!isLoading && error && (
          <div className="message-state" role="alert">
            <span className="message-icon">!</span>
            <h2>События не загрузились</h2>
            <p>{error}</p>
            <button
              className="button button-primary"
              type="button"
              onClick={() => setRequestKey((value) => value + 1)}
            >
              Повторить
            </button>
          </div>
        )}

        {!isLoading && !error && events.length > 0 && (
          <div className="events-grid">
            {events.map((event) => (
              <EventCard event={event} key={event.id} />
            ))}
          </div>
        )}

        {!isLoading && !error && events.length === 0 && (
          <div className="message-state">
            <span className="message-icon">?</span>
            <h2>Ничего не найдено</h2>
            <p>Попробуй изменить запрос или выбрать другую категорию</p>
            <button
              className="button button-secondary"
              type="button"
              onClick={() => setFilters(initialFilters)}
            >
              Сбросить фильтры
            </button>
          </div>
        )}
      </section>

      <section className="how-it-works" id="how-it-works">
        <div className="page-container">
          <span className="eyebrow">Три шага</span>
          <h2>Как это работает</h2>
          <div className="steps-grid">
            <article>
              <span>01</span>
              <h3>Выбери событие</h3>
              <p>Найди интересную тему в каталоге</p>
            </article>
            <article>
              <span>02</span>
              <h3>Зафиксируй прогноз</h3>
              <p>Выбери исход и укажи игровые баллы</p>
            </article>
            <article>
              <span>03</span>
              <h3>Получи пруф</h3>
              <p>Поделись ссылкой с точным временем</p>
            </article>
          </div>
        </div>
      </section>
    </>
  )
}
