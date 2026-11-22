export type LedgerReason = 'welcome_bonus' | 'prediction_stake' | 'prediction_payout' | 'refund'

export interface WalletTransaction {
  id: string
  amountDelta: number
  reason: LedgerReason
  referenceType: 'event' | 'prediction' | 'system'
  referenceId: string
  description?: string | null
  createdAt: string
}

export interface WalletSummary {
  balance: number
  updatedAt: string
  transactions: WalletTransaction[]
}

export class WalletError extends Error {
  constructor(message: string, public readonly status: number) {
    super(message)
  }
}

const reasons = new Set(['welcome_bonus', 'prediction_stake', 'prediction_payout', 'refund'])
const references = new Set(['event', 'prediction', 'system'])
const isDate = (value: unknown) => typeof value === 'string' && Number.isFinite(Date.parse(value))
const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null

export function parseWallet(value: unknown): WalletSummary {
  if (!isRecord(value) || !Number.isSafeInteger(value.balance) ||
    (value.balance as number) < 0 || !isDate(value.updatedAt) || !Array.isArray(value.transactions) ||
    !value.transactions.every((item: unknown) => isRecord(item) &&
      typeof item.id === 'string' && Number.isSafeInteger(item.amountDelta) &&
      reasons.has(item.reason as string) && references.has(item.referenceType as string) &&
      typeof item.referenceId === 'string' && isDate(item.createdAt) &&
      (item.description == null || typeof item.description === 'string'))) {
    throw new WalletError('Сервер вернул некорректные данные кошелька', 502)
  }
  return value as unknown as WalletSummary
}

export async function fetchWallet(baseUrl: string, token: string | null, signal?: AbortSignal): Promise<WalletSummary> {
  if (!token) throw new WalletError('Войдите в аккаунт, чтобы увидеть свой баланс', 401)
  const response = await fetch(`${baseUrl.replace(/\/$/, '')}/api/v1/wallet`, {
    headers: { Authorization: `Bearer ${token}`, Accept: 'application/json' },
    cache: 'no-store',
    signal,
  })
  if (response.status === 401) throw new WalletError('Сессия истекла. Войдите в аккаунт повторно', 401)
  if (response.status === 403) throw new WalletError('Нет доступа к кошельку этого аккаунта', 403)
  if (!response.ok) throw new WalletError('Не удалось загрузить кошелёк. Попробуйте ещё раз', response.status)
  return parseWallet(await response.json())
}
