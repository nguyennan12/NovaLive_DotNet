import authorizedAxiosInstance from '../custom/authorizeAxios'

export const getCartAPI = async () => {
  const response = await authorizedAxiosInstance.get('cart')
  return response.metadata ?? response.data ?? response
}

export const addToCartAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('cart/items', data)
  return response.metadata ?? response.data ?? response
}

export const updateCartItemAPI = async ({ cartItemId, skuId, quantity, old_quantity }) => {
  const id = cartItemId || skuId
  const response = await authorizedAxiosInstance.put(`cart/items/${id}`, { quantity })
  return response.metadata ?? response.data ?? response
}

export const removeFromCartAPI = async ({ cartItemId, skuIds }) => {
  if (cartItemId) {
    const response = await authorizedAxiosInstance.delete(`cart/items/${cartItemId}`)
    return response.metadata ?? response.data ?? response
  }
  const response = await authorizedAxiosInstance.delete('cart/items', { data: { cartItemIds: skuIds } })
  return response.metadata ?? response.data ?? response
}

export const validateVoucherAPI = async (code) => {
  const response = await authorizedAxiosInstance.get(`discounts?code=${encodeURIComponent(code)}`)
  return response.metadata ?? response.data ?? response
}

