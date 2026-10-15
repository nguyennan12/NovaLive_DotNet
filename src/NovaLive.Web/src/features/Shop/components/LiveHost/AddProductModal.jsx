import AddIcon from '@mui/icons-material/Add'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CloseIcon from '@mui/icons-material/Close'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import ShoppingBagRoundedIcon from '@mui/icons-material/ShoppingBagRounded'
import {
  Box, CircularProgress, Dialog, DialogContent,
  DialogTitle, IconButton, InputAdornment, TextField, Typography
} from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { getAllProductWithStockAPI } from '~/common/apis/services/productService'
import { useDebounce } from '~/common/hooks/useDebounce'
import { formatVND } from '~/common/utils/formatters'

const SkuCard = ({ sku, added, onAdd, isAdding }) => (
  <Box sx={{
    display: 'flex',
    alignItems: 'center',
    gap: 1.5,
    p: 1.25,
    borderRadius: 2,
    border: '1px solid',
    borderColor: added ? 'rgba(52,133,247,0.35)' : 'rgba(0,0,0,0.08)',
    bgcolor: added ? 'rgba(52,133,247,0.05)' : '#fafafa',
    mb: 1,
    transition: 'all 0.15s ease'
  }}>
    {/* Thumbnail */}
    <Box sx={{
      width: 60, height: 60,
      borderRadius: 1.5,
      overflow: 'hidden',
      flexShrink: 0,
      bgcolor: '#f0f0f0'
    }}>
      <Box
        component="img"
        src={sku.spu_thumb}
        alt={sku.spu_name}
        sx={{ width: '100%', height: '100%', objectFit: 'cover' }}
        onError={(e) => { e.target.style.display = 'none' }}
      />
    </Box>

    {/* Info */}
    <Box sx={{ flex: 1, minWidth: 0 }}>
      <Typography sx={{
        fontSize: '0.82rem', fontWeight: 600, color: '#1a1a1a', lineHeight: 1.3
      }} noWrap>
        {sku.spu_name}
      </Typography>

      {sku.sku_name && (
        <Typography sx={{ fontSize: '0.7rem', color: '#777', mt: 0.2 }} noWrap>
          {sku.sku_name}
        </Typography>
      )}

      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mt: 0.35 }}>
        <Typography sx={{ fontSize: '0.85rem', fontWeight: 700, color: '#3485f7' }}>
          {formatVND(sku.sku_price)}
        </Typography>
        <Typography sx={{ fontSize: '0.65rem', color: '#aaa' }}>
          · Tồn: {sku.sku_stock}
        </Typography>
      </Box>
    </Box>

    {/* Nút thêm / đã thêm */}
    <Box sx={{ flexShrink: 0 }}>
      {added ? (
        <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 0.2, color: '#3485f7' }}>
          <CheckCircleIcon sx={{ fontSize: 22 }} />
          <Typography sx={{ fontSize: '0.6rem', fontWeight: 600 }}>Đã thêm</Typography>
        </Box>
      ) : (
        <IconButton
          onClick={() => onAdd(sku)}
          disabled={isAdding}
          size="small"
          sx={{
            bgcolor: '#5fa2ffff',
            color: '#ffffffff !important',
            width: 34, height: 34,
            '&:hover': { bgcolor: '#1a6de0' },
            '&.Mui-disabled': { bgcolor: 'rgba(52,133,247,0.4)', color: '#fff !important' }
          }}
        >
          {isAdding
            ? <CircularProgress size={14} color="inherit" />
            : <AddIcon fontSize="small" />
          }
        </IconButton>
      )}
    </Box>
  </Box>
)

const AddProductModal = ({ open, onClose, liveProducts = [], onAddProduct, isAdding }) => {
  const [search, setSearch] = useState('')
  const keyword = useDebounce(search, 300)

  // Reset search khi đóng modal

  const queryString = new URLSearchParams({
    keyword: keyword,
    limit: 20,
    page: 1
  }).toString()

  const { data, isLoading } = useQuery({
    queryKey: ['skus-live-search', keyword],
    queryFn: () => getAllProductWithStockAPI(queryString),
    enabled: open,
    staleTime: 60 * 1000
  })
  const skus = data?.items ?? []

  // Check theo skuId cụ thể
  const isSkuAdded = (sku) => {
    const skuId = sku._id?.toString()
    return liveProducts.some(lp =>
      lp.skus?.some(s => s.skuId?.toString() === skuId)
    )
  }

  const handleAdd = (sku) => {
    onAddProduct([{
      spu_id: sku.sku_spuId?.toString(),
      is_featured: false,
      skus: [{
        sku_id: sku._id?.toString(),
        live_price: sku.sku_price
      }]
    }])
  }

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: { borderRadius: 3, maxHeight: '82vh' }
      }}
    >
      <DialogTitle sx={{
        pb: 1.5,
        display: 'flex', alignItems: 'center', justifyContent: 'space-between'
      }}>
        <Typography fontWeight={700} fontSize="0.98rem">
          Thêm sản phẩm vào live
        </Typography>
        <IconButton onClick={onClose} size="small" sx={{ color: '#cacacaff' }}>
          <CloseIcon fontSize="small" />
        </IconButton>
      </DialogTitle>

      {/* Search bar */}
      <Box sx={{ px: 3, pb: 1.5 }}>
        <TextField
          fullWidth
          placeholder="Tìm kiếm sản phẩm..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          size="small"
          autoFocus
          slotProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchRoundedIcon sx={{ color: '#bbb', fontSize: 20 }} />
              </InputAdornment>
            )
          }}
          sx={{
            '& .MuiOutlinedInput-root': {
              borderRadius: 2,
              '& fieldset': { borderColor: '#e5e5e5' },
              '&:hover fieldset': { borderColor: '#3485f7' },
              '&.Mui-focused fieldset': { borderColor: '#3485f7' }
            }
          }}
        />
      </Box>

      <DialogContent sx={{ pt: 0.5 }}>
        {isLoading ? (
          <Box sx={{ py: 5, display: 'flex', justifyContent: 'center' }}>
            <CircularProgress size={32} sx={{ color: '#3485f7' }} />
          </Box>
        ) : skus.length === 0 ? (
          <Box sx={{ py: 6, textAlign: 'center' }}>
            <ShoppingBagRoundedIcon sx={{ fontSize: 44, color: '#e0e0e0', mb: 1.5 }} />
            <Typography sx={{ color: '#aaa', fontSize: '0.85rem' }}>
              {keyword ? 'Không tìm thấy sản phẩm phù hợp' : 'Shop chưa có sản phẩm nào'}
            </Typography>
          </Box>
        ) : (
          skus.map((sku) => (
            <SkuCard
              key={sku._id?.toString()}
              sku={sku}
              added={isSkuAdded(sku)}
              onAdd={handleAdd}
              isAdding={isAdding}
            />
          ))
        )}
      </DialogContent>
    </Dialog>
  )
}

export default AddProductModal
