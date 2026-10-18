import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useDispatch, useSelector } from 'react-redux'
import { toast } from 'react-toastify'
import { addToCartAPI, removeFromCartAPI, updateCartItemAPI } from '~/common/apis/services/cartService'
import { selectSelectedIds, setSelectedIds } from '~/store/cart/cartSlice'

export const CART_QUERY_KEY = ['cart']

export const useCartMutations = () => {
  const dispatch = useDispatch()
  const queryClient = useQueryClient()
  const selectedIds = useSelector(selectSelectedIds)

  const invalidateCart = () =>
    queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY })

  const addMutation = useMutation({
    mutationFn: addToCartAPI,
    onSuccess: () => {
      invalidateCart()
      toast.success('Đã thêm vào giỏ hàng!')
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

  return {
    addToCart: (data) => addMutation.mutate(data),
    addToCartAsync: (data) => addMutation.mutateAsync(data),
    //xóa 1 item ra khỏi cart
    removeItem: (cartItemId) => removeMutation.mutate({ cartItemIds: [String(cartItemId)] }),
    //xóa những items đã dc chọn ra khỏi cart
    removeSelectedItems: () => removeMutation.mutate({ cartItemIds: selectedIds }),
    updateQuantity: (cartItemId, newQty) => updateMutation.mutate({ cartItemId, quantity: newQty }),
    isPending: addMutation.isPending || removeMutation.isPending || updateMutation.isPending
  }
}
