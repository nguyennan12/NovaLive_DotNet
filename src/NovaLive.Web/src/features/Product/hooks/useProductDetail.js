import { useQuery } from '@tanstack/react-query'
import { getOneSkusAPI, getProductDetailAPI } from '~/common/apis/services/productService'

export const useProductDetail = (productId) => {
  return useQuery({
    queryKey: ['product_detail', productId],
    queryFn: () => getProductDetailAPI(productId),
    enabled: !!productId,
    staleTime: 2 * 60 * 1000
  })
}


export const useSelectedSku = (spuId, skuId) => {
  return useQuery({
    queryKey: ['sku_detail', spuId, skuId],
    queryFn: () => getOneSkusAPI(spuId, skuId),
    enabled: !!spuId && !!skuId,
    staleTime: 60 * 1000
  })
}
