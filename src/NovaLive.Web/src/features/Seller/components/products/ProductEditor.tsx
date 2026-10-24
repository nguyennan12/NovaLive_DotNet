import { Alert, Box, Button, Checkbox, FormControlLabel, Grid, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import type { CreateProductRequest, CreateSkuRequest, ProductDetail } from '../../types/contracts'
import { useCategories, useCreateProduct, useSaveProductChanges } from '../../hooks/useProducts'
import { useShopPermissions } from '../../hooks/useShopPermissions'
import { buildSkuMatrix, flattenCategories, productSchema } from '../../utils/product'
import { SellerCard } from '../shared/SellerCard'
import { MediaUpload } from '../shared/MediaUpload'
import { errorMessage } from '../../utils/errors'

interface DraftSku extends CreateSkuRequest { id?: string; isActive: boolean }
export function ProductEditor({ initial }: { initial?: ProductDetail }) {
  const [form, setForm] = useState<CreateProductRequest>(initial ? { name: initial.name, description: initial.description, categoryId: initial.categoryId, brand: initial.brand, thumbnailUrl: initial.thumbnailUrl, attributesConfigJson: initial.attributesConfigJson, attributes: initial.attributes, skus: [] } : { name: '', description: '', categoryId: '', brand: '', thumbnailUrl: '', attributesConfigJson: null, attributes: [], skus: [] })
  const [skus, setSkus] = useState<DraftSku[]>(initial ? initial.skus.map(sku => ({ ...sku, initialStock: 0 })) : [{ ...buildSkuMatrix([], 'NOVA')[0], isActive: true }])
  const [axes, setAxes] = useState<{ name: string; text: string }[]>([])
  const [prefix, setPrefix] = useState('NOVA')
  const [bulk, setBulk] = useState({ price: '', stock: '' })
  const [error, setError] = useState('')
  const categories = useCategories(); const create = useCreateProduct(); const update = useSaveProductChanges(); const navigate = useNavigate(); const { canOperate } = useShopPermissions()
  const pending = create.isPending || update.isPending
  const disabled = pending || (!initial && !canOperate) || initial?.status === 'Banned'
  function changeSku(index: number, patch: Partial<DraftSku>) { setSkus(rows => rows.map((row, i) => i === index ? { ...row, ...patch } : row)) }
  function generate() {
    try {
      const normalized = axes.map(axis => ({ name: axis.name, values: axis.text.split(',') }))
      setSkus(buildSkuMatrix(normalized, prefix).map(sku => ({ ...sku, isActive: true })))
      setForm({ ...form, attributesConfigJson: JSON.stringify(normalized) }); setError('')
    } catch (error) { setError(errorMessage(error)) }
  }
  async function submit(event: React.FormEvent) {
    event.preventDefault(); setError('')
    const parsed = productSchema.safeParse({ ...form, skus })
    if (!parsed.success) { setError(parsed.error.issues.map(issue => `${issue.path.join('.')} · ${issue.message}`).join('\n')); return }
    try {
      if (initial) {
        const { skus: _skus, ...data } = parsed.data
        void _skus
        await update.mutateAsync({ id: initial.id, data, skus: skus.filter(sku => sku.id).map(sku => ({ id: sku.id!, data: { sellPrice: sku.sellPrice, originalPrice: sku.originalPrice, weightGram: sku.weightGram, isActive: sku.isActive } })) })
      } else await create.mutateAsync(parsed.data)
      navigate('/seller/products')
    } catch (error) { setError(errorMessage(error)) }
  }
  return <Box component='form' onSubmit={event => void submit(event)}><Stack spacing={2.5}>
    {error && <Alert severity='error' sx={{ whiteSpace: 'pre-line' }}>{error}</Alert>}
    {initial?.status === 'Banned' && <Alert severity='warning'>Sản phẩm đã bị khóa. Vui lòng liên hệ bộ phận hỗ trợ.</Alert>}
    <Box component='fieldset' disabled={disabled} sx={{ border: 0, p: 0, m: 0, minWidth: 0 }}><Stack spacing={2.5}>
      <SellerCard title='Thông tin chung'><Grid container spacing={2}><Grid size={{ xs: 12, md: 8 }}><Stack spacing={2}><TextField label='Tên sản phẩm' required value={form.name} onChange={event => setForm({ ...form, name: event.target.value })} /><TextField select label='Danh mục' required value={form.categoryId} onChange={event => setForm({ ...form, categoryId: event.target.value })}><MenuItem value=''>Chọn danh mục</MenuItem>{flattenCategories(categories.data || []).map(category => <MenuItem key={category.id} value={category.id}>{category.name}</MenuItem>)}</TextField><TextField label='Thương hiệu' value={form.brand || ''} onChange={event => setForm({ ...form, brand: event.target.value || null })} /><TextField multiline minRows={5} label='Mô tả sản phẩm' required value={form.description} onChange={event => setForm({ ...form, description: event.target.value })} /></Stack></Grid><Grid size={{ xs: 12, md: 4 }}><MediaUpload label='Ảnh đại diện sản phẩm' urls={form.thumbnailUrl ? [form.thumbnailUrl] : []} onChange={urls => setForm({ ...form, thumbnailUrl: urls[0] || '' })} disabled={disabled} /></Grid></Grid></SellerCard>
      <SellerCard title='Thuộc tính kỹ thuật'><Stack spacing={1.5}>{form.attributes.map((attribute, index) => <Box key={index} sx={{ display: 'flex', gap: 1 }}><TextField label='Tên thuộc tính' size='small' value={attribute.name} onChange={event => setForm({ ...form, attributes: form.attributes.map((item, i) => i === index ? { ...item, name: event.target.value } : item) })} /><TextField label='Giá trị' size='small' value={attribute.value} onChange={event => setForm({ ...form, attributes: form.attributes.map((item, i) => i === index ? { ...item, value: event.target.value } : item) })} /><Button color='error' onClick={() => setForm({ ...form, attributes: form.attributes.filter((_, i) => i !== index) })}>Bỏ</Button></Box>)}<Button onClick={() => setForm({ ...form, attributes: [...form.attributes, { name: '', value: '' }] })}>Thêm thuộc tính</Button></Stack></SellerCard>
      {!initial && <SellerCard title='Cấu hình biến thể'><Stack spacing={2}><Alert severity='info'>Nhập các giá trị cách nhau bằng dấu phẩy. Sinh ma trận sẽ thay thế danh sách SKU đang nhập; tối đa 200 SKU.</Alert><TextField label='Tiền tố mã SKU' value={prefix} onChange={event => setPrefix(event.target.value)} />{axes.map((axis, index) => <Box key={index} sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}><TextField label='Trục biến thể (ví dụ: Màu)' value={axis.name} onChange={event => setAxes(axes.map((item, i) => i === index ? { ...item, name: event.target.value } : item))} /><TextField label='Giá trị (ví dụ: Trắng, Đen)' value={axis.text} onChange={event => setAxes(axes.map((item, i) => i === index ? { ...item, text: event.target.value } : item))} /><Button color='error' onClick={() => setAxes(axes.filter((_, i) => i !== index))}>Bỏ trục</Button></Box>)}<Stack direction='row' spacing={1}><Button onClick={() => setAxes([...axes, { name: '', text: '' }])}>Thêm trục biến thể</Button><Button variant='outlined' color='secondary' onClick={generate}>Sinh ma trận SKU</Button></Stack></Stack></SellerCard>}
      <SellerCard title={`Cấu hình SKU (${skus.length})`}>
        {initial && <Alert severity='info' sx={{ mb: 2 }}>Mã và trục biến thể không thể đổi bằng API hiện có. Điều chỉnh tồn kho tại <Link to='/seller/inventory'>Kho hàng</Link>; tồn đầu không được gửi khi sửa.</Alert>}
        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}><TextField type='number' size='small' label='Giá bán hàng loạt' value={bulk.price} onChange={event => setBulk({ ...bulk, price: event.target.value })} />{!initial && <TextField type='number' size='small' label='Tồn đầu hàng loạt' value={bulk.stock} onChange={event => setBulk({ ...bulk, stock: event.target.value })} />}<Button onClick={() => setSkus(skus.map(sku => ({ ...sku, ...(bulk.price !== '' ? { sellPrice: Number(bulk.price) } : {}), ...(!initial && bulk.stock !== '' ? { initialStock: Number(bulk.stock) } : {}) })))}>Áp dụng tất cả</Button></Box>
        <Stack spacing={2}>{skus.map((sku, index) => <Box key={sku.id || index} sx={{ p: 2, border: 1, borderColor: 'divider', borderRadius: 2 }}><Typography sx={{ fontWeight: 700, mb: 1 }}>{Object.values(JSON.parse(sku.attributesJson || '{}') as Record<string, string>).join(' / ') || 'Biến thể mặc định'}</Typography><Grid container spacing={1.5}>
          <Grid size={{ xs: 12, sm: 6, lg: 3 }}><TextField fullWidth size='small' label='Mã SKU' value={sku.skuCode} disabled={Boolean(initial)} onChange={event => changeSku(index, { skuCode: event.target.value })} /></Grid>
          {(['originalPrice', 'sellPrice', 'weightGram', ...(!initial ? ['initialStock'] as const : [])] as const).map(key => <Grid key={key} size={{ xs: 6, lg: 2 }}><TextField fullWidth type='number' size='small' label={{ originalPrice: 'Giá gốc (₫)', sellPrice: 'Giá bán (₫)', weightGram: 'Khối lượng (g)', initialStock: 'Tồn đầu' }[key]} value={sku[key]} onChange={event => changeSku(index, { [key]: Number(event.target.value) })} /></Grid>)}
        </Grid>{initial && <FormControlLabel label='SKU đang hoạt động' control={<Checkbox checked={sku.isActive} onChange={(_, checked) => changeSku(index, { isActive: checked })} />} />}{!initial && <MediaUpload label={`Ảnh SKU ${sku.skuCode}`} multiple urls={sku.images} onChange={images => changeSku(index, { images })} disabled={disabled} />}</Box>)}</Stack>
      </SellerCard>
    </Stack></Box>
    <Stack direction='row' spacing={1}><Button component={Link} to='/seller/products' disabled={pending}>Quay lại</Button><Button type='submit' variant='contained' color='secondary' disabled={disabled}>{pending ? 'Đang lưu…' : initial ? 'Lưu thay đổi' : 'Tạo sản phẩm'}</Button></Stack>
  </Stack></Box>
}
