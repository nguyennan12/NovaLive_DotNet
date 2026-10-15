import { Box, Typography } from '@mui/material'
import { HomeProductCard, HomeProductCardSkeleton } from './HomeProductCard'
import { SectionHeader } from '~/features/Product/components/shared/SectionHeader'
import { CircularProgress } from '@mui/material'

const ProductGridSection = ({ products = [], isLoading, lastItemRef, isFetchingNextPage }) => {
  return (
    <Box sx={{
      background: 'rgba(255, 255, 255, 0.1)',
      backdropFilter: 'blur(10px)',
      WebkitBackdropFilter: 'blur(10px)',
      borderRadius: 3,
      boxShadow: '0 4px 30px rgba(0, 0, 0, 0.05)',
      p: 2,
      my: { xs: 3.5, md: 5 }
    }}>
      <SectionHeader title={'Các sản phẩm khác'} />
      {isLoading ? (
        <Box sx={{
          display: 'grid',
          gridTemplateColumns: { xs: 'repeat(2, 1fr)', sm: 'repeat(3, 1fr)', md: 'repeat(4, 1fr)', lg: 'repeat(5, 1fr)' },
          gap: 1.5
        }}>
          {Array.from({ length: 10 }).map((_, i) => (
            <HomeProductCardSkeleton key={i} variant="portrait" />
          ))}
        </Box>
      ) : (
        <Box sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: 'repeat(2, 1fr)',
            sm: 'repeat(3, 1fr)',
            md: 'repeat(4, 1fr)',
            lg: 'repeat(5, 1fr)'
          },
          gap: 1.5
        }}>
          {products.map((product, index) => {
            const isLast = index === products.length - 1
            return (
              <HomeProductCard
                key={product.spu_code || product.mongo_id || product._id}
                product={product}
                variant="portrait"
                ref={isLast ? lastItemRef : null}
              />
            )
          })}
          {products.length === 0 && (
            <Box sx={{ gridColumn: '1 / -1', py: 5, textAlign: 'center' }}>
              <Typography sx={{ fontSize: '0.9rem', color: '#bbb' }}>
                Không tìm thấy sản phẩm nào phù hợp
              </Typography>
            </Box>
          )}
        </Box>
      )}
      {isFetchingNextPage && (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
          <CircularProgress size={30} />
        </Box>
      )}
    </Box>
  )
}

export default ProductGridSection
