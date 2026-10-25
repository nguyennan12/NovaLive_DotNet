import { Alert, Box, Button, LinearProgress, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import type { Campaign } from '../types/contracts'
import { useCampaigns, useRegistrations } from '../hooks/useMarketing'
import { useShopPermissions } from '../hooks/useShopPermissions'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { DataTable } from '../components/shared/DataTable'
import { StatusBadge } from '../components/shared/StatusBadge'
import { DateText } from '../components/shared/DateText'
import { MoneyText } from '../components/shared/MoneyText'
import { FlashRegistrationDialog } from '../components/marketing/FlashRegistrationDialog'
import { PAGE_SIZE } from '../constants'
import { canRegisterCampaign } from '../utils/marketing'

export default function FlashSalesPage() {
  const [campaign, setCampaign] = useState<Campaign | null>(null); const [page, setPage] = useState(1); const [status, setStatus] = useState('')
  const campaigns = useCampaigns(); const registrations = useRegistrations({ page, size: PAGE_SIZE, status: status || undefined }); const { canOperate } = useShopPermissions()
  return <><PageHeader title='Flash Sale' description='Đăng ký SKU với mức giảm tối thiểu 10% so với giá bán hiện tại.' /><Stack spacing={3}>
    <SellerCard title='Chiến dịch Flash Sale'><Alert severity='info' sx={{ mb: 2 }}>Chỉ đăng ký trước khi chiến dịch bắt đầu. Danh sách hiện tại chưa cung cấp chiến dịch sắp diễn ra; các chiến dịch đang chạy đã đóng đăng ký.</Alert><DataTable rows={campaigns.data || []} rowKey={row => row.id} loading={campaigns.isLoading} error={campaigns.error} onRetry={campaigns.refetch} emptyText='Chưa có chiến dịch được công bố.' columns={[
      { key: 'name', label: 'Chiến dịch', render: row => row.name }, { key: 'start', label: 'Bắt đầu', render: row => <DateText value={row.startAt} /> }, { key: 'end', label: 'Kết thúc', render: row => <DateText value={row.endAt} /> }, { key: 'status', label: 'Trạng thái', render: row => <StatusBadge status={row.status} /> }, { key: 'action', label: 'Thao tác', render: row => <Button variant='outlined' color='secondary' disabled={!canOperate || !canRegisterCampaign(row)} onClick={() => setCampaign(row)}>{canRegisterCampaign(row) ? 'Đăng ký SKU' : 'Đã đóng đăng ký'}</Button> }
    ]} /></SellerCard>
    <SellerCard title='Đăng ký của Shop'><Box sx={{ mb: 2 }}><TextField select label='Trạng thái đăng ký' value={status} size='small' onChange={event => { setStatus(event.target.value); setPage(1) }} sx={{ minWidth: 180 }}><MenuItem value=''>Tất cả</MenuItem>{['Pending', 'Approved', 'Rejected', 'Ended'].map(value => <MenuItem key={value} value={value}><StatusBadge status={value} /></MenuItem>)}</TextField></Box><DataTable rows={registrations.data?.items || []} rowKey={row => row.id} loading={registrations.isLoading} error={registrations.error} onRetry={registrations.refetch} page={page} totalPages={registrations.data?.totalPages} total={registrations.data?.total} onPage={setPage} columns={[
      { key: 'name', label: 'Sản phẩm / SKU', render: row => <>{row.spuName}<Typography variant='caption' component='div'>{row.skuCode}</Typography></> }, { key: 'price', label: 'Giá Flash Sale', render: row => <MoneyText value={row.flashPrice} /> }, { key: 'sold', label: 'Tiến độ bán', render: row => <Box sx={{ minWidth: 120 }}><Typography variant='caption'>{row.soldQty} / {row.quantity}</Typography><LinearProgress variant='determinate' value={row.quantity ? Math.min(100, row.soldQty / row.quantity * 100) : 0} color='secondary' aria-label='Tỷ lệ bán Flash Sale' /></Box> }, { key: 'status', label: 'Trạng thái', render: row => <StatusBadge status={row.status} /> }, { key: 'reason', label: 'Ghi chú', render: row => row.rejectionReason || '—' }
    ]} /></SellerCard>
  </Stack>{campaign && <FlashRegistrationDialog campaign={campaign} onClose={() => setCampaign(null)} />}</>
}
