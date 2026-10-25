import type { ReactNode } from 'react'
import { Box, Paper, Typography } from '@mui/material'

export function SellerCard({ children, title, action }: { children: ReactNode; title?: string; action?: ReactNode }) {
  return <Paper elevation={0} sx={{ border: 1, borderColor: 'divider', borderRadius: 3, p: { xs: 2, md: 2.5 }, minWidth: 0 }}>
    {(title || action) && <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, alignItems: 'center', mb: 2 }}><Typography component='h2' sx={{ fontWeight: 700 }}>{title}</Typography>{action}</Box>}{children}
  </Paper>
}
