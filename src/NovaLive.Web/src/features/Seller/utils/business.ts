import { MIN_PAYOUT } from '../constants'

export const hasSellerRole = (roles?: readonly string[], role?: string) => (roles ?? (role ? [role] : [])).some(value => value.toLowerCase() === 'seller')
export const isMissingSellerEndpoint = (status?: number) => status === 404 || status === 501

export function validatePayout(amount: number, balance: number): boolean {
  return Number.isFinite(amount) && Number.isInteger(amount) && amount >= MIN_PAYOUT && amount <= balance
}
export function validateStockAdjustment(onHand: number, reserved: number, change: number, type: 'Import' | 'ManualAdjust'): string | null {
  if (!Number.isInteger(change) || change === 0) return 'Số lượng điều chỉnh phải là số nguyên khác 0.'
  if (type === 'Import' && change <= 0) return 'Nhập hàng cần số lượng dương. Dùng Điều chỉnh thủ công để giảm tồn.'
  if (onHand + change < reserved) return 'Tồn vật lý sau điều chỉnh không được nhỏ hơn số lượng đang giữ chỗ.'
  return null
}
