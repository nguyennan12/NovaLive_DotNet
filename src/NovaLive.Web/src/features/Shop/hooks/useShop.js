import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useDispatch, useSelector } from 'react-redux'
import { toast } from 'react-toastify'
import { getShopByUserAPI, registerShopAPI, updateShopAPI } from '~/common/apis/services/shopService'
import { getApiErrorMessage } from '~/common/utils/formatters'
import { selectCurrentUser, setCurrentUser } from '~/store/user/userSlice'

export const useShop = () => {
  const currentUser = useSelector(selectCurrentUser)

  const { data: shop, isLoading } = useQuery({
    queryKey: ['my_shop', currentUser?._id],
    queryFn: getShopByUserAPI,
    enabled: !!currentUser,
    retry: false,
    staleTime: 1000 * 60
  })

  return { shop, isLoading }
}

export const useShopMutation = () => {
  const dispatch = useDispatch()
  const queryClient = useQueryClient()
  const currentUser = useSelector(selectCurrentUser)

  const registerMutation = useMutation({
    mutationFn: registerShopAPI,
    onSuccess: (data) => {
      if (data?.id) {
        dispatch(setCurrentUser({ ...currentUser, shopId: data.id, user_shop: data.id }))
        // Reflect the newly-created Pending shop immediately. Waiting for an
        // invalidation refetch left the profile in its pre-registration state.
        queryClient.setQueryData(['my_shop', currentUser?._id], data)
      }
      queryClient.invalidateQueries({ queryKey: ['my_profile', currentUser?._id] })
      queryClient.invalidateQueries({ queryKey: ['my_shop', currentUser?._id] })
      toast.success('Đăng ký cửa hàng thành công!')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error, 'Đăng ký cửa hàng thất bại!'))
    }
  })

  const updateMutation = useMutation({
    mutationFn: updateShopAPI,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my_shop', currentUser?._id] })
      toast.success('Cập nhật cửa hàng thành công!')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error, 'Cập nhật thất bại!'))
    }
  })

  return {
    registerShop: registerMutation.mutate,
    isRegistering: registerMutation.isPending,
    updateShop: updateMutation.mutate,
    isUpdatingShop: updateMutation.isPending
  }
}
