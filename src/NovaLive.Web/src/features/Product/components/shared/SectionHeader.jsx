import { Box, Typography } from '@mui/material'
import { gradientText } from '~/theme'

export const SectionHeader = ({ title }) => (
  <Box sx={{
    display: 'flex', alignItems: 'center',
    gap: { xs: 1.5, sm: 2 }, p: 1, borderRadius: '8px'
  }}>
    <Box sx={{
      width: { xs: 4, sm: 6 },
      height: { xs: 28, sm: 36 },
      borderRadius: '4px',
      bgcolor: '#0095ffff',
      flexShrink: 0
    }} />

    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
      <Typography sx={{
        fontSize: { xs: '1.5rem', sm: '2.5rem' },
        fontWeight: 900,
        ...gradientText,
        letterSpacing: '-0.02em', lineHeight: 1
      }}>{title}</Typography>

    </Box>
  </Box>
)