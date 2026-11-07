import { Box, Divider, Rating, Typography } from '@mui/material'

const ReviewRatingSummary = ({ avg = 0, total = 0 }) => {
  return (
    <Box sx={{
      display: 'flex', gap: { xs: 2, sm: 4 }, alignItems: 'center',
      flexDirection: { xs: 'column', sm: 'row' },
      bgcolor: 'rgba(245,158,11,0.06)', borderRadius: 2, p: { xs: 1.5, sm: 2 }, mb: 2.5
    }}>
      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', flexShrink: 0 }}>
        <Typography sx={{ fontSize: '3rem', fontWeight: 900, color: '#f59e0b', lineHeight: 1 }}>
          {Number(avg).toFixed(1)}
        </Typography>
        <Rating value={avg} readOnly precision={0.5} sx={{ color: '#f59e0b', mt: 0.5 }} size="small" />
        <Typography sx={{ fontSize: '0.72rem', color: '#aaa', mt: 0.5 }}>{total} đánh giá</Typography>
      </Box>

      <Divider orientation="vertical" flexItem sx={{ display: { xs: 'none', sm: 'block' }, borderColor: '#eee' }} />
      <Typography sx={{ flex: 1, fontSize: '0.85rem', color: '#777', textAlign: { xs: 'center', sm: 'left' } }}>
        Điểm trung bình trên {total} lượt đánh giá
      </Typography>
    </Box>
  )
}

export default ReviewRatingSummary
