import ShoppingCartCheckoutRoundedIcon from '@mui/icons-material/ShoppingCartCheckoutRounded'
import { Alert, Box, Button, Container, Grid, Typography } from '@mui/material'
import { useMemo, useState } from 'react'
import { createPortal } from 'react-dom'
import { useForm } from 'react-hook-form'
import { useSelector } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { selectCurrentUser } from '~/store/user/userSlice'
import { buildShopOrderIds } from '~/common/utils/builder'
import { formatVND } from '~/common/utils/formatters'
import { useAddress } from '~/features/Address/hooks/useAddresss'
import { useCart } from '~/features/Cart/hooks/useCart'
import { useApplyDiscounts } from '~/features/Discount/hook/useApplyDiscounts'
import PaymentQrModal from '../components/shared/PaymentQrModal'
import OrderAddressSection from '../components/OrderPage/OrderAddressSection'
import OrderPaymentMethod from '../components/OrderPage/OrderPaymentMethod'
import OrderProductList from '../components/OrderPage/OrderProductList'
import OrderShippingInfo from '../components/OrderPage/OrderShippingInfo'
import OrderSummary from '../components/OrderPage/OrderSummary'
import OrderVoucherSection from '../components/OrderPage/OrderVoucherSection'
import { useCheckout } from '../hooks/useCheckout'
import { useOrderMutation } from '../hooks/useOrderMutation'

