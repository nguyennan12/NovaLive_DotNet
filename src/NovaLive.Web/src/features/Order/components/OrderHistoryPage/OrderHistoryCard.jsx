import PaymentRoundedIcon from '@mui/icons-material/PaymentRounded'
import ShoppingBagOutlinedIcon from '@mui/icons-material/ShoppingBagOutlined'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import { Box, Button, Chip, CircularProgress, Divider, Skeleton, Typography } from '@mui/material'
import { forwardRef, useState } from 'react'
import { useSelector } from 'react-redux'
import { formatDate, formatVND } from '~/common/utils/formatters'
import { selectCurrentUser } from '~/store/user/userSlice'
import { STATUS_CONFIG, canCancelOrder, isDeliveredWithinReturnWindow } from '../../constants/orderStatus'
import CancelOrderDialog from '../shared/CancelOrderDialog'
import CodOtpDialog from '../shared/CodOtpDialog'
import { glassSx } from '~/theme'

function StatusChip({ status }) {
  const cfg = STATUS_CONFIG[status] || {}
  const Icon = cfg.Icon
  return (
    <Chip
      size="small"
      icon={Icon ? <Icon sx={{ fontSize: '13px !important', color: `${cfg.color} !important` }} /> : undefined}
      label={cfg.label || status}
      sx={{
        height: 22, fontSize: '0.7rem', fontWeight: 700,
        bgcolor: cfg.bgColor, color: cfg.color,
        border: '1px solid', borderColor: cfg.borderColor,
        '& .MuiChip-label': { px: 0.9 }
      }}
    />
  )
}

