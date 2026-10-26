import type { ReactNode } from 'react'
import { Box, Typography } from '@mui/material'

export function PageHeader({ title, description, action }: { title: string; description?: string; action?: ReactNode }) {
  return <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 2, flexWrap: 'wrap', mb: 3 }}>
    <Box><Typography variant='h5' component='h1' sx={{ fontWeight: 800, color: 'primary.contrastText' }}>{title}</Typography>{description && <Typography color='text.secondary' sx={{ mt: 0.5 }}>{description}</Typography>}</Box>
    {action}
  </Box>
}
