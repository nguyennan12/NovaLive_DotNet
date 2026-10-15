import { Box, Typography } from '@mui/material'
import ProductActionCards from '../components/ProductPage/ProductActionCards'
import ProductFilterBar from '../components/ProductPage/ProductFilterBar'
import ProductList from '../components/ProductPage/ProductList'
import { FormProvider, useForm } from 'react-hook-form'
import { useQuery } from '@tanstack/react-query'
import { getProductStatsAPI } from '~/common/apis/services/productService'

const ProductsPage = () => {
  const methods = useForm({
    defaultValues: {
      keyword: '',
      category: 'all',
      stock: 'all',
      status: 'all',
      sort: 'newest'
    }
  })

  const { data: serverStats, isLoading } = useQuery({
    queryKey: ['products_stats'],
    queryFn: () => getProductStatsAPI()
  })

  const stats = {
    totalProducts: serverStats?.totalProducts || 0,
    lowStockCount: serverStats?.lowStockCount || 0,
    outOfStockCount: serverStats?.outOfStockCount || 0,
    addedThisWeek: 0,
    activeDiscounts: 0,
    endingSoonDiscounts: 0,
    nextEventDate: 'Oct 30',
    nextEventTitle: 'Flash Sale',
    daysRemaining: 5
  }

  return (
    <FormProvider {...methods}>
      {/* 1. Biến Box tổng thành Flex Column, chiếm 100% chiều cao component cha */}
      <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>

        {/* Header - flexShrink: 0 để khối này không bao giờ bị bóp nhỏ lại */}
        <Box sx={{ mb: 3, flexShrink: 0 }}>
          <Typography
            sx={{
              fontSize: '1.25rem',
              fontWeight: 700,
              color: 'primary.contrastText',
              letterSpacing: '-0.02em'
            }}
          >
            Products
          </Typography>
          <Typography sx={{ fontSize: '0.8rem', color: 'primary.contrastText', opacity: 0.45, mt: 0.25 }}>
            Manage your store inventory, stock and promotions
          </Typography>
        </Box>

        {/* Các Component phía trên cũng không được bóp nhỏ */}
        <Box sx={{ flexShrink: 0 }}>
          <ProductActionCards stats={stats} loading={isLoading} />
        </Box>

        <Box sx={{ flexShrink: 0 }}>
          <ProductFilterBar />
        </Box>
        <Box sx={{ flexGrow: 1, minHeight: 0, display: 'flex', flexDirection: 'column' }}>
          <ProductList />
        </Box>

      </Box>
    </FormProvider>
  )
}

export default ProductsPage