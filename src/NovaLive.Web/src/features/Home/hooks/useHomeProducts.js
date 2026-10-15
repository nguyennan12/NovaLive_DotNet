/* eslint-disable indent */
import { useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { queryProductAPI } from '~/common/apis/services/productService'
import { buildQueryParamsProducts } from '~/common/utils/builder'
import { toArrayProducts } from '~/common/utils/converter'
import { useInfiniteProducts } from '~/features/Product/hooks/useProducts'
import { PRICE_SLIDER_MAX, PRICE_SLIDER_MIN } from '../configs/homeFilter.config'
import { useDebounce } from '~/common/hooks/useDebounce'


const sortList = (list, sort) => {
  const arr = [...list]
  switch (sort) {
    case 'price_asc': return arr.sort((a, b) => (a.spu_price || 0) - (b.spu_price || 0))
    case 'price_desc': return arr.sort((a, b) => (b.spu_price || 0) - (a.spu_price || 0))
    case 'name_az': return arr.sort((a, b) => (a.spu_name || '').localeCompare(b.spu_name || ''))
    default: return arr
  }
}

export const useHomeProducts = (filters = {}) => {
  const [minPrice, maxPrice] = filters.priceRange ?? [PRICE_SLIDER_MIN, PRICE_SLIDER_MAX]
  const isFiltering = minPrice > PRICE_SLIDER_MIN || maxPrice < PRICE_SLIDER_MAX
    || !!filters.category || !!filters.keyword || !!filters.shopId
    || !!filters.stock || !!filters.status
  const search = useDebounce(filters.keyword, 280)
  //xử lý query params
  const queryString = useMemo(() => {
    if (!isFiltering) return ''
    const params = buildQueryParamsProducts({
      sort: filters.sort,
      category: filters.category,
      keyword: search,
      stock: filters.stock,
      status: filters.status,
      shopId: filters.shopId,
      minPrice,
      ...(maxPrice < PRICE_SLIDER_MAX ? { maxPrice } : {})
    })
    return new URLSearchParams(params).toString()
  }, [isFiltering, minPrice, maxPrice, filters.sort, filters.category, search, filters.shopId, filters.status, filters.stock])

  //fetch product base
  const { products, isLoading, fetchNextPage, hasNextPage, isFetchingNextPage } = useInfiniteProducts()

  //fetch product filter
  const { data: rawFiltered = [], isLoading: filterLoading } = useQuery({
    queryKey: ['home_filtered_price', queryString],
    queryFn: () => queryProductAPI(queryString),
    staleTime: 60 * 1000,
    enabled: isFiltering && !!queryString
  })

  //get product filter
  const filteredProducts = useMemo(
    () => sortList(toArrayProducts(rawFiltered), filters.sort),
    [rawFiltered, filters.sort]
  )

  const sortedAll = useMemo(() => sortList(products, filters.sort), [products, filters.sort])
  const featuredProducts = useMemo(() => sortedAll.slice(0, 10), [sortedAll])
  //get products best seller
  const bestSellers = useMemo(
    () => [...products].sort((a, b) => (b.total_sold || 0) - (a.total_sold || 0)).slice(0, 6),
    [products]
  )
  //list product cuối cùng
  const newArrivals = useMemo(() => sortedAll, [sortedAll])

  return {
    isFiltering,
    filteredProducts,
    filterLoading,
    featuredProducts,
    bestSellers,
    newArrivals,
    isLoading,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage
  }
}
