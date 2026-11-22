import type { WalletSummary } from '../api/wallet'

// Separate demo fixture: the real wallet is always read from the server.
export const mockWallet: WalletSummary = {
  balance: 1000,
  updatedAt: '2026-11-21T12:00:00Z',
  transactions: [{
    id: 'demo-welcome',
    amountDelta: 1000,
    reason: 'welcome_bonus',
    referenceType: 'system',
    referenceId: 'demo-user',
    description: 'Стартовый бонус при регистрации в «СПОРИМ»',
    createdAt: '2026-11-21T12:00:00Z',
  }],
}
