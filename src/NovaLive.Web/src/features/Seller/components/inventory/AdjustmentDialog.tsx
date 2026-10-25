import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import { z } from 'zod'
import type { InventoryItem } from '../../types/contracts'
import { useAdjustInventory } from '../../hooks/useInventory'
import { validateStockAdjustment } from '../../utils/business'

export function AdjustmentDialog({ item, onClose }: { item: InventoryItem; onClose: () => void }) {
  const [quantity, setQuantity] = useState(''); const [changeType, setType] = useState<'Import' | 'ManualAdjust'>('Import'); const [note, setNote] = useState(''); const [error, setError] = useState('')
  const [operationId] = useState(() => crypto.randomUUID())
  const mutation = useAdjustInventory()
  const after = item.qtyOnHand + Number(quantity || 0)
  const invalidStock = after < item.reservedQty
  function submit() {
    const parsed = z.object({ qtyChange: z.number().int('Số lượng phải là số nguyên.').refine(value => value !== 0, 'Số lượng thay đổi phải khác 0.'), note: z.string().trim().min(3, 'Ghi chú cần ít nhất 3 ký tự.') }).safeParse({ qtyChange: Number(quantity), note })
    if (!parsed.success) { setError(parsed.error.issues.map(issue => issue.message).join(' ')); return }
    const validation = validateStockAdjustment(item.qtyOnHand, item.reservedQty, parsed.data.qtyChange, changeType)
    if (validation) { setError(validation); return }
    mutation.mutate({ skuId: item.skuId, ...parsed.data, changeType, operationId }, { onSuccess: onClose })
  }
  return <Dialog open fullWidth maxWidth='sm' onClose={mutation.isPending ? undefined : onClose} aria-labelledby='adjust-title'><DialogTitle id='adjust-title'>Điều chỉnh tồn · {item.skuCode}</DialogTitle><DialogContent><Stack spacing={2} sx={{ pt: 1 }}>
    <Typography>{item.spuName}</Typography><Alert severity='info'>Tồn vật lý: {item.qtyOnHand} · Giữ chỗ: {item.reservedQty} · Khả dụng: {item.qtyOnHand - item.reservedQty}</Alert>
    {error && <Alert severity='error'>{error}</Alert>}
    <TextField select label='Loại điều chỉnh' value={changeType} onChange={event => setType(event.target.value as 'Import' | 'ManualAdjust')}><MenuItem value='Import'>Nhập hàng (Import)</MenuItem><MenuItem value='ManualAdjust'>Điều chỉnh thủ công (ManualAdjust)</MenuItem></TextField>
    <TextField autoFocus type='number' required label='Số lượng thay đổi (+ / −)' value={quantity} onChange={event => setQuantity(event.target.value)} helperText='Nhập số âm để giảm, số dương để tăng tồn.' />
    <TextField label='Ghi chú bắt buộc' required multiline minRows={2} value={note} onChange={event => setNote(event.target.value)} />
    <Alert severity={invalidStock ? 'error' : 'info'}>Sau điều chỉnh: tồn vật lý {after}, khả dụng {after - item.reservedQty}.{invalidStock && ' Tồn vật lý không được nhỏ hơn giữ chỗ.'}</Alert>
  </Stack></DialogContent><DialogActions><Button disabled={mutation.isPending} onClick={onClose}>Quay lại</Button><Button variant='contained' color='secondary' disabled={mutation.isPending || invalidStock || quantity === ''} onClick={submit}>{mutation.isPending ? 'Đang ghi sổ…' : 'Xác nhận điều chỉnh'}</Button></DialogActions></Dialog>
}
