import { useMutation, useQueryClient } from '@tanstack/react-query'
import {
  createDiscountAPI,
  deleteDiscountAPI,
  updateDiscountAPI
} from '~/common/apis/services/discountService'

export const useDiscountMutations = () => {
  const queryClient = useQueryClient()


  const invalidateDiscounts = () => {
    queryClient.invalidateQueries({ queryKey: ['discounts'] })
  }

  const createMutation = useMutation({
    mutationFn: createDiscountAPI,
    onSuccess: invalidateDiscounts
  })

  const updateMutation = useMutation({
    mutationFn: ({ discountId, payload }) => updateDiscountAPI(discountId, payload),
    onSuccess: invalidateDiscounts
  })

  const deleteMutation = useMutation({
    mutationFn: deleteDiscountAPI,
    onSuccess: invalidateDiscounts
  })

  return {
    addDiscount: createMutation.mutateAsync,
    updateDiscount: ({ targetDiscount, payload }) => {
      const id = targetDiscount?.id ?? targetDiscount?._id
      if (!id) throw new Error('Missing discount id')
      return updateMutation.mutateAsync({ discountId: id, payload })
    },
    deleteDiscount: (targetDiscount) => {
      const id = targetDiscount?.id ?? targetDiscount?._id
      if (!id) throw new Error('Missing discount id')
      return deleteMutation.mutateAsync(id)
    },
    // Trạng thái loading chung cho các nút bấm UI
    isMutating: createMutation.isPending || updateMutation.isPending || deleteMutation.isPending
  }
}
