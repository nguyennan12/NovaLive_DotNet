import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import ReceiptLongRoundedIcon from '@mui/icons-material/ReceiptLongRounded'
import { Box, Chip, Divider, Typography } from '@mui/material'
import { formatVND } from '~/common/utils/formatters'
import { glassSx, gradientText } from '~/theme'

function SummaryRow({ label, value, isDiscount = false, highlight = false }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 0.45 }}>
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

function ShipRow({ value, hasFreeShip }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 0.45 }}>
      <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.6)' }}>Phí vận chuyển</Typography>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
        {hasFreeShip && (
          <Chip
            icon={<LocalShippingRoundedIcon sx={{ fontSize: '10px !important' }} />}
            label="Freeship"
            size="small"
            sx={{
              height: 18, fontSize: '0.61rem', fontWeight: 700,
              bgcolor: 'rgba(245,158,11,0.1)', color: '#d97706',
              border: '1px solid rgba(245,158,11,0.28)',
              '& .MuiChip-label': { px: 0.7 },
              '& .MuiChip-icon': { color: '#d97706', ml: 0.4 }
            }}
          />
        )}
        <Typography sx={{ fontSize: '0.88rem', fontWeight: 500, color: 'primary.contrastText' }}>
          {formatVND(value)}
        </Typography>
      </Box>
    </Box>
  )
}

function OrderSummary({
  totalRawPrice = 0,
  totalShopDiscount = 0,
  amoutGlobalDiscountProduct = 0,
  totalFeeShip = 0,
  totalDiscount = 0,
  finalCheckout = 0,
  hasFreeShip = false,
  itemCount = 0
}) {
  return (
    <Box sx={{
      bgcolor: 'primary.main', ...glassSx,
      borderRadius: 3, p: 2.5,
      border: '1px solid', borderColor: 'divider'
    }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <ReceiptLongRoundedIcon sx={{ fontSize: 18, color: 'secondary.main' }} />
        <Typography sx={{ fontWeight: 700, fontSize: '0.93rem', ...gradientText }}>
          Tóm tắt đơn hàng
        </Typography>
      </Box>

      {/* Số lượng sản phẩm */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 0.45 }}>
        <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.6)' }}>Sản phẩm</Typography>
        <Typography sx={{ fontSize: '0.88rem', fontWeight: 600, color: 'primary.contrastText' }}>
          {itemCount} sản phẩm
        </Typography>
      </Box>

      <SummaryRow label="Tạm tính" value={totalRawPrice} />

      {totalShopDiscount > 0 && (
        <SummaryRow label="Giảm từ shop" value={totalShopDiscount} isDiscount />
      )}
      {amoutGlobalDiscountProduct > 0 && (
        <SummaryRow label="Voucher giảm giá" value={amoutGlobalDiscountProduct} isDiscount />
      )}

      <ShipRow value={totalFeeShip} hasFreeShip={hasFreeShip} />

      <Divider sx={{ my: 1.5, borderColor: 'divider' }} />

      {/* Tổng tiết kiệm */}
      {totalDiscount > 0 && (
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
          <Typography sx={{ fontSize: '0.8rem', color: 'success.main', fontWeight: 600 }}>
            Tiết kiệm được
          </Typography>
          <Typography sx={{ fontSize: '0.88rem', fontWeight: 700, color: 'success.main' }}>
            &minus;&nbsp;{formatVND(totalDiscount)}
          </Typography>
        </Box>
      )}

      {/* Tổng thanh toán */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end' }}>
        <Typography sx={{ fontSize: '0.9rem', fontWeight: 600, color: 'primary.contrastText' }}>
          Tổng thanh toán
        </Typography>
        <Typography sx={{ fontSize: '1.3rem', fontWeight: 800, color: 'secondary.main', lineHeight: 1 }}>
          {formatVND(finalCheckout)}
        </Typography>
      </Box>
    </Box>
  )
}

export default OrderSummary
