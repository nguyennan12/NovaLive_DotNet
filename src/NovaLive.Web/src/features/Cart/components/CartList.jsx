import DeleteSweepRoundedIcon from '@mui/icons-material/DeleteSweepRounded'
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined'
import { Box, Button, Typography } from '@mui/material'
import { useCart } from '../hooks/useCart'
import CartShopGroup from './CartShopGroup'

function CartList() {
  const {
    allItems, selectedIds, shopGroups,
    removeSelectedItems
  } = useCart()

  if (allItems.length === 0) {
    return (
      <Box sx={{
        display: 'flex', flexDirection: 'column',
        alignItems: 'center', justifyContent: 'center',
        py: 12, gap: 2
      }}>
        <ShoppingCartOutlinedIcon sx={{ fontSize: 68, color: 'rgba(45,45,45,0.18)' }} />
        <Typography sx={{
          color: 'rgba(45,45,45,0.38)', fontSize: '0.95rem', fontWeight: 500
        }}>
          Giỏ hàng của bạn đang trống
        </Typography>
      </Box>
    )
  }

  return (
    <Box>
      <Box sx={{
        display: 'flex', alignItems: 'center', gap: 1.5,
        mb: 1.5, px: 0.5
      }}>
        <Typography sx={{
          fontSize: '0.82rem', color: 'rgba(45,45,45,0.6)',
          flex: 1, userSelect: 'none'
        }}>
          Tất cả ({allItems?.length} sản phẩm)
        </Typography>

        {selectedIds?.length > 0 && (
          <Button
            size="small"
            color="error"
            startIcon={<DeleteSweepRoundedIcon sx={{ fontSize: 17 }} />}
            onClick={removeSelectedItems}
            sx={{
              textTransform: 'none', fontSize: '0.78rem',
              py: 0.3, px: 1, fontWeight: 500,
              bgcolor: 'rgba(255, 3, 3, 0.6)',
              color: '#fff',
              ':hover': { bgcolor: '#f10000ff' }
            }}
          >
            Xóa ({selectedIds.length}) sản phẩm ra khỏi giỏ hàng
          </Button>
        )}
      </Box>

      {/* Nhóm sản phẩm theo shop */}
      {shopGroups.map(group => (
        <CartShopGroup key={group.shopId} group={group} />
      ))}
    </Box>
  )
}

export default CartList
