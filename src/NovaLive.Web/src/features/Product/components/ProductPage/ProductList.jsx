import { Alert, Box, CircularProgress, Typography } from '@mui/material'
import { useFormContext, useWatch } from 'react-hook-form'
import { useInfiniteQuery } from '@tanstack/react-query'
import { useInfiniteScroll } from '~/common/hooks/useScroll'
import { normalizeProduct } from '~/common/apis/services/productService'
import { getProductTablePage } from '~/features/Seller/services/productService'
import { useDebounce } from '~/common/hooks/useDebounce'
import ProductCard from './ProductCard'

const PAGE_SIZE = 24

const sortProducts = (products, sort) => {
  const items = [...products]
  switch (sort) {
  case 'price_asc': return items.sort((a, b) => a.spu_price - b.spu_price)
  case 'price_desc': return items.sort((a, b) => b.spu_price - a.spu_price)
  case 'name_az': return items.sort((a, b) => a.spu_name.localeCompare(b.spu_name))
  default: return items
  }
}

const ProductList = () => {
  const { control } = useFormContext()
  const labelFiltler = useWatch({ control })
  const keyword = useDebounce(labelFiltler.keyword, 280)
  const filters = {
    keyword,
    categoryId: labelFiltler.category === 'all' ? undefined : labelFiltler.category
  }
  const status = labelFiltler.status === 'published'
    ? 'Active'
    : labelFiltler.status === 'draft'
      ? 'Draft'
      : undefined
  const query = useInfiniteQuery({
    queryKey: ['seller_product_cards', filters, status],
    queryFn: async ({ pageParam = 1 }) => {
      const result = await getProductTablePage({ ...filters, page: pageParam, size: PAGE_SIZE }, status)
      return {
        ...result,
        items: result.items.map(normalizeProduct)
      }
    },
    initialPageParam: 1,
    getNextPageParam: (lastPage, allPages) => {
      const page = lastPage.page || allPages.length
      if (lastPage.totalPages) return page < lastPage.totalPages ? page + 1 : undefined
      return lastPage.items.length === PAGE_SIZE ? page + 1 : undefined
    },
    staleTime: 30 * 1000
  })
  const allProducts = query.data?.pages.flatMap((page) => page.items) || []
  const filteredProducts = sortProducts(allProducts.filter((product) => {
    const quantity = Number(product.spu_quantity ?? 0)
    const stockMatches = labelFiltler.stock === 'in'
      ? quantity >= 10
      : labelFiltler.stock === 'low'
        ? quantity > 0 && quantity < 10
        : labelFiltler.stock === 'out'
          ? quantity <= 0
          : true
    const statusMatches = labelFiltler.status === 'published'
      ? product.status === 'Active'
      : labelFiltler.status === 'draft'
        ? product.status === 'Draft'
        : labelFiltler.status !== 'sold out' || quantity <= 0
    return stockMatches && statusMatches
  }), labelFiltler.sort)
  const lastProductRef = useInfiniteScroll({
    isFiltering: false,
    isFetchingNextPage: query.isFetchingNextPage,
    hasNextPage: query.hasNextPage,
    fetchNextPage: query.fetchNextPage
  })

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
        {(query.isLoading || query.isFetchingNextPage) && (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 1 }}>
            <CircularProgress size={20} />
          </Box>
        )}
      </Box>
      {query.isError && <Alert severity='error' sx={{ mt: 1 }}>{query.error?.response?.data?.detail || query.error?.message || 'Không tải được danh sách sản phẩm.'}</Alert>}
      {!query.isLoading && !query.isError && filteredProducts.length === 0 && (
        <Alert severity='info' sx={{ mt: 1 }}>
          {keyword || filters.categoryId || labelFiltler.stock !== 'all' || labelFiltler.status !== 'all'
            ? 'Không tìm thấy sản phẩm khớp với bộ lọc.'
            : 'Shop chưa có sản phẩm.'}
        </Alert>
      )}
      {query.hasNextPage && !query.isFetchingNextPage && (
        <Box sx={{ display: 'flex', justifyContent: 'center', p: 1 }}>
          <Typography onClick={() => query.fetchNextPage()} role='button' tabIndex={0} sx={{ cursor: 'pointer', color: 'secondary.main' }}>
            Tải thêm sản phẩm
          </Typography>
        </Box>
      )}
    </>
  )
}

export default ProductList
