import authorizedAxiosInstance from '../custom/authorizeAxios'

export const normalizeInventoryItem = (item) => {
  if (!item) return item
  return {
    ...item,
    inventoryId: item.skuId || item.id || item._id,
    skuId: item.skuId || item.id || item._id,
    product: {
      spu_name: item.spuName || item.product?.spu_name || item.skuCode
    },
    sku: {
      sku_name: item.skuCode || item.sku?.sku_name
    },
    total_reserved: item.reservedQty ?? item.total_reserved ?? 0,
    qtyOnHand: item.qtyOnHand ?? 0,
    availableQty: item.availableQty ?? 0,
    reservations: []
  }
}

export const normalizeHistoryItem = (h) => {
  if (!h) return h
  const isIn = h.qtyChange > 0 || h.changeType === 'Restock' || h.changeType === 'IN'
  return {
    ...h,
    _id: h.id || h._id,
    id: h.id || h._id,
    inven_type: isIn ? 'IN' : 'OUT',
    inven_productId: {
      spu_name: h.spuName || h.inven_productId?.spu_name || 'Product'
    },
    inven_skuId: {
      sku_id: h.skuCode || h.inven_skuId?.sku_id || 'SKU'
    },
    inven_stock: Math.abs(h.qtyChange || h.inven_stock || 0),
    inven_note: h.note || h.inven_note || '',
    inven_user: {
      user_name: 'Warehouse System'
    },
    createdAt: h.createdAt
  }
}

/**
 * Adjust stock on hand (Stock In or Stock Out)
 * @param {object} payload - AdjustInventoryRequest
 * @param {string} payload.skuId
 * @param {number} payload.qtyChange
 * @param {string} payload.changeType
 * @param {string} [payload.note]
 */
export const addInventoryAPI = async (payload) => {
  const body = {
    skuId: payload.skuId,
    qtyChange: Number(payload.qtyChange ?? payload.stock ?? 0),
    changeType: payload.changeType || (payload.type === 'OUT' ? 'Damage' : 'Restock'),
    note: payload.note || null
  }
  const res = await authorizedAxiosInstance.post('seller/inventory/adjust', body)
  return res?.data !== undefined ? res.data : res
}

/**
 * Fetch inventory stock movement histories with filters
 */
export const getHistoryInventoryAPI = async (query = '') => {
  const q = typeof query === 'string' ? query : new URLSearchParams(query).toString()
  const res = await authorizedAxiosInstance.get(`seller/inventory/histories?${q}`)
  const raw = res?.data !== undefined ? res.data : res
  const items = (raw?.items ?? (Array.isArray(raw) ? raw : [])).map(normalizeHistoryItem)
  return {
    items,
    totalItems: raw?.totalCount ?? items.length,
    totalPages: raw?.totalPages ?? 1,
    page: raw?.page ?? 1,
    size: raw?.size ?? items.length
  }
}

/**
 * Aggregate real-time stock-in and stock-out analytics from history
 */
export const getChartArtInventoryAPI = async () => {
  try {
    const res = await authorizedAxiosInstance.get('seller/inventory/histories?size=100')
    const raw = res?.data !== undefined ? res.data : res
    const items = raw?.items ?? (Array.isArray(raw) ? raw : [])
    if (items.length === 0) return []

    const grouped = {}
    items.forEach((item) => {
      const dateKey = new Date(item.createdAt).toLocaleDateString('vi-VN', {
        month: '2-digit',
        day: '2-digit'
      })
      if (!grouped[dateKey]) {
        grouped[dateKey] = { label: dateKey, in: 0, out: 0, net: 0 }
      }
      if (item.qtyChange > 0) {
        grouped[dateKey].in += item.qtyChange
      } else {
        grouped[dateKey].out += Math.abs(item.qtyChange)
      }
      grouped[dateKey].net = grouped[dateKey].in - grouped[dateKey].out
    })

    return Object.values(grouped).slice(-7)
  } catch (_e) {
    return []
  }
}

/**
 * Fetch reserved stock (held by active checkouts)
 */
export const getReservedStocAPI = async () => {
  try {
    const res = await authorizedAxiosInstance.get('seller/inventory?size=100')
    const raw = res?.data !== undefined ? res.data : res
    const items = raw?.items ?? (Array.isArray(raw) ? raw : [])
    return items
      .filter((i) => (i.reservedQty || 0) > 0)
      .map(normalizeInventoryItem)
  } catch (_e) {
    return []
  }
}

/**
 * Fetch seller inventory stock list
 */
export const getSellerInventoryAPI = async (params = {}) => {
  const q = new URLSearchParams(params).toString()
  const res = await authorizedAxiosInstance.get(`seller/inventory?${q}`)
  const raw = res?.data !== undefined ? res.data : res
  const items = (raw?.items ?? (Array.isArray(raw) ? raw : [])).map(normalizeInventoryItem)
  return {
    items,
    totalCount: raw?.totalCount ?? items.length,
    totalPages: raw?.totalPages ?? 1,
    page: raw?.page ?? 1,
    size: raw?.size ?? items.length
  }
}