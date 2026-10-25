import * as mocks from '../mocks/shop'
import { request } from './transport'

import type { Shop, ShopProfileRequest, UploadResponse, Warehouse, WarehouseRequest } from '../types/contracts'
import type { BankDetails, District, Province, Ward } from '../types/viewModels'
import { getDistrictsAPI, getProvincesAPI, getWardsAPI } from '~/common/apis/services/shippingService'
import { MOCK_ONLY } from '../constants'

export const getShop = () => request<Shop>('Hồ sơ', { url: 'seller/shop/me' }, () => mocks.mockGetShop())
export const updateShop = (data: ShopProfileRequest) => request<Shop>('Hồ sơ', { method: 'PUT', url: 'seller/shop/me', data }, () => mocks.mockUpdateShop(data))
export const getWarehouses = () => request<Warehouse[]>('Kho lấy hàng', { url: 'seller/shop/addresses' }, () => mocks.mockGetWarehouses())
export const saveWarehouse = (id: string | null, data: WarehouseRequest) => request<Warehouse>('Kho lấy hàng', { method: id ? 'PUT' : 'POST', url: `seller/shop/addresses${id ? `/${id}` : ''}`, data }, () => mocks.mockSaveWarehouse(id, data))
export const deleteWarehouse = (id: string) => request<unknown>('Kho lấy hàng', { method: 'DELETE', url: `seller/shop/addresses/${id}` }, () => mocks.mockDeleteWarehouse(id))
export const uploadMedia = (file: File, bucket = 'products') => {
  const data = new FormData(); data.append('file', file); data.append('bucket', bucket)
  return request<UploadResponse>('Tải tệp', { method: 'POST', url: 'upload/media', data }, () => mocks.mockUploadMedia(file))
}
// Existing backend exposes only BankAccount. No invented KYC lookup endpoint.
export async function getBankDetails(): Promise<BankDetails> {
  const shop = await getShop()
  return MOCK_ONLY ? mocks.mockBankDetails(shop) : { bankAccount: shop.bankAccount || null, bankName: null, bankHolder: null, bankBranch: null }
}
function requireAddressProvider() {
  if (!import.meta.env.VITE_GHN_API_URL || !import.meta.env.VITE_GHN_API_TOKEN) {
    throw new Error('Chưa cấu hình nguồn địa chỉ vận chuyển. Không thể tải tỉnh/thành, quận/huyện, phường/xã. Vui lòng liên hệ bộ phận kỹ thuật.')
  }
}
export async function getProvinces(): Promise<Province[]> { if (MOCK_ONLY) return mocks.mockProvinces(); requireAddressProvider(); return getProvincesAPI() }
export async function getDistricts(provinceId: number): Promise<District[]> { if (MOCK_ONLY) return mocks.mockDistricts(provinceId); requireAddressProvider(); return getDistrictsAPI(provinceId) }
export async function getWards(districtId: number): Promise<Ward[]> { if (MOCK_ONLY) return mocks.mockWards(districtId); requireAddressProvider(); return getWardsAPI(districtId) }
