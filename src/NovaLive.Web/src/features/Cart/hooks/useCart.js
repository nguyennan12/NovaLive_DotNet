import { useQuery } from '@tanstack/react-query'
import { useDispatch, useSelector } from 'react-redux'
import { getCartAPI } from '~/common/apis/services/cartService'
import { deselectAll, selectGuestItems, toggleSelect } from '~/store/cart/cartSlice'
import { selectCurrentUser } from '~/store/user/userSlice'
import { useCartCalc } from './useCartCalc'
import { useCartMutations } from './useCartMutations'

export const CART_QUERY_KEY = ['cart']

export const useCart = () => {
  const dispatch = useDispatch()
  const currentUser = useSelector(selectCurrentUser)
  const guestItems = useSelector(selectGuestItems)

  const { data, isLoading } = useQuery({
    queryKey: CART_QUERY_KEY,
    queryFn: getCartAPI,
    enabled: !!currentUser,
    staleTime: 1000 * 30
  })


  const guestGroups = Object.values(guestItems.reduce((groups, item) => {
    const key = String(item.shopId || 'guest')
    if (!groups[key]) groups[key] = { shopId: item.shopId || key, shopName: item.shopName || 'Sản phẩm', items: [] }
    groups[key].items.push(item)
    return groups
  }, {}))
  const shopGroups = currentUser
    ? (Array.isArray(data?.items) ? data.items : [])
    : guestGroups
  const cartId = currentUser ? data?.cartId : 'guest-cart'
  const cartCount = currentUser
    ? data?.cartCount
    : guestItems.reduce((total, item) => total + item.quantity, 0)

  const calculations = useCartCalc(shopGroups)
  const mutations = useCartMutations()

  return {
    ...calculations,
    ...mutations,
    isLoading: currentUser ? isLoading : false,
    shopGroups,
    cartId,
    cartCount,
    // cart selection
    toggleSelect: (cartItemId) => dispatch(toggleSelect(cartItemId)),
    deselectAll: () => dispatch(deselectAll())
  }
}

