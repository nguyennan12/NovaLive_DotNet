import GridViewRoundedIcon from '@mui/icons-material/GridViewRounded'
import { Box, Divider } from '@mui/material'
import { useCallback, useState } from 'react'
import { useInfiniteScroll } from '~/common/hooks/useScroll'
import { BannerHomePage, PosterFirst } from '../components/BannerHomePage'
import CategorySection from '../components/CategorySection'
import HomeFilterBar from '../components/HomeFilterBar'
import ProductBestSellerSection from '../components/ProductBestSellerSection'
import ProductGridSection from '../components/ProductGridSection'
import ProductScrollSection from '../components/ProductScrollSection'
import { useHomeProducts } from '../hooks/useHomeProducts'
import { useFlashSale } from '../hooks/useFlashSale'

import { DEFAULT_FILTERS } from '~/common/utils/constant'

export const HomePage = () => {
  const [filters, setFilters] = useState(DEFAULT_FILTERS)

  const handleFilterChange = useCallback((key, value) => {
    setFilters(prev => ({ ...prev, [key]: value }))
  }, [])

  const { isFiltering, filteredProducts, filterLoading, featuredProducts, bestSellers, newArrivals, isLoading, fetchNextPage, hasNextPage, isFetchingNextPage } = useHomeProducts(filters)
  const lastProductRef = useInfiniteScroll({ isFiltering, isFetchingNextPage, hasNextPage, fetchNextPage })

  const { campaign, flashSaleProducts, isLoading: flashSaleLoading, hasActiveCampaign } = useFlashSale()

  // Show flash sale products when active, otherwise fall back to featured products
  const scrollProducts = hasActiveCampaign ? flashSaleProducts : featuredProducts
  const scrollLoading = hasActiveCampaign ? flashSaleLoading : isLoading

  return (
    <Box sx={{
      maxWidth: 1440,
      mx: 'auto',
      px: { xs: 1.5, sm: 2.5, md: 4, lg: 5 },
      pt: { xs: 2, md: 3 },
      pb: { xs: '100px', sm: '110px' },
      minHeight: '100vh',
      background: 'transparent'
    }}>
      <BannerHomePage flashSaleBanners={campaign?.campaign_banner ?? []} />

      <CategorySection />

      <Divider sx={{ mb: { xs: 2, md: 3 }, opacity: 0.4 }} />

      <ProductScrollSection products={scrollProducts} isLoading={scrollLoading} campaign={campaign} showFlashBadge={hasActiveCampaign} />

      <ProductBestSellerSection products={bestSellers} isLoading={isLoading} />

      <PosterFirst />

      <HomeFilterBar filters={filters} onFilterChange={handleFilterChange} />

      {isFiltering ? (
        <ProductGridSection
          title="Kết quả lọc"
          subtitle={!filterLoading && filteredProducts.length > 0
            ? `${filteredProducts.length} sản phẩm`
            : undefined}
          icon={<GridViewRoundedIcon sx={{ fontSize: 18, color: 'secondary.main' }} />}
          accent="#3485f7"
          products={filteredProducts}
          isLoading={filterLoading}
          hideSeeMore
        />
      ) : (
        <ProductGridSection products={newArrivals} isLoading={isLoading}
          lastItemRef={lastProductRef}
          isFetchingNextPage={isFetchingNextPage} />
      )}
    </Box>
  )
}
