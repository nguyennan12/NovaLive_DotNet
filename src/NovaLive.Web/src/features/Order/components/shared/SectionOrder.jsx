import {
  Box,
  Typography
} from '@mui/material'
import { glassSx } from '~/theme'


export const SectionTitle = ({ icon: Icon, title }) => {
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
      <Box sx={{ width: 3.5, height: 18, borderRadius: '2px', bgcolor: 'secondary.main', flexShrink: 0 }} />
      {Icon && <Icon sx={{ fontSize: 16, color: 'secondary.main' }} />}
      <Typography sx={{
        fontSize: '0.78rem', fontWeight: 800, letterSpacing: '0.06em',
        textTransform: 'uppercase', color: 'primary.contrastText'
      }}>
        {title}
      </Typography>
    </Box>
  )
}

export const SectionBox = ({ children }) => {
  return (
    <Box sx={{
      borderRadius: 2.5,
      ...glassSx,
      p: 2, mb: 2
    }}>
      {children}
    </Box>
  )
}