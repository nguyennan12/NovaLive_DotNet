import authorizedAxiosInstance from '../custom/authorizeAxios'

export const addInventoryAPI = async (data) => {
  const res = await authorizedAxiosInstance.post('seller/inventory/adjust', data)
  return res.metadata ?? res.data ?? res
}
export const getHistoryInventoryAPI = async (query) => {
  const res = await authorizedAxiosInstance.get(`seller/inventory/histories?${query}`)
  return res.metadata ?? res.data ?? res
}
export const getChartArtInventoryAPI = async (period) => {
  const res = await authorizedAxiosInstance.get(`seller/inventory?period=${period}`)
  return res.metadata ?? res.data ?? res
}
export const getReservedStocAPI = async () => {
  const res = await authorizedAxiosInstance.get('seller/inventory')
  return res.metadata ?? res.data ?? res
}