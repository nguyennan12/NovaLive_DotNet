import { Alert, Box, Button, Chip, Stack, Tab, Tabs, Typography } from '@mui/material'
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
  const [params, setParams] = useSearchParams()
  const requested = params.get('status') || (shipping ? 'Confirmed' : '')
  const status = ORDER_STATUSES.some(value => value === requested) ? requested : ''
  const [page, setPage] = useState(1)
  const filter = useDateFilter()
  const query = useOrders({ page, size: PAGE_SIZE, status: status || undefined, ...filter.range })
  const counts = useOrderCounts(filter.range)

  return (
    <>
      <PageHeader
        title='Quản lý đơn hàng & Vận chuyển'
        description='Xác nhận đơn, chọn kho lấy hàng, in phiếu gửi và theo dõi toàn diện tiến độ giao vận.'
      />

      <Box sx={{ mb: 2.5 }}>
        <DateRangeFilter value={filter.dates} onChange={value => { filter.setDates(value); setPage(1) }} />
      </Box>

      {counts.error && (
        <Alert
          severity='warning'
          sx={{ mb: 2, borderRadius: '12px' }}
          action={<Button size='small' onClick={() => void counts.refetch()}>Thử lại</Button>}
        >
          Chưa tải được bộ đếm trạng thái: {errorMessage(counts.error)}
        </Alert>
      )}

      <Box
        sx={{
          mb: 2.5,
          bgcolor: '#ffffff',
          p: 0.75,
          borderRadius: '14px',
          border: '1px solid #eef2f6',
          boxShadow: '0 2px 10px rgba(0,0,0,0.02)'
        }}
      >
        <Tabs
          value={status}
          variant='scrollable'
          scrollButtons='auto'
          aria-label='Trạng thái đơn hàng'
          onChange={(_, value: string) => {
            setParams(value ? { status: value } : {})
            setPage(1)
          }}
          sx={{
            minHeight: 40,
            '& .MuiTabs-indicator': {
              height: 3,
              borderRadius: 3,
              bgcolor: '#3b82f6'
            }
          }}
        >
          <Tab
            value=''
            label='Tất cả'
            sx={{ fontWeight: 600, fontSize: '0.85rem' }}
          />
          {ORDER_STATUSES.map(value => (
            <Tab
              key={value}
              value={value}
              label={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                  <span>{statusLabels[value]}</span>
                  {counts.data && (
                    <Chip
                      label={counts.data[value] ?? 0}
                      size='small'
                      sx={{
                        height: 20,
                        fontSize: '0.7rem',
                        fontWeight: 700,
                        bgcolor: status === value ? '#eff6ff' : '#f1f5f9',
                        color: status === value ? '#2563eb' : '#64748b'
                      }}
                    />
                  )}
                </Box>
              }
              sx={{ fontWeight: 600, fontSize: '0.85rem' }}
            />
          ))}
        </Tabs>
      </Box>

      <SellerCard title='Danh sách đơn hàng'>
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
          emptyText='Chưa có đơn hàng phù hợp với bộ lọc.'
          emptyAction={<Button onClick={() => { setParams({}); setPage(1) }}>Xem tất cả trạng thái</Button>}
          columns={[
            {
              key: 'code',
              label: 'Mã đơn',
              render: row => (
                <Button
                  component={Link}
                  to={`/seller/orders/${row.id}`}
                  variant='text'
                  sx={{
                    p: 0,
                    fontSize: '0.85rem',
                    fontWeight: 700,
                    color: '#2563eb',
                    '&:hover': { textDecoration: 'underline' }
                  }}
                >
                  {row.code}
                </Button>
              )
            },
            {
              key: 'recipient',
              label: 'Người nhận',
              render: row => (
                <Box>
                  <Typography sx={{ fontWeight: 600, fontSize: '0.85rem', color: '#1e293b' }}>
                    {row.recipient.recipientName}
                  </Typography>
                  <Typography variant='caption' sx={{ color: '#64748b' }}>
                    {row.recipient.phone}
                  </Typography>
                </Box>
              )
            },
            {
              key: 'items',
              label: 'Số món',
              render: row => (
                <Chip
                  size='small'
                  label={`${row.items.reduce((sum, item) => sum + item.quantity, 0)} món`}
                  variant='outlined'
                  sx={{ borderRadius: '6px', fontSize: '0.75rem', fontWeight: 600 }}
                />
              )
            },
            {
              key: 'total',
              label: 'Tổng tiền',
              render: row => (
                <Typography sx={{ fontWeight: 800, color: '#0f172a' }}>
                  <MoneyText value={row.subTotal} />
                </Typography>
              )
            },
            {
              key: 'payment',
              label: 'Thanh toán',
              render: row => (
                <Chip
                  size='small'
                  label={row.paymentMethod}
                  sx={{
                    bgcolor: row.paymentMethod === 'COD' ? '#fff7ed' : '#f0fdf4',
                    color: row.paymentMethod === 'COD' ? '#c2410c' : '#15803d',
                    fontWeight: 700,
                    fontSize: '0.72rem'
                  }}
                />
              )
            },
            {
              key: 'source',
              label: 'Nguồn',
              render: row => (
                <Typography variant='caption' sx={{ color: '#64748b', fontWeight: 500 }}>
                  {{ Online: 'Trực tuyến', Livestream: 'Livestream', FlashSale: 'Flash Sale' }[row.orderSource] || row.orderSource}
                </Typography>
              )
            },
            {
              key: 'status',
              label: 'Trạng thái',
              render: row => (
                <Stack spacing={0.5}>
                  <StatusBadge status={row.status} />
                  {row.status === 'Confirmed' && (
                    <Typography variant='caption' sx={{ color: row.warehouseAddressId ? '#16a34a' : '#d97706', fontWeight: 600 }}>
                      {row.warehouseAddressId ? '✓ Đã chọn kho' : '⚠ Cần chọn kho'}
                    </Typography>
                  )}
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
            }
          ]}
        />
      </SellerCard>
    </>
  )
}
