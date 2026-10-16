import { Box, Grid, Skeleton, Typography } from '@mui/material'
import AttachMoneyRoundedIcon from '@mui/icons-material/AttachMoneyRounded'
import LiveTvRoundedIcon from '@mui/icons-material/LiveTvRounded'
import PeopleRoundedIcon from '@mui/icons-material/PeopleRounded'
import ShoppingCartRoundedIcon from '@mui/icons-material/ShoppingCartRounded'
import { formatVND } from '~/common/utils/formatters'
import { useLiveStats } from '../../hooks/useLiveSessions'

const StatCard = ({ icon: Icon, iconColor, iconBg, label, value, sub }) => (
  <Box
    sx={{
      display: 'flex',
      alignItems: 'center',
      gap: 2,
      px: { xs: 1.5, sm: 2, md: 3 },
      py: { xs: 2, sm: 3 },
      bgcolor: 'rgba(255,255,255,0.8)',
      backdropFilter: 'blur(10px)',
      border: '1px solid rgba(255,255,255,0.3)',
      borderRadius: '16px',
      boxShadow: '0 4px 20px rgba(0,0,0,0.03)',
      transition: 'all 0.2s ease-in-out',
      height: '100%',
      '&:hover': {
        boxShadow: '0 8px 30px rgba(0,0,0,0.06)',
        transform: 'translateY(-3px)',
        borderColor: iconColor + '44'
      }
    }}
  >
    <Box
      sx={{
        width: { xs: 44, sm: 48, md: 56 },
        height: { xs: 44, sm: 48, md: 56 },
        borderRadius: '14px',
        bgcolor: iconBg,
        flexShrink: 0,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        boxShadow: `0 8px 16px ${iconColor}15`
      }}
    >
      <Icon sx={{ fontSize: { xs: 22, sm: 24, md: 28 }, color: iconColor }} />
    </Box>
    <Box>
      <Typography sx={{ fontSize: '0.85rem', fontWeight: 600, color: '#64748b', mb: 0.5 }}>
        {label}
      </Typography>
      <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 0.5 }}>
        <Typography
          sx={{
            fontSize: { xs: '1.25rem', sm: '1.4rem', md: '1.75rem' },
            fontWeight: 800,
            color: '#1e293b',
            letterSpacing: '-0.03em'
          }}
        >
          {value}
        </Typography>
        {sub && (
          <Typography sx={{ fontSize: '0.75rem', color: '#94a3b8', fontWeight: 500 }}>{sub}</Typography>
        )}
      </Box>
    </Box>
  </Box>
)

const LiveStatsBlock = () => {
  const { data: stats, isLoading } = useLiveStats()

  if (isLoading) {
    return (
      <Grid container spacing={2}>
        {Array.from({ length: 4 }).map((_, i) => (
          <Grid key={i} size={{ xs: 12, sm: 6, lg: 3 }}>
            <Skeleton variant='rectangular' height={100} sx={{ borderRadius: '16px' }} />
          </Grid>
        ))}
      </Grid>
    )
  }

  if (!stats) return null

  const cards = [
    {
      icon: LiveTvRoundedIcon,
      iconColor: '#3485f7',
      iconBg: '#eff6ff',
      label: 'Tổng Phiên Live',
      value: stats.total_sessions,
      sub: 'phiên'
    },
    {
      icon: AttachMoneyRoundedIcon,
      iconColor: '#10b981',
      iconBg: '#ecfdf5',
      label: 'Doanh Thu Live',
      value: formatVND(stats.total_revenue),
      sub: ''
    },
    {
      icon: PeopleRoundedIcon,
      iconColor: '#8b5cf6',
      iconBg: '#f5f3ff',
      label: 'Lượt Xem TB',
      value: stats.avg_viewers,
      sub: 'người/phiên'
    },
    {
      icon: ShoppingCartRoundedIcon,
      iconColor: '#f59e0b',
      iconBg: '#fffaf4',
      label: 'Giá Trị ĐH TB',
      value: formatVND(stats.avg_order_value),
      sub: ''
    }
  ]

  return (
    <Grid container spacing={2}>
      {cards.map((c, i) => (
        <Grid key={i} size={{ xs: 12, sm: 6, lg: 3 }}>
          <StatCard {...c} />
        </Grid>
      ))}
    </Grid>
  )
}

export default LiveStatsBlock
