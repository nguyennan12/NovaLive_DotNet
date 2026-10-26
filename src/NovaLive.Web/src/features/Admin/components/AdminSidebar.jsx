import { useState } from 'react'
import {
  Box,
  Typography,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Chip,
  IconButton,
  Divider,
  Tooltip,
  useTheme,
  useMediaQuery
} from '@mui/material'
import { useLocation, useNavigate } from 'react-router-dom'
import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import LiveTvRoundedIcon from '@mui/icons-material/LiveTvRounded'
import CategoryRoundedIcon from '@mui/icons-material/CategoryRounded'
import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import AccountBalanceWalletRoundedIcon from '@mui/icons-material/AccountBalanceWalletRounded'
import ReportProblemRoundedIcon from '@mui/icons-material/ReportProblemRounded'
import RateReviewRoundedIcon from '@mui/icons-material/RateReviewRounded'
import PeopleAltRoundedIcon from '@mui/icons-material/PeopleAltRounded'
import GavelRoundedIcon from '@mui/icons-material/GavelRounded'
import KeyboardDoubleArrowLeftRoundedIcon from '@mui/icons-material/KeyboardDoubleArrowLeftRounded'
import KeyboardDoubleArrowRightRoundedIcon from '@mui/icons-material/KeyboardDoubleArrowRightRounded'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import HomeRoundedIcon from '@mui/icons-material/HomeRounded'
import { gradientText } from '~/theme'

const ADMIN_NAV_GROUPS = [
  {
    title: 'Giám sát & Điều hành',
    items: [
      { key: 'dashboard', label: 'Tổng quan sàn', icon: DashboardRoundedIcon, path: '/admin/dashboard' },
      { key: 'livestreams', label: 'Giám sát Live', icon: LiveTvRoundedIcon, path: '/admin/livestreams', badge: '18 Live', badgeColor: 'error' },
      { key: 'disputes', label: 'Phân xử Tranh chấp', icon: GavelRoundedIcon, path: '/admin/disputes', badge: '2 vụ việc', badgeColor: 'error' },
      { key: 'reports', label: 'Khiếu nại & Tố cáo', icon: ReportProblemRoundedIcon, path: '/admin/reports', badge: '4 mới', badgeColor: 'warning' }
    ]
  },
  {
    title: 'Quản trị Đối tác & Hàng hóa',
    items: [
      { key: 'shops', label: 'Duyệt Shop (KYC)', icon: StorefrontRoundedIcon, path: '/admin/shops', badge: '3 chờ duyệt', badgeColor: 'info' },
      { key: 'users', label: 'Người dùng & RBAC', icon: PeopleAltRoundedIcon, path: '/admin/users' },
      { key: 'categories', label: 'Cây danh mục', icon: CategoryRoundedIcon, path: '/admin/categories' },
      { key: 'flash-sales', label: 'Flash Sale toàn sàn', icon: LocalOfferRoundedIcon, path: '/admin/flash-sales' }
    ]
  },
  {
    title: 'Tài chính & Đánh giá',
    items: [
      { key: 'payouts', label: 'Duyệt Payout (Ví)', icon: AccountBalanceWalletRoundedIcon, path: '/admin/payouts', badge: '2 lệnh', badgeColor: 'success' },
      { key: 'reviews', label: 'Kiểm duyệt đánh giá', icon: RateReviewRoundedIcon, path: '/admin/reviews' }
    ]
  }
]

