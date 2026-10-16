import { useQuery } from '@tanstack/react-query'
import { getAllDiscountOfShopAPI } from '~/common/apis/services/discountService'
import { normalizeDiscount } from '../utils/normalizeDiscount'

export const useShopDiscount = (shopId, enabled = true) => {
  const { data = [], isLoading } = useQuery({
    queryKey: ['discounts', 'shop', shopId],
    queryFn: () => getAllDiscountOfShopAPI(shopId, { status: 'active' }),
    enabled: !!shopId && enabled,
    staleTime: 60 * 1000,
    select: (raw) => {
      const list = Array.isArray(raw) ? raw : raw?.items ?? []
      return list
        .filter(d => d.discount_scope === 'shop' && d.discount_is_active)
        .map(normalizeDiscount)
        .filter(d => d.status === 'active')
    }
  })


  return {
    discounts: data,
    productDiscounts: data.filter(d => d.category === 'product'),
    freeshipDiscounts: data.filter(d => d.category === 'freeship'),
    isLoading
  }
}
