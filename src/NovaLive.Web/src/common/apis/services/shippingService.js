import axios from 'axios'

// Master-data endpoints của GHN không có /v2 trong path
const GHN_BASE = (import.meta.env.VITE_GHN_API_URL || '').replace(/\/v2$/, '')

const ghnAxios = axios.create({
  baseURL: GHN_BASE,
  headers: {
    Token: import.meta.env.VITE_GHN_API_TOKEN,
    'Content-Type': 'application/json'
  }
})

export const getProvincesAPI = async () => {
  const res = await ghnAxios.get('/master-data/province')
  return res.data.data ?? []
}

export const getDistrictsAPI = async (provinceId) => {
  const res = await ghnAxios.get('/master-data/district', { params: { province_id: provinceId } })
  return res.data.data ?? []
}

export const getWardsAPI = async (districtId) => {
  const res = await ghnAxios.get('/master-data/ward', { params: { district_id: districtId } })
  return res.data.data ?? []
}
