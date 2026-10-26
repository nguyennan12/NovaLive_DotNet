import { Alert, Box, Button, Grid, MenuItem, Stack, TextField, Typography } from '@mui/material'
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
  const wallet = useWallet(); const filter = useDateFilter(); const [page, setPage] = useState(1); const [type, setType] = useState('')
  const transactions = useWalletTransactions({ page, size: PAGE_SIZE, type: type || undefined, ...filter.range })
  return <><PageHeader title='Ví & Rút tiền' description='Số dư khả dụng, tiền đang khóa và sổ cái tài chính của Shop.' /><Stack spacing={3}>
    <QueryState loading={wallet.isLoading} error={wallet.error} onRetry={wallet.refetch}>{wallet.data && <Grid container spacing={2}><Grid size={{ xs: 12, sm: 6 }}><SellerCard title='Số dư khả dụng'><Typography component='div' variant='h4' sx={{ fontWeight: 800 }}><MoneyText value={wallet.data.balance} /></Typography></SellerCard></Grid><Grid size={{ xs: 12, sm: 6 }}><SellerCard title='Đang chờ rút / Bị khóa'><Typography component='div' variant='h4' sx={{ fontWeight: 800 }}><MoneyText value={wallet.data.lockedBalance} /></Typography></SellerCard></Grid></Grid>}</QueryState>
    <SellerCard title='Yêu cầu rút tiền'>{wallet.data && <PayoutForm balance={wallet.data.balance} />}</SellerCard>
    <SellerCard title='Sổ cái giao dịch'><DateRangeFilter value={filter.dates} onChange={value => { filter.setDates(value); setPage(1) }} /><Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}><TextField select size='small' label='Loại giao dịch' value={type} onChange={event => { setType(event.target.value); setPage(1) }} sx={{ minWidth: 220 }}><MenuItem value=''>Tất cả</MenuItem>{WALLET_TYPES.map(value => <MenuItem key={value} value={value}><StatusBadge status={value} /></MenuItem>)}</TextField>{(['PayoutLock', 'PayoutWithdrawal', 'PayoutFailedUnlock'] as const).map(value => <Button key={value} onClick={() => { setType(value); setPage(1) }}><StatusBadge status={value} /></Button>)}</Box>
      <Alert severity='info' sx={{ mb: 2 }}>Lệnh rút tiền được theo dõi qua các giao dịch khóa tiền, chuyển tiền và hoàn tiền rút thất bại. Số dư trước và tham chiếu riêng chưa được API sổ ví cung cấp.</Alert>
      <DataTable rows={transactions.data?.items || []} rowKey={row => row.id} loading={transactions.isLoading} error={transactions.error} onRetry={transactions.refetch} page={page} totalPages={transactions.data?.totalPages} total={transactions.data?.total} onPage={setPage} columns={[
        { key: 'type', label: 'Loại giao dịch', render: row => <StatusBadge status={row.type} /> },
        { key: 'amount', label: 'Số tiền', render: row => <MoneyText value={row.amount} signed /> },
        { key: 'before', label: 'Số dư trước', render: () => 'Chưa được cung cấp' },
        { key: 'after', label: 'Số dư sau', render: row => <MoneyText value={row.balanceAfter} /> },
        { key: 'description', label: 'Nội dung / Tham chiếu', render: row => row.description },
        { key: 'date', label: 'Thời gian', render: row => <DateText value={row.createdAt} /> }
      ]} />
    </SellerCard>
  </Stack></>
}
