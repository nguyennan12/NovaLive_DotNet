import type { ReactNode } from 'react'
import { useState, useSyncExternalStore } from 'react'
import { Alert, Box, Breadcrumbs, Button, Drawer, IconButton, Link as MuiLink, Stack, Tooltip, Typography, useMediaQuery } from '@mui/material'
import MenuRounded from '@mui/icons-material/MenuRounded'
import VolumeUpOutlined from '@mui/icons-material/VolumeUpOutlined'
import VolumeOffOutlined from '@mui/icons-material/VolumeOffOutlined'
import { Link, useLocation } from 'react-router-dom'
import { SellerSidebar } from './SellerSidebar'
import { NotificationBell } from './NotificationBell'
import { useSellerShop } from '../../hooks/useShop'
import { SellerPermissionsContext } from '../../hooks/useShopPermissions'
import { useOrderRealtime } from '../../hooks/useOrderRealtime'
import { getMockSnapshot, subscribeMock } from '../../services/transport'
import { sellerNavigation } from '../../config/navigation'
import { StatusBadge } from '../shared/StatusBadge'
import { errorMessage } from '../../utils/errors'

export function SellerLayout({ children }: { children: ReactNode }) {
  const [collapsed, setCollapsed] = useState(false)
  const [mobileOpen, setMobileOpen] = useState(false)
  const mobile = useMediaQuery('(max-width:899px)')
  const shop = useSellerShop()
  const realtime = useOrderRealtime()
  const mockModules = useSyncExternalStore(subscribeMock, getMockSnapshot)
  const location = useLocation()
  const current = sellerNavigation.flatMap(section => section.items).find(item => item.path && location.pathname.startsWith(`/seller/${item.path}`))
  const canOperate = shop.data?.status === 'Active'
  return <SellerPermissionsContext.Provider value={{ canOperate }}>
    <Box sx={{ background: 'linear-gradient(135deg, #f8faff 0%, #f0f4ff 50%, #f5f8ff 100%)', minHeight: '100dvh', display: 'flex' }}>
      {!mobile && <Box sx={{ position: 'sticky', top: 0, height: '100dvh', p: 2, flexShrink: 0 }}><SellerSidebar collapsed={collapsed} onCollapse={() => setCollapsed(!collapsed)} /></Box>}
      <Drawer open={mobileOpen} onClose={() => setMobileOpen(false)} slotProps={{ paper: { sx: { bgcolor: 'primary.main' } } }}><SellerSidebar collapsed={false} onCollapse={() => setMobileOpen(false)} onNavigate={() => setMobileOpen(false)} /></Drawer>
      <Box sx={{ flex: 1, minWidth: 0, p: { xs: 1.5, sm: 3 } }}>
        <Box component='header' sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 2, mb: 2, flexWrap: 'wrap' }}>
          <Stack direction='row' sx={{ alignItems: 'center' }} spacing={1}>{mobile && <IconButton aria-label='Mở menu cửa hàng' onClick={() => setMobileOpen(true)}><MenuRounded /></IconButton>}<Box><Typography sx={{ fontWeight: 700 }}>{shop.data?.shopName || 'Trung tâm người bán'}</Typography><Breadcrumbs aria-label='Đường dẫn trang' sx={{ fontSize: '0.75rem' }}><MuiLink component={Link} to='/seller' color='inherit'>Cửa hàng</MuiLink><Typography variant='caption'>{current?.label || 'Tổng quan'}</Typography></Breadcrumbs></Box></Stack>
          <Stack direction='row' sx={{ alignItems: 'center' }} spacing={1}>{shop.data && <StatusBadge status={shop.data.status} />}<Tooltip title={`Thông báo đơn: ${realtime.enabled ? realtime.state : 'chờ backend hỗ trợ nhóm theo JWT'}. ${realtime.sound ? 'Tắt' : 'Bật'} âm thanh`}><IconButton aria-label={realtime.sound ? 'Tắt âm thanh đơn hàng' : 'Bật âm thanh đơn hàng'} onClick={realtime.toggleSound}>{realtime.sound ? <VolumeUpOutlined /> : <VolumeOffOutlined />}</IconButton></Tooltip><NotificationBell /></Stack>
        </Box>
        <Stack spacing={1.5} sx={{ mb: 2 }}>
          {mockModules && <Alert severity='info'><Box component='details'><Box component='summary' sx={{ cursor: 'pointer' }}>Dữ liệu mẫu đang bật ({mockModules.split(',').length} module). Thao tác chỉ lưu trong phiên thử.</Box><Typography sx={{ mt: 1 }}>{mockModules}</Typography></Box></Alert>}
          {shop.error && <Alert severity='error' action={<Button color='inherit' onClick={() => void shop.refetch()}>Thử lại</Button>}>{errorMessage(shop.error)}</Alert>}
          {shop.data && !canOperate && <Alert severity='warning'>Cửa hàng đang ở trạng thái <strong>{shop.data.status}</strong>. Chức năng tạo sản phẩm và tiếp nhận đơn mới đang tạm khóa. <MuiLink component={Link} to='/seller/profile'>Xem hồ sơ</MuiLink></Alert>}
        </Stack>
        <Box component='main' id='seller-main' sx={{ minWidth: 0 }}>{children}</Box>
      </Box>
    </Box>
  </SellerPermissionsContext.Provider>
}
