import type { ReactNode } from 'react'
import { Box, Paper, Typography } from '@mui/material'

export function SellerCard({ children, title, action }: { children: ReactNode; title?: string; action?: ReactNode }) {
  return (
    <Paper
      elevation={0}
      sx={{
        bgcolor: '#ffffff',
        border: '1px solid #eef2f6',
        borderRadius: '16px',
        boxShadow: '0 4px 20px -2px rgba(15, 23, 42, 0.04)',
        p: { xs: 2, md: 2.5 },
        minWidth: 0,
        transition: 'box-shadow 0.2s ease, border-color 0.2s ease',
        '&:hover': {
          borderColor: '#e2e8f0',
          boxShadow: '0 8px 30px -4px rgba(15, 23, 42, 0.07)'
        }
      }}
    >
      {(title || action) && (
        <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2, alignItems: 'center', mb: 2 }}>
          <Typography component='h2' sx={{ fontWeight: 700, fontSize: '1.05rem', color: '#1e293b' }}>
            {title}
          </Typography>
          {action}
        </Box>
      )}
      {children}
    </Paper>
  )
}
