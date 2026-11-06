import AddIcon from '@mui/icons-material/Add'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import CloseIcon from '@mui/icons-material/Close'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import ShoppingBagRoundedIcon from '@mui/icons-material/ShoppingBagRounded'
import {
  Box, Button, CircularProgress, Dialog, DialogActions, DialogContent,
  DialogTitle, IconButton, InputAdornment, TextField, Typography
} from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { getSellerProductsAPI } from '~/common/apis/services/productService'
import { useDebounce } from '~/common/hooks/useDebounce'
import { formatVND } from '~/common/utils/formatters'

const SkuCard = ({ item, added, selected, onToggle, isAdding }) => {
  const thumb = item.thumbnailUrl || item.spu_thumb
  const name = item.name || item.spu_name
  const firstSku = item.skus?.[0] || item.sku_list?.[0]
  const price = firstSku?.sellPrice ?? firstSku?.price ?? item.minPrice ?? item.price ?? 0
  const stock = firstSku?.availableQty ?? firstSku?.qtyOnHand ?? item.spu_quantity ?? 0
  let variantText = ''
  try {
    if (firstSku?.attributesJson && firstSku.attributesJson !== '{}') {
      const parsed = JSON.parse(firstSku.attributesJson)
      variantText = Object.values(parsed).join(' · ')
    }
  } catch {
    variantText = firstSku?.skuCode || ''
  }

  return (
    <Box sx={{
      display: 'flex',
      alignItems: 'center',
      gap: 1.5,
      p: 1.25,
      borderRadius: 2,
      border: '1px solid',
      borderColor: added || selected ? 'rgba(52,133,247,0.55)' : 'rgba(0,0,0,0.08)',
      bgcolor: added || selected ? 'rgba(52,133,247,0.06)' : '#fafafa',
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
        {thumb ? (
          <Box
            component="img"
            src={thumb}
            alt={name}
            sx={{ width: '100%', height: '100%', objectFit: 'cover' }}
            onError={(e) => { e.target.style.display = 'none' }}
          />
        ) : (
          <Box sx={{ width: '100%', height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#bbb' }}>
            <ShoppingBagRoundedIcon fontSize="small" />
          </Box>
        )}
      </Box>

      {/* Info */}
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Typography sx={{
          fontSize: '0.82rem', fontWeight: 600, color: '#1a1a1a', lineHeight: 1.3
        }} noWrap>
          {name}
        </Typography>

        {variantText && (
          <Typography sx={{ fontSize: '0.7rem', color: '#777', mt: 0.2 }} noWrap>
            {variantText}
          </Typography>
        )}

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mt: 0.35 }}>
          <Typography sx={{ fontSize: '0.85rem', fontWeight: 700, color: '#3485f7' }}>
            {formatVND(price)}
          </Typography>
          <Typography sx={{ fontSize: '0.65rem', color: '#aaa' }}>
            · Tồn: {stock}
          </Typography>
        </Box>
      </Box>

      {/* Nút thêm / đã thêm */}
      <Box sx={{ flexShrink: 0 }}>
        {added || selected ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 0.2, color: '#3485f7' }}>
            <IconButton
              onClick={() => !added && onToggle(item)}
              disabled={added || isAdding}
              size="small"
              sx={{ color: '#3485f7', p: 0.25 }}
            >
              <CheckCircleIcon sx={{ fontSize: 24 }} />
            </IconButton>
            <Typography sx={{ fontSize: '0.6rem', fontWeight: 600 }}>{added ? 'Đã thêm' : 'Đã chọn'}</Typography>
          </Box>
        ) : (
          <IconButton
            onClick={() => onToggle(item)}
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
}

const AddProductModal = ({ open, onClose, liveProducts = [], onAddProduct, isAdding }) => {
  const [search, setSearch] = useState('')
  const [selected, setSelected] = useState({})
  const keyword = useDebounce(search, 300)

  const handleClose = () => {
    if (isAdding) return
    setSearch('')
    setSelected({})
    onClose()
  }

  const { data, isLoading } = useQuery({
    queryKey: ['seller-products-for-live', keyword],
    queryFn: () => getSellerProductsAPI({ keyword, limit: 50 }),
    enabled: open,
    staleTime: 30 * 1000
  })
  const products = data?.items ?? []

  const isSkuAdded = (item) => {
    const firstSku = item.skus?.[0] || item.sku_list?.[0]
    const skuId = (firstSku?.id || firstSku?._id || item.id || item._id)?.toString()
    const spuId = (item.id || item._id)?.toString()

    return liveProducts.some(lp => {
      const lpSkuId = (lp.skuId || lp.skus?.[0]?.skuId || lp.skus?.[0]?.id)?.toString()
      const lpSpuId = (lp.spuId || lp.productId)?.toString()
      return (skuId && lpSkuId === skuId) || (spuId && lpSpuId === spuId)
    })
  }

  const toRequest = (item) => {
    const firstSku = item.skus?.[0] || item.sku_list?.[0]
    const skuId = firstSku?.id || firstSku?._id || item.id || item._id
    const price = firstSku?.sellPrice ?? firstSku?.price ?? item.minPrice ?? item.price ?? 0

    return {
      skuId,
      productId: item.id || item._id,
      flashPrice: price,
      durationSeconds: 120
    }
  }

  const itemKey = (item) => String(toRequest(item).skuId)
  const selectedItems = useMemo(() => Object.values(selected), [selected])

  const toggleSelected = (item) => {
    const key = itemKey(item)
    setSelected(current => {
      const next = { ...current }
      if (next[key]) delete next[key]
      else next[key] = item
      return next
    })
  }

  const handleAddSelected = async () => {
    if (selectedItems.length === 0) return
    try {
      await onAddProduct(selectedItems.map(toRequest))
      setSelected({})
    } catch {
      // Mutation displays the API error and keeps the selection for retry.
    }
  }

  return (
    <Dialog
      open={open}
      onClose={handleClose}
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
        <IconButton onClick={handleClose} size="small" sx={{ color: '#cacacaff' }}>
          <CloseIcon fontSize="small" />
        </IconButton>
      </DialogTitle>

      {/* Search bar */}
      <Box sx={{ px: 3, pb: 1.5 }}>
        <TextField
          fullWidth
          placeholder="Tìm kiếm sản phẩm trong shop..."
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
        ) : products.length === 0 ? (
          <Box sx={{ py: 6, textAlign: 'center' }}>
            <ShoppingBagRoundedIcon sx={{ fontSize: 44, color: '#e0e0e0', mb: 1.5 }} />
            <Typography sx={{ color: '#aaa', fontSize: '0.85rem' }}>
              {keyword ? 'Không tìm thấy sản phẩm phù hợp' : 'Shop chưa có sản phẩm nào'}
            </Typography>
          </Box>
        ) : (
          products.map((item) => (
            <SkuCard
              key={(item.id || item._id)?.toString()}
              item={item}
              added={isSkuAdded(item)}
              selected={!!selected[itemKey(item)]}
              onToggle={toggleSelected}
              isAdding={isAdding}
            />
          ))
        )}
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2, borderTop: '1px solid #eee' }}>
        <Button onClick={handleClose} disabled={isAdding} sx={{ textTransform: 'none' }}>Đóng</Button>
        <Button
          variant="contained"
          onClick={handleAddSelected}
          disabled={selectedItems.length === 0 || isAdding}
          sx={{ textTransform: 'none', borderRadius: 2, minWidth: 150 }}
        >
          {isAdding ? <CircularProgress size={18} color="inherit" /> : `Thêm ${selectedItems.length} sản phẩm`}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default AddProductModal
