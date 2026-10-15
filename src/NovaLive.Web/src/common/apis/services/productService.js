import authorizedAxiosInstance from '../custom/authorizeAxios'
import { toast } from 'react-toastify'
import { toQueryString } from '~/common/utils/converter'


export const addProductAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('seller/products', data)
  if (response.message) toast.success(`${response.message}`)
  return response.metadata ?? response.data ?? response
}
export const getAllProductsAPI = async ({ page = 1, limit = 10, shopId } = {}) => {
  const query = toQueryString({ page, limit, ...(shopId && { shopId }) })
  const response = await authorizedAxiosInstance.get(`products?${query}`)
  return response.metadata ?? response.data ?? response
}
export const getAllProductWithStockAPI = async (query) => {
  const response = await authorizedAxiosInstance.get(`products/search?${query}`)
  return response.metadata ?? response.data ?? response
}

export const deleteProductAPI = async (productId) => {
  const response = await authorizedAxiosInstance.delete(`seller/products/${productId}`)
  if (response.message) toast.success(`${response.message}`)
  return response.metadata ?? response.data ?? response
}

export const updateProductAPI = async (productId, data) => {
  const response = await authorizedAxiosInstance.put(`seller/products/${productId}`, data)
  if (response.message) toast.success(`${response.message}`)
  return response.metadata ?? response.data ?? response
}

export const getProductDetailAPI = async (productId) => {
  const response = await authorizedAxiosInstance.get(`products/${productId}`)
  return response.metadata ?? response.data ?? response
}

export const queryProductAPI = async (query) => {
  const response = await authorizedAxiosInstance.get(`products/search?${query}`)
  return response.metadata ?? response.data ?? response
}

export const getProductStatsAPI = async () => {
  const response = await authorizedAxiosInstance.get('seller/products')
  return response.metadata ?? response.data ?? response
}

export const getOneSkusAPI = async (spuId, skuId) => {
  const response = await authorizedAxiosInstance.get(`products/${spuId}`)
  return response.metadata ?? response.data ?? response
}