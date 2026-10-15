import * as signalR from '@microsoft/signalr'
import { store } from '~/store/store'

const getAccessToken = () => {
  return store.getState()?.user?.currentUser?.accessToken || ''
}

export const createHubConnection = (hubPath) => {
  const url = hubPath.startsWith('http') ? hubPath : `${hubPath}`
  return new signalR.HubConnectionBuilder()
    .withUrl(url, {
      accessTokenFactory: () => getAccessToken(),
      withCredentials: true
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Information)
    .build()
}

export const createLivestreamHub = () => createHubConnection('/hubs/livestream')
export const createOrderHub = () => createHubConnection('/hubs/order')
export const createPaymentHub = () => createHubConnection('/hubs/payment')
