import { toQueryString } from '~/common/utils/converter'
import authorizedAxiosInstance from '../custom/authorizeAxios'

export const getAllDiscountAPI = async (params = {}) => {
  const query = toQueryString(params)
  const response = await authorizedAxiosInstance.get(`discounts/platform${query ? `?${query}` : ''}`)
  return response.metadata ?? response.data ?? response
}
export const getAllDiscountOfShopAPI = async (shopId, params) => {
  const query = toQueryString(params)
  const response = await authorizedAxiosInstance.get(`discounts/shop/${shopId}${query ? `?${query}` : ''}`)
  return response.metadata ?? response.data ?? response
}

export const queryDiscountAPI = async (params = {}) => {
  const query = toQueryString(params)
  const response = await authorizedAxiosInstance.get(`discounts/platform${query ? `?${query}` : ''}`)
  return response.metadata ?? response.data ?? response
}

export const createDiscountAPI = async (payload) => {
  const response = await authorizedAxiosInstance.post('seller/discounts', payload)
  return response.metadata ?? response.data ?? response
}

export const updateDiscountAPI = async (discountId, payload) => {
  const response = await authorizedAxiosInstance.put(`seller/discounts/${discountId}`, payload)
  return response.metadata ?? response.data ?? response
}

export const deleteDiscountAPI = async (discountId) => {
  const response = await authorizedAxiosInstance.delete(`seller/discounts/${discountId}`)
  return response.metadata ?? response.data ?? response
}
export const checkAvailableAPI = async (discountCode, totalOrder) => {
  const response = await authorizedAxiosInstance.post('discounts/validate', { code: discountCode, orderTotal: totalOrder })
  return response.metadata ?? response.data ?? response
}