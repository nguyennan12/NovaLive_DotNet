import { Box, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material'
import { useMemo, useState } from 'react'
import { Bar, CartesianGrid, ComposedChart, Legend, Line, ResponsiveContainer, Tooltip as RTooltip, XAxis, YAxis } from 'recharts'
import SectionCard from '~/features/Inventory/components/shared/SectionCard'
import { formatVND } from '~/common/utils/formatters'
import { useLiveRevenueChart } from '../../hooks/useLiveSessions'

const PERIODS = ['week', 'month', 'all']
const PERIOD_LABELS = { week: 'Tuần', month: 'Tháng', all: 'Tất cả' }

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
        minWidth: 170
      }}
    >
      <Typography sx={{ fontSize: '0.72rem', color: '#9ca3af', mb: 1, fontWeight: 600 }}>{label}</Typography>
      {payload.map((p) => (
        <Box key={p.dataKey} sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
          <Box
            sx={{
              width: 8,
              height: 8,
              borderRadius: '2px',
              bgcolor: p.fill || p.stroke,
              flexShrink: 0
            }}
          />
          <Typography sx={{ fontSize: '0.78rem', color: '#4b5563', flex: 1 }}>
            {p.dataKey === 'revenue' ? 'Doanh thu' : 'Lượt xem'}
          </Typography>
          <Typography sx={{ fontSize: '0.78rem', fontWeight: 700, color: '#111' }}>
            {p.dataKey === 'revenue' ? formatVND(p.value) : p.value}
          </Typography>
        </Box>
      ))}
    </Box>
  )
}

const StatPill = ({ label, value, color }) => (
  <Box
    sx={{
      textAlign: 'center',
      px: { xs: 1.5, sm: 2.5 },
      borderRight: '1px solid #eeeeee',
      '&:last-child': { borderRight: 'none' }
    }}
  >
    <Typography sx={{ fontSize: { xs: '0.95rem', sm: '1.1rem' }, fontWeight: 800, color, letterSpacing: '-0.03em' }}>
      {value}
    </Typography>
    <Typography sx={{ fontSize: '0.7rem', color: '#9ca3af', fontWeight: 500 }}>{label}</Typography>
  </Box>
)

const LiveRevenueChart = () => {
  const [period, setPeriod] = useState('week')
  const { data = [] } = useLiveRevenueChart(period)

  const totals = useMemo(
    () => ({
      revenue: data.reduce((s, d) => s + (d.revenue || 0), 0),
      viewers: data.reduce((s, d) => s + (d.viewers || 0), 0),
      sessions: data.filter((d) => d.revenue > 0).length
    }),
    [data]
  )

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
          {PERIOD_LABELS[p]}
        </ToggleButton>
      ))}
    </ToggleButtonGroup>
  )

  return (
    <SectionCard
      title='Doanh Thu Theo Phiên Live'
      subtitle='Doanh thu và lượt xem qua từng buổi'
      action={periodAction}
    >
      <Box
        sx={{
          display: 'flex',
          mb: 3,
          py: 1.5,
          bgcolor: '#fafafa',
          borderRadius: '10px',
          overflow: 'hidden',
          flexWrap: 'wrap'
        }}
      >
        <StatPill label='Tổng doanh thu' value={formatVND(totals.revenue)} color='#3485f7' />
        <StatPill label='Tổng lượt xem' value={totals.viewers} color='#8b5cf6' />
        <StatPill label='Phiên có DT' value={totals.sessions} color='#10b981' />
      </Box>

      <ResponsiveContainer width='100%' height={260}>
        <ComposedChart data={data} barGap={4} barCategoryGap='35%'>
          <CartesianGrid strokeDasharray='3 3' stroke='#f3f4f6' vertical={false} />
          <XAxis
            dataKey='label'
            tick={{ fontSize: 11, fill: '#9ca3af', fontWeight: 500 }}
            axisLine={false}
            tickLine={false}
          />
          <YAxis
            yAxisId='revenue'
            tick={{ fontSize: 10, fill: '#9ca3af' }}
            axisLine={false}
            tickLine={false}
            width={56}
            tickFormatter={(v) =>
              v >= 1000000 ? `${(v / 1000000).toFixed(1)}M` : `${(v / 1000).toFixed(0)}k`
            }
          />
          <YAxis
            yAxisId='viewers'
            orientation='right'
            tick={{ fontSize: 10, fill: '#9ca3af' }}
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
                {v === 'revenue' ? 'Doanh thu' : 'Lượt xem'}
              </span>
            )}
          />
          <Bar
            yAxisId='revenue'
            dataKey='revenue'
            fill='#3485f7'
            radius={[4, 4, 0, 0]}
            name='revenue'
          />
          <Line
            yAxisId='viewers'
            type='monotone'
            dataKey='viewers'
            stroke='#8b5cf6'
            strokeWidth={2}
            dot={{ r: 3, fill: '#8b5cf6', strokeWidth: 0 }}
            name='viewers'
          />
        </ComposedChart>
      </ResponsiveContainer>
    </SectionCard>
  )
}

export default LiveRevenueChart
