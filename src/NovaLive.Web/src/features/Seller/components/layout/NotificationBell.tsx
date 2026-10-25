import { Badge, Box, Button, Divider, IconButton, Menu, MenuItem, Typography } from '@mui/material'
import NotificationsNoneRounded from '@mui/icons-material/NotificationsNoneRounded'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useNotifications, useReadNotification, useUnreadCount } from '../../hooks/useDashboard'
import { QueryState } from '../shared/QueryState'
import { DateText } from '../shared/DateText'

export const safeNotificationLink = (link: string | null) => link && /^\/seller(?:\/|$)/.test(link) && !link.includes('\\') ? link : '/seller/notifications'
export function NotificationBell() {
  const [anchor, setAnchor] = useState<HTMLElement | null>(null)
  const count = useUnreadCount()
  const query = useNotifications({ page: 1, size: 5 })
  const read = useReadNotification()
  const navigate = useNavigate()
  return <>
    <IconButton aria-label={`Thông báo, ${count.data ?? 0} chưa đọc`} aria-haspopup='menu' aria-expanded={Boolean(anchor)} onClick={event => setAnchor(event.currentTarget)}><Badge badgeContent={count.data || 0} color='error'><NotificationsNoneRounded /></Badge></IconButton>
    <Menu anchorEl={anchor} open={Boolean(anchor)} onClose={() => setAnchor(null)} slotProps={{ paper: { sx: { width: 380, maxWidth: 'calc(100vw - 32px)' } } }}>
      <Box sx={{ px: 2, py: 1 }}><Typography sx={{ fontWeight: 700 }}>Thông báo cửa hàng</Typography></Box><Divider />
      <Box sx={{ p: 1 }}><QueryState loading={query.isLoading} error={query.error} empty={!query.data?.items.length} onRetry={query.refetch} emptyText='Bạn chưa có thông báo mới.'>{query.data?.items.map(item => <MenuItem key={item.id} disabled={read.isPending} onClick={() => { if (!item.isRead) read.mutate(item.id); setAnchor(null); navigate(safeNotificationLink(item.link)) }} sx={{ whiteSpace: 'normal', display: 'block', borderRadius: 1 }}><Typography sx={{ fontWeight: item.isRead ? 400 : 700 }}>{!item.isRead && '• '}{item.title}</Typography><Typography variant='caption' color='text.secondary'><DateText value={item.createdAt} /></Typography></MenuItem>)}</QueryState></Box>
      <Divider /><Button fullWidth component={Link} to='/seller/notifications' onClick={() => setAnchor(null)}>Xem tất cả thông báo</Button>
    </Menu>
  </>
}
