import axios from 'axios'
import { setCurrentUser, clearCurrentUser } from '~/store/user/userSlice'
import { store } from '~/store/store'

const authorizedAxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_BACKEND_URL
    ? `${import.meta.env.VITE_BACKEND_URL.replace(/\/$/, '')}/api/v1`
    : '/api/v1',
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
    const currentUser = store.getState()?.user?.currentUser

    // Only attempt refresh if user was actually logged in and it's a 401 on a non-auth route
    if (error.response?.status === 401 && !isAuthRoute && !originalRequest?._retry && currentUser) {
      originalRequest._retry = true
      if (!refreshTokenPromise) {
        refreshTokenPromise = import('~/common/apis/services/userService')
          .then(({ refreshTokenAPI }) => refreshTokenAPI())
          .then((res) => {
            const raw = res?.data !== undefined ? res.data : res
            const current = store.getState()?.user?.currentUser
            const token = raw?.accessToken || raw?.tokens?.accessToken
            const refreshToken = raw?.refreshToken || raw?.tokens?.refreshToken
            if (current && (token || raw?.user)) {
              const next = { ...current, ...raw?.user }
              if (token) next.accessToken = token
              else delete next.accessToken
              if (refreshToken) next.refreshToken = refreshToken
              else delete next.refreshToken
              store.dispatch(setCurrentUser(next))
            }
            return res
          })
          .catch((_error) => {
            store.dispatch(clearCurrentUser())
            // Only redirect to login if currently on a protected route!
            const pathname = window.location.pathname || '/'
            const isPublicRoute =
              pathname === '/' ||
              pathname === '/login' ||
              pathname === '/register' ||
              pathname === '/verify' ||
              pathname === '/live' ||
              pathname === '/products' ||
              pathname.startsWith('/product/')
            if (!isPublicRoute) {
              window.location.href = `/login?redirect=${encodeURIComponent(pathname)}`
            }
            return Promise.reject(_error)
          })
          .finally(() => {
            refreshTokenPromise = null
          })
      }
      return refreshTokenPromise.then((res) => {
        const raw = res?.data !== undefined ? res.data : res
        const refreshedToken = raw?.accessToken || raw?.tokens?.accessToken
        if (refreshedToken) {
          if (originalRequest.headers && typeof originalRequest.headers.set === 'function') {
            originalRequest.headers.set('Authorization', `Bearer ${refreshedToken}`)
          } else if (originalRequest.headers) {
            originalRequest.headers['Authorization'] = `Bearer ${refreshedToken}`
          }
        } else if (originalRequest.headers) {
          delete originalRequest.headers.Authorization
          delete originalRequest.headers['Authorization']
        }
        return authorizedAxiosInstance(originalRequest)
      })
    }

    if (error.response?.status === 401 && (isAuthRoute || originalRequest?._retry)) {
      if (currentUser) {
        store.dispatch(clearCurrentUser())
      }
    }

    return Promise.reject(error)
  }
)

export default authorizedAxiosInstance
