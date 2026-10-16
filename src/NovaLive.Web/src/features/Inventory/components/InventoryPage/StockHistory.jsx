import HistoryRoundedIcon from '@mui/icons-material/HistoryRounded'
import { Box, MenuItem, Pagination, Select, Skeleton, Typography } from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { getHistoryInventoryAPI } from '~/common/apis/services/inventoryService'
import EmptyState from '../shared/EmptyState'
import SectionCard from '../shared/SectionCard'
import StatusBadge from '../shared/StatusBadge'
import { LIMIT } from '~/common/utils/constant'


const fmt = (iso) => {
  const d = new Date(iso)
  return d.toLocaleString('en-GB', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' })
}

const COLS = ['Type', 'Product', 'SKU', 'Qty', 'Time', 'User', 'Note']
const GRID = '80px 2fr 1fr 60px 1.2fr 1.5fr 2fr'

const StockHistory = ({ loading = false }) => {

  const [page, setPage] = useState(1)
  const [type, setType] = useState('all')

  const params = { page, limit: LIMIT.INVENTORY, type }

  const queryString = new URLSearchParams(params).toString()
  const { data = [] } = useQuery({
    queryKey: ['history_inventory', queryString],
    queryFn: () => getHistoryInventoryAPI(queryString)
  })

  const histories = data?.items || []
  const totalPages = data?.totalPages || 1
  const totalItems = data?.totalItems || 0

  const handlePageChange = (event, value) => {
    setPage(value)
  }

  return (
    <SectionCard
      title='Stock History'
      subtitle={`  ${histories.length} records / ${totalItems} histories `}
      action={
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Select
            value={type}
            onChange={(e) => setType(e.target.value)}
            size='small'
            sx={{
              fontSize: '0.78rem', height: 34, borderRadius: '8px', minWidth: 110,
              '& fieldset': { borderColor: '#eeeeee' }
            }}
          >
            <MenuItem value='all' sx={{ fontSize: '0.78rem' }}>All Types</MenuItem>
            <MenuItem value='IN' sx={{ fontSize: '0.78rem' }}>Stock In</MenuItem>
            <MenuItem value='OUT' sx={{ fontSize: '0.78rem' }}>Stock Out</MenuItem>
          </Select>
        </Box>
      }
    >
      {/* Header */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: GRID,
          gap: 1,
          px: 1.5,
          py: 1,
          bgcolor: '#f9fafb',
          borderRadius: '8px',
          mb: 1
        }}
      >
        {COLS.map((c) => (
          <Typography key={c} sx={{ fontSize: '0.65rem', fontWeight: 700, color: '#9ca3af', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
            {c}
          </Typography>
        ))}
      </Box>

      {/* Rows */}
      {loading ? (
        Array.from({ length: 5 }).map((_, i) => (
          <Box key={i} sx={{ display: 'grid', gridTemplateColumns: GRID, gap: 1, px: 1.5, py: 1.25, borderBottom: '1px solid #f3f4f6' }}>
            {[60, 160, 90, 40, 100, 130, 140].map((w, j) => (
              <Skeleton key={j} variant='rounded' width={w} height={14} sx={{ borderRadius: '4px' }} />
            ))}
          </Box>
        ))
      ) : histories.length === 0 ? (
        <EmptyState icon={HistoryRoundedIcon} title='No history found' subtitle='Adjust filters to see results' />
      ) : (
        histories.map((h) => {
          return <Box
            key={h._id}
            sx={{
              display: 'grid',
              gridTemplateColumns: GRID,
              gap: 1,
              px: 1.5,
              py: 1.25,
              borderBottom: '1px solid #f9fafb',
              borderRadius: '6px',
              alignItems: 'center',
              transition: 'bgcolor 0.12s',
              '&:hover': { bgcolor: '#fafafa' }
            }}
          >
            <StatusBadge type={h.inven_type} />

            <Box>
              <Typography sx={{ fontSize: '0.8rem', fontWeight: 600, color: '#2d2d2d', lineHeight: 1.3 }} noWrap>
                {h.inven_productId.spu_name}
              </Typography>
            </Box>

            <Typography sx={{ fontSize: '0.72rem', color: '#9ca3af', fontFamily: 'monospace' }}>
              {h.inven_skuId?.sku_id || '—'}
            </Typography>

            <Typography
              sx={{
                fontSize: '0.85rem',
                fontWeight: 800,
                color: h.inven_type === 'IN' ? 'secondary.main' : 'third.main',
                letterSpacing: '-0.02em'
              }}
            >
              {h.inven_type === 'IN' ? '+' : '-'}{h.inven_quantity}
            </Typography>

            <Typography sx={{ fontSize: '0.72rem', color: '#6b7280' }}>{fmt(h.updatedAt)}</Typography>

            <Typography sx={{ fontSize: '0.72rem', color: '#6b7280' }} noWrap>{h.inven_userEmail}</Typography>

            <Typography sx={{ fontSize: '0.72rem', color: '#9ca3af' }} noWrap>{h.inven_note || '—'}</Typography>
          </Box>
        })
      )}
      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3, mb: 2 }}>
        <Pagination
          count={totalPages}
          page={page}
          onChange={handlePageChange}
          color="primary"
        />
      </Box>
    </SectionCard>
  )
}

export default StockHistory