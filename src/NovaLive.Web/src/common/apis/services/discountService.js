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
  const endpoint = params.scope === 'seller'
    ? 'seller/discounts'
    : params.shopId
      ? `discounts/shop/${params.shopId}`
      : 'discounts/platform'
  const sellerParams = params.scope === 'seller'
    ? { ...params, size: params.size ?? params.limit }
    : params
  const sellerQuery = params.scope === 'seller' ? toQueryString(sellerParams) : query
  const response = await authorizedAxiosInstance.get(`${endpoint}${sellerQuery ? `?${sellerQuery}` : ''}`)
  return response.metadata ?? response.data ?? response
}

export const createDiscountAPI = async (payload) => {
  const response = await authorizedAxiosInstance.post('seller/discounts', payload)
  return response.metadata ?? response.data ?? response
}

export const updateDiscountAPI = async (discountId, payload) => {
  const response = await authorizedAxiosInstance.put(`seller/discounts/${discountId}`, {
    name: payload.name,
    maxUses: payload.maxUses,
    validTo: payload.validTo,
    isActive: payload.isActive
  })
  return response.metadata ?? response.data ?? response
}

export const deleteDiscountAPI = async (discountId) => {
  const response = await authorizedAxiosInstance.delete(`seller/discounts/${discountId}`)
  return response.metadata ?? response.data ?? response
}
export const checkAvailableAPI = async ({ code, shopId = null, cartItemIds }) => {
  if (!Array.isArray(cartItemIds) || cartItemIds.length === 0) {
    throw new Error('Chọn sản phẩm trước khi áp dụng voucher')
  }
  const response = await authorizedAxiosInstance.post('discounts/validate', { code, shopId, cartItemIds })
  if (response?.isValid === false) {
    throw new Error(response.reasonIfInvalid || 'Voucher không khả dụng')
  }
  return response
}