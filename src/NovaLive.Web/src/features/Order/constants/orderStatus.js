import HourglassEmptyRoundedIcon from '@mui/icons-material/HourglassEmptyRounded'
import AutorenewRoundedIcon from '@mui/icons-material/AutorenewRounded'
import CheckCircleOutlineRoundedIcon from '@mui/icons-material/CheckCircleOutlineRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import CancelRoundedIcon from '@mui/icons-material/CancelRounded'

export const ORDER_STATUS = {
  PENDING: 'pending',
  PROCESSING: 'processing',
  CONFIRMED: 'confirmed',
  SHIPPED: 'shipped',
  DELIVERED: 'delivered',
  CANCELLED: 'cancelled'
}

export const STATUS_CONFIG = {
  pending: {
    label: 'Chờ xác nhận',
    color: '#f59e0b',
    bgColor: 'rgba(245,158,11,0.1)',
    borderColor: 'rgba(245,158,11,0.25)',
    Icon: HourglassEmptyRoundedIcon
  },
  processing: {
    label: 'Đang xử lý',
    color: '#3485f7',
    bgColor: 'rgba(52,133,247,0.1)',
    borderColor: 'rgba(52,133,247,0.25)',
    Icon: AutorenewRoundedIcon
  },
  confirmed: {
    label: 'Đã xác nhận',
    color: '#3485f7',
    bgColor: 'rgba(52,133,247,0.1)',
    borderColor: 'rgba(52,133,247,0.25)',
    Icon: CheckCircleOutlineRoundedIcon
  },
  shipped: {
    label: 'Đang giao',
    color: '#8b5cf6',
    bgColor: 'rgba(139,92,246,0.1)',
    borderColor: 'rgba(139,92,246,0.25)',
    Icon: LocalShippingRoundedIcon
  },
  delivered: {
    label: 'Đã giao',
    color: '#16a34a',
    bgColor: 'rgba(22,163,74,0.1)',
    borderColor: 'rgba(22,163,74,0.25)',
    Icon: CheckCircleRoundedIcon
  },
  cancelled: {
    label: 'Đã hủy',
    color: '#dc2626',
    bgColor: 'rgba(220,38,38,0.1)',
    borderColor: 'rgba(220,38,38,0.25)',
    Icon: CancelRoundedIcon
  }
}

// Tabs cho OrderHistoryPage
export const ORDER_TABS = [
  { label: 'Chờ xác nhận', status: 'pending' },
  { label: 'Đang xử lý', status: 'processing' },
  { label: 'Đang giao', status: 'shipped' },
  { label: 'Đã giao', status: 'delivered' },
  { label: 'Đã hủy', status: 'cancelled' },
  { label: 'Tất cả', statuses: 'all' }
]

// Timeline các bước theo thứ tự logic của đơn hàng
export const STATUS_TIMELINE_STEPS = [
  { status: 'pending', label: 'Đặt hàng thành công' },
  { status: 'processing', label: 'Đang xử lý' },
  { status: 'confirmed', label: 'Đã xác nhận' },
  { status: 'shipped', label: 'Đang giao hàng' },
  { status: 'delivered', label: 'Giao hàng thành công' }
]

// Số ngày cho phép trả hàng sau khi đã giao
export const RETURN_WINDOW_DAYS = 10

export const canCancelOrder = (status) => status === ORDER_STATUS.PENDING

export const isDeliveredWithinReturnWindow = (status, deliveredAt) => {
  if (status !== ORDER_STATUS.DELIVERED) return false
  if (!deliveredAt) return false
  const daysDiff = (Date.now() - new Date(deliveredAt).getTime()) / (1000 * 60 * 60 * 24)
  return daysDiff <= RETURN_WINDOW_DAYS
}

export const getPaymentMethodLabel = (method) => {
  const map = { cod: 'COD (Tiền mặt)', vnpay: 'VNPay', momo: 'MoMo', stripe: 'Stripe' }
  return map[method] || method
}

export const getPaymentStatusLabel = (status) => {
  const map = { pending: 'Chờ thanh toán', paid: 'Đã thanh toán', failed: 'Thanh toán thất bại' }
  return map[status] || status
}

export const getPaymentStatusColor = (status) => {
  const map = { pending: '#f59e0b', paid: '#16a34a', failed: '#dc2626' }
  return map[status] || '#666'
}
