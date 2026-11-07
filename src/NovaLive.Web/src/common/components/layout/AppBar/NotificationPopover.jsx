import CheckCircleOutlineRoundedIcon from '@mui/icons-material/CheckCircleOutlineRounded'
import CheckRoundedIcon from '@mui/icons-material/CheckRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import NotificationsNoneRoundedIcon from '@mui/icons-material/NotificationsNoneRounded'
import NotificationsOffRoundedIcon from '@mui/icons-material/NotificationsOffRounded'
import PersonRoundedIcon from '@mui/icons-material/PersonRounded'
import {
  Box,
  Button,
  CircularProgress,
  Divider,
  Popover,
  Typography
} from '@mui/material'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import {
  getNotificationsAPI,
  markAllNotificationsReadAPI,
  markNotificationReadAPI
} from '~/common/apis/services/notificationService'

const formatRelativeTime = (dateStr) => {
  if (!dateStr) return ''
  const date = new Date(dateStr)
  const now = new Date()
  const diffSec = Math.floor((now - date) / 1000)

  if (diffSec < 60) return 'vừa xong'
  const diffMin = Math.floor(diffSec / 60)
  if (diffMin < 60) return `${diffMin} phút trước`
  const diffHours = Math.floor(diffMin / 60)
  if (diffHours < 24) return `${diffHours} giờ trước`
  const diffDays = Math.floor(diffHours / 24)
  if (diffDays === 1) return 'một ngày trước'
  if (diffDays < 30) return `${diffDays} ngày trước`
  const diffMonths = Math.floor(diffDays / 30)
  if (diffMonths < 12) return `${diffMonths} tháng trước`
  return `${Math.floor(diffMonths / 12)} năm trước`
}

