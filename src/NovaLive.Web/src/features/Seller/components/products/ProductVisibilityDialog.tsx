import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, Switch, FormControlLabel } from '@mui/material'
import { useUpdateSku } from '../../hooks/useProducts'
import { useProduct } from '../../hooks/useProducts'
import { QueryState } from '../shared/QueryState'
import type { ProductDetail } from '../../types/contracts'

export function ProductVisibilityDialog({ product, onClose }: { product: ProductDetail; onClose: () => void }) {
  const query = useProduct(product.id)
  const update = useUpdateSku()
  return <Dialog open onClose={update.isPending ? undefined : onClose} fullWidth maxWidth='sm' aria-labelledby='visibility-title'><DialogTitle id='visibility-title'>Hiển thị SKU · {product.name}</DialogTitle><DialogContent><Stack spacing={2}>
    <Alert severity='info'>Công tắc điều khiển từng SKU. Trạng thái SPU do backend quản lý; hiện chưa có API bật lại SPU đã ẩn.</Alert>
    <QueryState loading={query.isLoading} error={query.error} onRetry={query.refetch}>{query.data?.skus.map(sku => <FormControlLabel key={sku.id} label={sku.skuCode} control={<Switch checked={sku.isActive} disabled={update.isPending || product.status === 'Banned' || product.status === 'Inactive'} onChange={(_, checked) => update.mutate({ id: sku.id, data: { sellPrice: sku.sellPrice, originalPrice: sku.originalPrice, weightGram: sku.weightGram, isActive: checked } })} />} />)}</QueryState>
  </Stack></DialogContent><DialogActions><Button onClick={onClose} disabled={update.isPending}>Đóng</Button></DialogActions></Dialog>
}
