import { Box, Typography } from '@mui/material'
import InboxRoundedIcon from '@mui/icons-material/InboxRounded'

const EmptyState = ({ icon: Icon = InboxRoundedIcon, title = 'No data', subtitle = '' }) => (
  <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', py: 5, gap: 1 }}>
    <Box
      sx={{
        width: 52,
        height: 52,
        borderRadius: '14px',
        bgcolor: '#f3f4f6',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        mb: 0.5
      }}
    >
      <Icon sx={{ fontSize: 26, color: '#9ca3af' }} />
    </Box>
    <Typography sx={{ fontSize: '0.875rem', fontWeight: 600, color: '#6b7280' }}>{title}</Typography>
    {subtitle && <Typography sx={{ fontSize: '0.75rem', color: '#9ca3af' }}>{subtitle}</Typography>}
  </Box>
)

export default EmptyState