import VerifiedRoundedIcon from '@mui/icons-material/VerifiedRounded'
import { Avatar, Box, Rating, Typography } from '@mui/material'
import { formatDate } from '~/common/utils/formatters'

const AVATAR_COLORS = ['#3485f7', '#8b5cf6', '#f59e0b', '#10b981', '#ef4444', '#06b6d4']

const avatarColor = (name = '') =>
  AVATAR_COLORS[name.charCodeAt(0) % AVATAR_COLORS.length]

const timeAgo = (isoStr) => {
  const diff = Math.floor((Date.now() - new Date(isoStr)) / 1000)
  if (diff < 60) return 'Vừa xong'
  if (diff < 3600) return `${Math.floor(diff / 60)} phút trước`
  if (diff < 86400) return `${Math.floor(diff / 3600)} giờ trước`
  if (diff < 86400 * 30) return `${Math.floor(diff / 86400)} ngày trước`
  return formatDate(isoStr)
}

const ReviewCard = ({ review }) => {
  const buyerName = review.buyerName || 'Người mua'

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5 }}>
        <Avatar
          src={review.buyerAvatarUrl || undefined}
          sx={{
            width: 38, height: 38, flexShrink: 0,
            bgcolor: avatarColor(buyerName),
            fontSize: '0.9rem', fontWeight: 800
          }}
        >
          {buyerName.charAt(0)}
        </Avatar>

        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
              <Typography sx={{ fontSize: '0.82rem', fontWeight: 700, color: 'primary.contrastText' }}>
                {buyerName}
              </Typography>
              <Rating
                value={review.rating}
                readOnly size="small"
                sx={{ color: '#f59e0b', '& .MuiRating-icon': { fontSize: 13 } }}
              />
            </Box>
          </Box>

          <Typography sx={{ fontSize: '0.72rem', color: '#bbb', mt: 0.3, mb: 1 }}>
            {timeAgo(review.createdAt)} · <VerifiedRoundedIcon sx={{ fontSize: 12, verticalAlign: 'middle', color: '#16a34a' }} /> Đã mua hàng
          </Typography>

          {review.title && <Typography sx={{ fontSize: '0.86rem', fontWeight: 700, color: '#333', mb: 0.5 }}>{review.title}</Typography>}

          <Typography sx={{ fontSize: '0.84rem', color: '#444', lineHeight: 1.6 }}>
            {review.content || 'Người mua không để lại nhận xét.'}
          </Typography>
          {!!review.mediaUrls?.length && (
            <Box sx={{ display: 'flex', gap: 1, mt: 1.25, flexWrap: 'wrap' }}>
              {review.mediaUrls.map((url) => <Box key={url} component="img" src={url} alt="Ảnh đánh giá" sx={{ width: 76, height: 76, objectFit: 'cover', borderRadius: 1.5 }} />)}
            </Box>
          )}
          {review.sellerReply && (
            <Box sx={{ mt: 1.5, p: 1.25, bgcolor: 'rgba(52,133,247,0.06)', borderRadius: 1.5 }}>
              <Typography sx={{ fontSize: '0.75rem', fontWeight: 700, color: 'secondary.main', mb: 0.35 }}>Phản hồi từ shop</Typography>
              <Typography sx={{ fontSize: '0.8rem', color: '#555', lineHeight: 1.5 }}>{review.sellerReply}</Typography>
            </Box>
          )}
        </Box>
      </Box>
    </Box>
  )
}

export default ReviewCard
