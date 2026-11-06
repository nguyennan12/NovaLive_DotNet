import { Box, Button, Chip, MenuItem, Stack, TextField, Typography } from '@mui/material'
import NotificationsActiveOutlined from '@mui/icons-material/NotificationsActiveOutlined'
import CheckCircleOutlineRounded from '@mui/icons-material/CheckCircleOutlineRounded'
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
  const [page, setPage] = useState(1)
  const [filter, setFilter] = useState('all')
  const query = useNotifications({ page, size: PAGE_SIZE, isRead: filter === 'all' ? undefined : filter === 'read' })
  const read = useReadNotification()
  const all = useReadAllNotifications()

  return (
    <>
      <PageHeader
        title='Thông báo hệ thống'
        description='Cập nhật đơn hàng mới, yêu cầu hoàn hàng và các thông báo vận hành từ nền tảng.'
        action={
          <Button
            variant='outlined'
            color='secondary'
            disabled={all.isPending || !query.data?.items.length}
            onClick={() => all.mutate(undefined)}
            startIcon={<CheckCircleOutlineRounded />}
            sx={{ borderRadius: '10px' }}
          >
            Đánh dấu tất cả đã đọc
          </Button>
        }
      />

      <Box sx={{ mb: 2.5 }}>
        <TextField
          select
          size='small'
          label='Bộ lọc trạng thái'
          value={filter}
          onChange={event => { setFilter(event.target.value); setPage(1) }}
          sx={{ minWidth: 180 }}
        >
          <MenuItem value='all'>Tất cả thông báo</MenuItem>
          <MenuItem value='unread'>Chưa đọc</MenuItem>
          <MenuItem value='read'>Đã đọc</MenuItem>
        </TextField>
      </Box>

      <SellerCard>
        <DataTable
          rows={query.data?.items || []}
          rowKey={row => row.id}
          loading={query.isLoading}
          error={query.error}
          onRetry={query.refetch}
          page={page}
          totalPages={query.data?.totalPages}
          total={query.data?.total}
          onPage={setPage}
          emptyText='Không có thông báo phù hợp.'
          columns={[
            {
              key: 'content',
              label: 'Nội dung thông báo',
              render: row => (
                <Stack direction='row' spacing={1.5} sx={{ alignItems: 'flex-start' }}>
                  <Box
                    sx={{
                      width: 10,
                      height: 10,
                      borderRadius: '50%',
                      bgcolor: row.isRead ? 'transparent' : '#3b82f6',
                      mt: 0.75,
                      flexShrink: 0
                    }}
                  />
                  <Box>
                    <Typography sx={{ fontWeight: row.isRead ? 500 : 700, color: row.isRead ? '#475569' : '#0f172a', fontSize: '0.88rem' }}>
                      {row.title}
                    </Typography>
                    <Typography variant='caption' sx={{ color: '#64748b', display: 'block', mt: 0.25 }}>
                      {row.content}
                    </Typography>
                  </Box>
                </Stack>
              )
            },
            {
              key: 'date',
              label: 'Thời gian',
              render: row => (
                <Typography variant='caption' sx={{ color: '#64748b' }}>
                  <DateText value={row.createdAt} />
                </Typography>
              )
            },
            {
              key: 'action',
              label: 'Thao tác',
              render: row => (
                <Stack direction='row' spacing={1}>
                  {!row.isRead && (
                    <Button
                      size='small'
                      disabled={read.isPending}
                      onClick={() => read.mutate(row.id)}
                      sx={{ fontSize: '0.78rem' }}
                    >
                      Đã đọc
                    </Button>
                  )}
                  <Button
                    component={Link}
                    to={safeNotificationLink(row.link)}
                    size='small'
                    variant='outlined'
                    sx={{ borderRadius: '8px', fontSize: '0.78rem' }}
                  >
                    Xem chi tiết
                  </Button>
                </Stack>
              )
            }
          ]}
        />
      </SellerCard>
    </>
  )
}
