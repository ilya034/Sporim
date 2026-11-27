import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getEvent } from '../api/events'
import { EventVisual } from '../components/EventVisual'
import { Icon } from '../components/Icon'
import type { EventDetails } from '../types'
import {
  categoryLabels,
  formatCompactNumber,
  formatDate,
  statusLabels,
} from '../utils/formatters'

export function EventDetailsPage() {
  const { eventId = '' } = useParams()
  const [event, setEvent] = useState<EventDetails | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [requestKey, setRequestKey] = useState(0)

  useEffect(() => {
    let ignore = false
    setIsLoading(true)
    setError(null)

    getEvent(eventId)
      .then((response) => {
        if (!ignore) setEvent(response)
      })
      .catch((requestError: unknown) => {
        if (!ignore) {
          setError(
            requestError instanceof Error
              ? requestError.message
              : 'Не удалось загрузить событие',
          )
        }
      })
      .finally(() => {
        if (!ignore) setIsLoading(false)
      })

    return () => {
      ignore = true
    }
  }, [eventId, requestKey])

  if (isLoading) {
    return (
      <div className="page-container details-loading" aria-label="Загрузка события">
        <div className="skeleton skeleton-label" />
        <div className="skeleton skeleton-details-heading" />
        <div className="details-layout">
          <div className="skeleton skeleton-details-card" />
          <div className="skeleton skeleton-details-card" />
        </div>
      </div>
    )
  }

  if (error || !event) {
    return (
      <div className="page-container detail-message">
        <div className="message-state" role="alert">
          <span className="message-icon">!</span>
          <h1>{error ?? 'Событие не найдено'}</h1>
          <p>Проверь адрес или вернись к списку событий</p>
          <div className="message-actions">
            <Link className="button button-secondary" to="/">
              Вернуться в каталог
            </Link>
            <button
              className="button button-primary"
              type="button"
              onClick={() => setRequestKey((value) => value + 1)}
            >
              Повторить
            </button>
          </div>
        </div>
      </div>
    )
  }

  const isOpen = event.status === 'open'
  const winningOutcomeId = event.resolution?.winningOutcomeId

  return (
    <div className="page-container event-details-page">
      <Link className="back-link" to="/">
        <Icon name="arrow-left" size={19} />
        Все события
      </Link>

      <div className="details-heading">
        <div>
          <div className="card-badges">
            <span className="badge">{categoryLabels[event.category]}</span>
            <span className={`badge status-badge status-${event.status}`}>
              {statusLabels[event.status]}
            </span>
          </div>
          <h1>{event.title}</h1>
          <p>{event.description}</p>
        </div>
        <EventVisual category={event.category} />
      </div>

      <div className="event-facts">
        <div>
          <Icon name="clock" />
          <span>Приём прогнозов</span>
          <strong>{isOpen ? `до ${formatDate(event.acceptUntil)}` : 'завершён'}</strong>
        </div>
        <div>
          <Icon name="calendar" />
          <span>Дата события</span>
          <strong>{formatDate(event.eventDate)}</strong>
        </div>
        <div>
          <Icon name="users" />
          <span>Участников</span>
          <strong>{formatCompactNumber(event.participantsCount)}</strong>
        </div>
      </div>

      <div className="details-layout">
        <section className="detail-panel outcomes-panel">
          <div className="panel-heading">
            <span className="eyebrow">Мнение участников</span>
            <h2>Варианты исхода</h2>
          </div>

          <div className="outcome-list">
            {event.outcomes.map((outcome, index) => {
              const isWinner = winningOutcomeId === outcome.id
              const isMine = event.myPrediction?.outcomeId === outcome.id
              return (
                <article
                  className={`outcome-option ${index === 0 ? 'yes' : 'no'}${isWinner ? ' winner' : ''}`}
                  key={outcome.id}
                >
                  <div className="outcome-title">
                    <span>{outcome.code === 'YES' ? 'Да' : 'Нет'}</span>
                    <div>
                      <h3>{outcome.title}</h3>
                      {isMine && <small>Ваш прогноз · {event.myPrediction?.stakeAmount} баллов</small>}
                      {isWinner && <small><Icon name="check" size={15} /> Победивший исход</small>}
                    </div>
                    <strong>{outcome.consensusPercent ?? 0}%</strong>
                  </div>
                  <div className="consensus-track">
                    <span style={{ width: `${outcome.consensusPercent ?? 0}%` }} />
                  </div>
                </article>
              )
            })}
          </div>

          {isOpen ? (
            <div className="panel-note">
              <span>Прогнозы принимаются</span>
              Выбор исхода и отправка прогноза появятся в следующей задаче
            </div>
          ) : (
            <div className="panel-note closed">
              <span>{statusLabels[event.status]}</span>
              Новые прогнозы на это событие больше не принимаются
            </div>
          )}
        </section>

        <aside className="details-sidebar">
          <section className="detail-panel">
            <span className="eyebrow">Условия</span>
            <h2>Как определится результат</h2>
            <p>{event.resolutionRules}</p>
          </section>

          {event.resolution && (
            <section className="detail-panel resolution-panel">
              <span className="eyebrow">Официальный результат</span>
              <h2>Событие завершено</h2>
              <p>{event.resolution.factSummary}</p>
              <span className="resolution-date">
                Зафиксировано {formatDate(event.resolution.resolvedAt)}
              </span>
              {event.resolution.sourceUrl && (
                <a
                  className="source-link"
                  href={event.resolution.sourceUrl}
                  target="_blank"
                  rel="noreferrer"
                >
                  Открыть источник
                  <Icon name="arrow-right" size={17} />
                </a>
              )}
            </section>
          )}
        </aside>
      </div>
    </div>
  )
}
