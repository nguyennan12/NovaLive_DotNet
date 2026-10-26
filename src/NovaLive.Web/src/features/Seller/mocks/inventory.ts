import { db, paginate, required, saveMock } from './database'
import type { AdjustInventoryRequest, InventoryHistory, InventoryItem, PagedResult } from '../types/contracts'
import type { InventoryFilters, HistoryFilters } from '../services/inventoryService'
import { validateStockAdjustment } from '../utils/business'

export function mockGetInventory(params: InventoryFilters): PagedResult<InventoryItem> | Promise<PagedResult<InventoryItem>> { return paginate(db().inventory.filter(item => (!params.keyword || `${item.spuName} ${item.skuCode}`.toLowerCase().includes(params.keyword.toLowerCase())) && (!params.lowStock || item.qtyOnHand - item.reservedQty <= item.minStock)), params) }

export function mockGetInventoryHistory(params: HistoryFilters): PagedResult<InventoryHistory> | Promise<PagedResult<InventoryHistory>> { return paginate(db().histories.filter(item => (!params.skuId || item.skuId === params.skuId) && (!params.from || item.createdAt >= params.from) && (!params.to || item.createdAt <= params.to)).sort((a, b) => b.createdAt.localeCompare(a.createdAt)), params) }

export function mockAdjustInventory(data: AdjustInventoryRequest): InventoryItem | Promise<InventoryItem> {
  const item = required(db().inventory.find(item => item.skuId === data.skuId), 'SKU')
  if (db().histories.some(history => history.id === data.operationId)) return structuredClone(item)
  const invalid = validateStockAdjustment(item.qtyOnHand, item.reservedQty, data.qtyChange, data.changeType)
  if (invalid) throw new Error(invalid)
  const before = item.qtyOnHand
  item.qtyOnHand += data.qtyChange; item.availableQty = item.qtyOnHand - item.reservedQty; item.lastUpdated = new Date().toISOString()
  const sku = db().products.flatMap(product => product.skus).find(sku => sku.id === item.skuId)
  if (sku) { sku.qtyOnHand = item.qtyOnHand; sku.availableQty = item.availableQty }
  db().histories.unshift({ id: data.operationId, skuId: item.skuId, skuCode: item.skuCode, spuName: item.spuName, changeType: data.changeType, qtyBefore: before, qtyChange: data.qtyChange, reservedBefore: item.reservedQty, reservedChange: 0, qtyAfter: item.qtyOnHand, refType: null, refId: null, note: data.note, createdBy: db().shop.ownerId, createdAt: item.lastUpdated })
  saveMock(); return structuredClone(item)
}
