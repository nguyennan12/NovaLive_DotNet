import { useState, useEffect } from 'react'
import {
  Box,
  Typography,
  Button,
  CircularProgress,
  Snackbar,
  Alert,
  Avatar,
  CardMedia
} from '@mui/material'
import CalendarMonthRoundedIcon from '@mui/icons-material/CalendarMonthRounded'
import NotificationsNoneRoundedIcon from '@mui/icons-material/NotificationsNoneRounded'
import NotificationsActiveRoundedIcon from '@mui/icons-material/NotificationsActiveRounded'
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded'
import ShoppingBagOutlinedIcon from '@mui/icons-material/ShoppingBagOutlined'
import LocalFireDepartmentRoundedIcon from '@mui/icons-material/LocalFireDepartmentRounded'
import { getUpcomingSessionsAPI } from '~/common/apis/services/liveService'
import { formatVND } from '~/common/utils/formatters'

export const LiveUpcomingScheduleCard = ({
  activeLiveCount = 0,
  currentLive = null,
  activeTab = 'live',
  onTabChange
}) => {
  const [upcomingLives, setUpcomingLives] = useState([])
  const [loading, setLoading] = useState(true)
  const [reminders, setReminders] = useState({})
  const [snackbarMessage, setSnackbarMessage] = useState('')

  const isStreaming = activeLiveCount > 0 && Boolean(currentLive)
  const [selectedTab, setSelectedTab] = useState(isStreaming ? 'live' : 'schedule')

  useEffect(() => {
    if (activeTab) {
      setSelectedTab(activeTab)
    }
  }, [activeTab])

  useEffect(() => {
    let mounted = true
    const fetchUpcoming = async () => {
      try {
        const res = await getUpcomingSessionsAPI({ limit: 10 })
        if (mounted && Array.isArray(res?.lives)) {
          setUpcomingLives(res.lives)
        }
      } catch {
        // Handle error gracefully
      } finally {
        if (mounted) setLoading(false)
      }
    }

    fetchUpcoming()
    return () => { mounted = false }
  }, [])

  const handleToggleReminder = (id, title) => {
    setReminders(prev => {
      const nextState = !prev[id]
      setSnackbarMessage(nextState ? `Đã đặt nhắc hẹn cho phiên "${title}"` : `Đã hủy nhắc hẹn`)
      return { ...prev, [id]: nextState }
    })
  }

  const handleTabClick = (tab) => {
    setSelectedTab(tab)
    if (onTabChange) onTabChange(tab)
  }

  const formatScheduleTime = (dateStr) => {
    if (!dateStr) return { time: '20:00', date: 'Sắp tới' }
    try {
      const date = new Date(dateStr)
      const hours = String(date.getHours()).padStart(2, '0')
      const minutes = String(date.getMinutes()).padStart(2, '0')
      const time = `${hours}:${minutes}`

      const today = new Date()
      const isToday =
        date.getDate() === today.getDate() &&
        date.getMonth() === today.getMonth() &&
        date.getFullYear() === today.getFullYear()

      const tomorrow = new Date()
      tomorrow.setDate(today.getDate() + 1)
      const isTomorrow =
        date.getDate() === tomorrow.getDate() &&
        date.getMonth() === tomorrow.getMonth() &&
        date.getFullYear() === tomorrow.getFullYear()

      let dateLabel = ''
      if (isToday) dateLabel = 'Hôm nay'
      else if (isTomorrow) dateLabel = 'Ngày mai'
      else {
        const days = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy']
        dateLabel = days[date.getDay()]
      }

      return { time, date: dateLabel }
    } catch {
      return { time: '20:00', date: 'Sắp tới' }
    }
  }

  return (
    <Box
      sx={{
        width: '100%',
        height: 'fit-content', // Co ngắn lại theo nội dung, không fullheight
        maxHeight: 'calc(100vh - 32px)',
        bgcolor: '#ffffff',
        borderRadius: '24px',
        p: 2.5,
        display: 'flex',
        flexDirection: 'column',
        boxShadow: '0 4px 20px rgba(0,0,0,0.03)',
        border: '1px solid rgba(226, 232, 240, 0.6)',
        boxSizing: 'border-box',
        overflow: 'hidden'
      }}
    >
      {/* Top Status Switch Pills */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'flex-end',
          mb: 2,
          flexShrink: 0
        }}
      >
        <Box
          sx={{
            display: 'inline-flex',
            alignItems: 'center',
            p: 0.4,
            borderRadius: '9999px',
            bgcolor: '#f1f5f9'
          }}
        >
          {/* Tab 1: Đang live */}
          <Box
            onClick={() => isStreaming && handleTabClick('live')}
            sx={{
              px: 1.8,
              py: 0.6,
              borderRadius: '9999px',
              fontSize: '0.825rem',
              fontWeight: (selectedTab === 'live' && isStreaming) ? 700 : 500,
              bgcolor: (selectedTab === 'live' && isStreaming) ? '#2563eb' : 'transparent',
              color: (selectedTab === 'live' && isStreaming) ? '#ffffff' : '#64748b',
              cursor: isStreaming ? 'pointer' : 'default',
              transition: 'all 0.2s ease',
              userSelect: 'none'
            }}
          >
            Đang live
          </Box>

          {/* Tab 2: Lịch live / Chưa có live */}
          <Box
            onClick={() => handleTabClick('schedule')}
            sx={{
              px: 1.8,
              py: 0.6,
              borderRadius: '9999px',
              fontSize: '0.825rem',
              fontWeight: (!isStreaming || selectedTab === 'schedule') ? 700 : 500,
              bgcolor: (!isStreaming || selectedTab === 'schedule') ? '#2563eb' : 'transparent',
              color: (!isStreaming || selectedTab === 'schedule') ? '#ffffff' : '#64748b',
              cursor: 'pointer',
              transition: 'all 0.2s ease',
              userSelect: 'none'
            }}
          >
            {isStreaming ? 'Lịch live' : 'Chưa có live'}
          </Box>
        </Box>
      </Box>

      {/* CONTENT AREA */}
      {selectedTab === 'live' && isStreaming ? (
        /* LIVE ACTIVE DETAILS VIEW */
        <Box sx={{ flex: 1, minHeight: 0, overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: 2 }}>
          {/* Header */}
          <Box>
            <Typography sx={{ fontSize: '1.15rem', fontWeight: 700, color: '#0f172a', lineHeight: 1.3 }}>
              {currentLive.title || 'Phiên Livestream trực tiếp'}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mt: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.6 }}>
                <Box
                  sx={{
                    width: 8,
                    height: 8,
                    borderRadius: '50%',
                    bgcolor: '#ef4444',
                    boxShadow: '0 0 8px rgba(239, 68, 68, 0.8)'
                  }}
                />
                <Typography sx={{ fontSize: '0.78rem', color: '#ef4444', fontWeight: 700 }}>
                  TRỰC TIẾP
                </Typography>
              </Box>

              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: '#64748b' }}>
                <VisibilityRoundedIcon sx={{ fontSize: 16 }} />
                <Typography sx={{ fontSize: '0.78rem', fontWeight: 600 }}>
                  {currentLive.viewerCount ?? currentLive.live_viewer_count ?? 1} đang xem
                </Typography>
              </Box>
            </Box>
          </Box>

          {/* Shop Card */}
          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1.5,
              p: 1.5,
              borderRadius: '16px',
              bgcolor: '#f8fafc',
              border: '1px solid #e2e8f0'
            }}
          >
            <Avatar
              src={currentLive.shop?.shop_logo || currentLive.shopLogoUrl}
              sx={{ width: 44, height: 44, bgcolor: '#2563eb', fontWeight: 700 }}
            >
              {(currentLive.shop?.shop_name || currentLive.shopName || 'N').charAt(0)}
            </Avatar>
            <Box sx={{ flex: 1, minWidth: 0 }}>
              <Typography noWrap sx={{ fontSize: '0.92rem', fontWeight: 700, color: '#0f172a' }}>
                {currentLive.shop?.shop_name || currentLive.shopName || 'NovaLive Partner'}
              </Typography>
              <Typography sx={{ fontSize: '0.75rem', color: '#64748b', mt: 0.2 }}>
                Kênh bán hàng chính thức
              </Typography>
            </Box>
          </Box>

          {/* Pinned / Highlight Products Section */}
          <Box sx={{ mt: 0.5 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.8, mb: 1.2 }}>
              <LocalFireDepartmentRoundedIcon sx={{ color: '#ef4444', fontSize: 20 }} />
              <Typography sx={{ fontSize: '0.92rem', fontWeight: 700, color: '#0f172a' }}>
                Sản phẩm nổi bật trong live
              </Typography>
            </Box>

            {currentLive.activePinnedProduct ? (
              <Box
                sx={{
                  display: 'flex',
                  gap: 1.5,
                  p: 1.5,
                  borderRadius: '16px',
                  bgcolor: '#eff6ff',
                  border: '1.5px solid #bfdbfe'
                }}
              >
                {currentLive.activePinnedProduct.thumbnailUrl && (
                  <CardMedia
                    component="img"
                    image={currentLive.activePinnedProduct.thumbnailUrl}
                    sx={{ width: 68, height: 68, borderRadius: '10px', objectFit: 'cover' }}
                  />
                )}
                <Box sx={{ flex: 1, minWidth: 0 }}>
                  <Typography noWrap sx={{ fontSize: '0.88rem', fontWeight: 700, color: '#0f172a' }}>
                    {currentLive.activePinnedProduct.spuName}
                  </Typography>
                  <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 1, mt: 0.5 }}>
                    <Typography sx={{ fontSize: '1rem', fontWeight: 800, color: '#2563eb' }}>
                      {formatVND(currentLive.activePinnedProduct.effectivePrice)}
                    </Typography>
                    {currentLive.activePinnedProduct.originalPrice > currentLive.activePinnedProduct.effectivePrice && (
                      <Typography sx={{ fontSize: '0.75rem', color: '#94a3b8', textDecoration: 'line-through' }}>
                        {formatVND(currentLive.activePinnedProduct.originalPrice)}
                      </Typography>
                    )}
                  </Box>
                  <Button
                    size="small"
                    variant="contained"
                    fullWidth
                    sx={{
                      mt: 1,
                      py: 0.6,
                      borderRadius: '8px',
                      textTransform: 'none',
                      fontSize: '0.78rem',
                      fontWeight: 700,
                      bgcolor: '#2563eb',
                      '&:hover': { bgcolor: '#1d4ed8' }
                    }}
                  >
                    Mua ngay trên live
                  </Button>
                </Box>
              </Box>
            ) : (
              <Box
                sx={{
                  p: 2,
                  borderRadius: '16px',
                  bgcolor: '#f8fafc',
                  border: '1px dashed #cbd5e1',
                  textAlign: 'center'
                }}
              >
                <ShoppingBagOutlinedIcon sx={{ fontSize: 28, color: '#94a3b8', mb: 0.5 }} />
                <Typography sx={{ fontSize: '0.8rem', color: '#64748b' }}>
                  Shop đang giới thiệu sản phẩm trực tiếp. Bấm biểu tượng giỏ hàng trong video để xem sản phẩm!
                </Typography>
              </Box>
            )}
          </Box>
        </Box>
      ) : (
        /* UPCOMING SCHEDULE VIEW */
        <Box sx={{ display: 'flex', flexDirection: 'column' }}>
          <Typography
            sx={{
              fontSize: '1.15rem',
              fontWeight: 700,
              color: '#0f172a',
              mb: 1.5,
              flexShrink: 0
            }}
          >
            Lịch live sắp tới
          </Typography>

          <Box sx={{ overflowY: 'auto', pr: 0.5 }}>
            {loading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
                <CircularProgress size={24} sx={{ color: '#2563eb' }} />
              </Box>
            ) : upcomingLives.length === 0 ? (
              <Box
                sx={{
                  display: 'flex',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  py: 3,
                  px: 1.5,
                  textAlign: 'center'
                }}
              >
                <Box
                  sx={{
                    width: 44,
                    height: 44,
                    borderRadius: '50%',
                    bgcolor: '#f1f5f9',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: '#94a3b8',
                    mb: 1.2
                  }}
                >
                  <CalendarMonthRoundedIcon sx={{ fontSize: 24 }} />
                </Box>
                <Typography sx={{ fontSize: '0.88rem', fontWeight: 600, color: '#475569', mb: 0.4 }}>
                  Chưa có lịch phát sóng sắp tới
                </Typography>
                <Typography sx={{ fontSize: '0.78rem', color: '#94a3b8', lineHeight: 1.4, maxWidth: 240 }}>
                  Các phiên livestream được lên lịch bởi shop sẽ xuất hiện tại đây.
                </Typography>
              </Box>
            ) : (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.2 }}>
                {upcomingLives.map((session) => {
                  const { time, date } = formatScheduleTime(session.scheduledAt || session.live_schedule_at)
                  const hasReminder = Boolean(reminders[session.id])
                  const shopName = session.shop?.shop_name || session.shopName || 'NovaLive Partner'

                  return (
                    <Box
                      key={session.id}
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 1.2,
                        p: 1,
                        borderRadius: '14px',
                        transition: 'background-color 0.15s ease',
                        '&:hover': { bgcolor: '#f8fafc' }
                      }}
                    >
                      {/* Time column */}
                      <Box sx={{ minWidth: 58, textAlign: 'left' }}>
                        <Typography
                          sx={{
                            fontSize: '0.92rem',
                            fontWeight: 700,
                            color: '#2563eb',
                            lineHeight: 1.1
                          }}
                        >
                          {time}
                        </Typography>
                        <Typography
                          sx={{
                            fontSize: '0.74rem',
                            color: '#64748b',
                            mt: 0.2
                          }}
                        >
                          {date}
                        </Typography>
                      </Box>

                      {/* Title & Shop column */}
                      <Box sx={{ flex: 1, minWidth: 0 }}>
                        <Typography
                          noWrap
                          sx={{
                            fontSize: '0.86rem',
                            fontWeight: 600,
                            color: '#0f172a',
                            lineHeight: 1.2
                          }}
                        >
                          {session.title || 'Livestream ưu đãi'}
                        </Typography>
                        <Typography
                          noWrap
                          sx={{
                            fontSize: '0.74rem',
                            color: '#64748b',
                            mt: 0.2
                          }}
                        >
                          {shopName}
                        </Typography>
                      </Box>

                      {/* Reminder button */}
                      <Button
                        size="small"
                        variant={hasReminder ? 'contained' : 'outlined'}
                        onClick={() => handleToggleReminder(session.id, session.title)}
                        startIcon={
                          hasReminder ? (
                            <NotificationsActiveRoundedIcon sx={{ fontSize: 15 }} />
                          ) : (
                            <NotificationsNoneRoundedIcon sx={{ fontSize: 15 }} />
                          )
                        }
                        sx={{
                          borderRadius: '9999px',
                          textTransform: 'none',
                          fontSize: '0.76rem',
                          fontWeight: 600,
                          py: 0.4,
                          px: 1.2,
                          minWidth: 'auto',
                          borderColor: '#2563eb',
                          color: hasReminder ? '#ffffff' : '#2563eb',
                          bgcolor: hasReminder ? '#2563eb' : 'transparent',
                          '&:hover': {
                            borderColor: '#1d4ed8',
                            bgcolor: hasReminder ? '#1d4ed8' : '#eff6ff'
                          }
                        }}
                      >
                        {hasReminder ? 'Đã đặt' : 'Nhắc tôi'}
                      </Button>
                    </Box>
                  )
                })}
              </Box>
            )}
          </Box>
        </Box>
      )}

      {/* Toast Notification */}
      <Snackbar
        open={Boolean(snackbarMessage)}
        autoHideDuration={3000}
        onClose={() => setSnackbarMessage('')}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert
          severity="success"
          onClose={() => setSnackbarMessage('')}
          sx={{ borderRadius: '12px', boxShadow: '0 4px 12px rgba(0,0,0,0.1)' }}
        >
          {snackbarMessage}
        </Alert>
      </Snackbar>
    </Box>
  )
}