export const AdminSidebar = ({ collapsed, setCollapsed }) => {
  const location = useLocation()
  const navigate = useNavigate()
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))

  return (
    <Box
      component="aside"
      sx={{
        width: collapsed ? 72 : 260,
        minWidth: collapsed ? 72 : 260,
        height: '100%',
        bgcolor: '#ffffff',
        borderRight: '1px solid #e2e8f0',
        display: 'flex',
        flexDirection: 'column',
        transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
        overflow: 'hidden',
        boxShadow: '0 4px 20px rgba(0, 0, 0, 0.03)',
        zIndex: 10
      }}
    >
      {/* Brand Header */}
      <Box
        sx={{
          p: collapsed ? 1.5 : 2.5,
          display: 'flex',
          alignItems: 'center',
          justifyContent: collapsed ? 'center' : 'space-between',
          borderBottom: '1px solid #f1f5f9',
          minHeight: 68
        }}
      >
        <Box
          onClick={() => navigate('/admin/dashboard')}
          sx={{
            display: 'flex',
            alignItems: 'center',
            gap: 1.5,
            cursor: 'pointer'
          }}
        >
          <Box
            sx={{
              width: 38,
              height: 38,
              borderRadius: '10px',
              background: 'linear-gradient(135deg, #3465c8 0%, #1e40af 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: '#ffffff',
              boxShadow: '0 4px 12px rgba(37, 99, 235, 0.25)',
              flexShrink: 0
            }}
          >
            <PlayArrowIcon sx={{ fontSize: '1.4rem' }} />
          </Box>
          {!collapsed && (
            <Box>
              <Typography
                variant="h6"
                sx={{
                  fontWeight: 800,
                  fontSize: '1.15rem',
                  lineHeight: 1.1,
                  ...gradientText
                }}
              >
                NovaLive
              </Typography>
              <Typography
                variant="caption"
                sx={{
                  color: '#64748b',
                  fontWeight: 700,
                  fontSize: '0.7rem',
                  textTransform: 'uppercase',
                  letterSpacing: '0.08em'
                }}
              >
                Admin Platform
              </Typography>
            </Box>
          )}
        </Box>

        {!isMobile && !collapsed && (
          <IconButton
            size="small"
            onClick={() => setCollapsed(!collapsed)}
            sx={{
              color: '#94a3b8',
              bgcolor: '#f8fafc',
              border: '1px solid #e2e8f0',
              '&:hover': { bgcolor: '#f1f5f9', color: '#334155' }
            }}
          >
            <KeyboardDoubleArrowLeftRoundedIcon fontSize="small" />
          </IconButton>
        )}
      </Box>

      {/* Navigation Groups */}
      <Box
        sx={{
          flex: 1,
          overflowY: 'auto',
          overflowX: 'hidden',
          py: 2,
          px: collapsed ? 1 : 1.5,
          '&::-webkit-scrollbar': { width: 4 },
          '&::-webkit-scrollbar-thumb': { bgcolor: '#cbd5e1', borderRadius: 2 }
        }}
      >
        {ADMIN_NAV_GROUPS.map((group, groupIdx) => (
          <Box key={groupIdx} sx={{ mb: 2 }}>
            {!collapsed && (
              <Typography
                variant="caption"
                sx={{
                  px: 1.5,
                  mb: 0.8,
                  display: 'block',
                  color: '#94a3b8',
                  fontWeight: 700,
                  fontSize: '0.7rem',
                  textTransform: 'uppercase',
                  letterSpacing: '0.05em'
                }}
              >
                {group.title}
              </Typography>
            )}
            <List disablePadding>
              {group.items.map((item) => {
                const Icon = item.icon
                const isActive = location.pathname === item.path
                return (
                  <ListItem key={item.key} disablePadding sx={{ mb: 0.5 }}>
                    <Tooltip title={collapsed ? item.label : ''} placement="right" arrow>
                      <ListItemButton
                        onClick={() => navigate(item.path)}
                        sx={{
                          borderRadius: '10px',
                          py: 1,
                          px: collapsed ? 1.5 : 1.5,
                          justifyContent: collapsed ? 'center' : 'flex-start',
                          bgcolor: isActive ? 'rgba(52, 101, 200, 0.08)' : 'transparent',
                          color: isActive ? '#1e40af' : '#475569',
                          fontWeight: isActive ? 700 : 500,
                          transition: 'all 0.15s ease',
                          '&:hover': {
                            bgcolor: isActive ? 'rgba(52, 101, 200, 0.12)' : '#f8fafc',
                            color: '#1e40af'
                          }
                        }}
                      >
                        <ListItemIcon
                          sx={{
                            minWidth: collapsed ? 'auto' : 36,
                            color: isActive ? '#2563eb' : '#64748b',
                            justifyContent: 'center'
                          }}
                        >
                          <Icon sx={{ fontSize: '1.25rem' }} />
                        </ListItemIcon>
                        {!collapsed && (
                          <>
                            <ListItemText
                              primary={item.label}
                              primaryTypographyProps={{
                                fontSize: '0.875rem',
                                fontWeight: isActive ? 700 : 500
                              }}
                            />
                            {item.badge && (
                              <Chip
                                label={item.badge}
                                size="small"
                                color={item.badgeColor || 'default'}
                                sx={{
                                  height: 20,
                                  fontSize: '0.68rem',
                                  fontWeight: 700,
                                  borderRadius: '6px'
                                }}
                              />
                            )}
                          </>
                        )}
                      </ListItemButton>
                    </Tooltip>
                  </ListItem>
                )
              })}
            </List>
          </Box>
        ))}
      </Box>

      {/* Bottom Switcher / Collapse Toggle */}
      <Divider sx={{ borderColor: '#f1f5f9' }} />
      <Box sx={{ p: 1.5 }}>
        <Tooltip title={collapsed ? 'Trở về trang chủ' : ''} placement="right">
          <ListItemButton
            onClick={() => navigate('/')}
            sx={{
              borderRadius: '10px',
              py: 1,
              px: 1.5,
              color: '#64748b',
              justifyContent: collapsed ? 'center' : 'flex-start',
              '&:hover': { bgcolor: '#f8fafc', color: '#0f172a' }
            }}
          >
            <ListItemIcon sx={{ minWidth: collapsed ? 'auto' : 36, color: 'inherit' }}>
              <HomeRoundedIcon fontSize="small" />
            </ListItemIcon>
            {!collapsed && (
              <ListItemText
                primary="Về trang mua sắm"
                primaryTypographyProps={{ fontSize: '0.85rem', fontWeight: 600 }}
              />
            )}
          </ListItemButton>
        </Tooltip>

        {collapsed && !isMobile && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 1 }}>
            <IconButton
              size="small"
              onClick={() => setCollapsed(false)}
              sx={{ color: '#94a3b8', bgcolor: '#f8fafc', border: '1px solid #e2e8f0' }}
            >
              <KeyboardDoubleArrowRightRoundedIcon fontSize="small" />
            </IconButton>
          </Box>
        )}
      </Box>
    </Box>
  )
}

export default AdminSidebar
