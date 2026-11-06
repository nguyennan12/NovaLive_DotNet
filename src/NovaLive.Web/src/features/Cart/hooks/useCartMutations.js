import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useDispatch, useSelector } from 'react-redux'
import { toast } from 'react-toastify'
import { addToCartAPI, removeFromCartAPI, updateCartItemAPI } from '~/common/apis/services/cartService'
import {
  addGuestItem, removeGuestItems, selectSelectedIds,
  setSelectedIds, updateGuestQuantity
} from '~/store/cart/cartSlice'
import { clearCurrentUser, selectCurrentUser } from '~/store/user/userSlice'

export const CART_QUERY_KEY = ['cart']

export const useCartMutations = () => {
  const dispatch = useDispatch()
  const queryClient = useQueryClient()
  const selectedIds = useSelector(selectSelectedIds)
  const currentUser = useSelector(selectCurrentUser)

  const invalidateCart = () =>
    queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY })

  const addMutation = useMutation({
    mutationFn: addToCartAPI,
    onSuccess: () => {
      invalidateCart()
      toast.success('Đã thêm vào giỏ hàng!')
    },
    onError: (err) => {
      if (err?.response?.status !== 401) {
        toast.error(err?.response?.data?.detail ?? err?.response?.data?.message ?? 'Không thể thêm sản phẩm vào giỏ hàng')
      }
    }
  })

  const updateMutation = useMutation({
    mutationFn: updateCartItemAPI,
    onSuccess: invalidateCart,
    onError: (err) =>
      toast.error(err?.response?.data?.message ?? 'Không thể cập nhật số lượng')
  })

  const removeMutation = useMutation({
    mutationFn: removeFromCartAPI,
    onSuccess: (_, variables) => {
      invalidateCart()
      const removedIds = variables.cartItemIds.map(String)
      dispatch(setSelectedIds(selectedIds.filter(id => !removedIds.includes(id))))
    },
    onError: (err) =>
      toast.error(err?.response?.data?.message ?? 'Không thể xóa sản phẩm')
  })

  const addGuest = (data) => {
    const cartItemId = `guest-${data.skuId}`
    dispatch(addGuestItem(data))
    toast.success('Đã thêm vào giỏ hàng!')
    return { ...data.guestProduct, ...data, cartItemId, id: cartItemId, _id: cartItemId }
  }

  const addAsync = async (data) => {
    if (!currentUser) return addGuest(data)
    try {
      return await addMutation.mutateAsync(data)
    } catch (error) {
      if (error?.response?.status === 401) {
        dispatch(clearCurrentUser())
        return addGuest(data)
      }
      throw error
    }
  }

  const removeItem = (cartItemId) => {
    if (!currentUser || String(cartItemId).startsWith('guest-')) {
      dispatch(removeGuestItems([String(cartItemId)]))
      return
    }
    removeMutation.mutate({ cartItemIds: [String(cartItemId)] })
  }

  const removeSelectedItems = () => {
    if (!currentUser) {
      dispatch(removeGuestItems(selectedIds))
      return
    }
    removeMutation.mutate({ cartItemIds: selectedIds })
  }

  const updateQuantity = (cartItemId, quantity) => {
    if (!currentUser || String(cartItemId).startsWith('guest-')) {
      dispatch(updateGuestQuantity({ cartItemId, quantity }))
      return
    }
    updateMutation.mutate({ cartItemId, quantity })
  }

  return {
    addToCart: (data) => { addAsync(data).catch(() => {}) },
    addToCartAsync: addAsync,
    //xóa 1 item ra khỏi cart
    removeItem,
    //xóa những items đã dc chọn ra khỏi cart
    removeSelectedItems,
    updateQuantity,
    isPending: Boolean(currentUser) && (addMutation.isPending || removeMutation.isPending || updateMutation.isPending)
  }
}
