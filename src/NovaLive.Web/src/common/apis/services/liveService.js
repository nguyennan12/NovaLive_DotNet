import authorizedAxiosInstance from '../custom/authorizeAxios'

/**
 * Normalizes a livestream session DTO to match both new .NET schema
 * and frontend player/feed expectations.
 */
export const normalizeLiveSession = (session) => {
  if (!session) return session
  return {
    ...session,
    _id: session.id || session._id,
    id: session.id || session._id,
    live_title: session.title || session.live_title,
    title: session.title || session.live_title,
    live_thumb: session.thumbnailUrl || session.live_thumb,
    thumbnailUrl: session.thumbnailUrl || session.live_thumb,
    live_status: session.status || session.live_status,
    status: session.status || session.live_status,
    live_viewer_count: session.viewerCount ?? session.live_viewer_count ?? 0,
    viewerCount: session.viewerCount ?? session.live_viewer_count ?? 0,
    live_code: session.channelName || session.live_code || (session.id || session._id || '').toString(),
    live_schedule_at: session.scheduledAt || session.startedAt || session.createdAt || session.live_schedule_at,
    live_actual_start: session.startedAt || session.live_actual_start,
    live_actual_end: session.endedAt || session.live_actual_end,
    live_metrics: session.live_metrics || {
      peak_viewers: session.peakViewers ?? session.peakViewerCount ?? 0,
      total_orders: session.totalOrdersCount ?? 0,
      total_revenue: session.totalRevenue ?? 0
    },
    channelName: session.channelName || (session.id ? session.id.toString() : ''),
    shop: {
      _id: session.shopId,
      shop_name: session.shopName,
      shop_logo: session.shopLogoUrl
    }
  }
}

/**
 * Audience joins livestream and receives Agora audience token
 */
export const joinLiveAPI = async (liveId, request = {}) => {
  const response = await authorizedAxiosInstance.post(`livestreams/${liveId}/token`, request)
  const raw = response?.data !== undefined ? response.data : response
  return {
    token: raw?.token,
    channelName: raw?.channelName || (liveId ? liveId.toString() : ''),
    account: raw?.account,
    role: raw?.role,
    ...raw
  }
}

/**
 * Seller starts a new livestream session
 */
export const createLiveSessionAPI = async (payload) => {
  const body = {
    title: payload.title || payload.live_title || 'NovaLive Stream',
    thumbnailUrl: payload.thumbnailUrl || payload.live_thumb || null,
    warehouseAddressId: payload.warehouseAddressId || null
  }
  const response = await authorizedAxiosInstance.post('seller/livestreams/start', body)
  const raw = response?.data !== undefined ? response.data : response
  return {
    token: raw?.publisherToken,
    publisherToken: raw?.publisherToken,
    channelName: raw?.channelName || (raw?.id ? raw.id.toString() : ''),
    ...normalizeLiveSession(raw)
  }
}

export const scheduleLiveSessionAPI = async (payload) => {
  const response = await authorizedAxiosInstance.post('seller/livestreams/schedule', {
    title: payload.title,
    thumbnailUrl: payload.thumbnailUrl || null,
    scheduledAt: payload.scheduledAt
  })
  const raw = response?.data !== undefined ? response.data : response
  return normalizeLiveSession(raw)
}

/**
 * Seller renews publisher token for an ongoing livestream
 */
export const startLiveAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.post(`seller/livestreams/${liveId}/token`)
  const raw = response?.data !== undefined ? response.data : response
  return {
    token: raw?.token,
    publisherToken: raw?.token,
    channelName: raw?.channelName || (liveId ? liveId.toString() : ''),
    ...raw
  }
}

export const startScheduledLiveAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.post(`seller/livestreams/${liveId}/start`)
  const raw = response?.data !== undefined ? response.data : response
  return {
    token: raw?.publisherToken,
    publisherToken: raw?.publisherToken,
    channelName: raw?.channelName || (liveId ? liveId.toString() : ''),
    ...normalizeLiveSession(raw)
  }
}

/**
 * Seller ends an active livestream session
 */
export const endLiveAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.post(`seller/livestreams/${liveId}/end`)
  return response?.data !== undefined ? response.data : response
}

/**
 * Retrieves public active livestream sessions for feed/discovery
 */
