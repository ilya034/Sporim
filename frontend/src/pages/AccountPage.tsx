import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getDemoWallet, getDemoPrediction, sessionHeaders } from '../api/predictions'
import { mockEvents } from '../data/mockEvents'
import type { UserPredictionSummary } from '../types'
import { formatDate } from '../utils/formatters'

interface Entry {
  eventId: string
  eventTitle: string
  prediction: UserPredictionSummary
}

interface Account {
  id: string
  displayName: string
  balance: number
  history: Entry[]
}

async function loadAccount(signal: AbortSignal): Promise<Account> {
  if (import.meta.env.VITE_USE_MOCK_API !== 'false') {
    const history = mockEvents.flatMap(event => {
      const prediction = getDemoPrediction(event.id) ?? event.myPrediction
      return prediction ? [{ eventId: event.id, eventTitle: event.title, prediction }] : []
    })
    return { id: 'demo', displayName: 'Демо-профиль', balance: getDemoWallet().balance, history }
  }
  const headers = sessionHeaders()
  if (!headers) throw new Error('Войдите в аккаунт, чтобы посмотреть историю')
  const base = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000').replace(/\/$/, '')
  async function get(path: string) {
    const response = await fetch(base + '/api/v1/' + path, { headers, signal, cache: 'no-store' })
    if (!response.ok) throw new Error(response.status === 401 ? 'Войдите в аккаунт повторно' : 'Не удалось загрузить профиль')
    return response.json()
  }
  const [profile, history] = await Promise.all([get('me'), get('predictions/my')])
  return { id: profile.id, displayName: profile.displayName, balance: profile.balance, history }
}

function ProfileSettings({ account }: { account: Account }) {
  const navigate = useNavigate()
  const storageKey = 'sporim.profile.' + account.id
  const [settings, setSettings] = useState(() => {
    const defaults = { name: account.displayName, email: '', notifications: true }
    try {
      const saved = JSON.parse(localStorage.getItem(storageKey) || 'null')
      return saved && typeof saved.name === 'string' && typeof saved.email === 'string'
        && typeof saved.notifications === 'boolean'
        ? { name: String(saved.name), email: String(saved.email), notifications: Boolean(saved.notifications) }
        : defaults
    } catch { return defaults }
  })
  const [message, setMessage] = useState('')
  return <form className="profile-settings" onChange={() => setMessage('')} onSubmit={event => {
    event.preventDefault()
    const name = settings.name.trim()
    if (!name) { setMessage('Введите имя'); return }
    try {
      localStorage.setItem(storageKey, JSON.stringify({ ...settings, name }))
      setSettings({ ...settings, name })
      setMessage('Изменения сохранены на этом устройстве')
    } catch { setMessage('Не удалось сохранить изменения на этом устройстве') }
  }}>
    <h2>Личные данные</h2>
    <label>Имя<input required maxLength={80} autoComplete="nickname" value={settings.name}
      onChange={event => setSettings({ ...settings, name: event.target.value })} /></label>
    <label>Email<input type="email" autoComplete="email" placeholder="name@example.com" value={settings.email}
      onChange={event => setSettings({ ...settings, email: event.target.value })} /></label>
    <label className="profile-toggle"><input type="checkbox" checked={settings.notifications}
      onChange={event => setSettings({ ...settings, notifications: event.target.checked })} />
      Уведомления об изменениях рынков и результатах</label>
    <div className="profile-actions"><button className="button button-primary">Сохранить</button>
      <button type="button" className="button button-secondary" onClick={() => {
        sessionStorage.removeItem('sporim.accessToken')
        window.dispatchEvent(new Event('sporim:session-changed'))
        navigate('/')
      }}>Выйти</button></div>
    {message && <p role="status">{message}</p>}
  </form>
}

export function AccountPage() {
  const [account, setAccount] = useState<Account | null>(null)
  const [error, setError] = useState('')
  const [attempt, setAttempt] = useState(0)
  useEffect(() => {
    const reload = () => setAttempt(value => value + 1)
    window.addEventListener('sporim:session-changed', reload)
    const controller = new AbortController()
    setAccount(null)
    setError('')
    loadAccount(controller.signal).then(result => {
      if (!controller.signal.aborted) setAccount(result)
    }).catch(cause => {
      if (!controller.signal.aborted) setError(cause instanceof TypeError ? 'Нет связи с сервером' : cause.message)
    })
    return () => {
      controller.abort()
      window.removeEventListener('sporim:session-changed', reload)
    }
  }, [attempt])

  return <section className="page-container favorites-page account-page">
    <header><h1>Профиль и настройки</h1><p>Управляйте данными и уведомлениями.</p></header>
    {error ? <div role="alert">{error} <button className="button button-secondary" onClick={() => setAttempt(value => value + 1)}>Повторить</button></div>
      : !account ? <p role="status">Загружаем профиль…</p>
      : <>
        <ProfileSettings key={account.id} account={account} />
        <details className="account-activity"><summary>Баланс и история прогнозов</summary>
        <div className="detail-panel">
          <p>Баланс · {account.balance.toLocaleString('ru-RU')} б.</p>
          <div className="favorites-actions"><Link className="card-link" to="/wallet">История операций</Link>
          <Link className="card-link" to="/favorites">Избранное</Link></div>
        </div>
        <h2>История прогнозов</h2>
        {account.history.length === 0 ? <div className="message-state"><h3>Портфель пока пуст</h3>
          <Link className="button button-primary" to="/">Выбрать первое событие</Link></div>
          : [...account.history].sort((a, b) => Date.parse(b.prediction.createdAt) - Date.parse(a.prediction.createdAt)).map(({ eventId, eventTitle, prediction }) =>
            <article className="detail-panel" key={prediction.id}>
              <Link className="card-link" to={'/events/' + eventId}>{eventTitle}</Link>
              <p>{prediction.outcomeTitle} · {prediction.stakeAmount} б.</p>
              <small>{formatDate(prediction.createdAt)} · {prediction.isWin == null ? 'Ожидает результата' : prediction.isWin ? 'Выигрыш' : 'Проигрыш'}</small>
            </article>)}
        </details>
      </>}
  </section>
}
