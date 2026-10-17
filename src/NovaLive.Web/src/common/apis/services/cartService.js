import authorizedAxiosInstance from '../custom/authorizeAxios'

const normalizeCartItem = (item) => {
  if (!item) return item
  const id = item.cartItemId || item.id || item._id
  const skuId = item.skuId || item.sku_id
  const spuId = item.spuId || item.spu_id
  const quantity = item.quantity || item.count || 1
  const price = item.price ?? item.sellPrice ?? item.sell_price ?? 0

  return {
    ...item,
    cartItemId: id,
    _id: id,
    id,
    skuId,
    spuId,
    quantity,
    count: quantity,
    price,
    sell_price: price,
    spu_name: item.spuName || item.spu_name || '',
    spuName: item.spuName || item.spu_name || '',
    thumbnailUrl: item.thumbnailUrl || item.spu_thumb || '',
    spu_thumb: item.thumbnailUrl || item.spu_thumb || '',
    availableStock: item.availableStock ?? item.stock ?? 999
  }
}

const normalizeShopGroup = (group) => {
  if (!group) return group
  const items = Array.isArray(group.items) ? group.items.map(normalizeCartItem) : []
  return {
    ...group,
    shopId: group.shopId || group.shop_id,
    shopName: group.shopName || group.shop_name || 'Shop',
    items
  }
}

export const getCartAPI = async () => {
  const res = await authorizedAxiosInstance.get('cart')
  const rawGroups = res?.shopGroups || res?.items || (Array.isArray(res) ? res : [])
  const shopGroups = rawGroups.map(normalizeShopGroup)

  return {
    userId: res?.userId,
    cartId: res?.userId || res?.cartId,
    totalItemsCount: res?.totalItemsCount ?? shopGroups.reduce((acc, g) => acc + g.items.length, 0),
    cartCount: res?.totalItemsCount ?? shopGroups.reduce((acc, g) => acc + g.items.length, 0),
    grandTotal: res?.grandTotal ?? 0,
    shopGroups,
    items: shopGroups // backward compatibility with useCart
  }
}

export const addToCartAPI = async ({ skuId, quantity = 1 }) => {
  return await authorizedAxiosInstance.post('cart/items', { skuId, quantity })
}

export const updateCartItemAPI = async ({ cartItemId, skuId, quantity }) => {
  const targetId = cartItemId || skuId
  return await authorizedAxiosInstance.put(`cart/items/${targetId}`, { quantity })
}

export const removeCartItemAPI = async (cartItemId) => {
  return await authorizedAxiosInstance.delete(`cart/items/${cartItemId}`)
}

export const removeCartItemsAPI = async (cartItemIds = []) => {
  return await authorizedAxiosInstance.delete('cart/items', { data: { cartItemIds } })
}

// Backward compatibility alias for multi/single deletion
export const removeFromCartAPI = async ({ skuIds, cartItemIds }) => {
  const ids = cartItemIds || skuIds || []
  if (ids.length === 1) {
    return await removeCartItemAPI(ids[0])
  }
  return await removeCartItemsAPI(ids)
}

export const validateVoucherAPI = async ({ code, shopId = null, cartItemIds = [] }) => {
  if (typeof code === 'string' && arguments.length === 1) {
    return await authorizedAxiosInstance.post('discounts/validate', {
      code,
      shopId: null,
      cartItemIds: []
    })
  }
  return await authorizedAxiosInstance.post('discounts/validate', { code, shopId, cartItemIds })
}
