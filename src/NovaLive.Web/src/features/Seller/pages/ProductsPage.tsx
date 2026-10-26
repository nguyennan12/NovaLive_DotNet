import { Avatar, Box, Button, MenuItem, Stack, TextField, Typography } from '@mui/material'
import { Link } from 'react-router-dom'
import { useState } from 'react'
import { useCategories, useHideProduct, useProductTable } from '../hooks/useProducts'
import { useDebouncedValue } from '../hooks/useSellerQuery'
import { useShopPermissions } from '../hooks/useShopPermissions'
import { PAGE_SIZE } from '../constants'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { DataTable } from '../components/shared/DataTable'
import { StatusBadge } from '../components/shared/StatusBadge'
import { MoneyText } from '../components/shared/MoneyText'
import { ConfirmDialog } from '../components/shared/ConfirmDialog'
import { ProductVisibilityDialog } from '../components/products/ProductVisibilityDialog'
import { flattenCategories } from '../utils/product'
import type { ProductDetail } from '../types/contracts'

export default function ProductsPage() {
  const [page, setPage] = useState(1); const [keyword, setKeyword] = useState(''); const [categoryId, setCategory] = useState(''); const [status, setStatus] = useState('')
  const [target, setTarget] = useState<ProductDetail | null>(null); const [visibility, setVisibility] = useState<ProductDetail | null>(null)
  const query = useProductTable({ page, size: PAGE_SIZE, keyword: useDebouncedValue(keyword), categoryId: categoryId || undefined }, status || undefined)
  const categories = useCategories(); const remove = useHideProduct(); const { canOperate } = useShopPermissions()
  const createButton = <Button component={Link} to='/seller/products/new' variant='contained' color='secondary' disabled={!canOperate}>Thêm sản phẩm</Button>
  return <><PageHeader title='Sản phẩm' description='Quản lý SPU, biến thể SKU, giá và khả năng hiển thị.' action={createButton} />
    <Box sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap', mb: 2 }}><TextField size='small' label='Tìm tên sản phẩm' value={keyword} onChange={event => { setKeyword(event.target.value); setPage(1) }} /><TextField select label='Danh mục' size='small' value={categoryId} onChange={event => { setCategory(event.target.value); setPage(1) }} sx={{ minWidth: 190 }}><MenuItem value=''>Tất cả danh mục</MenuItem>{flattenCategories(categories.data || []).map(item => <MenuItem key={item.id} value={item.id}>{item.name}</MenuItem>)}</TextField><TextField select label='Trạng thái' size='small' value={status} onChange={event => { setStatus(event.target.value); setPage(1) }} sx={{ minWidth: 160 }}><MenuItem value=''>Tất cả</MenuItem>{['Draft', 'Active', 'Inactive', 'Banned'].map(value => <MenuItem key={value} value={value}><StatusBadge status={value} /></MenuItem>)}</TextField></Box>
    <SellerCard><DataTable rows={query.data?.items || []} rowKey={row => row.id} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={query.data?.totalPages} total={query.data?.total} onPage={setPage} emptyText='Không tìm thấy sản phẩm phù hợp.' emptyAction={createButton} columns={[
      { key: 'name', label: 'Sản phẩm', render: row => <Stack direction='row' spacing={1.5}><Avatar variant='rounded' src={row.thumbnailUrl} alt={row.name}>{row.name[0]}</Avatar><Box><Typography sx={{ fontWeight: 700 }}>{row.name}</Typography><Typography variant='caption' color='text.secondary'>{row.categoryName}</Typography></Box></Stack> },
      { key: 'price', label: 'Khoảng giá', render: row => <><MoneyText value={row.minPrice} />{row.maxPrice !== row.minPrice && <> – <MoneyText value={row.maxPrice} /></>}</> },
      { key: 'qty', label: 'Tồn khả dụng', render: row => row.skus.reduce((sum, sku) => sum + sku.qtyOnHand - sku.reservedQty, 0) },
      { key: 'status', label: 'Trạng thái', render: row => <StatusBadge status={row.status} /> },
      { key: 'action', label: 'Thao tác', render: row => <Stack direction='row' sx={{ flexWrap: 'wrap' }}><Button component={Link} to={`/seller/products/${row.id}/edit`}>Sửa</Button><Button onClick={() => setVisibility(row)}>Hiển thị SKU</Button><Button color='error' disabled={row.status === 'Banned' || row.status === 'Inactive'} onClick={() => setTarget(row)}>Ẩn</Button></Stack> }
    ]} /></SellerCard>
    {visibility && <ProductVisibilityDialog product={visibility} onClose={() => setVisibility(null)} />}<ConfirmDialog open={Boolean(target)} title='Ẩn sản phẩm?' description={`Sản phẩm “${target?.name || ''}” sẽ ngừng bán. Hiện backend chưa cung cấp API bật lại SPU đã ẩn.`} danger pending={remove.isPending} onClose={() => setTarget(null)} onConfirm={() => { if (target) remove.mutate(target.id, { onSuccess: () => setTarget(null) }) }} />
  </>
}
