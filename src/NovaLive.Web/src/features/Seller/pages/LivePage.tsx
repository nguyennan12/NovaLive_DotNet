import { Button, Grid, Stack, Typography } from '@mui/material'
import { useState } from 'react'
import { useSellerLives } from '../hooks/useDashboard'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { DataTable } from '../components/shared/DataTable'
import { DateText } from '../components/shared/DateText'
import { MoneyText } from '../components/shared/MoneyText'
import { StatusBadge } from '../components/shared/StatusBadge'
import { PAGE_SIZE } from '../constants'
import { Link } from 'react-router-dom'

export default function LivePage() {
  const [page, setPage] = useState(1); const query = useSellerLives({ page, size: PAGE_SIZE })
  const rows = query.data?.items || []
  return <><PageHeader title='Livestream của Shop' description='Danh sách phiên live, lượng người xem và hiệu quả bán hàng.' /><Stack spacing={3}>
    {query.data && <Grid container spacing={2}><Grid size={{ xs: 12, md: 4 }}><SellerCard title='Phiên live trên trang'><Typography variant='h4'>{rows.length}</Typography></SellerCard></Grid><Grid size={{ xs: 12, md: 4 }}><SellerCard title='Đỉnh người xem trên trang'><Typography variant='h4'>{Math.max(0, ...rows.map(item => item.peakViewers))}</Typography></SellerCard></Grid><Grid size={{ xs: 12, md: 4 }}><SellerCard title='Doanh số các phiên trên trang'><Typography component='div' variant='h4'><MoneyText value={rows.reduce((sum, item) => sum + item.totalRevenue, 0)} /></Typography></SellerCard></Grid></Grid>}
    <SellerCard title='Phiên livestream'><DataTable rows={rows} rowKey={row => row.id} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={query.data?.totalPages} total={query.data?.total} onPage={setPage} emptyText='Shop chưa có phiên live trong danh sách.' emptyAction={<Button component={Link} to='/seller/products'>Chuẩn bị sản phẩm cho phiên live</Button>} columns={[
      { key: 'title', label: 'Phiên live', render: row => row.title }, { key: 'status', label: 'Trạng thái', render: row => <StatusBadge status={row.status} /> }, { key: 'peak', label: 'Đỉnh người xem', render: row => row.peakViewers }, { key: 'orders', label: 'Đơn hàng', render: row => row.totalOrdersCount }, { key: 'revenue', label: 'Doanh số', render: row => <MoneyText value={row.totalRevenue} /> }, { key: 'start', label: 'Bắt đầu', render: row => <DateText value={row.startedAt} /> }, { key: 'end', label: 'Kết thúc', render: row => <DateText value={row.endedAt} /> }
    ]} /></SellerCard>
  </Stack></>
}
