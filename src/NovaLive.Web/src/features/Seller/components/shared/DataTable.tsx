import type { ReactNode } from 'react'
import { Box, Pagination, Skeleton, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from '@mui/material'
import { QueryState } from './QueryState'

export interface Column<T> { key: string; label: string; render: (row: T) => ReactNode; align?: 'left' | 'right' | 'center' }
export function DataTable<T>({ rows, columns, rowKey, loading, error, onRetry, emptyText, emptyAction, page = 1, totalPages = 0, total, onPage, rowWarning, rowMuted }: { rows: T[]; columns: Column<T>[]; rowKey: (row: T) => string; loading: boolean; error: unknown; onRetry: () => unknown; emptyText?: string; emptyAction?: ReactNode; page?: number; totalPages?: number; total?: number; onPage?: (page: number) => void; rowWarning?: (row: T) => boolean; rowMuted?: (row: T) => boolean }) {
  return <Box>
    <QueryState loading={false} error={error} empty={!loading && rows.length === 0} onRetry={onRetry} emptyText={emptyText} emptyAction={emptyAction}>
      <TableContainer sx={{ overflowX: 'auto' }}><Table size='small' aria-label='Bảng dữ liệu' aria-busy={loading} sx={{ minWidth: 650 }}>
        <TableHead><TableRow>{columns.map(column => <TableCell key={column.key} scope='col' align={column.align} sx={{ fontWeight: 700, bgcolor: 'action.hover', whiteSpace: 'nowrap', py: 1.5 }}>{column.label}</TableCell>)}</TableRow></TableHead>
        <TableBody>{loading ? Array.from({ length: 5 }, (_, i) => <TableRow key={i}>{columns.map(column => <TableCell key={column.key}><Skeleton height={38} /></TableCell>)}</TableRow>) : rows.map(row => <TableRow key={rowKey(row)} hover sx={{ bgcolor: rowWarning?.(row) ? 'action.selected' : undefined, opacity: rowMuted?.(row) ? 0.5 : 1 }}>{columns.map(column => <TableCell key={column.key} align={column.align} sx={{ py: 1.5 }}>{column.render(row)}</TableCell>)}</TableRow>)}</TableBody>
      </Table></TableContainer>
    </QueryState>
    {onPage && totalPages > 0 && <Box sx={{ display: 'flex', gap: 2, mt: 2, justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap' }}><Typography color='text.secondary'>{total ?? rows.length} kết quả</Typography><Pagination getItemAriaLabel={(type, value, selected) => type === 'page' ? (selected ? 'Trang hiện tại ' : 'Đến trang ') + value : ({ next: 'Trang sau', previous: 'Trang trước', first: 'Trang đầu', last: 'Trang cuối', 'start-ellipsis': 'Các trang trước', 'end-ellipsis': 'Các trang sau' }[type] || 'Phân trang')} aria-label='Phân trang' count={totalPages} page={page} onChange={(_, value) => onPage(value)} shape='rounded' /></Box>}
  </Box>
}
