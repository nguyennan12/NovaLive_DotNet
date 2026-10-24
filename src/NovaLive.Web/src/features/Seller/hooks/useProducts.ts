import * as service from '../services/productService'
import { useSellerMutation, useSellerQuery } from './useSellerQuery'
import type { ProductFilters, UpdateProductRequest, UpdateSkuRequest } from '../types/contracts'

export const useCategories = () => useSellerQuery(['categories'], service.getCategories)
export const useProducts = (filters: ProductFilters) => useSellerQuery(['products', filters], () => service.getProducts(filters))
export const useProduct = (id?: string) => useSellerQuery(['product', id], () => service.getProduct(id!), Boolean(id))
export const useAllProducts = () => useSellerQuery(['all-products'], service.allSellerProducts)
export const useCreateProduct = () => useSellerMutation(service.createProduct, 'Đã tạo sản phẩm.')
export const useUpdateProduct = () => useSellerMutation(({ id, data }: { id: string; data: UpdateProductRequest }) => service.updateProduct(id, data), 'Đã cập nhật thông tin sản phẩm.')
export const useUpdateSku = () => useSellerMutation(({ id, data }: { id: string; data: UpdateSkuRequest }) => service.updateSku(id, data), 'Đã cập nhật SKU.')
export const useHideProduct = () => useSellerMutation(service.hideProduct, 'Đã ẩn sản phẩm.')
export const useSkuOptions = (productId: string) => useSellerQuery(['sku-options', productId], () => service.getProduct(productId), Boolean(productId))
export const useProductTable = (filters: ProductFilters, status?: string) => useSellerQuery(['product-table', filters, status], () => service.getProductTablePage(filters, status))
export const useSaveProductChanges = () => useSellerMutation(({ id, data, skus }: { id: string; data: UpdateProductRequest; skus: { id: string; data: UpdateSkuRequest }[] }) => service.saveProductChanges(id, data, skus), 'Đã lưu sản phẩm và các SKU.')
