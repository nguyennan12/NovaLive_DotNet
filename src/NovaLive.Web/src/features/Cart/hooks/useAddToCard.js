import { toast } from 'react-toastify'
import { useCartMutations } from '~/features/Cart/hooks/useCartMutations'

export const useAddToCart = ({ product, selectedSku, quantity }) => {
  const { addToCart, isPending } = useCartMutations()

  const handleAddToCart = () => {
    if (!product) return

    const skuId = selectedSku?._id
    if (!skuId) {
      toast.warning('Vui lòng chọn phiên bản sản phẩm')
      return
    }

    const payload = {
      skuId,
      productId: selectedSku.sku_spuId,
      shopId: product.spu_shopId,
      quantity,
      guestProduct: {
        spuId: product._id,
        name: product.spu_name,
        skuName: selectedSku.sku_name ? { 'Phân loại': selectedSku.sku_name } : null,
        image: product.spu_thumb,
        price: selectedSku.sku_price ?? product.spu_price,
        availableStock: selectedSku.sku_stock,
        shopId: product.spu_shopId,
        shopName: product.spu_shop_name || 'Shop'
      }
    }

    addToCart(payload)
  }

  return { handleAddToCart, isPending }
}
