import HistoryRounded from '@mui/icons-material/HistoryRounded'
import Inventory2Outlined from '@mui/icons-material/Inventory2Outlined'
import SearchRounded from '@mui/icons-material/SearchRounded'
import WarningAmberRounded from '@mui/icons-material/WarningAmberRounded'
import WarehouseOutlined from '@mui/icons-material/WarehouseOutlined'
import { Box, Button, Chip, InputAdornment, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useMemo, useState, type ReactNode } from 'react'
import { useSearchParams } from 'react-router-dom'
import type { InventoryItem } from '../types/contracts'
import { useInventory, useInventoryHistory } from '../hooks/useInventory'
import { useDebouncedValue } from '../hooks/useSellerQuery'
import { PageHeader } from '../components/shared/PageHeader'
import { DataTable } from '../components/shared/DataTable'
import { SellerCard } from '../components/shared/SellerCard'
import { AdjustmentDialog } from '../components/inventory/AdjustmentDialog'
import { InventoryHistoryDrawer } from '../components/inventory/InventoryHistoryDrawer'
import { DateText } from '../components/shared/DateText'
import { StatusBadge } from '../components/shared/StatusBadge'
import { PAGE_SIZE } from '../constants'

function SummaryCard({ icon, label, value, tone = 'blue' }: { icon: ReactNode; label: string; value: number; tone?: 'blue' | 'green' | 'orange' | 'violet' }) {
  const colors = { blue: ['#eef4ff', '#568dfb'], green: ['#ecfdf3', '#22a06b'], orange: ['#fff7e8', '#e49a24'], violet: ['#f3efff', '#8067dc'] }[tone]
  return <SellerCard><Stack direction='row' spacing={1.5} sx={{ alignItems: 'center' }}><Box sx={{ width: 48, height: 48, borderRadius: 2.5, display: 'grid', placeItems: 'center', bgcolor: colors[0], color: colors[1] }}>{icon}</Box><Box><Typography color='text.secondary' variant='body2'>{label}</Typography><Typography variant='h5' sx={{ fontWeight: 800, mt: 0.25 }}>{value.toLocaleString('vi-VN')}</Typography></Box></Stack></SellerCard>
}

