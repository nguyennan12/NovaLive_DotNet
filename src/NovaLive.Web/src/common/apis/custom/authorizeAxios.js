// authorizeAxios.js — adapted for NovaLive .NET 9 Clean Architecture
import axios from 'axios'
import { clearCurrentUser } from '~/store/user/userSlice'
import { store } from '~/store/store'

const backendUrl = (import.meta.env.VITE_BACKEND_URL || import.meta.env.VITE_SOCKET_URL || '').replace(/\/$/, '')

const authorizedAxiosInstance = axios.create({
  baseURL: backendUrl ? `${backendUrl}/api/v1` : '/api/v1',
  timeout: 1000 * 60 * 10,
  withCredentials: true,
  headers: {
    'X-Requested-With': 'XMLHttpRequest'
  }
})

authorizedAxiosInstance.interceptors.request.use((config) => {
  const user = store.getState()?.user?.currentUser
  if (user?.accessToken) {
    config.headers['Authorization'] = `Bearer ${user.accessToken}`
  }
  return config
}, (error) => Promise.reject(error))

let refreshTokenPromise = null

authorizedAxiosInstance.interceptors.response.use(
  (response) => {
    const resData = response.data
    // Bridge .NET ApiResponse<T> ({ success, message, data }) with legacy ({ metadata })
    if (resData && typeof resData === 'object') {
      if (resData.data !== undefined && resData.metadata === undefined) {
        resData.metadata = resData.data
      }
    }
    return resData
  },
  (error) => {
    const originalRequest = error.config
    const isAuthRoute = originalRequest?.url?.includes('/auth/login') ||
                        originalRequest?.url?.includes('/auth/refresh') ||
                        originalRequest?.url?.includes('/auth/register')

    // Handle 401 or 410 for token expiration
    if ((error.response?.status === 401 || error.response?.status === 410) && !isAuthRoute && !originalRequest?._retry) {
      originalRequest._retry = true
      if (!refreshTokenPromise) {
        refreshTokenPromise = import('~/common/apis/services/userService')
          .then(({ refreshTokenAPI }) => refreshTokenAPI())
          .catch((_error) => {
            store.dispatch(clearCurrentUser())
            window.location.href = '/login'
            return Promise.reject(_error)
          })
          .finally(() => { refreshTokenPromise = null })
      }
      return refreshTokenPromise.then(() => authorizedAxiosInstance(originalRequest))
    }

    if (error.response?.status === 401 && isAuthRoute) {
      store.dispatch(clearCurrentUser())
    }

    return Promise.reject(error)
  }
)

export default authorizedAxiosInstance