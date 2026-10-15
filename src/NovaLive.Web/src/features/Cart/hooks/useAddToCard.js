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
      quantity
    }

    addToCart(payload)
  }

  return { handleAddToCart, isPending }
}