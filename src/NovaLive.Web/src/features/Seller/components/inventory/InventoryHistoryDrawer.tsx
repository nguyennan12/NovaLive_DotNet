import { Box, Button, Drawer, Stack, Typography } from '@mui/material'
import { useState } from 'react'
import type { InventoryItem } from '../../types/contracts'
import { useInventoryHistory } from '../../hooks/useInventory'
import { DataTable } from '../shared/DataTable'
import { DateText } from '../shared/DateText'
import { StatusBadge } from '../shared/StatusBadge'
import { DateRangeFilter, useDateFilter } from '../shared/DateRangeFilter'
import { PAGE_SIZE } from '../../constants'

export function InventoryHistoryDrawer({ item, onClose }: { item: InventoryItem; onClose: () => void }) {
  const [page, setPage] = useState(1); const filter = useDateFilter()
  const query = useInventoryHistory({ page, size: PAGE_SIZE, skuId: item.skuId, ...filter.range })
  return <Drawer anchor='right' open onClose={onClose} slotProps={{ paper: { sx: { width: { xs: '100%', md: 900 }, p: 3 } } }}><Stack spacing={2}>
    <Box sx={{ display: 'flex', justifyContent: 'space-between', gap: 2 }}><Box><Typography component='h2' variant='h6'>Sổ biến động · {item.skuCode}</Typography><Typography color='text.secondary'>Sổ cái chỉ đọc. Mỗi điều chỉnh tạo một bản ghi mới.</Typography></Box><Button onClick={onClose}>Đóng</Button></Box>
    <DateRangeFilter value={filter.dates} onChange={value => { filter.setDates(value); setPage(1) }} />
    <DataTable rows={query.data?.items || []} rowKey={row => row.id} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={query.data?.totalPages} total={query.data?.total} onPage={setPage} columns={[
      { key: 'date', label: 'Thời gian', render: row => <DateText value={row.createdAt} /> },
      { key: 'type', label: 'Loại', render: row => <StatusBadge status={row.changeType} /> },
      { key: 'before', label: 'Trước', render: row => row.qtyBefore },
      { key: 'change', label: 'Thay đổi', render: row => `${row.qtyChange > 0 ? '+' : ''}${row.qtyChange}` },
      { key: 'after', label: 'Sau', render: row => row.qtyAfter },
      { key: 'reserved', label: 'Giữ chỗ (trước / đổi)', render: row => `${row.reservedBefore} / ${row.reservedChange > 0 ? '+' : ''}${row.reservedChange}` },
      { key: 'ref', label: 'Tham chiếu', render: row => <>{row.refType || '—'}<br />{row.refId}</> },
      { key: 'actor', label: 'Người thực hiện / Ghi chú', render: row => <>{row.createdBy || 'Hệ thống'}<br />{row.note}</> }
    ]} />
  </Stack></Drawer>
}
