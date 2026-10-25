import { Alert, Box, Button, MenuItem, Stack, Switch, TextField } from '@mui/material'
import { useState } from 'react'
import type { Discount } from '../types/contracts'
import { useDiscounts, useUpdateDiscount } from '../hooks/useMarketing'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { DataTable } from '../components/shared/DataTable'
import { DateText } from '../components/shared/DateText'
import { MoneyText } from '../components/shared/MoneyText'
import { StatusBadge } from '../components/shared/StatusBadge'
import { DiscountDialog } from '../components/marketing/DiscountDialog'
import { discountStatus } from '../utils/marketing'
import { PAGE_SIZE } from '../constants'

export default function DiscountsPage() {
  const [page, setPage] = useState(1); const [status, setStatus] = useState(''); const [editor, setEditor] = useState<{ initial: Discount | null } | null>(null)
  const query = useDiscounts({ page, size: PAGE_SIZE, status: status || undefined }); const update = useUpdateDiscount()
  return <><PageHeader title='Voucher của Shop' description='Tạo ưu đãi và kiểm soát hạn sử dụng. Không xóa cứng voucher.' action={<Button variant='contained' color='secondary' onClick={() => setEditor({ initial: null })}>Tạo voucher</Button>} /><Alert severity='info' sx={{ mb: 2 }}>Muốn ngừng voucher, dùng “Tắt kích hoạt”. Mã và giá trị giảm không thể đổi qua API chỉnh sửa.</Alert>
    <Box sx={{ mb: 2 }}><TextField select label='Hiệu lực' size='small' value={status} onChange={event => { setStatus(event.target.value); setPage(1) }} sx={{ minWidth: 180 }}><MenuItem value=''>Tất cả</MenuItem>{['Active', 'Scheduled', 'Expired', 'Inactive'].map(value => <MenuItem key={value} value={value}><StatusBadge status={value} /></MenuItem>)}</TextField></Box>
    <SellerCard><DataTable rows={query.data?.items || []} rowKey={row => row.id} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={query.data?.totalPages} total={query.data?.total} onPage={setPage} emptyAction={<Button onClick={() => setEditor({ initial: null })}>Tạo voucher đầu tiên</Button>} columns={[
      { key: 'code', label: 'Mã / Tên', render: row => <><strong>{row.code}</strong><br />{row.name}</> }, { key: 'discount', label: 'Giá trị giảm', render: row => row.discountType === 'PercentCart' ? <>{row.discountValue}% · tối đa <MoneyText value={row.maxDiscountAmount || 0} /></> : <MoneyText value={row.discountValue} /> },
      { key: 'uses', label: 'Đã dùng / Tối đa', render: row => `${row.usedCount} / ${row.maxUses ?? 'Không giới hạn'}` }, { key: 'dates', label: 'Hiệu lực', render: row => <><DateText value={row.validFrom} /><br /><DateText value={row.validTo} /></> }, { key: 'status', label: 'Trạng thái', render: row => <StatusBadge status={discountStatus(row)} /> },
      { key: 'action', label: 'Kích hoạt / Sửa', render: row => <Stack direction='row'><Switch checked={row.isActive} disabled={update.isPending} slotProps={{ input: { 'aria-label': `${row.isActive ? 'Tắt' : 'Bật'} voucher ${row.code}` } }} onChange={(_, checked) => update.mutate({ id: row.id, data: { name: row.name, maxUses: row.maxUses, validTo: row.validTo, isActive: checked } })} /><Button onClick={() => setEditor({ initial: row })}>Sửa</Button></Stack> }
    ]} /></SellerCard>{editor && <DiscountDialog initial={editor.initial} onClose={() => setEditor(null)} />}
  </>
}
