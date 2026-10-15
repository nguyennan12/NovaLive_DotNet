import { Box, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { Bar, BarChart, CartesianGrid, Legend, Tooltip as RTooltip, ResponsiveContainer, XAxis, YAxis } from 'recharts'
import { getChartArtInventoryAPI } from '~/common/apis/services/inventoryService'
import SectionCard from '../shared/SectionCard'

const PERIODS = ['today', 'week', 'month']

const CustomTooltip = ({ active, payload, label }) => {
  if (!active || !payload?.length) return null
  return (
    <Box
      sx={{
        bgcolor: '#fff',
        border: '1px solid #eeeeee',
        borderRadius: '10px',
        p: 1.5,
        boxShadow: '0 4px 16px rgba(0,0,0,0.1)',
        minWidth: 140
      }}
    >
      <Typography sx={{ fontSize: '0.72rem', color: '#9ca3af', mb: 1, fontWeight: 600 }}>
        {label}
      </Typography>
      {payload.map((p) => (
        <Box key={p.dataKey} sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
          <Box sx={{ width: 8, height: 8, borderRadius: '2px', bgcolor: p.fill, flexShrink: 0 }} />
          <Typography sx={{ fontSize: '0.78rem', color: '#4b5563', flex: 1 }}>
            {p.dataKey === 'in' ? 'Stock In' : p.dataKey === 'out' ? 'Stock Out' : 'Net'}
          </Typography>
          <Typography sx={{ fontSize: '0.78rem', fontWeight: 700, color: '#111' }}>
            {p.value}
          </Typography>
        </Box>
      ))}
    </Box>
  )
}

const StatPill = ({ label, value, color }) => (
  <Box sx={{ textAlign: 'center', px: 2.5, borderRight: '1px solid #eeeeee', '&:last-child': { borderRight: 'none' } }}>
    <Typography sx={{ fontSize: '1.25rem', fontWeight: 800, color, letterSpacing: '-0.03em' }}>
      {value}
    </Typography>
    <Typography sx={{ fontSize: '0.7rem', color: '#9ca3af', fontWeight: 500 }}>{label}</Typography>
  </Box>
)

const StockOverviewChart = () => {
  const [period, setPeriod] = useState('today')

  const { data = [] } = useQuery({
    queryKey: ['chart_inventory', period],
    queryFn: () => getChartArtInventoryAPI(period)
  })
  const totals = useMemo(() => {
    const totalIn = data.reduce((s, d) => s + d.in, 0)
    const totalOut = data.reduce((s, d) => s + d.out, 0)
    return { totalIn, totalOut, net: totalIn - totalOut }
  }, [data])

  const periodAction = (
    <ToggleButtonGroup
      value={period}
      exclusive
      onChange={(_, v) => v && setPeriod(v)}
      size='small'
      sx={{
        '& .MuiToggleButton-root': {
          fontSize: '0.72rem',
          fontWeight: 600,
          px: 1.5,
          py: 0.4,
          border: '1px solid #eeeeee',
          textTransform: 'none',
          color: '#9ca3af',
          '&.Mui-selected': {
            bgcolor: '#3485f7',
            color: '#fff',
            borderColor: '#3485f7',
            '&:hover': { bgcolor: '#2471e0' }
          },
          '&:not(.Mui-selected):hover': { bgcolor: '#f9fafb' }
        }
      }}
    >
      {PERIODS.map((p) => (
        <ToggleButton key={p} value={p}>
          {p.charAt(0).toUpperCase() + p.slice(1)}
        </ToggleButton>
      ))}
    </ToggleButtonGroup>
  )

  return (
    <SectionCard title='Stock Movement Overview' subtitle='Inbound vs outbound inventory flow' action={periodAction}>
      {/* Summary pills */}
      <Box sx={{ display: 'flex', mb: 3, py: 1.5, bgcolor: '#fafafa', borderRadius: '10px', overflow: 'hidden' }}>
        <StatPill label='Total In' value={`+${totals.totalIn}`} color='#3485f7' />
        <StatPill label='Total Out' value={`-${totals.totalOut}`} color='#8b5cf6' />
        <StatPill
          label='Net Stock'
          value={totals.net >= 0 ? `+${totals.net}` : `${totals.net}`}
          color={totals.net >= 0 ? '#22c55e' : '#ef4444'}
        />
      </Box>

      {/* Chart */}
      <ResponsiveContainer width='100%' height={240}>
        <BarChart data={data} barGap={4} barCategoryGap='30%'>
          <CartesianGrid strokeDasharray='3 3' stroke='#f3f4f6' vertical={false} />
          <XAxis
            dataKey='label'
            tick={{ fontSize: 11, fill: '#9ca3af', fontWeight: 500 }}
            axisLine={false}
            tickLine={false}
          />
          <YAxis
            tick={{ fontSize: 11, fill: '#9ca3af' }}
            axisLine={false}
            tickLine={false}
            width={36}
          />
          <RTooltip content={<CustomTooltip />} cursor={{ fill: '#f3f4f6', radius: 4 }} />
          <Legend
            iconType='square'
            iconSize={8}
            formatter={(v) => (
              <span style={{ fontSize: '0.72rem', color: '#6b7280', fontWeight: 600 }}>
                {v === 'in' ? 'Stock In' : 'Stock Out'}
              </span>
            )}
          />
          <Bar dataKey='in' fill='#3485f7' radius={[4, 4, 0, 0]} name='in' />
          <Bar dataKey='out' fill='#c4b5fd' radius={[4, 4, 0, 0]} name='out' />
        </BarChart>
      </ResponsiveContainer>
    </SectionCard>
  )
}

export default StockOverviewChart