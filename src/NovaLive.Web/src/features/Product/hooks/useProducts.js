import { useInfiniteQuery, useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { getAllProductsAPI } from '~/common/apis/services/productService'
import { LIMIT } from '~/common/utils/constant'
import { toArrayProducts } from '~/common/utils/converter'

export const useProducts = (params = {}) => {
  const { data = [] } = useQuery({
    queryKey: ['products', params],
    queryFn: () => getAllProductsAPI(params),
    staleTime: 2 * 60 * 1000
  })
  const products = data?.products ?? []
  return { products }
}

export const useInfiniteProducts = () => {

  const { data: infiniteData, fetchNextPage, hasNextPage, isFetchingNextPage, isLoading } = useInfiniteQuery({
    queryKey: ['product_infinite'],
    queryFn: ({ pageParam = 1 }) => getAllProductsAPI({ page: pageParam, limit: LIMIT.PRODUCTS }),
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) => {
      const productsArray = toArrayProducts(lastPage)
      if (productsArray.length === LIMIT.PRODUCTS) {
        return allPages.length + 1
      }
      return undefined
    },
    staleTime: 5 * 60 * 1000
  })
  const products = useMemo(() => {
    if (!infiniteData) return []
    return infiniteData.pages.flatMap(page => toArrayProducts(page))
  }, [infiniteData])

  return {
    products,
    isLoading,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage
  }
}