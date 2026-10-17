import { useQuery } from '@tanstack/react-query'
import { calculateCheckoutAPI } from '~/common/apis/services/orderService'

export const useCheckout = (payload) => {
  const enabled = !!payload && (
    (Array.isArray(payload.cartItemIds) && payload.cartItemIds.length > 0) ||
    !!payload.cartId
  )

  const { data: checkout = {} } = useQuery({
    queryKey: ['checkout-preview', payload],
    queryFn: () => calculateCheckoutAPI(payload),
    enabled,
    staleTime: 1000 * 5
  })

  // Normalize between new .NET DTO and legacy preview structure
  const totalRawPrice = checkout.totalItemsAmount ?? checkout.checkoutOrder?.totalRawPrice ?? checkout.totalPrice ?? 0
  const feeShip = checkout.totalShippingFee ?? checkout.checkoutOrder?.feeShip ?? checkout.feeShip ?? 0
  const totalShopDiscount = checkout.totalShopDiscount ?? checkout.checkoutOrder?.totalShopDiscount ?? 0
  const amoutGlobalDiscountProduct = checkout.totalPlatformDiscount ?? checkout.amoutGlobalDiscountProduct ?? 0
  const amoutGlobalDiscountShipping = checkout.totalShippingDiscount ?? checkout.amoutGlobalDiscountShipping ?? 0
  const finalCheckout = checkout.grandTotal ?? checkout.checkoutOrder?.finalCheckout ?? Math.max(0, totalRawPrice + feeShip - totalShopDiscount - amoutGlobalDiscountProduct - amoutGlobalDiscountShipping)

  const totalFeeShip = Math.max(0, feeShip - amoutGlobalDiscountShipping)
  const totalDiscount = totalShopDiscount + amoutGlobalDiscountProduct + amoutGlobalDiscountShipping
  const hasFreeShip = amoutGlobalDiscountShipping > 0

  return {
    ...checkout,
    totalRawPrice,
    totalShopDiscount,
    amoutGlobalDiscountProduct,
    amoutGlobalDiscountShipping,
    finalCheckout,
    totalFeeShip,
    totalDiscount,
    feeShip,
    hasFreeShip
  }
}