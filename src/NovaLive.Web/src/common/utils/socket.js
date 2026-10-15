import { io } from 'socket.io-client'

let socket = null

export const getSocket = () => {
  if (!socket) {
    // Kết nối qua Vite proxy (/socket.io → backend:3031)
    // Không cần URL tuyệt đối, tránh CORS trong Docker
    socket = io({
      path: '/socket.io',
      withCredentials: true,
      autoConnect: false,
      transports: ['websocket', 'polling']
    })
  }
  return socket
}

export const connectSocket = (userId) => {
  const s = getSocket()
  if (!s.connected) {
    s.connect()
    s.once('connect', () => s.emit('join-room', userId))
  } else {
    s.emit('join-room', userId)
  }
}
