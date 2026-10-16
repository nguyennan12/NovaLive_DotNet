import authorizedAxiosInstance from '../custom/authorizeAxios'

export const joinLiveAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.post(`livestream/${liveId}/join`)
  return response.metadata
}
export const startLiveAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.post(`livestream/${liveId}/start`)
  return response.metadata
}
export const endLiveAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.post(`livestream/${liveId}/end`)
  return response.metadata
}
export const createLiveSessionAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('livestream', data)
  return response.metadata
}
export const getActiveSessionsAPI = async (params) => {
  const response = await authorizedAxiosInstance.get('livestream/active', { params })
  return response.metadata
}

export const updateLiveSessionAPI = async (liveId, data) => {
  const response = await authorizedAxiosInstance.patch(`livestream/${liveId}`, data)
  return response.metadata
}

export const cancelLiveSessionAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.patch(`livestream/${liveId}/cancel`)
  return response.metadata
}

export const addProductToLiveSessionAPI = async (liveId, products) => {
  const response = await authorizedAxiosInstance.post(`livestream/${liveId}/product`, products)
  return response.metadata
}

export const getLiveSessionByIdAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.get(`livestream/${liveId}`)
  return response.metadata
}

export const removeProductFromLiveSessionAPI = async (liveId, productId) => {
  const response = await authorizedAxiosInstance.delete(`livestream/${liveId}/product/${productId}`)
  return response.metadata
}

export const getLiveHistoryAPI = async (params) => {
  const response = await authorizedAxiosInstance.get(`livestream/history?${params}`)
  return response.metadata
}

export const getUpommingLiveSessionsAPI = async () => {
  const response = await authorizedAxiosInstance.get('livestream/upcomming')
  return response.metadata
}

export const getLiveStatsAPI = async () => {
  const response = await authorizedAxiosInstance.get('livestream/stats')
  return response.metadata
}


const REVENUE_DATA = {
  week: [
    { label: 'T2', revenue: 2100000, viewers: 180 },
    { label: 'T3', revenue: 0, viewers: 0 },
    { label: 'T4', revenue: 3800000, viewers: 280 },
    { label: 'T5', revenue: 0, viewers: 0 },
    { label: 'T6', revenue: 5200000, viewers: 450 },
    { label: 'T7', revenue: 8100000, viewers: 520 },
    { label: 'CN', revenue: 1500000, viewers: 140 }
  ],
  month: [
    { label: 'T1', revenue: 5200000, viewers: 450 },
    { label: 'T2', revenue: 3800000, viewers: 280 },
    { label: 'T3', revenue: 8100000, viewers: 520 },
    { label: 'T4', revenue: 2300000, viewers: 190 },
    { label: 'T5', revenue: 4150000, viewers: 340 },
    { label: 'T6', revenue: 9700000, viewers: 610 },
    { label: 'T7', revenue: 2900000, viewers: 230 },
    { label: 'T8', revenue: 6300000, viewers: 395 }
  ],
  all: [
    { label: 'Th.1', revenue: 12500000, viewers: 890 },
    { label: 'Th.2', revenue: 18200000, viewers: 1240 },
    { label: 'Th.3', revenue: 9700000, viewers: 720 },
    { label: 'Th.4', revenue: 48500000, viewers: 2810 }
  ]
}

export const getLiveRevenueChartAPI = async (period) => REVENUE_DATA[period] ?? REVENUE_DATA.week

// export const getLiveRevenueChartAPI = async (period) => {
//   const response = await authorizedAxiosInstance.get(`livestream/chart?period=${period}`)
//   return response.metadata
// }
