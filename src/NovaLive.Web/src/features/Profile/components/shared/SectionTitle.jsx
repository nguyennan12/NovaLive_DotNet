import { Box, Typography } from '@mui/material'

// Accent bar + section title — consistent with STYLE.md section header pattern
const SectionTitle = ({ title, icon: Icon, accentColor = 'secondary.main' }) => (
  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
    <Box sx={{ width: 3.5, height: 22, borderRadius: '2px', bgcolor: accentColor, flexShrink: 0 }} />
    {Icon && <Icon sx={{ fontSize: 18, color: accentColor }} />}
    <Typography sx={{ fontWeight: 700, fontSize: '0.95rem', color: 'primary.contrastText' }}>
      {title}
    </Typography>
  </Box>
)

export default SectionTitle
