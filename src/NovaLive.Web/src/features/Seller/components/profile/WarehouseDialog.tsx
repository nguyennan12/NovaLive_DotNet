import { Alert, Autocomplete, Button, Checkbox, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, Stack, TextField } from '@mui/material'
import { useState } from 'react'
import { z } from 'zod'
import type { Warehouse, WarehouseRequest } from '../../types/contracts'
import { useDistricts, useProvinces, useSaveWarehouse, useWards } from '../../hooks/useShop'
import { errorMessage } from '../../utils/errors'

const empty: WarehouseRequest = { warehouseName: '', contactName: '', contactPhone: '', provinceId: null, provinceName: '', districtId: null, districtName: '', wardCode: null, wardName: '', detailAddress: '', isPrimary: false, isReturn: false }
const schema = z.object({ warehouseName: z.string().trim().min(1, 'Nhập tên kho.'), contactName: z.string().trim().min(1, 'Nhập người liên hệ.'), contactPhone: z.string().regex(/^0\d{9}$/, 'Số điện thoại kho không hợp lệ.'), provinceId: z.number('Chọn tỉnh/thành.'), provinceName: z.string(), districtId: z.number('Chọn quận/huyện.'), districtName: z.string(), wardCode: z.string('Chọn phường/xã.').min(1, 'Chọn phường/xã.'), wardName: z.string(), detailAddress: z.string().trim().min(3, 'Nhập địa chỉ chi tiết.'), isPrimary: z.boolean(), isReturn: z.boolean() })
export function WarehouseDialog({ initial, onClose }: { initial: Warehouse | null; onClose: () => void }) {
  const [form, setForm] = useState<WarehouseRequest>(initial || empty)
  const [error, setError] = useState('')
  const provinces = useProvinces(); const districts = useDistricts(form.provinceId); const wards = useWards(form.districtId)
  const save = useSaveWarehouse(onClose)
  const field = (name: keyof WarehouseRequest, label: string) => <TextField fullWidth label={label} value={form[name] ?? ''} onChange={event => setForm({ ...form, [name]: event.target.value })} />
  async function submit() {
    const parsed = schema.safeParse(form)
    if (!parsed.success) { setError(parsed.error.issues.map(issue => issue.message).join(' ')); return }
    try { await save.mutateAsync({ id: initial?.id || null, data: parsed.data }) } catch { /* Mutation toast. */ }
  }
  return <Dialog open onClose={save.isPending ? undefined : onClose} maxWidth='sm' fullWidth aria-labelledby='warehouse-title'><DialogTitle id='warehouse-title'>{initial ? 'Sửa kho hàng' : 'Thêm kho hàng'}</DialogTitle><DialogContent><Stack spacing={2} sx={{ pt: 1 }}>
    {error && <Alert severity='error'>{error}</Alert>}{field('warehouseName', 'Tên kho')}{field('contactName', 'Người liên hệ')}{field('contactPhone', 'Điện thoại kho')}
    <Autocomplete clearText='Xóa lựa chọn' closeText='Đóng' openText='Mở' loadingText='Đang tải…' noOptionsText='Không có kết quả' options={provinces.data || []} loading={provinces.isLoading} value={provinces.data?.find(item => item.ProvinceID === form.provinceId) || null} getOptionLabel={item => item.ProvinceName} onChange={(_, value) => setForm({ ...form, provinceId: value?.ProvinceID || null, provinceName: value?.ProvinceName || '', districtId: null, districtName: '', wardCode: null, wardName: '' })} renderInput={params => <TextField {...params} label='Tỉnh / Thành phố' />} />
    <Autocomplete clearText='Xóa lựa chọn' closeText='Đóng' openText='Mở' loadingText='Đang tải…' noOptionsText='Không có kết quả' options={districts.data || []} loading={districts.isLoading && Boolean(form.provinceId)} disabled={!form.provinceId} value={districts.data?.find(item => item.DistrictID === form.districtId) || null} getOptionLabel={item => item.DistrictName} onChange={(_, value) => setForm({ ...form, districtId: value?.DistrictID || null, districtName: value?.DistrictName || '', wardCode: null, wardName: '' })} renderInput={params => <TextField {...params} label='Quận / Huyện' />} />
    <Autocomplete clearText='Xóa lựa chọn' closeText='Đóng' openText='Mở' loadingText='Đang tải…' noOptionsText='Không có kết quả' options={wards.data || []} loading={wards.isLoading && Boolean(form.districtId)} disabled={!form.districtId} value={wards.data?.find(item => String(item.WardCode) === form.wardCode) || null} getOptionLabel={item => item.WardName} onChange={(_, value) => setForm({ ...form, wardCode: value ? String(value.WardCode) : null, wardName: value?.WardName || '' })} renderInput={params => <TextField {...params} label='Phường / Xã' />} />
    {[provinces, districts, wards].filter(query => query.error).map((query, index) => <Alert key={index} severity='error' action={<Button onClick={() => void query.refetch()}>Thử lại</Button>}>{errorMessage(query.error)}</Alert>)}
    {field('detailAddress', 'Số nhà, đường')}
    <FormControlLabel control={<Checkbox checked={form.isPrimary} onChange={(_, checked) => setForm({ ...form, isPrimary: checked })} />} label='Kho lấy hàng mặc định' />
    <FormControlLabel control={<Checkbox checked={form.isReturn} onChange={(_, checked) => setForm({ ...form, isReturn: checked })} />} label='Kho nhận hàng hoàn' />
  </Stack></DialogContent><DialogActions><Button disabled={save.isPending} onClick={onClose}>Quay lại</Button><Button variant='contained' color='secondary' disabled={save.isPending} onClick={() => void submit()}>{save.isPending ? 'Đang lưu…' : 'Lưu kho'}</Button></DialogActions></Dialog>
}
