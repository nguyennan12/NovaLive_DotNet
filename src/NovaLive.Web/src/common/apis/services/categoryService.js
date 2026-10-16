import authorizedAxiosInstance from '../custom/authorizeAxios'

export const createCategoryAPI = async (data) => {
  const res = await authorizedAxiosInstance.post('categories', data)
  return res.metadata ?? res.data ?? res
}

export const createCategoryBulkAPI = async (data) => {
  const res = await authorizedAxiosInstance.post('categories/bulk', data)
  return res.metadata ?? res.data ?? res
}

export const getAllCategoryAPI = async (params = {}) => {
  const res = await authorizedAxiosInstance.get('categories', { params })
  return res.metadata ?? res.data ?? res
}

export const getCategoryByIdAPI = async (id) => {
  const res = await authorizedAxiosInstance.get(`categories/${id}`)
  return res.metadata ?? res.data ?? res
}

export const getAttributeByCategorySlugAPI = async (slugArr = []) => {
  const slug = Array.isArray(slugArr) ? slugArr.join(',') : slugArr
  const res = await authorizedAxiosInstance.get('categories/attribute', { params: { slug } })
  return res.metadata ?? res.data ?? res
}

export const addAttributeToCategoryAPI = async ({ categoryId, data }) => {
  const res = await authorizedAxiosInstance.post(`categories/${categoryId}/attribute`, data)
  return res.metadata ?? res.data ?? res
}