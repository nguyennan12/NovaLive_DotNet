import { useCallback, useEffect, useRef, useState } from 'react'
import { createHubConnection } from '~/common/services/signalr/signalrService'
import { HubConnectionState } from '@microsoft/signalr'

const normalizePinned = (product) => {
  if (!product) return null
  return {
    ...product,
    live_price: product.live_price ?? product.flashPrice ?? product.price ?? product.skus?.[0]?.live_price ?? 0
  }
}

export const useLiveSocket = ({
  liveCode, // sessionId
  userId,
  userName,
  avatar,
  liveProducts = [],
  initialViewers = 0,
  initialLikes = 0
}) => {
  const [viewers, setViewers] = useState(initialViewers)
  const [likes, setLikes] = useState(initialLikes)
  const [comments, setComments] = useState([])
  const [pinnedProduct, setPinnedProduct] = useState(() => normalizePinned(liveProducts.find(p => p.is_featured) ?? null))
  const [commentError, setCommentError] = useState(null)
  const [prevLiveCode, setPrevLiveCode] = useState(liveCode)

  const connectionRef = useRef(null)
  const liveCodeRef = useRef(liveCode)
  const userRef = useRef({ userId, userName, avatar })
  const liveProductsRef = useRef(liveProducts)

  useEffect(() => { liveCodeRef.current = liveCode }, [liveCode])
  useEffect(() => { userRef.current = { userId, userName, avatar } }, [userId, userName, avatar])
  useEffect(() => { liveProductsRef.current = liveProducts }, [liveProducts])

  // Reset state when switching to another live session
  if (liveCode !== prevLiveCode) {
    setPrevLiveCode(liveCode)
    setViewers(initialViewers)
    setLikes(initialLikes)
    setComments([])
    setCommentError(null)

    const initialPinned = normalizePinned(liveProducts.find(p => p.is_featured) ?? null)
    setPinnedProduct(initialPinned)
  }

  // SignalR connection
  useEffect(() => {
    if (!liveCode) return

    const connection = createHubConnection('/hubs/livestream')
    connectionRef.current = connection

    const onViewers = (count) => setViewers(Number(count))
    const onReaction = () => setLikes(prev => prev + 1)
    const onComment = (comment) => {
      setComments(prev => {
        const next = [...prev, {
          _id: comment.id || comment._id || Math.random().toString(),
          userName: comment.displayName || comment.userName || 'Người xem',
          message: comment.content || comment.message || '',
          avatar: comment.avatar || '',
          createdAt: comment.createdAt || new Date().toISOString()
        }]
        return next.length > 100 ? next.slice(-100) : next
      })
    }
    const onPinned = (product) => {
      const full = liveProductsRef.current.find(
        p => String(p.skuId || p.productId) === String(product.skuId || product.productId)
      )
      setPinnedProduct(normalizePinned(full ?? product))
    }
    const onUnpinned = () => {
      setPinnedProduct(null)
    }

    connection.on('ViewerCountUpdated', onViewers)
    connection.on('ReceiveReaction', onReaction)
    connection.on('ReceiveMessage', onComment)
    connection.on('ProductPinned', onPinned)
    connection.on('ProductUnpinned', onUnpinned)

    const start = async () => {
      try {
        await connection.start()
        const count = await connection.invoke('JoinLiveSession', liveCode)
        if (typeof count === 'number') setViewers(count)
      } catch (err) {
        console.warn('SignalR Livestream connection failed, falling back:', err)
      }
    }

    start()

    return () => {
      if (connection.state === HubConnectionState.Connected) {
        connection.invoke('LeaveLiveSession', liveCode).catch(() => {})
      }
      connection.stop().catch(() => {})
      connectionRef.current = null
    }
  }, [liveCode])

  const sendLike = useCallback(() => {
    setLikes(prev => prev + 1)
    if (connectionRef.current?.state === HubConnectionState.Connected) {
      connectionRef.current.send('SendReaction', 'heart')
    }
  }, [])

  const sendComment = useCallback(async (message) => {
    if (!message?.trim()) return
    const { userName } = userRef.current
    // Optimistic UI append
    setComments(prev => [
      ...prev,
      {
        _id: Math.random().toString(),
        userName: userName || 'Bạn',
        message,
        createdAt: new Date().toISOString()
      }
    ])

    if (connectionRef.current?.state === HubConnectionState.Connected) {
      try {
        await connectionRef.current.invoke('SendMessage', message)
      } catch (err) {
        setCommentError(err.message || 'Không thể gửi bình luận')
        setTimeout(() => setCommentError(null), 3000)
      }
    }
  }, [])

  return { viewers, likes, comments, pinnedProduct, sendLike, sendComment, commentError }
}
