import { Box, MenuItem, TextField } from '@mui/material'
import { useState } from 'react'
import type { DateRange } from '../../types/contracts'
import { dayToUtc, todayInput } from '../../utils/format'

export function useDateFilter() {
  const today = todayInput()
  const start = new Date(`${today}T00:00:00+07:00`); start.setUTCDate(start.getUTCDate() - 29)
  const initialFrom = new Intl.DateTimeFormat('sv-SE', { timeZone: 'Asia/Ho_Chi_Minh' }).format(start)
  const [dates, setDates] = useState({ from: initialFrom, to: today })
  const valid = !dates.from || !dates.to || dates.from <= dates.to
  const range: DateRange = valid ? { from: dayToUtc(dates.from), to: dayToUtc(dates.to, true) } : {}
  return { dates, setDates, range, valid }
}
export function DateRangeFilter({ value, onChange }: { value: { from: string; to: string }; onChange: (value: { from: string; to: string }) => void }) {
  const [preset, setPreset] = useState('30')
  const invalid = Boolean(value.from && value.to && value.from > value.to)
  const applyPreset = (days: string) => {
    setPreset(days)
    if (days === 'custom') return
    const to = todayInput(); const from = new Date(`${to}T00:00:00+07:00`)
    from.setUTCDate(from.getUTCDate() - Number(days) + 1)
    onChange({ from: new Intl.DateTimeFormat('sv-SE', { timeZone: 'Asia/Ho_Chi_Minh' }).format(from), to })
  }
  return <Box sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap', mb: 2 }}>
    <TextField select label='Khoảng thời gian' size='small' value={preset} onChange={event => applyPreset(event.target.value)} sx={{ minWidth: 155 }}>{[['1', 'Hôm nay'], ['7', '7 ngày'], ['30', '30 ngày'], ['custom', 'Tùy chọn']].map(([key, label]) => <MenuItem key={key} value={key}>{label}</MenuItem>)}</TextField>
    <TextField type='date' label='Từ ngày' size='small' value={value.from} slotProps={{ inputLabel: { shrink: true } }} onChange={event => { setPreset('custom'); onChange({ ...value, from: event.target.value }) }} />
    <TextField type='date' label='Đến ngày' size='small' error={invalid} helperText={invalid ? 'Ngày kết thúc phải từ ngày bắt đầu trở đi.' : undefined} value={value.to} slotProps={{ inputLabel: { shrink: true } }} onChange={event => { setPreset('custom'); onChange({ ...value, to: event.target.value }) }} />
  </Box>
}
