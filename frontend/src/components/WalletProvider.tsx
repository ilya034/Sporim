import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from 'react'
import { fetchWallet, WalletError, type WalletSummary } from '../api/wallet'
import { mockWallet } from '../data/mockWallet'

const isDemo = import.meta.env.VITE_USE_MOCK_API !== 'false'
const baseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000'

interface WalletState {
  wallet: WalletSummary | null
  loading: boolean
  error: string | null
  unauthorized: boolean
  isDemo: boolean
  refresh: () => void
}

const WalletContext = createContext<WalletState | null>(null)

export function WalletProvider({ children }: { children: ReactNode }) {
  const [wallet, setWallet] = useState<WalletSummary | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [unauthorized, setUnauthorized] = useState(false)
  const request = useRef<AbortController | null>(null)

  const refresh = useCallback(() => {
    request.current?.abort()
    const controller = new AbortController()
    request.current = controller
    setLoading(true)
    setError(null)
    setUnauthorized(false)
    // Do not show a previous account's balance while checking the current session.
    setWallet(null)
    async function load() {
      try {
        const token = isDemo ? null : sessionStorage.getItem('sporim.accessToken')
        const result = isDemo ? structuredClone(mockWallet) : await fetchWallet(baseUrl, token, controller.signal)
        if (!controller.signal.aborted) setWallet(result)
      } catch (cause) {
        if (controller.signal.aborted) return
        setUnauthorized(cause instanceof WalletError && cause.status === 401)
        setError(cause instanceof WalletError ? cause.message : 'Нет связи с сервером. Проверьте соединение и повторите загрузку')
      } finally {
        if (!controller.signal.aborted) setLoading(false)
      }
    }
    void load()
  }, [])

  useEffect(() => {
    refresh()
    window.addEventListener('focus', refresh)
    window.addEventListener('sporim:wallet-changed', refresh)
    window.addEventListener('sporim:session-changed', refresh)
    return () => {
      request.current?.abort()
      window.removeEventListener('focus', refresh)
      window.removeEventListener('sporim:wallet-changed', refresh)
      window.removeEventListener('sporim:session-changed', refresh)
    }
  }, [refresh])

  return <WalletContext.Provider value={{ wallet, loading, error, unauthorized, isDemo, refresh }}>{children}</WalletContext.Provider>
}

export function useWallet() {
  const state = useContext(WalletContext)
  if (!state) throw new Error('useWallet requires WalletProvider')
  return state
}
