import { Alert, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import type { OrderReturn } from '../../types/contracts'
import { useWarehouses } from '../../hooks/useShop'
import { useDecideReturn } from '../../hooks/useReturns'
import { StatusBadge } from '../shared/StatusBadge'
import { DateText } from '../shared/DateText'
import { MoneyText } from '../shared/MoneyText'
import { MediaGallery } from '../shared/MediaGallery'
import { MediaUpload } from '../shared/MediaUpload'
import { ConfirmDialog } from '../shared/ConfirmDialog'
import { DataTable } from '../shared/DataTable'
import { ReturnCountdown, useReturnDeadline } from './ReturnCountdown'
import { returnReasons } from '../../utils/returns'
import { Link } from 'react-router-dom'

export function ReturnDetailDialog({ item, onClose }: { item: OrderReturn; onClose: () => void }) {
  const warehouses = useWarehouses(); const mutation = useDecideReturn(); const remaining = useReturnDeadline(item.createdAt)
  const [action, setAction] = useState<'approve' | 'reject' | 'received' | null>(null)
  const [note, setNote] = useState(''); const [evidence, setEvidence] = useState<string[]>([])
  const address = item.returnAddress || warehouses.data?.find(warehouse => warehouse.isReturn)
  const pending = item.status === 'Pending' && remaining > 0
  const receiving = ['SellerApproved', 'AdminApproved'].includes(item.status)
  function confirm() {
    if (action === 'approve') mutation.mutate({ id: item.id, decision: { action, note: note.trim() } }, { onSuccess: onClose })
    if (action === 'reject') mutation.mutate({ id: item.id, decision: { action, reason: note.trim(), evidenceUrls: evidence } }, { onSuccess: onClose })
    if (action === 'received') mutation.mutate({ id: item.id, decision: { action } }, { onSuccess: onClose })
  }
  return <><Dialog open fullWidth maxWidth='md' onClose={mutation.isPending ? undefined : onClose} aria-labelledby='return-detail-title'><DialogTitle id='return-detail-title'>Trả hàng · {item.subOrderCode}</DialogTitle><DialogContent><Stack spacing={2.5}>
    <Stack direction='row' spacing={2}><StatusBadge status={item.status} /><DateText value={item.createdAt} /></Stack>
    {item.status === 'Pending' && <><ReturnCountdown createdAt={item.createdAt} /><Alert severity={remaining < 86_400_000 ? 'warning' : 'info'}>Shop có 3 ngày kể từ khi tạo yêu cầu để phản hồi. Quá hạn sẽ được hệ thống chuyển tới bộ phận phân xử.</Alert></>}
    {item.status === 'AdminDispute' && <Alert severity='info'>Đang chờ Admin phân xử. Shop chỉ có thể xem thông tin yêu cầu này.</Alert>}
    <Typography><strong>Lý do:</strong> {returnReasons[item.reason]}</Typography>
    <Typography><strong>Số tiền yêu cầu hoàn:</strong> <MoneyText value={item.requestedRefundAmount} /></Typography>
    {item.approvedRefundAmount !== null && <Typography>Số tiền hoàn được duyệt: <MoneyText value={item.approvedRefundAmount} /></Typography>}
    <DataTable rows={item.items} rowKey={row => row.orderItemId} loading={false} error={null} onRetry={() => undefined} columns={[
      { key: 'name', label: 'Sản phẩm', render: row => row.spuName }, { key: 'sku', label: 'SKU', render: row => row.skuCode }, { key: 'qty', label: 'Số lượng hoàn', render: row => row.quantity }, { key: 'refund', label: 'Đơn giá hoàn', render: row => <MoneyText value={row.refundPrice} /> }
    ]} />
    <Box><Typography component='h3' sx={{ fontWeight: 700, mb: 1 }}>Bằng chứng của người mua</Typography>{item.evidenceUrls.length ? <MediaGallery urls={item.evidenceUrls} /> : <Typography color='text.secondary'>Không có tệp bằng chứng.</Typography>}</Box>
    {item.sellerResponse && <Alert severity='info'>Phản hồi của Shop: {item.sellerResponse}</Alert>}{Boolean(item.sellerEvidenceUrls?.length) && <MediaGallery urls={item.sellerEvidenceUrls || []} />}
    <Box><Typography component='h3' sx={{ fontWeight: 700 }}>Kho nhận hàng hoàn</Typography>{address ? <Typography>{address.warehouseName} · {address.contactName} · {address.contactPhone}<br />{[address.detailAddress, address.wardName, address.districtName, address.provinceName].join(', ')}</Typography> : <Alert severity='warning' action={<Button component={Link} to='/seller/profile'>Cấu hình kho</Button>}>Cần có kho nhận hàng hoàn trước khi đồng ý.</Alert>}</Box>
  </Stack></DialogContent><DialogActions><Button disabled={mutation.isPending} onClick={onClose}>Đóng</Button>{pending && <><Button color='error' onClick={() => { setNote(''); setAction('reject') }}>Từ chối</Button><Button color='secondary' variant='contained' disabled={!address || warehouses.isLoading} onClick={() => { setNote(''); setAction('approve') }}>Đồng ý hoàn hàng</Button></>}{receiving && <Button color='secondary' variant='contained' onClick={() => setAction('received')}>Xác nhận đã nhận hàng hoàn</Button>}</DialogActions></Dialog>
    <ConfirmDialog open={Boolean(action)} title={action === 'approve' ? 'Đồng ý yêu cầu trả hàng?' : action === 'reject' ? 'Từ chối yêu cầu trả hàng?' : 'Xác nhận đã nhận hàng hoàn?'} description={action === 'received' ? 'Tiền hoàn sẽ bị trừ từ ví shop. Chỉ xác nhận khi đã nhận và kiểm tra đủ hàng hoàn.' : action === 'approve' ? `Người mua sẽ gửi hàng về ${address?.warehouseName || 'kho nhận hàng hoàn'} đã hiển thị ở trên.` : 'Nhập lý do và tải ảnh/video đối chứng. Người mua có thể yêu cầu phân xử.'} danger={action !== 'approve'} pending={mutation.isPending} disabled={action === 'reject' ? note.trim().length < 3 || !evidence.length || remaining <= 0 : action === 'approve' ? !address || remaining <= 0 : false} onClose={() => setAction(null)} onConfirm={confirm}>
      {action !== 'received' && <Stack spacing={2}><TextField label={action === 'approve' ? 'Ghi chú (không bắt buộc)' : 'Lý do từ chối (bắt buộc)'} fullWidth multiline minRows={3} value={note} onChange={event => setNote(event.target.value)} />{action === 'reject' && <MediaUpload label='Ảnh / Video đối chứng bắt buộc' multiple video urls={evidence} onChange={setEvidence} disabled={mutation.isPending} />}</Stack>}
    </ConfirmDialog>
  </>
}
