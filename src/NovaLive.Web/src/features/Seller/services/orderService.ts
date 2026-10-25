import * as mocks from '../mocks/order'
import { request } from './transport'

import type { CreateShippingRequest, OrderDetail, OrderFilters, OrderStatus, OrderSummary, PagedResult, ShippingOrder, Tracking } from '../types/contracts'
import { ORDER_STATUSES } from '../constants'
import { normalizeSellerTracking } from '../utils/format'

export const getOrders = (params: OrderFilters) => request<PagedResult<OrderSummary>>('Đơn hàng', { url: 'seller/orders', params }, () => mocks.mockGetOrders(params))
export const getOrder = (id: string) => request<OrderDetail>('Đơn hàng', { url: `seller/orders/${id}` }, () => mocks.mockGetOrder(id))
export async function getOrderPage(params: OrderFilters): Promise<PagedResult<OrderDetail>> {
  const page = await getOrders(params)
  return { ...page, items: await Promise.all(page.items.map(item => getOrder(item.id))) }
}
export const getOrderCounts = async (range: Pick<OrderFilters, 'from' | 'to'>) => Object.fromEntries(await Promise.all(ORDER_STATUSES.map(async status => [status, (await getOrders({ ...range, page: 1, size: 1, status })).total] as const))) as Record<OrderStatus, number>

export const confirmOrder = (id: string, warehouseAddressId: string) => request<unknown>('Đơn hàng', { method: 'PUT', url: `seller/orders/${id}/confirm`, data: { warehouseAddressId } }, () => mocks.mockConfirmOrder(id, warehouseAddressId))
export const cancelOrder = (id: string, reason: string) => request<unknown>('Đơn hàng', { method: 'POST', url: `seller/orders/${id}/cancel`, data: { reason } }, () => mocks.mockCancelOrder(id, reason))
export const createShipment = (data: CreateShippingRequest) => request<ShippingOrder>('Vận chuyển', { method: 'POST', url: 'seller/shipping/create-order', data }, () => mocks.mockCreateShipment(data))
export async function getTracking(id: string): Promise<Tracking> {
  const result = await request<Tracking | Tracking[]>('Vận chuyển', { url: `seller/shipping/${id}/tracking` }, () => mocks.mockGetTracking(id))
  return normalizeSellerTracking(result)
}
// Minimal A6 PDF for mock mode only; real labels are the backend's unmodified PDF blob.

export const getShippingLabel = (id: string) => request<Blob>('Vận chuyển', { url: `seller/shipping/${id}/label`, responseType: 'blob' }, () => mocks.mockGetShippingLabel(id))
export interface CustomerSummary { id: string; name: string; phone: string; orderCount: number; totalSpent: number; lastOrderAt: string; lastOrderId: string }
export async function getCustomers(): Promise<CustomerSummary[]> {
  const customers = new Map<string, CustomerSummary>()
  let page = 1
  while (true) {
    const summaries = await getOrders({ page, size: 100 })
    // Bound detail fan-out to five requests; this is derived from shop-owned orders only.
    for (let offset = 0; offset < summaries.items.length; offset += 5) {
      const orders = await Promise.all(summaries.items.slice(offset, offset + 5).map(item => getOrder(item.id)))
      for (const order of orders) {
        const key = `${order.recipient.phone}:${order.recipient.recipientName}`
        const row = customers.get(key) || { id: key, name: order.recipient.recipientName, phone: order.recipient.phone, orderCount: 0, totalSpent: 0, lastOrderAt: order.createdAt, lastOrderId: order.id }
        row.orderCount++
        if (order.status !== 'Cancelled' && order.status !== 'PendingPayment') row.totalSpent += order.subTotal
        if (order.createdAt > row.lastOrderAt) { row.lastOrderAt = order.createdAt; row.lastOrderId = order.id }
        customers.set(key, row)
      }
    }
    if (!summaries.items.length || page >= summaries.totalPages) break
    page++
  }
  return [...customers.values()].sort((a, b) => b.lastOrderAt.localeCompare(a.lastOrderAt))
}
