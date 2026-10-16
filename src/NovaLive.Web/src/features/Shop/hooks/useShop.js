import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useDispatch, useSelector } from 'react-redux'
import { toast } from 'react-toastify'
import { getShopByUserAPI, registerShopAPI, updateShopAPI } from '~/common/apis/services/shopService'
import { selectCurrentUser, setCurrentUser } from '~/store/user/userSlice'

export const useShop = () => {
  const currentUser = useSelector(selectCurrentUser)
  const shopId = currentUser?.user_shop

  const { data: shop, isLoading } = useQuery({
    queryKey: ['my_shop', currentUser?._id],
    queryFn: getShopByUserAPI,
    enabled: !!shopId,
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
      if (data?.id) dispatch(setCurrentUser({ ...currentUser, user_shop: data.id }))
      queryClient.invalidateQueries({ queryKey: ['my_profile', currentUser?._id] })
      queryClient.invalidateQueries({ queryKey: ['my_shop', currentUser?._id] })
      toast.success('Đăng ký cửa hàng thành công!')
    },
    onError: (error) => {
      toast.error(error?.response?.data?.message || 'Đăng ký cửa hàng thất bại!')
    }
  })

  const updateMutation = useMutation({
    mutationFn: updateShopAPI,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my_shop', currentUser?._id] })
      toast.success('Cập nhật cửa hàng thành công!')
    },
    onError: (error) => {
      toast.error(error?.response?.data?.message || 'Cập nhật thất bại!')
    }
  })

  return {
    registerShop: registerMutation.mutate,
    isRegistering: registerMutation.isPending,
    updateShop: updateMutation.mutate,
    isUpdatingShop: updateMutation.isPending
  }
}
