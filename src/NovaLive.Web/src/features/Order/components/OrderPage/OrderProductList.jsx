import { Box, Typography } from '@mui/material'
import CartShopGroup from '~/features/Cart/components/CartShopGroup'
import { gradientText } from '~/theme'


function OrderProductList({ shopGroups }) {
  if (!shopGroups || shopGroups.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 5, color: 'rgba(45,45,45,0.4)' }}>
        <Typography sx={{ fontSize: '0.9rem' }}>Không có sản phẩm nào được chọn</Typography>
      </Box>
    )
  }

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
        <Box sx={{ width: 3.5, height: 22, borderRadius: '2px', bgcolor: 'secondary.main', flexShrink: 0 }} />
        <Typography sx={{ fontSize: '0.93rem', fontWeight: 700, ...gradientText }}>
          Đơn hàng của bạn
        </Typography>
      </Box>

      {shopGroups.map(group => (
        <CartShopGroup key={group.shopId} group={group} />
      ))}
    </Box>
  )
}

export default OrderProductList
