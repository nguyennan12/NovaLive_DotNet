import { Chip } from '@mui/material'

export const statusLabels: Record<string, string> = {
  Pending: 'Chờ xử lý', Active: 'Đang hoạt động', Inactive: 'Đã tắt', Suspended: 'Tạm đình chỉ', Banned: 'Bị khóa', Closed: 'Đã đóng', Draft: 'Bản nháp',
  PendingPayment: 'Chờ thanh toán', Confirmed: 'Đã xác nhận', Processing: 'Đang chuẩn bị', Shipping: 'Đang giao', Delivered: 'Đã giao', Completed: 'Hoàn thành', Cancelled: 'Đã hủy', ReturnRequested: 'Yêu cầu trả hàng', Returned: 'Đã trả hàng', Refunded: 'Đã hoàn tiền',
  SellerApproved: 'Shop đồng ý', SellerRejected: 'Shop từ chối', AdminDispute: 'Chờ phân xử', AdminApproved: 'Được duyệt sau phân xử', AdminRejected: 'Bị từ chối sau phân xử', Approved: 'Đã duyệt', Rejected: 'Bị từ chối', Ended: 'Đã kết thúc', Scheduled: 'Sắp diễn ra', Expired: 'Hết hạn', Live: 'Đang phát',
  OrderRevenue: 'Doanh thu đơn hàng', OrderRefund: 'Hoàn tiền đơn hàng', CodCommissionDeduct: 'Hoa hồng COD', PayoutLock: 'Khóa tiền rút', PayoutWithdrawal: 'Đã chuyển tiền rút', PayoutFailedUnlock: 'Hoàn tiền rút thất bại', PenaltyDeduct: 'Khấu trừ phạt', ManualAdjustment: 'Điều chỉnh thủ công',
  Import: 'Nhập hàng', ManualAdjust: 'Điều chỉnh thủ công', SaleConfirmed: 'Chốt bán', ReturnIn: 'Nhận hàng hoàn', ReserveAdd: 'Giữ chỗ', ReserveRelease: 'Giải phóng giữ chỗ', SaleCancelled: 'Hủy bán', ReadyToPick: 'Chờ lấy hàng'
}
const good = new Set(['Active', 'Confirmed', 'Completed', 'Delivered', 'SellerApproved', 'AdminApproved', 'Approved'])
const bad = new Set(['Banned', 'Suspended', 'Cancelled', 'SellerRejected', 'AdminRejected', 'Rejected'])
export function StatusBadge({ status }: { status: string }) {
  const color = good.has(status) ? 'success' : bad.has(status) ? 'error' : status === 'Pending' || status === 'PendingPayment' || status === 'AdminDispute' ? 'warning' : 'default'
  return <Chip size='small' variant='outlined' color={color} label={statusLabels[status] || status} sx={{ fontWeight: 600, fontSize: '0.72rem' }} />
}
