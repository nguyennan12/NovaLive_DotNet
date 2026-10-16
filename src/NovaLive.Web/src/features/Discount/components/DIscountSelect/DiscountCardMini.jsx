import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import ContentCopyRoundedIcon from '@mui/icons-material/ContentCopyRounded'
import { Box, Button, CircularProgress, Tooltip, Typography } from '@mui/material'
import { formatVND } from '~/common/utils/formatters'
import { CATEGORY_THEME, fmtValue } from '../../utils/normalizeDiscount'

export function DiscountCardMini({ discount, selected, onSelect, subtotal = 0, loading = false }) {
  const { name, code, category = 'product', type, value, minOrder, status } = discount
  const theme = CATEGORY_THEME[category] ?? CATEGORY_THEME.product
  const Icon = theme.icon
  const isExpired = status === 'expired'
  const meetsMinOrder = !minOrder || subtotal >= minOrder
  const isDisabled = isExpired || !meetsMinOrder || loading
  const handleCopy = (e) => {
    e.stopPropagation()
    if (code) navigator.clipboard.writeText(code)
  }

  return (
    <Box
      onClick={!isDisabled ? onSelect : undefined}
      sx={{
        display: 'flex',
        borderRadius: '10px',
        bgcolor: '#fff',
        overflow: 'hidden',
        boxShadow: selected
          ? '0 0 0 2px #3485f7, 0 2px 10px rgba(52,133,247,0.18)'
          : '0 1px 6px rgba(0,0,0,0.07), 0 0 0 1px rgba(0,0,0,0.05)',
        opacity: isDisabled ? 0.52 : 1,
        cursor: isDisabled ? 'default' : 'pointer',
        transition: 'all 0.15s',
        WebkitMaskImage: `
          radial-gradient(circle at 27% 0, transparent 8px, black 9px),
          radial-gradient(circle at 27% 100%, transparent 8px, black 9px)
        `,
        WebkitMaskComposite: 'destination-in',
        maskComposite: 'intersect',
        '&:hover': isDisabled ? {} : {
          transform: 'translateY(-1px)',
          boxShadow: selected
            ? '0 0 0 2px #3485f7, 0 4px 14px rgba(52,133,247,0.22)'
            : '0 3px 14px rgba(0,0,0,0.1)'
        }
      }}
    >
      {/* Left stub */}
      <Box sx={{
        width: '26%', flexShrink: 0,
        display: 'flex', flexDirection: 'column',
        alignItems: 'center', justifyContent: 'center',
        bgcolor: theme.stubBg, py: 1.5, px: 0.5, gap: 0.4,
        position: 'relative',
        '&::after': {
          content: '""', position: 'absolute', right: 0, top: '10px', bottom: '10px',
          borderRight: `2px dashed ${theme.dash}`
        }
      }}>
        <Icon sx={{ fontSize: 13, color: theme.stubText }} />
        <Typography sx={{
          fontSize: category === 'freeship' ? '0.82rem' : '1rem',
          fontWeight: 900, color: theme.stubText,
          letterSpacing: '-0.03em', lineHeight: 1, textAlign: 'center'
        }}>
          {fmtValue(category, type, value)}
        </Typography>
      </Box>

      {/* Right content */}
      <Box sx={{ flex: 1, px: 1.25, py: 1, display: 'flex', flexDirection: 'column', justifyContent: 'space-between', minWidth: 0 }}>
        <Box>
          <Typography noWrap sx={{ fontSize: '0.77rem', fontWeight: 700, color: '#1a1a1a', lineHeight: 1.35 }}>
            {name}
          </Typography>

          {code && (
            <Tooltip title="Click để copy mã">
              <Box
                onClick={handleCopy}
                sx={{
                  display: 'inline-flex', alignItems: 'center', gap: 0.4,
                  mt: 0.35, px: 0.65, py: 0.1,
                  bgcolor: '#f3f4f6', borderRadius: '4px',
                  border: '1.5px dashed #d1d5db', cursor: 'pointer',
                  '&:hover': { bgcolor: '#e5e7eb' }
                }}
              >
                <Typography sx={{ fontSize: '0.6rem', fontWeight: 700, color: '#374151', letterSpacing: '0.07em', fontFamily: 'monospace' }}>
                  {code}
                </Typography>
                <ContentCopyRoundedIcon sx={{ fontSize: 9, color: '#6b7280' }} />
              </Box>
            </Tooltip>
          )}
        </Box>

        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: 0.5, gap: 0.5 }}>
          <Typography sx={{
            fontSize: '0.6rem',
            color: !meetsMinOrder && !isExpired ? '#ef4444' : '#9ca3af',
            flexShrink: 0
          }}>
            {isExpired
              ? 'Đã hết hạn'
              : minOrder > 0
                ? `Đơn tối thiểu ${formatVND(minOrder)}`
                : 'Không giới hạn đơn'
            }
          </Typography>

          {loading ? (
            <CircularProgress size={16} sx={{ color: 'secondary.main', flexShrink: 0 }} />
          ) : selected ? (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.35, flexShrink: 0 }}>
              <CheckCircleRoundedIcon sx={{ fontSize: 16, color: 'secondary.main' }} />
            </Box>
          ) : (
            <Button
              size="small"
              disabled={isDisabled}
              onClick={(e) => { e.stopPropagation(); onSelect() }}
              sx={{
                fontSize: '0.63rem', fontWeight: 700, textTransform: 'none',
                py: 0.2, px: 0.85, minWidth: 'unset', borderRadius: '6px', flexShrink: 0,
                bgcolor: 'secondary.main', color: '#fff',
                '&:hover': { bgcolor: '#4e96f6' },
                '&.Mui-disabled': { bgcolor: 'rgba(0,0,0,0.1)', color: 'rgba(0,0,0,0.35)' }
              }}
            >
              Chọn
            </Button>
          )}
        </Box>
      </Box>
    </Box>
  )
}
