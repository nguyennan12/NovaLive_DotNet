import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined'
import { Box, Chip, Typography } from '@mui/material'
import { LOGO } from '~/common/utils/constant'
import { formatVND } from '~/common/utils/formatters'
import { glassSx } from '~/theme'

// TODO: Thay SHIPPING_POLICIES bằng data từ API chính sách vận chuyển khi backend có
// TODO: Tích hợp API GHN/GHTK để tính phí ship động theo địa chỉ user
const SHIPPING_POLICIES = [
  'Miễn phí vận chuyển cho đơn hàng từ 500.000 ₫ khi áp dụng voucher freeship.',
  'Giao hàng nhanh (1–3 ngày làm việc): 25.000 ₫',
  'Giao hàng tiêu chuẩn (4–7 ngày): Từ 15.000 ₫',
  'Hỗ trợ đổi trả miễn phí trong 7 ngày nếu hàng lỗi từ nhà sản xuất.',
  'Đóng gói cẩn thận, phụ phí bảo vệ hàng dễ vỡ: +10.000 ₫.'
]

function OrderShippingInfo({ feeShip = 0, hasFreeShip = false, amountFreeShip = 0 }) {
  return (
    <Box sx={{
      bgcolor: 'primary.main', ...glassSx,
      borderRadius: 3, p: 2.5,
      border: '1px solid', borderColor: 'divider'
    }}>
      {/* Section header */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.75 }}>
        <Box sx={{ width: 3.5, height: 22, borderRadius: '2px', bgcolor: '#f59e0b', flexShrink: 0 }} />
        <Typography sx={{ fontWeight: 700, fontSize: '0.93rem', color: '#f59e0b' }}>
          Thông tin vận chuyển
        </Typography>
      </Box>

      {/* Đơn vị vận chuyển + phí */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 2 }}>
        <Box sx={{
          width: 34, height: 34, borderRadius: 1.5, flexShrink: 0,
          bgcolor: '#d97706',
          display: 'flex', alignItems: 'center', justifyContent: 'center',
          overflow: 'hidden'
        }}>
          <Box
            component="img"
            src={LOGO.GHN}
            alt={'GHN'}
            sx={{ width: '100%', height: '100%', objectFit: 'contain' }}
          />
        </Box>

        <Box sx={{ flex: 1 }}>
          <Typography sx={{ fontSize: '0.84rem', fontWeight: 700, color: 'primary.contrastText', mb: 0.3 }}>
            Giao Hàng Nhanh (GHN)
          </Typography>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
            {hasFreeShip ? (
              <>
                <Chip
                  label="FREESHIP"
                  size="small"
                  sx={{
                    height: 19, fontSize: '0.63rem', fontWeight: 800,
                    bgcolor: 'rgba(245,158,11,0.12)', color: '#d97706',
                    border: '1px solid rgba(245,158,11,0.3)',
                    '& .MuiChip-label': { px: 0.8 }
                  }}
                />
                <Typography sx={{
                  fontSize: '0.78rem', color: 'rgba(45,45,45,0.45)',
                  textDecoration: 'line-through'
                }}>
                  {formatVND(feeShip + amountFreeShip)}
                </Typography>
                <Typography sx={{ fontSize: '0.85rem', fontWeight: 800, color: '#d97706' }}>
                  {formatVND(feeShip)}
                </Typography>
              </>
            ) : (
              <>
                <Typography sx={{ fontSize: '0.85rem', fontWeight: 700, color: '#d97706' }}>
                  {formatVND(feeShip)}
                </Typography>
                <Typography sx={{ fontSize: '0.72rem', color: 'rgba(45,45,45,0.45)' }}>
                  · Dự kiến 1–3 ngày
                </Typography>
              </>
            )}
          </Box>
        </Box>
      </Box>

      {/* Chính sách vận chuyển */}
      <Box sx={{
        bgcolor: 'rgba(245,158,11,0.04)',
        borderRadius: 2, p: 1.5,
        border: '1px dashed rgba(245,158,11,0.28)'
      }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mb: 1 }}>
          <InfoOutlinedIcon sx={{ fontSize: 13, color: '#d97706' }} />
          <Typography sx={{
            fontSize: '0.7rem', fontWeight: 800, color: '#d97706',
            textTransform: 'uppercase', letterSpacing: '0.05em'
          }}>
            Chính sách vận chuyển
          </Typography>
        </Box>

        <Box component="ul" sx={{ m: 0, pl: 2, display: 'flex', flexDirection: 'column', gap: 0.35 }}>
          {SHIPPING_POLICIES.map((policy, i) => (
            <Typography
              key={i} component="li"
              sx={{ fontSize: '0.75rem', color: 'rgba(45,45,45,0.62)', lineHeight: 1.55 }}
            >
              {policy}
            </Typography>
          ))}
        </Box>
      </Box>
    </Box>
  )
}

export default OrderShippingInfo
