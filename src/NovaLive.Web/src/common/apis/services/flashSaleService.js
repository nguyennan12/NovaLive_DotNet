import authorizedAxiosInstance from '../custom/authorizeAxios'

export const getActiveCampaignAPI = async () => {
  const response = await authorizedAxiosInstance.get('flash-sales/active')
  return response.metadata ?? response.data ?? response
}

export const getFlashSaleItemsAPI = async (campaignId) => {
  const response = await authorizedAxiosInstance.get(`flash-sales/${campaignId}/products`)
  return response.metadata ?? response.data ?? response
}

