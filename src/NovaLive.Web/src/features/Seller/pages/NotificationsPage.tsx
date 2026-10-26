import { Box, Button, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useNotifications, useReadAllNotifications, useReadNotification } from '../hooks/useDashboard'
import { PageHeader } from '../components/shared/PageHeader'
import { DataTable } from '../components/shared/DataTable'
import { SellerCard } from '../components/shared/SellerCard'
import { DateText } from '../components/shared/DateText'
import { PAGE_SIZE } from '../constants'
import { safeNotificationLink } from '../components/layout/NotificationBell'

export default function NotificationsPage() {
  const [page, setPage] = useState(1); const [filter, setFilter] = useState('all')
  const query = useNotifications({ page, size: PAGE_SIZE, isRead: filter === 'all' ? undefined : filter === 'read' })
  const read = useReadNotification(); const all = useReadAllNotifications()
  return <><PageHeader title='Thông báo' description='Đơn mới, yêu cầu hoàn hàng và cập nhật vận hành.' action={<Button variant='outlined' color='secondary' disabled={all.isPending || !query.data?.items.length} onClick={() => all.mutate(undefined)}>Đánh dấu tất cả đã đọc</Button>} />
    <Box sx={{ mb: 2 }}><TextField select size='small' label='Trạng thái đọc' value={filter} onChange={event => { setFilter(event.target.value); setPage(1) }} sx={{ minWidth: 180 }}><MenuItem value='all'>Tất cả</MenuItem><MenuItem value='unread'>Chưa đọc</MenuItem><MenuItem value='read'>Đã đọc</MenuItem></TextField></Box>
    <SellerCard><DataTable rows={query.data?.items || []} rowKey={row => row.id} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={query.data?.totalPages} total={query.data?.total} onPage={setPage} emptyText='Không có thông báo phù hợp.' columns={[
      { key: 'content', label: 'Nội dung', render: row => <><Typography sx={{ fontWeight: row.isRead ? 400 : 700 }}>{!row.isRead && '• Chưa đọc · '}{row.title}</Typography><Typography variant='caption' color='text.secondary'>{row.content}</Typography></> },
      { key: 'date', label: 'Thời gian', render: row => <DateText value={row.createdAt} /> },
      { key: 'action', label: 'Thao tác', render: row => <Stack direction='row'>{!row.isRead && <Button disabled={read.isPending} onClick={() => read.mutate(row.id)}>Đã đọc</Button>}<Button component={Link} to={safeNotificationLink(row.link)}>Xem chi tiết</Button></Stack> }
    ]} /></SellerCard>
  </>
}
