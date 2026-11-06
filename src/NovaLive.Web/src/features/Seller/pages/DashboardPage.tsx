import { useState, useMemo } from 'react'
import {
  Alert,
  Box,
  Button,
  Grid,
  Stack,
  ToggleButton,
  ToggleButtonGroup,
  Typography,
  useTheme
} from '@mui/material'
import TrendingUpRounded from '@mui/icons-material/TrendingUpRounded'
import ShoppingBagOutlined from '@mui/icons-material/ShoppingBagOutlined'
import AccountBalanceWalletOutlined from '@mui/icons-material/AccountBalanceWalletOutlined'
import HourglassEmptyRounded from '@mui/icons-material/HourglassEmptyRounded'
import ArrowForwardRounded from '@mui/icons-material/ArrowForwardRounded'
import { Link } from 'react-router-dom'
import {
  Area,
  AreaChart,
  CartesianGrid,
  Cell,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis
} from 'recharts'
import { useDashboard } from '../hooks/useDashboard'
import { DateRangeFilter, useDateFilter } from '../components/shared/DateRangeFilter'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { QueryState } from '../components/shared/QueryState'
import { MoneyText } from '../components/shared/MoneyText'
import { DataTable } from '../components/shared/DataTable'
import { statusLabels } from '../components/shared/StatusBadge'
import { money } from '../utils/format'

function StatKpiCard({
  title,
  value,
  icon,
  color,
  bg
}: {
  title: string
  value: React.ReactNode
  icon: React.ReactNode
  color: string
  bg: string
}) {
  return (
    <SellerCard>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1.5 }}>
        <Typography variant='body2' sx={{ color: '#64748b', fontWeight: 600 }}>
          {title}
        </Typography>
        <Box sx={{ width: 42, height: 42, borderRadius: '12px', bgcolor: bg, color, display: 'grid', placeItems: 'center' }}>
          {icon}
        </Box>
      </Box>
      <Typography component='div' variant='h5' sx={{ fontWeight: 800, color: '#0f172a', letterSpacing: '-0.02em' }}>
        {value}
      </Typography>
    </SellerCard>
  )
}

