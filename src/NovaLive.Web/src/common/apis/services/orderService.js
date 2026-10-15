import authorizedAxiosInstance from '../custom/authorizeAxios'

export const checkoutAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('order/checkout', data)
  return response.metadata
}
export const OrderByUserAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('order', data)
  return response.metadata
}
export const createVNPayUrlAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('payment/create_url', data)
  return response.metadata
}
export const confirmCodOTPAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('payment/cod', data)
  return response.metadata
}
export const resendCodOTPAPI = async (data) => {
  const response = await authorizedAxiosInstance.post('access/sendmail', data)
  return response.metadata
}
export const verifyVNPayReturnAPI = async (params) => {
  const response = await authorizedAxiosInstance.get('payment/vnpay_return', { params })
  return response.metadata
}
export const getMyOrdersAPI = async (filters) => {
  const params = new URLSearchParams(filters)
  const response = await authorizedAxiosInstance.get(`order/my-orders?${params}`)
  return response.metadata
}
export const getOrderDetailAPI = async (orderId) => {
  const response = await authorizedAxiosInstance.get(`order/my-orders/${orderId}`)
  return response.metadata
}
export const cancelOrderAPI = async (orderId) => {
  const response = await authorizedAxiosInstance.patch(`order/${orderId}/cancel`)
  return response.metadata
}