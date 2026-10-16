import authorizedAxiosInstance from '../custom/authorizeAxios'

export const getInfoShopAPI = async (shopId) => {
  const response = await authorizedAxiosInstance.get(`shops/${shopId}`)
  return response.metadata ?? response.data ?? response
}

export const getShopByUserAPI = async () => {
  const response = await authorizedAxiosInstance.get('shops/me')
  return response.metadata ?? response.data ?? response
}

export const registerShopAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('shops/register', data)
  return response.metadata ?? response.data ?? response
}

export const updateShopAPI = async ({ shopId, data }) => {
  const response = await authorizedAxiosInstance.put('seller/shop', data)
  return response.metadata ?? response.data ?? response
}