import CloseRoundedIcon from '@mui/icons-material/CloseRounded'
import ContentCopyRoundedIcon from '@mui/icons-material/ContentCopyRounded'
import LocalShippingOutlinedIcon from '@mui/icons-material/LocalShippingOutlined'
import PaymentRoundedIcon from '@mui/icons-material/PaymentRounded'
import ReceiptLongRoundedIcon from '@mui/icons-material/ReceiptLongRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import {
  Box, Button, Chip, CircularProgress, Dialog, DialogContent, Divider,
  IconButton, Skeleton, Tooltip, Typography, useMediaQuery, useTheme
} from '@mui/material'
import { useState } from 'react'
import { useSelector } from 'react-redux'
import { toast } from 'react-toastify'
import { formatDate, formatVND } from '~/common/utils/formatters'
import { glassSx, gradientText } from '~/theme'
import { selectCurrentUser } from '~/store/user/userSlice'
import { STATUS_CONFIG, canCancelOrder, getPaymentMethodLabel, getPaymentStatusColor, getPaymentStatusLabel } from '../../constants/orderStatus'
import { useOrderDetail } from '../../hooks/useOrderDetail'
import { useOrderMutation } from '../../hooks/useOrderMutation'
import CancelOrderDialog from '../shared/CancelOrderDialog'
import CodOtpDialog from '../shared/CodOtpDialog'
import OrderStatusTimeline from './OrderStatusTimeline'
import { SectionBox } from '../shared/SectionOrder'
import { SectionTitle } from '../shared/SectionOrder'


// ─── Product ──────────────────────────────────────────────────────

function ProductRow({ product }) {
  const { productId, quantity } = product
  if (!productId) return null
  const name = productId.spu_name || 'Sản phẩm'
  const thumb = productId.spu_thumb
  const price = productId.spu_price || 0

  return (
    <Box sx={{ display: 'flex', gap: 1.5, py: 1, ...glassSx }}>
      <Box
        component="img"
        src={thumb}
        alt={name}
        sx={{
          width: 56, height: 56, borderRadius: 1.5,
          objectFit: 'cover', flexShrink: 0,
          border: '1px solid', borderColor: 'divider'
        }}
        onError={(e) => { e.target.style.display = 'none' }}
      />
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Typography sx={{
          fontSize: '0.83rem', fontWeight: 600, color: 'primary.contrastText',
          overflow: 'hidden', textOverflow: 'ellipsis',
          display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical'
        }}>
          {name}
        </Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: 0.5 }}>
          <Typography sx={{ fontSize: '0.73rem', color: 'rgba(45,45,45,0.5)' }}>x{quantity}</Typography>
          <Typography sx={{ fontSize: '0.83rem', fontWeight: 700, color: 'secondary.main' }}>
            {formatVND(price * quantity)}
          </Typography>
        </Box>
      </Box>
    </Box>
  )
}

// ─── PriceLine ──────────────────────────────────────────────────────

function PriceLine({ label, value, highlight = false, discount = false }) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', py: 0.4, ...glassSx }}>
      <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.6)' }}>{label}</Typography>
      <Typography sx={{
        fontSize: highlight ? '1rem' : '0.8rem',
        fontWeight: highlight ? 800 : 500,
        color: discount ? '#16a34a' : (highlight ? 'secondary.main' : 'primary.contrastText')
      }}>
        {discount && value > 0 ? `-${formatVND(value)}` : formatVND(value)}
      </Typography>
    </Box>
  )
}

// ─── OrderDetailContent ──────────────────────────────────────────────────────

