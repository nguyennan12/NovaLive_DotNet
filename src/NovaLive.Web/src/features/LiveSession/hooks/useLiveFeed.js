/* eslint-disable react-hooks/exhaustive-deps */
import { useCallback, useEffect, useRef, useState } from 'react'
import { getActiveSessionsAPI } from '~/common/apis/services/liveService'

export const useLiveFeed = () => {
  const [lives, setLives] = useState([])
  const [currentIndex, setCurrentIndex] = useState(0)
  const [loading, setLoading] = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const nextCursorRef = useRef(null)
  const hasMoreRef = useRef(true)

  const fetchInitial = useCallback(async (isPolling = false) => {
    try {
      const res = await getActiveSessionsAPI({ limit: 5 })
      const fetched = res.lives || []
      setLives(prev => {
        if (isPolling) {
          // If we had no lives and now we have, or list length changed:
          if (prev.length === 0 && fetched.length > 0) return fetched
          return prev
        }
        return fetched
      })
      nextCursorRef.current = res.nextCursor
      hasMoreRef.current = res.hasMore
    } catch {
      // Handle error
    } finally {
      if (!isPolling) setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchInitial(false)
  }, [fetchInitial])

  // Poll for new live sessions when list is empty
  useEffect(() => {
    let timer = null
    if (lives.length === 0) {
      timer = setInterval(() => {
        fetchInitial(true)
      }, 3000)
    }
    return () => {
      if (timer) clearInterval(timer)
    }
  }, [lives.length, fetchInitial])

  const fetchMore = useCallback(async () => {
    if (loadingMore || !hasMoreRef.current) return
    setLoadingMore(true)
    try {
      const res = await getActiveSessionsAPI({ limit: 5, cursor: nextCursorRef.current })
      setLives(prev => [...prev, ...(res.lives || [])])
      nextCursorRef.current = res.nextCursor
      hasMoreRef.current = res.hasMore
    } catch {
      // Handle error
    } finally {
      setLoadingMore(false)
    }
  }, [loadingMore])

  const goNext = () => {
    if (currentIndex >= lives.length - 1) return
    const nextIndex = currentIndex + 1
    setCurrentIndex(nextIndex)
    if (nextIndex >= lives.length - 2) fetchMore()
  }

  const goPrev = () => {
    if (currentIndex <= 0) return
    setCurrentIndex(prev => prev - 1)
  }

  const selectLiveById = (id) => {
    const idx = lives.findIndex(l => (l.id || l._id) === id)
    if (idx !== -1) {
      setCurrentIndex(idx)
    }
  }

  const touchStartY = useRef(null)

  const handleTouchStart = (e) => {
    touchStartY.current = e.touches[0].clientY
  }

  const handleTouchEnd = (e) => {
    if (touchStartY.current === null) return
    const diff = touchStartY.current - e.changedTouches[0].clientY
    if (Math.abs(diff) < 50) return
    diff > 0 ? goNext() : goPrev()
    touchStartY.current = null
  }

  const isScrolling = useRef(false)
  const handleWheel = useCallback((e) => {
    if (isScrolling.current) return

    isScrolling.current = true

    e.deltaY > 0 ? goNext() : goPrev()

    setTimeout(() => {
      isScrolling.current = false
    }, 500)
  }, [currentIndex, lives.length])

  return {
    lives,
    currentIndex,
    goNext,
    goPrev,
    selectLiveById,
    loadingMore,
    handleTouchEnd,
    handleTouchStart,
    handleWheel,
    loading
  }
}
