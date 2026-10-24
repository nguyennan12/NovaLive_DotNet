import { Alert, Avatar, Box, Button, Grid, Stack, Typography } from '@mui/material'
import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { useOrder, usePrintLabel } from '../hooks/useOrders'
import { useShopPermissions } from '../hooks/useShopPermissions'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { QueryState } from '../components/shared/QueryState'
import { DataTable } from '../components/shared/DataTable'
import { StatusBadge } from '../components/shared/StatusBadge'
import { DateText } from '../components/shared/DateText'
import { MoneyText } from '../components/shared/MoneyText'
import { OrderActionDialogs } from '../components/orders/OrderActionDialogs'
import { TrackingPanel } from '../components/orders/TrackingPanel'
import { snapshot } from '../utils/format'

export default function OrderDetailPage() {
  const { id } = useParams(); const query = useOrder(id); const order = query.data
  const [action, setAction] = useState<'confirm' | 'cancel' | 'ship' | null>(null); const [tracking, setTracking] = useState(false)
  const { canOperate } = useShopPermissions(); const print = usePrintLabel()
  const shipped = order && ['Shipping', 'Delivered', 'Completed', 'ReturnRequested', 'Returned', 'Refunded'].includes(order.status)
  const paid = order && (order.paymentMethod === 'COD' || ['Paid', 'Success'].includes(order.paymentStatus))
  return <><PageHeader title={order ? `Đơn hàng ${order.code}` : 'Chi tiết đơn hàng'} action={<Button component={Link} to='/seller/orders'>Về danh sách</Button>} /><QueryState loading={query.isLoading} error={query.error} onRetry={query.refetch}>{order && <Stack spacing={2.5}>
    <SellerCard><Stack direction='row' spacing={1} sx={{ flexWrap: 'wrap', gap: 1, alignItems: 'center' }}><StatusBadge status={order.status} /><Typography>{order.paymentMethod} · {order.orderSource === 'Online' ? 'Trực tuyến' : order.orderSource}</Typography><DateText value={order.createdAt} />{order.status === 'Confirmed' && <Button variant='outlined' disabled={!canOperate} onClick={() => setAction('confirm')}>{order.warehouseAddressId ? 'Đổi kho lấy hàng' : 'Xác nhận & Chọn kho'}</Button>}{order.status === 'Confirmed' && paid && <Button variant='contained' color='secondary' disabled={!canOperate || !order.warehouseAddressId} onClick={() => setAction('ship')}>Giao hàng</Button>}{['PendingPayment', 'Confirmed', 'Processing'].includes(order.status) && <Button color='error' onClick={() => setAction('cancel')}>Hủy đơn</Button>}{shipped && <><Button disabled={print.isPending} onClick={() => void print.print(order.id)}>{print.isPending ? 'Đang tải PDF…' : 'In phiếu giao A6'}</Button><Button onClick={() => setTracking(!tracking)}>{tracking ? 'Ẩn hành trình' : 'Xem hành trình'}</Button></>}</Stack>{order.status === 'PendingPayment' && <Alert severity='info' sx={{ mt: 2 }}>Đơn online chưa thanh toán. Chưa thể chọn kho hoặc giao hàng.</Alert>}{order.cancelReason && <Alert severity='warning' sx={{ mt: 2 }}>Lý do hủy: {order.cancelReason}</Alert>}</SellerCard>
    <SellerCard title='Sản phẩm trong đơn'><DataTable rows={order.items} rowKey={row => row.id} loading={false} error={null} onRetry={query.refetch} columns={[
      { key: 'product', label: 'Sản phẩm (bản chụp lúc mua)', render: row => { const item = snapshot(row.skuSnapshotJson); return <Stack direction='row' spacing={1}><Avatar src={item.image} variant='rounded' alt={item.name}>{item.name[0]}</Avatar><Box><Typography sx={{ fontWeight: 600 }}>{item.name}</Typography><Typography variant='caption'>{item.variant}</Typography></Box></Stack> } },
      { key: 'qty', label: 'Số lượng', render: row => row.quantity }, { key: 'unit', label: 'Đơn giá', render: row => <MoneyText value={row.unitPrice} /> }, { key: 'total', label: 'Thành tiền', render: row => <MoneyText value={row.lineTotal} /> }
    ]} /></SellerCard>
    <Grid container spacing={2.5}><Grid size={{ xs: 12, md: 6 }}><SellerCard title='Địa chỉ nhận hàng'><Stack spacing={1}><Typography sx={{ fontWeight: 700 }}>{order.recipient.recipientName} · {order.recipient.phone}</Typography><Typography>{[order.recipient.detailAddress, order.recipient.wardName, order.recipient.districtName, order.recipient.provinceName].join(', ')}</Typography></Stack></SellerCard></Grid><Grid size={{ xs: 12, md: 6 }}><SellerCard title='Chi tiết thanh toán'><Stack spacing={1}>{[
      ['Tiền hàng', order.itemAmount], ['Phí vận chuyển', order.shippingFee], ['Voucher Shop', -order.shopDiscountAmount], ['Voucher sàn', -order.platformDiscountAmount], ['Giảm phí vận chuyển', -order.shippingDiscountAmount], ['Tổng thanh toán', order.subTotal]
    ].map(([label, value]) => <Box key={label} sx={{ display: 'flex', justifyContent: 'space-between', gap: 2 }}><Typography>{label}</Typography><MoneyText value={Number(value)} /></Box>)}<Alert severity='info'>Tiền Shop thực nhận chưa có trong API chi tiết đơn. Doanh thu được ghi nhận ở sổ ví khi đủ điều kiện thanh toán.</Alert></Stack></SellerCard></Grid></Grid>
    <SellerCard title='Lịch sử trạng thái'><Stack spacing={2}>{order.timeline.map((item, index) => <Box key={index} sx={{ borderLeft: 2, borderColor: 'divider', pl: 2 }}><DateText value={item.changedAt} /><Typography component='div' sx={{ my: 0.5 }}><StatusBadge status={item.toStatus} /> · {({ Seller: 'Shop', Buyer: 'Người mua', System: 'Hệ thống', Admin: 'Quản trị viên' } as Record<string, string>)[item.changedByRole || ''] || item.changedByRole}</Typography><Typography color='text.secondary'>{item.note}</Typography></Box>)}</Stack></SellerCard>
    {tracking && <TrackingPanel id={order.id} />}{action && <OrderActionDialogs key={action} order={order} action={action} onClose={() => setAction(null)} />}
  </Stack>}</QueryState></>
}
