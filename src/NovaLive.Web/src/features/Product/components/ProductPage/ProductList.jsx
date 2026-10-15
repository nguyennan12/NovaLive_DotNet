import { Box, CircularProgress, Typography } from '@mui/material'
import { useFormContext, useWatch } from 'react-hook-form'
import { useSelector } from 'react-redux'
import { useInfiniteScroll } from '~/common/hooks/useScroll'
import { selectCurrentUser } from '~/store/user/userSlice'
import { useHomeProducts } from '~/features/Home/hooks/useHomeProducts'
import ProductCard from './ProductCard'

const ProductList = () => {
  const user = useSelector(selectCurrentUser)
  const { control } = useFormContext()
  const labelFiltler = useWatch({ control })

  const filters = {
    ...labelFiltler,
    shopId: user.user_shop
  }

  const { isFiltering, filteredProducts, fetchNextPage, hasNextPage, isFetchingNextPage } = useHomeProducts(filters)
  const lastProductRef = useInfiniteScroll({ isFiltering, isFetchingNextPage, hasNextPage, fetchNextPage })

  return (
    <>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1.5 }}>
        <Typography sx={{ fontSize: '0.875rem', fontWeight: 600, color: 'primary.contrastText' }}>
          Product List
        </Typography>
        <Typography sx={{ fontSize: '0.75rem', color: 'primary.contrastText', opacity: 0.45 }}>
          Total {filteredProducts.length} items
        </Typography>
      </Box>

      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: 'repeat(auto-fill, minmax(135px, 1fr))',
            sm: 'repeat(auto-fill, 220px)'
          },
          gap: 1.5,
          width: '100%',
          justifyContent: 'center',
          p: 1
        }}
      >
        {filteredProducts.map((product, idx) => {
          const isLast = filteredProducts.length === idx + 1
          return (
            <Box key={product.spu_code} ref={isLast ? lastProductRef : null} >
              <ProductCard product={product} index={idx} />
            </Box>
          )
        })}
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 1 }}>
          <CircularProgress size={20} />
        </Box>
      </Box>
    </>
  )
}

export default ProductList
