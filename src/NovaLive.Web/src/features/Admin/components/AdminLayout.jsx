import { useState } from 'react'
import {
  Box,
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Avatar,
  Badge,
  Menu,
  MenuItem,
  ListItemIcon,
  Divider,
  Chip,
  useTheme,
  useMediaQuery
} from '@mui/material'
import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import MenuIcon from '@mui/icons-material/Menu'
import NotificationsNoneRoundedIcon from '@mui/icons-material/NotificationsNoneRounded'
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded'
import AccountCircleRoundedIcon from '@mui/icons-material/AccountCircleRounded'
import FiberManualRecordIcon from '@mui/icons-material/FiberManualRecord'
import AdminSidebar from './AdminSidebar'
import ErrorBoundary from '~/common/components/ErrorBoundary'
import { useDispatch, useSelector } from 'react-redux'
import { logoutUserAPI, selectCurrentUser } from '~/store/user/userSlice'

export const AdminLayout = () => {
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))
  const [collapsed, setCollapsed] = useState(isMobile)
  const [anchorEl, setAnchorEl] = useState(null)
  const currentUser = useSelector(selectCurrentUser)
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const location = useLocation()

  const handleOpenMenu = (event) => setAnchorEl(event.currentTarget)
  const handleCloseMenu = () => setAnchorEl(null)

  const handleLogout = () => {
    handleCloseMenu()
    dispatch(logoutUserAPI())
    navigate('/login')
  }

  const getPageTitle = () => {
    const path = location.pathname
    if (path.includes('livestreams')) return 'Giám sát Livestream Realtime'
    if (path.includes('disputes')) return 'Trọng tài & Phân xử Tranh chấp (Dispute Arbitration)'
    if (path.includes('shops')) return 'Phê duyệt & Quản trị Shop (KYC)'
    if (path.includes('reports')) return 'Xử lý Khiếu nại & Tố cáo'
    if (path.includes('categories')) return 'Quản lý Cây danh mục'
    if (path.includes('flash-sales')) return 'Chiến dịch Flash Sale toàn sàn'
    if (path.includes('payouts')) return 'Đối soát & Duyệt lệnh rút tiền (Payouts)'
    if (path.includes('users')) return 'Quản trị Người dùng & Phân quyền (RBAC)'
    if (path.includes('reviews')) return 'Kiểm duyệt đánh giá sản phẩm'
    return 'Trung tâm Điều hành Nền tảng (Platform Overview)'
  }

  return (
    <Box sx={{ display: 'flex', height: '100vh', overflow: 'hidden', bgcolor: '#f8fafc' }}>
      {/* Sidebar */}
      <AdminSidebar collapsed={collapsed} setCollapsed={setCollapsed} />

      {/* Main Content Area */}
      <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', height: '100%', overflow: 'hidden' }}>
        {/* Top Navbar */}
        <AppBar
          position="static"
          elevation={0}
          sx={{
            bgcolor: '#ffffff',
            borderBottom: '1px solid #e2e8f0',
            color: '#0f172a',
            px: { xs: 1.5, sm: 3 },
            py: 0.5
          }}
        >
          <Toolbar disableGutters sx={{ minHeight: 64, justifyContent: 'space-between' }}>
            {/* Left: Mobile Toggle & Page Title */}
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
              {isMobile && (
                <IconButton
                  edge="start"
                  onClick={() => setCollapsed(!collapsed)}
                  sx={{ color: '#64748b' }}
                >
                  <MenuIcon />
                </IconButton>
              )}
              <Box>
                <Typography variant="h6" sx={{ fontWeight: 800, fontSize: { xs: '1rem', sm: '1.2rem' }, color: '#0f172a' }}>
                  {getPageTitle()}
                </Typography>
                <Typography variant="caption" sx={{ color: '#64748b', display: { xs: 'none', sm: 'block' } }}>
                  NovaLive Operations & Management System
                </Typography>
              </Box>
            </Box>

            {/* Right: Platform Status, Notifications & Profile */}
            <Box sx={{ display: 'flex', alignItems: 'center', gap: { xs: 1, sm: 2 } }}>
              {/* Platform Health Status Indicator */}
              <Chip
                icon={<FiberManualRecordIcon sx={{ fontSize: '10px !important', color: '#16a34a', animation: 'pulse 2s infinite' }} />}
                label="Hệ thống: Bình thường"
                size="small"
                sx={{
                  bgcolor: 'rgba(22, 163, 74, 0.08)',
                  color: '#15803d',
                  fontWeight: 600,
                  fontSize: '0.75rem',
                  display: { xs: 'none', md: 'inline-flex' },
                  border: '1px solid rgba(22, 163, 74, 0.2)'
                }}
              />

              {/* Notification Bell */}
              <IconButton
                sx={{
                  color: '#64748b',
                  bgcolor: '#f8fafc',
                  border: '1px solid #e2e8f0',
                  '&:hover': { bgcolor: '#f1f5f9' }
                }}
              >
                <Badge badgeContent={5} color="error" sx={{ '& .MuiBadge-badge': { fontSize: 10, height: 18, minWidth: 18 } }}>
                  <NotificationsNoneRoundedIcon fontSize="small" />
                </Badge>
              </IconButton>

              {/* User Avatar & Menu */}
              <Box
                onClick={handleOpenMenu}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: 1.2,
                  cursor: 'pointer',
                  p: 0.5,
                  borderRadius: '10px',
                  '&:hover': { bgcolor: '#f1f5f9' }
                }}
              >
                <Avatar
                  src={currentUser?.user_avatar || currentUser?.avatarUrl}
                  sx={{
                    width: 36,
                    height: 36,
                    bgcolor: '#3465c8',
                    fontSize: '0.9rem',
                    fontWeight: 700
                  }}
                >
                  {(currentUser?.user_name || currentUser?.fullName || 'Admin')[0].toUpperCase()}
                </Avatar>
                <Box sx={{ display: { xs: 'none', lg: 'block' }, textAlign: 'left' }}>
                  <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', lineHeight: 1.2 }}>
                    {currentUser?.user_name || currentUser?.fullName || 'Super Admin'}
                  </Typography>
                  <Typography variant="caption" sx={{ color: '#64748b', fontSize: '0.7rem' }}>
                    Platform Administrator
                  </Typography>
                </Box>
              </Box>

              {/* Profile Dropdown Menu */}
              <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={handleCloseMenu}
                transformOrigin={{ horizontal: 'right', vertical: 'top' }}
                anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
                PaperProps={{
                  sx: {
                    mt: 1.5,
                    minWidth: 200,
                    borderRadius: '12px',
                    boxShadow: '0 10px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.05)',
                    border: '1px solid #e2e8f0'
                  }
                }}
              >
                <Box sx={{ px: 2, py: 1.5 }}>
                  <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
                    {currentUser?.user_name || currentUser?.fullName || 'Super Admin'}
                  </Typography>
                  <Typography variant="caption" sx={{ color: '#64748b' }}>
                    {currentUser?.user_email || currentUser?.email || 'admin@novalive.online'}
                  </Typography>
                </Box>
                <Divider />
                <MenuItem onClick={() => { handleCloseMenu(); navigate('/profile') }}>
                  <ListItemIcon><AccountCircleRoundedIcon fontSize="small" /></ListItemIcon>
                  Hồ sơ cá nhân
                </MenuItem>
                <Divider />
                <MenuItem onClick={handleLogout} sx={{ color: '#dc2626' }}>
                  <ListItemIcon sx={{ color: '#dc2626' }}><LogoutRoundedIcon fontSize="small" /></ListItemIcon>
                  Đăng xuất
                </MenuItem>
              </Menu>
            </Box>
          </Toolbar>
        </AppBar>

        {/* Scrollable Page Outlet */}
        <Box
          component="main"
          sx={{
            flex: 1,
            overflowY: 'auto',
            p: { xs: 2, sm: 3 },
            bgcolor: '#f8fafc'
          }}
        >
          <ErrorBoundary>
            <Outlet />
          </ErrorBoundary>
        </Box>
      </Box>
    </Box>
  )
}

export default AdminLayout
