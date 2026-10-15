import { Box, Typography, Skeleton } from '@mui/material'
import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import { DiscountCard } from '../shared/DiscountCard'

const CardSkeleton = () => (
  <Box sx={{ bgcolor: '#fff', border: '1px solid #eeeeee', borderRadius: '14px', p: 2, display: 'flex', gap: 2 }}>
    <Skeleton variant='rounded' width='27%' height={110} sx={{ borderRadius: '8px', flexShrink: 0 }} />
    <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', gap: 1.5 }}>
      <Skeleton variant='rounded' width='60%' height={16} sx={{ borderRadius: '4px' }} />
      <Skeleton variant='rounded' width='40%' height={14} sx={{ borderRadius: '4px' }} />
      <Skeleton variant='rounded' width='80%' height={10} sx={{ borderRadius: '4px' }} />
      <Skeleton variant='rounded' width='100%' height={6} sx={{ borderRadius: '4px', mt: 'auto' }} />
    </Box>
  </Box>
)

const EmptyState = ({ hasFilter }) => (
  <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', py: 8, gap: 1.5 }}>
    <Box sx={{ width: 56, height: 56, borderRadius: '16px', bgcolor: '#f3f4f6', display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 0.5 }}>
      <LocalOfferRoundedIcon sx={{ fontSize: 28, color: '#9ca3af' }} />
    </Box>
    <Typography sx={{ fontSize: '0.9rem', fontWeight: 700, color: '#4b5563' }}>
      {hasFilter ? 'Không tìm thấy kết quả' : 'Chưa có discount nào'}
    </Typography>
    <Typography sx={{ fontSize: '0.78rem', color: '#9ca3af' }}>
      {hasFilter ? 'Thử thay đổi bộ lọc hoặc từ khoá tìm kiếm' : 'Tạo discount đầu tiên để bắt đầu'}
    </Typography>
  </Box>
)

export const DiscountList = ({ discounts, loading, hasFilter, onEdit, onDelete }) => {
  if (loading) {
    return (
      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', lg: 'repeat(3, 1fr)' }, gap: 2 }}>
        {Array.from({ length: 6 }).map((_, i) => <CardSkeleton key={i} />)}
      </Box>
    )
  }

  if (!discounts.length) {
    return (
      <Box sx={{ bgcolor: '#fff', border: '1px solid #eeeeee', borderRadius: '12px' }}>
        <EmptyState hasFilter={hasFilter} />
      </Box>
    )
  }

  return (
    <Box sx={{
      display: 'grid',
      gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', lg: 'repeat(3, 1fr)' },
      gap: 2.5,
      py: 1.5
    }}>
      {discounts.map((d) => (
        <DiscountCard key={d.id} discount={d} onEdit={onEdit} onDelete={onDelete} />
      ))}
    </Box>
  )
}