const NotificationPopover = ({ anchorEl, open, onClose, currentUser }) => {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const { data, isLoading } = useQuery({
    queryKey: ['user_notifications'],
    queryFn: () => getNotificationsAPI({ page: 1, size: 20 }),
    enabled: Boolean(currentUser),
    refetchInterval: 30000,
    staleTime: 10000
  })

  const markReadMutation = useMutation({
    mutationFn: markNotificationReadAPI,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user_notifications'] })
    }
  })

  const markAllReadMutation = useMutation({
    mutationFn: markAllNotificationsReadAPI,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['user_notifications'] })
    }
  })

  const rawItems = data?.items || (Array.isArray(data) ? data : [])
  const items = Array.isArray(rawItems) ? rawItems : []
  const unreadCount = items.filter((n) => !n.isRead).length

  const handleItemClick = (item) => {
    if (!item.isRead) {
      markReadMutation.mutate(item.id)
    }
    onClose()
    if (item.link) {
      navigate(item.link)
    } else if (item.refType === 'ParentOrder' || item.type === 'OrderPlaced') {
      navigate('/orders')
    }
  }

  const handleMarkAllRead = (e) => {
    e.stopPropagation()
    if (unreadCount > 0) {
      markAllReadMutation.mutate()
    }
  }

  return (
    <Popover
      anchorEl={anchorEl}
      open={open}
      onClose={onClose}
      anchorOrigin={{
        vertical: 'bottom',
        horizontal: 'right'
      }}
      transformOrigin={{
        vertical: 'top',
        horizontal: 'right'
      }}
      slotProps={{
        paper: {
          sx: {
            mt: 1.5,
            width: { xs: 'calc(100vw - 32px)', sm: 390 },
            maxWidth: 420,
            borderRadius: '16px',
            boxShadow: '0 12px 36px rgba(15, 23, 42, 0.14), 0 2px 8px rgba(15, 23, 42, 0.06)',
            border: '1px solid rgba(226, 232, 240, 0.8)',
            overflow: 'hidden',
            bgcolor: '#ffffff'
          }
        }
      }}
    >
      {/* Header */}
      <Box
        sx={{
          px: 2.2,
          py: 1.75,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          borderBottom: '1px solid #f1f5f9',
          bgcolor: '#ffffff'
        }}
      >
        <Typography sx={{ fontWeight: 700, fontSize: '1.05rem', color: '#1e293b' }}>
          Thông báo
        </Typography>

        {currentUser && unreadCount > 0 && (
          <Box
            onClick={handleMarkAllRead}
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 0.5,
              cursor: 'pointer',
              color: '#64748b',
              fontSize: '0.82rem',
              fontWeight: 500,
              py: 0.5,
              px: 1,
              borderRadius: '8px',
              transition: 'all 0.15s ease',
              '&:hover': {
                color: '#2563eb',
                bgcolor: 'rgba(37, 99, 235, 0.06)'
              }
            }}
          >
            <CheckRoundedIcon sx={{ fontSize: 16 }} />
            <span>Đã đọc tất cả</span>
          </Box>
        )}
      </Box>

      {/* List Container */}
      <Box
        sx={{
          maxHeight: 440,
          overflowY: 'auto',
          '&::-webkit-scrollbar': { width: 5 },
          '&::-webkit-scrollbar-track': { bgcolor: 'transparent' },
          '&::-webkit-scrollbar-thumb': {
            bgcolor: 'rgba(148, 163, 184, 0.4)',
            borderRadius: 4
          }
        }}
      >
        {!currentUser ? (
          <Box sx={{ p: 4, textAlign: 'center', color: '#64748b' }}>
            <NotificationsNoneRoundedIcon sx={{ fontSize: 44, color: '#94a3b8', mb: 1.5 }} />
            <Typography sx={{ fontSize: '0.92rem', fontWeight: 600, color: '#334155', mb: 0.5 }}>
              Chưa đăng nhập
            </Typography>
            <Typography sx={{ fontSize: '0.8rem', color: '#64748b', mb: 2 }}>
              Vui lòng đăng nhập để nhận và xem các thông báo mới nhất.
            </Typography>
            <Button
              variant="contained"
              size="small"
              onClick={() => {
                onClose()
                navigate('/login')
              }}
              sx={{
                borderRadius: '20px',
                textTransform: 'none',
                fontWeight: 600,
                px: 2.5
              }}
            >
              Đăng nhập ngay
            </Button>
          </Box>
        ) : isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 5 }}>
            <CircularProgress size={28} />
          </Box>
        ) : items.length === 0 ? (
          <Box sx={{ p: 4.5, textAlign: 'center', color: '#64748b' }}>
            <NotificationsOffRoundedIcon sx={{ fontSize: 42, color: '#cbd5e1', mb: 1.25 }} />
            <Typography sx={{ fontSize: '0.9rem', fontWeight: 600, color: '#475569' }}>
              Bạn chưa có thông báo mới
            </Typography>
            <Typography sx={{ fontSize: '0.78rem', color: '#94a3b8', mt: 0.5 }}>
              Các cập nhật về đơn hàng và tin tức hệ thống sẽ hiển thị tại đây.
            </Typography>
          </Box>
        ) : (
          items.map((item) => {
            const isOrder = item.refType === 'ParentOrder' || item.refType === 'SubOrder' || item.type === 'OrderPlaced'
            return (
              <Box
                key={item.id}
                onClick={() => handleItemClick(item)}
                sx={{
                  display: 'flex',
                  alignItems: 'flex-start',
                  gap: 1.5,
                  p: '14px 18px',
                  borderBottom: '1px solid #f1f5f9',
                  bgcolor: item.isRead ? '#ffffff' : 'rgba(238, 242, 255, 0.45)',
                  cursor: 'pointer',
                  transition: 'background-color 0.15s ease',
                  '&:hover': {
                    bgcolor: 'rgba(241, 245, 249, 0.7)'
                  }
                }}
              >
                {/* Status Icon */}
                <Box
                  sx={{
                    width: 36,
                    height: 36,
                    borderRadius: '50%',
                    flexShrink: 0,
                    bgcolor: isOrder ? '#f0fdf4' : '#eff6ff',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    mt: 0.25
                  }}
                >
                  {isOrder ? (
                    <CheckCircleOutlineRoundedIcon sx={{ fontSize: 20, color: '#16a34a' }} />
                  ) : (
                    <NotificationsNoneRoundedIcon sx={{ fontSize: 20, color: '#2563eb' }} />
                  )}
                </Box>

                {/* Content */}
                <Box sx={{ flexGrow: 1, minWidth: 0 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 1 }}>
                    <Typography
                      sx={{
                        fontWeight: item.isRead ? 600 : 700,
                        fontSize: '0.88rem',
                        color: '#0f172a',
                        lineHeight: 1.3
                      }}
                    >
                      {item.title}
                    </Typography>
                    {!item.isRead && (
                      <Box
                        sx={{
                          width: 8,
                          height: 8,
                          borderRadius: '50%',
                          bgcolor: '#2563eb',
                          flexShrink: 0
                        }}
                      />
                    )}
                  </Box>

                  <Typography
                    sx={{
                      fontSize: '0.82rem',
                      color: '#475569',
                      mt: 0.5,
                      lineHeight: 1.38,
                      display: '-webkit-box',
                      WebkitLineClamp: 2,
                      WebkitBoxOrient: 'vertical',
                      overflow: 'hidden'
                    }}
                  >
                    {item.content || item.body}
                  </Typography>

                  {/* Footer: relative time & tag pill */}
                  <Box
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'space-between',
                      mt: 1
                    }}
                  >
                    <Typography sx={{ fontSize: '0.74rem', color: '#64748b' }}>
                      {formatRelativeTime(item.createdAt)}
                    </Typography>

                    <Box
                      sx={{
                        display: 'inline-flex',
                        alignItems: 'center',
                        gap: 0.5,
                        px: 1,
                        py: 0.25,
                        borderRadius: '12px',
                        bgcolor: '#f1f5f9'
                      }}
                    >
                      {isOrder ? (
                        <LocalShippingRoundedIcon sx={{ fontSize: 13, color: '#475569' }} />
                      ) : (
                        <PersonRoundedIcon sx={{ fontSize: 13, color: '#475569' }} />
                      )}
                      <Typography sx={{ fontSize: '0.72rem', color: '#475569', fontWeight: 500 }}>
                        {isOrder ? 'Đơn hàng' : 'Cá nhân'}
                      </Typography>
                    </Box>
                  </Box>
                </Box>
              </Box>
            )
          })
        )}
      </Box>
    </Popover>
  )
}

export default NotificationPopover
