import { useCallback, useEffect, useRef, useState } from 'react'
import { createHubConnection } from '~/common/services/signalr/signalrService'
import { HubConnectionState } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'

const normalizePinned = (product) => {
  if (!product) return null
  return {
    ...product,
    productId: product.productId ?? product.spuId ?? product.spu_id,
    name: product.name ?? product.spuName,
    thumb: product.thumb ?? product.thumbnailUrl,
    live_price: product.live_price ?? product.effectivePrice ?? product.flashPrice ?? product.sellPrice ?? product.price ?? product.skus?.[0]?.live_price ?? 0,
    original_price: product.original_price ?? product.originalPrice,
    is_featured: product.is_featured ?? product.isPinned ?? false,
    skus: product.skus?.length ? product.skus : [{
      skuId: product.skuId,
      sku_name: product.skuCode,
      live_price: product.effectivePrice ?? product.flashPrice ?? product.sellPrice,
      original_price: product.originalPrice
    }]
  }
}

export const useLiveSocket = ({
  liveCode, // sessionId
  liveProducts = [],
  initialViewers = 0,
  initialLikes = 0
}) => {
  const queryClient = useQueryClient()
  const [viewers, setViewers] = useState(initialViewers)
  const [likes, setLikes] = useState(initialLikes)
  const [comments, setComments] = useState([])
  const [products, setProducts] = useState(() => liveProducts.map(normalizePinned))
  const [commentError, setCommentError] = useState(null)

  const connectionRef = useRef(null)
  const liveProductsRef = useRef(liveProducts)

  useEffect(() => { liveProductsRef.current = liveProducts }, [liveProducts])
  // The initial list arrives asynchronously from the session-detail query.
  // eslint-disable-next-line react-hooks/set-state-in-effect
  useEffect(() => { setProducts(liveProducts.map(normalizePinned)) }, [liveProducts])

  // SignalR connection
  useEffect(() => {
    if (!liveCode) return

    const connection = createHubConnection('/hubs/livestream')
    connectionRef.current = connection

    const onViewers = (event) => {
      const count = Number(event?.viewerCount ?? event?.ViewerCount ?? event)
      if (!Number.isFinite(count)) return
      setViewers(count)
      queryClient.setQueryData(['live-session-products', liveCode], current =>
        current ? { ...current, viewerCount: count, live_viewer_count: count } : current)
    }
    const onReaction = (batch) => {
      const count = Number(batch?.totalBatchCount ?? batch?.TotalBatchCount ?? 1)
      setLikes(prev => prev + (Number.isFinite(count) ? count : 1))
    }
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
      const normalized = normalizePinned(full ?? product)
      setProducts(current => {
        const without = current.filter(item => String(item.skuId) !== String(normalized.skuId))
        return [...without, normalized]
      })
      queryClient.setQueryData(['live-session-products', liveCode], current => {
        if (!current) return current
        const existing = current.live_products ?? []
        const without = existing.filter(item => String(item.skuId) !== String(normalized.skuId))
        return {
          ...current,
          activePinnedProduct: normalized,
          live_products: [...without.map(item => ({ ...item, isPinned: false, is_featured: false })), normalized]
        }
      })
    }
    const onUnpinned = (event) => {
      const skuId = event?.skuId ?? event?.SkuId
      setProducts(current => current.filter(item => String(item.skuId) !== String(skuId)))
      queryClient.setQueryData(['live-session-products', liveCode], current => current ? {
        ...current,
        activePinnedProduct: String(current.activePinnedProduct?.skuId) === String(skuId)
          ? null : current.activePinnedProduct,
        live_products: (current.live_products ?? []).filter(item => String(item.skuId) !== String(skuId))
      } : current)
    }

    connection.on('ViewerCountUpdated', onViewers)
    connection.on('ReceiveReaction', onReaction)
    connection.on('ReceiveMessage', onComment)
    connection.on('ProductPinned', onPinned)
    connection.on('ProductUnpinned', onUnpinned)
    connection.onreconnected(async () => {
      try {
        const count = await connection.invoke('JoinLiveSession', liveCode)
        if (typeof count === 'number') setViewers(count)
        setCommentError(null)
        queryClient.invalidateQueries({ queryKey: ['live-session-products', liveCode] })
      } catch {
        setCommentError('Không thể kết nối lại phòng live')
      }
    })

    const start = async () => {
      try {
        await connection.start()
        const count = await connection.invoke('JoinLiveSession', liveCode)
        if (typeof count === 'number') setViewers(count)
      } catch {
        setCommentError('Không kết nối được tới phòng live')
      }
    }

    start()

    return () => {
      if (connection.state === HubConnectionState.Connected) {
        connection.invoke('LeaveLiveSession', liveCode)
          .catch(() => {})
          .finally(() => connection.stop().catch(() => {}))
      } else {
        connection.stop().catch(() => {})
      }
      connectionRef.current = null
    }
  }, [liveCode, queryClient])

  const sendLike = useCallback(() => {
    if (connectionRef.current?.state === HubConnectionState.Connected) {
      connectionRef.current.send('SendReaction', 'heart')
    }
  }, [])

  const sendComment = useCallback(async (message) => {
    if (!message?.trim()) return
    if (connectionRef.current?.state === HubConnectionState.Connected) {
      try {
        await connectionRef.current.invoke('SendMessage', message.trim())
      } catch (err) {
        setCommentError(err.message || 'Không thể gửi bình luận')
        setTimeout(() => setCommentError(null), 3000)
      }
    } else {
      setCommentError('Chưa kết nối được tới phòng live')
      setTimeout(() => setCommentError(null), 3000)
    }
  }, [])

  return { viewers, likes, comments, products, sendLike, sendComment, commentError }
}
