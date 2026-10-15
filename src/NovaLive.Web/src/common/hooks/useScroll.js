import { useEffect, useRef, useCallback } from 'react'
import { useLocation } from 'react-router-dom'

export default function ScrollToTop() {
  const { pathname } = useLocation()

  useEffect(() => {
    window.scrollTo(0, 0)
  }, [pathname])

  return null
}

export const useInfiniteScroll = ({
  isFiltering,
  isFetchingNextPage,
  hasNextPage,
  fetchNextPage
}) => {
  const observer = useRef()

  return useCallback(node => {
    if (isFiltering || isFetchingNextPage) return

    if (observer.current) observer.current.disconnect()

    observer.current = new IntersectionObserver(entries => {
      if (entries[0].isIntersecting && hasNextPage) {
        fetchNextPage()
      }
    })

    if (node) observer.current.observe(node)
  }, [isFiltering, isFetchingNextPage, hasNextPage, fetchNextPage])
}