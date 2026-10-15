import { Chip } from '@mui/material'

const STATUS_CONFIG = {
  scheduled: { label: 'scheduled', bg: '#f0fbfdff', text: '#2c8ae8ff' },
  live: { label: 'live', bg: 'rgba(203, 255, 182, 0.4)', text: '#33a203ff' },
  ended: { label: 'ended', bg: '#fdf8f0ff', text: '#f1aa27ff' },
  cancelled: { label: 'cancelled', bg: '#fdf0f0ff', text: '#e82f2cff' }
}

const LiveStatusBadge = ({ status, size = 'small' }) => {
  const cfg = STATUS_CONFIG[status] || STATUS_CONFIG.scheduled
  return (
    <Chip
      label={cfg.label}
      size={size}
      sx={{
        p: 1,
        bgcolor: cfg.bg,
        color: cfg.text,
        fontWeight: 600,
        fontSize: '0.7rem',
        ...(status === 'live' && {
          animation: 'livePulse 1.5s ease-in-out infinite',
          '@keyframes livePulse': {
            '0%, 100%': { opacity: 1 },
            '50%': { opacity: 0.55 }
          }
        })
      }}
    />
  )
}

export default LiveStatusBadge
