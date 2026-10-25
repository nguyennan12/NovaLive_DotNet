import { db, paginate, saveMock } from './database'
import type { DateRange, PagedResult, PageRequest, Payout, Wallet, WalletTransaction } from '../types/contracts'
import { validatePayout } from '../utils/business'

export function mockGetWallet(): Wallet | Promise<Wallet> { return structuredClone(db().wallet) }

export function mockGetWalletTransactions(params: PageRequest & DateRange & { type?: string }): PagedResult<WalletTransaction> | Promise<PagedResult<WalletTransaction>> { return paginate(db().transactions.filter(item => (!params.type || item.type === params.type) && (!params.from || item.createdAt >= params.from) && (!params.to || item.createdAt <= params.to)), params) }

export function mockRequestPayout(amount: number): Payout | Promise<Payout> {
  if (!validatePayout(amount, db().wallet.balance)) throw new Error('Số tiền rút không hợp lệ hoặc vượt quá số dư khả dụng.')
  db().wallet.balance -= amount; db().wallet.lockedBalance += amount
  const id = crypto.randomUUID(); const createdAt = new Date().toISOString()
  db().transactions.unshift({ id: crypto.randomUUID(), walletId: 'mock-wallet', amount: -amount, type: 'PayoutLock', description: `Lệnh rút ${id}`, balanceAfter: db().wallet.balance, createdAt }); saveMock()
  return { id, shopId: db().shop.id, amount, bankName: 'Vietcombank (mẫu)', bankAccount: db().shop.bankAccount || '', bankHolder: 'NGUYEN QUOC DAT (mẫu)', status: 'Pending', transferRef: null, reason: null, createdAt }
}
