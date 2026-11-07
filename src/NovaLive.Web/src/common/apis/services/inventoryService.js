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
    qtyOnHand: Number(item.qtyOnHand ?? 0),
    availableQty: Number(item.availableQty ?? (Number(item.qtyOnHand ?? 0) - Number(item.reservedQty ?? 0))),
    reservations: []
  }
}

export const normalizeHistoryItem = (h) => {
  if (!h) return h
  const qtyChange = Number(h.qtyChange ?? h.qty_change ?? h.inven_quantity ?? 0)
  const reservedChange = Number(h.reservedChange ?? h.reserved_change ?? 0)
  const createdAt = h.createdAt || h.created_at || h.updatedAt || h.updated_at || null
  const isReservation = qtyChange === 0 && reservedChange !== 0
  const isIn = qtyChange > 0 || h.changeType === 'Import' || h.changeType === 'ReturnIn' || h.changeType === 'IN'
  return {
    ...h,
    _id: h.id || h._id,
    id: h.id || h._id,
    inven_type: isReservation
      ? (reservedChange > 0 ? 'RESERVE_IN' : 'RESERVE_OUT')
      : (isIn ? 'IN' : 'OUT'),
    inven_productId: {
      spu_name: h.spuName || h.inven_productId?.spu_name || 'Product'
    },
    inven_skuId: {
      sku_id: h.skuCode || h.inven_skuId?.sku_id || 'SKU'
    },
    inven_stock: Math.abs(isReservation ? reservedChange : qtyChange),
    inven_quantity: Math.abs(isReservation ? reservedChange : qtyChange),
    inven_delta: isReservation ? reservedChange : qtyChange,
    inven_note: h.note || h.inven_note || '',
    inven_user: {
      user_name: 'Warehouse System'
    },
    createdAt
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
export const getChartArtInventoryAPI = async (period = 'today') => {
  try {
    const now = new Date()
    const from = new Date(now)
    if (period === 'today') from.setHours(0, 0, 0, 0)
    else if (period === 'week') {
      from.setDate(from.getDate() - 6)
      from.setHours(0, 0, 0, 0)
    } else {
      from.setDate(from.getDate() - 29)
      from.setHours(0, 0, 0, 0)
    }
    const filter = new URLSearchParams({ from: from.toISOString(), to: now.toISOString(), page: '1', size: '100' })
    const firstResponse = await authorizedAxiosInstance.get(`seller/inventory/histories?${filter}`)
    const firstPage = firstResponse?.data !== undefined ? firstResponse.data : firstResponse
    const items = [...(firstPage?.items ?? (Array.isArray(firstPage) ? firstPage : []))]
    const totalPages = Number(firstPage?.totalPages ?? 1)
    if (totalPages > 1) {
      const remainingPages = await Promise.all(Array.from({ length: totalPages - 1 }, (_, index) => {
        const pageQuery = new URLSearchParams({ from: from.toISOString(), to: now.toISOString(), page: String(index + 2), size: '100' })
        return authorizedAxiosInstance.get(`seller/inventory/histories?${pageQuery}`)
      }))
      remainingPages.forEach(response => {
        const page = response?.data !== undefined ? response.data : response
        items.push(...(page?.items ?? []))
      })
    }
    if (items.length === 0) return []

    const grouped = {}
    items.forEach((item) => {
      const createdAt = new Date(item.createdAt || item.created_at)
      if (!Number.isFinite(createdAt.getTime())) return
      const dateKey = period === 'today'
        ? createdAt.toLocaleTimeString('vi-VN', { hour: '2-digit' })
        : createdAt.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' })
      if (!grouped[dateKey]) {
        grouped[dateKey] = { label: dateKey, in: 0, out: 0, net: 0, timestamp: createdAt.getTime() }
      }
      const qtyChange = Number(item.qtyChange ?? item.qty_change ?? 0)
      // Reservation changes do not alter physical stock and must not appear as Stock Out.
      if (qtyChange === 0) return
      if (qtyChange > 0) {
        grouped[dateKey].in += qtyChange
      } else {
        grouped[dateKey].out += Math.abs(qtyChange)
      }
      grouped[dateKey].net = grouped[dateKey].in - grouped[dateKey].out
    })

    return Object.values(grouped).sort((a, b) => a.timestamp - b.timestamp).map(({ timestamp, ...point }) => point)
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
