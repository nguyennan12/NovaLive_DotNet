import { Avatar, Box, Button, Grid, InputAdornment, Stack, TextField, Typography } from '@mui/material'
import SearchRounded from '@mui/icons-material/SearchRounded'
import PeopleAltOutlined from '@mui/icons-material/PeopleAltOutlined'
import ShoppingBagOutlined from '@mui/icons-material/ShoppingBagOutlined'
import MonetizationOnOutlined from '@mui/icons-material/MonetizationOnOutlined'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useCustomers } from '../hooks/useOrders'
import { useDebouncedValue } from '../hooks/useSellerQuery'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { DataTable } from '../components/shared/DataTable'
import { DateText } from '../components/shared/DateText'
import { MoneyText } from '../components/shared/MoneyText'
import { PAGE_SIZE } from '../constants'

export default function CustomersPage() {
  const query = useCustomers()
  const [keyword, setKeyword] = useState('')
  const [page, setPage] = useState(1)
  const search = useDebouncedValue(keyword).toLowerCase()
  const rows = (query.data || []).filter(item => `${item.name} ${item.phone}`.toLowerCase().includes(search))

  const totalSpent = rows.reduce((acc, r) => acc + (r.totalSpent || 0), 0)
  const totalOrders = rows.reduce((acc, r) => acc + (r.orderCount || 0), 0)

  return (
    <>
      <PageHeader
        title='Khách hàng của Shop'
        description='Danh sách và lịch sử người nhận từ các đơn hàng thuộc cửa hàng.'
      />

      <Grid container spacing={2} sx={{ mb: 2.5 }}>
        <Grid size={{ xs: 12, sm: 4 }}>
          <SellerCard>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Box>
                <Typography variant='caption' sx={{ color: '#64748b', fontWeight: 600 }}>Tổng khách hàng</Typography>
                <Typography variant='h5' sx={{ fontWeight: 800, mt: 0.5, color: '#0f172a' }}>{rows.length}</Typography>
              </Box>
              <Box sx={{ width: 44, height: 44, borderRadius: '12px', bgcolor: '#eff6ff', color: '#2563eb', display: 'grid', placeItems: 'center' }}>
                <PeopleAltOutlined />
              </Box>
            </Box>
          </SellerCard>
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <SellerCard>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Box>
                <Typography variant='caption' sx={{ color: '#64748b', fontWeight: 600 }}>Tổng đơn đã đặt</Typography>
                <Typography variant='h5' sx={{ fontWeight: 800, mt: 0.5, color: '#0f172a' }}>{totalOrders}</Typography>
              </Box>
              <Box sx={{ width: 44, height: 44, borderRadius: '12px', bgcolor: '#ecfdf5', color: '#059669', display: 'grid', placeItems: 'center' }}>
                <ShoppingBagOutlined />
              </Box>
            </Box>
          </SellerCard>
        </Grid>
        <Grid size={{ xs: 12, sm: 4 }}>
          <SellerCard>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
              <Box>
                <Typography variant='caption' sx={{ color: '#64748b', fontWeight: 600 }}>Tổng giá trị tích lũy</Typography>
                <Typography variant='h5' sx={{ fontWeight: 800, mt: 0.5, color: '#2563eb' }}>
                  <MoneyText value={totalSpent} />
                </Typography>
              </Box>
              <Box sx={{ width: 44, height: 44, borderRadius: '12px', bgcolor: '#f5f3ff', color: '#7c3aed', display: 'grid', placeItems: 'center' }}>
                <MonetizationOnOutlined />
              </Box>
            </Box>
          </SellerCard>
        </Grid>
      </Grid>

      <SellerCard
        title='Danh sách khách hàng'
        action={
          <TextField
            size='small'
            placeholder='Tìm tên hoặc số điện thoại...'
            value={keyword}
            onChange={event => { setKeyword(event.target.value); setPage(1) }}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position='start'>
                    <SearchRounded fontSize='small' sx={{ color: '#94a3b8' }} />
                  </InputAdornment>
                )
              }
            }}
            sx={{ width: { xs: '100%', sm: 260 } }}
          />
        }
      >
        <DataTable
          rows={rows.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)}
          rowKey={row => row.id}
          loading={query.isLoading}
          error={query.error}
          onRetry={query.refetch}
          page={page}
          totalPages={Math.ceil(rows.length / PAGE_SIZE)}
          total={rows.length}
          onPage={setPage}
          emptyText='Chưa có khách hàng phù hợp.'
          emptyAction={<Button component={Link} to='/seller/orders'>Xem đơn hàng</Button>}
          columns={[
            {
              key: 'name',
              label: 'Người nhận',
              render: row => (
                <Stack direction='row' spacing={1.5} sx={{ alignItems: 'center' }}>
                  <Avatar sx={{ width: 34, height: 34, bgcolor: '#eff6ff', color: '#2563eb', fontWeight: 700, fontSize: '0.85rem' }}>
                    {row.name ? row.name[0].toUpperCase() : 'U'}
                  </Avatar>
                  <Typography sx={{ fontWeight: 600, fontSize: '0.88rem' }}>{row.name}</Typography>
                </Stack>
              )
            },
            { key: 'phone', label: 'Điện thoại', render: row => <Typography sx={{ color: '#64748b', fontSize: '0.85rem' }}>{row.phone}</Typography> },
            {
              key: 'count',
              label: 'Số đơn',
              render: row => (
                <Typography sx={{ fontWeight: 700, bgcolor: '#f1f5f9', px: 1, py: 0.25, borderRadius: '6px', display: 'inline-block', fontSize: '0.8rem' }}>
                  {row.orderCount} đơn
                </Typography>
              )
            },
            {
              key: 'spent',
              label: 'Giá trị đơn hợp lệ',
              render: row => (
                <Typography sx={{ fontWeight: 800, color: '#0f172a' }}>
                  <MoneyText value={row.totalSpent} />
                </Typography>
              )
            },
            { key: 'last', label: 'Đơn gần nhất', render: row => <DateText value={row.lastOrderAt} /> },
            {
              key: 'action',
              label: 'Thao tác',
              render: row => (
                <Button
                  component={Link}
                  to={`/seller/orders/${row.lastOrderId}`}
                  size='small'
                  variant='outlined'
                  sx={{ borderRadius: '8px', fontSize: '0.78rem' }}
                >
                  Xem đơn gần nhất
                </Button>
              )
            }
          ]}
        />
      </SellerCard>
    </>
  )
}
