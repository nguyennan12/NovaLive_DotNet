import { Box, Chip, IconButton, ListItemButton, ListItemIcon, ListItemText, Tooltip, Typography } from '@mui/material'
import ChevronLeftRounded from '@mui/icons-material/ChevronLeftRounded'
import ChevronRightRounded from '@mui/icons-material/ChevronRightRounded'
import PlayArrow from '@mui/icons-material/PlayArrow'
import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import CategoryRoundedIcon from '@mui/icons-material/CategoryRounded'
import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import AccountBalanceWalletRoundedIcon from '@mui/icons-material/AccountBalanceWalletRounded'
import DiscountRoundedIcon from '@mui/icons-material/DiscountRounded'
import RateReviewRoundedIcon from '@mui/icons-material/RateReviewRounded'
import HomeRoundedIcon from '@mui/icons-material/HomeRounded'
import { Link, useLocation } from 'react-router-dom'
import { gradientText } from '~/theme'

const ADMIN_NAVIGATION = [
  {
    label: 'Điều hành & Giám sát',
    items: [
      { path: 'dashboard', label: 'Tổng quan sàn', icon: DashboardRoundedIcon, exact: true }
    ]
  },
  {
    label: 'Quản trị đối tác',
    items: [
      { path: 'shops', label: 'Duyệt hồ sơ Shop', icon: StorefrontRoundedIcon },
      { path: 'payouts', label: 'Duyệt giải ngân ví', icon: AccountBalanceWalletRoundedIcon }
    ]
  },
  {
    label: 'Hàng hóa & Khuyến mãi',
    items: [
      { path: 'categories', label: 'Cây danh mục', icon: CategoryRoundedIcon },
      { path: 'flash-sales', label: 'Flash Sale sàn', icon: LocalOfferRoundedIcon },
      { path: 'discounts', label: 'Mã giảm giá sàn', icon: DiscountRoundedIcon }
    ]
  },
  {
    label: 'Kiểm duyệt',
    items: [
      { path: 'reviews', label: 'Kiểm duyệt đánh giá', icon: RateReviewRoundedIcon }
    ]
  }
]

export const AdminSidebar = ({ collapsed, onCollapse, onNavigate }) => {
  const location = useLocation()

  return (
    <Box
      component="nav"
      aria-label="Menu quản trị sàn"
      sx={{
        width: collapsed ? 68 : 250,
        height: '100%',
        bgcolor: '#ffffff',
        border: 1,
        borderColor: 'divider',
        borderRadius: 3,
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
        transition: 'width 0.2s',
        boxShadow: '0 2px 12px rgba(0,0,0,0.03)'
      }}
    >
      {/* Brand Header */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          p: collapsed ? 1 : 2,
          minHeight: 64,
          borderBottom: 1,
          borderColor: 'divider'
        }}
      >
        {!collapsed && (
          <Box
            component={Link}
            to="/admin/dashboard"
            sx={{ display: 'flex', alignItems: 'center', textDecoration: 'none', gap: 0.75 }}
          >
            <PlayArrow color="secondary" />
            <Typography sx={{ ...gradientText, fontWeight: 800, fontSize: '1.1rem' }}>
              NovaLive
            </Typography>
            <Chip
              label="ADMIN"
              size="small"
              sx={{
                height: 20,
                fontSize: '0.65rem',
                fontWeight: 800,
                bgcolor: 'rgba(0, 149, 255, 0.1)',
                color: '#0080db',
                borderRadius: '6px'
              }}
            />
          </Box>
        )}
        <IconButton
          onClick={onCollapse}
          size="small"
          aria-label={collapsed ? 'Mở rộng menu' : 'Thu gọn menu'}
        >
          {collapsed ? <ChevronRightRounded /> : <ChevronLeftRounded />}
        </IconButton>
      </Box>

      {/* Nav List */}
      <Box sx={{ overflowY: 'auto', flex: 1, py: 1.5, px: 1 }}>
        {ADMIN_NAVIGATION.map((section) => (
          <Box key={section.label} sx={{ mb: 1.5 }}>
            {!collapsed && (
              <Typography
                sx={{
                  px: 1,
                  py: 0.75,
                  color: 'text.secondary',
                  fontSize: '0.7rem',
                  textTransform: 'uppercase',
                  letterSpacing: '0.08em',
                  fontWeight: 700
                }}
              >
                {section.label}
              </Typography>
            )}
            {section.items.map((item) => {
              const itemPath = `/admin/${item.path}`
              const active = item.exact
                ? location.pathname === '/admin' || location.pathname === '/admin/' || location.pathname === '/admin/dashboard'
                : location.pathname === itemPath || location.pathname.startsWith(`${itemPath}/`)
              const Icon = item.icon

              return (
                <Tooltip key={item.path} title={collapsed ? item.label : ''} placement="right">
                  <ListItemButton
                    component={Link}
                    to={itemPath}
                    onClick={onNavigate}
                    selected={active}
                    aria-current={active ? 'page' : undefined}
                    sx={{
                      borderRadius: 2,
                      mb: 0.5,
                      px: collapsed ? 1.5 : 1.5,
                      minHeight: 40,
                      color: active ? '#ffffff' : 'text.primary',
                      '&.Mui-selected': {
                        bgcolor: 'secondary.main',
                        color: 'secondary.contrastText',
                        '&:hover': { bgcolor: 'secondary.dark' }
                      },
                      '&:hover': {
                        bgcolor: active ? 'secondary.dark' : 'rgba(0, 149, 255, 0.08)'
                      }
                    }}
                  >
                    <ListItemIcon
                      sx={{
                        minWidth: collapsed ? 24 : 32,
                        color: active ? 'inherit' : 'text.secondary'
                      }}
                    >
                      <Icon fontSize="small" />
                    </ListItemIcon>
                    {!collapsed && (
                      <ListItemText
                        primary={item.label}
                        slotProps={{
                          primary: {
                            sx: {
                              fontSize: '0.84rem',
                              fontWeight: active ? 700 : 500,
                              color: 'inherit'
                            }
                          }
                        }}
                      />
                    )}
                  </ListItemButton>
                </Tooltip>
              )
            })}
          </Box>
        ))}

        {/* Back to Home Link */}
        <Box sx={{ pt: 1, borderTop: 1, borderColor: 'divider', mt: 1 }}>
          <Tooltip title={collapsed ? 'Về trang mua sắm' : ''} placement="right">
            <ListItemButton
              component={Link}
              to="/"
              onClick={onNavigate}
              sx={{
                borderRadius: 2,
                px: collapsed ? 1.5 : 1.5,
                minHeight: 40,
                color: 'text.secondary',
                '&:hover': { bgcolor: 'rgba(0,0,0,0.04)' }
              }}
            >
              <ListItemIcon sx={{ minWidth: collapsed ? 24 : 32, color: 'text.secondary' }}>
                <HomeRoundedIcon fontSize="small" />
              </ListItemIcon>
              {!collapsed && (
                <ListItemText
                  primary="Về trang mua sắm"
                  slotProps={{ primary: { sx: { fontSize: '0.82rem', fontWeight: 500 } } }}
                />
              )}
            </ListItemButton>
          </Tooltip>
        </Box>
      </Box>
    </Box>
  )
}

export default AdminSidebar
