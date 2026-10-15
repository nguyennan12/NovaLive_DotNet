import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import { Box, Chip, Skeleton, Tooltip, Typography } from '@mui/material'

// Single field row: label | value + optional verification badge | edit button
const InfoRow = ({ label, value, verified, loading }) => {
  if (loading) {
    return (
      <Box sx={{
        display: 'flex', alignItems: 'center', py: 1.5,
        borderBottom: '1px solid', borderColor: 'divider'
      }}>
        <Skeleton width={110} height={18} sx={{ mr: 2, flexShrink: 0 }} />
        <Skeleton width="45%" height={18} />
      </Box>
    )
  }

  return (
    <Box sx={{
      display: 'flex', alignItems: 'center', py: 1.5,
      borderBottom: '1px solid', borderColor: 'divider',
      '&:last-child': { borderBottom: 'none' }
    }}>
      <Typography sx={{ width: { xs: 100, sm: 130 }, color: 'text.secondary', flexShrink: 0, fontSize: '0.83rem' }}>
        {label}
      </Typography>

      <Box sx={{ flex: 1, display: 'flex', alignItems: 'center', gap: 1, minWidth: 0 }}>
        {value ? (
          <Typography sx={{ fontWeight: 500, fontSize: '0.875rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
            {value}
          </Typography>
        ) : (
          <Typography component="span" sx={{ color: 'text.disabled', fontSize: '0.83rem', fontStyle: 'italic' }}>
            Chưa cập nhật
          </Typography>
        )}

        {verified === true && (
          <Tooltip title="Đã xác thực">
            <CheckCircleIcon sx={{ fontSize: 15, color: 'success.main', flexShrink: 0 }} />
          </Tooltip>
        )}
        {verified === false && (
          <Chip
            label="Xác thực ngay"
            size="small"
            variant="outlined"
            color="warning"
            sx={{ height: 18, fontSize: '0.68rem', cursor: 'pointer', flexShrink: 0 }}
          />
        )}
      </Box>

    </Box>
  )
}

export default InfoRow
