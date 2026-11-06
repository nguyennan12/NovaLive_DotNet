import authorizedAxiosInstance from '../custom/authorizeAxios'

export const createReviewAPI = async (payload) =>
  authorizedAxiosInstance.post('reviews', payload)

export const getProductReviewsAPI = async (productId, { page = 1, size = 20 } = {}) =>
  authorizedAxiosInstance.get(`products/${productId}/reviews`, { params: { page, size } })
