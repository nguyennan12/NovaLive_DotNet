import authorizedAxiosInstance from '../custom/authorizeAxios'

// 1. Shops Management
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

// 2. Payouts Management
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

// 3. Platform Dashboard Report
export const getAdminDashboardReportAPI = async ({ from, to } = {}) => {
  const params = {}
  if (from) params.from = from
  if (to) params.to = to
  const res = await authorizedAxiosInstance.get('admin/reports/dashboard', { params })
  return res.metadata ?? res.data ?? res
}

// 4. Categories Management
export const getAdminCategoriesAPI = async () => {
  const res = await authorizedAxiosInstance.get('categories')
  return res.metadata ?? res.data ?? res
}

export const createAdminCategoryAPI = async (data) => {
  const res = await authorizedAxiosInstance.post('admin/categories', data)
  return res.metadata ?? res.data ?? res
}

export const updateAdminCategoryAPI = async (id, data) => {
  const res = await authorizedAxiosInstance.put(`admin/categories/${id}`, data)
  return res.metadata ?? res.data ?? res
}

export const deleteAdminCategoryAPI = async (id) => {
  const res = await authorizedAxiosInstance.delete(`admin/categories/${id}`)
  return res.metadata ?? res.data ?? res
}

// 5. Livestreams Active Moderation
export const getAdminActiveLivestreamsAPI = async ({ page = 1, size = 20 } = {}) => {
  const res = await authorizedAxiosInstance.get('livestreams/active', { params: { page, size } })
  return res.metadata ?? res.data ?? res
}

// 6. Flash Sales Management
export const getAdminFlashSalesAPI = async () => {
  const res = await authorizedAxiosInstance.get('flash-sales/active')
  return res.metadata ?? res.data ?? res
}

export const createAdminFlashSaleAPI = async (data) => {
  const res = await authorizedAxiosInstance.post('admin/flash-sales', data)
  return res.metadata ?? res.data ?? res
}

export const approveFlashSaleItemAPI = async (itemId) => {
  const res = await authorizedAxiosInstance.put(`admin/flash-sales/items/${itemId}/approve`)
  return res.metadata ?? res.data ?? res
}

export const rejectFlashSaleItemAPI = async (itemId, payload = {}) => {
  const res = await authorizedAxiosInstance.put(`admin/flash-sales/items/${itemId}/reject`, payload)
  return res.metadata ?? res.data ?? res
}

// 7. Platform Discounts Management
export const getAdminDiscountsAPI = async ({ status, page = 1, size = 20 } = {}) => {
  const params = { page, size }
  if (status && status !== 'ALL') params.status = status
  const res = await authorizedAxiosInstance.get('admin/discounts', { params })
  return res.metadata ?? res.data ?? res
}

export const createAdminDiscountAPI = async (data) => {
  const res = await authorizedAxiosInstance.post('admin/discounts', data)
  return res.metadata ?? res.data ?? res
}

export const updateAdminDiscountAPI = async (id, data) => {
  const res = await authorizedAxiosInstance.put(`admin/discounts/${id}`, data)
  return res.metadata ?? res.data ?? res
}

export const deleteAdminDiscountAPI = async (id) => {
  const res = await authorizedAxiosInstance.delete(`admin/discounts/${id}`)
  return res.metadata ?? res.data ?? res
}

// 8. Reviews Moderation
export const hideReviewAPI = async (reviewId, payload = {}) => {
  const res = await authorizedAxiosInstance.put(`admin/reviews/${reviewId}/hide`, payload)
  return res.metadata ?? res.data ?? res
}
