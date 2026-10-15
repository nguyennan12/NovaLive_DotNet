import { Box, Typography, Tooltip, Skeleton } from '@mui/material'
import AccessTimeRoundedIcon from '@mui/icons-material/AccessTimeRounded'
import LockOutlinedIcon from '@mui/icons-material/LockOutlined'
import SectionCard from '../shared/SectionCard'
import EmptyState from '../shared/EmptyState'
import { getReservedStocAPI } from '~/common/apis/services/inventoryService'
import { useQuery } from '@tanstack/react-query'

const fmt = (iso) => {
  if (!iso) return ''
  const d = new Date(iso)
  return d.toLocaleString('en-GB', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' })
}


const getExpiresAt = (iso) => new Date(new Date(iso).getTime() + 15 * 60 * 1000)
const isExpired = (expiresAt) => expiresAt < new Date()

const ReservedStock = ({ loading = false }) => {
  const { data: reserveds = [], isLoading } = useQuery({
    queryKey: ['reserved_stock'],
    queryFn: getReservedStocAPI,
    staleTime: 30 * 1000
  })

  const isDataLoading = loading || isLoading

  return (
    <SectionCard
      title='Reserved Stock'
      subtitle={`${reserveds.length} product variations temporarily held`}
    >
      {isDataLoading ? (
        Array.from({ length: 3 }).map((_, i) => (
          <Box key={i} sx={{ py: 1.5, borderBottom: '1px solid #f3f4f6' }}>
            <Skeleton variant='rounded' height={16} width='60%' sx={{ mb: 1, borderRadius: '4px' }} />
            <Skeleton variant='rounded' height={14} width='40%' sx={{ borderRadius: '4px' }} />
          </Box>
        ))
      ) : reserveds.length === 0 ? (
        <EmptyState icon={LockOutlinedIcon} title='No reserved stock' subtitle='Reservations will appear here' />
      ) : (
        reserveds.map((r) => (
          <Box key={r.inventoryId} sx={{ mb: 2.5 }}>
            {/* Header: Thông tin SKU và Tổng số lượng */}
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1, px: 0.5 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Typography sx={{ fontSize: '0.85rem', fontWeight: 700, color: '#2d2d2d' }}>
                  {r.product?.spu_name}
                </Typography>
                {r.sku?.sku_name && (
                  <Typography sx={{ fontSize: '0.82rem', fontWeight: 500, color: '#737373' }}>
                    - {r.sku.sku_name}
                  </Typography>
                )}
              </Box>
              <Typography sx={{ fontSize: '0.75rem', fontWeight: 600, color: '#2d2d2d' }}>
                Total: {r.total_reserved}
              </Typography>
            </Box>

            {/* Danh sách các orders đang giữ SKU này */}
            {r.orders?.map((order) => {
              const expiresAt = getExpiresAt(order.order_createdAt)
              const expired = isExpired(expiresAt)

              return (
                <Box
                  key={order.orderId}
                  sx={{
                    py: 1.5,
                    px: 1.5,
                    mb: 1,
                    borderRadius: '10px',
                    bgcolor: expired ? '#fef2f2' : '#fafafa',
                    border: '1px solid',
                    borderColor: expired ? '#fecaca' : '#eeeeee',
                    transition: 'box-shadow 0.15s',
                    '&:hover': { boxShadow: '0 2px 8px rgba(0,0,0,0.06)' }
                  }}
                >
                  <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 0.75 }}>
                    <Box sx={{ flex: 1 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Typography sx={{ fontSize: '0.75rem', color: '#6b7280' }}>
                          Order: <span style={{ color: '#3485f7', fontWeight: 600 }}>{order.order_trackingNumber}</span>
                        </Typography>
                        <Box sx={{ width: 3, height: 3, borderRadius: '50%', bgcolor: '#d1d5db' }} />
                        <Typography sx={{ fontSize: '0.75rem', color: '#6b7280' }}>
                          Qty: <strong style={{ color: '#2d2d2d' }}>{order.quantity}</strong>
                        </Typography>
                      </Box>
                    </Box>

                    <Box
                      sx={{
                        px: 1,
                        py: 0.3,
                        borderRadius: '6px',
                        bgcolor: expired ? '#fee2e2' : '#eff6ff',
                        flexShrink: 0
                      }}
                    >
                      <Typography sx={{ fontSize: '0.65rem', fontWeight: 700, color: expired ? '#ef4444' : '#3485f7' }}>
                        {expired ? 'EXPIRED' : 'ACTIVE'}
                      </Typography>
                    </Box>
                  </Box>

                  <Box sx={{ display: 'flex', gap: 2 }}>
                    <Tooltip title='Reserved at'>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.4 }}>
                        <AccessTimeRoundedIcon sx={{ fontSize: 12, color: '#9ca3af' }} />
                        <Typography sx={{ fontSize: '0.68rem', color: '#9ca3af' }}>{fmt(order.order_createdAt)}</Typography>
                      </Box>
                    </Tooltip>
                    <Tooltip title='Expires at (15 mins)'>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.4 }}>
                        <AccessTimeRoundedIcon sx={{ fontSize: 12, color: expired ? '#ef4444' : '#f59e0b' }} />
                        <Typography sx={{ fontSize: '0.68rem', color: expired ? '#ef4444' : '#f59e0b', fontWeight: 500 }}>
                          {fmt(expiresAt)}
                        </Typography>
                      </Box>
                    </Tooltip>
                  </Box>
                </Box>
              )
            })}
          </Box>
        ))
      )}
    </SectionCard>
  )
}

export default ReservedStock