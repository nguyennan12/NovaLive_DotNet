import authorizedAxiosInstance from '../custom/authorizeAxios'

export const RETURN_REASONS = [
  { value: 'WrongItem', label: 'Giao sai sản phẩm / sai mẫu mã' },
  { value: 'Defective', label: 'Sản phẩm bị lỗi / không hoạt động' },
  { value: 'DamagedInShipping', label: 'Hàng bị bể vỡ, hư hỏng do vận chuyển' },
  { value: 'NotAsDescribed', label: 'Hàng không đúng mô tả / quảng cáo' },
  { value: 'ChangeOfMind', label: 'Thay đổi ý định mua hàng' }
]

export const requestReturnAPI = async (payload) => {
  const res = await authorizedAxiosInstance.post('returns', payload)
  return res.metadata ?? res.data ?? res
}

export const getMyReturnsAPI = async ({ status, page = 1, size = 20 } = {}) => {
  const params = { page, size }
  if (status) params.status = status
  const res = await authorizedAxiosInstance.get('returns', { params })
  return res.metadata ?? res.data ?? res
}

export const getReturnDetailAPI = async (id) => {
  const res = await authorizedAxiosInstance.get(`returns/${id}`)
  return res.metadata ?? res.data ?? res
}
