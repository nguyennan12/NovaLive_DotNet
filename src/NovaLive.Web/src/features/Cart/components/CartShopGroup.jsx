import LocalOfferOutlinedIcon from '@mui/icons-material/LocalOfferOutlined'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import { Box, Button, Checkbox, Chip, Divider, Typography } from '@mui/material'
import { useMemo, useState } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { selectSelectedIds, setSelectedIds } from '~/store/cart/cartSlice'
import { selectAppliedShopDiscounts } from '~/store/discount/discountSlice'
import ShopDiscountPopup from '~/features/Discount/components/DIscountSelect/ShopDiscountPopup'
import { glassSx } from '~/theme'
import CartItem from './CartItem'


function CartShopGroup({ group }) {
  const { shopId, shopName, items } = group
  const dispatch = useDispatch()
  const selectedIds = useSelector(selectSelectedIds)
  const shopDiscounts = useSelector(selectAppliedShopDiscounts)
  const appliedDiscount = shopDiscounts?.[String(shopId)]

  const [discountOpen, setDiscountOpen] = useState(false)

  const groupSkuIds = items.map(i => String(i.skuId))
  const allGroupSelected = groupSkuIds.length > 0 && groupSkuIds.every(id => selectedIds.includes(id))
  const someGroupSelected = groupSkuIds.some(id => selectedIds.includes(id))

  const shopSubtotal = useMemo(() => {
    return items
      .filter(i => selectedIds.includes(String(i.skuId)))
      .reduce((s, i) => s + (i.isFlashSale ? i.flashSalePrice : i.price) * i.quantity, 0)
  }, [items, selectedIds])


  const handleToggleGroup = () => {
    if (allGroupSelected) {
      dispatch(setSelectedIds(selectedIds.filter(id => !groupSkuIds.includes(id))))
    } else {
      const next = [...new Set([...selectedIds, ...groupSkuIds])]
      dispatch(setSelectedIds(next))
    }
  }

  return (
    <Box sx={{
      bgcolor: 'primary.main',
      ...glassSx, p: 0,
      borderRadius: 3, mb: 2,
      overflow: 'hidden',
      border: '1px solid', borderColor: 'divider'
    }}>
      {/* Shop header */}
      <Box sx={{
        display: 'flex', alignItems: 'center', gap: 1,
        px: 2, py: 1.25,
        borderBottom: '1px solid', borderColor: 'divider',
        bgcolor: 'rgba(83,155,255,0.04)'
      }}>
        <Checkbox
          checked={allGroupSelected}
          indeterminate={someGroupSelected && !allGroupSelected}
          onChange={handleToggleGroup}
          size="small"
          sx={{ p: 0, flexShrink: 0, color: 'rgba(0,0,0,0.28)', '&.Mui-checked': { color: 'secondary.main' } }}
        />
        <StorefrontRoundedIcon sx={{ fontSize: 17, color: 'secondary.main' }} />
        <Typography sx={{ fontSize: '0.85rem', fontWeight: 700, color: 'primary.contrastText', flex: 1 }}>
          {shopName}
        </Typography>
      </Box>

      {/* Danh sách sản phẩm */}
      <Box sx={{ px: 2 }}>
        {items.map((item, idx) => (
          <Box key={String(item.skuId)}>
            <CartItem item={item} />
            {idx < items.length - 1 && (
              <Divider sx={{ borderColor: 'divider', opacity: 0.7 }} />
            )}
          </Box>
        ))}
      </Box>

      {/* Discount banner */}
      <Box sx={{
        px: 2, py: 1,
        borderTop: '1px solid', borderColor: 'divider',
        display: 'flex', alignItems: 'center', gap: 1,
        bgcolor: appliedDiscount ? 'rgba(52,133,247,0.04)' : 'rgba(245,158,11,0.04)'
      }}>
        <LocalOfferOutlinedIcon sx={{ fontSize: 15, color: appliedDiscount ? 'secondary.main' : 'fourth.main', flexShrink: 0 }} />

        {appliedDiscount ? (
          <Chip
            label={`${appliedDiscount.code || appliedDiscount.name}`}
            size="small"
            sx={{
              height: 20, fontSize: '0.68rem', fontWeight: 700,
              bgcolor: 'rgba(52,133,247,0.1)', color: 'secondary.main',
              border: '1px solid rgba(52,133,247,0.25)',
              '& .MuiChip-label': { px: 0.9 }
            }}
          />
        ) : (
          <Typography sx={{ fontSize: '0.76rem', color: 'rgba(45,45,45,0.55)', flex: 1 }}>
            Khuyến mãi từ shop
          </Typography>
        )}

        <Box sx={{ ml: 'auto' }}>
          <Button
            size="small"
            onClick={() => setDiscountOpen(true)}
            sx={{
              fontSize: '0.74rem', fontWeight: 600,
              color: appliedDiscount ? 'secondary.main' : 'fourth.main',
              textTransform: 'none', py: 0.25, px: 1.25, minWidth: 'unset',
              '&:hover': {
                bgcolor: appliedDiscount ? 'rgba(52,133,247,0.08)' : 'rgba(245,158,11,0.08)'
              }
            }}
          >
            {appliedDiscount ? 'Thay đổi' : 'Chọn mã'}
          </Button>
        </Box>
      </Box>

      <ShopDiscountPopup
        open={discountOpen}
        onClose={() => setDiscountOpen(false)}
        shopId={shopId}
        shopName={shopName}
        subtotal={shopSubtotal}
      />
    </Box>
  )
}

export default CartShopGroup
