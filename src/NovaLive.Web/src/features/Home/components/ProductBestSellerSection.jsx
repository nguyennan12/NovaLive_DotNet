import { Box, Typography } from '@mui/material'
import ArrowForwardIosRoundedIcon from '@mui/icons-material/ArrowForwardIosRounded'
import { HomeProductCard, HomeProductCardSkeleton } from './HomeProductCard'
import VerifiedIcon from '@mui/icons-material/Verified'

const rankColors = ['#f59e0b', '#9ca3af', '#cd7c2f']

const ProductBestSellerSection = ({ products = [], isLoading }) => {
  return (
    <Box sx={{
      background: 'rgba(255, 255, 255, 0.1)',
      backdropFilter: 'blur(10px)',
      WebkitBackdropFilter: 'blur(10px)',
      borderRadius: 3,
      boxShadow: '0 4px 30px rgba(0, 0, 0, 0.05)',
      p: 2,
      mb: { xs: 3.5, md: 4 }
    }}>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2.5 }}>
        <Box sx={{
          display: 'flex', alignItems: 'center',
          gap: { xs: 1.5, sm: 2 }, p: 1, borderRadius: '8px'
        }}>
          <Box sx={{
            width: { xs: 4, sm: 6 },
            height: { xs: 28, sm: 36 },
            borderRadius: '4px',
            bgcolor: '#ffd51aff',
            flexShrink: 0
          }} />

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <VerifiedIcon sx={{
              fontSize: { xs: '1.5rem', sm: '2.5rem' },
              fontWeight: 900, color: '#ffd51aff',
              letterSpacing: '-0.02em', lineHeight: 1
            }} />
            <Typography sx={{
              fontSize: { xs: '1.5rem', sm: '2.5rem' },
              fontWeight: 900,
              background: 'linear-gradient(to right, #FFD700 0%, #FFA500 100%)',
              WebkitBackgroundClip: 'text',
              WebkitTextFillColor: 'transparent',
              backgroundClip: 'text',
              color: 'transparent',
              letterSpacing: '-0.02em', lineHeight: 1
            }}>BEST SALLER</Typography>

          </Box>
        </Box>

        <Box sx={{
          display: 'flex', alignItems: 'center', gap: 0.5,
          cursor: 'pointer', opacity: 0.75, '&:hover': { opacity: 1 }
        }}
        // onClick={onViewAll}
        >
          <Typography sx={{
            fontSize: { xs: '0.85rem', sm: '1rem' },
            color: 'secondary.main', fontWeight: 600
          }}>Xem tất cả</Typography>
          <ArrowForwardIosRoundedIcon sx={{
            fontSize: { xs: 12, sm: 14 }, color: 'secondary.main'
          }} />
        </Box>
      </Box>

      {isLoading ? (
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 1.5 }}>
          {Array.from({ length: 4 }).map((_, i) => (
            <HomeProductCardSkeleton key={i} variant="landscape" />
          ))}
        </Box>
      ) : (
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 1.5 }}>
          {products.map((product, idx) => (
            <Box key={product.spu_code || product.mongo_id || product._id} sx={{ position: 'relative' }}>
              {idx < 3 && (
                <Box sx={{
                  position: 'absolute', top: -8, left: 8, zIndex: 1,
                  width: 24, height: 24, borderRadius: '50%',
                  bgcolor: rankColors[idx],
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  boxShadow: '0 2px 8px rgba(0,0,0,0.22)'
                }}>
                  <Typography sx={{ fontSize: '0.65rem', fontWeight: 900, color: '#fff', lineHeight: 1 }}>
                    #{idx + 1}
                  </Typography>
                </Box>
              )}
              <HomeProductCard product={product} variant="landscape" />
            </Box>
          ))}
          {products.length === 0 && (
            <Box sx={{ gridColumn: '1 / -1', py: 3, textAlign: 'center' }}>
              <Typography sx={{ fontSize: '0.85rem', color: '#bbb' }}>Không có sản phẩm nào</Typography>
            </Box>
          )}
        </Box>
      )}
    </Box>
  )
}

export default ProductBestSellerSection
