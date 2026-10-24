import * as service from '../services/shopService'
import { useSellerMutation, useSellerQuery } from './useSellerQuery'
import type { WarehouseRequest } from '../types/contracts'

export const useSellerShop = () => useSellerQuery(['shop'], service.getShop)
export const useWarehouses = () => useSellerQuery(['warehouses'], service.getWarehouses)
export const useBankDetails = () => useSellerQuery(['bank'], service.getBankDetails)
export const useUpdateShop = () => useSellerMutation(service.updateShop, 'Đã lưu hồ sơ cửa hàng.')
export const useSaveWarehouse = (onSuccess?: () => void) => useSellerMutation(({ id, data }: { id: string | null; data: WarehouseRequest }) => service.saveWarehouse(id, data), 'Đã lưu kho hàng.', onSuccess)
export const useDeleteWarehouse = () => useSellerMutation(service.deleteWarehouse, 'Đã xóa kho hàng.')
export const useUploadMedia = () => useSellerMutation(({ file, bucket }: { file: File; bucket?: string }) => service.uploadMedia(file, bucket), 'Đã tải tệp lên.')
export const useProvinces = () => useSellerQuery(['address', 'provinces'], service.getProvinces)
export const useDistricts = (id: number | null) => useSellerQuery(['address', 'districts', id], () => service.getDistricts(id!), id !== null)
export const useWards = (id: number | null) => useSellerQuery(['address', 'wards', id], () => service.getWards(id!), id !== null)
