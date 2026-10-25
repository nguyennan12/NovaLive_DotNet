import { Alert, Box, Button, Grid, Stack, TextField } from '@mui/material'
import { useForm, useWatch } from 'react-hook-form'
import { z } from 'zod'
import type { Shop, ShopProfileRequest } from '../../types/contracts'
import { useUpdateShop } from '../../hooks/useShop'
import { SellerCard } from '../shared/SellerCard'
import { MediaUpload } from '../shared/MediaUpload'
import { StatusBadge } from '../shared/StatusBadge'

const schema = z.object({ shopName: z.string().trim().min(2, 'Tên cửa hàng cần ít nhất 2 ký tự.'), description: z.string().max(5000, 'Mô tả tối đa 5.000 ký tự.'), logoUrl: z.string().nullable(), bannerUrl: z.string().nullable(), phone: z.string().regex(/^0\d{9}$/, 'Nhập số điện thoại gồm 10 chữ số, bắt đầu bằng 0.'), email: z.email('Địa chỉ email không hợp lệ.') })
export function ShopProfileForm({ shop }: { shop: Shop }) {
  const mutation = useUpdateShop()
  const { register, handleSubmit, control, setValue, setError, formState: { errors } } = useForm<ShopProfileRequest>({ defaultValues: shop })
  const logoUrl = useWatch({ control, name: 'logoUrl' })
  const bannerUrl = useWatch({ control, name: 'bannerUrl' })
  async function submit(values: ShopProfileRequest) {
    const parsed = schema.safeParse(values)
    if (!parsed.success) { setError('root', { message: parsed.error.issues.map(issue => issue.message).join(' ') }); return }
    try { await mutation.mutateAsync(parsed.data) } catch { /* Mutation hook displays backend errors. */ }
  }
  return <SellerCard title='Hồ sơ cửa hàng' action={<StatusBadge status={shop.status} />}><Box component='form' onSubmit={handleSubmit(submit)} noValidate><Stack spacing={2.5}>
    {errors.root && <Alert severity='error'>{errors.root.message}</Alert>}
    <TextField label='Tên cửa hàng' {...register('shopName', { required: 'Vui lòng nhập tên cửa hàng.' })} error={Boolean(errors.shopName)} helperText={errors.shopName?.message} />
    <TextField label='Mô tả cửa hàng' multiline minRows={3} {...register('description')} />
    <Grid container spacing={2}><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth label='Điện thoại liên hệ' {...register('phone')} /></Grid><Grid size={{ xs: 12, sm: 6 }}><TextField fullWidth type='email' label='Email liên hệ' {...register('email')} /></Grid></Grid>
    <Grid container spacing={2}><Grid size={{ xs: 12, sm: 6 }}><MediaUpload label='Logo cửa hàng' urls={logoUrl ? [logoUrl] : []} onChange={urls => setValue('logoUrl', urls[0] || null, { shouldDirty: true })} disabled={mutation.isPending} /></Grid><Grid size={{ xs: 12, sm: 6 }}><MediaUpload label='Ảnh bìa cửa hàng' urls={bannerUrl ? [bannerUrl] : []} onChange={urls => setValue('bannerUrl', urls[0] || null, { shouldDirty: true })} disabled={mutation.isPending} /></Grid></Grid>
    {shop.reviewerNote && <Alert severity='info'>{shop.reviewerNote}</Alert>}
    <Box><Button type='submit' variant='contained' color='secondary' disabled={mutation.isPending}>{mutation.isPending ? 'Đang lưu…' : 'Lưu hồ sơ'}</Button></Box>
  </Stack></Box></SellerCard>
}
