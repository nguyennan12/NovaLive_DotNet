import { Alert, Box, Button, Grid, MenuItem, Stack, TextField, Typography } from '@mui/material'
import AccountBalanceWalletRounded from '@mui/icons-material/AccountBalanceWalletRounded'
import LockRounded from '@mui/icons-material/LockRounded'
import { useState } from 'react'
import { useWallet, useWalletTransactions } from '../hooks/useWallet'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { QueryState } from '../components/shared/QueryState'
import { DataTable } from '../components/shared/DataTable'
import { MoneyText } from '../components/shared/MoneyText'
import { DateText } from '../components/shared/DateText'
import { StatusBadge } from '../components/shared/StatusBadge'
import { DateRangeFilter, useDateFilter } from '../components/shared/DateRangeFilter'
import { PayoutForm } from '../components/wallet/PayoutForm'
import { PAGE_SIZE, WALLET_TYPES } from '../constants'

export default function WalletPage() {
  const wallet = useWallet()
  const filter = useDateFilter()
  const [page, setPage] = useState(1)
  const [type, setType] = useState('')
  const transactions = useWalletTransactions({ page, size: PAGE_SIZE, type: type || undefined, ...filter.range })

  return (
    <>
      <PageHeader
        title='Ví & Tài chính'
        description='Số dư khả dụng, tiền đang khóa và sổ cái tài chính của Shop.'
      />

      <Stack spacing={3}>
        <QueryState loading={wallet.isLoading} error={wallet.error} onRetry={wallet.refetch}>
          {wallet.data && (
            <Grid container spacing={2.5}>
              <Grid size={{ xs: 12, md: 6 }}>
                <Box
                  sx={{
                    background: 'linear-gradient(135deg, #1e40af 0%, #3b82f6 100%)',
                    borderRadius: '20px',
                    p: { xs: 2.5, md: 3 },
                    color: '#ffffff',
                    boxShadow: '0 10px 25px -5px rgba(59, 130, 246, 0.4)',
                    position: 'relative',
                    overflow: 'hidden'
                  }}
                >
                  <Box sx={{ position: 'relative', zIndex: 1 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                      <Typography sx={{ fontSize: '0.82rem', fontWeight: 600, opacity: 0.85, textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                        Số dư ví khả dụng
                      </Typography>
                      <AccountBalanceWalletRounded sx={{ fontSize: 24, opacity: 0.9 }} />
                    </Box>
                    <Typography component='div' variant='h3' sx={{ fontWeight: 800, letterSpacing: '-0.02em', mb: 1 }}>
                      <MoneyText value={wallet.data.balance} />
                    </Typography>
                    <Typography sx={{ fontSize: '0.8rem', opacity: 0.75 }}>
                      Sẵn sàng để rút về tài khoản ngân hàng liên kết
                    </Typography>
                  </Box>
                </Box>
              </Grid>

              <Grid size={{ xs: 12, md: 6 }}>
                <Box
                  sx={{
                    bgcolor: '#ffffff',
                    border: '1px solid #e2e8f0',
                    borderRadius: '20px',
                    p: { xs: 2.5, md: 3 },
                    boxShadow: '0 4px 20px -2px rgba(15, 23, 42, 0.04)',
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center'
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1.5 }}>
                    <Box sx={{ width: 42, height: 42, borderRadius: '12px', bgcolor: '#fffbeb', color: '#d97706', display: 'grid', placeItems: 'center' }}>
                      <LockRounded />
                    </Box>
                    <Box>
                      <Typography sx={{ fontSize: '0.82rem', fontWeight: 600, color: '#64748b' }}>
                        Tiền đang giữ chỗ / Tạm khóa
                      </Typography>
                      <Typography component='div' variant='h4' sx={{ fontWeight: 800, color: '#0f172a' }}>
                        <MoneyText value={wallet.data.lockedBalance} />
                      </Typography>
                    </Box>
                  </Box>
                  <Typography variant='caption' sx={{ color: '#94a3b8' }}>
                    Bao gồm lệnh rút tiền đang xử lý và các khoản hoàn hàng đang đối soát
                  </Typography>
                </Box>
              </Grid>
            </Grid>
          )}
        </QueryState>

        <SellerCard title='Tạo yêu cầu rút tiền'>
          {wallet.data && <PayoutForm balance={wallet.data.balance} />}
        </SellerCard>

        <SellerCard title='Lịch sử biến động số dư'>
          <Box sx={{ mb: 2 }}>
            <DateRangeFilter value={filter.dates} onChange={value => { filter.setDates(value); setPage(1) }} />
          </Box>

          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2.5 }}>
            <TextField
              select
              size='small'
              label='Loại giao dịch'
              value={type}
              onChange={event => { setType(event.target.value); setPage(1) }}
              sx={{ minWidth: 200 }}
            >
              <MenuItem value=''>Tất cả loại giao dịch</MenuItem>
              {WALLET_TYPES.map(value => (
                <MenuItem key={value} value={value}>
                  <StatusBadge status={value} />
                </MenuItem>
              ))}
            </TextField>
          </Box>

          <Alert severity='info' sx={{ mb: 2.5, borderRadius: '12px' }}>
            Lệnh rút tiền được theo dõi qua các giao dịch khóa tiền, chuyển tiền và hoàn tiền rút thất bại.
          </Alert>

          <DataTable
            rows={transactions.data?.items || []}
            rowKey={row => row.id}
            loading={transactions.isLoading}
            error={transactions.error}
            onRetry={transactions.refetch}
            page={page}
            totalPages={transactions.data?.totalPages}
            total={transactions.data?.total}
            onPage={setPage}
            columns={[
              { key: 'type', label: 'Loại giao dịch', render: row => <StatusBadge status={row.type} /> },
              {
                key: 'amount',
                label: 'Số tiền',
                render: row => (
                  <Typography
                    sx={{
                      fontWeight: 800,
                      color: row.amount > 0 ? '#16a34a' : '#dc2626'
                    }}
                  >
                    <MoneyText value={row.amount} signed />
                  </Typography>
                )
              },
              {
                key: 'after',
                label: 'Số dư sau',
                render: row => <Typography sx={{ fontWeight: 700 }}><MoneyText value={row.balanceAfter} /></Typography>
              },
              { key: 'description', label: 'Nội dung / Diễn giải', render: row => row.description },
              { key: 'date', label: 'Thời gian', render: row => <DateText value={row.createdAt} /> }
            ]}
          />
        </SellerCard>
      </Stack>
    </>
  )
}
