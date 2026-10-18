import authorizedAxiosInstance from '../custom/authorizeAxios'

export const getInfoShopAPI = async (shopId) => {
  const response = await authorizedAxiosInstance.get(`shops/${shopId}`)
  return response?.data !== undefined ? response.data : response
}

export const getShopByUserAPI = async () => {
  const response = await authorizedAxiosInstance.get('shops/me')
  return response?.data !== undefined ? response.data : response
}

export const registerShopAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('shops/register', data)
  return response?.data !== undefined ? response.data : response
}

export const updateShopAPI = async (payload) => {
  // Accepts either { data } or raw data object
  const body = payload?.data || payload
  const response = await authorizedAxiosInstance.put('seller/shop/me', body)
  return response?.data !== undefined ? response.data : response
}

export const getShopAddressesAPI = async () => {
  const response = await authorizedAxiosInstance.get('seller/shop/addresses')
  return response?.data !== undefined ? response.data : response
}

export const addShopAddressAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('seller/shop/addresses', data)
  return response?.data !== undefined ? response.data : response
}

export const updateShopAddressAPI = async (id, data) => {
  const response = await authorizedAxiosInstance.put(`seller/shop/addresses/${id}`, data)
  return response?.data !== undefined ? response.data : response
}

export const deleteShopAddressAPI = async (id) => {
  const response = await authorizedAxiosInstance.delete(`seller/shop/addresses/${id}`)
  return response?.data !== undefined ? response.data : response
}

export const followShopAPI = async (shopId) => {
  const response = await authorizedAxiosInstance.post(`shops/${shopId}/follow`)
  return response?.data !== undefined ? response.data : response
}

export const unfollowShopAPI = async (shopId) => {
  const response = await authorizedAxiosInstance.delete(`shops/${shopId}/follow`)
  return response?.data !== undefined ? response.data : response
}