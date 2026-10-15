import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { gradientText } from '~/theme'
import { authLogoBoxSx } from '../authStyles'

function AuthHeader({ title, subtitle }) {
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mb: 3 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
        <Box sx={authLogoBoxSx}>
          <PlayArrowIcon sx={{ color: '#fff', fontSize: '1.4rem' }} />
        </Box>
        <Typography
          variant="h6"
          sx={{
            fontWeight: 800,
            ...gradientText,
            letterSpacing: '-0.4px',
            lineHeight: 1
          }}
        >
          NovaLive
        </Typography>
      </Box>
      <Typography variant="h5" sx={{ fontWeight: 800, color: '#0f172a', letterSpacing: '-0.025em', mt: 0.5 }}>
        {title}
      </Typography>
      {subtitle && (
        <Typography variant="body2" sx={{ color: '#64748b', mt: 0.5, fontSize: '0.875rem', textAlign: 'center' }}>
          {subtitle}
        </Typography>
      )}
    </Box>
  )
}

export default AuthHeader
