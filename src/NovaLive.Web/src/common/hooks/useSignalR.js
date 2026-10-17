import { useEffect, useRef, useState, useCallback } from 'react'
import { HubConnectionState } from '@microsoft/signalr'
import { createHubConnection } from '../services/signalr/signalrService'

// ─── Payment Hub Hook ────────────────────────────────────────────────────────
/**
 * Hook for listening to realtime payment events (VietQR / MoMo dynamic QR).
 */
export const usePaymentHub = ({ paymentId, onPaymentSuccess, enabled = true } = {}) => {
  const [connectionState, setConnectionState] = useState(HubConnectionState.Disconnected)
  const connectionRef = useRef(null)
  const onPaymentSuccessRef = useRef(onPaymentSuccess)

  useEffect(() => {
    onPaymentSuccessRef.current = onPaymentSuccess
  }, [onPaymentSuccess])

  useEffect(() => {
    if (!enabled || !paymentId) return

    const connection = createHubConnection('/hubs/payment')
    connectionRef.current = connection

    connection.on('PaymentSuccess', (payload) => {
      onPaymentSuccessRef.current?.(payload)
    })

    const start = async () => {
      try {
        await connection.start()
        setConnectionState(connection.state)
        await connection.invoke('JoinPayment', paymentId)
      } catch (err) {
        console.error('Failed to connect to PaymentHub:', err)
      }
    }

    start()

    return () => {
      if (connection.state === HubConnectionState.Connected) {
        connection.invoke('LeavePayment', paymentId).catch(() => {})
      }
      connection.stop().catch(() => {})
      connectionRef.current = null
      setConnectionState(HubConnectionState.Disconnected)
    }
  }, [paymentId, enabled])

  return { connectionState }
}

// ─── Order Hub Hook ──────────────────────────────────────────────────────────
/**
 * Hook for listening to realtime order notifications for Seller and Buyer.
 */
export const useOrderHub = ({
  shopId,
  customerId,
  onOrderPlaced,
  onOrderStatusChanged,
  enabled = true
} = {}) => {
  const [connectionState, setConnectionState] = useState(HubConnectionState.Disconnected)
  const connectionRef = useRef(null)
  const onOrderPlacedRef = useRef(onOrderPlaced)
  const onOrderStatusChangedRef = useRef(onOrderStatusChanged)

  useEffect(() => {
    onOrderPlacedRef.current = onOrderPlaced
    onOrderStatusChangedRef.current = onOrderStatusChanged
  }, [onOrderPlaced, onOrderStatusChanged])

  useEffect(() => {
    if (!enabled || (!shopId && !customerId)) return

    const connection = createHubConnection('/hubs/order')
    connectionRef.current = connection

    connection.on('OrderPlaced', (order) => {
      onOrderPlacedRef.current?.(order)
    })

    connection.on('OrderStatusChanged', (payload) => {
      onOrderStatusChangedRef.current?.(payload)
    })

    const start = async () => {
      try {
        await connection.start()
        setConnectionState(connection.state)
        if (shopId) {
          await connection.invoke('JoinShop', shopId)
        }
        if (customerId) {
          await connection.invoke('JoinCustomer', customerId)
        }
      } catch (err) {
        console.error('Failed to connect to OrderNotificationHub:', err)
      }
    }

    start()

    return () => {
      if (connection.state === HubConnectionState.Connected) {
        if (shopId) connection.invoke('LeaveShop', shopId).catch(() => {})
      }
      connection.stop().catch(() => {})
      connectionRef.current = null
      setConnectionState(HubConnectionState.Disconnected)
    }
  }, [shopId, customerId, enabled])

  return { connectionState }
}

// ─── Livestream Hub Hook ─────────────────────────────────────────────────────
/**
 * Hook for connecting to LivestreamHub (Chat, Reactions, Pinned Products, Viewers).
 */
export const useLivestreamHub = ({
  sessionId,
  onMessage,
  onReaction,
  onProductPinned,
  onProductUnpinned,
  onViewerCountUpdated,
  enabled = true
} = {}) => {
  const [connectionState, setConnectionState] = useState(HubConnectionState.Disconnected)
  const [viewerCount, setViewerCount] = useState(0)
  const connectionRef = useRef(null)

  const callbacksRef = useRef({
    onMessage,
    onReaction,
    onProductPinned,
    onProductUnpinned,
    onViewerCountUpdated
  })

  useEffect(() => {
    callbacksRef.current = {
      onMessage,
      onReaction,
      onProductPinned,
      onProductUnpinned,
      onViewerCountUpdated
    }
  }, [onMessage, onReaction, onProductPinned, onProductUnpinned, onViewerCountUpdated])

  useEffect(() => {
    if (!enabled || !sessionId) return

    const connection = createHubConnection('/hubs/livestream')
    connectionRef.current = connection

    connection.on('ReceiveMessage', (comment) => {
      callbacksRef.current.onMessage?.(comment)
    })

    connection.on('ReceiveReaction', (type) => {
      callbacksRef.current.onReaction?.(type)
    })

    connection.on('ProductPinned', (product) => {
      callbacksRef.current.onProductPinned?.(product)
    })

    connection.on('ProductUnpinned', (skuId) => {
      callbacksRef.current.onProductUnpinned?.(skuId)
    })

    connection.on('ViewerCountUpdated', (count) => {
      setViewerCount(count)
      callbacksRef.current.onViewerCountUpdated?.(count)
    })

    const start = async () => {
      try {
        await connection.start()
        setConnectionState(connection.state)
        const initialCount = await connection.invoke('JoinLiveSession', sessionId)
        if (typeof initialCount === 'number') {
          setViewerCount(initialCount)
        }
      } catch (err) {
        console.error('Failed to connect to LivestreamHub:', err)
      }
    }

    start()

    return () => {
      if (connection.state === HubConnectionState.Connected) {
        connection.invoke('LeaveLiveSession', sessionId).catch(() => {})
      }
      connection.stop().catch(() => {})
      connectionRef.current = null
      setConnectionState(HubConnectionState.Disconnected)
    }
  }, [sessionId, enabled])

  const sendMessage = useCallback(async (content) => {
    if (connectionRef.current?.state === HubConnectionState.Connected) {
      return await connectionRef.current.invoke('SendMessage', content)
    }
    throw new Error('SignalR Livestream connection is not active')
  }, [])

  const sendReaction = useCallback((type) => {
    if (connectionRef.current?.state === HubConnectionState.Connected) {
      connectionRef.current.send('SendReaction', type)
    }
  }, [])

  return {
    connectionState,
    viewerCount,
    sendMessage,
    sendReaction,
    isConnected: connectionState === HubConnectionState.Connected
  }
}
