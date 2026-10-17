import authorizedAxiosInstance from '../custom/authorizeAxios'

const normalizeAddress = (addr) => {
  if (!addr) return addr
  const id = addr.id || addr._id
  const name = addr.recipientName || addr.name_user || ''
  const phone = addr.phone || addr.phone_user || ''
  const detail = addr.detailAddress || addr.address_user || ''
  const fullAddress = [detail, addr.wardName, addr.districtName, addr.provinceName].filter(Boolean).join(', ')

  return {
    ...addr,
    _id: id,
    id,
    recipientName: name,
    name_user: name,
    phone,
    phone_user: phone,
    detailAddress: detail,
    address_user: fullAddress || detail,
    isDefault: addr.isDefault ?? addr.is_default ?? false,
    is_default: addr.isDefault ?? addr.is_default ?? false
  }
}

export const getMyAddressesAPI = async () => {
  const res = await authorizedAxiosInstance.get('users/me/addresses')
  const list = Array.isArray(res) ? res : (res?.data || res?.items || [])
  return list.map(normalizeAddress)
}

export const createAddressAPI = async (data) => {
  const payload = {
    recipientName: data.recipientName || data.name_user,
    phone: data.phone || data.phone_user,
    provinceId: data.provinceId ?? data.province_id ?? null,
    provinceName: data.provinceName || data.province_name || '',
    districtId: data.districtId ?? data.district_id ?? null,
    districtName: data.districtName || data.district_name || '',
    wardCode: data.wardCode || data.ward_code || null,
    wardName: data.wardName || data.ward_name || '',
    detailAddress: data.detailAddress || data.address_user || '',
    isDefault: data.isDefault ?? data.is_default ?? false
  }
  const res = await authorizedAxiosInstance.post('users/me/addresses', payload)
  return normalizeAddress(res)
}

export const updateAddressAPI = async (addressId, data) => {
  const payload = {
    recipientName: data.recipientName || data.name_user,
    phone: data.phone || data.phone_user,
    provinceId: data.provinceId ?? data.province_id ?? null,
    provinceName: data.provinceName || data.province_name || '',
    districtId: data.districtId ?? data.district_id ?? null,
    districtName: data.districtName || data.district_name || '',
    wardCode: data.wardCode || data.ward_code || null,
    wardName: data.wardName || data.ward_name || '',
    detailAddress: data.detailAddress || data.address_user || '',
    isDefault: data.isDefault ?? data.is_default ?? false
  }
  const res = await authorizedAxiosInstance.put(`users/me/addresses/${addressId}`, payload)
  return normalizeAddress(res)
}

export const deleteAddressAPI = async (addressId) => {
  return await authorizedAxiosInstance.delete(`users/me/addresses/${addressId}`)
}

export const setDefaultAddressAPI = async (addressId) => {
  return await authorizedAxiosInstance.put(`users/me/addresses/${addressId}/default`)
}

// ─── Backward compatibility aliases ──────────────────────────────────────────
export const getUserAddressAPI = async (addressId) => {
  const list = await getMyAddressesAPI()
  if (addressId) {
    const found = list.find((a) => a.id === addressId || a._id === addressId)
    if (found) return found
  }
  return list.find((a) => a.isDefault) || list[0] || null
}

export const getAllUserAddressAPI = async () => {
  return await getMyAddressesAPI()
}