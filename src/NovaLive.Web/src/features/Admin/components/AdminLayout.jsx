import { useState } from 'react'
import {
  Avatar,
  Box,
  Breadcrumbs,
  Chip,
  Divider,
  Drawer,
  IconButton,
  ListItemIcon,
  Menu,
  MenuItem,
  Stack,
  Typography,
  useMediaQuery,
  Link as MuiLink
} from '@mui/material'
import MenuRounded from '@mui/icons-material/MenuRounded'
import LogoutRoundedIcon from '@mui/icons-material/LogoutRounded'
import PersonOutlineRoundedIcon from '@mui/icons-material/PersonOutlineRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import CircleIcon from '@mui/icons-material/Circle'
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useDispatch, useSelector } from 'react-redux'
import { logoutUserAPI, selectCurrentUser } from '~/store/user/userSlice'
import AdminSidebar from './AdminSidebar'

const ROUTE_LABELS = {
  '/admin': 'Tổng quan sàn',
  '/admin/dashboard': 'Tổng quan sàn',
  '/admin/shops': 'Phê duyệt & Quản trị Shop',
  '/admin/payouts': 'Duyệt giải ngân ví (Payout)',
  '/admin/livestreams': 'Giám sát Livestream Realtime',
  '/admin/categories': 'Quản lý Cây danh mục',
  '/admin/flash-sales': 'Chiến dịch Flash Sale sàn',
  '/admin/discounts': 'Mã giảm giá toàn sàn',
  '/admin/reviews': 'Kiểm duyệt đánh giá sản phẩm',
  '/admin/users': 'Người dùng & Phân quyền'
}

