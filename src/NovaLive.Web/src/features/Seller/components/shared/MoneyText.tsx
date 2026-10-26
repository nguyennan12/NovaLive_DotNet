import { Typography } from '@mui/material'
import { money } from '../../utils/format'

export function MoneyText({ value, signed = false }: { value: number; signed?: boolean }) {
  return <Typography component='span' variant='inherit' sx={{ whiteSpace: 'nowrap', fontWeight: 600, color: signed && value !== 0 ? value > 0 ? 'success.main' : 'error.main' : 'inherit' }}>{signed && value > 0 ? '+' : ''}{money(value)}</Typography>
}
