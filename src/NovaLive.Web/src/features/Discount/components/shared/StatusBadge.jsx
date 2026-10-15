import { Box, Typography } from '@mui/material'

const CFG = {
  active: { label: 'Active', dot: '#22c55e', bg: '#f0fdf4', text: '#15803d' },
  draft: { label: 'Draft', dot: '#9ca3af', bg: '#f9fafb', text: '#6b7280' },
  expired: { label: 'Expired', dot: '#ef4444', bg: '#fef2f2', text: '#b91c1c' }
}

export const StatusBadge = ({ status }) => {
  const c = CFG[status] || CFG['draft']
  return (
    <Box sx={{ display: 'inline-flex', alignItems: 'center', gap: 0.5, px: 1, py: 0.3, borderRadius: '6px', bgcolor: c.bg }}>
      <Box sx={{ width: 6, height: 6, borderRadius: '50%', bgcolor: c.dot }} />
      <Typography sx={{ fontSize: '0.68rem', fontWeight: 700, color: c.text, lineHeight: 1 }}>
        {c.label}
      </Typography>
    </Box>
  )
}

