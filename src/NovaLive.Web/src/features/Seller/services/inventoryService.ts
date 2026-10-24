import * as mocks from '../mocks/inventory'
import { request } from './transport'

import type { AdjustInventoryRequest, DateRange, InventoryHistory, InventoryItem, PagedResult, PageRequest } from '../types/contracts'

export interface InventoryFilters extends PageRequest { keyword?: string; lowStock?: boolean }
export interface HistoryFilters extends PageRequest, DateRange { skuId?: string }
export const getInventory = (params: InventoryFilters) => request<PagedResult<InventoryItem>>('Tồn kho', { url: 'seller/inventory', params }, () => mocks.mockGetInventory(params))
export const getInventoryHistory = (params: HistoryFilters) => request<PagedResult<InventoryHistory>>('Sổ kho', { url: 'seller/inventory/histories', params }, () => mocks.mockGetInventoryHistory(params))
export const adjustInventory = (data: AdjustInventoryRequest) => request<InventoryItem>('Tồn kho', { method: 'POST', url: 'seller/inventory/adjust', data }, () => mocks.mockAdjustInventory(data))
