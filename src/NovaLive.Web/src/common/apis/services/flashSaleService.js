import authorizedAxiosInstance from '../custom/authorizeAxios'

export const getActiveCampaignAPI = async () => {
  try {
    const response = await authorizedAxiosInstance.get('flash-sales/active')
    const raw = response?.data !== undefined ? response.data : response
    return Array.isArray(raw) ? raw : (raw?.items ?? (raw ? [raw] : []))
  } catch (error) {
    console.error('getActiveCampaignAPI error:', error)
    return []
  }
}

export const getFlashSaleItemsAPI = async (campaignId) => {
  if (!campaignId) return { items: [], totalCount: 0 }
  try {
    const response = await authorizedAxiosInstance.get(`flash-sales/${campaignId}/products`)
    const raw = response?.data !== undefined ? response.data : response
    return raw
  } catch (error) {
    console.error('getFlashSaleItemsAPI error:', error)
    return { items: [], totalCount: 0 }
  }
}

