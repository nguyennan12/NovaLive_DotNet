import { Box, IconButton, ListItemButton, ListItemIcon, ListItemText, Tooltip, Typography } from '@mui/material'
import ChevronLeftRounded from '@mui/icons-material/ChevronLeftRounded'
import ChevronRightRounded from '@mui/icons-material/ChevronRightRounded'
import PlayArrow from '@mui/icons-material/PlayArrow'
import { Link, useLocation } from 'react-router-dom'
import { sellerNavigation } from '../../config/navigation'
import { gradientText } from '~/theme'

export function SellerSidebar({ collapsed, onCollapse, onNavigate }: { collapsed: boolean; onCollapse: () => void; onNavigate?: () => void }) {
  const location = useLocation()
  return <Box component='nav' aria-label='Menu quản lý cửa hàng' sx={{ width: collapsed ? 60 : 240, height: '100%', bgcolor: 'primary.main', border: 1, borderColor: 'divider', borderRadius: 3, display: 'flex', flexDirection: 'column', overflow: 'hidden', transition: 'width 0.2s' }}>
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', p: collapsed ? 1 : 2, minHeight: 64, borderBottom: 1, borderColor: 'divider' }}>
      {!collapsed && <Box component={Link} to='/' sx={{ display: 'flex', alignItems: 'center', textDecoration: 'none', gap: 0.5 }}><PlayArrow color='secondary' /><Typography sx={{ ...gradientText, fontWeight: 800 }}>NovaLive</Typography></Box>}
      <IconButton onClick={onCollapse} size='small' aria-label={collapsed ? 'Mở rộng menu' : 'Thu gọn menu'}>{collapsed ? <ChevronRightRounded /> : <ChevronLeftRounded />}</IconButton>
    </Box>
    <Box sx={{ overflowY: 'auto', flex: 1, py: 1, px: 1 }}>
      {sellerNavigation.map(section => <Box key={section.label} sx={{ mb: 1.5 }}>
        {!collapsed && <Typography sx={{ px: 1, py: 1, color: 'text.secondary', fontSize: '0.7rem', textTransform: 'uppercase', letterSpacing: '0.08em' }}>{section.label}</Typography>}
        {section.items.map(item => {
          const path = `/seller${item.path ? `/${item.path}` : ''}`
          const active = item.path ? location.pathname === path || location.pathname.startsWith(`${path}/`) : location.pathname === '/seller' || location.pathname === '/seller/'
          const Icon = item.icon
          return <Tooltip key={item.path} title={collapsed ? item.label : ''} placement='right'><ListItemButton component={Link} to={path} onClick={onNavigate} selected={active} aria-current={active ? 'page' : undefined} sx={{ borderRadius: 2, mb: 0.5, px: collapsed ? 1.25 : 1.5, minHeight: 40, '&.Mui-selected': { bgcolor: 'secondary.main', color: 'secondary.contrastText', '&:hover': { bgcolor: 'secondary.dark' } } }}><ListItemIcon sx={{ minWidth: collapsed ? 24 : 32, color: 'inherit' }}><Icon fontSize='small' /></ListItemIcon>{!collapsed && <ListItemText primary={item.label} slotProps={{ primary: { sx: { fontSize: '0.82rem', fontWeight: active ? 600 : 400 } } }} />}</ListItemButton></Tooltip>
        })}
      </Box>)}
    </Box>
  </Box>
}