function OrderDetailContent({ orderId, onRequestClose, onOrderUpdated }) {
  const { data: order, isLoading } = useOrderDetail(orderId)
  const user = useSelector(selectCurrentUser)
  const { cancelMutation, retryVNPayMutation } = useOrderMutation()
  const [copied, setCopied] = useState(false)
  const [showCancel, setShowCancel] = useState(false)
  const [showCodRetry, setShowCodRetry] = useState(false)

  const handleCopy = (text) => {
    navigator.clipboard.writeText(text).then(() => {
      setCopied(true)
      toast.success('Đã sao chép mã đơn hàng!')
      setTimeout(() => setCopied(false), 2000)
    })
  }

  if (isLoading) {
    return (
      <Box sx={{ p: 2 }}>
        {[1, 2, 3].map(i => (
          <Box key={i} sx={{ mb: 2 }}>
            <Skeleton variant="rounded" height={100} sx={{ borderRadius: 2.5 }} />
          </Box>
        ))}
      </Box>
    )
  }

  if (!order) {
    return (
      <Box sx={{ p: 4, textAlign: 'center' }}>
        <Typography sx={{ color: 'rgba(45,45,45,0.5)', fontSize: '0.875rem' }}>
          Không thể tải thông tin đơn hàng.
        </Typography>
      </Box>
    )
  }

  const {
    order_trackingNumber,
    order_status,
    order_products = [],
    order_checkout = {},
    order_shipping = {},
    order_payment = {},
    order_appliedDiscountCodes = [],
    createdAt
  } = order

  const cfg = STATUS_CONFIG[order_status] || {}
  const StatusIcon = cfg.Icon

  // Payment retry logic
  const isPendingPayment = order_status === 'pending' && order_payment.paymentStatus === 'pending'
  const canRetryVNPay = isPendingPayment && order_payment.method === 'vnpay'
  const canRetryCOD = isPendingPayment && order_payment.method === 'cod'

  const handleCancel = () => {
    cancelMutation.mutate(order._id, {
      onSuccess: () => {
        setShowCancel(false)
        onOrderUpdated?.()
        onRequestClose?.()
      }
    })
  }

  const handleRetryVNPay = () => {
    retryVNPayMutation.mutate({
      trackingNumber: order_trackingNumber,
      amount: order_checkout.finalCheckout
    })
  }

  // Group by shop
  const shopMap = {}
  order_products.forEach(p => {
    const shopName = p.productId?.spu_shopId?.shop_name || 'Shop'
    if (!shopMap[shopName]) shopMap[shopName] = []
    shopMap[shopName].push(p)
  })

  const shippingAddress = typeof order_shipping === 'string'
    ? order_shipping
    : [order_shipping.street, order_shipping.state, order_shipping.city, order_shipping.country]
      .filter(Boolean).join(', ')

  const totalDiscount = order_checkout.totalDiscount || 0
  const feeShip = order_checkout.feeShip || 0
  const totalPrice = order_checkout.totalPrice || 0
  const finalCheckout = order_checkout.finalCheckout || 0

  return (
    <Box>
      {/* Status banner */}
      <Box sx={{
        display: 'flex', alignItems: 'center', gap: 1.5,
        p: 2, mb: 2,
        background: cfg.bgColor || 'rgba(0,0,0,0.03)',
        border: '1px solid', borderColor: cfg.borderColor || 'divider',
        borderRadius: 2.5,
        backdropFilter: 'blur(6px)',
        WebkitBackdropFilter: 'blur(6px)'
      }}>
        {StatusIcon && <StatusIcon sx={{ fontSize: 28, color: cfg.color, flexShrink: 0 }} />}
        <Box sx={{ flex: 1 }}>
          <Typography sx={{ fontSize: '0.9rem', fontWeight: 800, color: cfg.color }}>
            {cfg.label}
          </Typography>
          <Typography sx={{ fontSize: '0.72rem', color: 'rgba(45,45,45,0.45)', mt: 0.25 }}>
            Cập nhật: {formatDate(order.updatedAt)}
          </Typography>
        </Box>
      </Box>

      {/* Action buttons — chỉ hiện khi đơn còn pending */}
      {(canCancelOrder(order_status) || canRetryVNPay || canRetryCOD) && (
        <SectionBox sx={{ mb: 2 }}>
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            {canCancelOrder(order_status) && (
              <Button
                size="small"
                onClick={() => setShowCancel(true)}
                sx={{
                  fontSize: '0.8rem', fontWeight: 600, py: 0.75, px: 2, borderRadius: 2,
                  border: '1px solid rgba(220,38,38,0.4)', color: '#dc2626',
                  '&:hover': { bgcolor: 'rgba(220,38,38,0.06)', borderColor: '#dc2626' }
                }}
              >
                Hủy đơn hàng
              </Button>
            )}

            {canRetryVNPay && (
              <Button
                size="small"
                disabled={retryVNPayMutation.isPending}
                startIcon={retryVNPayMutation.isPending
                  ? <CircularProgress size={13} sx={{ color: '#fff' }} />
                  : <PaymentRoundedIcon sx={{ fontSize: '15px !important' }} />}
                onClick={handleRetryVNPay}
                sx={{
                  fontSize: '0.8rem', fontWeight: 700, py: 0.75, px: 2, borderRadius: 2,
                  background: 'linear-gradient(90deg,#f59e0b,#ef8e00)',
                  color: '#fff',
                  '&:hover': { boxShadow: '0 3px 12px rgba(245,158,11,0.4)' },
                  '&:disabled': { opacity: 0.6, color: '#fff' }
                }}
              >
                Thanh toán VNPay
              </Button>
            )}

            {canRetryCOD && (
              <Button
                size="small"
                onClick={() => setShowCodRetry(true)}
                sx={{
                  fontSize: '0.8rem', fontWeight: 600, py: 0.75, px: 2, borderRadius: 2,
                  background: 'linear-gradient(90deg,#568dfb,#69aedc)',
                  color: '#fff',
                  '&:hover': { boxShadow: '0 3px 12px rgba(52,133,247,0.35)' }
                }}
              >
                Xác nhận OTP COD
              </Button>
            )}
          </Box>
        </SectionBox>
      )}

      {/* Tracking number */}
      <SectionBox>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <ReceiptLongRoundedIcon sx={{ fontSize: 16, color: 'rgba(45,45,45,0.4)' }} />
          <Typography sx={{ fontSize: '0.78rem', color: 'rgba(45,45,45,0.55)', flex: 1 }}>
            Mã đơn hàng:
          </Typography>
          <Typography sx={{ fontSize: '0.8rem', fontWeight: 700, ...gradientText }}>
            {order_trackingNumber}
          </Typography>
          <Tooltip title={copied ? 'Đã sao chép!' : 'Sao chép'}>
            <IconButton size="small" onClick={() => handleCopy(order_trackingNumber)} sx={{ p: 0.5 }}>
              <ContentCopyRoundedIcon sx={{ fontSize: 14, color: 'rgba(45,45,45,0.4)' }} />
            </IconButton>
          </Tooltip>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 0.75 }}>
          <Typography sx={{ fontSize: '0.78rem', color: 'rgba(45,45,45,0.55)', flex: 1 }}>Ngày đặt:</Typography>
          <Typography sx={{ fontSize: '0.8rem', fontWeight: 600, color: 'primary.contrastText' }}>
            {formatDate(createdAt)}
          </Typography>
        </Box>
      </SectionBox>

      {/* Timeline */}
      <SectionBox>
        <SectionTitle icon={LocalShippingOutlinedIcon} title="Hành trình đơn hàng" />
        <OrderStatusTimeline order={order} />
      </SectionBox>

      {/* Products grouped by shop */}
      {Object.entries(shopMap).map(([shopName, products]) => (
        <SectionBox key={shopName}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mb: 1 }}>
            <StorefrontRoundedIcon sx={{ fontSize: 14, color: 'secondary.main' }} />
            <Typography sx={{ fontSize: '0.8rem', fontWeight: 700, color: 'primary.contrastText' }}>
              {shopName}
            </Typography>
          </Box>
          {products.map((p, i) => (
            <Box key={String(p.skuId || i)}>
              <ProductRow product={p} />
              {i < products.length - 1 && <Divider sx={{ borderColor: 'divider' }} />}
            </Box>
          ))}
        </SectionBox>
      ))}

      {/* Price summary */}
      <SectionBox>
        <SectionTitle icon={ReceiptLongRoundedIcon} title="Tóm tắt thanh toán" />
        <PriceLine label="Tạm tính" value={totalPrice} />
        {totalDiscount > 0 && <PriceLine label="Giảm giá" value={totalDiscount} discount />}
        <PriceLine label="Phí vận chuyển" value={feeShip} />
        {order_appliedDiscountCodes.length > 0 && (
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, my: 0.5 }}>
            {order_appliedDiscountCodes.map(code => (
              <Chip
                key={code} label={code} size="small"
                sx={{
                  height: 18, fontSize: '0.65rem', fontWeight: 700,
                  bgcolor: 'rgba(22,163,74,0.1)', color: '#16a34a',
                  border: '1px solid rgba(22,163,74,0.2)',
                  '& .MuiChip-label': { px: 0.75 }
                }}
              />
            ))}
          </Box>
        )}
        <Divider sx={{ borderColor: 'divider', my: 0.75 }} />
        <PriceLine label="Tổng thanh toán" value={finalCheckout} highlight />
      </SectionBox>

      {/* Shipping address */}
      {shippingAddress && (
        <SectionBox>
          <SectionTitle icon={LocalShippingOutlinedIcon} title="Địa chỉ giao hàng" />
          <Typography sx={{ fontSize: '0.82rem', color: 'primary.contrastText', lineHeight: 1.6 }}>
            {shippingAddress}
          </Typography>
        </SectionBox>
      )}

      {/* Payment info */}
      <SectionBox>
        <SectionTitle icon={PaymentRoundedIcon} title="Thanh toán" />
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography sx={{ fontSize: '0.82rem', color: 'rgba(45,45,45,0.6)' }}>Phương thức:</Typography>
          <Typography sx={{ fontSize: '0.82rem', fontWeight: 600, color: 'primary.contrastText' }}>
            {getPaymentMethodLabel(order_payment.method)}
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 0.75 }}>
          <Typography sx={{ fontSize: '0.82rem', color: 'rgba(45,45,45,0.6)' }}>Trạng thái:</Typography>
          <Typography sx={{
            fontSize: '0.82rem', fontWeight: 700,
            color: getPaymentStatusColor(order_payment.paymentStatus)
          }}>
            {getPaymentStatusLabel(order_payment.paymentStatus)}
          </Typography>
        </Box>
      </SectionBox>

      {/* Dialogs */}
      <CancelOrderDialog
        open={showCancel}
        onClose={() => setShowCancel(false)}
        onConfirm={handleCancel}
        loading={cancelMutation.isPending}
        trackingNumber={order_trackingNumber}
      />

      <CodOtpDialog
        open={showCodRetry}
        onClose={() => setShowCodRetry(false)}
        orderId={order._id}
        email={user?.user_email}
        onSuccess={() => {
          setShowCodRetry(false)
          onOrderUpdated?.()
          onRequestClose?.()
        }}
      />
    </Box>
  )
}

