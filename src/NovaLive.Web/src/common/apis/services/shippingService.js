import authorizedAxiosInstance from '../custom/authorizeAxios'

export const getProvincesAPI = async () => {
  return await authorizedAxiosInstance.get('shipping/provinces')
}

export const getDistrictsAPI = async (provinceId) => {
  return await authorizedAxiosInstance.get('shipping/districts', { params: { province_id: provinceId } })
}

export const getWardsAPI = async (districtId) => {
  return await authorizedAxiosInstance.get('shipping/wards', { params: { district_id: districtId } })
}
