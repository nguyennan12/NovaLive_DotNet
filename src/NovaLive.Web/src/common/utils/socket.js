// socket.js — Migrated from Socket.IO to SignalR
export { createHubConnection } from '../services/signalr/signalrService'
export { usePaymentHub, useOrderHub, useLivestreamHub } from '../hooks/useSignalR'
