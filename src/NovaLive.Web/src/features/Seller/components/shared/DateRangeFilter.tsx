import { Box, MenuItem, TextField } from '@mui/material'
import { useState } from 'react'
import type { DateRange } from '../../types/contracts'
import { dateInput, dayToUtc } from '../../utils/format'

export function useDateFilter() {
  const today = dateInput()
  const initialFrom = `${today.substring(0, 7)}-01`
  const [dates, setDates] = useState({ from: initialFrom, to: today })
  const valid = !dates.from || !dates.to || dates.from <= dates.to
  const range: DateRange = valid ? { from: dayToUtc(dates.from), to: dayToUtc(dates.to, true) } : {}
  return { dates, setDates, range, valid }
}

export function DateRangeFilter({
  value,
  onChange
}: {
  value: { from: string; to: string }
  onChange: (value: { from: string; to: string }) => void
}) {
  const [preset, setPreset] = useState('month')
  const invalid = Boolean(value.from && value.to && value.from > value.to)

  const applyPreset = (key: string) => {
    setPreset(key)
    if (key === 'custom') return
    const to = dateInput()
    if (key === 'month') {
      onChange({ from: `${to.substring(0, 7)}-01`, to })
      return
    }
    const from = new Date(`${to}T00:00:00+07:00`)
    from.setUTCDate(from.getUTCDate() - Number(key) + 1)
    onChange({ from: dateInput(from), to })
  }

  return (
    <Box sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap', mb: 2 }}>
      <TextField
        select
        label='Khoảng thời gian'
        size='small'
        value={preset}
        onChange={event => applyPreset(event.target.value)}
        sx={{ minWidth: 155 }}
      >
        {[
          ['month', 'Tháng này'],
          ['1', 'Hôm nay'],
          ['7', '7 ngày qua'],
          ['30', '30 ngày qua'],
          ['custom', 'Tùy chọn']
        ].map(([key, label]) => (
          <MenuItem key={key} value={key}>
            {label}
          </MenuItem>
        ))}
      </TextField>
      <TextField
        type='date'
        label='Từ ngày'
        size='small'
        value={value.from}
        slotProps={{ inputLabel: { shrink: true } }}
        onChange={event => {
          setPreset('custom')
          onChange({ ...value, from: event.target.value })
        }}
      />
      <TextField
        type='date'
        label='Đến ngày'
        size='small'
        error={invalid}
        helperText={invalid ? 'Ngày kết thúc phải từ ngày bắt đầu trở đi.' : undefined}
        value={value.to}
        slotProps={{ inputLabel: { shrink: true } }}
        onChange={event => {
          setPreset('custom')
          onChange({ ...value, to: event.target.value })
        }}
      />
    </Box>
  )
}
