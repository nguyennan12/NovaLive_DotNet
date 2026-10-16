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
    mutationFn: ({ discountCode, payload }) => updateDiscountAPI(discountCode, payload),
    onSuccess: invalidateDiscounts
  })

  const deleteMutation = useMutation({
    mutationFn: deleteDiscountAPI,
    onSuccess: invalidateDiscounts
  })

  return {
    addDiscount: createMutation.mutateAsync,
    updateDiscount: ({ targetDiscount, payload }) => {
      const code = targetDiscount?.code ?? targetDiscount?.discount_code
      if (!code) throw new Error('Missing discount code')
      return updateMutation.mutateAsync({ discountCode: code, payload })
    },
    deleteDiscount: (targetDiscount) => {
      const code = targetDiscount?.code ?? targetDiscount?.discount_code
      if (!code) throw new Error('Missing discount code')
      return deleteMutation.mutateAsync(code)
    },
    // Trạng thái loading chung cho các nút bấm UI
    isMutating: createMutation.isPending || updateMutation.isPending || deleteMutation.isPending
  }
}