export default function InventoryPage() {
  const [searchParams] = useSearchParams()
  const [lowStock, setLowStock] = useState(searchParams.get('lowStock') === 'true')
  const [keyword, setKeyword] = useState('')
  const [status, setStatus] = useState<'all' | 'available' | 'reserved'>('all')
  const [page, setPage] = useState(1)
  const [adjust, setAdjust] = useState<InventoryItem | null>(null)
  const [history, setHistory] = useState<InventoryItem | null>(null)
  const debouncedKeyword = useDebouncedValue(keyword)
  const query = useInventory({ page, size: PAGE_SIZE, lowStock: lowStock || undefined, keyword: debouncedKeyword || undefined })
  const historyQuery = useInventoryHistory({ page: 1, size: 6 })
  const items = useMemo(() => query.data?.items || [], [query.data?.items])
  const visibleItems = useMemo(() => items.filter(item => status === 'all' || (status === 'available' ? item.availableQty > 0 : item.reservedQty > 0)), [items, status])
  const totals = useMemo(() => items.reduce((acc, item) => ({ physical: acc.physical + item.qtyOnHand, reserved: acc.reserved + item.reservedQty, available: acc.available + item.availableQty, low: acc.low + (item.availableQty <= item.minStock ? 1 : 0) }), { physical: 0, reserved: 0, available: 0, low: 0 }), [items])

  return <>
    <PageHeader title='Quản lý kho' description='Theo dõi tồn vật lý, hàng giữ chỗ và lịch sử biến động của từng SKU.' action={<Button variant='contained' color='secondary' startIcon={<WarehouseOutlined />} disabled={items.length === 0} onClick={() => setAdjust(items[0])}>Điều chỉnh tồn kho</Button>} />

    <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', xl: 'repeat(4, 1fr)' }, gap: 2, mb: 2.5 }}>
      <SummaryCard icon={<Inventory2Outlined />} label='Tồn vật lý trên trang' value={totals.physical} />
      <SummaryCard icon={<WarehouseOutlined />} label='Tồn khả dụng' value={totals.available} tone='green' />
      <SummaryCard icon={<HistoryRounded />} label='Đang giữ chỗ' value={totals.reserved} tone='violet' />
      <SummaryCard icon={<WarningAmberRounded />} label='SKU sắp hết hàng' value={totals.low} tone='orange' />
    </Box>

    <SellerCard title='Danh sách tồn kho' action={<Typography color='text.secondary' variant='body2'>{query.data?.total || 0} SKU</Typography>}>
      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'minmax(280px, 1fr) 210px auto' }, gap: 1.5, mb: 2 }}>
        <TextField size='small' placeholder='Tìm theo tên sản phẩm hoặc mã SKU...' value={keyword} onChange={event => { setKeyword(event.target.value); setPage(1) }} slotProps={{ input: { startAdornment: <InputAdornment position='start'><SearchRounded fontSize='small' /></InputAdornment> } }} />
        <TextField select size='small' label='Trạng thái tồn' value={status} onChange={event => setStatus(event.target.value as typeof status)}><MenuItem value='all'>Tất cả trạng thái</MenuItem><MenuItem value='available'>Còn hàng</MenuItem><MenuItem value='reserved'>Có hàng giữ chỗ</MenuItem></TextField>
        <Button variant={lowStock ? 'contained' : 'outlined'} color='warning' startIcon={<WarningAmberRounded />} onClick={() => { setLowStock(value => !value); setPage(1) }}>Sắp hết hàng</Button>
      </Box>

      <DataTable rows={visibleItems} rowKey={row => row.skuId} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={query.data?.totalPages} total={query.data?.total} onPage={setPage} emptyText='Không có SKU phù hợp với bộ lọc.' rowWarning={row => row.availableQty <= row.minStock} columns={[
        { key: 'name', label: 'Sản phẩm', render: row => <Box><Typography sx={{ fontWeight: 700 }}>{row.spuName}</Typography><Typography variant='caption' color='text.secondary'>{row.skuCode}</Typography></Box> },
        { key: 'physical', label: 'Tồn vật lý', render: row => <Typography sx={{ fontWeight: 700 }}>{row.qtyOnHand}</Typography>, align: 'right' },
        { key: 'reserved', label: 'Giữ chỗ', render: row => row.reservedQty, align: 'right' },
        { key: 'available', label: 'Khả dụng', render: row => <Typography sx={{ fontWeight: 800, color: row.availableQty <= row.minStock ? 'warning.main' : 'success.main' }}>{row.availableQty}</Typography>, align: 'right' },
        { key: 'status', label: 'Trạng thái', render: row => <Chip size='small' label={row.availableQty <= 0 ? 'Hết hàng' : row.availableQty <= row.minStock ? 'Sắp hết' : 'Còn hàng'} color={row.availableQty <= 0 ? 'error' : row.availableQty <= row.minStock ? 'warning' : 'success'} variant='outlined' /> },
        { key: 'updated', label: 'Cập nhật', render: row => <DateText value={row.lastUpdated} /> },
        { key: 'action', label: 'Thao tác', render: row => <Stack direction='row' spacing={0.5}><Button size='small' onClick={() => setAdjust(row)}>Điều chỉnh</Button><Button size='small' onClick={() => setHistory(row)}>Lịch sử</Button></Stack> }
      ]} />
    </SellerCard>

    <Box sx={{ mt: 2.5 }}><SellerCard title='Biến động kho gần đây' action={<Typography color='text.secondary' variant='body2'>{historyQuery.data?.total || 0} bản ghi</Typography>}>
      <DataTable rows={historyQuery.data?.items || []} rowKey={row => row.id} loading={historyQuery.isLoading} error={historyQuery.error} onRetry={historyQuery.refetch} emptyText='Chưa có biến động kho.' columns={[
        { key: 'type', label: 'Loại', render: row => <StatusBadge status={row.changeType} /> },
        { key: 'product', label: 'Sản phẩm / SKU', render: row => <><Typography sx={{ fontWeight: 600 }}>{row.spuName}</Typography><Typography variant='caption' color='text.secondary'>{row.skuCode}</Typography></> },
        { key: 'change', label: 'Thay đổi', render: row => <Typography sx={{ fontWeight: 800, color: row.qtyChange >= 0 ? 'success.main' : 'error.main' }}>{row.qtyChange > 0 ? '+' : ''}{row.qtyChange}</Typography>, align: 'right' },
        { key: 'after', label: 'Tồn sau', render: row => row.qtyAfter, align: 'right' },
        { key: 'time', label: 'Thời gian', render: row => <DateText value={row.createdAt} /> },
        { key: 'note', label: 'Ghi chú', render: row => row.note || '—' }
      ]} />
    </SellerCard></Box>

    {adjust && <AdjustmentDialog item={adjust} onClose={() => setAdjust(null)} />}
    {history && <InventoryHistoryDrawer item={history} onClose={() => setHistory(null)} />}
  </>
}
