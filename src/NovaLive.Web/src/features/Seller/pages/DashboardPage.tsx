import { Alert, Box, Button, Grid, Stack, Typography, useTheme } from '@mui/material'
import { Link } from 'react-router-dom'
import { Area, AreaChart, CartesianGrid, Cell, Pie, PieChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { useDashboard } from '../hooks/useDashboard'
import { DateRangeFilter, useDateFilter } from '../components/shared/DateRangeFilter'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { QueryState } from '../components/shared/QueryState'
import { MoneyText } from '../components/shared/MoneyText'
import { DataTable } from '../components/shared/DataTable'
import { statusLabels } from '../components/shared/StatusBadge'
import { money } from '../utils/format'

export default function DashboardPage() {
  const filter = useDateFilter()
  const query = useDashboard(filter.range)
  const data = query.data
  const theme = useTheme()
  const colors = [theme.palette.secondary.main, theme.palette.success.main, theme.palette.warning.main, theme.palette.error.main, theme.palette.info.main, theme.palette.text.secondary]
  return <>
    <PageHeader title='Tổng quan cửa hàng' description='Theo dõi doanh số, tồn kho và những việc cần xử lý.' />
    <DateRangeFilter value={filter.dates} onChange={filter.setDates} />
    {!filter.valid && <Alert severity='warning' sx={{ mb: 2 }}>Vui lòng chọn khoảng ngày hợp lệ để xem báo cáo.</Alert>}
    <QueryState loading={query.isLoading} error={query.error} onRetry={query.refetch}>
      {data && <Stack spacing={2.5}>
        <Grid container spacing={2}>{[
          { label: 'Doanh thu thuần', value: <MoneyText value={data.totalSalesRevenue} /> },
          { label: 'Số đơn trong kỳ', value: data.totalOrdersCount },
          { label: 'Ví khả dụng', value: <MoneyText value={data.walletAvailableBalance} /> },
          { label: 'Đơn chờ xử lý', value: data.pendingOrdersCount }
        ].map(item => <Grid key={item.label} size={{ xs: 12, sm: 6, xl: 3 }}><SellerCard><Typography color='text.secondary'>{item.label}</Typography><Typography component='div' variant='h5' sx={{ fontWeight: 800, mt: 1 }}>{item.value}</Typography></SellerCard></Grid>)}</Grid>
        {!data.totalOrdersCount && <Alert severity='info' action={<Button component={Link} to='/seller/products'>Quản lý sản phẩm</Button>}>Chưa có đơn hàng trong khoảng thời gian này. Bạn có thể đổi khoảng ngày hoặc bổ sung sản phẩm.</Alert>}
        <SellerCard title='Cần xử lý ngay'><Grid container spacing={2}>{[
          { label: 'Đơn cần chọn kho và chuẩn bị', value: data.actions.pendingOrders, to: '/seller/orders?status=Confirmed' },
          { label: 'Yêu cầu hoàn hàng sắp hết hạn', value: data.actions.expiringReturns, to: '/seller/returns?status=Pending' },
          { label: 'SKU sắp hết hàng', value: data.actions.lowStockSkus, to: '/seller/inventory?lowStock=true' }
        ].map(item => <Grid key={item.to} size={{ xs: 12, md: 4 }}><Button component={Link} to={item.to} fullWidth variant='outlined' color={item.value ? 'warning' : 'secondary'} sx={{ justifyContent: 'space-between', textAlign: 'left', gap: 1 }}>{item.label}<strong>{item.value}</strong></Button></Grid>)}</Grid></SellerCard>
        <Grid container spacing={2.5}>
          <Grid size={{ xs: 12, lg: 8 }}><SellerCard title='Doanh thu theo ngày'><QueryState loading={false} error={null} empty={!data.dailyRevenue.length} emptyText='Chưa có doanh thu trong kỳ đã chọn.' onRetry={query.refetch}><Box sx={{ height: 290, minWidth: 0 }}><ResponsiveContainer width='100%' height='100%'><AreaChart data={data.dailyRevenue}><CartesianGrid stroke={theme.palette.divider} strokeDasharray='3 3' /><XAxis dataKey='date' tickFormatter={value => String(value).slice(5).split('-').reverse().join('/')} /><YAxis width={75} tickFormatter={value => `${Number(value) / 1_000}k`} /><Tooltip formatter={value => money(Number(value))} /><Area type='monotone' dataKey='revenue' name='Doanh thu' stroke={theme.palette.secondary.main} fill={theme.palette.secondary.main} fillOpacity={0.15} /></AreaChart></ResponsiveContainer></Box></QueryState></SellerCard></Grid>
          <Grid size={{ xs: 12, lg: 4 }}><SellerCard title='Đơn hàng theo trạng thái'><Box sx={{ height: 230 }}><ResponsiveContainer width='100%' height='100%'><PieChart><Pie data={data.orderStatuses.filter(item => item.count > 0).map(item => ({ ...item, label: statusLabels[item.status] || item.status }))} dataKey='count' nameKey='label' innerRadius={55} outerRadius={85}>{data.orderStatuses.filter(item => item.count > 0).map((item, i) => <Cell key={item.status} fill={colors[i % colors.length]} />)}</Pie><Tooltip /></PieChart></ResponsiveContainer></Box><Stack spacing={0.5}>{data.orderStatuses.filter(item => item.count > 0).map(item => <Box key={item.status} sx={{ display: 'flex', justifyContent: 'space-between' }}><Typography variant='caption'>{statusLabels[item.status]}</Typography><Typography variant='caption' sx={{ fontWeight: 700 }}>{item.count}</Typography></Box>)}</Stack></SellerCard></Grid>
        </Grid>
        <SellerCard title='Top 10 SKU bán chạy'><DataTable rows={data.topSkus} rowKey={row => row.skuId} loading={false} error={null} onRetry={query.refetch} emptyAction={<Button component={Link} to='/seller/products'>Xem sản phẩm</Button>} columns={[
          { key: 'name', label: 'Sản phẩm', render: row => row.spuName }, { key: 'code', label: 'SKU', render: row => row.skuCode }, { key: 'qty', label: 'Đã bán', render: row => row.soldQuantity, align: 'right' }, { key: 'revenue', label: 'Doanh thu', render: row => <MoneyText value={row.totalRevenue} />, align: 'right' }
        ]} /></SellerCard>
        <SellerCard title='Hiệu quả Livestream' action={<Button component={Link} to='/seller/live'>Xem phiên live</Button>}><DataTable rows={data.recentLives} rowKey={row => row.sessionId} loading={false} error={null} onRetry={query.refetch} columns={[
          { key: 'title', label: 'Phiên live', render: row => row.title }, { key: 'viewers', label: 'Đỉnh người xem', render: row => row.peakViewers }, { key: 'orders', label: 'Đơn hàng', render: row => row.totalOrdersCount }, { key: 'revenue', label: 'Doanh số', render: row => <MoneyText value={row.totalRevenue} /> }
        ]} /></SellerCard>
      </Stack>}
    </QueryState>
  </>
}
