import LocalFireDepartmentIcon from '@mui/icons-material/LocalFireDepartment'
import { Box, Button, Typography } from '@mui/material'
import { useDispatch } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { formatVND } from '~/common/utils/formatters'
import { useCartMutations } from '~/features/Cart/hooks/useCartMutations'
import { setSelectedIds } from '~/store/cart/cartSlice'
import { glassSx } from '~/theme'

export const ProductLiveCard = ({ liveShop, product, onClick }) => {
  const firstSku = product.skus?.[0]

  const dispatch = useDispatch()
  const navigate = useNavigate()
  const { addToCartAsync } = useCartMutations()

  const displayPrice = firstSku?.live_price ?? product.live_price ?? 0
  const originalPrice = firstSku?.original_price ?? product.original_price
  const hasSale = originalPrice && originalPrice > displayPrice

  const handleBuyNow = async () => {
    try {
      const payload = {
        skuId: firstSku.skuId,
        productId: product.productId,
        shopId: liveShop._id,
        quantity: 1
      }

      await addToCartAsync(payload)
      dispatch(setSelectedIds([String(firstSku.skuId)]))
      navigate('/order')
    } catch {
      // toast đã được xử lý trong addMutation
    }
  }

  return (
    <Box
      onClick={() => onClick(product)}
      sx={{
        bgcolor: '#fafafa',
        borderRadius: '14px',
        overflow: 'hidden',
        display: 'flex',
        ...glassSx,
        mx: 1.5,
        mb: 1,
        p: 0,
        cursor: 'pointer',
        transition: 'transform 0.2s ease, box-shadow 0.2s ease',
        '&:hover': { transform: 'translateX(3px)', boxShadow: '0 4px 16px rgba(52,133,247,0.2)' }
      }}
    >
      {/* Hình ảnh */}
      <Box sx={{ width: 88, flexShrink: 0, overflow: 'hidden', minHeight: 88, position: 'relative' }}>
        <Box
          component="img"
          src={product.thumb}
          alt={product.name}
          sx={{ width: '100%', height: '100%', objectFit: 'cover' }}
        />
        {product.is_featured && (
          <Box sx={{
            position: 'absolute', top: 0, left: 0, right: 0,
            background: 'linear-gradient(to bottom, rgba(255,107,53,0.88), transparent)',
            display: 'flex', alignItems: 'center', gap: 0.3,
            px: 0.6, py: 0.35
          }}>
            <LocalFireDepartmentIcon sx={{ color: '#fff', fontSize: 10 }} />
            <Typography sx={{ color: '#fff', fontSize: '0.58rem', fontWeight: 700 }}>Ghim</Typography>
          </Box>
        )}
      </Box>

      {/* Nội dung */}
      <Box sx={{
        flex: 1, px: 1.25, py: 1,
        display: 'flex', flexDirection: 'column', justifyContent: 'space-between',
        overflow: 'hidden'
      }}>
        <Box>
          <Typography sx={{
            fontSize: '0.85rem', fontWeight: 700, color: '#2d2d2d',
            display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical',
            overflow: 'hidden', lineHeight: 1.4
          }}>
            {product.name}
          </Typography>

          {firstSku?.sku_name && (
            <Typography sx={{ fontSize: '0.68rem', color: '#999', mt: 0.25 }} noWrap>
              {firstSku.sku_name}
            </Typography>
          )}
        </Box>

        {/* Phần giá và Nút mua */}
        <Box sx={{
          display: 'flex',
          alignItems: 'flex-end',
          justifyContent: 'space-between',
          mt: 0.5
        }}>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.2 }}>
            {/* Hiển thị giá gốc nếu có sale */}
            {hasSale && (
              <Typography sx={{
                fontSize: '0.7rem',
                color: '#bbb',
                textDecoration: 'line-through',
                lineHeight: 1
              }}>
                {formatVND(originalPrice)}
              </Typography>
            )}

            {/* Giá sale (giá hiện tại) */}
            <Typography sx={{
              fontSize: '0.95rem', fontWeight: 800,
              color: '#3485f7', letterSpacing: '-0.02em', lineHeight: 1
            }}>
              {formatVND(displayPrice)}
            </Typography>
          </Box>

          <Button
            variant="contained"
            size="small"
            disableElevation
            onClick={(e) => {
              e.stopPropagation()
              handleBuyNow()
            }}
            sx={{
              minWidth: 'auto',
              px: 1.5,
              py: 0.5,
              borderRadius: '8px',
              fontSize: '0.65rem',
              fontWeight: 700,
              textTransform: 'none',
              background: 'linear-gradient(90deg, #568dfbff, #69aedc, #8acdde)',
              color: '#ffffff',
              boxShadow: '0 2px 8px rgba(52,133,247,0.3)'
            }}
          >
            Mua ngay
          </Button>
        </Box>
      </Box>
    </Box>
  )
}