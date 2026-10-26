import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, TextField } from '@mui/material'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { OrderDetail, ShippingProvider } from '../../types/contracts'
import { useWarehouses } from '../../hooks/useShop'
import { useCancelOrder, useConfirmOrder, useCreateShipment } from '../../hooks/useOrders'
import { QueryState } from '../shared/QueryState'
import { ConfirmDialog } from '../shared/ConfirmDialog'

export function OrderActionDialogs({ order, action, onClose }: { order: OrderDetail; action: 'confirm' | 'cancel' | 'ship'; onClose: () => void }) {
  const warehouses = useWarehouses(); const confirm = useConfirmOrder(); const cancel = useCancelOrder(); const shipment = useCreateShipment()
  const [warehouse, setWarehouse] = useState(order.warehouseAddressId || '')
  const [reason, setReason] = useState(''); const [provider, setProvider] = useState<ShippingProvider>('GHN'); const [serviceCode, setServiceCode] = useState('')
  const selected = warehouse || warehouses.data?.find(item => item.isPrimary)?.id || ''
  const pending = confirm.isPending || cancel.isPending || shipment.isPending
  const codeValid = provider === 'GHN' ? /^\d+$/.test(serviceCode) && Number(serviceCode) > 0 : Boolean(serviceCode.trim())
  if (action === 'cancel') return <ConfirmDialog open title={`Hủy đơn ${order.code}?`} description='Đơn hàng sẽ bị hủy. Tồn giữ chỗ và thanh toán được backend xử lý theo trạng thái hiện tại. Vui lòng nhập lý do rõ ràng.' danger pending={pending} disabled={reason.trim().length < 3 || reason.length > 500} onClose={onClose} confirmText='Xác nhận hủy đơn' onConfirm={() => cancel.mutate({ id: order.id, reason: reason.trim() }, { onSuccess: onClose })}><TextField fullWidth autoFocus required multiline minRows={3} label='Lý do hủy đơn' value={reason} onChange={event => setReason(event.target.value)} helperText={`${reason.length}/500 ký tự · tối thiểu 3 ký tự`} /></ConfirmDialog>
  return <Dialog open onClose={pending ? undefined : onClose} fullWidth maxWidth='sm' aria-labelledby='order-action-title'><DialogTitle id='order-action-title'>{action === 'confirm' ? 'Chọn kho & Xác nhận chuẩn bị đơn' : 'Tạo vận đơn'} · {order.code}</DialogTitle><DialogContent><Stack spacing={2} sx={{ pt: 1 }}>
    <QueryState loading={warehouses.isLoading} error={warehouses.error} empty={!warehouses.data?.length} onRetry={warehouses.refetch} emptyText='Chưa có kho lấy hàng.' emptyAction={<Button component={Link} to='/seller/profile'>Thêm kho hàng</Button>}><TextField select fullWidth label='Kho lấy hàng' value={selected} onChange={event => setWarehouse(event.target.value)}>{warehouses.data?.map(item => <MenuItem key={item.id} value={item.id}>{item.warehouseName}{item.isPrimary ? ' · Mặc định' : ''}</MenuItem>)}</TextField></QueryState>
    {action === 'ship' && <><TextField select label='Đơn vị vận chuyển' value={provider} onChange={event => { setProvider(event.target.value as ShippingProvider); setServiceCode('') }}>{(['GHN', 'GHTK', 'ViettelPost'] as const).map(value => <MenuItem key={value} value={value}>{value}</MenuItem>)}</TextField><TextField label='Mã dịch vụ vận chuyển' required value={serviceCode} onChange={event => setServiceCode(event.target.value)} helperText={provider === 'GHN' ? 'Nhập ServiceTypeId nguyên dương do GHN cung cấp.' : 'Nhập mã dịch vụ trong hợp đồng với đơn vị vận chuyển.'} /><Alert severity='info'>Chưa có API tra cứu dịch vụ vận chuyển. Shop cần nhập đúng mã từ đơn vị vận chuyển; phí thực tế do backend xác nhận khi tạo vận đơn.</Alert></>}
  </Stack></DialogContent><DialogActions><Button disabled={pending} onClick={onClose}>Quay lại</Button><Button variant='contained' color='secondary' disabled={pending || !selected || (action === 'ship' && !codeValid)} onClick={() => action === 'confirm' ? confirm.mutate({ id: order.id, warehouseAddressId: selected }, { onSuccess: onClose }) : shipment.mutate({ subOrderId: order.id, provider, serviceCode: serviceCode.trim(), pickupAddressId: selected }, { onSuccess: onClose })}>{pending ? 'Đang xử lý…' : action === 'confirm' ? 'Xác nhận kho lấy hàng' : 'Tạo vận đơn'}</Button></DialogActions></Dialog>
}
