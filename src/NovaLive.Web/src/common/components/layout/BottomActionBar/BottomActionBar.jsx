import LiveTvIcon from '@mui/icons-material/LiveTv'
import AccountCircleOutlinedIcon from '@mui/icons-material/AccountCircleOutlined'
import ArticleOutlinedIcon from '@mui/icons-material/ArticleOutlined'
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined'
import HomeOutlinedIcon from '@mui/icons-material/HomeOutlined'
import Badge from '@mui/material/Badge'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import { useLocation, useNavigate } from 'react-router-dom'
import { useCart } from '~/features/Cart/hooks/useCart'
import { glassSx } from '~/theme'

const BASE_NAV_ITEMS = [
  { label: 'Đơn hàng', Icon: ArticleOutlinedIcon, path: '/orders' },
  { label: 'Live', Icon: LiveTvIcon, path: '/live' },
  { label: 'Hồ sơ', Icon: AccountCircleOutlinedIcon, path: '/profile' }
]

function BottomActionBar() {
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const { cartCount } = useCart()

  const navItems = [
    { label: 'Home', Icon: HomeOutlinedIcon, path: '/' },
    { label: 'Giỏ hàng', Icon: ShoppingCartOutlinedIcon, path: '/cart', badge: cartCount },
    ...BASE_NAV_ITEMS
  ]

  return (
    <Box
      sx={{
        position: 'fixed',
        bottom: { xs: 14, sm: 24 },
        left: '50%',
        transform: 'translateX(-50%)',
        zIndex: 1200,
        display: 'flex',
        alignItems: 'center',
        gap: { xs: 1, sm: 2 },
        px: { xs: 2, sm: 4 },
        py: { xs: 0.75, sm: 1 },
        background: 'rgba(255, 255, 255, 0.18)',
        ...glassSx,
        borderRadius: '60px',
        boxShadow: '0 8px 32px rgba(52,100,200,0.13), 0 2px 10px rgba(0,0,0,0.07)',
        whiteSpace: 'nowrap'
      }}
    >
      {navItems.map(({ label, Icon, path, badge }) => {
        const isActive = pathname === path

        return (
          <Box
            key={path}
            onClick={() => navigate(path)}
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 0.3,
              cursor: 'pointer',
              px: { xs: 2.5, sm: 3 },
              py: { xs: 0.9, sm: 1.1 },
              borderRadius: '48px',
              transition: 'all 0.22s cubic-bezier(.4,0,.2,1)',
              background: isActive
                ? 'rgba(52,133,247,0.13)'
                : 'transparent',
              '&:hover': {
                background: isActive
                  ? 'rgba(52,133,247,0.18)'
                  : 'rgba(52,133,247,0.07)',
                transform: 'translateY(-2px)'
              },
              '&:active': { transform: 'scale(0.93)' }
            }}
          >
            <Badge
              badgeContent={badge}
              color="error"
              invisible={!badge}
              sx={{ '& .MuiBadge-badge': { fontSize: '0.6rem', minWidth: 16, height: 16, p: '0 3px' } }}
            >
              <Icon sx={{
                fontSize: { xs: 22, sm: 24 },
                color: isActive ? 'secondary.main' : 'rgba(45,45,45,0.48)',
                transition: 'color 0.2s'
              }} />
            </Badge>

            <Typography sx={{
              fontSize: '0.58rem',
              fontWeight: isActive ? 700 : 500,
              color: isActive ? 'secondary.main' : 'rgba(45,45,45,0.5)',
              lineHeight: 1.2,
              transition: 'all 0.2s',
              display: { xs: 'none', sm: 'block' }
            }}>
              {label}
            </Typography>
          </Box>
        )
      })}
    </Box>
  )
}

export default BottomActionBar
