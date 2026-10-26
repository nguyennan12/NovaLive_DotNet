import authorizedAxiosInstance from '../custom/authorizeAxios'

const normalizeCartItem = (item) => {
  if (!item) return item
  const id = item.cartItemId || item.id || item._id
  const skuId = item.skuId || item.sku_id
  const spuId = item.spuId || item.spu_id
  const quantity = item.quantity ?? item.count ?? 1
  const price = item.price ?? item.sellPrice ?? item.sell_price ?? 0
  let skuName = item.skuName || item.sku_name
  if (!skuName && item.attributesJson) {
    try { skuName = JSON.parse(item.attributesJson) } catch { skuName = null }
  }
  const name = item.spuName || item.spu_name || item.name || ''
  const image = item.thumbnailUrl || item.spu_thumb || item.image || ''

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
    name,
    spu_name: name,
    spuName: name,
    image,
    thumbnailUrl: image,
    spu_thumb: image,
    skuName,
    availableStock: item.availableStock ?? item.stock
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

export const updateCartItemAPI = async ({ cartItemId, quantity }) => {
  if (!cartItemId) throw new Error('cartItemId is required to update a cart item')
  return await authorizedAxiosInstance.put(`cart/items/${cartItemId}`, { quantity })
}

export const removeCartItemAPI = async (cartItemId) => {
  return await authorizedAxiosInstance.delete(`cart/items/${cartItemId}`)
}

export const removeCartItemsAPI = async (cartItemIds = []) => {
  return await authorizedAxiosInstance.delete('cart/items', { data: { cartItemIds } })
}

// Backward compatibility alias for multi/single deletion
export const removeFromCartAPI = async ({ cartItemIds = [] }) => {
  const ids = cartItemIds
  if (ids.length === 1) {
    return await removeCartItemAPI(ids[0])
  }
  return await removeCartItemsAPI(ids)
}

export const validateVoucherAPI = async ({ code, shopId = null, cartItemIds = [] }) => {
  return await authorizedAxiosInstance.post('discounts/validate', { code, shopId, cartItemIds })
}
