import { Outlet } from 'react-router-dom'
import { Header } from './Header'
import { ScrollToTop } from './ScrollToTop'

export function Layout() {
  return (
    <div className="app-shell">
      <ScrollToTop />
      <Header />
      <main>
        <Outlet />
      </main>
      <footer className="site-footer">
        <div className="footer-inner">
          <span>СПОРИМ · социальные прогнозы</span>
          <span>Учебный MVP · 2026</span>
        </div>
      </footer>
    </div>
  )
}
