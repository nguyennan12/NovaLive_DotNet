import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded'
import { Box, IconButton, Tooltip, Typography } from '@mui/material'
import { formatVND } from '~/common/utils/formatters'

const LiveProductItem = ({ product, onRemove }) => {
  const firstSku = product.skus?.[0]
  const name = product.spuName || product.name || 'Sản phẩm'
  const thumb = product.thumbnailUrl || product.thumb || ''
  const livePrice = product.effectivePrice ?? product.flashPrice ?? firstSku?.live_price ?? product.sellPrice ?? 0
  const originalPrice = product.originalPrice ?? firstSku?.original_price ?? livePrice
  const hasDiscount = originalPrice > livePrice
  const code = product.skuCode || product.code || firstSku?.sku_name || ''
  const targetId = product.skuId || firstSku?.sku_id || product.productId || product.id

  return (
    <Box sx={{
      display: 'flex',
      alignItems: 'center',
      gap: 1.5,
      p: 1.25,
      borderRadius: 2,
      bgcolor: 'rgba(255,255,255,0.04)',
      border: '1px solid rgba(255,255,255,0.07)',
      mb: 1,
      transition: 'background 0.15s ease',
      '&:hover': { bgcolor: 'rgba(255,255,255,0.08)' }
    }}>
      {/* Thumbnail */}
      <Box sx={{
        width: 56, height: 56,
        borderRadius: 1.5,
        overflow: 'hidden',
        flexShrink: 0,
        bgcolor: '#2a2a2a'
      }}>
        <Box
          component="img"
          src={thumb}
          alt={name}
          sx={{ width: '100%', height: '100%', objectFit: 'cover' }}
          onError={(e) => { e.target.style.display = 'none' }}
        />
      </Box>

      {/* Info */}
      <Box sx={{ flex: 1, minWidth: 0 }}>
        <Typography sx={{
          fontSize: '0.82rem', fontWeight: 600, color: '#f0f0f0',
          lineHeight: 1.3, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis'
        }}>
          {name}
        </Typography>

        {code && (
          <Typography sx={{ fontSize: '0.68rem', color: '#aaa', mt: 0.2 }} noWrap>
            {code}
          </Typography>
        )}

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mt: 0.4 }}>
          <Typography sx={{ fontSize: '0.82rem', fontWeight: 700, color: '#3485f7', lineHeight: 1 }}>
            {formatVND(livePrice)}
          </Typography>
          {hasDiscount && (
            <Typography sx={{ fontSize: '0.65rem', color: '#666', textDecoration: 'line-through' }}>
              {formatVND(originalPrice)}
            </Typography>
          )}
        </Box>
      </Box>

      {/* Remove */}
      <Tooltip title="Xoá khỏi live" placement="left">
        <IconButton
          size="small"
          onClick={() => onRemove(targetId)}
          sx={{
            color: '#ff5f5f',
            flexShrink: 0,
            width: 32, height: 32,
            '&:hover': { bgcolor: 'rgba(255,95,95,0.12)' }
          }}
        >
          <DeleteOutlineRoundedIcon fontSize="small" />
        </IconButton>
      </Tooltip>
    </Box>
  )
}

export default LiveProductItem
