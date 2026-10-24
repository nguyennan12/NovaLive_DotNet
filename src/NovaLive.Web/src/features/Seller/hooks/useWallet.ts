import * as service from '../services/walletService'
import { useSellerMutation, useSellerQuery } from './useSellerQuery'
import type { DateRange, PageRequest } from '../types/contracts'

export const useWallet = () => useSellerQuery(['wallet'], service.getWallet)
export const useWalletTransactions = (params: PageRequest & DateRange & { type?: string }) => useSellerQuery(['wallet-transactions', params], () => service.getWalletTransactions(params))
export const useRequestPayout = () => useSellerMutation(service.requestPayout, 'Đã gửi yêu cầu rút tiền. Theo dõi trạng thái trong sổ ví.')
