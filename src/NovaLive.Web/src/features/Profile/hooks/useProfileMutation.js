import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useDispatch, useSelector } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { changePasswordAPI, logoutAllDevicesAPI, updateMyProfileAPI } from '~/common/apis/services/userService'
import { clearCurrentUser, selectCurrentUser, setCurrentUser } from '~/store/user/userSlice'

export const useProfileMutation = () => {
  const queryClient = useQueryClient()
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const currentUser = useSelector(selectCurrentUser)

  const updateMutation = useMutation({
    mutationFn: updateMyProfileAPI,
    onSuccess: (updatedUser) => {
      queryClient.invalidateQueries({ queryKey: ['my_profile', currentUser?._id] })
      if (updatedUser) dispatch(setCurrentUser({ ...currentUser, ...updatedUser }))
      toast.success('Cập nhật thông tin thành công!')
    },
    onError: (error) => {
      toast.error(error?.response?.data?.message || 'Cập nhật thất bại!')
    }
  })

  const changePasswordMutation = useMutation({
    mutationFn: changePasswordAPI,
    onSuccess: () => toast.success('Đổi mật khẩu thành công!'),
    onError: (error) => {
      toast.error(error?.response?.data?.message || 'Đổi mật khẩu thất bại!')
    }
  })

  const logoutAllMutation = useMutation({
    mutationFn: logoutAllDevicesAPI,
    onSuccess: () => {
      dispatch(clearCurrentUser())
      navigate('/login')
    },
    onError: () => toast.error('Không thể đăng xuất tất cả thiết bị!')
  })

  return {
    updateProfile: updateMutation.mutate,
    isUpdating: updateMutation.isPending,
    changePassword: changePasswordMutation.mutate,
    isChangingPassword: changePasswordMutation.isPending,
    logoutAll: logoutAllMutation.mutate,
    isLoggingOut: logoutAllMutation.isPending
  }
}
