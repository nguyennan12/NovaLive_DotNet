import ChatBubbleOutlineRoundedIcon from '@mui/icons-material/ChatBubbleOutlineRounded'
import ClearIcon from '@mui/icons-material/Clear'
import LoginRoundedIcon from '@mui/icons-material/LoginRounded'
import NotificationsNoneIcon from '@mui/icons-material/NotificationsNone'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import SearchIcon from '@mui/icons-material/Search'
import Badge from '@mui/material/Badge'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import InputBase from '@mui/material/InputBase'
import { useTheme } from '@mui/material/styles'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import { useState } from 'react'
import { useSelector } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import Profiles, { iconBoxSx } from './Menu/Profile'
import NotificationPopover from './NotificationPopover'
import { getNotificationsAPI } from '~/common/apis/services/notificationService'
import { selectCurrentUser } from '~/store/user/userSlice'
import { gradientText } from '~/theme'

function AppBar() {
  const [searchValue, setSearchValue] = useState('')
  const [notifAnchor, setNotifAnchor] = useState(null)
  const navigate = useNavigate()
  const theme = useTheme()
  const currentUser = useSelector(selectCurrentUser)

  const { data: notifData } = useQuery({
    queryKey: ['user_notifications'],
    queryFn: () => getNotificationsAPI({ page: 1, size: 20 }),
    enabled: Boolean(currentUser),
    refetchInterval: 30000,
    staleTime: 10000
  })

  const notifItems = notifData?.items || (Array.isArray(notifData) ? notifData : [])
  const unreadNotifCount = currentUser
    ? (Array.isArray(notifItems) ? notifItems.filter((n) => !n.isRead).length : 0)
    : 0

  const handleSearch = (e) => {
    e.preventDefault()
    const keyword = searchValue.trim()
    if (keyword) {
      navigate(`/products?keyword=${encodeURIComponent(keyword)}`)
      setSearchValue('')
    }
  }

  return (
    <Box
      component="header"
      sx={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 1200,
        height: theme.app.appBarHeight,
        background: 'rgba(248,250,255,0.78)',
        backdropFilter: 'blur(20px)',
        WebkitBackdropFilter: 'blur(20px)',
        borderBottom: '1px solid rgba(255,255,255,0.5)',
        boxShadow: '0 2px 16px rgba(52,133,247,0.07)',
        display: 'flex',
        alignItems: 'center',
        px: { xs: 2, sm: 3, md: 5 },
        gap: { xs: 1.5, md: 3 }
      }}
    >
      {/* Logo */}
      <Box
        onClick={() => navigate('/')}
        sx={{
          flexShrink: 0,
          cursor: 'pointer',
          userSelect: 'none',
          display: 'flex',
          alignItems: 'center',
          gap: 0.75
        }}
      >
        <Box sx={{
          width: 28, height: 28, borderRadius: '8px',
          background: 'linear-gradient(90deg, #69bef7ff, #53e6eeff)',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          boxShadow: '0 2px 8px rgba(52,133,247,0.3)'
        }}>
          <PlayArrowIcon sx={{ color: '#fff', fontSize: '1.25rem' }} />
        </Box>
        <Typography variant="h6" sx={{
          fontWeight: 800,
          fontSize: { xs: '1rem', sm: '1.1rem' },
          ...gradientText,
          letterSpacing: '-0.4px',
          lineHeight: 1,
          display: { xs: 'none', sm: 'block' }
        }}>
          NovaLive
        </Typography>
      </Box>

      {/* Search Bar */}
      <Box sx={{ flex: 1, display: 'flex', justifyContent: 'center' }}>
        <Box component="form"
          onSubmit={handleSearch}
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1,
            width: '100%',
            maxWidth: { xs: '100%', md: '600px' },
            height: '40px',
            borderRadius: '50px',
            background: 'rgba(52,133,247,0.05)',
            border: '1.5px solid rgba(52,133,247,0.12)',
            px: 2,
            transition: 'all 0.22s ease',
            '&:focus-within': {
              background: 'rgba(52,133,247,0.08)',
              border: '1.5px solid rgba(52,133,247,0.38)',
              boxShadow: '0 0 0 3px rgba(52,133,247,0.08)'
            }
          }}>
          <IconButton type="submit" edge="end" sx={{ p: 0.5 }}>
            <SearchIcon sx={{ fontSize: 18, color: 'rgba(52,133,247,0.55)', flexShrink: 0 }} />
          </IconButton>

          <InputBase
            placeholder="Tìm kiếm sản phẩm, shop..."
            value={searchValue}
            onChange={(e) => setSearchValue(e.target.value)}
            sx={{
              flex: 1,
              fontSize: '0.875rem',
              '& input': {
                color: '#2d2d2d',
                '&::placeholder': { color: 'rgba(45,45,45,0.38)', opacity: 1 }
              }
            }}
          />
          {searchValue && (
            <ClearIcon
              onClick={() => setSearchValue('')}
              sx={{
                fontSize: 16, cursor: 'pointer', flexShrink: 0,
                color: 'rgba(45,45,45,0.35)',
                '&:hover': { color: 'rgba(45,45,45,0.7)' }
              }}
            />
          )}
        </Box>
      </Box>

      {/* Right actions: Thông báo -> Tin nhắn -> Profile / Đăng nhập */}
      <Box sx={{ flexShrink: 0, display: 'flex', alignItems: 'center', gap: { xs: 0.75, sm: 1 } }}>
        {/* Notifications */}
        <IconButton
          size="small"
          onClick={(e) => setNotifAnchor(e.currentTarget)}
          sx={{ p: 0.25 }}
        >
          <Badge
            badgeContent={unreadNotifCount}
            color="error"
            max={9}
            sx={{ '& .MuiBadge-badge': { fontSize: '0.6rem', minWidth: 16, height: 16, p: '0 3px' } }}
          >
            <Box sx={iconBoxSx}>
              <NotificationsNoneIcon sx={{ fontSize: '1.2rem' }} />
            </Box>
          </Badge>
        </IconButton>

        <NotificationPopover
          anchorEl={notifAnchor}
          open={Boolean(notifAnchor)}
          onClose={() => setNotifAnchor(null)}
          currentUser={currentUser}
        />

        {/* Messages */}
        <IconButton size="small" sx={{ p: 0.25 }}>
          <Tooltip title="Tin nhắn">
            <Box sx={iconBoxSx}>
              <ChatBubbleOutlineRoundedIcon sx={{ fontSize: '1.2rem' }} />
            </Box>
          </Tooltip>
        </IconButton>

        {/* Profile (khi đã login) hoặc Nút Đăng nhập (khi chưa login) */}
        {currentUser ? (
          <Profiles />
        ) : (
          <Button
            onClick={() => navigate('/login')}
            startIcon={<LoginRoundedIcon sx={{ fontSize: '16px !important' }} />}
            sx={{
              height: 36,
              borderRadius: '24px',
              px: { xs: 1.5, sm: 2 },
              fontSize: '0.82rem',
              fontWeight: 700,
              textTransform: 'none',
              whiteSpace: 'nowrap',
              color: 'secondary.main',
              bgcolor: 'rgba(83,155,255,0.10)',
              border: '1px solid rgba(83,155,255,0.25)',
              transition: 'all 0.2s ease',
              '&:hover': {
                bgcolor: 'rgba(83,155,255,0.18)',
                borderColor: 'rgba(83,155,255,0.45)',
                transform: 'translateY(-1px)'
              }
            }}
          >
            Đăng nhập
          </Button>
        )}
      </Box>
    </Box>
  )
}

export default AppBar
