import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useWallet } from '../components/WalletProvider'
import type { LedgerReason } from '../api/wallet'

const labels: Record<LedgerReason, string> = {
  welcome_bonus: 'Стартовый бонус',
  prediction_stake: 'Ставка на прогноз',
  prediction_payout: 'Выплата за прогноз',
  refund: 'Возврат баллов',
}
const number = new Intl.NumberFormat('ru-RU')
const date = new Intl.DateTimeFormat('ru-RU', { dateStyle: 'medium', timeStyle: 'short' })

export function WalletPage() {
  const { wallet, loading, error, unauthorized, isDemo, refresh } = useWallet()
  const [filter, setFilter] = useState<'all' | 'income' | 'expense'>('all')
  const transactions = [...(wallet?.transactions ?? [])]
    .sort((a, b) => Date.parse(b.createdAt) - Date.parse(a.createdAt))
    .filter((item) => filter === 'all' || (filter === 'income' ? item.amountDelta > 0 : item.amountDelta < 0))

  return (
    <section className="page-container wallet-page">
      <div className="section-heading">
        <div><span className="eyebrow">Твои игровые баллы</span><h1>Кошелёк</h1></div>
        <button className="button button-ghost" onClick={refresh} disabled={loading}>
          {loading ? 'Обновляем…' : 'Обновить'}
        </button>
      </div>
      {isDemo && <p className="wallet-demo">Демонстрационный кошелёк · тестовые данные</p>}
      <div aria-live="polite" aria-busy={loading}>
        {loading && <div className="wallet-card wallet-loading" role="status">Загружаем баланс и операции…</div>}
        {!loading && error && (
          <div className="message-state" role="alert">
            <h2>{unauthorized ? 'Нужен вход в аккаунт' : 'Кошелёк недоступен'}</h2>
            <p>{error}</p>
            {unauthorized && <p>Авторизация подключается отдельно. После входа обновите кошелёк</p>}
            <button className="button button-primary" onClick={refresh}>Повторить загрузку</button>
          </div>
        )}
        {!loading && wallet && <>
          <div className="wallet-overview">
            <div className="wallet-card wallet-balance-card">
              <span className="eyebrow">Доступный баланс</span>
              <div className="wallet-amount">{number.format(wallet.balance)} <span>баллов</span></div>
              <p>{wallet.balance === 0 ? 'Пока нет баллов для новых прогнозов' : 'Баллы доступны для участия в прогнозах'}</p>
              <small>Обновлено: {date.format(new Date(wallet.updatedAt))}</small>
              <Link className="button button-primary" to="/">К событиям</Link>
            </div>
            <aside className="wallet-card wallet-info">
              <h2>Как работает баланс</h2>
              <p>При первом входе начисляется стартовый бонус. Баллы списываются при размещении прогноза, выплаты и возвраты появляются в истории операций</p>
              <p>Баллы игровые. Пополнение деньгами и вывод средств в этой версии недоступны</p>
            </aside>
          </div>
          <div className="wallet-history">
            <div className="section-heading"><h2>История операций</h2><span className="result-count">Операций: {transactions.length}</span></div>
            <div className="wallet-filters" role="group" aria-label="Фильтр операций">
              {([['all', 'Все'], ['income', 'Начисления'], ['expense', 'Списания']] as const).map(([value, label]) => (
                <button key={value} className="button button-ghost" aria-pressed={filter === value} onClick={() => setFilter(value)}>{label}</button>
              ))}
            </div>
            {transactions.length === 0 ? <div className="message-state"><h3>{wallet.transactions.length === 0 ? 'Операций пока нет' : 'Таких операций пока нет'}</h3><p>Здесь появятся начисления, ставки, выплаты и возвраты баллов</p></div> :
              <ul className="wallet-transactions">
                {transactions.map((item) => <li key={item.id} className="wallet-transaction">
                  <span className={`wallet-transaction-icon ${item.amountDelta < 0 ? 'expense' : 'income'}`} aria-hidden="true">{item.amountDelta < 0 ? '↗' : '↙'}</span>
                  <div className="wallet-transaction-copy"><strong>{labels[item.reason]}</strong><p>{item.description || labels[item.reason]}</p><time dateTime={item.createdAt}>{date.format(new Date(item.createdAt))}</time></div>
                  <strong className={`wallet-delta ${item.amountDelta < 0 ? 'expense' : 'income'}`}>{item.amountDelta > 0 ? '+' : ''}{number.format(item.amountDelta)} <span>б.</span></strong>
                </li>)}
              </ul>}
          </div>
        </>}
      </div>
    </section>
  )
}
