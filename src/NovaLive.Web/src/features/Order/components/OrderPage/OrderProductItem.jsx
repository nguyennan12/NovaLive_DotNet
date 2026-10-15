import StorefrontOutlinedIcon from '@mui/icons-material/StorefrontOutlined'
import { Avatar, Box, Chip, Typography } from '@mui/material'
import { formatVND } from '~/common/utils/formatters'

function OrderProductItem({ item }) {
  const skuLabel = item.skuName
    ? Object.values(item.skuName).filter(Boolean).join(' / ')
    : null

  return (
    <Box sx={{ display: 'flex', gap: 1.5, py: 1.75, alignItems: 'flex-start' }}>
      {/* Ảnh sản phẩm */}
      <Avatar
        src={item.image}
        variant="rounded"
        alt={item.name}
        sx={{ width: 72, height: 72, borderRadius: 2, flexShrink: 0, bgcolor: '#f0f4ff' }}
      />

      <Box sx={{ flex: 1, minWidth: 0 }}>
        {/* Tên sản phẩm */}
        <Typography sx={{
          fontSize: '0.875rem', fontWeight: 600, lineHeight: 1.4, mb: 0.4,
          display: '-webkit-box', WebkitLineClamp: 2,
          WebkitBoxOrient: 'vertical', overflow: 'hidden'
        }}>
          {item.name || 'Sản phẩm'}
        </Typography>

        {/* SKU variation */}
        {skuLabel && (
          <Chip
            label={skuLabel}
            size="small"
            sx={{
              fontSize: '0.68rem', height: 20, mb: 0.6,
              bgcolor: 'rgba(83,155,255,0.1)', color: 'secondary.main', fontWeight: 600,
              '& .MuiChip-label': { px: 0.9 }
            }}
          />
        )}

        {/* Tên shop */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.4, mb: 0.6 }}>
          <StorefrontOutlinedIcon sx={{ fontSize: 12, color: 'rgba(45,45,45,0.4)' }} />
          <Typography sx={{ fontSize: '0.72rem', color: 'rgba(45,45,45,0.5)' }}>
            {item.shopName || 'Shop'}
          </Typography>
        </Box>

        {/* Giá + số lượng */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography sx={{ fontSize: '0.93rem', fontWeight: 700, color: 'secondary.main' }}>
            {formatVND(item.price)}
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography sx={{ fontSize: '0.76rem', color: 'rgba(45,45,45,0.45)' }}>
              Số lượng:
            </Typography>
            <Typography sx={{ fontSize: '0.84rem', fontWeight: 700, color: 'primary.contrastText' }}>
              {item.quantity}
            </Typography>
          </Box>
        </Box>

        {/* Thành tiền */}
        <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 0.5 }}>
          <Typography sx={{ fontSize: '0.78rem', color: 'rgba(45,45,45,0.5)' }}>
            Thành tiền:&nbsp;
            <Box component="span" sx={{ fontWeight: 700, color: 'primary.contrastText' }}>
              {formatVND(item.price * item.quantity)}
            </Box>
          </Typography>
        </Box>
      </Box>
    </Box>
  )
}

export default OrderProductItem
