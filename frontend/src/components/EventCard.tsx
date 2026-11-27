import { Link } from 'react-router-dom'
import type { EventCardItem } from '../types'
import {
  categoryLabels,
  formatCompactNumber,
  formatDate,
  statusLabels,
} from '../utils/formatters'
import { EventVisual } from './EventVisual'
import { useFavorites } from './FavoritesProvider'
import { Icon } from './Icon'

interface EventCardProps {
  event: EventCardItem
}

export function EventCard({ event }: EventCardProps) {
  const { favoriteIds, toggleFavorite } = useFavorites()
  const isFavorite = favoriteIds.includes(event.id)
  const isClosed = event.status !== 'open'

  return (
    <article className="event-card">
      <button
        className="favorite-toggle"
        type="button"
        aria-label={`Избранное: ${event.title}`}
        aria-pressed={isFavorite}
        onClick={() => toggleFavorite(event.id)}
      >
        <span aria-hidden="true">{isFavorite ? '★' : '☆'}</span>
      </button>
      <EventVisual category={event.category} compact />

      <div className="event-card-content">
        <div className="card-badges">
          <span className="badge">{categoryLabels[event.category]}</span>
          <span className={`badge status-badge status-${event.status}`}>
            {statusLabels[event.status]}
          </span>
        </div>

        <h2>
          <Link to={`/events/${event.id}`}>{event.title}</Link>
        </h2>
        <p>{event.descriptionSnippet}</p>

        <div className="card-meta">
          <span>
            <Icon name="clock" size={17} />
            {isClosed ? 'Приём завершён' : `До ${formatDate(event.acceptUntil)}`}
          </span>
          <span>
            <Icon name="users" size={17} />
            {formatCompactNumber(event.participantsCount)} прогнозов
          </span>
        </div>

        <div className="outcome-preview" aria-label="Распределение прогнозов">
          {event.outcomes.slice(0, 2).map((outcome, index) => (
            <span className={index === 0 ? 'yes' : 'no'} key={outcome.id}>
              {outcome.code === 'YES' ? 'Да' : 'Нет'}{' '}
              <strong>{outcome.consensusPercent ?? 0}%</strong>
            </span>
          ))}
        </div>

        <Link className="card-link" to={`/events/${event.id}`}>
          Открыть событие
          <Icon name="arrow-right" size={18} />
        </Link>
      </div>
    </article>
  )
}
