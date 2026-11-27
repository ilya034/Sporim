import { createContext, useContext, useState, type ReactNode } from 'react'

const storageKey = 'sporim.favoriteEventIds'

function readFavorites(): string[] {
  try {
    const saved: unknown = JSON.parse(localStorage.getItem(storageKey) ?? '[]')
    return Array.isArray(saved) ? [...new Set(saved.filter((id): id is string => typeof id === 'string'))] : []
  } catch {
    return []
  }
}

const FavoritesContext = createContext<{
  favoriteIds: string[]
  toggleFavorite: (eventId: string) => void
} | null>(null)

export function FavoritesProvider({ children }: { children: ReactNode }) {
  const [favoriteIds, setFavoriteIds] = useState(readFavorites)

  function toggleFavorite(eventId: string) {
    const next = favoriteIds.includes(eventId)
      ? favoriteIds.filter((id) => id !== eventId)
      : [eventId, ...favoriteIds]
    setFavoriteIds(next)
    try {
      localStorage.setItem(storageKey, JSON.stringify(next))
    } catch {
      // Keep favorites usable for this visit when storage is unavailable.
    }
  }

  return <FavoritesContext.Provider value={{ favoriteIds, toggleFavorite }}>{children}</FavoritesContext.Provider>
}

export function useFavorites() {
  const state = useContext(FavoritesContext)
  if (!state) throw new Error('useFavorites requires FavoritesProvider')
  return state
}
