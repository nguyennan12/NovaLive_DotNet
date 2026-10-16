import { Box, Typography } from '@mui/material'
const SectionCard = ({ title, subtitle, action, children, sx = {} }) => (
  <Box
    sx={{
      bgcolor: '#fff',
      border: '1px solid #eeeeee',
      borderRadius: '12px',
      overflow: 'hidden',
      boxShadow: '0 1px 4px rgba(0,0,0,0.04)',
      transition: 'box-shadow 0.18s',
      '&:hover': { boxShadow: '0 4px 16px rgba(0,0,0,0.07)' },
      ...sx
    }}
  >
    {(title || action) && (
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          px: 2.5,
          py: 2,
          borderBottom: '1px solid #eeeeee'
        }}
      >
        <Box>
          {title && (
            <Typography sx={{ fontSize: '0.875rem', fontWeight: 700, color: '#2d2d2d', letterSpacing: '-0.01em' }}>
              {title}
            </Typography>
          )}
          {subtitle && (
            <Typography sx={{ fontSize: '0.72rem', color: '#9ca3af', mt: 0.2 }}>
              {subtitle}
            </Typography>
          )}
        </Box>
        {action && <Box>{action}</Box>}
      </Box>
    )}
    <Box sx={{ p: 2.5 }}>{children}</Box>
  </Box>
)

export default SectionCard