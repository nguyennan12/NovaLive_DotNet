import BoltRoundedIcon from '@mui/icons-material/BoltRounded'
import ShoppingBasketRoundedIcon from '@mui/icons-material/ShoppingBasketRounded'
import { Box, Skeleton, Typography } from '@mui/material'
import React from 'react'
import { useNavigate } from 'react-router-dom'
import { formatSold, formatVND } from '~/common/utils/formatters'

const PortraitCard = React.forwardRef(({ product, showFlashBadge = false }, ref) => {
  const navigate = useNavigate()
  // Only apply flash sale pricing/badge when explicitly in flash sale context
  const isFlashSale = showFlashBadge && product.is_flash_sale
  return (
    <Box
      ref={ref}
      onClick={() => navigate(`/product/${product.spu_code}`)}
      sx={{
        bgcolor: 'primary.main',
        borderRadius: '16px',
        overflow: 'hidden',
        boxShadow: '0 1px 10px rgba(79,79,79,0.18)',
        transition: 'transform 0.2s ease, box-shadow 0.2s ease',
        cursor: 'pointer',
        '&:hover': { transform: 'translateY(-4px)', boxShadow: '0 8px 24px rgba(52,133,247,0.15)' }
      }}
    >
      <Box sx={{ position: 'relative', height: { xs: 155, sm: 195, md: 215 }, overflow: 'hidden' }}>
        <Box
          component="img"
          src={product.spu_thumb}
          alt={product.spu_name}
          sx={{ width: '100%', height: '100%', objectFit: 'cover' }}
        />
        {isFlashSale && (
          <Box sx={{
            position: 'absolute', top: 10, right: -1,
            background: 'linear-gradient(90deg, #ff6a00 20%, #ee0979 100%)',
            px: 1, py: 0.4,
            display: 'flex', alignItems: 'center', gap: 0.3,
            clipPath: 'polygon(8px 0%, 100% 0%, 100% 100%, 8px 100%, 0% 50%)',
            boxShadow: '0 2px 8px rgba(238,9,121,0.35)',
            minWidth: 62
          }}>
            <BoltRoundedIcon sx={{ fontSize: 11, color: '#fff' }} />
            <Typography sx={{ fontSize: '0.62rem', fontWeight: 800, color: '#fff', lineHeight: 1, letterSpacing: '0.04em' }}>
              SALE
            </Typography>
          </Box>
        )}
      </Box>

      <Box sx={{ p: { xs: 1.25, sm: 1.5 } }}>
        <Typography noWrap sx={{ fontSize: { xs: '0.82rem', sm: '0.9rem' }, fontWeight: 700, color: 'primary.contrastText', mb: 0.85 }}>
          {product.spu_name}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 0.75 }}>
          <Typography sx={{
            fontSize: { xs: '1rem', sm: '1.1rem' }, fontWeight: 800,
            color: isFlashSale ? '#e8472a' : 'secondary.main',
            letterSpacing: '-0.02em', lineHeight: 1
          }}>
            {formatVND(isFlashSale ? product.flash_price : product.spu_price)}
          </Typography>
          {isFlashSale && (
            <Typography sx={{ fontSize: '0.7rem', color: '#aaa', textDecoration: 'line-through', lineHeight: 1 }}>
              {formatVND(product.spu_price)}
            </Typography>
          )}
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.4, mt: 0.4 }}>
          <ShoppingBasketRoundedIcon sx={{ fontSize: 11, color: '#aaa' }} />
          <Typography sx={{ fontSize: '0.65rem', color: '#aaa', fontWeight: 500 }}>
            {formatSold(product.total_sold)} lượt mua
          </Typography>
        </Box>
      </Box>
    </Box >
  )
})

const LandscapeCard = ({ product }) => {
  const navigate = useNavigate()
  return (
    <Box
      onClick={() => navigate(`/product/${product.spu_code}`)}
      sx={{
        bgcolor: 'primary.main',
        borderRadius: '14px',
        overflow: 'hidden',
        boxShadow: '0 1px 8px rgba(79,79,79,0.16)',
        display: 'flex',
        transition: 'transform 0.2s ease, box-shadow 0.2s ease',
        cursor: 'pointer',
        '&:hover': { transform: 'translateX(3px)', boxShadow: '0 4px 16px rgba(52,133,247,0.15)' }
      }}
    >
      <Box sx={{ width: { xs: 90, sm: 120, md: 135 }, flexShrink: 0, overflow: 'hidden', minHeight: { xs: 90, sm: 115 } }}>
        <Box
          component="img"
          src={product.spu_thumb}
          alt={product.spu_name}
          sx={{ width: '100%', height: '100%', objectFit: 'cover' }}
        />
      </Box>
      <Box sx={{ flex: 1, p: { xs: 1.25, sm: 1.75 }, display: 'flex', flexDirection: 'column', justifyContent: 'space-between', overflow: 'hidden' }}>
        <Typography sx={{
          fontSize: { xs: '0.85rem', sm: '0.92rem' }, fontWeight: 700, color: 'primary.contrastText',
          display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden', lineHeight: 1.4
        }}>
          {product.spu_name}
        </Typography>
        <Box sx={{ mt: 0.75 }}>
          <Typography sx={{ fontSize: { xs: '1rem', sm: '1.1rem' }, fontWeight: 800, color: 'secondary.main', letterSpacing: '-0.02em', lineHeight: 1 }}>
            {formatVND(product.spu_price)}
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.4, mt: 0.35 }}>
            <ShoppingBasketRoundedIcon sx={{ fontSize: 11, color: '#aaa' }} />
            <Typography sx={{ fontSize: '0.65rem', color: '#aaa', fontWeight: 500 }}>
              {formatSold(product.total_sold)} lượt mua
            </Typography>
          </Box>
        </Box>
      </Box>
    </Box>
  )
}

export const HomeProductCardSkeleton = ({ variant = 'portrait' }) => {
  if (variant === 'landscape') {
    return (
      <Box sx={{ display: 'flex', borderRadius: '14px', overflow: 'hidden', height: 115, bgcolor: 'primary.main' }}>
        <Skeleton variant="rectangular" width={120} height="100%" sx={{ flexShrink: 0 }} />
        <Box sx={{ flex: 1, p: 1.5 }}>
          <Skeleton variant="text" width="80%" height={18} />
          <Skeleton variant="text" width="60%" height={16} sx={{ mt: 0.5 }} />
          <Skeleton variant="text" width="45%" height={24} sx={{ mt: 1.5 }} />
        </Box>
      </Box>
    )
  }
  return (
    <Box sx={{ borderRadius: '16px', overflow: 'hidden', bgcolor: 'primary.main' }}>
      <Skeleton variant="rectangular" height={200} />
      <Box sx={{ p: 1.5 }}>
        <Skeleton variant="text" width="85%" height={18} />
        <Skeleton variant="text" width="50%" height={24} sx={{ mt: 0.75 }} />
        <Skeleton variant="text" width="40%" height={14} sx={{ mt: 0.4 }} />
      </Box>
    </Box>
  )
}

export const HomeProductCard = React.forwardRef(({ product, variant = 'portrait', showFlashBadge = false }, ref) => {
  if (variant === 'landscape') return <LandscapeCard product={product} />
  return <PortraitCard ref={ref} product={product} showFlashBadge={showFlashBadge} />
})
