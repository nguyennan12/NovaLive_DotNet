/* eslint-disable indent */
import { useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { queryDiscountAPI } from '~/common/apis/services/discountService'
import { useDebounce } from '~/common/hooks/useDebounce'
import { buildQueryParamsDiscounts } from '~/common/utils/builder'
import { LIMIT } from '~/common/utils/constant'
import { useDiscountMutations } from './useDiscountMutations'
import { normalizeDiscount } from '../utils/normalizeDiscount'

const safeArray = (value) => (Array.isArray(value) ? value : [])

export const useDiscounts = (filters) => {
  const discountMutations = useDiscountMutations()
  const search = useDebounce(filters.search)

  const listQueryParams = useMemo(() => {
    return buildQueryParamsDiscounts({
      limit: LIMIT.DISCOUNT,
      page: 1,
      shopId: filters.shopId,
      search,
      scope: filters.scope,
      status: filters.status,
      type: filters.type,
      category: filters.category
    })
  }, [search, filters.status, filters.type, filters.category, filters.shopId, filters.scope])

  const {
    data: filtered = [], isLoading, isFetching, error } = useQuery({
      queryKey: ['discounts', 'list', listQueryParams],
      queryFn: () => queryDiscountAPI(listQueryParams),
      placeholderData: (previousData) => previousData,
      staleTime: 20 * 1000,
      select: (raw) => safeArray(raw?.items).map(normalizeDiscount)
    })

  const stats = useMemo(() => ({
    total: filtered.length,
    active: filtered.filter((d) => d.status === 'active').length,
    draft: filtered.filter((d) => d.status === 'draft').length,
    expired: filtered.filter((d) => d.status === 'expired').length,
    freeship: filtered.filter((d) => d.category === 'freeship').length,
    product: filtered.filter((d) => d.category === 'product').length
  }), [filtered])

  const { productDiscounts, shippingDiscounts } = useMemo(() => {
    return {
      productDiscounts: filtered.filter((d) => d.category === 'product'),
      shippingDiscounts: filtered.filter((d) => d.category === 'freeship')
    }
  }, [filtered])

  return {
    productDiscounts,
    shippingDiscounts,
    filtered,
    stats,
    isLoading,
    isFetching,
    error,
    ...discountMutations
  }
}
