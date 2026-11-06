import { Alert, Box, Button, LinearProgress, MenuItem, Stack, TextField, Typography } from '@mui/material'
import BoltRounded from '@mui/icons-material/BoltRounded'
import CalendarMonthRounded from '@mui/icons-material/CalendarMonthRounded'
import { useState } from 'react'
import type { Campaign } from '../types/contracts'
import { useCampaigns, useRegistrations } from '../hooks/useMarketing'
import { useShopPermissions } from '../hooks/useShopPermissions'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { DataTable } from '../components/shared/DataTable'
import { StatusBadge } from '../components/shared/StatusBadge'
import { DateText } from '../components/shared/DateText'
import { MoneyText } from '../components/shared/MoneyText'
import { FlashRegistrationDialog } from '../components/marketing/FlashRegistrationDialog'
import { PAGE_SIZE } from '../constants'
import { canRegisterCampaign } from '../utils/marketing'

export default function FlashSalesPage() {
  const [campaign, setCampaign] = useState<Campaign | null>(null)
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState('')
  const campaigns = useCampaigns()
  const registrations = useRegistrations({ page, size: PAGE_SIZE, status: status || undefined })
  const { canOperate } = useShopPermissions()

  return (
    <>
      <PageHeader
        title='Chiến dịch Flash Sale'
        description='Tham gia các chiến dịch giảm giá chớp nhoáng của sàn để bùng nổ doanh số.'
      />

      <Stack spacing={3}>
        <SellerCard title='Các chiến dịch Flash Sale từ Sàn'>
          <Alert severity='info' sx={{ mb: 2.5, borderRadius: '12px' }}>
            Shop cần đăng ký SKU tham gia trước khi chiến dịch bắt đầu ít nhất 24 giờ.
          </Alert>

          <DataTable
            rows={campaigns.data || []}
            rowKey={row => row.id}
            loading={campaigns.isLoading}
            error={campaigns.error}
            onRetry={campaigns.refetch}
            emptyText='Chưa có chiến dịch nào được mở.'
            columns={[
              {
                key: 'name',
                label: 'Tên chiến dịch',
                render: row => (
                  <Stack direction='row' spacing={1} sx={{ alignItems: 'center' }}>
                    <Box sx={{ width: 32, height: 32, borderRadius: '8px', bgcolor: '#fff7ed', color: '#ea580c', display: 'grid', placeItems: 'center' }}>
                      <BoltRounded fontSize='small' />
                    </Box>
                    <Typography sx={{ fontWeight: 700, fontSize: '0.88rem', color: '#0f172a' }}>
                      {row.name}
                    </Typography>
                  </Stack>
                )
              },
              {
                key: 'start',
                label: 'Bắt đầu',
                render: row => (
                  <Stack direction='row' spacing={0.5} sx={{ alignItems: 'center' }}>
                    <CalendarMonthRounded sx={{ fontSize: 14, color: '#94a3b8' }} />
                    <DateText value={row.startAt} />
                  </Stack>
                )
              },
              {
                key: 'end',
                label: 'Kết thúc',
                render: row => (
                  <Stack direction='row' spacing={0.5} sx={{ alignItems: 'center' }}>
                    <CalendarMonthRounded sx={{ fontSize: 14, color: '#94a3b8' }} />
                    <DateText value={row.endAt} />
                  </Stack>
                )
              },
              { key: 'status', label: 'Trạng thái', render: row => <StatusBadge status={row.status} /> },
              {
                key: 'action',
                label: 'Thao tác',
                render: row => (
                  <Button
                    variant='contained'
                    size='small'
                    disabled={!canOperate || !canRegisterCampaign(row)}
                    onClick={() => setCampaign(row)}
                    sx={{
                      borderRadius: '8px',
                      textTransform: 'none',
                      fontSize: '0.78rem',
                      fontWeight: 700,
                      bgcolor: canRegisterCampaign(row) ? '#f97316' : undefined,
                      '&:hover': { bgcolor: '#ea580c' }
                    }}
                  >
                    {canRegisterCampaign(row) ? 'Đăng ký SKU' : 'Đã đóng'}
                  </Button>
                )
              }
            ]}
          />
        </SellerCard>

        <SellerCard
          title='Sản phẩm Shop đã đăng ký'
          action={
            <TextField
              select
              label='Lọc trạng thái'
              value={status}
              size='small'
              onChange={event => { setStatus(event.target.value); setPage(1) }}
              sx={{ minWidth: 180 }}
            >
              <MenuItem value=''>Tất cả trạng thái</MenuItem>
              {['Pending', 'Approved', 'Rejected', 'Ended'].map(value => (
                <MenuItem key={value} value={value}>
                  <StatusBadge status={value} />
                </MenuItem>
              ))}
            </TextField>
          }
        >
          <DataTable
            rows={registrations.data?.items || []}
            rowKey={row => row.id}
            loading={registrations.isLoading}
            error={registrations.error}
            onRetry={registrations.refetch}
            page={page}
            totalPages={registrations.data?.totalPages}
            total={registrations.data?.total}
            onPage={setPage}
            emptyText='Shop chưa đăng ký sản phẩm nào cho chiến dịch.'
            columns={[
              {
                key: 'name',
                label: 'Sản phẩm / SKU',
                render: row => (
                  <Box>
                    <Typography sx={{ fontWeight: 600, fontSize: '0.85rem' }}>{row.spuName}</Typography>
                    <Typography variant='caption' sx={{ color: '#64748b', fontFamily: 'monospace' }}>{row.skuCode}</Typography>
                  </Box>
                )
              },
              {
                key: 'price',
                label: 'Giá Flash Sale',
                render: row => (
                  <Typography sx={{ fontWeight: 800, color: '#ea580c' }}>
                    <MoneyText value={row.flashPrice} />
                  </Typography>
                )
              },
              {
                key: 'sold',
                label: 'Tiến độ bán',
                render: row => (
                  <Box sx={{ minWidth: 130 }}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant='caption' sx={{ fontWeight: 700 }}>{row.soldQty} / {row.quantity}</Typography>
                      <Typography variant='caption' sx={{ color: '#64748b' }}>
                        {row.quantity ? Math.round(row.soldQty / row.quantity * 100) : 0}%
                      </Typography>
                    </Box>
                    <LinearProgress
                      variant='determinate'
                      value={row.quantity ? Math.min(100, row.soldQty / row.quantity * 100) : 0}
                      sx={{ height: 6, borderRadius: 3, bgcolor: '#ffedd5', '& .MuiLinearProgress-bar': { bgcolor: '#f97316' } }}
                    />
                  </Box>
                )
              },
              { key: 'status', label: 'Trạng thái', render: row => <StatusBadge status={row.status} /> },
              {
                key: 'reason',
                label: 'Ghi chú duyệt',
                render: row => <Typography variant='caption' sx={{ color: '#64748b' }}>{row.rejectionReason || '—'}</Typography>
              }
            ]}
          />
        </SellerCard>
      </Stack>

      {campaign && <FlashRegistrationDialog campaign={campaign} onClose={() => setCampaign(null)} />}
    </>
  )
}
