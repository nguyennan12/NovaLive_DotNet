import { useInfiniteQuery, useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { getMyOrdersAPI } from '~/common/apis/services/orderService'
import { LIMIT } from '~/common/utils/constant'

export const useMyOrders = () => {
  const { data: orders = [], isLoading, isError, refetch } = useQuery({
    queryKey: ['my_orders'],
    queryFn: () => getMyOrdersAPI({ limit: 100 }),
    staleTime: 30_000
  })
  return { orders, isLoading, isError, refetch }
}


export const useInfiniteOrders = (status, enabled = true) => {
  const { data: infiniteData, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading, isError, error, refetch } = useInfiniteQuery({
    queryKey: ['my_orders_infinite', status],
    queryFn: ({ pageParam = 1 }) => getMyOrdersAPI({ status, limit: LIMIT.ORDER, page: pageParam }),
    initialPageParam: 1,
    enabled,
    getNextPageParam: (lastPage, allPages) => {
      if (lastPage.length < LIMIT.ORDER) {
        return undefined
      }
      return allPages.length + 1
    },
    staleTime: 5 * 60 * 1000
  })
  const orders = useMemo(() => {
    if (!infiniteData) return []
    return infiniteData.pages.flat()
  }, [infiniteData])

  return {
    orders,
    isLoading,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage,
    isError,
    error,
    refetch
  }
}
