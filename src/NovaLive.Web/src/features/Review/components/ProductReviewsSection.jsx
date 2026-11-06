import RateReviewOutlinedIcon from '@mui/icons-material/RateReviewOutlined'
import { Alert, Box, Button, CircularProgress, Divider, Typography } from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useSelector } from 'react-redux'
import { getMyOrdersAPI } from '~/common/apis/services/orderService'
import { selectCurrentUser } from '~/store/user/userSlice'
import { glassSx } from '~/theme'
import ReviewCard from './ReviewCard'
import ReviewOrderDialog from './ReviewOrderDialog'
import { useProductReviews } from '../hooks/useProductReviews'

const ProductReviewsSection = ({ productId, skuIds = [] }) => {
  const user = useSelector(selectCurrentUser)
  const [showReview, setShowReview] = useState(false)
  const { reviews, summary, isLoading, isError, fetchNextPage, hasNextPage, isFetchingNextPage, refetch } = useProductReviews(productId)
  const { data: deliveredOrders = [] } = useQuery({
    queryKey: ['review-eligible-orders', user?.id || user?._id, productId],
    queryFn: () => getMyOrdersAPI({ status: 'delivered', limit: 100 }),
    enabled: Boolean(user && productId && skuIds.length),
    staleTime: 30_000
  })
  const productSkuIds = new Set(skuIds.map(String))
  const reviewableItems = deliveredOrders.flatMap(order => order.order_products || [])
    .filter(item => item.id && item.skuId && productSkuIds.has(String(item.skuId)))

  return (
    <Box sx={{ bgcolor: 'primary.main', ...glassSx }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2.5 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box sx={{ width: 3.5, height: 22, borderRadius: '2px', bgcolor: '#f59e0b', flexShrink: 0 }} />
          <Typography sx={{ fontSize: { xs: '1rem', sm: '1.1rem' }, fontWeight: 800, color: 'primary.contrastText' }}>
            Đánh giá sản phẩm
          </Typography>
          {!isLoading && !isError && <Typography sx={{ fontSize: '0.78rem', color: '#aaa', fontWeight: 500 }}>({summary.total} đánh giá)</Typography>}
        </Box>
        {reviewableItems.length > 0 && (
          <Button size="small" variant="outlined" startIcon={<RateReviewOutlinedIcon />} onClick={() => setShowReview(true)} sx={{ textTransform: 'none', fontWeight: 700 }}>
            Viết đánh giá
          </Button>
        )}
      </Box>

      <Divider sx={{ borderColor: '#f0f0f0' }} />

      {isError ? (
        <Alert severity="error" action={<Button color="inherit" size="small" onClick={() => refetch()}>Thử lại</Button>} sx={{ my: 2 }}>
          Không tải được đánh giá sản phẩm.
        </Alert>
      ) : isLoading ? (
        <Box sx={{ py: 3, display: 'flex', justifyContent: 'center' }}><CircularProgress size={24} /></Box>
      ) : reviews.length === 0 ? (
        <Box sx={{ py: 3, textAlign: 'center' }}>
          <Typography sx={{ fontSize: '0.85rem', color: 'rgba(45,45,45,0.45)' }}>
            Chưa có đánh giá nào cho sản phẩm này.
          </Typography>
        </Box>
      ) : (
        reviews.map((review, idx) => (
          <Box key={review.id || idx}>
            <ReviewCard review={review} />
            {idx < reviews.length - 1 && <Divider sx={{ borderColor: '#f5f5f5' }} />}
          </Box>
        ))
      )}
      {hasNextPage && (
        <Box sx={{ display: 'flex', justifyContent: 'center', pt: 1 }}>
          <Button onClick={() => fetchNextPage()} disabled={isFetchingNextPage} sx={{ textTransform: 'none', fontWeight: 700 }}>
            {isFetchingNextPage ? <CircularProgress size={18} /> : 'Xem thêm đánh giá'}
          </Button>
        </Box>
      )}
      <ReviewOrderDialog
        open={showReview}
        onClose={() => setShowReview(false)}
        items={reviewableItems}
      />
    </Box>
  )
}

export default ProductReviewsSection