const OrderHistoryCard = forwardRef(function OrderHistoryCard({ order, onViewDetail, cancelMutation, retryVNPayMutation, onOrderUpdated }, ref) {
  const [showCancel, setShowCancel] = useState(false)
  const [showCodRetry, setShowCodRetry] = useState(false)
  const user = useSelector(selectCurrentUser)

  const { _id, order_trackingNumber, order_status, order_products = [], order_checkout = {}, order_payment = {}, createdAt, deliveredAt } = order

  // Group products by shop
  const shopMap = {}
  order_products.forEach(p => {
    const shopName = p.productId?.spu_shopId?.shop_name || 'Shop'
    if (!shopMap[shopName]) shopMap[shopName] = []
    shopMap[shopName].push(p)
  })
  const shopNames = Object.keys(shopMap)
  const firstProduct = order_products[0]
  const remainCount = order_products.length - 1

  // Payment retry conditions
  const isPendingPayment = order_status === 'pending' && order_payment.paymentStatus === 'pending'
  const canRetryVNPay = isPendingPayment && order_payment.method === 'vnpay'
  const canRetryCOD = isPendingPayment && order_payment.method === 'cod'

  const withinReturn = isDeliveredWithinReturnWindow(order_status, deliveredAt)

  const handleCancel = () => {
    cancelMutation.mutate(_id, { onSuccess: () => setShowCancel(false) })
  }

  const handleRetryVNPay = () => {
    retryVNPayMutation?.mutate({
      trackingNumber: order_trackingNumber,
      amount: order_checkout.finalCheckout
    })
  }

  return (
    <>
      <Box ref={ref} sx={{
        borderRadius: 3, mb: 2,
        border: '1px solid', borderColor: 'divider',
        ...glassSx,
        overflow: 'hidden',
        transition: 'box-shadow 0.2s',
        '&:hover': { boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }
      }}>
        {/* Header */}
        <Box sx={{
          display: 'flex', alignItems: 'center', gap: 1,
          px: 2, py: 1.25,
          bgcolor: 'rgba(83,155,255,0.04)',
          borderBottom: '1px solid', borderColor: 'divider'
        }}>
          <StorefrontRoundedIcon sx={{ fontSize: 16, color: 'secondary.main', flexShrink: 0 }} />
          <Typography sx={{ fontSize: '0.82rem', fontWeight: 700, color: 'primary.contrastText', flex: 1 }}>
            {shopNames.length === 1
              ? shopNames[0]
              : shopNames.length > 1
                ? `${shopNames[0]} +${shopNames.length - 1} shop khác`
                : 'Shop'}
          </Typography>
          <Typography sx={{ fontSize: '0.72rem', color: 'rgba(45,45,45,0.45)', mr: 1 }}>
            {formatDate(createdAt)}
          </Typography>
          <StatusChip status={order_status} />
        </Box>

        {/* Product preview */}
        <Box sx={{ px: 2, py: 1.5 }}>
          {firstProduct ? (
            <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5 }}>
              <Box
                component="img"
                src={firstProduct.productId?.spu_thumb}
                alt={firstProduct.productId?.spu_name}
                sx={{
                  width: 64, height: 64, borderRadius: 1.5,
                  objectFit: 'cover', flexShrink: 0,
                  border: '1px solid', borderColor: 'divider',
                  bgcolor: 'rgba(0,0,0,0.03)'
                }}
                onError={(e) => { e.target.style.display = 'none' }}
              />
              <Box sx={{ flex: 1, minWidth: 0 }}>
                <Typography sx={{
                  fontSize: '0.85rem', fontWeight: 600,
                  color: 'primary.contrastText',
                  overflow: 'hidden', textOverflow: 'ellipsis',
                  display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical'
                }}>
                  {firstProduct.productId?.spu_name || 'Sản phẩm'}
                </Typography>
                <Typography sx={{ fontSize: '0.75rem', color: 'rgba(45,45,45,0.5)', mt: 0.25 }}>
                  x{firstProduct.quantity}
                </Typography>
                {remainCount > 0 && (
                  <Typography sx={{ fontSize: '0.73rem', color: 'secondary.main', mt: 0.5, fontWeight: 600 }}>
                    +{remainCount} sản phẩm khác
                  </Typography>
                )}
              </Box>
            </Box>
          ) : (
            <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.45)' }}>
              Không có thông tin sản phẩm
            </Typography>
          )}
        </Box>

        <Divider sx={{ borderColor: 'divider' }} />

        {/* Footer: total + actions */}
        <Box sx={{
          display: 'flex', alignItems: 'center',
          px: 2, py: 1.25, gap: 1,
          flexWrap: 'wrap'
        }}>
          {/* Tracking number */}
          <Typography sx={{ fontSize: '0.7rem', color: 'rgba(45,45,45,0.4)', flex: 1, minWidth: 120 }}>
            #{order_trackingNumber}
          </Typography>

          {/* Total */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mr: 1 }}>
            <Typography sx={{ fontSize: '0.75rem', color: 'rgba(45,45,45,0.5)' }}>Tổng:</Typography>
            <Typography sx={{ fontSize: '0.92rem', fontWeight: 800, color: 'secondary.main' }}>
              {formatVND(order_checkout.finalCheckout)}
            </Typography>
          </Box>

          {/* Action buttons */}
          <Box sx={{ display: 'flex', gap: 0.75, flexWrap: 'wrap' }}>
            {canCancelOrder(order_status) && (
              <Button
                size="small"
                onClick={() => setShowCancel(true)}
                sx={{
                  fontSize: '0.76rem', fontWeight: 600, py: 0.5, px: 1.5,
                  border: '1px solid rgba(220,38,38,0.4)', color: '#dc2626',
                  '&:hover': { bgcolor: 'rgba(220,38,38,0.06)', borderColor: '#dc2626' }
                }}
              >
                Hủy đơn
              </Button>
            )}

            {canRetryVNPay && (
              <Button
                size="small"
                disabled={retryVNPayMutation?.isPending}
                startIcon={retryVNPayMutation?.isPending
                  ? <CircularProgress size={12} sx={{ color: '#fff' }} />
                  : <PaymentRoundedIcon sx={{ fontSize: '14px !important' }} />}
                onClick={handleRetryVNPay}
                sx={{
                  fontSize: '0.76rem', fontWeight: 700, py: 0.5, px: 1.5,
                  bgcolor: '#ffba43ff', color: '#fff',
                  '&:hover': { bgcolor: '#d97706' },
                  '&:disabled': { bgcolor: 'rgba(245,158,11,0.5)', color: '#fff' }
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
                  fontSize: '0.76rem', fontWeight: 600, py: 0.5, px: 1.5,
                  border: '1px solid rgba(52,133,247,0.4)', color: 'secondary.main',
                  bgcolor: 'rgba(52,133,247,0.06)',
                  '&:hover': { bgcolor: 'rgba(52,133,247,0.12)' }
                }}
              >
                Xác nhận OTP
              </Button>
            )}

            {order_status === 'delivered' && withinReturn && (
              <Button
                size="small"
                onClick={() => alert('Tính năng trả hàng đang phát triển')}
                sx={{
                  fontSize: '0.76rem', fontWeight: 600, py: 0.5, px: 1.5,
                  border: '1px solid', borderColor: 'divider',
                  color: 'primary.contrastText',
                  '&:hover': { bgcolor: 'rgba(0,0,0,0.04)' }
                }}
              >
                Trả hàng
              </Button>
            )}

            {order_status === 'delivered' && withinReturn && (
              <Button
                size="small"
                onClick={() => alert('Tính năng đánh giá đang phát triển')}
                sx={{
                  fontSize: '0.76rem', fontWeight: 600, py: 0.5, px: 1.5,
                  border: '1px solid rgba(52,133,247,0.35)', color: 'secondary.main',
                  '&:hover': { bgcolor: 'rgba(52,133,247,0.06)' }
                }}
              >
                Đánh giá
              </Button>
            )}

            {order_status === 'delivered' && !withinReturn && (
              <Button
                size="small"
                startIcon={<ShoppingBagOutlinedIcon sx={{ fontSize: '14px !important' }} />}
                sx={{
                  fontSize: '0.76rem', fontWeight: 600, py: 0.5, px: 1.5,
                  border: '1px solid rgba(52,133,247,0.35)', color: 'secondary.main',
                  '&:hover': { bgcolor: 'rgba(52,133,247,0.06)' }
                }}
              >
                Mua lại
              </Button>
            )}

            <Button
              size="small"
              onClick={() => onViewDetail(order)}
              sx={{
                fontSize: '0.76rem', fontWeight: 600, py: 0.5, px: 1.5,
                bgcolor: 'secondary.main', color: '#fff',
                '&:hover': { bgcolor: '#2b77f5' }
              }}
            >
              Chi tiết
            </Button>
          </Box>
        </Box>
      </Box>

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
        orderId={_id}
        email={user?.user_email}
        onSuccess={() => {
          setShowCodRetry(false)
          onOrderUpdated?.()
        }}
      />
    </>
  )
})

