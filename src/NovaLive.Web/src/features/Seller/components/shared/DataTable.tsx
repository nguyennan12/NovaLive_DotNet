import type { ReactNode } from 'react'
import { Box, Pagination, Skeleton, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from '@mui/material'
import { QueryState } from './QueryState'

export interface Column<T> { key: string; label: string; render: (row: T) => ReactNode; align?: 'left' | 'right' | 'center' }

export function DataTable<T>({
  rows,
  columns,
  rowKey,
  loading,
  error,
  onRetry,
  emptyText,
  emptyAction,
  page = 1,
  totalPages = 0,
  total,
  onPage,
  rowWarning,
  rowMuted
}: {
  rows: T[]
  columns: Column<T>[]
  rowKey: (row: T) => string
  loading: boolean
  error: unknown
  onRetry: () => unknown
  emptyText?: string
  emptyAction?: ReactNode
  page?: number
  totalPages?: number
  total?: number
  onPage?: (page: number) => void
  rowWarning?: (row: T) => boolean
  rowMuted?: (row: T) => boolean
}) {
  return (
    <Box>
      <QueryState loading={false} error={error} empty={!loading && rows.length === 0} onRetry={onRetry} emptyText={emptyText} emptyAction={emptyAction}>
        <TableContainer sx={{ overflowX: 'auto', borderRadius: '12px', border: '1px solid #f1f5f9' }}>
          <Table size='small' aria-label='Bảng dữ liệu' aria-busy={loading} sx={{ minWidth: 650 }}>
            <TableHead>
              <TableRow sx={{ bgcolor: '#f8fafc' }}>
                {columns.map(column => (
                  <TableCell
                    key={column.key}
                    scope='col'
                    align={column.align}
                    sx={{
                      fontWeight: 700,
                      color: '#475569',
                      fontSize: '0.78rem',
                      textTransform: 'uppercase',
                      letterSpacing: '0.04em',
                      whiteSpace: 'nowrap',
                      py: 1.5,
                      borderBottom: '1px solid #e2e8f0'
                    }}
                  >
                    {column.label}
                  </TableCell>
                ))}
              </TableRow>
            </TableHead>
            <TableBody>
              {loading ? (
                Array.from({ length: 5 }, (_, i) => (
                  <TableRow key={i}>
                    {columns.map(column => (
                      <TableCell key={column.key} sx={{ py: 1.5, borderBottom: '1px solid #f1f5f9' }}>
                        <Skeleton height={28} />
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              ) : (
                rows.map(row => (
                  <TableRow
                    key={rowKey(row)}
                    hover
                    sx={{
                      bgcolor: rowWarning?.(row) ? '#fffbeb' : undefined,
                      opacity: rowMuted?.(row) ? 0.5 : 1,
                      transition: 'background-color 0.15s ease',
                      '&:hover': {
                        bgcolor: '#f8faff !important'
                      }
                    }}
                  >
                    {columns.map(column => (
                      <TableCell
                        key={column.key}
                        align={column.align}
                        sx={{
                          py: 1.5,
                          fontSize: '0.85rem',
                          color: '#334155',
                          borderBottom: '1px solid #f1f5f9'
                        }}
                      >
                        {column.render(row)}
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </QueryState>
      {onPage && totalPages > 0 && (
        <Box sx={{ display: 'flex', gap: 2, mt: 2.5, justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap' }}>
          <Typography variant='caption' sx={{ color: '#64748b', fontWeight: 500 }}>
            Tổng cộng {total ?? rows.length} bản ghi
          </Typography>
          <Pagination
            count={totalPages}
            page={page}
            onChange={(_, value) => onPage(value)}
            shape='rounded'
            size='small'
            color='primary'
            sx={{
              '& .MuiPaginationItem-root.Mui-selected': {
                bgcolor: '#3b82f6',
                color: '#fff',
                fontWeight: 700
              }
            }}
          />
        </Box>
      )}
    </Box>
  )
}
