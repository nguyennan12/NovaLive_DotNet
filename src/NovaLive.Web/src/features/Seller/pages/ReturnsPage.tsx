import { Box, Button, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useReturns } from '../hooks/useReturns'
import { PAGE_SIZE, RETURN_STATUSES } from '../constants'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { DataTable } from '../components/shared/DataTable'
import { DateText } from '../components/shared/DateText'
import { MoneyText } from '../components/shared/MoneyText'
import { StatusBadge } from '../components/shared/StatusBadge'
import { ReturnCountdown } from '../components/returns/ReturnCountdown'
import { ReturnDetailDialog } from '../components/returns/ReturnDetailDialog'
import { returnReasons } from '../utils/returns'

export default function ReturnsPage() {
  const [params, setParams] = useSearchParams(); const [page, setPage] = useState(1); const [id, setId] = useState<string | null>(null)
  const status = params.get('status') || ''
  const query = useReturns({ page, size: PAGE_SIZE, status: RETURN_STATUSES.some(value => value === status) ? status : undefined })
  const item = query.data?.items.find(item => item.id === id)
  return <><PageHeader title='Trả hàng / Hoàn tiền' description='Phản hồi trong 3 ngày. Yêu cầu đang phân xử chỉ được xem.' />
    <Box sx={{ mb: 2 }}><TextField select label='Trạng thái yêu cầu' value={status} onChange={event => { setParams(event.target.value ? { status: event.target.value } : {}); setPage(1) }} size='small' sx={{ minWidth: 240 }}><MenuItem value=''>Tất cả</MenuItem>{RETURN_STATUSES.map(value => <MenuItem key={value} value={value}><StatusBadge status={value} /></MenuItem>)}</TextField></Box>
    <SellerCard><DataTable rows={query.data?.items || []} rowKey={row => row.id} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={query.data?.totalPages} total={query.data?.total} onPage={value => { setPage(value); setId(null) }} emptyText='Không có yêu cầu hoàn hàng phù hợp.' columns={[
      { key: 'code', label: 'Mã đơn / Yêu cầu', render: row => <><Button onClick={() => setId(row.id)}>{row.subOrderCode}</Button><Typography variant='caption' component='div'>{row.id.slice(0, 8)}</Typography></> },
      { key: 'reason', label: 'Lý do', render: row => returnReasons[row.reason] }, { key: 'amount', label: 'Tiền hoàn', render: row => <MoneyText value={row.requestedRefundAmount} /> },
      { key: 'status', label: 'Trạng thái / Hạn phản hồi', render: row => <Stack spacing={0.5}><StatusBadge status={row.status} />{row.status === 'Pending' && <ReturnCountdown createdAt={row.createdAt} />}</Stack> },
      { key: 'date', label: 'Ngày yêu cầu', render: row => <DateText value={row.createdAt} /> }, { key: 'action', label: 'Thao tác', render: row => <Button onClick={() => setId(row.id)}>Xem & Xử lý</Button> }
    ]} /></SellerCard>{item && <ReturnDetailDialog item={item} onClose={() => setId(null)} />}
  </>
}