export const getActiveSessionsAPI = async (params = {}) => {
  const page = params.page || 1
  const size = params.limit || params.size || 20
  const response = await authorizedAxiosInstance.get(`livestreams/active?page=${page}&size=${size}`)
  const raw = response?.data !== undefined ? response.data : response
  const items = (raw?.items ?? (Array.isArray(raw) ? raw : [])).map(normalizeLiveSession)
  return {
    lives: items,
    items,
    nextCursor: raw?.page < raw?.totalPages ? raw.page + 1 : null,
    hasMore: raw?.page < raw?.totalPages,
    totalCount: raw?.totalCount ?? items.length,
    page: raw?.page ?? page,
    size: raw?.size ?? size
  }
}

/**
 * Retrieves details of a single livestream session
 */
export const getLiveSessionByIdAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.get(`livestreams/${liveId}`)
  const raw = response?.data !== undefined ? response.data : response
  const normalized = normalizeLiveSession(raw)
  return {
    ...normalized,
    live_products: raw?.activePinnedProduct ? [raw.activePinnedProduct] : []
  }
}

/**
 * Retrieves currently pinned product for a livestream session
 */
export const getPinnedProductAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.get(`livestreams/${liveId}/pinned-product`)
  return response?.data !== undefined ? response.data : response
}

/**
 * Seller pins a product / flash deal into the livestream
 */
export const addProductToLiveSessionAPI = async (liveId, payload) => {
  const body = {
    skuId: payload.skuId || payload.productId,
    flashPrice: payload.flashPrice ? Number(payload.flashPrice) : null,
    quantityLimit: payload.quantityLimit ? Number(payload.quantityLimit) : null,
    durationSeconds: payload.durationSeconds ? Number(payload.durationSeconds) : 120
  }
  const response = await authorizedAxiosInstance.post(`seller/livestreams/${liveId}/pin-product`, body)
  return response?.data !== undefined ? response.data : response
}

/**
 * Seller unpins a product from the livestream
 */
export const removeProductFromLiveSessionAPI = async (liveId, skuId) => {
  const response = await authorizedAxiosInstance.delete(`seller/livestreams/${liveId}/pin-product/${skuId}`)
  return response?.data !== undefined ? response.data : response
}

/**
 * Livestream history - fetched from real backend orders/livestreams, no mock data
 */
export const getLiveHistoryAPI = async (params = {}) => {
  const response = await authorizedAxiosInstance.get('seller/livestreams', { params })
  const raw = response?.data !== undefined ? response.data : response
  const lives = (raw?.items || []).map(normalizeLiveSession)
  return {
    lives,
    totalPages: raw?.totalPages ?? 1,
    totalCount: raw?.total ?? lives.length,
    page: raw?.page ?? params.page ?? 1,
    size: raw?.size ?? params.size ?? lives.length
  }
}

/**
 * Returns revenue analytics computed from real order statistics, never hardcoded mocks
 */
export const getLiveRevenueChartAPI = async (period = 'week') => {
  const response = await authorizedAxiosInstance.get('seller/livestreams/analytics', { params: { period } })
  const raw = response?.data !== undefined ? response.data : response
  return (raw?.points || []).map(point => ({
    label: point.label,
    revenue: point.revenue || 0,
    viewers: point.peakViewers || 0,
    units: point.unitsSold || 0
  }))
}

export const getUpommingLiveSessionsAPI = async () => {
  const response = await authorizedAxiosInstance.get('seller/livestreams', { params: { page: 1, size: 100, status: 'scheduled' } })
  const raw = response?.data !== undefined ? response.data : response
  return (raw?.items || []).map(normalizeLiveSession)
}

export const getLiveStatsAPI = async () => {
  const response = await authorizedAxiosInstance.get('seller/livestreams/analytics', { params: { period: 'all' } })
  const raw = response?.data !== undefined ? response.data : response
  return {
    total_sessions: raw?.totalSessions || 0,
    total_revenue: raw?.totalRevenue || 0,
    avg_viewers: raw?.averagePeakViewers || 0,
    avg_order_value: raw?.averageRevenuePerSession || 0,
    total_units_sold: raw?.totalUnitsSold || 0
  }
}

export const updateLiveSessionAPI = async (liveId, payload) => {
  const response = await authorizedAxiosInstance.put(`seller/livestreams/${liveId}/schedule`, {
    title: payload.title,
    thumbnailUrl: payload.thumbnailUrl || null,
    scheduledAt: payload.scheduledAt
  })
  return response?.data !== undefined ? response.data : response
}

export const cancelLiveSessionAPI = async (liveId) => {
  const response = await authorizedAxiosInstance.delete(`seller/livestreams/${liveId}/schedule`)
  return response?.data !== undefined ? response.data : response
}
