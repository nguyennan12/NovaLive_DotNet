import authorizedAxiosInstance from '../custom/authorizeAxios'

const normalizeShop = (shop) => shop && ({
  ...shop,
  id: shop.id || shop._id,
  _id: shop.id || shop._id,
  shop_name: shop.shopName || shop.shop_name || '',
  shop_slug: shop.slug || shop.shop_slug || '',
  shop_status: shop.status || shop.shop_status || '',
  shop_logo: shop.logoUrl || shop.shop_logo || '',
  shop_banner: shop.bannerUrl || shop.shop_banner || '',
  shop_contact: { phone: shop.phone || shop.shop_contact?.phone || '', email: shop.email || shop.shop_contact?.email || '' },
  shop_metrics: {
    ...shop.shop_metrics,
    rating_avg: shop.rating ?? shop.shop_metrics?.rating_avg,
    total_products: shop.productsCount ?? shop.shop_metrics?.total_products,
    follower_count: shop.followersCount ?? shop.shop_metrics?.follower_count,
    total_sold: shop.totalSold ?? shop.shop_metrics?.total_sold
  }
})

export const getInfoShopAPI = async (shopId) => {
  const response = await authorizedAxiosInstance.get(`shops/${shopId}`)
  return normalizeShop(response?.data !== undefined ? response.data : response)
}

export const getShopByUserAPI = async () => {
  const response = await authorizedAxiosInstance.get('shops/me')
  return normalizeShop(response?.data !== undefined ? response.data : response)
}

export const registerShopAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('shops/register', data)
  return normalizeShop(response?.data !== undefined ? response.data : response)
}

export const updateShopAPI = async (payload) => {
  const body = payload?.data || payload
  const request = {
    shopName: body.shopName || body.shop_name,
    description: body.description || '',
    logoUrl: body.logoUrl ?? null,
    bannerUrl: body.bannerUrl ?? null,
    phone: body.phone || body.shop_contact?.phone || '',
    email: body.email || body.shop_contact?.email || ''
  }
  const response = await authorizedAxiosInstance.put('seller/shop/me', request)
  return normalizeShop(response?.data !== undefined ? response.data : response)
}

export const getShopAddressesAPI = async () => {
  const response = await authorizedAxiosInstance.get('seller/shop/addresses')
  return response?.data !== undefined ? response.data : response
}

export const addShopAddressAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('seller/shop/addresses', data)
  return response?.data !== undefined ? response.data : response
}

export const updateShopAddressAPI = async (id, data) => {
  const response = await authorizedAxiosInstance.put(`seller/shop/addresses/${id}`, data)
  return response?.data !== undefined ? response.data : response
}

export const deleteShopAddressAPI = async (id) => {
  const response = await authorizedAxiosInstance.delete(`seller/shop/addresses/${id}`)
  return response?.data !== undefined ? response.data : response
}

export const followShopAPI = async (shopId) => {
  const response = await authorizedAxiosInstance.post(`shops/${shopId}/follow`)
  return response?.data !== undefined ? response.data : response
}

export const unfollowShopAPI = async (shopId) => {
  const response = await authorizedAxiosInstance.delete(`shops/${shopId}/follow`)
  return response?.data !== undefined ? response.data : response
}
