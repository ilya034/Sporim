import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { getEvent } from '../api/events'
import { EventCard } from '../components/EventCard'
import { useFavorites } from '../components/FavoritesProvider'
import type { EventDetails } from '../types'

interface FavoriteItem {
  id: string
  event: EventDetails | null
}

export function FavoritesPage() {
  const { favoriteIds, toggleFavorite } = useFavorites()
  const [items, setItems] = useState<FavoriteItem[]>([])
  const [loading, setLoading] = useState(true)
  const [attempt, setAttempt] = useState(0)

  useEffect(() => {
    let active = true
    Promise.all(favoriteIds.map(async (id) => {
      const event = await getEvent(id).catch(() => null)
      return { id, event }
    })).then((results) => {
      if (active) {
        setItems(results)
        setLoading(false)
      }
    })
    return () => { active = false }
  }, [favoriteIds, attempt])

  const visibleItems = items.filter(({ id }) => favoriteIds.includes(id))

  return (
    <section className="page-container favorites-page">
      <header>
        <h1>Избранное</h1>
        <p>События, за которыми ты следишь</p>
      </header>

      {favoriteIds.length === 0 ? (
        <div className="message-state">
          <h2>Пока нет избранных событий</h2>
          <p>Сохрани событие из каталога, чтобы быстро к нему вернуться.</p>
          <Link className="button button-primary" to="/">Перейти к событиям</Link>
        </div>
      ) : loading ? (
        <p role="status">Загружаем события…</p>
      ) : (
        <div className="events-grid">
          {visibleItems.map(({ id, event }) => event ? (
            <EventCard event={event} key={id} />
          ) : (
            <div className="detail-panel" key={id}>
              <h2>Событие недоступно</h2>
              <div className="favorites-actions">
                <button className="button button-secondary" type="button" onClick={() => {
                  setLoading(true)
                  setAttempt((value) => value + 1)
                }}>Повторить</button>
                <button className="button button-secondary" type="button" onClick={() => toggleFavorite(id)}>
                  Убрать из избранного
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
      {favoriteIds.length > 0 && <Link className="card-link" to="/">Добавить событие</Link>}
    </section>
  )
}
