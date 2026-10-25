import { db, paginate, required, saveMock } from './database'
import type { Campaign, CreateDiscountRequest, Discount, FlashRegistration, FlashRegistrationRequest, PagedResult, PageRequest, UpdateDiscountRequest } from '../types/contracts'
import { validateFlashRegistration } from '../utils/marketing'

export function mockGetDiscounts(params: PageRequest & { status?: string }): PagedResult<Discount> | Promise<PagedResult<Discount>> {
  const now = Date.now()
  return paginate(db().discounts.filter(item => {
    if (params.status === 'Active') return item.isActive && Date.parse(item.validFrom) <= now && Date.parse(item.validTo) >= now
    if (params.status === 'Inactive') return !item.isActive
    if (params.status === 'Scheduled') return Date.parse(item.validFrom) > now
    if (params.status === 'Expired') return Date.parse(item.validTo) < now
    return true
  }), params)
}

export function mockCreateDiscount(data: CreateDiscountRequest): Discount | Promise<Discount> {
  if (db().discounts.some(item => item.code.toLowerCase() === data.code.toLowerCase())) throw new Error('Mã voucher đã tồn tại.')
  const item: Discount = { ...data, id: crypto.randomUUID(), shopId: db().shop.id, usedCount: 0, isActive: true, createdAt: new Date().toISOString() }
  db().discounts.unshift(item); saveMock(); return structuredClone(item)
}

export function mockUpdateDiscount(id: string, data: UpdateDiscountRequest): Discount | Promise<Discount> {
  const item = required(db().discounts.find(item => item.id === id), 'Voucher'); Object.assign(item, data); saveMock(); return structuredClone(item)
}

export function mockGetCampaigns(): Campaign[] | Promise<Campaign[]> { return structuredClone(db().campaigns) }

export function mockGetRegistrations(params: PageRequest & { status?: string }): PagedResult<FlashRegistration> | Promise<PagedResult<FlashRegistration>> { return paginate(db().registrations.filter(item => !params.status || item.status === params.status), params) }

export function mockRegisterFlashSale(campaignId: string, data: FlashRegistrationRequest): FlashRegistration | Promise<FlashRegistration> {
  const product = required(db().products.find(item => item.skus.some(sku => sku.id === data.skuId)), 'Sản phẩm')
  const sku = required(product.skus.find(item => item.id === data.skuId), 'SKU')
  const invalid = validateFlashRegistration(data.flashPrice, sku.sellPrice, data.quantity, sku.qtyOnHand - sku.reservedQty, data.perUserLimit)
  if (invalid) throw new Error(invalid)
  if (db().registrations.some(item => item.campaignId === campaignId && item.skuId === sku.id && item.status !== 'Rejected')) throw new Error('SKU đã được đăng ký trong chiến dịch này.')
  const item: FlashRegistration = { ...data, id: crypto.randomUUID(), campaignId, spuId: product.id, spuName: product.name, skuCode: sku.skuCode, thumbnailUrl: product.thumbnailUrl, originalPrice: sku.sellPrice, soldQty: 0, reservedQty: 0, status: 'Pending', rejectionReason: null }
  db().registrations.unshift(item); saveMock(); return structuredClone(item)
}
