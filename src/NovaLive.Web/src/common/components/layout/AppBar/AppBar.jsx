import ChatBubbleOutlineRoundedIcon from '@mui/icons-material/ChatBubbleOutlineRounded'
import ClearIcon from '@mui/icons-material/Clear'
import NotificationsNoneIcon from '@mui/icons-material/NotificationsNone'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import SearchIcon from '@mui/icons-material/Search'
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined'
import Badge from '@mui/material/Badge'
import Box from '@mui/material/Box'
import IconButton from '@mui/material/IconButton'
import InputBase from '@mui/material/InputBase'
import { useTheme } from '@mui/material/styles'
import Tooltip from '@mui/material/Tooltip'
import Typography from '@mui/material/Typography'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { gradientText } from '~/theme'

const iconBoxSx = {
  display: 'flex', alignItems: 'center', justifyContent: 'center',
  minWidth: 36, height: 36,
  bgcolor: 'rgba(83,155,255,0.10)',
  border: '1px solid rgba(83,155,255,0.25)',
  borderRadius: '50%',
  color: 'secondary.main'
}

function AppBar() {
  const [searchValue, setSearchValue] = useState('')
  const navigate = useNavigate()
  const theme = useTheme()

  const handleSearch = (e) => {
    e.preventDefault()
    const keyword = searchValue
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
        sx={{ flexShrink: 0, cursor: 'pointer', userSelect: 'none', display: 'flex', alignItems: 'center', gap: 0.75 }}
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

      {/* Search */}
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
          <IconButton type="submit" edge="end">
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

      {/* Right icons: thông báo, chat, cài đặt, profile */}
      <Box sx={{ flexShrink: 0, display: 'flex', alignItems: 'center', gap: { xs: 0.25, sm: 0.5 } }}>

        <IconButton size="small">
          <Badge badgeContent={3} color="error" max={9}
            sx={{ '& .MuiBadge-badge': { fontSize: '0.6rem', minWidth: 16, height: 16, p: '0 3px' } }}
          >
            <Tooltip title='Thông báo'>
              <Box sx={iconBoxSx}>
                <NotificationsNoneIcon sx={{ fontSize: '1.2rem' }} />
              </Box>
            </Tooltip>
          </Badge>
        </IconButton>

        <IconButton size="small">
          <Tooltip title='Tin nhắn'>
            <Box sx={iconBoxSx}>
              <ChatBubbleOutlineRoundedIcon sx={{ fontSize: '1.2rem' }} />
            </Box>
          </Tooltip>
        </IconButton>

        <IconButton size="small">
          <Tooltip title='Cài đặt'>
            <Box sx={iconBoxSx}>
              <SettingsOutlinedIcon sx={{ fontSize: '1.2rem' }} />
            </Box>
          </Tooltip>
        </IconButton>
      </Box>
    </Box>
  )
}

export default AppBar
