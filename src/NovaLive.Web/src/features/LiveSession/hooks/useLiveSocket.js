import { useCallback, useEffect, useRef, useState } from 'react'
import { connectSocket, getSocket } from '~/common/utils/socket'

const normalizePinned = (product) => {
  if (!product) return null
  return {
    ...product,
    live_price: product.live_price ?? product.skus?.[0]?.live_price ?? 0
  }
}

export const useLiveSocket = ({
  liveCode,
  userId,
  userName,
  avatar,
  liveProducts = [],
  initialViewers = 0,
  initialLikes = 0
}) => {
  //state thao tác trên live
  const [viewers, setViewers] = useState(initialViewers)
  const [likes, setLikes] = useState(initialLikes)
  const [comments, setComments] = useState([])
  const [pinnedProduct, setPinnedProduct] = useState(() => normalizePinned(liveProducts.find(p => p.is_featured) ?? null))
  const [commentError, setCommentError] = useState(null)
  //state phiên live trc đó để check xem có lướt k
  const [prevLiveCode, setPrevLiveCode] = useState(liveCode)


  const liveCodeRef = useRef(liveCode)
  const userRef = useRef({ userId, userName, avatar })
  const liveProductsRef = useRef(liveProducts)

  useEffect(() => { liveCodeRef.current = liveCode }, [liveCode])
  useEffect(() => { userRef.current = { userId, userName, avatar } }, [userId, userName, avatar])
  useEffect(() => { liveProductsRef.current = liveProducts }, [liveProducts])

  // Reset khi đổi sang live khác
  if (liveCode !== prevLiveCode) {
    setPrevLiveCode(liveCode)
    setViewers(initialViewers)
    setLikes(initialLikes)
    setComments([])
    setCommentError(null)

    const initialPinned = normalizePinned(liveProducts.find(p => p.is_featured) ?? null)
    setPinnedProduct(initialPinned)
  }

  //logic socket
  useEffect(() => {
    if (!liveCode || !userId) return
    //user join live
    connectSocket(userId)
    const socket = getSocket()
    socket.emit('join-live', liveCode)

    const onViewers = (count) => setViewers(Number(count))
    const onLikes = ({ total }) => setLikes(Number(total))
    const onComment = (comment) =>
      setComments(prev => {
        const next = [...prev, comment]
        return next.length > 100 ? next.slice(-100) : next
      })
    const onPinned = (socketProduct) => {
      const full = liveProductsRef.current.find(
        p => String(p.productId) === String(socketProduct.productId)
      )
      setPinnedProduct(normalizePinned(full ?? socketProduct))
    }
    const onCommentError = (msg) => {
      setCommentError(msg)
      setTimeout(() => setCommentError(null), 3000)
    }

    socket.on('UPDATE_VIEWERS', onViewers)
    socket.on('UPDATE_LIKES', onLikes)
    socket.on('NEW_COMMENT', onComment)
    socket.on('PRODUCT_PINNED', onPinned)
    socket.on('COMMENT_ERROR', onCommentError)
    //khi lướt sang live khác sẽ vô return(đầu tiên là leave và set lại stat)
    return () => {
      socket.emit('leave-live', liveCode)
      socket.off('UPDATE_VIEWERS', onViewers)
      socket.off('UPDATE_LIKES', onLikes)
      socket.off('NEW_COMMENT', onComment)
      socket.off('PRODUCT_PINNED', onPinned)
      socket.off('COMMENT_ERROR', onCommentError)
    }
  }, [liveCode, userId])

  const sendLike = useCallback(() => {
    setLikes(prev => prev + 1) // optimistic update
    getSocket().emit('send-like', liveCodeRef.current)
  }, [])

  const sendComment = useCallback((message) => {
    const { userId, userName, avatar } = userRef.current
    getSocket().emit('send-comment', {
      liveCode: liveCodeRef.current,
      userName,
      userId,
      avatar,
      message
    })
  }, [])

  return { viewers, likes, comments, pinnedProduct, sendLike, sendComment, commentError }
}
