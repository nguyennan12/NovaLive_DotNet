import type { ReactNode } from 'react'
import { Alert, Box, Button, Skeleton, Typography } from '@mui/material'
import { errorMessage } from '../../utils/errors'

export function QueryState({ loading, error, empty, onRetry, emptyText = 'Chưa có dữ liệu.', emptyAction, children }: { loading: boolean; error: unknown; empty?: boolean; onRetry: () => unknown; emptyText?: string; emptyAction?: ReactNode; children: ReactNode }) {
  if (loading) return <Box aria-busy='true' aria-label='Đang tải dữ liệu' sx={{ display: 'grid', gap: 1.5 }}>{[1, 2, 3].map(key => <Skeleton key={key} variant='rounded' height={64} />)}</Box>
  if (error) return <Alert severity='error' action={<Button color='inherit' onClick={() => void onRetry()}>Thử lại</Button>}>{errorMessage(error)}</Alert>
  if (empty) return <Box sx={{ py: 5, textAlign: 'center' }}><Typography color='text.secondary' sx={{ mb: 2 }}>{emptyText}</Typography>{emptyAction || <Button onClick={() => void onRetry()}>Tải lại dữ liệu</Button>}</Box>
  return children
}
