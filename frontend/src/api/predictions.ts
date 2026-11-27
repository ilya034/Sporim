import { mockWallet } from '../data/mockWallet'
import type { EventDetails, UserPredictionSummary } from '../types'
import type { WalletSummary } from './wallet'

const demoStorageKey = 'sporim.demo-predictions'
const baseUrl = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000').replace(/\/$/, '')

interface DemoState {
  wallet: WalletSummary
  predictions: Record<string, UserPredictionSummary>
}

function readDemoState(): DemoState {
  const saved = sessionStorage.getItem(demoStorageKey)
  return saved ? JSON.parse(saved) as DemoState : { wallet: structuredClone(mockWallet), predictions: {} }
}

export function getDemoWallet() {
  return readDemoState().wallet
}

export function getDemoPrediction(eventId: string) {
  return readDemoState().predictions[eventId] ?? null
}

export function sessionHeaders() {
  const token = sessionStorage.getItem('sporim.accessToken')
  return token ? { Authorization: `Bearer ${token}` } : undefined
}

export function acceptsPredictions(event: EventDetails) {
  return event.status === 'open' && Date.parse(event.acceptUntil) > Date.now()
}

interface PredictionRequest {
  outcomeId: string
  stake: number
}

interface PredictionResponse {
  prediction: UserPredictionSummary
  updatedBalance: number
  isDuplicateReplay: boolean
}

export async function submitPrediction(
  event: EventDetails,
  request: PredictionRequest,
  key: string,
  isDemo: boolean,
): Promise<PredictionResponse> {
  if (isDemo) {
    const state = readDemoState()
    const existing = state.predictions[event.id] ?? event.myPrediction
    if (existing) {
      return { prediction: existing, updatedBalance: state.wallet.balance, isDuplicateReplay: true }
    }

    if (!acceptsPredictions(event)) throw new Error('Приём прогнозов уже закрыт')
    const outcome = event.outcomes.find((item) => item.id === request.outcomeId)
    if (!outcome || !Number.isSafeInteger(request.stake) || request.stake < 0) {
      throw new Error('Проверь выбранный исход и ставку')
    }
    if (request.stake > state.wallet.balance) throw new Error('Недостаточно баллов')

    const createdAt = new Date().toISOString()
    const prediction: UserPredictionSummary = {
      id: key,
      shortCode: '',
      proofUrl: '',
      outcomeId: outcome.id,
      outcomeTitle: outcome.title,
      stakeAmount: request.stake,
      createdAt,
      isWin: null,
    }
    state.predictions[event.id] = prediction
    state.wallet.balance -= request.stake
    state.wallet.updatedAt = createdAt
    if (request.stake > 0) {
      state.wallet.transactions.unshift({
        id: key,
        amountDelta: -request.stake,
        reason: 'prediction_stake',
        referenceType: 'prediction',
        referenceId: key,
        description: event.title,
        createdAt,
      })
    }
    sessionStorage.setItem(demoStorageKey, JSON.stringify(state))
    return { prediction, updatedBalance: state.wallet.balance, isDuplicateReplay: false }
  }

  const headers = sessionHeaders()
  if (!headers) throw new Error('Войдите, чтобы сделать прогноз')
  const response = await fetch(`${baseUrl}/api/v1/events/${encodeURIComponent(event.id)}/predictions`, {
    method: 'POST',
    headers: { ...headers, 'Content-Type': 'application/json', 'Idempotency-Key': key },
    body: JSON.stringify(request),
  })
  if (!response.ok) {
    const error = await response.json().catch(() => null)
    if (response.status === 401) throw new Error('Сессия истекла. Войдите повторно')
    if (error?.code === 'EVENT_CLOSED') throw new Error('Приём прогнозов уже закрыт')
    throw new Error(error?.message ?? 'Не удалось отправить прогноз. Попробуй ещё раз')
  }
  return response.json() as Promise<PredictionResponse>
}
