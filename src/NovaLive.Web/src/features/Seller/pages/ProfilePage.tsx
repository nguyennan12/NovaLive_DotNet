import { Button, Chip, Stack, Typography } from '@mui/material'
import { useState } from 'react'
import { useDeleteWarehouse, useSellerShop, useWarehouses } from '../hooks/useShop'
import { PageHeader } from '../components/shared/PageHeader'
import { QueryState } from '../components/shared/QueryState'
import { DataTable } from '../components/shared/DataTable'
import { SellerCard } from '../components/shared/SellerCard'
import { ConfirmDialog } from '../components/shared/ConfirmDialog'
import { ShopProfileForm } from '../components/profile/ShopProfileForm'
import { WarehouseDialog } from '../components/profile/WarehouseDialog'
import type { Warehouse } from '../types/contracts'

export default function ProfilePage() {
  const shop = useSellerShop(); const warehouses = useWarehouses(); const remove = useDeleteWarehouse()
  const [editor, setEditor] = useState<{ initial: Warehouse | null } | null>(null)
  const [target, setTarget] = useState<Warehouse | null>(null)
  return <><PageHeader title='Hồ sơ & Kho của Shop' description='Thông tin liên hệ và địa chỉ vận hành cửa hàng.' /><Stack spacing={3}>
    <QueryState loading={shop.isLoading} error={shop.error} onRetry={shop.refetch}>{shop.data && <ShopProfileForm key={shop.data.id} shop={shop.data} />}</QueryState>
    <SellerCard title='Kho lấy hàng & Kho nhận hoàn' action={<Button color='secondary' variant='outlined' onClick={() => setEditor({ initial: null })}>Thêm kho</Button>}><DataTable rows={warehouses.data || []} rowKey={row => row.id} loading={warehouses.isLoading} error={warehouses.error} onRetry={warehouses.refetch} emptyText='Chưa có kho. Thêm kho để xác nhận và giao đơn hàng.' emptyAction={<Button onClick={() => setEditor({ initial: null })}>Thêm kho đầu tiên</Button>} columns={[
      { key: 'name', label: 'Kho', render: row => <Stack spacing={0.5}><Typography sx={{ fontWeight: 700 }}>{row.warehouseName}</Typography>{row.isPrimary && <Chip size='small' label='Kho lấy hàng mặc định' color='info' variant='outlined' />}{row.isReturn && <Chip size='small' label='Kho nhận hàng hoàn' color='success' variant='outlined' />}</Stack> },
      { key: 'contact', label: 'Liên hệ', render: row => <>{row.contactName}<br />{row.contactPhone}</> },
      { key: 'address', label: 'Địa chỉ', render: row => [row.detailAddress, row.wardName, row.districtName, row.provinceName].join(', ') },
      { key: 'action', label: 'Thao tác', render: row => <Stack direction='row'><Button onClick={() => setEditor({ initial: row })}>Sửa</Button><Button color='error' onClick={() => setTarget(row)}>Xóa</Button></Stack> }
    ]} /></SellerCard>
  </Stack>{editor && <WarehouseDialog initial={editor.initial} onClose={() => setEditor(null)} />}<ConfirmDialog open={Boolean(target)} title='Xóa địa chỉ kho?' description={`Kho ${target?.warehouseName || ''} sẽ không còn dùng để lấy hoặc nhận hàng. Kho đang gắn với đơn hàng có thể bị backend từ chối xóa.`} danger pending={remove.isPending} onClose={() => setTarget(null)} onConfirm={() => { if (target) remove.mutate(target.id, { onSuccess: () => setTarget(null) }) }} /></>
}
