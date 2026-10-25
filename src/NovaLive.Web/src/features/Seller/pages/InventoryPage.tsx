import { Alert, Box, Button, Checkbox, FormControlLabel, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import type { InventoryItem } from '../types/contracts'
import { useInventory } from '../hooks/useInventory'
import { useDebouncedValue } from '../hooks/useSellerQuery'
import { PageHeader } from '../components/shared/PageHeader'
import { DataTable } from '../components/shared/DataTable'
import { SellerCard } from '../components/shared/SellerCard'
import { AdjustmentDialog } from '../components/inventory/AdjustmentDialog'
import { InventoryHistoryDrawer } from '../components/inventory/InventoryHistoryDrawer'
import { PAGE_SIZE } from '../constants'

export default function InventoryPage() {
  const [searchParams] = useSearchParams(); const [lowStock, setLowStock] = useState(searchParams.get('lowStock') === 'true')
  const [keyword, setKeyword] = useState(''); const [page, setPage] = useState(1); const [adjust, setAdjust] = useState<InventoryItem | null>(null); const [history, setHistory] = useState<InventoryItem | null>(null)
  const query = useInventory({ page, size: PAGE_SIZE, lowStock: lowStock || undefined, keyword: useDebouncedValue(keyword) || undefined })
  return <><PageHeader title='Kho hàng' description='Tồn khả dụng = Tồn vật lý − Đang giữ chỗ. Lịch sử điều chỉnh là sổ cái chỉ đọc.' />
    <Alert severity='info' sx={{ mb: 2 }}>SKU sắp hết hàng khi tồn khả dụng ≤ mức tồn tối thiểu. Không được giảm tồn vật lý xuống dưới số lượng giữ chỗ.</Alert>
    <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mb: 2 }}><TextField size='small' label='Tìm tên sản phẩm / mã SKU' value={keyword} onChange={event => { setKeyword(event.target.value); setPage(1) }} /><FormControlLabel label='Chỉ SKU sắp hết hàng' control={<Checkbox checked={lowStock} onChange={(_, checked) => { setLowStock(checked); setPage(1) }} />} /></Box>
    <SellerCard><DataTable rows={query.data?.items || []} rowKey={row => row.skuId} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={query.data?.totalPages} total={query.data?.total} onPage={setPage} rowWarning={row => row.qtyOnHand - row.reservedQty <= row.minStock} columns={[
      { key: 'name', label: 'Sản phẩm / SKU', render: row => <>{row.spuName}<Typography variant='caption' component='div'>{row.skuCode}</Typography></> },
      { key: 'physical', label: 'Tồn vật lý', render: row => row.qtyOnHand, align: 'right' },
      { key: 'reserved', label: 'Đang giữ chỗ', render: row => row.reservedQty, align: 'right' },
      { key: 'available', label: 'Khả dụng', render: row => <><strong>{row.qtyOnHand - row.reservedQty}</strong>{row.qtyOnHand - row.reservedQty <= row.minStock && <Typography variant='caption' component='div' color='warning.main'>Sắp hết · mức tối thiểu {row.minStock}</Typography>}</>, align: 'right' },
      { key: 'action', label: 'Thao tác', render: row => <Stack direction='row'><Button onClick={() => setAdjust(row)}>Điều chỉnh</Button><Button onClick={() => setHistory(row)}>Lịch sử</Button></Stack> }
    ]} /></SellerCard>{adjust && <AdjustmentDialog item={adjust} onClose={() => setAdjust(null)} />}{history && <InventoryHistoryDrawer item={history} onClose={() => setHistory(null)} />}
  </>
}
