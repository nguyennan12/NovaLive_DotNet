import authorizedAxiosInstance from '../custom/authorizeAxios'

export const uploadSingleImageAPI = async (file, bucket = 'products') => {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('bucket', bucket)

  const res = await authorizedAxiosInstance.post('upload/media', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })

  return res.metadata ?? res.data ?? res
}

export const uploadAvatarAPI = async (file) => {
  return uploadSingleImageAPI(file, 'products')
}

export const uploadKycDocumentAPI = async (file) => {
  const formData = new FormData()
  formData.append('file', file)
  const res = await authorizedAxiosInstance.post('upload/kyc-document', formData, {
    headers: { 'Content-Type': 'multipart/form-data' }
  })
  return res.metadata ?? res.data ?? res
}

export const uploadShopLogoAPI = async (file) => {
  return uploadSingleImageAPI(file, 'products')
}