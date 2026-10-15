import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded'
import { Avatar, Box, Checkbox, Chip, IconButton, Typography } from '@mui/material'
import { formatVND } from '~/common/utils/formatters'
import { useCart } from '../hooks/useCart'
import { QuantityStepper } from './shared/QuantityStepper'

function CartItem({ item }) {
  const { selectedIds, toggleSelect, updateQuantity, removeItem, isPending } = useCart()
  const skuIdStr = String(item.skuId)
  const isSelected = selectedIds.includes(skuIdStr) //biến kiểm tra product đã dc chọn chưa

  const variationLabel = item.skuName ? Object.values(item.skuName).filter(Boolean) : null

  const handleQuantityChange = (newQty) => {
    if (newQty < 1) return
    updateQuantity(item.skuId, newQty, item.quantity)
  }

  return (
    <Box sx={{ py: 1.75 }}>
      <Box sx={{ display: 'flex', gap: 1.5, alignItems: 'flex-start' }}>
        {/* Checkbox */}
        <Checkbox
          checked={isSelected}
          onChange={() => toggleSelect(skuIdStr)}
          size="small"
          sx={{
            mt: 0.75, p: 0, flexShrink: 0,
            color: 'rgba(0,0,0,0.28)',
            '&.Mui-checked': { color: 'secondary.main' }
          }}
        />

        {/* Ảnh sản phẩm */}
        <Avatar
          src={item.image}
          variant="rounded"
          alt={item.name}
          sx={{ width: 76, height: 76, borderRadius: 2, flexShrink: 0, bgcolor: '#f0f4ff' }}
        />

        <Box sx={{ flex: 1, minWidth: 0 }}>
          {/* Tên */}
          <Typography sx={{
            fontSize: '0.875rem', fontWeight: 600, color: 'primary.contrastText',
            lineHeight: 1.35, mb: 0.5,
            display: '-webkit-box', WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical', overflow: 'hidden'
          }}>
            {item.name}
          </Typography>

          {/* SKU variation chip */}
          {variationLabel && (
            <Chip
              label={variationLabel}
              size="small"
              sx={{
                fontSize: '0.68rem', height: 20, mb: 0.75,
                bgcolor: 'rgba(83,155,255,0.1)', color: 'secondary.main', fontWeight: 600
              }}
            />
          )}

          {/* Giá + stepper + xóa */}
          <Box sx={{
            display: 'flex', alignItems: 'center',
            justifyContent: 'space-between', flexWrap: 'wrap', gap: 1, mt: 0.5
          }}>
            <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 0.75 }}>
              <Typography sx={{ fontSize: '0.95rem', fontWeight: 700, color: item.isFlashSale ? '#e8472a' : 'secondary.main' }}>
                {formatVND(item.isFlashSale ? item.flashSalePrice : item.price)}
              </Typography>
              {item.isFlashSale && (
                <Typography sx={{ fontSize: '0.72rem', color: '#aaa', textDecoration: 'line-through' }}>
                  {formatVND(item.price)}
                </Typography>
              )}
            </Box>

            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
              <QuantityStepper
                value={item.quantity}
                onChange={handleQuantityChange}
                disabled={isPending}
              />
              <IconButton
                size="small"
                onClick={() => removeItem(item.skuId)}
                sx={{ color: 'error.main', borderRadius: '8px', '&:hover': { bgcolor: 'rgba(239,68,68,0.08)' } }}
              >
                <DeleteOutlineRoundedIcon sx={{ fontSize: 18 }} />
              </IconButton>
            </Box>
          </Box>
        </Box>
      </Box>
    </Box>
  )
}

export default CartItem
