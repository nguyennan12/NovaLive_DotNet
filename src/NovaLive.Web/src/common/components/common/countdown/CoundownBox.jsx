import { Box, Typography } from '@mui/material'
import useCountdown from '~/common/hooks/useCountdown'

const LABELS = ['ngày', 'giờ', 'phút', 'giây']

export const FlashSaleCountdown = ({ endTime }) => {
  const time = useCountdown(endTime)
  const showDays = time.d > 0
  const values = showDays
    ? [time.d, time.h, time.m, time.s]
    : [time.h, time.m, time.s]
  const labels = showDays ? LABELS : LABELS.slice(1)

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-start', ml: { xs: 1, sm: 2 } }}>
      {/* Number boxes row */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        {values.map((v, i) => (
          <Box key={i} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Box sx={{
              bgcolor: '#1a1a1a', color: '#fff',
              fontWeight: 800, fontSize: '0.85rem',
              fontFamily: 'monospace',
              px: 0.9, py: 0.3, borderRadius: '5px',
              minWidth: 28, textAlign: 'center', letterSpacing: '0.05em'
            }}>
              {String(v).padStart(2, '0')}
            </Box>
            {i < values.length - 1 && (
              <Typography sx={{ fontWeight: 800, fontSize: { xs: '1rem', sm: '1.25rem' }, color: 'text.primary', lineHeight: 1 }}>:</Typography>
            )}
          </Box>
        ))}
      </Box>

      {/* Labels row — aligned under each box */}
      <Box sx={{ display: 'flex', gap: 0.5, mt: 0.25 }}>
        {labels.map((label, i) => (
          <Box key={i} sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <Typography sx={{ fontSize: '0.52rem', color: 'text.secondary', fontWeight: 600, minWidth: 28, textAlign: 'center', letterSpacing: '0.04em' }}>
              {label}
            </Typography>
            {/* spacer matching the colon width so labels stay aligned */}
            {i < labels.length - 1 && <Box sx={{ minWidth: 10 }} />}
          </Box>
        ))}
      </Box>
    </Box>
  )
}
