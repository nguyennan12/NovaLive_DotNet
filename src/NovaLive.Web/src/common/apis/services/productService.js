import authorizedAxiosInstance from '../custom/authorizeAxios'
import { toast } from 'react-toastify'
import { toQueryString } from '~/common/utils/converter'

/**
 * Normalizes backend .NET product DTOs to ensure full backward compatibility
 * with existing UI components expecting spu_* or camelCase properties.
 */
export const normalizeProduct = (p) => {
  if (!p) return p
  const spuId = p.id || p._id
  const sourceSkus = p.skus || p.sku_list || []
  const skuPrices = sourceSkus
    .map((sku) => Number(sku.sellPrice ?? sku.price ?? sku.sku_price))
    .filter((price) => Number.isFinite(price) && price > 0)
  const totalStock = p.totalStock ?? p.stock ?? p.spu_quantity ?? sourceSkus.reduce(
    (total, sku) => total + Number(sku.availableQty ?? ((sku.qtyOnHand ?? sku.initialStock ?? sku.stock ?? sku.sku_stock ?? 0) - (sku.reservedQty ?? 0))),
    0
  )
  return {
    ...p,
    _id: spuId,
    id: spuId,
    spu_code: spuId,
    spu_name: p.name || p.spu_name,
    name: p.name || p.spu_name,
    spu_description: p.description || p.spu_description,
    description: p.description || p.spu_description,
    spu_thumb: p.thumbnailUrl || p.spu_thumb || '',
    thumbnailUrl: p.thumbnailUrl || p.spu_thumb || '',
    spu_shopId: p.shopId || p.spu_shopId,
    shopId: p.shopId || p.spu_shopId,
    spu_shop_name: p.shopName || p.spu_shop_name || '',
    shopName: p.shopName || p.spu_shop_name || '',
    spu_category: p.categoryId || p.spu_category,
    categoryId: p.categoryId || p.spu_category,
    categoryName: p.categoryName || '',
    total_sold: p.soldCount ?? p.total_sold ?? 0,
    spu_price: p.minPrice ?? p.price ?? p.spu_price ?? (skuPrices.length ? Math.min(...skuPrices) : 0),
    price: p.minPrice ?? p.price ?? p.spu_price ?? (skuPrices.length ? Math.min(...skuPrices) : 0),
    spu_quantity: totalStock,
    sku_list: sourceSkus.map((s) => {
      const skuId = s.id || s._id || s.sku_id
      let attrName = ''
      try {
        if (s.attributesJson) {
          const parsed = JSON.parse(s.attributesJson)
          attrName = Object.values(parsed).join(' - ')
        }
      } catch {
        attrName = s.skuCode || ''
      }
      return {
        ...s,
        _id: skuId,
        id: skuId,
        sku_id: skuId,
        sku_spuId: s.spuId || s.sku_spuId || spuId,
        sku_code: s.skuCode || s.sku_code || skuId,
        sku_name: attrName || s.skuCode || s.name || 'Mặc định',
        sku_price: s.sellPrice ?? s.price ?? s.sku_price,
        sku_original_price: s.originalPrice ?? s.sku_original_price,
        sku_stock: s.availableQty ?? ((s.qtyOnHand ?? s.initialStock ?? s.stock ?? s.sku_stock ?? 0) - (s.reservedQty ?? 0))
      }
    })
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
  try {
    const rawParams = typeof params === 'object' && params !== null ? { ...params } : {}
    if (rawParams.limit && !rawParams.size) rawParams.size = rawParams.limit
    const query = typeof params === 'string' ? params : toQueryString(rawParams)
    const response = await authorizedAxiosInstance.get(`products?${query}`)
    const raw = response?.data !== undefined ? response.data : response
    const items = (raw?.items ?? raw?.products ?? (Array.isArray(raw) ? raw : [])).map(normalizeProduct)
    return {
      ...raw,
      products: items,
      items,
      totalCount: raw?.totalCount ?? raw?.total ?? items.length,
      page: raw?.page ?? 1,
      size: raw?.size ?? items.length
    }
  } catch (error) {
    console.error('getAllProductsAPI error:', error)
    return {
      products: [],
      items: [],
      totalCount: 0,
      page: 1,
      size: 0
    }
  }
}

export const getAllProductWithStockAPI = async (query = '') => {
  try {
    const q = typeof query === 'string' ? query : toQueryString(query)
    const response = await authorizedAxiosInstance.get(`products/search?${q}`)
    const raw = response?.data !== undefined ? response.data : response
    const items = (raw?.items ?? raw?.products ?? (Array.isArray(raw) ? raw : [])).map(normalizeProduct)
    return {
      ...raw,
      products: items,
      items,
      totalCount: raw?.totalCount ?? items.length
    }
  } catch (error) {
    console.error('getAllProductWithStockAPI error:', error)
    return { products: [], items: [], totalCount: 0 }
  }
}

export const getSellerProductsAPI = async (params = {}) => {
  const rawParams = typeof params === 'object' && params !== null ? { ...params } : {}
  if (rawParams.limit && !rawParams.size) rawParams.size = rawParams.limit
  const query = typeof params === 'string' ? params : toQueryString(rawParams)
  const response = await authorizedAxiosInstance.get(`seller/products?${query}`)
  const raw = response?.data !== undefined ? response.data : response
  const items = (raw?.items ?? raw?.products ?? (Array.isArray(raw) ? raw : [])).map(normalizeProduct)
  return {
    ...raw,
    products: items,
    items,
    totalCount: raw?.totalCount ?? items.length,
    page: raw?.page ?? 1,
    size: raw?.size ?? items.length
  }
}

export const getSellerProductDetailAPI = async (productId) => {
  const response = await authorizedAxiosInstance.get(`seller/products/${productId}`)
  const raw = response?.data !== undefined ? response.data : response
  return normalizeProduct(raw)
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
  try {
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
  } catch (error) {
    console.error('queryProductAPI error:', error)
    return { products: [], items: [], totalCount: 0 }
  }
}

export const getProductStatsAPI = async () => {
  try {
    const [productResponse, inventoryResponse] = await Promise.all([
      authorizedAxiosInstance.get('seller/products?page=1&size=100'),
      authorizedAxiosInstance.get('seller/inventory?page=1&size=100')
    ])
    const productRaw = productResponse?.data !== undefined ? productResponse.data : productResponse
    const inventoryRaw = inventoryResponse?.data !== undefined ? inventoryResponse.data : inventoryResponse
    const inventory = inventoryRaw?.items ?? (Array.isArray(inventoryRaw) ? inventoryRaw : [])
    const available = (item) => Number(item.availableQty ?? ((item.qtyOnHand ?? item.stock ?? 0) - (item.reservedQty ?? 0)))
    return {
      totalProducts: productRaw?.totalCount ?? productRaw?.items?.length ?? 0,
      lowStockCount: inventory.filter(item => available(item) > 0 && available(item) <= Number(item.minStock ?? 0)).length,
      outOfStockCount: inventory.filter(item => available(item) <= 0).length
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
