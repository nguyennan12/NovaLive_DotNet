import { useState } from 'react'
import { toast } from 'react-toastify'
import * as service from '../services/orderService'
import { useSellerMutation, useSellerQuery } from './useSellerQuery'
import type { DateRange, OrderFilters } from '../types/contracts'
import { errorMessage } from '../utils/errors'

export const useOrders = (params: OrderFilters) => useSellerQuery(['orders', params], () => service.getOrderPage(params))
export const useOrderCounts = (range: DateRange) => useSellerQuery(['order-counts', range], () => service.getOrderCounts(range))
export const useOrder = (id?: string) => useSellerQuery(['order', id], () => service.getOrder(id!), Boolean(id))
export const useCustomers = () => useSellerQuery(['customers'], service.getCustomers)
export const useConfirmOrder = () => useSellerMutation(({ id, warehouseAddressId }: { id: string; warehouseAddressId: string }) => service.confirmOrder(id, warehouseAddressId), 'Đã xác nhận kho lấy hàng.')
export const useCancelOrder = () => useSellerMutation(({ id, reason }: { id: string; reason: string }) => service.cancelOrder(id, reason), 'Đã hủy đơn hàng.')
export const useCreateShipment = () => useSellerMutation(service.createShipment, 'Đã tạo vận đơn.')
export const useTracking = (id: string, enabled: boolean) => useSellerQuery(['tracking', id], () => service.getTracking(id), enabled)
export function usePrintLabel() {
  const [isPending, setIsPending] = useState(false)
  async function print(id: string) {
    if (isPending) return
    const tab = window.open('about:blank', '_blank')
    if (!tab) { toast.error('Vui lòng cho phép mở tab mới để in phiếu giao hàng.'); return }
    tab.opener = null
    setIsPending(true)
    try {
      const blob = await service.getShippingLabel(id)
      if (blob.type && !blob.type.includes('pdf')) throw new Error('Máy chủ chưa trả về phiếu PDF hợp lệ.')
      const url = URL.createObjectURL(blob)
      tab.location.replace(url)
      setTimeout(() => URL.revokeObjectURL(url), 120_000)
    } catch (error) { tab.close(); toast.error(errorMessage(error)) }
    finally { setIsPending(false) }
  }
  return { print, isPending }
}
