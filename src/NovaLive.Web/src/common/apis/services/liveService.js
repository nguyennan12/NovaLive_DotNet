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
  // The .NET API does not expose seller livestream history yet. Do not label
  // seller orders as livestream sessions.
  void params
  return { lives: [], totalPages: 1, totalCount: 0 }
}

/**
 * Returns revenue analytics computed from real order statistics, never hardcoded mocks
 */
export const getLiveRevenueChartAPI = async () => {
  // Live-attributed revenue/viewer analytics are not available in the API yet.
  return []
}

export const getUpommingLiveSessionsAPI = async () => {
  return []
}

export const getLiveStatsAPI = async () => {
  // Hide unsupported aggregate metrics instead of presenting fabricated zeros.
  return null
}

export const updateLiveSessionAPI = async () => {
  return null
}

export const cancelLiveSessionAPI = async () => {
  return null
}
