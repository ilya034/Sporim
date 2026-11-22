import { test, afterEach } from 'node:test'
import assert from 'node:assert/strict'
import { mkdtempSync, rmSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { join } from 'node:path'
import { execFileSync } from 'node:child_process'
import { createRequire } from 'node:module'
import { fileURLToPath } from 'node:url'

// Run the actual TypeScript API module without an additional test dependency.
const compiledDir = mkdtempSync(join(tmpdir(), 'sporim-wallet-test-'))
let api
try {
  const tsc = new URL('../node_modules/typescript/bin/tsc', import.meta.url)
  const source = fileURLToPath(new URL('../src/api/wallet.ts', import.meta.url))
  execFileSync(process.execPath, [fileURLToPath(tsc), source, '--outDir', compiledDir, '--module', 'commonjs', '--target', 'es2022', '--skipLibCheck'], { cwd: compiledDir })
  api = createRequire(import.meta.url)(join(compiledDir, 'wallet.js'))
} finally {
  rmSync(compiledDir, { recursive: true, force: true })
}
const { parseWallet, fetchWallet, WalletError } = api
const originalFetch = globalThis.fetch
afterEach(() => { globalThis.fetch = originalFetch })
const fixture = () => ({ balance: 0, updatedAt: '2026-11-21T12:00:00Z', transactions: [] })

test('accepts zero balance and empty history', () => {
  assert.deepEqual(parseWallet(fixture()), fixture())
})
test('accepts all contract reasons including signed debits and refunds', () => {
  const wallet = fixture()
  wallet.transactions = ['welcome_bonus', 'prediction_stake', 'prediction_payout', 'refund'].map((reason, i) => ({
    id: String(i), amountDelta: i === 1 ? -250 : 1000, reason,
    referenceType: 'system', referenceId: 'user', createdAt: wallet.updatedAt,
  }))
  assert.equal(parseWallet(wallet).transactions[1].amountDelta, -250)
})
test('rejects negative, unsafe and malformed balances instead of displaying false numbers', () => {
  for (const balance of [-1, 1.5, '1000', Number.MAX_SAFE_INTEGER + 1, null]) {
    assert.throws(() => parseWallet({ ...fixture(), balance }), WalletError)
  }
  assert.throws(() => parseWallet({ ...fixture(), updatedAt: 'bad date' }), WalletError)
  assert.throws(() => parseWallet({ ...fixture(), transactions: [{}] }), WalletError)
})
test('does not request private wallet without a session', async () => {
  globalThis.fetch = () => assert.fail('unexpected network request')
  await assert.rejects(fetchWallet('http://api', null), (e) => e.status === 401)
})
test('sends bearer token, avoids cache, forwards cancellation, and parses response', async () => {
  const controller = new AbortController()
  globalThis.fetch = async (url, init) => {
    assert.equal(url, 'http://api/api/v1/wallet')
    assert.equal(init.headers.Authorization, 'Bearer test-token')
    assert.equal(init.cache, 'no-store')
    assert.equal(init.signal, controller.signal)
    return Response.json(fixture())
  }
  assert.deepEqual(await fetchWallet('http://api/', 'test-token', controller.signal), fixture())
})
test('handles expired session, forbidden access and server errors without substituting demo balance', async () => {
  for (const status of [401, 403, 500]) {
    globalThis.fetch = async () => new Response(null, { status })
    await assert.rejects(fetchWallet('http://api', 'token'), (e) => e.status === status)
  }
})
test('propagates network failures for retry', async () => {
  globalThis.fetch = async () => { throw new TypeError('offline') }
  await assert.rejects(fetchWallet('http://api', 'token'), /offline/)
})
