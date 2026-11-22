import { Link } from 'react-router-dom'

export function NotFoundPage() {
  return (
    <div className="page-container detail-message">
      <div className="message-state">
        <span className="message-icon">404</span>
        <h1>Страница не найдена</h1>
        <p>Такого адреса нет, но в каталоге есть актуальные события</p>
        <Link className="button button-primary" to="/">
          Перейти к событиям
        </Link>
      </div>
    </div>
  )
}