export const AdminLayout = () => {
  const [collapsed, setCollapsed] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const [anchorEl, setAnchorEl] = useState(null)
  const mobile = useMediaQuery('(max-width:899px)')

  const currentUser = useSelector(selectCurrentUser)
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const location = useLocation()

  const currentLabel = ROUTE_LABELS[location.pathname] || 'Quản trị'

  const handleOpenMenu = (event) => setAnchorEl(event.currentTarget)
  const handleCloseMenu = () => setAnchorEl(null)

  const handleLogout = () => {
    handleCloseMenu()
    dispatch(logoutUserAPI())
    navigate('/login')
  }

  const isSeller = Boolean(currentUser?.shopId || currentUser?.user_shop || currentUser?.roles?.includes('Seller'))

  return (
    <Box
      sx={{
        background: 'linear-gradient(135deg, #f8faff 0%, #f0f4ff 50%, #f5f8ff 100%)',
        minHeight: '100dvh',
        display: 'flex'
      }}
    >
      {/* Desktop Sticky Sidebar */}
      {!mobile && (
        <Box sx={{ position: 'sticky', top: 0, height: '100dvh', p: 2, flexShrink: 0 }}>
          <AdminSidebar
            collapsed={collapsed}
            onCollapse={() => setCollapsed(!collapsed)}
          />
        </Box>
      )}

      {/* Mobile Drawer Sidebar */}
      <Drawer
        open={mobileOpen}
        onClose={() => setMobileOpen(false)}
        slotProps={{ paper: { sx: { bgcolor: '#ffffff', p: 1 } } }}
      >
        <AdminSidebar
          collapsed={false}
          onCollapse={() => setMobileOpen(false)}
          onNavigate={() => setMobileOpen(false)}
        />
      </Drawer>

      {/* Main Content Area */}
      <Box sx={{ flex: 1, minWidth: 0, p: { xs: 1.5, sm: 3 } }}>
        {/* Top Header */}
        <Box
          component="header"
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            gap: 2,
            mb: 3,
            flexWrap: 'wrap'
          }}
        >
          {/* Left: Mobile Toggle & Page Title & Breadcrumbs */}
          <Stack direction="row" alignItems="center" spacing={1.5}>
            {mobile && (
              <IconButton aria-label="Mở menu quản trị" onClick={() => setMobileOpen(true)}>
                <MenuRounded />
              </IconButton>
            )}
            <Box>
              <Typography sx={{ fontWeight: 800, fontSize: '1.25rem', color: '#0f172a' }}>
                {currentLabel}
              </Typography>
              <Breadcrumbs aria-label="Đường dẫn trang" sx={{ fontSize: '0.78rem' }}>
                <MuiLink component={Link} to="/admin/dashboard" color="inherit" underline="hover">
                  Quản trị sàn
                </MuiLink>
                <Typography variant="caption" sx={{ color: 'text.secondary', fontWeight: 600 }}>
                  {currentLabel}
                </Typography>
              </Breadcrumbs>
            </Box>
          </Stack>

          {/* Right: Status Pill & Profile */}
          <Stack direction="row" alignItems="center" spacing={1.5}>
            <Chip
              icon={<CircleIcon sx={{ fontSize: '8px !important', color: '#10b981 !important' }} />}
              label="Hệ thống: Trực tuyến"
              size="small"
              sx={{
                bgcolor: '#ecfdf5',
                color: '#047857',
                border: '1px solid #d1fae5',
                fontWeight: 600,
                fontSize: '0.75rem',
                height: 28,
                display: { xs: 'none', sm: 'inline-flex' }
              }}
            />

            <Box
              onClick={handleOpenMenu}
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1,
                cursor: 'pointer',
                p: 0.5,
                pr: 1.5,
                borderRadius: '24px',
                bgcolor: '#ffffff',
                border: '1px solid #e2e8f0',
                transition: 'all 0.15s ease',
                '&:hover': { bgcolor: 'rgba(0, 149, 255, 0.04)', borderColor: '#cbd5e1' }
              }}
            >
              <Avatar
                src={currentUser?.user_avatar || ''}
                sx={{
                  width: 32,
                  height: 32,
                  fontSize: '0.85rem',
                  fontWeight: 700,
                  bgcolor: 'secondary.main',
                  color: '#ffffff'
                }}
              >
                {currentUser?.user_name?.[0]?.toUpperCase() || 'A'}
              </Avatar>
              <Box sx={{ display: { xs: 'none', sm: 'block' }, textAlign: 'left' }}>
                <Typography sx={{ fontSize: '0.8rem', fontWeight: 700, color: '#0f172a', lineHeight: 1.2 }}>
                  {currentUser?.user_name || 'Admin'}
                </Typography>
                <Typography sx={{ fontSize: '0.68rem', color: '#64748b' }}>
                  Quản trị viên
                </Typography>
              </Box>
            </Box>

            <Menu
              anchorEl={anchorEl}
              open={Boolean(anchorEl)}
              onClose={handleCloseMenu}
              anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
              transformOrigin={{ vertical: 'top', horizontal: 'right' }}
              slotProps={{
                paper: {
                  sx: {
                    mt: 1,
                    minWidth: 180,
                    borderRadius: 2,
                    boxShadow: '0 4px 20px rgba(0,0,0,0.08)'
                  }
                }
              }}
            >
              <MenuItem onClick={() => { handleCloseMenu(); navigate('/profile') }}>
                <ListItemIcon><PersonOutlineRoundedIcon fontSize="small" /></ListItemIcon>
                Hồ sơ cá nhân
              </MenuItem>
              {isSeller && (
                <MenuItem onClick={() => { handleCloseMenu(); navigate('/dashboard/shop') }}>
                  <ListItemIcon><StorefrontRoundedIcon fontSize="small" /></ListItemIcon>
                  Kênh Người Bán
                </MenuItem>
              )}
              <Divider />
              <MenuItem onClick={handleLogout} sx={{ color: 'error.main' }}>
                <ListItemIcon><LogoutRoundedIcon fontSize="small" color="error" /></ListItemIcon>
                Đăng xuất
              </MenuItem>
            </Menu>
          </Stack>
        </Box>

        {/* Content Outlet */}
        <Box component="main" id="admin-main" sx={{ minWidth: 0 }}>
          <Outlet />
        </Box>
      </Box>
    </Box>
  )
}

export default AdminLayout
