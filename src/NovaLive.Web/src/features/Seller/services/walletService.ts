import * as mocks from '../mocks/wallet'
import { request } from './transport'

import type { DateRange, PagedResult, PageRequest, Payout, Wallet, WalletTransaction } from '../types/contracts'

export const getWallet = () => request<Wallet>('Ví', { url: 'seller/wallet' }, () => mocks.mockGetWallet())
export const getWalletTransactions = (params: PageRequest & DateRange & { type?: string }) => request<PagedResult<WalletTransaction>>('Sổ ví', { url: 'seller/wallet/transactions', params }, () => mocks.mockGetWalletTransactions(params))
export const requestPayout = (amount: number) => request<Payout>('Rút tiền', { method: 'POST', url: 'seller/wallet/payout', data: { amount } }, () => mocks.mockRequestPayout(amount))
