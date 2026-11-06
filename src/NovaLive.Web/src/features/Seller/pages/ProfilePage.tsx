import { Box, Button, Chip, Stack, Typography } from '@mui/material'
import AddRounded from '@mui/icons-material/AddRounded'
import WarehouseRounded from '@mui/icons-material/WarehouseRounded'
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
  const shop = useSellerShop()
  const warehouses = useWarehouses()
  const remove = useDeleteWarehouse()
  const [editor, setEditor] = useState<{ initial: Warehouse | null } | null>(null)
  const [target, setTarget] = useState<Warehouse | null>(null)

  return (
    <>
      <PageHeader
        title='Hồ sơ cửa hàng & Kho bãi'
        description='Thông tin định danh, liên hệ và các địa chỉ kho vận hành của Shop.'
      />

      <Stack spacing={3}>
        <QueryState loading={shop.isLoading} error={shop.error} onRetry={shop.refetch}>
          {shop.data && <ShopProfileForm key={shop.data.id} shop={shop.data} />}
        </QueryState>

        <SellerCard
          title='Kho lấy hàng & Kho nhận hoàn'
          action={
            <Button
              color='secondary'
              variant='contained'
              startIcon={<AddRounded />}
              onClick={() => setEditor({ initial: null })}
              sx={{ borderRadius: '10px' }}
            >
              Thêm kho
            </Button>
          }
        >
          <DataTable
            rows={warehouses.data || []}
            rowKey={row => row.id}
            loading={warehouses.isLoading}
            error={warehouses.error}
            onRetry={warehouses.refetch}
            emptyText='Chưa có kho. Thêm kho để xác nhận và giao đơn hàng.'
            emptyAction={<Button onClick={() => setEditor({ initial: null })}>Thêm kho đầu tiên</Button>}
            columns={[
              {
                key: 'name',
                label: 'Tên kho',
                render: row => (
                  <Stack spacing={0.5}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <WarehouseRounded sx={{ fontSize: 18, color: '#3b82f6' }} />
                      <Typography sx={{ fontWeight: 700, fontSize: '0.88rem' }}>{row.warehouseName}</Typography>
                    </Box>
                    <Stack direction='row' spacing={0.5}>
                      {row.isPrimary && <Chip size='small' label='Kho lấy hàng mặc định' color='info' variant='outlined' sx={{ fontSize: '0.7rem' }} />}
                      {row.isReturn && <Chip size='small' label='Kho nhận hàng hoàn' color='success' variant='outlined' sx={{ fontSize: '0.7rem' }} />}
                    </Stack>
                  </Stack>
                )
              },
              {
                key: 'contact',
                label: 'Liên hệ',
                render: row => (
                  <Box>
                    <Typography sx={{ fontWeight: 600, fontSize: '0.82rem' }}>{row.contactName}</Typography>
                    <Typography variant='caption' sx={{ color: '#64748b' }}>{row.contactPhone}</Typography>
                  </Box>
                )
              },
              {
                key: 'address',
                label: 'Địa chỉ chi tiết',
                render: row => (
                  <Typography sx={{ fontSize: '0.82rem', color: '#475569' }}>
                    {[row.detailAddress, row.wardName, row.districtName, row.provinceName].join(', ')}
                  </Typography>
                )
              },
              {
                key: 'action',
                label: 'Thao tác',
                render: row => (
                  <Stack direction='row' spacing={1}>
                    <Button size='small' variant='outlined' onClick={() => setEditor({ initial: row })} sx={{ borderRadius: '6px' }}>
                      Sửa
                    </Button>
                    <Button size='small' color='error' onClick={() => setTarget(row)} sx={{ borderRadius: '6px' }}>
                      Xóa
                    </Button>
                  </Stack>
                )
              }
            ]}
          />
        </SellerCard>
      </Stack>

      {editor && <WarehouseDialog initial={editor.initial} onClose={() => setEditor(null)} />}
      <ConfirmDialog
        open={Boolean(target)}
        title='Xóa địa chỉ kho?'
        description={`Kho ${target?.warehouseName || ''} sẽ không còn dùng để lấy hoặc nhận hàng. Kho đang gắn với đơn hàng có thể bị backend từ chối xóa.`}
        danger
        pending={remove.isPending}
        onClose={() => setTarget(null)}
        onConfirm={() => {
          if (target) remove.mutate(target.id, { onSuccess: () => setTarget(null) })
        }}
      />
    </>
  )
}