export function OrderHistoryCardSkeleton() {
  return (
    <Box sx={{
      bgcolor: 'primary.main', borderRadius: 3, mb: 2,
      border: '1px solid', borderColor: 'divider', overflow: 'hidden'
    }}>
      <Box sx={{ px: 2, py: 1.25, bgcolor: 'rgba(83,155,255,0.04)', borderBottom: '1px solid', borderColor: 'divider', display: 'flex', gap: 1 }}>
        <Skeleton width={140} height={18} />
        <Box sx={{ flex: 1 }} />
        <Skeleton width={90} height={22} sx={{ borderRadius: 10 }} />
      </Box>
      <Box sx={{ px: 2, py: 1.5, display: 'flex', gap: 1.5 }}>
        <Skeleton variant="rounded" width={64} height={64} />
        <Box sx={{ flex: 1 }}>
          <Skeleton width="70%" height={18} />
          <Skeleton width={40} height={14} sx={{ mt: 0.5 }} />
        </Box>
      </Box>
      <Divider sx={{ borderColor: 'divider' }} />
      <Box sx={{ px: 2, py: 1.25, display: 'flex', alignItems: 'center', justifyContent: 'flex-end', gap: 1 }}>
        <Skeleton width={100} height={18} />
        <Skeleton width={70} height={30} sx={{ borderRadius: 1 }} />
        <Skeleton width={70} height={30} sx={{ borderRadius: 1 }} />
      </Box>
    </Box>
  )
}

export default OrderHistoryCard
