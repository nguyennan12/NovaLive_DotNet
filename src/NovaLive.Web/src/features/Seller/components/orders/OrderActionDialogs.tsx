import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, TextField } from '@mui/material'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { OrderDetail, ShippingProvider } from '../../types/contracts'
import { useWarehouses } from '../../hooks/useShop'
import { useCancelOrder, useConfirmOrder, useCreateShipment, useSimulateShipmentDelivered } from '../../hooks/useOrders'
import { QueryState } from '../shared/QueryState'
import { ConfirmDialog } from '../shared/ConfirmDialog'
import { sellerBtnStyles } from '../../theme/sellerBtnStyles'

export function OrderActionDialogs({
  order,
  action,
  onClose
}: {
  order: OrderDetail
  action: 'confirm' | 'cancel' | 'ship' | 'deliver'
  onClose: () => void
}) {
  const warehouses = useWarehouses()
  const confirm = useConfirmOrder()
  const cancel = useCancelOrder()
  const shipment = useCreateShipment()
  const deliver = useSimulateShipmentDelivered()
  const [warehouse, setWarehouse] = useState(order.warehouseAddressId || '')
  const [reason, setReason] = useState('')
  const provider: ShippingProvider = 'GHN'
  const serviceCode = '2'
  const selected = warehouse || warehouses.data?.find(item => item.isPrimary)?.id || ''
  const pending = confirm.isPending || cancel.isPending || shipment.isPending || deliver.isPending

  if (action === 'deliver') {
    return (
      <ConfirmDialog
        open
        title={`${order.paymentMethod === 'COD' ? 'Xác nhận giao & đối soát COD' : 'Xác nhận đã giao'} · ${order.code}`}
        description={
          order.paymentMethod === 'COD'
            ? 'Chỉ xác nhận nếu GHN đã giao hàng và thu COD thành công. Hệ thống sẽ ghi doanh thu COD vào ví Shop một lần; thao tác này không thay thế bằng chứng giao hàng từ GHN.'
            : 'Thao tác xác nhận trạng thái đã giao. Đơn thanh toán online đã được ghi có vào ví khi webhook thanh toán thành công.'
        }
        pending={pending}
        onClose={onClose}
        confirmText={order.paymentMethod === 'COD' ? 'Xác nhận đã thu COD' : 'Xác nhận đã giao'}
        onConfirm={() => deliver.mutate(order.id, { onSuccess: onClose })}
      />
    )
  }

  if (action === 'cancel') {
    return (
      <ConfirmDialog
        open
        title={`Hủy đơn ${order.code}?`}
        description="Đơn hàng sẽ bị hủy. Tồn giữ chỗ và thanh toán được backend xử lý theo trạng thái hiện tại. Vui lòng nhập lý do rõ ràng."
        danger
        pending={pending}
        disabled={reason.trim().length < 3 || reason.length > 500}
        onClose={onClose}
        confirmText="Xác nhận hủy đơn"
        onConfirm={() => cancel.mutate({ id: order.id, reason: reason.trim() }, { onSuccess: onClose })}
      >
        <TextField
          fullWidth
          autoFocus
          required
          multiline
          minRows={3}
          label="Lý do hủy đơn"
          value={reason}
          onChange={event => setReason(event.target.value)}
          helperText={`${reason.length}/500 ký tự · tối thiểu 3 ký tự`}
        />
      </ConfirmDialog>
    )
  }

  return (
    <Dialog
      open
      onClose={pending ? undefined : onClose}
      fullWidth
      maxWidth="sm"
      PaperProps={{ sx: { borderRadius: '12px' } }}
      aria-labelledby="order-action-title"
    >
      <DialogTitle id="order-action-title" sx={{ fontWeight: 700, fontSize: '1.1rem', pt: 2.5, px: 3 }}>
        {action === 'confirm' ? 'Chọn kho & Xác nhận chuẩn bị đơn' : 'Tạo vận đơn'} · {order.code}
      </DialogTitle>
      <DialogContent sx={{ px: 3 }}>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <QueryState
            loading={warehouses.isLoading}
            error={warehouses.error}
            empty={!warehouses.data?.length}
            onRetry={warehouses.refetch}
            emptyText="Chưa có kho lấy hàng."
            emptyAction={<Button component={Link} to="/seller/profile">Thêm kho hàng</Button>}
          >
            <TextField
              select
              fullWidth
              label="Kho lấy hàng"
              value={selected}
              onChange={event => setWarehouse(event.target.value)}
            >
              {warehouses.data?.map(item => (
                <MenuItem key={item.id} value={item.id}>
                  {item.warehouseName}
                  {item.isPrimary ? ' · Mặc định' : ''}
                </MenuItem>
              ))}
            </TextField>
          </QueryState>
          {action === 'ship' && (
            <Alert severity="info" sx={{ borderRadius: '8px' }}>
              Đã chọn GHN cho kiện nhẹ dưới 20 kg. Hệ thống kiểm tra khối lượng trước khi tạo vận đơn; phí thực tế do GHN trả về.
            </Alert>
          )}
        </Stack>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2.5, pt: 1, gap: 1 }}>
        <Button
          disabled={pending}
          onClick={onClose}
          sx={{ textTransform: 'none', fontWeight: 600, color: '#64748b', borderRadius: '8px' }}
        >
          Quay lại
        </Button>
        <Button
          variant="contained"
          disabled={pending || !selected}
          onClick={() =>
            action === 'confirm'
              ? confirm.mutate({ id: order.id, warehouseAddressId: selected }, { onSuccess: onClose })
              : shipment.mutate({ subOrderId: order.id, provider, serviceCode, pickupAddressId: selected }, { onSuccess: onClose })
          }
          sx={{
            ...sellerBtnStyles.primary,
            px: 2.5,
            py: 0.8,
            fontSize: '0.875rem'
          }}
        >
          {pending ? 'Đang xử lý…' : action === 'confirm' ? 'Xác nhận kho lấy hàng' : 'Tạo vận đơn GHN'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
