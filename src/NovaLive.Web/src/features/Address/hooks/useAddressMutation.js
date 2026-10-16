import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import { createAddressAPI } from '~/common/apis/services/addressService'

export const useAddressMutations = (userId) => {
  const queryClient = useQueryClient()

  const createMutation = useMutation({
    mutationFn: createAddressAPI,
    onSuccess: () => {
      queryClient.refetchQueries({ queryKey: ['all_address_user', userId] })
      toast.success('Thêm địa chỉ thành công!')
    },
    onError: (error) => {
      toast.error(error?.response?.data?.message || 'Thêm địa chỉ thất bại!')
    }
  })

  return {
    createAddress: createMutation.mutate,
    isCreating: createMutation.isPending
  }
}
