import ShoppingCartCheckoutRoundedIcon from '@mui/icons-material/ShoppingCartCheckoutRounded'
import { Box, Button, Container, Grid, Typography } from '@mui/material'
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
import CodOtpDialog from '../components/shared/CodOtpDialog'
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

  //state item và checkout
  const { selectedItems, cartId } = useCart()
  const canCheckout = selectedItems.length > 0
  const { appliedshopDiscounts, appliedProductVoucher, appliedFreeshipVoucher } = useApplyDiscounts()
  const { control, handleSubmit } = useForm({ mode: 'onChange', defaultValues: { paymentMethod: 'cod' } })

  //state địa chỉ của user
  const { addressUser, allAddressUser } = useAddress(user)
  const [selectedAddressOverride, setSelectedAddressOverride] = useState(null)
  const selectedAddress = selectedAddressOverride ?? addressUser

  // State cho COD OTP dialog
  const [createdOrder, setCreatedOrder] = useState(null)
  const [showCodOtp, setShowCodOtp] = useState(false)

  // Build payload cho useCheckout
  const checkoutPayload = useMemo(() => {
    if (!canCheckout || !cartId) return undefined
    return {
      cartId,
      shopOrderIds: buildShopOrderIds(selectedItems, appliedshopDiscounts),
      userAddressId: selectedAddress?._id,
      productDiscountCode: appliedProductVoucher?.code ?? '',
      shippingDiscountCode: appliedFreeshipVoucher?.code ?? ''
    }
  }, [cartId, selectedItems, appliedshopDiscounts, appliedProductVoucher, appliedFreeshipVoucher, canCheckout, selectedAddress?._id])
  const checkoutData = useCheckout(checkoutPayload)

  // Group selected items theo shop để hiển thị trong OrderProductList
  const shopGroups = useMemo(() => {
    const map = {}
    selectedItems.forEach(item => {
      const key = String(item.shopId)
      if (!map[key]) map[key] = { shopId: item.shopId, shopName: item.shopName, items: [] }
      map[key].items.push(item)
    })
    return Object.values(map)
  }, [selectedItems])

  // Mutation đặt hàng
  const { orderMutation } = useOrderMutation({ setCreatedOrder, setShowCodOtp })

  const hasAddress = !!(selectedAddress || user?.default_address_id)
  const canSubmit = canCheckout && hasAddress && !orderMutation.isPending

  const onSubmit = (formData) => {
    if (!hasAddress) {
      toast.warning('Vui lòng chọn địa chỉ giao hàng trước khi đặt hàng')
      return
    }
    if (!canCheckout) {
      toast.warning('Không có sản phẩm nào để đặt hàng')
      return
    }

    orderMutation.mutate({
      cartId,
      client_totalCheckout: checkoutData.finalCheckout,
      shopOrderIds: buildShopOrderIds(selectedItems, appliedshopDiscounts),
      userAddressId: selectedAddress?._id || user?.default_address_id,
      productDiscountCode: appliedProductVoucher?.code ?? '',
      shippingDiscountCode: appliedFreeshipVoucher?.code ?? '',
      userPayment: formData.paymentMethod
    })
  }

  const handleCodOtpClose = () => {
    setShowCodOtp(false)
    toast.info('Đơn hàng đã tạo. Kiểm tra email để xác nhận OTP hoàn tất.')
    navigate('/')
  }

  const handleCodOtpSuccess = () => {
    setShowCodOtp(false)
    navigate('/')
  }

  return (
    <>
      <Container maxWidth="lg" sx={{ pt: { xs: 2.5, md: 4 }, pb: { xs: '100px', md: '80px' } }}>
        <Grid container columns={10} spacing={{ xs: 2, md: 3 }}>

          {/*  Left column: Sản phẩm, Địa chỉ, Vận chuyển, Voucher */}
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
              />

              {/* Voucher toàn sàn (reuse GlobalDiscountSection) */}
              <OrderVoucherSection subtotal={checkoutData.totalRawPrice} />
            </Box>
          </Grid>

          {/* Right column: Summary + Thanh toán (sticky)  */}
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
                hasFreeShip={checkoutData.hasFreeShip}
                itemCount={selectedItems.length}
              />

              {/* Nút đặt hàng — chỉ hiện trên desktop; mobile dùng sticky bottom */}
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
              {formatVND(checkoutData.totalPrice)}
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

      {/* Dialog xác nhận OTP cho COD */}
      <CodOtpDialog
        open={showCodOtp}
        onClose={handleCodOtpClose}
        orderId={createdOrder?._id}
        email={user?.user_email}
        onSuccess={handleCodOtpSuccess}
      />
    </>
  )
}

export default OrderPage
