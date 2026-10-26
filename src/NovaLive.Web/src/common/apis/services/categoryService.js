import authorizedAxiosInstance from '../custom/authorizeAxios'

export const normalizeCategory = (cat) => {
  if (!cat) return cat
  return {
    ...cat,
    _id: cat.id || cat._id,
    id: cat.id || cat._id,
    cat_name: cat.name || cat.cat_name,
    name: cat.name || cat.cat_name,
    cat_slug: cat.slug || cat.cat_slug,
    slug: cat.slug || cat.cat_slug,
    children: (cat.children || []).map(normalizeCategory)
  }
}

export const getAllCategoryAPI = async (params = {}) => {
  const res = await authorizedAxiosInstance.get('categories', { params })
  const raw = res?.data !== undefined ? res.data : res
  const list = Array.isArray(raw) ? raw : (raw?.items || [])
  return list.map(normalizeCategory)
}

export const getCategoryByIdAPI = async (id) => {
  const res = await authorizedAxiosInstance.get(`categories/${id}`)
  const raw = res?.data !== undefined ? res.data : res
  return normalizeCategory(raw)
}

export const createCategoryAPI = async (data) => {
  const res = await authorizedAxiosInstance.post('admin/categories', data)
  return res?.data !== undefined ? res.data : res
}

export const updateCategoryAPI = async (id, data) => {
  const res = await authorizedAxiosInstance.put(`admin/categories/${id}`, data)
  return res?.data !== undefined ? res.data : res
}

export const deleteCategoryAPI = async (id) => {
  const res = await authorizedAxiosInstance.delete(`admin/categories/${id}`)
  return res?.data !== undefined ? res.data : res
}

export const getAttributeByCategorySlugAPI = async () => {
  // Backend .NET embeds category attributes directly into categories / products;
  // returns an empty array safely to prevent 404 crashes.
  return []
}

export const addAttributeToCategoryAPI = async () => {
  return []
}

export const createCategoryBulkAPI = async () => {
  return []
}