import ArrowForwardIosRoundedIcon from '@mui/icons-material/ArrowForwardIosRounded'
import { Box, Divider, Typography } from '@mui/material'
import { glassSx } from '~/theme'
import ReviewCard from './ReviewCard'
import ReviewRatingSummary from './ReviewRatingSummary'
import { useProductReviews } from '../hooks/useProductReviews'

const ProductReviewsSection = ({ productId, ratingAvg, ratingCount }) => {
  const { reviews, summary } = useProductReviews(productId)

  const avg = ratingAvg ?? summary.avg
  const total = ratingCount ?? summary.total

  return (
    <Box sx={{ bgcolor: 'primary.main', ...glassSx }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2.5 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box sx={{ width: 3.5, height: 22, borderRadius: '2px', bgcolor: '#f59e0b', flexShrink: 0 }} />
          <Typography sx={{ fontSize: { xs: '1rem', sm: '1.1rem' }, fontWeight: 800, color: 'primary.contrastText' }}>
            Đánh giá sản phẩm
          </Typography>
          <Typography sx={{ fontSize: '0.78rem', color: '#aaa', fontWeight: 500 }}>
            ({total} đánh giá)
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, cursor: 'pointer', opacity: 0.75, '&:hover': { opacity: 1 } }}>
          <Typography sx={{ fontSize: '0.78rem', color: 'secondary.main', fontWeight: 600 }}>Xem tất cả</Typography>
          <ArrowForwardIosRoundedIcon sx={{ fontSize: 11, color: 'secondary.main' }} />
        </Box>
      </Box>

      <ReviewRatingSummary avg={avg} total={total} distribution={summary.distribution} />

      <Divider sx={{ borderColor: '#f0f0f0' }} />

      {reviews.map((review, idx) => (
        <Box key={review._id}>
          <ReviewCard review={review} />
          {idx < reviews.length - 1 && <Divider sx={{ borderColor: '#f5f5f5' }} />}
        </Box>
      ))}
    </Box>
  )
}

export default ProductReviewsSection
