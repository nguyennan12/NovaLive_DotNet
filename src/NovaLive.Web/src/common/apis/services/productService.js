import authorizedAxiosInstance from '../custom/authorizeAxios'
import { toast } from 'react-toastify'
import { toQueryString } from '~/common/utils/converter'

/**
 * Normalizes backend .NET product DTOs to ensure full backward compatibility
 * with existing UI components expecting spu_* or camelCase properties.
 */
export const normalizeProduct = (p) => {
  if (!p) return p
  return {
    ...p,
    _id: p.id || p._id,
    id: p.id || p._id,
    spu_code: p.id || p.spu_code || p.slug,
    spu_name: p.name || p.spu_name,
    name: p.name || p.spu_name,
    spu_description: p.description || p.spu_description,
    description: p.description || p.spu_description,
    spu_thumb: p.thumbnailUrl || p.spu_thumb || '',
    thumbnailUrl: p.thumbnailUrl || p.spu_thumb || '',
    spu_price: p.minPrice ?? p.price ?? p.spu_price ?? 0,
    price: p.minPrice ?? p.price ?? p.spu_price ?? 0,
    spu_quantity: p.totalStock ?? p.stock ?? p.spu_quantity ?? 100,
    sku_list: (p.skus || p.sku_list || []).map((s) => ({
      ...s,
      _id: s.id || s._id,
      id: s.id || s._id,
      sku_code: s.skuCode || s.sku_code || s.id,
      sku_price: s.sellPrice ?? s.price ?? s.sku_price,
      sku_original_price: s.originalPrice ?? s.sku_original_price,
      sku_stock: s.initialStock ?? s.stock ?? s.sku_stock
    }))
  }
}

export const addProductAPI = async (data) => {
  const payload = data.Name !== undefined || data.name !== undefined ? data : {
    name: data.spu_name || data.name,
    description: data.spu_description || data.description || '',
    categoryId: data.spu_category || data.categoryId,
    brand: data.brand || null,
    thumbnailUrl: data.spu_thumb || data.thumbnailUrl || '',
    attributesConfigJson: data.spu_attributes ? JSON.stringify(data.spu_attributes) : null,
    skus: (data.sku_list || []).map(sku => ({
      skuCode: sku.sku_code || `SKU-${Date.now()}-${Math.random().toString(36).slice(2, 6)}`,
      attributesJson: sku.attributes ? JSON.stringify(sku.attributes) : '{}',
      originalPrice: Number(sku.sku_original_price || sku.sku_price || data.spu_price || 0),
      sellPrice: Number(sku.sku_price || data.spu_price || 0),
      weightGram: Number(sku.weight || 200),
      initialStock: Number(sku.sku_stock || data.spu_quantity || 10),
      images: sku.images || []
    })),
    attributes: []
  }

  const response = await authorizedAxiosInstance.post('seller/products', payload)
  if (response?.message) toast.success(response.message)
  return normalizeProduct(response?.data !== undefined ? response.data : response)
}

export const getAllProductsAPI = async (params = {}) => {
  const query = typeof params === 'string' ? params : toQueryString(params)
  const response = await authorizedAxiosInstance.get(`products?${query}`)
  const raw = response?.data !== undefined ? response.data : response
  const items = (raw?.items ?? raw?.products ?? (Array.isArray(raw) ? raw : [])).map(normalizeProduct)
  return {
    products: items,
    items,
    totalCount: raw?.totalCount ?? raw?.total ?? items.length,
    page: raw?.page ?? 1,
    size: raw?.size ?? items.length,
    ...raw
  }
}

export const getAllProductWithStockAPI = async (query = '') => {
  const q = typeof query === 'string' ? query : toQueryString(query)
  const response = await authorizedAxiosInstance.get(`products/search?${q}`)
  const raw = response?.data !== undefined ? response.data : response
  const items = (raw?.items ?? raw?.products ?? (Array.isArray(raw) ? raw : [])).map(normalizeProduct)
  return {
    products: items,
    items,
    totalCount: raw?.totalCount ?? items.length,
    ...raw
  }
}

export const deleteProductAPI = async (productId) => {
  const response = await authorizedAxiosInstance.delete(`seller/products/${productId}`)
  if (response?.message) toast.success(response.message)
  return response?.data !== undefined ? response.data : response
}

export const updateProductAPI = async (productId, data) => {
  const payload = data.Name !== undefined || data.name !== undefined ? data : {
    name: data.spu_name || data.name,
    description: data.spu_description || data.description || '',
    categoryId: data.spu_category || data.categoryId,
    brand: data.brand || null,
    thumbnailUrl: data.spu_thumb || data.thumbnailUrl || '',
    attributesConfigJson: data.spu_attributes ? JSON.stringify(data.spu_attributes) : null,
    attributes: []
  }

  const response = await authorizedAxiosInstance.put(`seller/products/${productId}`, payload)
  if (response?.message) toast.success(response.message)
  return normalizeProduct(response?.data !== undefined ? response.data : response)
}

export const getProductDetailAPI = async (productId) => {
  const response = await authorizedAxiosInstance.get(`products/${productId}`)
  const raw = response?.data !== undefined ? response.data : response
  return normalizeProduct(raw)
}

export const queryProductAPI = async (query = '') => {
  const q = typeof query === 'string' ? query : toQueryString(query)
  const response = await authorizedAxiosInstance.get(`products/search?${q}`)
  const raw = response?.data !== undefined ? response.data : response
  const items = (raw?.items ?? raw?.products ?? (Array.isArray(raw) ? raw : [])).map(normalizeProduct)
  return {
    products: items,
    items,
    totalCount: raw?.totalCount ?? items.length,
    ...raw
  }
}

export const getProductStatsAPI = async () => {
  try {
    const response = await authorizedAxiosInstance.get('seller/products?page=1&size=100')
    const raw = response?.data !== undefined ? response.data : response
    const items = raw?.items ?? raw?.products ?? (Array.isArray(raw) ? raw : [])
    return {
      totalProducts: raw?.totalCount ?? items.length,
      lowStockCount: items.filter(p => (p.totalStock ?? p.spu_quantity ?? 0) < 10 && (p.totalStock ?? p.spu_quantity ?? 0) > 0).length,
      outOfStockCount: items.filter(p => (p.totalStock ?? p.spu_quantity ?? 0) === 0).length,
      products: items
    }
  } catch (_e) {
    return { totalProducts: 0, lowStockCount: 0, outOfStockCount: 0, products: [] }
  }
}

export const getOneSkusAPI = async (spuId, skuId) => {
  const response = await authorizedAxiosInstance.get(`products/${spuId}`)
  const raw = response?.data !== undefined ? response.data : response
  const normalized = normalizeProduct(raw)
  if (skuId && normalized?.sku_list) {
    const sku = normalized.sku_list.find(s => s.id === skuId || s._id === skuId)
    if (sku) return sku
  }
  return normalized
}

export const updateSkuPriceAPI = async (skuId, data) => {
  const response = await authorizedAxiosInstance.put(`seller/skus/${skuId}`, data)
  return response?.data !== undefined ? response.data : response
}