import AddRoundedIcon from '@mui/icons-material/AddRounded'
import RemoveRoundedIcon from '@mui/icons-material/RemoveRounded'
import { Box, IconButton, Typography } from '@mui/material'

export const QuantityStepper = ({ value, onChange, disabled }) => {
  return (
    <Box sx={{
      display: 'flex', alignItems: 'center',
      border: '1px solid', borderColor: 'divider',
      borderRadius: '8px', overflow: 'hidden', height: 32
    }}>
      <IconButton
        size="small"
        onClick={() => onChange(value - 1)}
        disabled={disabled || value <= 1}
        sx={{ borderRadius: 0, px: 0.75, '&:hover': { bgcolor: 'rgba(52,133,247,0.07)' } }}
      >
        <RemoveRoundedIcon sx={{ fontSize: 16 }} />
      </IconButton>

      <Typography sx={{
        width: 32, textAlign: 'center', fontSize: '0.85rem',
        fontWeight: 700, color: 'primary.contrastText', userSelect: 'none'
      }}>
        {value}
      </Typography>

      <IconButton
        size="small"
        onClick={() => onChange(value + 1)}
        disabled={disabled}
        sx={{ borderRadius: 0, px: 0.75, '&:hover': { bgcolor: 'rgba(52,133,247,0.07)' } }}
      >
        <AddRoundedIcon sx={{ fontSize: 16 }} />
      </IconButton>
    </Box>
  )
}