import { Alert, Button, Checkbox, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, Grid, MenuItem, Stack, TextField } from '@mui/material'
import { useState } from 'react'
import type { CreateDiscountRequest, Discount } from '../../types/contracts'
import { useCreateDiscount, useUpdateDiscount } from '../../hooks/useMarketing'
import { discountSchema } from '../../utils/marketing'
import { inputToUtc, localInput } from '../../utils/format'
import { errorMessage } from '../../utils/errors'

export function DiscountDialog({ initial, onClose }: { initial: Discount | null; onClose: () => void }) {
  const [form, setForm] = useState<CreateDiscountRequest & { isActive: boolean }>(() => initial || { code: '', name: '', discountType: 'PercentCart', discountValue: 10, minOrderAmount: 0, maxDiscountAmount: 50_000, maxUses: null, perUserLimit: 1, validFrom: new Date().toISOString(), validTo: new Date(Date.now() + 7 * 86_400_000).toISOString(), isPublic: true, appliesTo: 'AllProducts', targetIds: null, isActive: true })
  const [error, setError] = useState('')
  const create = useCreateDiscount(); const update = useUpdateDiscount(); const pending = create.isPending || update.isPending
  async function submit() {
    const { isActive, ...payload } = form
    const parsed = discountSchema.safeParse(payload)
    if (!parsed.success) { setError(parsed.error.issues.map(issue => issue.message).join(' ')); return }
    if (initial && form.maxUses !== null && form.maxUses < initial.usedCount) { setError('Tổng lượt dùng không được nhỏ hơn số lượt đã sử dụng.'); return }
    try {
      if (initial) await update.mutateAsync({ id: initial.id, data: { name: form.name.trim(), validTo: form.validTo, maxUses: form.maxUses, isActive } })
      else await create.mutateAsync(parsed.data)
      onClose()
    } catch (error) { setError(errorMessage(error)) }
  }
  return <Dialog open fullWidth maxWidth='md' onClose={pending ? undefined : onClose} aria-labelledby='discount-title'><DialogTitle id='discount-title'>{initial ? 'Sửa voucher' : 'Tạo voucher của Shop'}</DialogTitle><DialogContent><Stack spacing={2} sx={{ pt: 1 }}>
    {error && <Alert severity='error'>{error}</Alert>}
    {initial && <Alert severity='info'>{initial.usedCount > 0 ? `Voucher đã dùng ${initial.usedCount} lần. Mã và giá trị giảm được khóa.` : 'Khi sửa, backend chỉ cho đổi tên, tổng lượt dùng, ngày kết thúc và kích hoạt.'} Để ngừng voucher, hãy tắt kích hoạt.</Alert>}
    <Grid container spacing={2}><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label='Mã voucher' value={form.code} disabled={Boolean(initial)} onChange={event => setForm({ ...form, code: event.target.value.toUpperCase() })} /></Grid><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label='Tên voucher' value={form.name} onChange={event => setForm({ ...form, name: event.target.value })} /></Grid></Grid>
    <Grid container spacing={2}><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth select label='Loại giảm' disabled={Boolean(initial)} value={form.discountType} onChange={event => setForm({ ...form, discountType: event.target.value as 'PercentCart' | 'FixedCart' })}><MenuItem value='PercentCart'>Giảm theo phần trăm</MenuItem><MenuItem value='FixedCart'>Giảm số tiền cố định</MenuItem></TextField></Grid><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth type='number' label={form.discountType === 'PercentCart' ? 'Mức giảm (%)' : 'Mức giảm (₫)'} disabled={Boolean(initial)} value={form.discountValue} onChange={event => setForm({ ...form, discountValue: Number(event.target.value) })} /></Grid></Grid>
    <Grid container spacing={2}><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth type='number' label='Đơn tối thiểu (₫)' disabled={Boolean(initial)} value={form.minOrderAmount} onChange={event => setForm({ ...form, minOrderAmount: Number(event.target.value) })} /></Grid><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth type='number' label='Giảm tối đa (₫)' required={form.discountType === 'PercentCart'} disabled={Boolean(initial)} value={form.maxDiscountAmount ?? ''} onChange={event => setForm({ ...form, maxDiscountAmount: event.target.value === '' ? null : Number(event.target.value) })} /></Grid></Grid>
    <Grid container spacing={2}><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth type='number' label='Tổng lượt dùng tối đa' helperText='Để trống nếu không giới hạn.' value={form.maxUses ?? ''} onChange={event => setForm({ ...form, maxUses: event.target.value === '' ? null : Number(event.target.value) })} /></Grid><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth type='number' label='Số lượt mỗi người' disabled={Boolean(initial)} value={form.perUserLimit} onChange={event => setForm({ ...form, perUserLimit: Number(event.target.value) })} /></Grid></Grid>
    <Grid container spacing={2}><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth type='datetime-local' label='Bắt đầu (giờ Việt Nam)' slotProps={{ inputLabel: { shrink: true } }} disabled={Boolean(initial)} value={localInput(form.validFrom)} onChange={event => { if (event.target.value) setForm({ ...form, validFrom: inputToUtc(event.target.value) }) }} /></Grid><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth type='datetime-local' label='Kết thúc (giờ Việt Nam)' slotProps={{ inputLabel: { shrink: true } }} value={localInput(form.validTo)} onChange={event => { if (event.target.value) setForm({ ...form, validTo: inputToUtc(event.target.value) }) }} /></Grid></Grid>
    <FormControlLabel label='Hiển thị công khai' control={<Checkbox checked={form.isPublic} disabled={Boolean(initial)} onChange={(_, checked) => setForm({ ...form, isPublic: checked })} />} />{initial && <FormControlLabel label='Kích hoạt voucher' control={<Checkbox checked={form.isActive} onChange={(_, checked) => setForm({ ...form, isActive: checked })} />} />}
  </Stack></DialogContent><DialogActions><Button disabled={pending} onClick={onClose}>Quay lại</Button><Button variant='contained' color='secondary' disabled={pending} onClick={() => void submit()}>{pending ? 'Đang lưu…' : 'Lưu voucher'}</Button></DialogActions></Dialog>
}
