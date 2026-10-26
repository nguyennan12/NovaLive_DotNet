import { useQuery } from '@tanstack/react-query'
import authorizedAxiosInstance from '~/common/apis/custom/authorizeAxios'

export const useProductReviews = (productId) => {
  const { data, isLoading } = useQuery({
    queryKey: ['product_reviews', productId],
    queryFn: async () => {
      try {
        const res = await authorizedAxiosInstance.get(`products/${productId}/reviews`)
        return res?.data !== undefined ? res.data : res
      } catch (_e) {
        return {
          reviews: [],
          summary: { avg: 5, total: 0, distribution: { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 } }
        }
      }
    },
    enabled: Boolean(productId),
    staleTime: 60 * 1000
  })

  return {
    reviews: data?.reviews || [],
    summary: data?.summary || { avg: 5, total: 0, distribution: { 5: 0, 4: 0, 3: 0, 2: 0, 1: 0 } },
    isLoading
  }
}
