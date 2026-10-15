import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import ShoppingCartCheckoutRoundedIcon from '@mui/icons-material/ShoppingCartCheckoutRounded'
import { Box, Button, Chip, Divider, Typography } from '@mui/material'
import { useNavigate } from 'react-router-dom'
import { useSelector } from 'react-redux'
import { selectCurrentUser } from '~/store/user/userSlice'
import { buildShopOrderIds } from '~/common/utils/builder'
import { formatVND } from '~/common/utils/formatters'
import GlobalDiscountSection from '~/features/Discount/components/DIscountSelect/GlobalDiscountSection'
import { useApplyDiscounts } from '~/features/Discount/hook/useApplyDiscounts'
import { useCheckout } from '~/features/Order/hooks/useCheckout'
import { glassSx, gradientText } from '~/theme'
import { useCart } from '../hooks/useCart'


function SummaryRow({ label, value, highlight = false, isDiscount = false }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 0.4 }}>
      <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.6)' }}>{label}</Typography>
      <Typography sx={{
        fontSize: highlight ? '1.05rem' : '0.88rem',
        fontWeight: highlight ? 800 : 500,
        color: isDiscount ? 'success.main' : highlight ? 'secondary.main' : 'primary.contrastText'
      }}>
        {isDiscount && value > 0 ? `− ${formatVND(value)}` : formatVND(value)}
      </Typography>
    </Box>
  )
}

function CartSummary() {
  const navigate = useNavigate()
  const { selectedItems, cartId } = useCart()
  const user = useSelector(selectCurrentUser)
  const { setProductVoucher, setFreeshipVoucher,
    clearFreeshipVoucher, clearProductVoucher,
    appliedFreeshipVoucher, appliedProductVoucher, appliedshopDiscounts } = useApplyDiscounts()

  let payload = {}
  const canCheckout = selectedItems.length > 0
  if (canCheckout) {
    payload = {
      cartId,
      shopOrderIds: buildShopOrderIds(selectedItems, appliedshopDiscounts),
      userAddressId: user.default_address_id,
      productDiscountCode: appliedProductVoucher?.code ?? '',
      shippingDiscountCode: appliedFreeshipVoucher?.code ?? ''
    }
  }
  const { totalRawPrice, totalShopDiscount, finalCheckout, totalFeeShip, amoutGlobalDiscountProduct, totalDiscount, hasFreeShip } = useCheckout(payload)
  // const canCheckout = selectedItems.length > 0
  // const totalDiscount = (shopDiscountTotal || 0) + (productVoucherDiscount || 0)

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, position: { md: 'sticky' }, top: { md: 90 } }}>

      {/* Khối tính tiền */}
      <Box sx={{ bgcolor: 'primary.main', ...glassSx, borderRadius: 3, p: 2.5 }}>
        <Typography sx={{ fontSize: '0.98rem', fontWeight: 700, ...gradientText, mb: 2 }}>
          Giỏ hàng của bạn
        </Typography>

        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5 }}>
          <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.6)' }}>Sản phẩm đã chọn</Typography>
          <Typography sx={{ fontSize: '0.88rem', fontWeight: 600, color: 'primary.contrastText' }}>
            {selectedItems.length}
          </Typography>
        </Box>

        <SummaryRow label="Tạm tính" value={totalRawPrice} />

        {totalShopDiscount > 0 && (
          <SummaryRow label="Giảm từ shop" value={totalShopDiscount} isDiscount />
        )}

        {amoutGlobalDiscountProduct > 0 && (
          <SummaryRow label="Voucher giảm giá" value={amoutGlobalDiscountProduct} isDiscount />
        )}

        {/* Freeship chip — shipping hiện tại = 0, hiển thị thông báo */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 0.4 }}>
          <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.6)' }}>Phí vận chuyển</Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
            {hasFreeShip && (
              <Chip
                icon={<LocalShippingRoundedIcon sx={{ fontSize: '11px !important' }} />}
                label="Freeship"
                size="small"
                sx={{
                  height: 18, fontSize: '0.62rem', fontWeight: 700,
                  bgcolor: 'rgba(245,158,11,0.1)', color: '#d97706',
                  border: '1px solid rgba(245,158,11,0.3)',
                  '& .MuiChip-label': { px: 0.75 },
                  '& .MuiChip-icon': { color: '#d97706', ml: 0.5 }
                }}
              />
            )}
            <Typography sx={{ fontSize: '0.88rem', fontWeight: 500, color: 'primary.contrastText' }}>
              {formatVND(totalFeeShip)}
            </Typography>
          </Box>
        </Box>

        <Divider sx={{ my: 1.5, borderColor: 'divider' }} />

        {totalDiscount > 0 && (
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.75 }}>
            <Typography sx={{ fontSize: '0.8rem', color: 'success.main', fontWeight: 500 }}>Tiết kiệm được</Typography>
            <Typography sx={{ fontSize: '0.88rem', fontWeight: 700, color: 'success.main' }}>
              &minus;&nbsp;{formatVND(totalDiscount)}
            </Typography>
          </Box>
        )}

        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', mb: 2.5 }}>
          <Typography sx={{ fontSize: '0.88rem', fontWeight: 600, color: 'primary.contrastText' }}>Tổng thanh toán</Typography>
          <Typography sx={{ fontSize: '1.3rem', fontWeight: 800, color: 'secondary.main', lineHeight: 1 }}>
            {formatVND(finalCheckout)}
          </Typography>
        </Box>

        <Button
          variant="contained"
          fullWidth
          disabled={!canCheckout}
          startIcon={<ShoppingCartCheckoutRoundedIcon />}
          onClick={() => canCheckout && navigate('/order')}
          sx={{
            textTransform: 'none', fontWeight: 700, fontSize: '0.93rem',
            borderRadius: '12px', py: 1.3,
            ...(canCheckout && {
              background: 'linear-gradient(90deg, #568dfb, #69aedc, #8acdde)',
              color: '#fff',
              '&:hover': { boxShadow: '0 6px 20px rgba(52,133,247,0.4)' }
            })
          }}
        >
          {canCheckout ? `Thanh toán (${selectedItems.length} sản phẩm)` : 'Chọn sản phẩm để mua'}
        </Button>
      </Box>

      {/* Voucher toàn sàn */}
      <GlobalDiscountSection
        appliedProductVoucher={appliedProductVoucher}
        setProductVoucher={setProductVoucher}
        clearProductVoucher={clearProductVoucher}
        appliedFreeshipVoucher={appliedFreeshipVoucher}
        setFreeshipVoucher={setFreeshipVoucher}
        clearFreeshipVoucher={clearFreeshipVoucher}
        subtotal={totalRawPrice}
      />
    </Box>
  )
}

export default CartSummary
