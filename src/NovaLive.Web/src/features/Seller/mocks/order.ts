import { db, paginate, required, saveMock } from './database'
import type { CreateShippingRequest, OrderDetail, OrderFilters, OrderStatus, OrderSummary, PagedResult, ShippingOrder, Tracking } from '../types/contracts'

function mockTransition(id: string, status: OrderStatus, note: string) {
  const item = required(db().orders.find(item => item.id === id), 'Đơn hàng')
  item.timeline.push({ fromStatus: item.status, toStatus: status, changedByRole: 'Seller', note, changedAt: new Date().toISOString() })
  item.status = status; saveMock(); return { id, status }
}

function mockLabel(id: string): Blob {
  const code = required(db().orders.find(order => order.id === id), 'Đơn hàng').code
  const stream = `BT /F1 18 Tf 18 370 Td (NOVALIVE - PHIEU MAU) Tj 0 -35 Td (${code}) Tj 0 -35 Td (KHONG DUNG DE GIAO HANG) Tj ET`
  const objects = ['<< /Type /Catalog /Pages 2 0 R >>', '<< /Type /Pages /Kids [3 0 R] /Count 1 >>', '<< /Type /Page /Parent 2 0 R /MediaBox [0 0 298 420] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>', '<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>', `<< /Length ${stream.length} >>\nstream\n${stream}\nendstream`]
  let pdf = '%PDF-1.4\n'
  const offsets = [0]
  objects.forEach((object, i) => { offsets.push(pdf.length); pdf += `${i + 1} 0 obj\n${object}\nendobj\n` })
  const xref = pdf.length
  pdf += `xref\n0 6\n0000000000 65535 f \n${offsets.slice(1).map(offset => `${String(offset).padStart(10, '0')} 00000 n \n`).join('')}trailer\n<< /Size 6 /Root 1 0 R >>\nstartxref\n${xref}\n%%EOF`
  return new Blob([pdf], { type: 'application/pdf' })
}

export function mockGetOrders(params: OrderFilters): PagedResult<OrderSummary> | Promise<PagedResult<OrderSummary>> { return paginate(db().orders.filter(item => (!params.status || item.status === params.status) && (!params.from || item.createdAt >= params.from) && (!params.to || item.createdAt <= params.to)), params) }

export function mockGetOrder(id: string): OrderDetail | Promise<OrderDetail> { return structuredClone(required(db().orders.find(item => item.id === id), 'Đơn hàng')) }

export function mockConfirmOrder(id: string, warehouseAddressId: string): unknown | Promise<unknown> {
  const order = required(db().orders.find(item => item.id === id), 'Đơn hàng')
  if (order.status !== 'Confirmed') throw new Error('Chỉ chọn kho cho đơn đã xác nhận thanh toán hoặc COD.')
  required(db().warehouses.find(item => item.id === warehouseAddressId), 'Kho lấy hàng')
  order.warehouseAddressId = warehouseAddressId
  return mockTransition(id, 'Confirmed', 'Đã chọn kho lấy hàng')
}

export function mockCancelOrder(id: string, reason: string): unknown | Promise<unknown> {
  const order = required(db().orders.find(item => item.id === id), 'Đơn hàng')
  if (!['PendingPayment', 'Confirmed', 'Processing'].includes(order.status)) throw new Error('Không thể hủy đơn ở trạng thái hiện tại.')
  order.cancelReason = reason
  return mockTransition(id, 'Cancelled', reason)
}

export function mockCreateShipment(data: CreateShippingRequest): ShippingOrder | Promise<ShippingOrder> {
  const order = required(db().orders.find(item => item.id === data.subOrderId), 'Đơn hàng')
  if (order.status !== 'Confirmed' || !order.warehouseAddressId) throw new Error('Vui lòng chọn kho cho đơn đã xác nhận trước khi giao hàng.')
  mockTransition(order.id, 'Shipping', `Bàn giao cho ${data.provider}`)
  return { id: crypto.randomUUID(), subOrderId: order.id, provider: data.provider, trackingCode: `MOCK-${order.code}`, shippingFee: order.shippingFee, status: 'ReadyToPick', createdAt: new Date().toISOString() }
}

export function mockGetTracking(id: string): Tracking | Promise<Tracking> {
  const order = required(db().orders.find(item => item.id === id), 'Đơn hàng')
  return { subOrderId: id, provider: 'GHN', trackingCode: `MOCK-${order.code}`, status: order.status, isStale: false, events: order.timeline.map(item => ({ status: item.toStatus, description: item.note || '', occurredAt: item.changedAt })) }
}

export function mockGetShippingLabel(id: string): Blob | Promise<Blob> { return mockLabel(id) }
