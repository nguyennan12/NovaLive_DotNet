import { useInfiniteQuery } from '@tanstack/react-query'
import { getProductReviewsAPI } from '~/common/apis/services/reviewService'

export const useProductReviews = (productId) => {
  const query = useInfiniteQuery({
    queryKey: ['product_reviews', productId],
    queryFn: ({ pageParam }) => getProductReviewsAPI(productId, { page: pageParam, size: 20 }),
    initialPageParam: 1,
    getNextPageParam: (lastPage) => lastPage.page < lastPage.totalPages ? lastPage.page + 1 : undefined,
    enabled: Boolean(productId),
    staleTime: 60 * 1000
  })

  const pages = query.data?.pages || []
  const reviews = pages.flatMap(page => page.items || [])
  const total = pages.at(-1)?.total || 0
  const avg = reviews.length
    ? reviews.reduce((sum, review) => sum + Number(review.rating || 0), 0) / reviews.length
    : 0

  return {
    reviews,
    summary: { avg, total },
    isLoading: query.isLoading,
    isError: query.isError,
    fetchNextPage: query.fetchNextPage,
    hasNextPage: query.hasNextPage,
    isFetchingNextPage: query.isFetchingNextPage,
    refetch: query.refetch
  }
}
