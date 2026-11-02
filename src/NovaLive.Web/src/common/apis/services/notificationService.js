import authorizedAxiosInstance from '../custom/authorizeAxios'

export const getNotificationsAPI = async (params = {}) => {
  const response = await authorizedAxiosInstance.get('notifications', { params })
  return response?.data !== undefined ? response.data : response
}

export const markNotificationReadAPI = async (id) => {
  const response = await authorizedAxiosInstance.put(`notifications/${id}/read`)
  return response?.data !== undefined ? response.data : response
}

export const markAllNotificationsReadAPI = async () => {
  const response = await authorizedAxiosInstance.put('notifications/read-all')
  return response?.data !== undefined ? response.data : response
}
