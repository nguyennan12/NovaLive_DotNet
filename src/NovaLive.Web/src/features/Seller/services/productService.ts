import * as mocks from '../mocks/product'
import { request } from './transport'

import type { Category, CreateProductRequest, PagedResult, Product, ProductDetail, ProductFilters, Sku, UpdateProductRequest, UpdateSkuRequest } from '../types/contracts'

export const getCategories = () => request<Category[]>('Danh mục', { url: 'categories' }, () => mocks.mockGetCategories())
export const getProducts = (params: ProductFilters) => request<PagedResult<Product>>('Sản phẩm', { url: 'seller/products', params }, () => mocks.mockGetProducts(params))
export const getProduct = (id: string) => request<ProductDetail>('Sản phẩm', { url: `seller/products/${id}` }, () => mocks.mockGetProduct(id))
export const createProduct = (data: CreateProductRequest) => request<ProductDetail>('Sản phẩm', { method: 'POST', url: 'seller/products', data }, () => mocks.mockCreateProduct(data))
export const updateProduct = (id: string, data: UpdateProductRequest) => request<ProductDetail>('Sản phẩm', { method: 'PUT', url: `seller/products/${id}`, data }, () => mocks.mockUpdateProduct(id, data))
export const updateSku = (id: string, data: UpdateSkuRequest) => request<Sku>('Sản phẩm', { method: 'PUT', url: `seller/skus/${id}`, data }, () => mocks.mockUpdateSku(id, data))
export const hideProduct = (id: string) => request<unknown>('Sản phẩm', { method: 'DELETE', url: `seller/products/${id}` }, () => mocks.mockHideProduct(id))
export async function allSellerProducts(): Promise<Product[]> {
  const result: Product[] = []
  let page = 1
  while (true) {
    const data = await getProducts({ page, size: 100 })
    result.push(...data.items)
    if (page >= data.totalPages || !data.items.length) return result
    page++
  }
}
export async function getProductTablePage(filters: ProductFilters, status?: string): Promise<PagedResult<ProductDetail>> {
  let page: PagedResult<Product>
  if (status) {
    const rows = (await allSellerProducts()).filter(item => item.status === status && (!filters.categoryId || item.categoryId === filters.categoryId) && (!filters.keyword || item.name.toLowerCase().includes(filters.keyword.toLowerCase())))
    page = { items: rows.slice((filters.page - 1) * filters.size, filters.page * filters.size), page: filters.page, size: filters.size, total: rows.length, totalPages: Math.ceil(rows.length / filters.size) }
  } else page = await getProducts(filters)
  return { ...page, items: await Promise.all(page.items.map(item => getProduct(item.id))) }
}
export async function saveProductChanges(id: string, data: UpdateProductRequest, skus: { id: string; data: UpdateSkuRequest }[]): Promise<void> {
  await updateProduct(id, data)
  let updated = 0
  try { for (const sku of skus) { await updateSku(sku.id, sku.data); updated++ } }
  catch (error) { throw new Error(`Đã lưu thông tin chung và ${updated}/${skus.length} SKU. Hãy tải lại để kiểm tra trước khi tiếp tục. ${error instanceof Error ? error.message : ''}`) }
}
