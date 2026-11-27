import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { FavoritesProvider } from './components/FavoritesProvider'
import { FavoritesPage } from './pages/FavoritesPage'
import { Layout } from './components/Layout'
import { EventDetailsPage } from './pages/EventDetailsPage'
import { EventsPage } from './pages/EventsPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { WalletPage } from './pages/WalletPage'

const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      { index: true, element: <EventsPage /> },
      { path: 'events/:eventId', element: <EventDetailsPage /> },
      { path: 'favorites', element: <FavoritesPage /> },
      { path: 'wallet', element: <WalletPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])

export default function App() {
  return <FavoritesProvider><RouterProvider router={router} /></FavoritesProvider>
}
