import { useQuery } from '@tanstack/react-query'
import { checkoutAPI } from '~/common/apis/services/orderService'

export const useCheckout = (payload) => {
  const enabled = !!payload?.cartId
  const { data: checkout = {} } = useQuery({
    queryKey: ['checkout', payload],
    queryFn: () => checkoutAPI(payload),
    enabled,
    staleTime: 0
  })

  const { amoutGlobalDiscountProduct = 0, amoutGlobalDiscountShipping = 0, checkoutOrder = {} } = checkout
  const { totalRawPrice = 0, feeShip = 0, totalShopDiscount = 0, finalCheckout = 0 } = checkoutOrder

  const totalFeeShip = Math.max(0, feeShip - amoutGlobalDiscountShipping)
  const totalDiscount = amoutGlobalDiscountProduct + amoutGlobalDiscountShipping + totalShopDiscount

  const hasFreeShip = feeShip !== totalFeeShip
  return {
    ...checkout,
    totalRawPrice,
    totalShopDiscount,
    finalCheckout,
    totalFeeShip,
    totalDiscount,
    hasFreeShip
  }
}