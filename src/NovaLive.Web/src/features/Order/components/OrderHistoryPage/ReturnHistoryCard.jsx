import { Box, Button, Chip, Divider, Typography } from '@mui/material'
import AssignmentReturnRoundedIcon from '@mui/icons-material/AssignmentReturnRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import { formatDate, formatVND } from '~/common/utils/formatters'
import { glassSx } from '~/theme'
import OrderThumbnail from '../shared/OrderThumbnail'

export const RETURN_STATUS_CONFIG = {
  Pending: {
    label: 'Chờ Shop xử lý',
    color: '#f59e0b',
    bgColor: 'rgba(245,158,11,0.1)',
    borderColor: 'rgba(245,158,11,0.25)'
  },
  SellerApproved: {
    label: 'Shop đồng ý gửi hàng',
    color: '#0095ff',
    bgColor: 'rgba(0,149,255,0.1)',
    borderColor: 'rgba(0,149,255,0.25)'
  },
  SellerRejected: {
    label: 'Shop từ chối',
    color: '#dc2626',
    bgColor: 'rgba(220,38,38,0.1)',
    borderColor: 'rgba(220,38,38,0.25)'
  },
  AdminDispute: {
    label: 'Đang Admin phân xử',
    color: '#8b5cf6',
    bgColor: 'rgba(139,92,246,0.1)',
    borderColor: 'rgba(139,92,246,0.25)'
  },
  Completed: {
    label: 'Hoàn tiền thành công',
    color: '#16a34a',
    bgColor: 'rgba(22,163,74,0.1)',
    borderColor: 'rgba(22,163,74,0.25)'
  },
  BuyerWins: {
    label: 'Buyer thắng đang hoàn tiền',
    color: '#16a34a',
    bgColor: 'rgba(22,163,74,0.1)',
    borderColor: 'rgba(22,163,74,0.25)'
  },
  SellerWins: {
    label: 'Shop thắng',
    color: '#64748b',
    bgColor: 'rgba(100,116,139,0.1)',
    borderColor: 'rgba(100,116,139,0.25)'
  }
}

export const REASON_MAP = {
  WrongItem: 'Giao sai sản phẩm / mẫu mã',
  Defective: 'Sản phẩm lỗi kỹ thuật',
  DamagedInShipping: 'Hư hỏng khi vận chuyển',
  NotAsDescribed: 'Không đúng mô tả',
  ChangeOfMind: 'Đổi ý định mua'
}

export default function ReturnHistoryCard({ item, onViewDetail }) {
  const cfg = RETURN_STATUS_CONFIG[item.status] || {
    label: item.status || 'Đang xử lý',
    color: '#64748b',
    bgColor: 'rgba(100,116,139,0.1)',
    borderColor: 'rgba(100,116,139,0.25)'
  }

  const items = item.items || []
  const firstItem = items[0]
  const remainCount = items.length - 1
  const thumbnail = item.evidenceUrls?.[0]

  return (
    <Box sx={{
      borderRadius: 3,
      mb: 2,
      border: '1px solid',
      borderColor: 'divider',
      ...glassSx,
      overflow: 'hidden',
      transition: 'box-shadow 0.2s',
      '&:hover': { boxShadow: '0 4px 20px rgba(0,0,0,0.08)' }
    }}>
      {/* Header */}
      <Box sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 1,
        px: 2,
        py: 1.25,
        bgcolor: 'rgba(83,155,255,0.04)',
        borderBottom: '1px solid',
        borderColor: 'divider'
      }}>
        <StorefrontRoundedIcon sx={{ fontSize: 16, color: 'secondary.main', flexShrink: 0 }} />
        <Typography sx={{ fontSize: '0.82rem', fontWeight: 700, color: 'primary.contrastText', flex: 1 }}>
          {item.shopName || 'Nova Flagship Store'}
        </Typography>
        <Typography sx={{ fontSize: '0.72rem', color: 'rgba(45,45,45,0.45)', mr: 1 }}>
          {formatDate(item.createdAt)}
        </Typography>
        <Chip
          size="small"
          icon={<AssignmentReturnRoundedIcon sx={{ fontSize: '13px !important', color: `${cfg.color} !important` }} />}
          label={cfg.label}
          sx={{
            height: 22,
            fontSize: '0.7rem',
            fontWeight: 700,
            bgcolor: cfg.bgColor,
            color: cfg.color,
            border: '1px solid',
            borderColor: cfg.borderColor,
            '& .MuiChip-label': { px: 0.9 }
          }}
        />
      </Box>

      {/* Product items */}
      <Box sx={{ p: 2, display: 'flex', gap: 1.5, alignItems: 'flex-start' }}>
        <OrderThumbnail src={thumbnail} alt={firstItem?.spuName || 'Sản phẩm hoàn'} width={68} height={68} />

        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Typography sx={{
            fontSize: '0.88rem',
            fontWeight: 600,
            color: 'primary.contrastText',
            lineHeight: 1.3,
            mb: 0.5,
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical'
          }}>
            {firstItem?.spuName || 'Yêu cầu trả hàng / hoàn tiền'}
          </Typography>

          <Typography sx={{ fontSize: '0.75rem', color: 'rgba(45,45,45,0.5)', mb: 0.5 }}>
            Lý do: <strong>{REASON_MAP[item.reason] || item.reason || 'Yêu cầu hoàn trả'}</strong>
            {firstItem?.skuCode ? ` • SKU: ${firstItem.skuCode}` : ''}
          </Typography>

          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 0.5 }}>
            <Typography sx={{ fontSize: '0.78rem', color: 'rgba(45,45,45,0.6)' }}>
              x{firstItem?.quantity || 1}
              {remainCount > 0 ? ` (+${remainCount} sản phẩm khác)` : ''}
            </Typography>
            <Typography sx={{ fontSize: '0.84rem', fontWeight: 700, color: '#ef4444' }}>
              {formatVND(firstItem?.refundPrice || item.requestedRefundAmount || 0)}
            </Typography>
          </Box>
        </Box>
      </Box>

      {/* Footer */}
      <Divider />
      <Box sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        px: 2,
        py: 1.25,
        bgcolor: 'rgba(83,155,255,0.02)',
        flexWrap: 'wrap',
        gap: 1
      }}>
        <Typography sx={{ fontSize: '0.75rem', color: 'rgba(45,45,45,0.45)', fontFamily: 'monospace' }}>
          #{item.subOrderCode || String(item.id).substring(0, 8)}
        </Typography>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mr: 'auto', ml: { sm: 2 } }}>
          <Typography sx={{ fontSize: '0.75rem', color: 'rgba(45,45,45,0.5)' }}>
            Yêu cầu hoàn:
          </Typography>
          <Typography sx={{ fontSize: '0.92rem', fontWeight: 800, color: '#ef4444' }}>
            {formatVND(item.requestedRefundAmount || 0)}
          </Typography>
        </Box>

        <Button
          size="small"
          onClick={() => onViewDetail(item)}
          sx={{
            fontSize: '0.76rem',
            fontWeight: 600,
            py: 0.5,
            px: 1.5,
            bgcolor: 'secondary.main',
            color: '#fff',
            borderRadius: '8px',
            '&:hover': { bgcolor: '#2b77f5' }
          }}
        >
          Chi tiết
        </Button>
      </Box>
    </Box>
  )
}
