import { Box, Typography } from '@mui/material'
import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import CheckCircleOutlineRoundedIcon from '@mui/icons-material/CheckCircleOutlineRounded'
import ArchiveOutlinedIcon from '@mui/icons-material/ArchiveOutlined'

const StatCard = ({ icon: Icon, iconColor, iconBg, label, value, valueColor, sub }) => (
  <Box sx={{
    display: 'flex',
    alignItems: 'center',
    gap: 2,
    px: { xs: 1.5, sm: 2, md: 3 },
    py: { xs: 2, sm: 3, md: 4 },
    bgcolor: 'rgba(255, 255, 255, 0.8)',
    backdropFilter: 'blur(10px)',
    border: '1px solid rgba(255, 255, 255, 0.3)',
    borderRadius: '16px',
    boxShadow: '0 4px 20px rgba(0,0,0,0.03)',
    transition: 'all 0.2s ease-in-out',
    height: '100%',
    '&:hover': {
      boxShadow: '0 8px 30px rgba(0,0,0,0.06)',
      transform: 'translateY(-3px)',
      borderColor: iconColor + '44'
    }
  }}>
    <Box sx={{
      width: { xs: 44, sm: 48, md: 56 },
      height: { xs: 44, sm: 48, md: 56 },
      borderRadius: '14px',
      bgcolor: iconBg,
      flexShrink: 0,
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      boxShadow: `0 8px 16px ${iconColor}15`
    }}>
      <Icon sx={{ fontSize: { xs: 22, sm: 24, md: 28 }, color: iconColor }} />
    </Box>
    <Box>
      <Typography sx={{ fontSize: '0.85rem', fontWeight: 600, color: '#64748b', mb: 0.5 }}>
        {label}
      </Typography>
      <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 0.5 }}>
        <Typography sx={{ fontSize: { xs: '1.25rem', sm: '1.4rem', md: '1.75rem' }, fontWeight: 800, color: valueColor || '#1e293b', letterSpacing: '-0.03em' }}>
          {value}
        </Typography>
        {sub && (
          <Typography sx={{ fontSize: '0.75rem', color: '#94a3b8', fontWeight: 500 }}>
            {sub}
          </Typography>
        )}
      </Box>
    </Box>
  </Box>
)

export const DiscountStat = ({ stats }) => (
  // Bỏ marginBottom nếu bạn đã quản lý khoảng cách ở component cha
  <Box>
    {/* Sử dụng CSS Grid để ép cứng layout 2x2 */}
    <Box sx={{
      display: 'grid',
      gridTemplateColumns: {
        xs: 'repeat(2, 1fr)',
        sm: 'repeat(4, 1fr)',
        lg: 'repeat(2, 1fr)'
      },
      gap: 2 // Giảm gap một chút để 4 card dễ fit hơn
    }}>
      <StatCard
        icon={LocalOfferRoundedIcon}
        iconColor='#3b82f6' iconBg='#eff6ff'
        label='Tổng discount' value={stats.total} sub='mã'
        valueColor='#3b82f6'
      />
      <StatCard
        icon={LocalShippingRoundedIcon}
        iconColor='#f59e0b' iconBg='#fffaf4'
        label='Free Ship' value={stats.freeship} sub='ưu đãi'
        valueColor='#f59e0b'
      />
      <StatCard
        icon={CheckCircleOutlineRoundedIcon}
        iconColor='#10b981' iconBg='#ecfdf5'
        label='Đang chạy' value={stats.active} sub='active'
        valueColor='#10b981'
      />
      <StatCard
        icon={ArchiveOutlinedIcon}
        iconColor='#f43f5e' iconBg='#fff1f2'
        label='Hết hạn' value={stats.expired} sub='mã'
        valueColor='#f43f5e'
      />
    </Box>
  </Box>
)