import authorizedAxiosInstance from '../custom/authorizeAxios'

/**
 * Calculates realtime draft checkout (subtotal, shipping, discounts, grand total)
 * @param {object} payload - CalculateCheckoutRequest
 * @param {Array<string>} payload.cartItemIds
 * @param {string} payload.shippingAddressId
 * @param {Array<{ shopId: string, code: string }>} [payload.shopVouchers]
 * @param {string} [payload.platformProductVoucherCode]
 * @param {string} [payload.platformFreeshipVoucherCode]
 */
export const calculateCheckoutAPI = async (payload) => {
  return await authorizedAxiosInstance.post('orders/calculate-checkout', payload)
}

/**
 * Submits official multi-shop checkout transaction
 * @param {object} payload - CheckoutRequest
 * @param {Array<string>} payload.cartItemIds
 * @param {string} payload.shippingAddressId
 * @param {Array<{ shopId: string, provider: string, serviceCode: string }>} payload.shippingProviders
 * @param {Array<{ shopId: string, code: string }>} [payload.shopVouchers]
 * @param {string} [payload.platformProductVoucherCode]
 * @param {string} [payload.platformFreeshipVoucherCode]
 * @param {string} payload.paymentMethod - 'MoMo' | 'VietQR' | 'COD'
 * @param {string} [payload.note]
 * @param {string} [idempotencyKey]
 */
export const checkoutAPI = async (payload, idempotencyKey) => {
  const key = idempotencyKey || (typeof crypto !== 'undefined' && crypto.randomUUID ? crypto.randomUUID() : `idemp-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`)
  return await authorizedAxiosInstance.post('orders/checkout', payload, {
    headers: {
      'Idempotency-Key': key
    }
  })
}

export const getMyOrdersAPI = async (filters = {}) => {
  const params = new URLSearchParams()
  if (filters.status && filters.status !== 'all') params.append('status', filters.status)
  if (filters.page) params.append('page', filters.page)
  if (filters.size || filters.limit) params.append('size', filters.size || filters.limit)
  if (filters.from) params.append('from', filters.from)
  if (filters.to) params.append('to', filters.to)

  return await authorizedAxiosInstance.get(`orders?${params.toString()}`)
}

export const getOrderDetailAPI = async (orderId) => {
  return await authorizedAxiosInstance.get(`orders/${orderId}`)
}

export const cancelOrderAPI = async (orderId, reason = 'Buyer cancelled') => {
  return await authorizedAxiosInstance.post(`orders/${orderId}/cancel`, { reason })
}

export const getOrderTrackingAPI = async (orderId) => {
  return await authorizedAxiosInstance.get(`orders/${orderId}/tracking`)
}

// ─── Payment APIs ────────────────────────────────────────────────────────────

export const generateVietQrAPI = async (orderCode) => {
  return await authorizedAxiosInstance.get(`payments/vietqr/generate/${orderCode}`)
}

export const createMomoPaymentAPI = async ({ orderId, orderInfo = 'Thanh toan don hang NovaLive', extraData = '' }) => {
  return await authorizedAxiosInstance.post('payments/momo/create', {
    orderId: String(orderId),
    orderInfo,
    extraData
  })
}

export const getPaymentStatusAPI = async (paymentId) => {
  return await authorizedAxiosInstance.get(`payments/${paymentId}/status`)
}

// ─── Backward compatibility aliases ──────────────────────────────────────────
export const OrderByUserAPI = checkoutAPI