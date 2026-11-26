import { useEffect, useRef, useState, type FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { getEvent } from '../api/events'
import { acceptsPredictions, sessionHeaders, submitPrediction } from '../api/predictions'
import { EventVisual } from '../components/EventVisual'
import { Icon } from '../components/Icon'
import { useWallet } from '../components/WalletProvider'
import type { EventDetails } from '../types'
import {
  categoryLabels,
  formatCompactNumber,
  formatDate,
  statusLabels,
} from '../utils/formatters'

export function EventDetailsPage() {
  const { eventId = '' } = useParams()
  return <EventDetailsContent key={eventId} eventId={eventId} />
}

function EventDetailsContent({ eventId }: { eventId: string }) {
  const { wallet, loading: walletLoading, error: walletError, isDemo } = useWallet()
  const [outcomeId, setOutcomeId] = useState('')
  const [stakeInput, setStakeInput] = useState('100')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submissionError, setSubmissionError] = useState('')
  const request = useRef<{ outcomeId: string; stake: number; key: string } | null>(null)
  const submitting = useRef(false)
  const [deadlinePassed, setDeadlinePassed] = useState(false)
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

  useEffect(() => {
    if (!event || !acceptsPredictions(event)) return
    const timer = window.setInterval(() => {
      if (!acceptsPredictions(event)) setDeadlinePassed(true)
    }, 1000)
    return () => window.clearInterval(timer)
  }, [event])

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

  const currentEvent = event
  const isOpen = !deadlinePassed && acceptsPredictions(event)
  const canPredict = isOpen && !event.myPrediction
  const needsLogin = !isDemo && !sessionHeaders()

  async function handlePrediction(submitEvent: FormEvent<HTMLFormElement>) {
    submitEvent.preventDefault()
    if (submitting.current || !canPredict || needsLogin || !wallet) return
    const stake = Number(stakeInput)
    if (!acceptsPredictions(currentEvent)) {
      setDeadlinePassed(true)
      return
    }
    if (!outcomeId || !stakeInput.trim() || !Number.isSafeInteger(stake) || stake < 0) {
      setSubmissionError('Выбери исход и введи целое неотрицательное число баллов')
      return
    }
    if (stake > wallet.balance) {
      setSubmissionError('Недостаточно баллов для этой ставки')
      return
    }

    if (request.current?.outcomeId !== outcomeId || request.current.stake !== stake) {
      request.current = { outcomeId, stake, key: crypto.randomUUID() }
    }
    submitting.current = true
    setIsSubmitting(true)
    setSubmissionError('')
    try {
      const { prediction } = await submitPrediction(currentEvent, { outcomeId, stake }, request.current.key, isDemo)
      setEvent({ ...currentEvent, myPrediction: prediction })
      window.dispatchEvent(new Event('sporim:wallet-changed'))
    } catch (cause) {
      setSubmissionError(cause instanceof TypeError
        ? 'Нет связи с сервером. Повтори отправку'
        : cause instanceof Error ? cause.message : 'Не удалось отправить прогноз')
    } finally {
      submitting.current = false
      setIsSubmitting(false)
    }
  }
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

          <form onSubmit={handlePrediction}>
            <fieldset className="outcome-list prediction-outcomes" disabled={isSubmitting}>
              <legend className="prediction-legend">{canPredict ? 'Выбери исход' : 'Исходы события'}</legend>
              {event.outcomes.map((outcome, index) => {
                const isWinner = winningOutcomeId === outcome.id
                const isMine = event.myPrediction?.outcomeId === outcome.id
                const OutcomeContainer = canPredict ? 'label' : 'div'
                return (
                  <OutcomeContainer
                    className={`outcome-option ${index === 0 ? 'yes' : 'no'}${isWinner ? ' winner' : ''}${canPredict ? ' selectable' : ''}${canPredict && outcomeId === outcome.id ? ' selected' : ''}`}
                    key={outcome.id}
                  >
                    {canPredict && (
                      <input
                        type="radio"
                        name="prediction-outcome"
                        value={outcome.id}
                        checked={outcomeId === outcome.id}
                        onChange={() => setOutcomeId(outcome.id)}
                        aria-label={outcome.title}
                        required
                      />
                    )}
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
                  </OutcomeContainer>
                )
              })}
            </fieldset>

            {event.myPrediction ? (
              <div className="panel-note" role="status">
                <span>Прогноз зафиксирован</span>
                {event.myPrediction.outcomeTitle} · {event.myPrediction.stakeAmount} б.
                {event.myPrediction.shortCode && (
                  <small>Код подтверждения · {event.myPrediction.shortCode}</small>
                )}
              </div>
            ) : canPredict ? (
              <fieldset className="prediction-form panel-note" disabled={isSubmitting || needsLogin || !wallet}>
                <label htmlFor="prediction-stake">Ставка, игровые баллы</label>
                <div className="prediction-controls">
                  <input
                    id="prediction-stake"
                    type="number"
                    min={0}
                    max={wallet?.balance}
                    step={1}
                    required
                    value={stakeInput}
                    onChange={(change) => setStakeInput(change.target.value)}
                  />
                  <button className="button button-primary" type="submit" disabled={!outcomeId}>
                    {isSubmitting ? 'Отправляем…' : 'Зафиксировать прогноз'}
                  </button>
                </div>
                <small>{needsLogin
                  ? 'Войди в аккаунт, чтобы отправить прогноз'
                  : wallet
                    ? `${isDemo ? 'Демо-баланс' : 'Доступно'} · ${wallet.balance} б. Прогноз нельзя изменить.`
                    : walletLoading ? 'Загружаем баланс…' : walletError}</small>
              </fieldset>
            ) : (
              <div className="panel-note closed">
                <span>Приём прогнозов завершён</span>
                Новые прогнозы на это событие больше не принимаются
              </div>
            )}
            {submissionError && <p className="prediction-error" role="alert">{submissionError}</p>}
          </form>
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
