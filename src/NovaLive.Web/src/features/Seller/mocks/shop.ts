import { db, required, saveMock } from './database'
import type { Shop, ShopProfileRequest, UploadResponse, Warehouse, WarehouseRequest } from '../types/contracts'
import type { BankDetails, District, Province, Ward } from '../types/viewModels'

export function mockGetShop(): Shop | Promise<Shop> { return structuredClone(db().shop) }

export function mockUpdateShop(data: ShopProfileRequest): Shop | Promise<Shop> { Object.assign(db().shop, data); saveMock(); return structuredClone(db().shop) }

export function mockGetWarehouses(): Warehouse[] | Promise<Warehouse[]> { return structuredClone(db().warehouses) }

export function mockSaveWarehouse(id: string | null, data: WarehouseRequest): Warehouse | Promise<Warehouse> {
  const list = db().warehouses
  const item = id ? required(list.find(item => item.id === id), 'Kho') : { id: crypto.randomUUID(), ...data }
  if (data.isPrimary) list.forEach(item => { item.isPrimary = false })
  Object.assign(item, data)
  if (!id) list.push(item)
  saveMock(); return structuredClone(item)
}

export function mockDeleteWarehouse(id: string): unknown | Promise<unknown> { db().warehouses = db().warehouses.filter(item => item.id !== id); saveMock(); return {} }

export function mockUploadMedia(file: File): UploadResponse | Promise<UploadResponse> { return new Promise<UploadResponse>((resolve, reject) => {
    const reader = new FileReader()
    reader.onerror = () => reject(new Error('Không đọc được tệp.'))
    reader.onload = () => typeof reader.result === 'string' ? resolve({ url: reader.result, fileName: file.name }) : reject(new Error('Không đọc được tệp.'))
    reader.readAsDataURL(file)
  }) }
export const mockBankDetails = (shop: Shop): BankDetails => ({ bankAccount: shop.bankAccount || null, bankName: 'Vietcombank (dữ liệu mẫu)', bankHolder: 'NGUYEN QUOC DAT (mẫu)', bankBranch: 'Chi nhánh Thủ Đức (mẫu)' })
export const mockProvinces = (): Province[] => [{ ProvinceID: 202, ProvinceName: 'Hồ Chí Minh' }, { ProvinceID: 201, ProvinceName: 'Hà Nội' }]
export const mockDistricts = (provinceId: number): District[] => provinceId === 202 ? [{ DistrictID: 3695, DistrictName: 'Thủ Đức' }] : [{ DistrictID: 1485, DistrictName: 'Cầu Giấy' }]
export const mockWards = (districtId: number): Ward[] => districtId === 3695 ? [{ WardCode: '90768', WardName: 'Linh Trung' }] : [{ WardCode: '1A0601', WardName: 'Dịch Vọng' }]
