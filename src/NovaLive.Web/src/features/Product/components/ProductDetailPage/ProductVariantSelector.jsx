import CheckRoundedIcon from '@mui/icons-material/CheckRounded'
import { Box, Typography } from '@mui/material'
import { QuantityStepper } from '~/features/Cart/components/shared/QuantityStepper'

// Variant selection theo sku_tier_idx sẽ được wired qua API sau
const ProductVariantSelector = ({ skuList = [], selectedSkuId, onSelect, thumbFallback }) => {
  if (!skuList.length) return null

  return (
    <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: { xs: 1.5, sm: 2 } }}>
      {/* Label */}
      <Typography sx={{
        width: 72, flexShrink: 0,
        fontSize: '0.82rem', fontWeight: 600, color: '#9ca3af',
        pt: 1.2, lineHeight: 1.4
      }}>
        Phân Loại
      </Typography>

      {/* Card grid: 2 cols */}
      <Box sx={{
        display: 'grid',
        gridTemplateColumns: 'repeat(3, 1fr)',
        gap: 1,
        flex: 1
      }}>
        {skuList.map((sku) => {
          const isSelected = sku.sku_id === selectedSkuId
          const imgSrc = sku.sku_thumb || thumbFallback

          return (
            <Box
              key={sku.sku_id ?? sku._id}
              onClick={() => onSelect(sku.sku_id ?? sku._id)}
              sx={{
                display: 'flex', alignItems: 'center', gap: 1,
                p: '8px 10px',
                borderRadius: '10px',
                border: '1.5px solid',
                borderColor: isSelected ? 'secondary.main' : '#e0e0e0',
                bgcolor: isSelected ? 'rgba(52,133,247,0.06)' : '#fafafa',
                cursor: 'pointer',
                position: 'relative',
                transition: 'all 0.15s',
                '&:hover': {
                  borderColor: 'secondary.main',
                  bgcolor: 'rgba(52,133,247,0.04)'
                }
              }}
            >
              {/* Thumbnail */}
              {imgSrc ? (
                <Box
                  component="img"
                  src={imgSrc}
                  alt={sku.sku_name}
                  sx={{ width: 36, height: 36, borderRadius: '6px', objectFit: 'cover', flexShrink: 0 }}
                />
              ) : (
                <Box sx={{ width: 36, height: 36, borderRadius: '6px', bgcolor: '#f0f4ff', flexShrink: 0 }} />
              )}

              {/* SKU name */}
              <Typography sx={{
                fontSize: '0.78rem', fontWeight: isSelected ? 700 : 500,
                color: isSelected ? 'secondary.main' : '#2d2d2d',
                lineHeight: 1.35,
                overflow: 'hidden',
                display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical'
              }}>
                {sku.sku_name || sku.sku_id}
              </Typography>

              {/* Checkmark badge */}
              {isSelected && (
                <Box sx={{
                  position: 'absolute', bottom: 4, right: 4,
                  width: 16, height: 16, borderRadius: '50%',
                  bgcolor: 'secondary.main',
                  display: 'flex', alignItems: 'center', justifyContent: 'center'
                }}>
                  <CheckRoundedIcon sx={{ fontSize: 11, color: '#fff' }} />
                </Box>
              )}
            </Box>
          )
        })}
      </Box>
    </Box>
  )
}

export const SkuPriceLine = ({ selectedSku, quantity, onChangeQuantity }) => {
  const stock = selectedSku?.sku_stock
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap', mt: 1 }}>
      <Typography sx={{ fontSize: '0.78rem', color: '#aaa', fontWeight: 500 }}>
        Phân loại: <strong style={{ color: '#555' }}>{selectedSku?.sku_name ?? '—'}</strong>
      </Typography>
      {stock !== undefined && (
        <Typography sx={{
          fontSize: '0.78rem', fontWeight: 600,
          color: stock === 0 ? '#ef4444' : stock <= 10 ? '#f59e0b' : '#22c55e'
        }}>
          {stock === 0 ? 'Hết hàng' : stock <= 10 ? `Còn ${stock} sản phẩm` : `Còn hàng (${stock})`}
        </Typography>
      )}
      <QuantityStepper value={quantity} onChange={onChangeQuantity} />
    </Box>
  )
}

export default ProductVariantSelector
