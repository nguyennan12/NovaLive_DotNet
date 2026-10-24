import { Alert, Stack, Typography } from '@mui/material'
import { useTracking } from '../../hooks/useOrders'
import { QueryState } from '../shared/QueryState'
import { DateText } from '../shared/DateText'
import { StatusBadge } from '../shared/StatusBadge'
import { SellerCard } from '../shared/SellerCard'

export function TrackingPanel({ id }: { id: string }) {
  const query = useTracking(id, true)
  return <SellerCard title='Hành trình vận chuyển'><QueryState loading={query.isLoading} error={query.error} onRetry={query.refetch} empty={Boolean(query.data && !query.data.events.length)} emptyText='Đơn vị vận chuyển chưa cập nhật hành trình.'>{query.data && <Stack spacing={2}><Typography>{query.data.provider} · {query.data.trackingCode}</Typography>{query.data.isStale && <Alert severity='warning'>Dữ liệu hành trình đang chờ đồng bộ từ đơn vị vận chuyển.</Alert>}{query.data.events.map((event, index) => <Stack key={index} spacing={0.5}><DateText value={event.occurredAt} /><div><StatusBadge status={event.status} /></div><Typography>{event.description || 'Cập nhật vận chuyển'}</Typography></Stack>)}</Stack>}</QueryState></SellerCard>
}
