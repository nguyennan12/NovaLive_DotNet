import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import Typography from '@mui/material/Typography'
import { useState } from 'react'
import { useSelector } from 'react-redux'
import { useParams } from 'react-router-dom'
import { selectCurrentUser } from '~/store/user/userSlice'
import LiveHost from '../components/LiveHost'
import LiveProductMobileDrawer from '../components/LiveHost/LiveProductMobileDrawer'
import LiveProductPanel from '../components/LiveHost/LiveProductPanel'
import { useLiveProducts } from '../hooks/useLiveProducts'

function ShopLive() {
  const { liveId } = useParams()
  const currentUser = useSelector(selectCurrentUser)
  const [mobileDrawerOpen, setMobileDrawerOpen] = useState(false)

  const { liveProducts, addProduct, removeProduct, isAdding } = useLiveProducts(liveId)

  if (!currentUser) return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100vh', bgcolor: '#0a0a0a' }}>
      <CircularProgress sx={{ color: '#fff' }} />
    </Box>
  )

  if (!liveId) return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100vh', bgcolor: '#0a0a0a' }}>
      <Typography color="white">Không tìm thấy live session</Typography>
    </Box>
  )

  return (
    <Box sx={{ display: 'flex', height: '100vh', bgcolor: '#0a0a0a', overflow: 'hidden' }}>
      {/* Cột video — căn giữa, chiếm phần còn lại */}
      <Box sx={{
        flex: 1,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        overflow: 'hidden'
      }}>
        <LiveHost
          liveId={liveId}
          liveProducts={liveProducts}
          onOpenProducts={() => setMobileDrawerOpen(true)}
        />
      </Box>

      {/* Sidebar sản phẩm — chỉ hiện trên desktop (md+) */}
      <Box sx={{ display: { xs: 'none', md: 'flex' }, width: 340, flexShrink: 0 }}>
        <LiveProductPanel
          liveProducts={liveProducts}
          onAddProduct={addProduct}
          onRemoveProduct={removeProduct}
          isAdding={isAdding}
        />
      </Box>

      {/* Drawer sản phẩm trên mobile */}
      <LiveProductMobileDrawer
        open={mobileDrawerOpen}
        onClose={() => setMobileDrawerOpen(false)}
        liveProducts={liveProducts}
        onAddProduct={addProduct}
        onRemoveProduct={removeProduct}
        isAdding={isAdding}
      />
    </Box>
  )
}

export default ShopLive
