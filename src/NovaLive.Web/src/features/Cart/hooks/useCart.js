import { useQuery } from '@tanstack/react-query'
import { useDispatch } from 'react-redux'
import { getCartAPI } from '~/common/apis/services/cartService'
import { deselectAll, toggleSelect } from '~/store/cart/cartSlice'
import { useCartCalc } from './useCartCalc'
import { useCartMutations } from './useCartMutations'

export const CART_QUERY_KEY = ['cart']

export const useCart = () => {
  const dispatch = useDispatch()

  const { data, isLoading } = useQuery({
    queryKey: CART_QUERY_KEY,
    queryFn: getCartAPI,
    staleTime: 1000 * 30
  })


  const shopGroups = Array.isArray(data?.items) ? data.items : []
  const cartId = data?.cartId
  const cartCount = data?.cartCount

  const calculations = useCartCalc(shopGroups)
  const mutations = useCartMutations()

  return {
    ...calculations,
    ...mutations,
    isLoading,
    shopGroups,
    cartId,
    cartCount,
    // cart selection
    toggleSelect: (skuId) => dispatch(toggleSelect(skuId)),
    deselectAll: () => dispatch(deselectAll())
  }
}

