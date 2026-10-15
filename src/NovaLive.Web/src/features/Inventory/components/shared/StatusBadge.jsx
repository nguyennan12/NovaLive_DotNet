import { Box, Typography } from '@mui/material'

const STATUS_MAP = {
  in_stock: { label: 'In Stock', dot: '#22c55e', bg: '#f0fdf4', text: '#15803d' },
  low_stock: { label: 'Low Stock', dot: '#f59e0b', bg: '#fffbeb', text: '#b45309' },
  out: { label: 'Out of Stock', dot: '#ef4444', bg: '#fef2f2', text: '#b91c1c' },
  IN: { label: 'Stock In', dot: '#3485f7', bg: '#eff6ff', text: '#1d4ed8' },
  OUT: { label: 'Stock Out', dot: '#8b5cf6', bg: '#f5f3ff', text: '#6d28d9' },
  reserved: { label: 'Reserved', dot: '#f59e0b', bg: '#fffbeb', text: '#b45309' }
}

const getStockStatus = (qty) => {
  if (qty <= 0) return 'out'
  if (qty <= 10) return 'low_stock'
  return 'in_stock'
}

const StatusBadge = ({ type, qty }) => {
  const key = type || getStockStatus(qty)
  const cfg = STATUS_MAP[key] || STATUS_MAP['in_stock']

  return (
    <Box
      sx={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 0.6,
        px: 1,
        py: 0.35,
        borderRadius: '6px',
        bgcolor: cfg.bg
      }}
    >
      <Box sx={{ width: 6, height: 6, borderRadius: '50%', bgcolor: cfg.dot, flexShrink: 0 }} />
      <Typography sx={{ fontSize: '0.7rem', fontWeight: 600, color: cfg.text, lineHeight: 1, whiteSpace: 'nowrap' }}>
        {cfg.label}
      </Typography>
    </Box>
  )
}

export default StatusBadge