function OrderPage() {
  const navigate = useNavigate()
  const user = useSelector(selectCurrentUser)

  // State giỏ hàng & sản phẩm được chọn
  const { selectedItems, cartId } = useCart()
  const canCheckout = selectedItems.length > 0
  const localSubtotal = useMemo(() => selectedItems.reduce(
    (sum, item) => sum + (item.isFlashSale ? item.flashSalePrice : item.price) * item.quantity,
    0
  ), [selectedItems])
  const { appliedshopDiscounts, appliedProductVoucher, appliedFreeshipVoucher } = useApplyDiscounts()
  const { control, handleSubmit } = useForm({
    mode: 'onChange',
    defaultValues: { paymentMethod: 'vietqr' }
  })

  // State địa chỉ người nhận
  const { addressUser, allAddressUser } = useAddress(user)
  const [selectedAddressOverride, setSelectedAddressOverride] = useState(null)
  const selectedAddress = selectedAddressOverride ?? addressUser
  const addressId = selectedAddress?.id || selectedAddress?._id || user?.default_address_id

  // State cho Modal Thanh toán VietQR / MoMo Realtime
  const [paymentQrData, setPaymentQrData] = useState(null)
  const [showPaymentQr, setShowPaymentQr] = useState(false)

  // Group selected items theo shop
  const shopGroups = useMemo(() => {
    const map = {}
    selectedItems.forEach((item) => {
      const key = String(item.shopId || item.shop_id || 'default')
      if (!map[key]) {
        map[key] = {
          shopId: item.shopId || item.shop_id,
          shopName: item.shopName || item.shop_name || 'Shop',
          items: []
        }
      }
      map[key].items.push(item)
    })
    return Object.values(map)
  }, [selectedItems])

  const cartItemIds = useMemo(() => {
    return selectedItems.map((item) => item.cartItemId || item.id || item._id).filter(Boolean)
  }, [selectedItems])

  // Build payload cho useCheckout preview
  const checkoutPayload = useMemo(() => {
    if (!canCheckout) return undefined
    return {
      cartItemIds,
      shippingAddressId: addressId,
      shopVouchers: (Array.isArray(appliedshopDiscounts) ? appliedshopDiscounts : Object.values(appliedshopDiscounts || {})).filter(Boolean).map((v) => ({ shopId: v.shopId, code: v.code })),
      platformProductVoucherCode: appliedProductVoucher?.code || null,
      platformFreeshipVoucherCode: appliedFreeshipVoucher?.code || null,
      // Legacy compatibility
      cartId,
      shopOrderIds: buildShopOrderIds(selectedItems, appliedshopDiscounts),
      userAddressId: addressId
    }
  }, [canCheckout, cartItemIds, addressId, appliedshopDiscounts, appliedProductVoucher, appliedFreeshipVoucher, cartId, selectedItems])

  const checkoutData = useCheckout(checkoutPayload, localSubtotal)

  // Mutation đặt hàng chính
  const { orderMutation } = useOrderMutation({
    onPaymentRequired: (data) => {
      setPaymentQrData(data)
      setShowPaymentQr(true)
    },
    onOrderSuccess: () => {
      navigate('/orders')
    }
  })

  const hasAddress = !!addressId
  const canSubmit = canCheckout && hasAddress && checkoutData.isPreviewReady && !orderMutation.isPending
  const shippingQuoteLabel = !hasAddress
    ? 'Chọn địa chỉ để tính phí'
    : checkoutData.isFetching
      ? 'Đang tính phí...'
      : checkoutData.isError
        ? 'Không tính được phí'
        : null
  const totalLabel = !hasAddress
    ? 'Chọn địa chỉ'
    : checkoutData.isFetching
      ? 'Đang tính...'
      : checkoutData.isError
        ? 'Không khả dụng'
        : null

  const onSubmit = (formData) => {
    if (!hasAddress) {
      toast.warning('Vui lòng chọn địa chỉ giao hàng trước khi đặt hàng')
      return
    }
    if (!canCheckout) {
      toast.warning('Không có sản phẩm nào để đặt hàng')
      return
    }

    const rawMethod = (formData.paymentMethod || 'vietqr').toLowerCase()
    const methodMapping = {
      vietqr: 'VietQR',
      momo: 'MoMo',
      cod: 'COD'
    }
    const backendPaymentMethod = methodMapping[rawMethod] || 'COD'

    const shippingProviders = shopGroups.map((g) => ({
      shopId: g.shopId,
      provider: 'GHN',
      serviceCode: '2'
    }))

    orderMutation.mutate({
      cartItemIds,
      shippingAddressId: addressId,
      shippingProviders,
      shopVouchers: (Array.isArray(appliedshopDiscounts) ? appliedshopDiscounts : Object.values(appliedshopDiscounts || {})).filter(Boolean).map((v) => ({ shopId: v.shopId, code: v.code })),
      platformProductVoucherCode: appliedProductVoucher?.code || null,
      platformFreeshipVoucherCode: appliedFreeshipVoucher?.code || null,
      paymentMethod: backendPaymentMethod,
      note: formData.note || '',
      // Backward compatibility fields
      cartId,
      client_totalCheckout: checkoutData.finalCheckout,
      shopOrderIds: buildShopOrderIds(selectedItems, appliedshopDiscounts),
      userAddressId: addressId,
      userPayment: rawMethod
    })
  }

  return (
    <>
      <Container maxWidth="lg" sx={{ pt: { xs: 2.5, md: 4 }, pb: { xs: '100px', md: '80px' } }}>
        <Grid container columns={10} spacing={{ xs: 2, md: 3 }}>

          {/* Left column: Sản phẩm, Địa chỉ, Vận chuyển, Voucher */}
          <Grid size={{ xs: 10, md: 7 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>

              {/* Danh sách sản phẩm theo shop */}
              <OrderProductList shopGroups={shopGroups} />

              {/* Địa chỉ giao hàng */}
              <OrderAddressSection
                selectedAddress={selectedAddress}
                allAddressUser={allAddressUser}
                onAddressChange={setSelectedAddressOverride}
              />

              {/* Thông tin vận chuyển */}
              <OrderShippingInfo
                amountFreeShip={checkoutData?.amoutGlobalDiscountShipping}
                feeShip={checkoutData.totalFeeShip}
                hasFreeShip={checkoutData.hasFreeShip}
                quoteLabel={shippingQuoteLabel}
              />

              {/* Voucher toàn sàn */}
              <OrderVoucherSection subtotal={checkoutData.totalRawPrice} />
            </Box>
          </Grid>

          {/* Right column: Summary + Thanh toán (sticky) */}
          <Grid size={{ xs: 10, md: 3 }}>
            <Box sx={{
              display: 'flex', flexDirection: 'column', gap: 2,
              position: { md: 'sticky' }, top: { md: 90 }
            }}>

              {/* Phương thức thanh toán */}
              <OrderPaymentMethod control={control} />

              {/* Tóm tắt đơn hàng */}
              <OrderSummary
                totalRawPrice={checkoutData.totalRawPrice}
                totalShopDiscount={checkoutData.totalShopDiscount}
                amoutGlobalDiscountProduct={checkoutData.amoutGlobalDiscountProduct}
                totalFeeShip={checkoutData.totalFeeShip}
                totalDiscount={checkoutData.totalDiscount}
                finalCheckout={checkoutData.finalCheckout}
                totalLabel={totalLabel}
                hasFreeShip={checkoutData.hasFreeShip}
                itemCount={selectedItems.length}
              />

              {checkoutData.isError && (
                <Alert severity="error" sx={{ borderRadius: 2 }}>
                  {checkoutData.error?.response?.data?.message || 'Không thể tính tổng đơn hàng. Vui lòng thử lại.'}
                </Alert>
              )}

              {/* Nút đặt hàng trên desktop */}
              <Box sx={{ display: { xs: 'none', md: 'block' } }}>
                <Button
                  variant="contained"
                  fullWidth
                  disabled={!canSubmit}
                  startIcon={<ShoppingCartCheckoutRoundedIcon />}
                  onClick={handleSubmit(onSubmit)}
                  sx={{
                    fontWeight: 700, fontSize: '0.98rem',
                    borderRadius: '12px', py: 1.5,
                    ...(canSubmit && {
                      background: 'linear-gradient(90deg, #568dfb, #69aedc, #8acdde)',
                      color: '#fff',
                      '&:hover': { boxShadow: '0 6px 20px rgba(52,133,247,0.4)' }
                    })
                  }}
                >
                  {orderMutation.isPending
                    ? 'Đang xử lý...'
                    : `Đặt hàng (${selectedItems.length} sản phẩm)`}
                </Button>
              </Box>
            </Box>
          </Grid>
        </Grid>
      </Container>

      {/* Mobile Sticky Bottom Bar */}
      {createPortal(
        <Box sx={{
          display: { xs: 'flex', md: 'none' },
          position: 'fixed', bottom: 0, left: 0, right: 0,
          px: 2, py: 1.5,
          bgcolor: 'rgba(255,255,255,0.94)',
          backdropFilter: 'blur(12px)',
          WebkitBackdropFilter: 'blur(12px)',
          borderTop: '1px solid rgba(0,0,0,0.08)',
          zIndex: 1300, gap: 2, alignItems: 'center'
        }}>
          <Box sx={{ flex: 1 }}>
            <Typography sx={{ fontSize: '0.72rem', color: 'rgba(45,45,45,0.5)' }}>
              Tổng thanh toán
            </Typography>
            <Typography sx={{ fontSize: '1.12rem', fontWeight: 800, color: 'secondary.main', lineHeight: 1.2 }}>
              {totalLabel || formatVND(checkoutData.finalCheckout || checkoutData.totalPrice || 0)}
            </Typography>
          </Box>

          <Button
            variant="contained"
            disabled={!canSubmit}
            startIcon={<ShoppingCartCheckoutRoundedIcon />}
            onClick={handleSubmit(onSubmit)}
            sx={{
              fontWeight: 700, fontSize: '0.93rem',
              borderRadius: '12px', px: 3, py: 1.25, flexShrink: 0,
              ...(canSubmit && {
                background: 'linear-gradient(90deg, #568dfb, #69aedc)',
                color: '#fff',
                '&:hover': { boxShadow: '0 4px 16px rgba(52,133,247,0.4)' }
              })
            }}
          >
            {orderMutation.isPending ? 'Đang xử lý...' : 'Đặt hàng'}
          </Button>
        </Box>,
        document.body
      )}

      {/* Modal Quét mã VietQR / MoMo Realtime SignalR */}
      <PaymentQrModal
        open={showPaymentQr}
        onClose={() => {
          setShowPaymentQr(false)
          navigate('/orders')
        }}
        paymentData={paymentQrData}
        onPaymentSuccess={() => {
          setShowPaymentQr(false)
        }}
      />
    </>
  )
}

export default OrderPage
