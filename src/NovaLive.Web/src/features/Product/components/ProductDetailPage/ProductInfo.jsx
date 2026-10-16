import AssignmentReturnRoundedIcon from '@mui/icons-material/AssignmentReturnRounded'
import BoltRoundedIcon from '@mui/icons-material/BoltRounded'
import FactCheckRoundedIcon from '@mui/icons-material/FactCheckRounded'
import NearMeIcon from '@mui/icons-material/NearMe'
import ShieldRoundedIcon from '@mui/icons-material/ShieldRounded'
import ShoppingBasketRoundedIcon from '@mui/icons-material/ShoppingBasketRounded'
import { Box, Chip, Divider, Rating, Skeleton, Tooltip, Typography } from '@mui/material'
import { useSelector } from 'react-redux'
import { selectCategories } from '~/store/product/categorySlice'
import { slugCateToNameCate } from '~/common/utils/converter'
import { formatSold, formatVND } from '~/common/utils/formatters'
import { gradientText } from '~/theme'


const ProductInfo = ({ product, selectedSku, flashSaleItem }) => {
  const categories = useSelector(selectCategories)

  if (!product) {
    return (
      <Box>
        <Skeleton variant="text" width="75%" height={40} />
        <Skeleton variant="text" width="45%" height={24} sx={{ mt: 0.75 }} />
        <Skeleton variant="rounded" height={80} sx={{ mt: 2, borderRadius: 2 }} />
        <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
          <Skeleton variant="rounded" width={80} height={28} sx={{ borderRadius: '8px' }} />
          <Skeleton variant="rounded" width={80} height={28} sx={{ borderRadius: '8px' }} />
        </Box>
      </Box>
    )
  }

  const displayPrice = selectedSku?.sku_price ?? product.spu_price
  const categoryChips = slugCateToNameCate(product?.spu_category, categories)

  return (
    <Box>
      {/* Product name */}
      <Box sx={{
        display: 'flex',
        alignItems: 'flex-start',
        justifyContent: 'space-between',
        gap: 2,
        width: '100%'
      }}>
        <Typography
          variant="h5"
          sx={{
            fontWeight: 800,
            color: 'primary.contrastText',
            lineHeight: 1.2,
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            overflow: 'hidden'
          }}
        >
          {product.spu_name}
        </Typography>

        <Tooltip title='share'>
          <Box sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            minWidth: 40,
            height: 40,
            bgcolor: 'rgba(83,155,255,0.10)',
            border: '1px solid rgba(83,155,255,0.25)',
            borderRadius: '50%',
            color: 'secondary.main'
          }}>
            <NearMeIcon sx={{ fontSize: '1.2rem' }} />
          </Box>
        </Tooltip>

      </Box>

      {/* Rating + sold row */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mt: 1, flexWrap: 'wrap' }}>
        <Rating
          value={product.spu_ratingsAvg ?? 0}
          precision={0.5}
          readOnly
          size="small"
          sx={{ color: 'fourth.main' }}
        />
        <Typography sx={{ fontSize: '0.82rem', color: 'fourth.main', fontWeight: 600 }}>
          {(product.spu_ratingsAvg ?? 0).toFixed(1)}
        </Typography>

        <Divider orientation="vertical" flexItem sx={{ borderColor: '#e0e0e0' }} />

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.4 }}>
          <ShoppingBasketRoundedIcon sx={{ fontSize: 13, color: '#aaa' }} />
          <Typography sx={{ fontSize: '0.78rem', color: '#aaa', fontWeight: 500 }}>
            {formatSold(product.total_sold ?? 0)} lượt mua
          </Typography>
        </Box>
      </Box>

      {/* Price block */}
      <Box sx={{
        mt: 2.5,
        background: 'rgba(255,255,255,0.1)',
        backdropFilter: 'blur(10px)',
        WebkitBackdropFilter: 'blur(10px)',
        borderRadius: 2,
        boxShadow: '0 2px 12px rgba(0,0,0,0.04)',
        p: 3
      }}>
        {flashSaleItem ? (
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mb: 0.75 }}>
              <BoltRoundedIcon sx={{ fontSize: 14, color: '#e8472a' }} />
              <Typography sx={{ fontSize: '0.72rem', fontWeight: 800, color: '#e8472a', letterSpacing: '0.06em', textTransform: 'uppercase' }}>
                Flash Sale
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 1.5 }}>
              <Typography variant="h5" sx={{ fontSize: { xs: '1.6rem', md: '2rem' }, fontWeight: 800, color: '#e8472a', letterSpacing: '-0.02em', lineHeight: 1.1 }}>
                {formatVND(flashSaleItem.flash_price)}
              </Typography>
              <Typography sx={{ fontSize: '0.95rem', color: '#aaa', textDecoration: 'line-through', lineHeight: 1 }}>
                {formatVND(displayPrice ?? 0)}
              </Typography>
            </Box>
          </Box>
        ) : (
          <Typography variant="h5" sx={{
            fontSize: { xs: '1.6rem', md: '2rem' },
            fontWeight: 800,
            ...gradientText,
            letterSpacing: '-0.02em',
            lineHeight: 1.1
          }}>
            {formatVND(displayPrice ?? 0)}
          </Typography>
        )}
      </Box>

      {/* Category chips */}
      {categoryChips.length > 0 && (
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.75, mt: 2 }}>
          {categoryChips.map(({ id, label }) => (
            <Chip
              key={id}
              label={label}
              size="small"
              variant="outlined"
              sx={{
                borderColor: 'secondary.main',
                color: 'secondary.main',
                fontSize: '0.72rem',
                borderRadius: '8px',
                fontWeight: 600
              }}
            />
          ))}
        </Box>
      )}

      {/* Policy */}
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mt: 2.5 }}>
        <Typography sx={{ fontSize: '0.68rem', fontWeight: 700, color: '#9ca3af', textTransform: 'uppercase', letterSpacing: '0.07em' }}>
          Chính sách mua hàng
        </Typography>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
          {[
            { Icon: AssignmentReturnRoundedIcon, label: 'Trả hàng miễn phí 15 ngày' },
            { Icon: FactCheckRoundedIcon, label: 'Đồng kiểm khi nhận hàng' },
            { Icon: ShieldRoundedIcon, label: 'Đảm bảo chính hãng' }
          ].map(({ Icon, label }) => (
            <Box key={label} sx={{
              display: 'flex', alignItems: 'center', gap: 0.75,
              bgcolor: 'rgba(83,155,255,0.10)',
              border: '1px solid rgba(83,155,255,0.25)',
              borderRadius: '20px', px: 1.5, py: 0.6
            }}>
              <Icon sx={{ fontSize: 14, color: 'secondary.main' }} />
              <Typography sx={{ fontSize: '0.75rem', fontWeight: 600, color: 'secondary.main', whiteSpace: 'nowrap' }}>
                {label}
              </Typography>
            </Box>
          ))}
        </Box>
        <Typography sx={{ fontSize: '0.72rem', color: '#9ca3af', lineHeight: 1.6, mt: 0.25 }}>
          Miễn phí trả hàng trong <strong style={{ color: '#539bff' }}>15 ngày</strong> — bạn có thể yên tâm mua sắm. Tại thời điểm nhận hàng, bạn có quyền đồng kiểm và trả hàng miễn phí nếu không đúng mô tả.
        </Typography>
      </Box>
    </Box>
  )
}

export default ProductInfo
