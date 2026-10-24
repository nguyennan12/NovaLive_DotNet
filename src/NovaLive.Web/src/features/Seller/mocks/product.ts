import { db, paginate, required, saveMock } from './database'
import type { Category, CreateProductRequest, PagedResult, Product, ProductDetail, ProductFilters, Sku, UpdateProductRequest, UpdateSkuRequest } from '../types/contracts'

export function mockGetCategories(): Category[] | Promise<Category[]> { return structuredClone(db().categories) }

export function mockGetProducts(params: ProductFilters): PagedResult<Product> | Promise<PagedResult<Product>> { return paginate(db().products.filter(product => (!params.keyword || product.name.toLowerCase().includes(params.keyword.toLowerCase())) && (!params.categoryId || product.categoryId === params.categoryId)), params) }

export function mockGetProduct(id: string): ProductDetail | Promise<ProductDetail> { return structuredClone(required(db().products.find(item => item.id === id), 'Sản phẩm')) }

export function mockCreateProduct(data: CreateProductRequest): ProductDetail | Promise<ProductDetail> {
  const codes = data.skus.map(item => item.skuCode.toLowerCase())
  if (new Set(codes).size !== codes.length || db().products.some(item => item.skus.some(sku => codes.includes(sku.skuCode.toLowerCase())))) throw new Error('Mã SKU đã tồn tại trong shop. Vui lòng chọn mã khác.')
  const id = crypto.randomUUID()
  const prices = data.skus.map(item => item.sellPrice)
  const product: ProductDetail = { ...data, id, shopId: db().shop.id, shopName: db().shop.shopName, categoryName: db().categories.flatMap(item => [item, ...item.children]).find(item => item.id === data.categoryId)?.name || '', slug: id, minPrice: Math.min(...prices), maxPrice: Math.max(...prices), rating: 0, soldCount: 0, status: 'Active', createdAt: new Date().toISOString(), skus: data.skus.map(sku => ({ ...sku, id: crypto.randomUUID(), spuId: id, qtyOnHand: sku.initialStock, reservedQty: 0, availableQty: sku.initialStock, isActive: true })) }
  db().products.unshift(product)
  product.skus.forEach(sku => db().inventory.push({ skuId: sku.id, skuCode: sku.skuCode, spuName: product.name, qtyOnHand: sku.qtyOnHand, reservedQty: 0, availableQty: sku.qtyOnHand, minStock: 5, lastUpdated: product.createdAt }))
  saveMock(); return structuredClone(product)
}

export function mockUpdateProduct(id: string, data: UpdateProductRequest): ProductDetail | Promise<ProductDetail> { const item = required(db().products.find(item => item.id === id), 'Sản phẩm'); Object.assign(item, data); saveMock(); return structuredClone(item) }

export function mockUpdateSku(id: string, data: UpdateSkuRequest): Sku | Promise<Sku> {
  const product = required(db().products.find(item => item.skus.some(sku => sku.id === id)), 'Sản phẩm')
  const item = required(product.skus.find(item => item.id === id), 'SKU')
  Object.assign(item, data)
  product.minPrice = Math.min(...product.skus.map(sku => sku.sellPrice))
  product.maxPrice = Math.max(...product.skus.map(sku => sku.sellPrice))
  saveMock(); return structuredClone(item)
}

export function mockHideProduct(id: string): unknown | Promise<unknown> { const item = required(db().products.find(item => item.id === id), 'Sản phẩm'); item.status = 'Inactive'; item.skus.forEach(sku => { sku.isActive = false }); saveMock(); return {} }
