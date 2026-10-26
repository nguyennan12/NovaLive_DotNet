import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import type { Campaign } from '../../types/contracts'
import { useAllProducts, useSkuOptions } from '../../hooks/useProducts'
import { useRegisterFlashSale } from '../../hooks/useMarketing'
import { QueryState } from '../shared/QueryState'
import { money } from '../../utils/format'
import { canRegisterCampaign, validateFlashRegistration } from '../../utils/marketing'

export function FlashRegistrationDialog({ campaign, onClose }: { campaign: Campaign; onClose: () => void }) {
  const [productId, setProductId] = useState(''); const [skuId, setSkuId] = useState(''); const [price, setPrice] = useState(''); const [quantity, setQuantity] = useState('1'); const [limit, setLimit] = useState('1'); const [error, setError] = useState('')
  const products = useAllProducts(); const detail = useSkuOptions(productId); const register = useRegisterFlashSale()
  const sku = detail.data?.skus.find(item => item.id === skuId)
  function submit() {
    if (!canRegisterCampaign(campaign)) { setError('Chiến dịch đã đóng đăng ký.'); return }
    if (!sku) { setError('Vui lòng chọn SKU.'); return }
    const message = validateFlashRegistration(Number(price), sku.sellPrice, Number(quantity), sku.qtyOnHand - sku.reservedQty, Number(limit))
    if (message) { setError(message); return }
    register.mutate({ campaignId: campaign.id, data: { skuId, flashPrice: Number(price), quantity: Number(quantity), perUserLimit: Number(limit) } }, { onSuccess: onClose })
  }
  return <Dialog open fullWidth maxWidth='sm' onClose={register.isPending ? undefined : onClose} aria-labelledby='flash-title'><DialogTitle id='flash-title'>Đăng ký · {campaign.name}</DialogTitle><DialogContent><Stack spacing={2} sx={{ pt: 1 }}>
    {error && <Alert severity='error'>{error}</Alert>}
    <QueryState loading={products.isLoading} error={products.error} empty={!products.data?.some(item => item.status === 'Active')} emptyText='Cần sản phẩm đang hoạt động để đăng ký Flash Sale.' onRetry={products.refetch}><TextField select label='Sản phẩm' value={productId} onChange={event => { setProductId(event.target.value); setSkuId(''); setPrice('') }}>{products.data?.filter(item => item.status === 'Active').map(item => <MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>)}</TextField></QueryState>
    {productId && <QueryState loading={detail.isLoading} error={detail.error} empty={!detail.data?.skus.some(item => item.isActive && item.availableQty > 0)} emptyText='Sản phẩm chưa có SKU khả dụng.' onRetry={detail.refetch}><TextField select label='SKU' value={skuId} onChange={event => setSkuId(event.target.value)}>{detail.data?.skus.filter(item => item.isActive && item.availableQty > 0).map(item => <MenuItem key={item.id} value={item.id}>{item.skuCode} · {money(item.sellPrice)} · còn {item.qtyOnHand - item.reservedQty}</MenuItem>)}</TextField></QueryState>}
    {sku && <Alert severity='info'>Giá hiện tại: {money(sku.sellPrice)}. Giá Flash Sale tối đa {money(Math.floor(sku.sellPrice * 0.9))}. Tồn khả dụng: {sku.qtyOnHand - sku.reservedQty}.</Alert>}
    <TextField label='Giá Flash Sale (₫)' type='number' value={price} onChange={event => setPrice(event.target.value)} />
    {sku && Number(price) > 0 && <Typography color={Number(price) <= sku.sellPrice * 0.9 ? 'success.main' : 'error.main'}>Mức giảm: {((1 - Number(price) / sku.sellPrice) * 100).toFixed(1)}% · yêu cầu tối thiểu 10%</Typography>}
    <TextField label='Số lượng đăng ký' type='number' value={quantity} onChange={event => setQuantity(event.target.value)} />
    <TextField label='Giới hạn mỗi người' type='number' value={limit} onChange={event => setLimit(event.target.value)} />
  </Stack></DialogContent><DialogActions><Button disabled={register.isPending} onClick={onClose}>Quay lại</Button><Button variant='contained' color='secondary' disabled={register.isPending || !sku} onClick={submit}>{register.isPending ? 'Đang đăng ký…' : 'Gửi đăng ký'}</Button></DialogActions></Dialog>
}
