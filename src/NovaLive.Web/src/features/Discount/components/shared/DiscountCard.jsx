import ContentCopyRoundedIcon from '@mui/icons-material/ContentCopyRounded'
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import { Box, IconButton, Tooltip, Typography } from '@mui/material'
import { formatDate } from '~/common/utils/formatters'
import { CATEGORY_THEME, fmtValue, fmtValueSub } from '../../utils/normalizeDiscount'
import { StatusBadge } from './StatusBadge'


const usagePct = (used, limit) => Math.min(100, Math.round((used / limit) * 100))

export const DiscountCard = ({ discount, onEdit, onDelete }) => {
  const { name, code, category = 'product', type, value, status, startDate, endDate, usageLimit, usedCount } = discount
  const theme = CATEGORY_THEME[category] || CATEGORY_THEME['product']
  const Icon = theme.icon
  const pct = usagePct(usedCount, usageLimit)
  const isExpired = status === 'expired'

  const handleCopy = () => {
    if (code) navigator.clipboard.writeText(code)
  }

  return (
    <Box
      sx={{
        position: 'relative',
        bgcolor: isExpired ? '#fafafa' : '#fff',
        borderRadius: '14px',
        boxShadow: isExpired
          ? '0 1px 4px rgba(0,0,0,0.05)'
          : '0 2px 12px rgba(0,0,0,0.07), 0 0 0 1px rgba(0,0,0,0.04)',
        overflow: 'visible',
        opacity: isExpired ? 0.7 : 1,
        transition: 'transform 0.2s ease, box-shadow 0.2s ease',
        WebkitMaskImage: `
      radial-gradient(circle at 30% 0, transparent 10px, black 11px),
      radial-gradient(circle at 30% 100%, transparent 10px, black 11px)
    `,
        WebkitMaskComposite: 'destination-in',
        maskComposite: 'intersect',
        '&:hover': isExpired ? {} : {
          transform: 'translateY(-3px)',
          boxShadow: `0 10px 32px rgba(0,0,0,0.1), 0 0 0 1.5px ${theme.stubAccent}33`
        }
      }}
    >


      <Box sx={{ display: 'flex' }}>
        {/* ── Left stub ── */}
        <Box sx={{
          width: '30%', flexShrink: 0,
          display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
          bgcolor: theme.stubBg, borderRadius: '14px 0 0 14px',
          py: 2.5, px: 1, gap: 0.75,
          position: 'relative',
          '&::after': {
            content: '""', position: 'absolute', right: 0, top: '14px', bottom: '14px',
            borderRight: `2px dashed ${theme.dash}`
          }
        }}>
          {/* Category icon */}
          <Box sx={{
            width: 32, height: 32, borderRadius: '10px',
            bgcolor: `${theme.stubAccent}18`,
            display: 'flex', alignItems: 'center', justifyContent: 'center'
          }}>
            <Icon sx={{ fontSize: 17, color: theme.stubAccent }} />
          </Box>

          {/* Big value */}
          <Typography sx={{
            fontSize: category === 'freeship' ? '1.3rem' : '1.6rem',
            fontWeight: 900, color: theme.stubAccent,
            letterSpacing: '-0.04em', lineHeight: 1
          }}>
            {fmtValue(category, type, value)}
          </Typography>

          <Typography sx={{
            fontSize: '0.55rem', fontWeight: 700, color: theme.stubText,
            textTransform: 'uppercase', letterSpacing: '0.08em', textAlign: 'center'
          }}>
            {fmtValueSub(category, type, value)}
          </Typography>
        </Box>

        {/* ── Right content ── */}
        <Box sx={{ flex: 1, p: 2, display: 'flex', flexDirection: 'column', gap: 1, minWidth: 0 }}>
          {/* Name row + status */}
          <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 1 }}>
            <Box sx={{ minWidth: 0, flex: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mb: 0.4 }}>
                {/* Category chip */}
                <Box sx={{ px: 0.75, py: 0.2, borderRadius: '4px', bgcolor: theme.chipBg, flexShrink: 0 }}>
                  <Typography sx={{ fontSize: '0.58rem', fontWeight: 700, color: theme.chipText, lineHeight: 1.4 }}>
                    {theme.chipLabel}
                  </Typography>
                </Box>
              </Box>

              <Typography noWrap sx={{ fontSize: '0.85rem', fontWeight: 700, color: '#1a1a1a', lineHeight: 1.3 }}>
                {name}
              </Typography>

              {/* Code */}
              {code ? (
                <Tooltip title='Click để copy mã'>
                  <Box
                    onClick={handleCopy}
                    sx={{
                      display: 'inline-flex', alignItems: 'center', gap: 0.5,
                      mt: 0.5, px: 0.875, py: 0.2, bgcolor: '#f3f4f6',
                      borderRadius: '5px', border: '1.5px dashed #d1d5db',
                      cursor: 'pointer', '&:hover': { bgcolor: '#e5e7eb' }
                    }}
                  >
                    <Typography sx={{ fontSize: '0.68rem', fontWeight: 700, color: '#374151', letterSpacing: '0.08em', fontFamily: 'monospace' }}>
                      {code}
                    </Typography>
                    <ContentCopyRoundedIcon sx={{ fontSize: 10, color: '#6b7280' }} />
                  </Box>
                </Tooltip>
              ) : (
                <Typography sx={{ fontSize: '0.65rem', color: '#9ca3af', mt: 0.4, fontStyle: 'italic' }}>
                  Không cần mã
                </Typography>
              )}
            </Box>

            <StatusBadge status={status} />
          </Box>

          {/* Date range */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
            <Typography sx={{ fontSize: '0.65rem', color: '#9ca3af' }}>{formatDate(startDate)}</Typography>
            <Box sx={{ width: 12, height: 1, bgcolor: '#e5e7eb' }} />
            <Typography sx={{ fontSize: '0.65rem', color: '#9ca3af' }}>{formatDate(endDate)}</Typography>
          </Box>

          {/* Usage bar */}
          <Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.4 }}>
              <Typography sx={{ fontSize: '0.6rem', color: '#9ca3af' }}>Đã sử dụng</Typography>
              <Typography sx={{ fontSize: '0.62rem', fontWeight: 600, color: '#4b5563' }}>
                {usedCount}/{usageLimit}
              </Typography>
            </Box>
            <Box sx={{ height: 4, bgcolor: '#f3f4f6', borderRadius: '4px', overflow: 'hidden' }}>
              <Box sx={{
                height: '100%', width: `${pct}%`, borderRadius: '4px',
                bgcolor: pct >= 100 ? '#ef4444' : pct >= 80 ? '#f59e0b' : theme.barColor,
                transition: 'width 0.5s ease'
              }} />
            </Box>
          </Box>

          {/* Actions */}
          <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 0.5, mt: 'auto' }}>
            <Tooltip title='Chỉnh sửa'>
              <IconButton size='small' onClick={() => onEdit(discount)}
                sx={{ borderRadius: '7px', color: '#9ca3af', '&:hover': { bgcolor: '#eff6ff', color: '#3485f7' } }}>
                <EditOutlinedIcon sx={{ fontSize: 14 }} />
              </IconButton>
            </Tooltip>
            <Tooltip title='Xoá'>
              <IconButton size='small' onClick={() => onDelete(discount)}
                sx={{ borderRadius: '7px', color: '#9ca3af', '&:hover': { bgcolor: '#fef2f2', color: '#ef4444' } }}>
                <DeleteOutlineRoundedIcon sx={{ fontSize: 14 }} />
              </IconButton>
            </Tooltip>
          </Box>
        </Box>
      </Box>
    </Box>
  )
}