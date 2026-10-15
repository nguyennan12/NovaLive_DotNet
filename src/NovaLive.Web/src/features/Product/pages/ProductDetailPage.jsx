import LocalMallRoundedIcon from '@mui/icons-material/LocalMallRounded'
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined'
import { Box, Button, Grid } from '@mui/material'
import { useEffect, useState } from 'react'
import { useDispatch } from 'react-redux'
import { useNavigate, useParams } from 'react-router-dom'
import { toast } from 'react-toastify'
import { PageSkeleton } from '~/common/components/common/loading/PageSkeleton'
import { useAddToCart } from '~/features/Cart/hooks/useAddToCard'
import { useCartMutations } from '~/features/Cart/hooks/useCartMutations'
import { setSelectedIds } from '~/store/cart/cartSlice'
import ProductGridSection from '~/features/Home/components/ProductGridSection'
import ProductReviewsSection from '~/features/Review/components/ProductReviewsSection'
import { glassSx } from '~/theme'
import ProductAttributesTable from '../components/ProductDetailPage/ProductAttributesTable'
import { ProductDescription } from '../components/ProductDetailPage/ProductDescription'
import ProductImageGallery from '../components/ProductDetailPage/ProductImageGallery'
import ProductInfo from '../components/ProductDetailPage/ProductInfo'
import ProductVariantSelector, { SkuPriceLine } from '../components/ProductDetailPage/ProductVariantSelector'
import ShopInfoCard from '../components/ProductDetailPage/ShopInfoCard'
import { useProductDetail, useSelectedSku } from '../hooks/useProductDetail'
import { useProducts } from '../hooks/useProducts'
import { useFlashSale } from '~/features/Home/hooks/useFlashSale'


const ProductDetailPage = () => {
  const { productId } = useParams()
  const navigate = useNavigate()
  const { data: product, isLoading, isError, error } = useProductDetail(productId)
  const [selectedSkuId, setSelectedSkuId] = useState(null)
  const { products } = useProducts()
  const [quantity, setQuantity] = useState(1)

  const onChangeQuantity = (newQty) => {
    if (newQty < 1) return
    setQuantity(newQty)
  }

  useEffect(() => {
    if (isError) {
      toast.error(error?.response?.data?.message ?? 'Không tìm thấy sản phẩm')
      navigate(-1)
    }
  }, [isError, error, navigate])

  const skuList = product?.sku_list ?? []
  const localSku = skuList.find((s) => s.sku_id === selectedSkuId)
    ?? skuList.find((s) => s.sku_default)
    ?? skuList[0]
    ?? null

  const { data: fetchedSku } = useSelectedSku(product?._id, selectedSkuId)
  const selectedSku = fetchedSku ?? localSku
  const attributes = product?.spu_attributes ?? []

  const { flashSaleProducts, hasActiveCampaign } = useFlashSale()
  const flashSaleItem = hasActiveCampaign
    ? flashSaleProducts.find(p => p._id === product?._id)
    : null

  const dispatch = useDispatch()
  const { addToCartAsync } = useCartMutations()
  const { handleAddToCart, isPending } = useAddToCart({ product, selectedSku, quantity })

  const handleBuyNow = async () => {
    if (!selectedSku?._id) {
      toast.warning('Vui lòng chọn phiên bản sản phẩm')
      return
    }
    try {
      await addToCartAsync({
        skuId: selectedSku._id,
        productId: selectedSku.sku_spuId,
        shopId: product.spu_shopId,
        quantity
      })
      dispatch(setSelectedIds([String(selectedSku._id)]))
      navigate('/order')
    } catch {
      // toast đã được xử lý trong addMutation
    }
  }

  return (
    <Box sx={{ px: { xs: 1.5, sm: 2.5, md: 4 }, pt: { xs: 2, md: 3 }, pb: { xs: '100px', sm: '110px' } }}>

      {isLoading ? <PageSkeleton /> : (
        <Grid container spacing={2.5}>

          <Grid size={{ xs: 12, md: 5 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Box sx={{ p: 2, bgcolor: 'primary.main', ...glassSx, borderRadius: '12px' }}>
                <ProductImageGallery thumbUrl={product?.spu_thumb} productName={product?.spu_name} />
              </Box>
            </Box>
          </Grid>

          <Grid size={{ xs: 12, md: 7 }}>
            <Box sx={{
              display: 'flex', flexDirection: 'column', gap: 2,
              bgcolor: 'primary.main', ...glassSx, borderRadius: '12px',
              height: '100%', justifyContent: 'space-between'
            }}>
              <ProductInfo product={product} selectedSku={selectedSku} flashSaleItem={flashSaleItem} />

              {/* Price + action buttons */}
              <Box>
                {skuList.length > 0 && (
                  <Box>
                    <ProductVariantSelector
                      skuList={skuList}
                      selectedSkuId={selectedSkuId}
                      onSelect={setSelectedSkuId}
                      thumbFallback={product?.spu_thumb}
                    />
                  </Box>
                )}
                <SkuPriceLine quantity={quantity} onChangeQuantity={onChangeQuantity} selectedSku={selectedSku} />

                <Box sx={{ display: 'flex', gap: 1.5, mt: 2 }}>
                  <Button
                    variant="outlined" fullWidth
                    startIcon={<ShoppingCartOutlinedIcon />}
                    loading={isPending}
                    onClick={handleAddToCart}
                    sx={{
                      textTransform: 'none', fontWeight: 700, fontSize: '0.9rem',
                      borderRadius: '12px', py: 1.25,
                      borderColor: 'secondary.main', color: 'secondary.main',
                      '&:hover': { bgcolor: 'rgba(52,133,247,0.06)' }
                    }}
                  >
                    Giỏ hàng
                  </Button>
                  <Button
                    variant="contained" fullWidth
                    startIcon={<LocalMallRoundedIcon />}
                    loading={isPending}
                    onClick={handleBuyNow}
                    sx={{
                      textTransform: 'none', fontWeight: 700, fontSize: '0.9rem',
                      borderRadius: '12px', py: 1.25,
                      bgcolor: 'secondary.main',
                      background: 'linear-gradient(90deg, #568dfbff, #69aedc, #8acdde)',
                      color: '#ffffff',
                      '&:hover': { bgcolor: '#4e96f6ff', boxShadow: '0 6px 20px rgba(52,133,247,0.45)' }
                    }}
                  >
                    Mua ngay
                  </Button>
                </Box>
              </Box>
            </Box>
          </Grid>

          {/* DESCRIPTION + ATTRIBUTE */}
          <Grid container size={12} spacing={2} sx={{ mt: 2 }}>
            {product?.spu_description && (
              <Grid size={{ xs: 12, md: 7 }}>
                <ProductDescription text={product.spu_description} sx={{ height: '100%' }} />
              </Grid>
            )}
            {attributes.length > 0 && (
              <Grid size={{ xs: 12, md: 5 }}>
                <Box sx={{ bgcolor: 'primary.main', ...glassSx }}>
                  <ProductAttributesTable attributes={attributes} />
                </Box>
              </Grid>
            )}
          </Grid>

          <Grid size={12}>
            <ShopInfoCard shopId={product?.spu_shopId} />
          </Grid>

          <Grid size={12}>
            <ProductReviewsSection
              productId={productId}
              ratingAvg={product?.spu_ratingsAvg}
              ratingCount={product?.spu_ratingCount}
            />
          </Grid>

          <Grid size={12}>
            <ProductGridSection products={products} isLoading={isLoading} />
          </Grid>

        </Grid>
      )}
    </Box>
  )
}

export default ProductDetailPage
