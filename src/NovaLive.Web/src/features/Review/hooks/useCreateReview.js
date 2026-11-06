import { useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import { createReviewAPI } from '~/common/apis/services/reviewService'

export const useCreateReview = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: createReviewAPI,
    onSuccess: () => {
      toast.success('Đánh giá đã được gửi')
      queryClient.invalidateQueries({ queryKey: ['product_reviews'] })
    },
    onError: (error) => {
      toast.error(error?.response?.data?.message || 'Không thể gửi đánh giá. Vui lòng thử lại.')
    }
  })
}
