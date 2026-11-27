import { Link, NavLink } from 'react-router-dom'
import { useWallet } from './WalletProvider'

export function Header() {
  const { wallet, loading, isDemo } = useWallet()
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
          <NavLink to="/" end>События</NavLink>
          <NavLink to="/favorites">Избранное</NavLink>
          <NavLink to="/wallet">Кошелёк</NavLink>
          <NavLink to="/account">Личный кабинет</NavLink>
          <a href="#how-it-works">Как это работает</a>
        </nav>

        <Link className="header-wallet" to="/wallet" aria-label={wallet ? `Баланс: ${wallet.balance} игровых баллов${isDemo ? ', демо' : ''}` : 'Открыть кошелёк'}>
          <span>{isDemo ? 'Демо-баланс' : 'Игровой баланс'}</span>
          <strong>{loading ? '…' : wallet ? `${new Intl.NumberFormat('ru-RU').format(wallet.balance)} б.` : 'Кошелёк'}</strong>
        </Link>
      </div>
    </header>
  )
}
