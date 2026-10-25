import * as service from '../services/inventoryService'
import { useSellerMutation, useSellerQuery } from './useSellerQuery'

export const useInventory = (params: service.InventoryFilters) => useSellerQuery(['inventory', params], () => service.getInventory(params))
export const useInventoryHistory = (params: service.HistoryFilters, enabled = true) => useSellerQuery(['inventory-history', params], () => service.getInventoryHistory(params), enabled)
export const useAdjustInventory = () => useSellerMutation(service.adjustInventory, 'Đã điều chỉnh tồn kho và ghi sổ biến động.')
