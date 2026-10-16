import authorizedAxiosInstance from '../custom/authorizeAxios'

export const getUserAddressAPI = async (addressId) => {
  const response = await authorizedAxiosInstance.get(`users/me/addresses`)
  return response.metadata ?? response.data ?? response
}
export const getAllUserAddressAPI = async () => {
  const response = await authorizedAxiosInstance.get('users/me/addresses')
  return response.metadata ?? response.data ?? response
}
export const createAddressAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('users/me/addresses', data)
  return response.metadata ?? response.data ?? response
}
export const updateAddressAPI = async (id, data) => {
  const response = await authorizedAxiosInstance.put(`users/me/addresses/${id}`, data)
  return response.metadata ?? response.data ?? response
}
export const deleteAddressAPI = async (id) => {
  const response = await authorizedAxiosInstance.delete(`users/me/addresses/${id}`)
  return response.metadata ?? response.data ?? response
}
export const setDefaultAddressAPI = async (id) => {
  const response = await authorizedAxiosInstance.put(`users/me/addresses/${id}/default`)
  return response.metadata ?? response.data ?? response
}