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

export const normalizeOrder = (o) => {
  if (!o) return o
  const id = o.id || o._id
  const total = o.grandTotal ?? o.order_checkout?.finalCheckout ?? o.order_checkout?.finalTotal ?? o.total ?? 0
  const code = o.orderCode || o.order_trackingNumber || o.code || id
  const subStatuses = (o.subOrders || []).map((sub) => String(sub.status || '').toLowerCase())
  const detailStatus = subStatuses.length === 0
    ? null
    : subStatuses.every((value) => value === 'cancelled')
      ? 'cancelled'
      : subStatuses.includes('pendingpayment')
        ? 'pending'
        : subStatuses.some((value) => ['confirmed', 'processing'].includes(value))
          ? 'processing'
          : subStatuses.includes('shipping')
            ? 'shipped'
            : subStatuses.every((value) => ['delivered', 'completed', 'cancelled'].includes(value))
              ? 'delivered'
              : 'processing'
  const status = String(o.status || o.order_status || detailStatus || o.paymentStatus || 'pending').toLowerCase()

  let rawItems = o.items || o.order_products || []
  if ((!rawItems || rawItems.length === 0) && Array.isArray(o.subOrders)) {
    rawItems = o.subOrders.flatMap((sub) => {
      const items = sub.items || []
      return items.map((it) => {
        let snapshot = {}
        try {
          snapshot = typeof it.skuSnapshotJson === 'string' ? JSON.parse(it.skuSnapshotJson) : (it.skuSnapshotJson || {})
        } catch {
          // ignore
        }
        return {
          id: it.id,
          skuId: it.skuId,
          quantity: it.quantity,
          price: it.unitPrice ?? it.originalPrice ?? 0,
          spuName: snapshot.SpuName || snapshot.spuName || snapshot.name || 'Sản phẩm',
          thumbnailUrl: snapshot.ThumbnailUrl || snapshot.thumbnailUrl || snapshot.thumb || '',
          shopName: sub.shopName || 'Nova Flagship Store'
        }
      })
    })
  }

  return {
    ...o,
    _id: id,
    id,
    order_trackingNumber: code,
    orderCode: code,
    order_status: status,
    order_checkout: {
      finalCheckout: total,
      finalTotal: total,
      totalPrice: total,
      ...o.order_checkout
    },
    order_payment: {
      paymentStatus: (o.paymentStatus || o.order_payment?.paymentStatus || 'pending').toLowerCase(),
      method: o.paymentMethod || o.order_payment?.method || 'VietQR',
      ...o.order_payment
    },
    order_products: rawItems.map((p) => ({
      ...p,
      productId: {
        spu_name: p.spuName || p.name || p.productId?.spu_name || 'Sản phẩm',
        spu_thumb: p.thumbnailUrl || p.thumb || p.productId?.spu_thumb || '',
        spu_shopId: {
          shop_name: p.shopName || o.shopName || p.productId?.spu_shopId?.shop_name || 'Nova Flagship Store'
        }
      },
      price: p.price ?? 0,
      quantity: p.quantity ?? 1
    }))
  }
}

export const getMyOrdersAPI = async (filters = {}) => {
  const params = new URLSearchParams()
  if (filters.status && filters.status !== 'all') params.append('status', filters.status)
  if (filters.page) params.append('page', filters.page)
  if (filters.size || filters.limit) params.append('size', filters.size || filters.limit)
  if (filters.from) params.append('from', filters.from)
  if (filters.to) params.append('to', filters.to)

  const res = await authorizedAxiosInstance.get(`orders?${params.toString()}`)
  const rawItems = res?.items ?? (Array.isArray(res) ? res : [])
  const enriched = await Promise.all(
    rawItems.map(async (item) => {
      try {
        const detail = await authorizedAxiosInstance.get(`orders/${item.id}`)
        return normalizeOrder(detail?.data !== undefined ? detail.data : detail)
      } catch {
        return normalizeOrder(item)
      }
    })
  )
  return enriched
}

export const getOrderDetailAPI = async (orderId) => {
  const res = await authorizedAxiosInstance.get(`orders/${orderId}`)
  return normalizeOrder(res?.data !== undefined ? res.data : res)
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

export const getPaymentStatusAPI = async (orderId) => {
  try {
    const res = await authorizedAxiosInstance.get(`orders/${orderId}`)
    const order = res?.data !== undefined ? res.data : res
    const isPaid = String(order?.paymentStatus || '').toLowerCase() === 'paid'
    return {
      status: order?.paymentStatus || (isPaid ? 'Paid' : 'Pending'),
      isPaid,
      order
    }
  } catch (_e) {
    return { status: 'Pending', isPaid: false }
  }
}

// ─── Backward compatibility aliases ──────────────────────────────────────────
export const OrderByUserAPI = checkoutAPI
