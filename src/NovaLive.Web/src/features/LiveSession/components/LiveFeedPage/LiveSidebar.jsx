import { useState, useEffect } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import {
  Box,
  Typography,
  IconButton,
  InputBase,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Avatar,
  CircularProgress
} from '@mui/material'
import ChevronLeftRoundedIcon from '@mui/icons-material/ChevronLeftRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import HomeRoundedIcon from '@mui/icons-material/HomeRounded'
import ExploreOutlinedIcon from '@mui/icons-material/ExploreOutlined'
import PeopleOutlineRoundedIcon from '@mui/icons-material/PeopleOutlineRounded'
import LiveTvRoundedIcon from '@mui/icons-material/LiveTvRounded'
import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined'
import PersonOutlineRoundedIcon from '@mui/icons-material/PersonOutlineRounded'
import PlayArrowRoundedIcon from '@mui/icons-material/PlayArrowRounded'
import { getFollowedShopsAPI } from '~/common/apis/services/shopService'
import { getMyOrdersAPI } from '~/common/apis/services/orderService'

export const LiveSidebar = ({ onToggleCollapse, onSelectLive }) => {
  const navigate = useNavigate()
  const location = useLocation()
  const [searchTerm, setSearchTerm] = useState('')
  const [followedShops, setFollowedShops] = useState([])
  const [loadingShops, setLoadingShops] = useState(true)
  const [orderCount, setOrderCount] = useState(0)

  useEffect(() => {
    let mounted = true
    const fetchData = async () => {
      try {
        const shops = await getFollowedShopsAPI()
        if (mounted && Array.isArray(shops)) {
          setFollowedShops(shops)
        }
      } catch {
        // Fallback gracefully
      } finally {
        if (mounted) setLoadingShops(false)
      }

      try {
        const ordersRes = await getMyOrdersAPI({ page: 1, limit: 10 })
        const total = ordersRes?.totalCount ?? ordersRes?.total ?? (Array.isArray(ordersRes?.items) ? ordersRes.items.length : 0)
        if (mounted && total > 0) {
          setOrderCount(total)
        }
      } catch {
        // Guest or unauthenticated
      }
    }

    fetchData()
    return () => { mounted = false }
  }, [])

  const handleSearchSubmit = (e) => {
    if (e.key === 'Enter' && searchTerm.trim()) {
      navigate(`/products?search=${encodeURIComponent(searchTerm.trim())}`)
    }
  }

  const navItems = [
    { label: 'Dành cho bạn', icon: <HomeRoundedIcon fontSize="small" />, path: '/' },
    { label: 'Khám phá', icon: <ExploreOutlinedIcon fontSize="small" />, path: '/products' },
    { label: 'Đang theo dõi', icon: <PeopleOutlineRoundedIcon fontSize="small" />, path: '/products' },
    { label: 'LIVE trực tiếp', icon: <LiveTvRoundedIcon fontSize="small" />, path: '/live', active: true },
    { label: 'Đơn hàng', icon: <ReceiptLongOutlinedIcon fontSize="small" />, path: '/orders', badge: orderCount },
    { label: 'Hồ sơ', icon: <PersonOutlineRoundedIcon fontSize="small" />, path: '/profile' }
  ]

  const getAvatarBg = (index) => {
    const colors = ['#2563eb', '#3b82f6', '#0284c7', '#0891b2', '#4f46e5']
    return colors[index % colors.length]
  }

  return (
    <Box
      sx={{
        width: '100%',
        height: '100%',
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
      {/* Brand Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2.2, flexShrink: 0 }}>
        <Box
          onClick={() => navigate('/')}
          sx={{ display: 'flex', alignItems: 'center', gap: 1.2, cursor: 'pointer' }}
        >
          <Box
            sx={{
              width: 34,
              height: 34,
              borderRadius: '10px',
              bgcolor: '#2563eb',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: '#ffffff',
              boxShadow: '0 4px 10px rgba(37, 99, 235, 0.25)'
            }}
          >
            <PlayArrowRoundedIcon sx={{ fontSize: 22 }} />
          </Box>
          <Typography sx={{ fontWeight: 700, fontSize: '1.2rem', color: '#0f172a', letterSpacing: '-0.3px' }}>
            NovaLive
          </Typography>
        </Box>

        {onToggleCollapse && (
          <IconButton
            size="small"
            onClick={onToggleCollapse}
            sx={{
              width: 28,
              height: 28,
              bgcolor: '#f1f5f9',
              color: '#64748b',
              '&:hover': { bgcolor: '#e2e8f0', color: '#0f172a' }
            }}
          >
            <ChevronLeftRoundedIcon fontSize="small" />
          </IconButton>
        )}
      </Box>

      {/* Search Input */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 1,
          px: 1.5,
          py: 0.8,
          borderRadius: '9999px',
          bgcolor: '#f8fafc',
          border: '1px solid #e2e8f0',
          mb: 2,
          flexShrink: 0,
          '&:focus-within': {
            borderColor: '#3b82f6',
            bgcolor: '#ffffff',
            boxShadow: '0 0 0 3px rgba(59, 130, 246, 0.1)'
          }
        }}
      >
        <SearchRoundedIcon sx={{ color: '#94a3b8', fontSize: 20 }} />
        <InputBase
          placeholder="Tìm kiếm"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          onKeyDown={handleSearchSubmit}
          sx={{
            fontSize: '0.875rem',
            color: '#1e293b',
            width: '100%',
            '& input::placeholder': { color: '#94a3b8', opacity: 1 }
          }}
        />
      </Box>

      {/* Nav List */}
      <List disablePadding sx={{ display: 'flex', flexDirection: 'column', gap: 0.5, mb: 1.5, flexShrink: 0 }}>
        {navItems.map((item) => {
          const isActive = item.active || location.pathname === item.path
          return (
            <ListItemButton
              key={item.label}
              onClick={() => navigate(item.path)}
              sx={{
                borderRadius: '14px',
                py: 0.9,
                px: 1.6,
                bgcolor: isActive ? '#e8f1fd' : 'transparent',
                color: isActive ? '#2563eb' : '#475569',
                '&:hover': {
                  bgcolor: isActive ? '#e8f1fd' : '#f8fafc',
                  color: isActive ? '#2563eb' : '#0f172a'
                }
              }}
            >
              <ListItemIcon
                sx={{
                  minWidth: 32,
                  color: isActive ? '#2563eb' : '#64748b'
                }}
              >
                {item.icon}
              </ListItemIcon>
              <ListItemText
                primary={item.label}
                primaryTypographyProps={{
                  fontSize: '0.88rem',
                  fontWeight: isActive ? 700 : 500
                }}
              />
              {Boolean(item.badge) && item.badge > 0 && (
                <Box
                  sx={{
                    minWidth: 18,
                    height: 18,
                    px: 0.6,
                    borderRadius: '9999px',
                    bgcolor: '#2563eb',
                    color: '#ffffff',
                    fontSize: '0.72rem',
                    fontWeight: 700,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center'
                  }}
                >
                  {item.badge}
                </Box>
              )}
            </ListItemButton>
          )
        })}
      </List>

      {/* Divider */}
      <Box sx={{ height: '1px', bgcolor: '#f1f5f9', mb: 1.5, flexShrink: 0 }} />

      {/* Followed Channels Section */}
      <Box sx={{ flex: 1, minHeight: 0, overflowY: 'auto', pr: 0.5 }}>
        <Typography
          sx={{
            fontSize: '0.825rem',
            fontWeight: 700,
            color: '#64748b',
            mb: 1.2,
            px: 0.5
          }}
        >
          Kênh đang theo dõi
        </Typography>

        {loadingShops ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 2 }}>
            <CircularProgress size={20} sx={{ color: '#94a3b8' }} />
          </Box>
        ) : followedShops.length === 0 ? (
          <Typography sx={{ fontSize: '0.8rem', color: '#94a3b8', px: 0.5, py: 1 }}>
            Chưa có kênh nào theo dõi
          </Typography>
        ) : (
          <List disablePadding sx={{ display: 'flex', flexDirection: 'column', gap: 0.8 }}>
            {followedShops.slice(0, 5).map((shop, idx) => (
              <Box
                key={shop.id}
                onClick={() => {
                  if (shop.isLive && shop.activeLiveId && onSelectLive) {
                    onSelectLive(shop.activeLiveId)
                  } else {
                    navigate(`/products?search=${encodeURIComponent(shop.shopName)}`)
                  }
                }}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 1.2,
                  p: 0.8,
                  borderRadius: '12px',
                  cursor: 'pointer',
                  transition: 'background-color 0.15s ease',
                  '&:hover': { bgcolor: '#f8fafc' }
                }}
              >
                <Avatar
                  src={shop.logoUrl}
                  sx={{
                    width: 36,
                    height: 36,
                    bgcolor: getAvatarBg(idx),
                    fontSize: '0.875rem',
                    fontWeight: 700,
                    color: '#ffffff'
                  }}
                >
                  {shop.shopName ? shop.shopName.charAt(0).toUpperCase() : 'S'}
                </Avatar>

                <Box sx={{ minWidth: 0, flex: 1 }}>
                  <Typography
                    noWrap
                    sx={{
                      fontSize: '0.86rem',
                      fontWeight: 600,
                      color: '#0f172a',
                      lineHeight: 1.2
                    }}
                  >
                    {shop.shopName}
                  </Typography>

                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.2 }}>
                    {shop.isLive ? (
                      <>
                        <Box
                          sx={{
                            width: 6,
                            height: 6,
                            borderRadius: '50%',
                            bgcolor: '#ef4444',
                            boxShadow: '0 0 6px rgba(239, 68, 68, 0.6)'
                          }}
                        />
                        <Typography sx={{ fontSize: '0.72rem', color: '#ef4444', fontWeight: 600 }}>
                          Đang livestream
                        </Typography>
                      </>
                    ) : (
                      <Typography
                        noWrap
                        sx={{
                          fontSize: '0.74rem',
                          color: '#64748b'
                        }}
                      >
                        {shop.description || 'Gian hàng chính hãng'}
                      </Typography>
                    )}
                  </Box>
                </Box>
              </Box>
            ))}

            {followedShops.length > 5 && (
              <Typography
                onClick={() => navigate('/products')}
                sx={{
                  fontSize: '0.8rem',
                  fontWeight: 600,
                  color: '#2563eb',
                  cursor: 'pointer',
                  px: 0.5,
                  pt: 0.5,
                  '&:hover': { textDecoration: 'underline' }
                }}
              >
                Xem thêm
              </Typography>
            )}
          </List>
        )}
      </Box>

      {/* Footer */}
      <Box sx={{ pt: 1.5, borderTop: '1px solid #f1f5f9', flexShrink: 0 }}>
        <Typography
          sx={{
            fontSize: '0.72rem',
            color: '#94a3b8',
            display: 'flex',
            alignItems: 'center',
            gap: 0.6,
            whiteSpace: 'nowrap',
            mb: 0.4
          }}
        >
          <span style={{ cursor: 'pointer' }}>Về NovaLive</span>
          <span style={{ color: '#cbd5e1' }}>•</span>
          <span style={{ cursor: 'pointer' }}>Điều khoản</span>
          <span style={{ color: '#cbd5e1' }}>•</span>
          <span style={{ cursor: 'pointer' }}>Bảo mật</span>
        </Typography>
        <Typography sx={{ fontSize: '0.72rem', color: '#94a3b8' }}>
          © 2026 NovaLive
        </Typography>
      </Box>
    </Box>
  )
}