export default function DashboardPage() {
  const filter = useDateFilter()
  const query = useDashboard(filter.range)
  const data = query.data
  const theme = useTheme()
  const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#64748b']

  // Toggle chart between 'month' (default) and 'day'
  const [chartMode, setChartMode] = useState<'month' | 'day'>('month')

  // Generate 12 months for current year if monthlyRevenue is not available
  const monthlyChartData = useMemo(() => {
    if (data?.monthlyRevenue && data.monthlyRevenue.length > 0) {
      return data.monthlyRevenue
    }
    const currentMonth = new Date().getMonth() + 1
    return Array.from({ length: 12 }, (_, i) => {
      const monthNum = i + 1
      const monthKey = `Tháng ${monthNum}`
      const prefix = `-${String(monthNum).padStart(2, '0')}-`
      const pointsInMonth = data?.dailyRevenue?.filter(p => p.date.includes(prefix)) || []
      const totalRev = pointsInMonth.reduce((sum, p) => sum + Number(p.revenue), 0)
      const revenue = totalRev > 0 ? totalRev : (monthNum === currentMonth ? Number(data?.totalSalesRevenue || 0) : 0)
      return {
        month: monthKey,
        revenue,
        ordersCount: pointsInMonth.reduce((sum, p) => sum + Number(p.ordersCount), 0)
      }
    })
  }, [data])

  const chartData = (chartMode === 'month' ? monthlyChartData : (data?.dailyRevenue || [])) as any[]

  return (
    <>
      <PageHeader
        title='Tổng quan cửa hàng'
        description='Theo dõi doanh số, tồn kho và những việc cần xử lý trong thời gian thực.'
      />

      <Box sx={{ mb: 2.5 }}>
        <DateRangeFilter value={filter.dates} onChange={filter.setDates} />
      </Box>

      {!filter.valid && (
        <Alert severity='warning' sx={{ mb: 2, borderRadius: '12px' }}>
          Vui lòng chọn khoảng ngày hợp lệ để xem báo cáo.
        </Alert>
      )}

      <QueryState loading={query.isLoading} error={query.error} onRetry={query.refetch}>
        {data && (
          <Stack spacing={2.5}>
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6, xl: 3 }}>
                <StatKpiCard
                  title='Doanh thu thuần'
                  value={<MoneyText value={data.totalSalesRevenue} />}
                  icon={<TrendingUpRounded />}
                  color='#2563eb'
                  bg='#eff6ff'
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6, xl: 3 }}>
                <StatKpiCard
                  title='Số đơn trong kỳ'
                  value={data.totalOrdersCount}
                  icon={<ShoppingBagOutlined />}
                  color='#10b981'
                  bg='#f0fdf4'
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6, xl: 3 }}>
                <StatKpiCard
                  title='Ví khả dụng'
                  value={<MoneyText value={data.walletAvailableBalance} />}
                  icon={<AccountBalanceWalletOutlined />}
                  color='#7c3aed'
                  bg='#f5f3ff'
                />
              </Grid>
              <Grid size={{ xs: 12, sm: 6, xl: 3 }}>
                <StatKpiCard
                  title='Đơn chờ xử lý'
                  value={data.pendingOrdersCount}
                  icon={<HourglassEmptyRounded />}
                  color='#d97706'
                  bg='#fffbeb'
                />
              </Grid>
            </Grid>

            {!data.totalOrdersCount && (
              <Alert
                severity='info'
                sx={{ borderRadius: '12px' }}
                action={
                  <Button component={Link} to='/seller/products' size='small' variant='contained'>
                    Quản lý sản phẩm
                  </Button>
                }
              >
                Chưa có đơn hàng trong khoảng thời gian này. Bạn có thể đổi khoảng ngày hoặc bổ sung sản phẩm.
              </Alert>
            )}

            <SellerCard title='Cần xử lý ngay'>
              <Grid container spacing={2}>
                {[
                  { label: 'Đơn cần chọn kho và chuẩn bị', value: data.actions.pendingOrders, to: '/seller/orders?status=Confirmed', tone: '#f59e0b', bg: '#fffbeb' },
                  { label: 'Yêu cầu hoàn hàng sắp hết hạn', value: data.actions.expiringReturns, to: '/seller/returns?status=Pending', tone: '#ef4444', bg: '#fef2f2' },
                  { label: 'SKU sắp hết hàng', value: data.actions.lowStockSkus, to: '/seller/inventory?lowStock=true', tone: '#3b82f6', bg: '#eff6ff' }
                ].map(item => (
                  <Grid key={item.to} size={{ xs: 12, md: 4 }}>
                    <Button
                      component={Link}
                      to={item.to}
                      fullWidth
                      variant='outlined'
                      sx={{
                        p: 1.75,
                        borderRadius: '12px',
                        justifyContent: 'space-between',
                        textAlign: 'left',
                        borderColor: item.value ? item.tone : '#e2e8f0',
                        bgcolor: item.value ? item.bg : '#ffffff',
                        color: item.value ? item.tone : '#334155',
                        '&:hover': {
                          borderColor: item.tone,
                          bgcolor: item.bg
                        }
                      }}
                    >
                      <Box>
                        <Typography sx={{ fontSize: '0.82rem', fontWeight: 600 }}>{item.label}</Typography>
                        <Typography sx={{ fontSize: '1.25rem', fontWeight: 800 }}>{item.value}</Typography>
                      </Box>
                      <ArrowForwardRounded sx={{ fontSize: 18 }} />
                    </Button>
                  </Grid>
                ))}
              </Grid>
            </SellerCard>

            <Grid container spacing={2.5}>
              <Grid size={{ xs: 12, lg: 8 }}>
                <SellerCard
                  title={chartMode === 'month' ? 'Doanh thu theo tháng' : 'Doanh thu theo ngày'}
                  action={
                    <ToggleButtonGroup
                      size='small'
                      value={chartMode}
                      exclusive
                      onChange={(_, val) => val && setChartMode(val)}
                      sx={{
                        height: 30,
                        bgcolor: '#f1f5f9',
                        borderRadius: '8px',
                        p: 0.25,
                        '& .MuiToggleButton-root': {
                          px: 1.5,
                          py: 0.2,
                          fontSize: '0.75rem',
                          fontWeight: 600,
                          textTransform: 'none',
                          border: 'none',
                          borderRadius: '6px',
                          color: '#64748b',
                          '&.Mui-selected': {
                            bgcolor: '#ffffff',
                            color: '#2563eb',
                            fontWeight: 700,
                            boxShadow: '0 1px 3px rgba(0,0,0,0.1)'
                          }
                        }
                      }}
                    >
                      <ToggleButton value='month'>Theo tháng</ToggleButton>
                      <ToggleButton value='day'>Theo ngày</ToggleButton>
                    </ToggleButtonGroup>
                  }
                >
                  <QueryState
                    loading={false}
                    error={null}
                    empty={!chartData.length}
                    emptyText='Chưa có doanh thu trong kỳ đã chọn.'
                    onRetry={query.refetch}
                  >
                    <Box sx={{ height: 300, minWidth: 0, pt: 1 }}>
                      <ResponsiveContainer width='100%' height='100%'>
                        <AreaChart data={chartData}>
                          <defs>
                            <linearGradient id='colorRevenue' x1='0' y1='0' x2='0' y2='1'>
                              <stop offset='5%' stopColor='#3b82f6' stopOpacity={0.3} />
                              <stop offset='95%' stopColor='#3b82f6' stopOpacity={0} />
                            </linearGradient>
                          </defs>
                          <CartesianGrid stroke='#f1f5f9' strokeDasharray='3 3' />
                          {chartMode === 'month' ? (
                            <XAxis dataKey='month' stroke='#94a3b8' fontSize={12} />
                          ) : (
                            <XAxis
                              dataKey='date'
                              stroke='#94a3b8'
                              fontSize={12}
                              tickFormatter={value => String(value).slice(5).split('-').reverse().join('/')}
                            />
                          )}
                          <YAxis
                            width={75}
                            stroke='#94a3b8'
                            fontSize={12}
                            tickFormatter={value => `${Number(value) / 1_000}k`}
                          />
                          <Tooltip formatter={(value) => money(Number(value))} />
                          <Area
                            type='monotone'
                            dataKey='revenue'
                            name='Doanh thu'
                            stroke='#3b82f6'
                            strokeWidth={2.5}
                            fill='url(#colorRevenue)'
                          />
                        </AreaChart>
                      </ResponsiveContainer>
                    </Box>
                  </QueryState>
                </SellerCard>
              </Grid>
              <Grid size={{ xs: 12, lg: 4 }}>
                <SellerCard title='Đơn hàng theo trạng thái'>
                  <Box sx={{ height: 230 }}>
                    <ResponsiveContainer width='100%' height='100%'>
                      <PieChart>
                        <Pie
                          data={data.orderStatuses.filter(item => item.count > 0).map(item => ({ ...item, label: statusLabels[item.status] || item.status }))}
                          dataKey='count'
                          nameKey='label'
                          innerRadius={55}
                          outerRadius={85}
                          paddingAngle={3}
                        >
                          {data.orderStatuses.filter(item => item.count > 0).map((item, i) => (
                            <Cell key={item.status} fill={colors[i % colors.length]} />
                          ))}
                        </Pie>
                        <Tooltip />
                      </PieChart>
                    </ResponsiveContainer>
                  </Box>
                  <Stack spacing={0.75} sx={{ mt: 1 }}>
                    {data.orderStatuses.filter(item => item.count > 0).map((item, i) => (
                      <Box key={item.status} sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Box sx={{ width: 8, height: 8, borderRadius: '50%', bgcolor: colors[i % colors.length] }} />
                          <Typography variant='caption' sx={{ color: '#475569' }}>{statusLabels[item.status]}</Typography>
                        </Box>
                        <Typography variant='caption' sx={{ fontWeight: 700, color: '#0f172a' }}>{item.count}</Typography>
                      </Box>
                    ))}
                  </Stack>
                </SellerCard>
              </Grid>
            </Grid>

            <SellerCard title='Top 10 SKU bán chạy'>
              <DataTable
                rows={data.topSkus}
                rowKey={row => row.skuId}
                loading={false}
                error={null}
                onRetry={query.refetch}
                emptyAction={<Button component={Link} to='/seller/products' variant='outlined' size='small'>Xem sản phẩm</Button>}
                columns={[
                  { key: 'name', label: 'Sản phẩm', render: row => <Typography sx={{ fontWeight: 600, fontSize: '0.85rem' }}>{row.spuName}</Typography> },
                  { key: 'code', label: 'SKU', render: row => <Typography variant='caption' sx={{ color: '#64748b', fontFamily: 'monospace' }}>{row.skuCode}</Typography> },
                  { key: 'qty', label: 'Đã bán', render: row => <Typography sx={{ fontWeight: 700 }}>{row.soldQuantity}</Typography>, align: 'right' },
                  { key: 'revenue', label: 'Doanh thu', render: row => <Typography sx={{ fontWeight: 800, color: '#2563eb' }}><MoneyText value={row.totalRevenue} /></Typography>, align: 'right' }
                ]}
              />
            </SellerCard>

            <SellerCard
              title='Hiệu quả Livestream'
              action={<Button component={Link} to='/seller/live' size='small' variant='outlined' sx={{ borderRadius: '8px' }}>Xem phiên live</Button>}
            >
              <DataTable
                rows={data.recentLives}
                rowKey={row => row.sessionId}
                loading={false}
                error={null}
                onRetry={query.refetch}
                columns={[
                  { key: 'title', label: 'Phiên live', render: row => <Typography sx={{ fontWeight: 600 }}>{row.title}</Typography> },
                  { key: 'viewers', label: 'Đỉnh người xem', render: row => row.peakViewers },
                  { key: 'orders', label: 'Đơn hàng', render: row => row.totalOrdersCount },
                  { key: 'revenue', label: 'Doanh số', render: row => <Typography sx={{ fontWeight: 700, color: '#2563eb' }}><MoneyText value={row.totalRevenue} /></Typography> }
                ]}
              />
            </SellerCard>
          </Stack>
        )}
      </QueryState>
    </>
  )
}
