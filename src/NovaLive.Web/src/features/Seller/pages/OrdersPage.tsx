import { Alert, Box, Button, Stack, Tab, Tabs, Typography } from '@mui/material'
import { useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { useOrderCounts, useOrders } from '../hooks/useOrders'
import { ORDER_STATUSES, PAGE_SIZE } from '../constants'
import { DateRangeFilter, useDateFilter } from '../components/shared/DateRangeFilter'
import { PageHeader } from '../components/shared/PageHeader'
import { DataTable } from '../components/shared/DataTable'
import { SellerCard } from '../components/shared/SellerCard'
import { StatusBadge, statusLabels } from '../components/shared/StatusBadge'
import { MoneyText } from '../components/shared/MoneyText'
import { DateText } from '../components/shared/DateText'
import { errorMessage } from '../utils/errors'

export default function OrdersPage({ shipping = false }: { shipping?: boolean }) {
  const [params, setParams] = useSearchParams(); const requested = params.get('status') || (shipping ? 'Confirmed' : '')
  const status = ORDER_STATUSES.some(value => value === requested) ? requested : ''
  const [page, setPage] = useState(1); const filter = useDateFilter()
  const query = useOrders({ page, size: PAGE_SIZE, status: status || undefined, ...filter.range })
  const counts = useOrderCounts(filter.range)
  return <><PageHeader title={shipping ? 'Vận chuyển & Giao hàng' : 'Đơn hàng'} description='COD và đơn đã thanh toán ở trạng thái Đã xác nhận. Chọn kho trước khi giao hàng.' />
    <DateRangeFilter value={filter.dates} onChange={value => { filter.setDates(value); setPage(1) }} />
    {counts.error && <Alert severity='warning' sx={{ mb: 1 }} action={<Button onClick={() => void counts.refetch()}>Thử lại</Button>}>Chưa tải được bộ đếm: {errorMessage(counts.error)}</Alert>}
    <Box sx={{ maxWidth: '100%', mb: 2 }}><Tabs value={status} variant='scrollable' scrollButtons='auto' aria-label='Trạng thái đơn hàng' onChange={(_, value: string) => { setParams(value ? { status: value } : {}); setPage(1) }}><Tab value='' label='Tất cả' />{ORDER_STATUSES.map(value => <Tab key={value} value={value} label={`${statusLabels[value]}${counts.data ? ` (${counts.data[value]})` : ''}`} />)}</Tabs></Box>
    <SellerCard><DataTable rows={query.data?.items || []} rowKey={row => row.id} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={query.data?.totalPages} total={query.data?.total} onPage={setPage} emptyText='Chưa có đơn hàng phù hợp với bộ lọc.' emptyAction={<Button onClick={() => { setParams({}); setPage(1) }}>Xem tất cả trạng thái</Button>} columns={[
      { key: 'code', label: 'Mã đơn', render: row => <Button component={Link} to={`/seller/orders/${row.id}`} sx={{ p: 0, fontSize: '0.8rem' }}>{row.code}</Button> },
      { key: 'recipient', label: 'Người nhận', render: row => <>{row.recipient.recipientName}<Typography variant='caption' component='div'>{row.recipient.phone}</Typography></> },
      { key: 'items', label: 'Số món', render: row => row.items.reduce((sum, item) => sum + item.quantity, 0) },
      { key: 'total', label: 'Tổng tiền', render: row => <MoneyText value={row.subTotal} /> },
      { key: 'payment', label: 'Thanh toán', render: row => row.paymentMethod },
      { key: 'source', label: 'Nguồn', render: row => ({ Online: 'Trực tuyến', Livestream: 'Livestream', FlashSale: 'Flash Sale' }[row.orderSource] || row.orderSource) },
      { key: 'status', label: 'Trạng thái', render: row => <Stack spacing={0.5}><StatusBadge status={row.status} />{row.status === 'Confirmed' && <Typography variant='caption'>{row.warehouseAddressId ? 'Đã chọn kho' : 'Cần chọn kho lấy hàng'}</Typography>}</Stack> },
      { key: 'date', label: 'Thời gian', render: row => <DateText value={row.createdAt} /> }
    ]} /></SellerCard>
  </>
}