// ─── OrderDetailDrawer ────────────────────────────────────────────────────────

function OrderDetailDrawer({ open, onClose, order, onOrderUpdated }) {
  const theme = useTheme()
  const fullScreen = useMediaQuery(theme.breakpoints.down('sm'))

  if (!order) return null

  return (
    <Dialog
      open={open}
      onClose={onClose}
      fullScreen={fullScreen}
      maxWidth="sm"
      fullWidth
    >
      {/* Header */}
      <Box sx={{
        display: 'flex', alignItems: 'center',
        px: 2.5, py: 1.5,
        borderBottom: '1px solid rgba(238,238,238,0.9)',
        background: 'rgba(255,255,255,0.65)',
        backdropFilter: 'blur(10px)',
        WebkitBackdropFilter: 'blur(10px)',
        flexShrink: 0
      }}>
        <Box sx={{ width: 3.5, height: 18, borderRadius: '2px', bgcolor: 'secondary.main', mr: 1.5, flexShrink: 0 }} />
        <Typography sx={{ fontSize: '1rem', fontWeight: 800, color: 'primary.contrastText', flex: 1 }}>
          Chi tiết đơn hàng
        </Typography>
        <IconButton size="small" onClick={onClose} sx={{ color: 'rgba(45,45,45,0.5)' }}>
          <CloseRoundedIcon sx={{ fontSize: 20 }} />
        </IconButton>
      </Box>

      <DialogContent sx={{
        p: 2, overflowY: 'auto',
        background: 'linear-gradient(180deg, transparent 0%, rgba(124, 176, 255, 0.2) 100%)'
      }}>
        <OrderDetailContent
          orderId={order._id}
          onRequestClose={onClose}
          onOrderUpdated={onOrderUpdated}
        />
      </DialogContent>
    </Dialog>
  )
}

export default OrderDetailDrawer
