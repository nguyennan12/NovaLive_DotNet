import InventoryRoundedIcon from '@mui/icons-material/InventoryRounded'
import { Box, MenuItem, Pagination, Select, Skeleton, Typography } from '@mui/material'
import EmptyState from '../shared/EmptyState'
import SectionCard from '../shared/SectionCard'
import StatusBadge from '../shared/StatusBadge'


const ProductStockList = ({ loading = false, isStale = false, skus, totalPages, params, setParams }) => {

  const handlePageChange = (event, value) => {
    setParams(prev => ({ ...prev, page: value }))
  }

  const selectSx = {
    fontSize: '0.78rem',
    height: 34,
    borderRadius: '8px',
    minWidth: 130,
    '& fieldset': { borderColor: '#eeeeee' },
    '&:hover fieldset': { borderColor: '#secondary.main' },
    '&.Mui-focused fieldset': { borderColor: '#secondary.main' }
  }

  return (
    <SectionCard
      title='Product Stock'
      subtitle={`${skus.length} products`}
      sx={{
        opacity: isStale ? 0.5 : 1,
        transition: 'opacity 0.2s ease-in-out'
      }}
      action={
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Select
            value={params.stock}
            onChange={(e) => {
              return setParams(prev => ({ ...prev, stock: e.target.value }))
            }}
            size='small'
            sx={selectSx}
          >
            <MenuItem value='all' sx={{ fontSize: '0.78rem' }}>All Status</MenuItem>
            <MenuItem value='in' sx={{ fontSize: '0.78rem' }}>In Stock</MenuItem>
            <MenuItem value='low' sx={{ fontSize: '0.78rem' }}>Low Stock</MenuItem>
            <MenuItem value='out' sx={{ fontSize: '0.78rem' }}>Out of Stock</MenuItem>
          </Select>
        </Box>
      }
    >
      {/* Table header */}
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: '2fr 1fr 1fr 1fr',
          gap: 1,
          px: 1.5,
          py: 1,
          bgcolor: '#f9fafb',
          borderRadius: '8px',
          mb: 1
        }}
      >
        {['Product', 'SKU', 'Stock', 'Status'].map((h) => (
          <Typography key={h} sx={{ fontSize: '0.68rem', fontWeight: 700, color: '#9ca3af', textTransform: 'uppercase', letterSpacing: '0.06em' }}>
            {h}
          </Typography>
        ))}
      </Box>

      {loading ? (
        Array.from({ length: 5 }).map((_, i) => (
          <Box key={i} sx={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr 1fr', gap: 1, px: 1.5, py: 1.25, borderBottom: '1px solid #f3f4f6' }}>
            {[180, 90, 50, 80].map((w, j) => (
              <Skeleton key={j} variant='rounded' width={w} height={16} sx={{ borderRadius: '4px' }} />
            ))}
          </Box>
        ))
      ) : skus.length === 0 ? (
        <EmptyState icon={InventoryRoundedIcon} title='No products found' subtitle='Try changing the filters' />
      ) : (
        skus.map((p) => {
          return <Box
            key={p._id}
            sx={{
              display: 'grid',
              gridTemplateColumns: '2fr 1fr 1fr 1fr',
              gap: 1,
              px: 1.5,
              py: 1.25,
              borderBottom: '1px solid #f9fafb',
              borderRadius: '6px',
              transition: 'bgcolor 0.12s',
              '&:hover': { bgcolor: '#fafafa' }
            }}
          >
            <Box>
              <Typography sx={{ fontSize: '0.82rem', fontWeight: 600, color: '#2d2d2d', lineHeight: 1.3 }}>
                {p.spu_name}
              </Typography>
              <Typography sx={{ fontSize: '0.7rem', color: '#9ca3af' }}>{p.sku_name ? p.sku_name : 'No Variation'}</Typography>
            </Box>
            <Typography sx={{ fontSize: '0.78rem', color: '#6b7280', alignSelf: 'center', fontFamily: 'monospace' }}>
              {p.sku_id}
            </Typography>
            <Typography
              sx={{
                fontSize: '0.85rem',
                fontWeight: 700,
                color: p.sku_stock <= 0 ? '#ef4444' : p.sku_stock <= 10 ? '#f59e0b' : '#2d2d2d',
                alignSelf: 'center'
              }}
            >
              {p.sku_stock}
            </Typography>
            <Box sx={{ alignSelf: 'center' }}>
              <StatusBadge qty={p.sku_stock} />
            </Box>
          </Box>
        })
      )}

      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3, mb: 2 }}>
        <Pagination
          count={totalPages}
          page={params.page}
          onChange={handlePageChange}
          color="primary"
        />
      </Box>

    </SectionCard>
  )
}

export default ProductStockList