import { HubConnectionBuilder, HttpTransportType, LogLevel } from '@microsoft/signalr'
import { store } from '~/store/store'

export const getSignalRBaseUrl = () => {
  const envUrl = import.meta.env.VITE_SIGNALR_URL || import.meta.env.VITE_BACKEND_URL || import.meta.env.VITE_SOCKET_URL || 'http://localhost:5001'
  return envUrl.replace(/\/$/, '')
}

/**
 * Creates a SignalR HubConnection with automatic token attachment and retry policy.
 * @param {string} hubPath - Relative hub path, e.g. '/hubs/livestream', '/hubs/order', '/hubs/payment'
 * @param {object} customOptions - Extra options for withUrl
 */
export const createHubConnection = (hubPath, customOptions = {}) => {
  const baseUrl = getSignalRBaseUrl()
  const path = hubPath.startsWith('/') ? hubPath : `/${hubPath}`
  const fullUrl = `${baseUrl}${path}`

  return new HubConnectionBuilder()
    .withUrl(fullUrl, {
      accessTokenFactory: () => {
        const token = store.getState()?.user?.currentUser?.accessToken
        return token || ''
      },
      withCredentials: true,
      transport: HttpTransportType.WebSockets | HttpTransportType.LongPolling,
      ...customOptions
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.None)
    .build()
}
