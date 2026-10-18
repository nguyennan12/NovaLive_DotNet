import axios from 'axios'
import { clearCurrentUser } from '~/store/user/userSlice'
import { store } from '~/store/store'

const authorizedAxiosInstance = axios.create({
  baseURL: '/api/v1',
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
    // Unwrap ApiResponse<T> (.data.data) or fallback to .data
    if (response?.data?.data !== undefined) {
      return response.data.data
    }
    return response.data
  },
  (error) => {
    const originalRequest = error.config
    const url = originalRequest?.url || ''
    const isAuthRoute = url.includes('auth/login') || url.includes('auth/refresh') || url.includes('auth/register')

    // Handle 401 Unauthorized for expired tokens by attempting refresh once
    if (error.response?.status === 401 && !isAuthRoute && !originalRequest?._retry) {
      originalRequest._retry = true
      if (!refreshTokenPromise) {
        refreshTokenPromise = import('~/common/apis/services/userService')
          .then(({ refreshTokenAPI }) => refreshTokenAPI())
          .then((res) => {
            const raw = res?.data !== undefined ? res.data : res
            if (raw?.accessToken) {
              const current = store.getState()?.user?.currentUser
              if (current) {
                store.dispatch({
                  type: 'user/setCurrentUser',
                  payload: { ...current, accessToken: raw.accessToken }
                })
              }
            }
            return res
          })
          .catch((_error) => {
            store.dispatch(clearCurrentUser())
            window.location.href = '/login'
            return Promise.reject(_error)
          })
          .finally(() => {
            refreshTokenPromise = null
          })
      }
      return refreshTokenPromise.then(() => authorizedAxiosInstance(originalRequest))
    }

    if (error.response?.status === 401 && (isAuthRoute || originalRequest?._retry)) {
      store.dispatch(clearCurrentUser())
    }

    return Promise.reject(error)
  }
)

export default authorizedAxiosInstance