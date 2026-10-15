import StarRoundedIcon from '@mui/icons-material/StarRounded'
import { Box, Divider, LinearProgress, Rating, Typography } from '@mui/material'

const StarBar = ({ star, count, total }) => {
  const pct = total > 0 ? Math.round((count / total) * 100) : 0
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.3, flexShrink: 0, width: 28 }}>
        <Typography sx={{ fontSize: '0.72rem', fontWeight: 700, color: '#555' }}>{star}</Typography>
        <StarRoundedIcon sx={{ fontSize: 11, color: '#f59e0b' }} />
      </Box>
      <LinearProgress
        variant="determinate"
        value={pct}
        sx={{
          flex: 1, height: 6, borderRadius: 3,
          bgcolor: '#f0f0f0',
          '& .MuiLinearProgress-bar': { bgcolor: '#f59e0b', borderRadius: 3 }
        }}
      />
      <Typography sx={{ fontSize: '0.7rem', color: '#aaa', flexShrink: 0, width: 24, textAlign: 'right' }}>
        {count}
      </Typography>
    </Box>
  )
}

const ReviewRatingSummary = ({ avg = 0, total = 0, distribution = {} }) => {
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

      <Box sx={{ flex: 1, width: '100%', display: 'flex', flexDirection: 'column', gap: 0.6 }}>
        {[5, 4, 3, 2, 1].map(star => (
          <StarBar key={star} star={star} count={distribution[star] ?? 0} total={total} />
        ))}
      </Box>
    </Box>
  )
}

export default ReviewRatingSummary
