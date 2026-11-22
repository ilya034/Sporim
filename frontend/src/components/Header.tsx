import { Link, NavLink } from 'react-router-dom'

export function Header() {
  return (
    <header className="site-header">
      <div className="header-inner">
        <Link className="logo" to="/" aria-label="СПОРИМ, на главную">
          <span className="logo-mark" aria-hidden="true">
            С
          </span>
          <span>СПОРИМ</span>
        </Link>

        <nav className="main-nav" aria-label="Основная навигация">
          <NavLink to="/">События</NavLink>
          <a href="#how-it-works">Как это работает</a>
        </nav>

        <button className="button button-ghost header-login" type="button">
          Войти
        </button>
      </div>
    </header>
  )
}
