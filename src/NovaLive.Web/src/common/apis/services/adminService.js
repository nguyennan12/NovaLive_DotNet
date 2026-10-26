import authorizedAxiosInstance from '../custom/authorizeAxios'

export const getAdminShopsAPI = async ({ status, keyword, page = 1, size = 20 } = {}) => {
  const params = { page, size }
  if (status && status !== 'ALL') params.status = status
  if (keyword) params.keyword = keyword
  const res = await authorizedAxiosInstance.get('admin/shops', { params })
  return res.metadata ?? res.data ?? res
}

export const approveShopAPI = async (id, { note = '' } = {}) => {
  const res = await authorizedAxiosInstance.put(`admin/shops/${id}/approve`, { note })
  return res.metadata ?? res.data ?? res
}

export const banShopAPI = async (id, { reason = '' } = {}) => {
  const res = await authorizedAxiosInstance.put(`admin/shops/${id}/ban`, { reason })
  return res.metadata ?? res.data ?? res
}

export const getAdminPayoutsAPI = async ({ status, page = 1, size = 20 } = {}) => {
  const params = { page, size }
  if (status && status !== 'ALL') params.status = status
  const res = await authorizedAxiosInstance.get('admin/payouts', { params })
  return res.metadata ?? res.data ?? res
}

export const approvePayoutAPI = async (id, payload = {}) => {
  const res = await authorizedAxiosInstance.put(`admin/payouts/${id}/approve`, payload)
  return res.metadata ?? res.data ?? res
}

export const rejectPayoutAPI = async (id, payload = {}) => {
  const res = await authorizedAxiosInstance.put(`admin/payouts/${id}/reject`, payload)
  return res.metadata ?? res.data ?? res
}

export const getAdminDashboardReportAPI = async ({ from, to } = {}) => {
  const params = {}
  if (from) params.from = from
  if (to) params.to = to
  const res = await authorizedAxiosInstance.get('admin/reports/dashboard', { params })
  return res.metadata ?? res.data ?? res
}
