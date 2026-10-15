import VisibilityIcon from '@mui/icons-material/Visibility'
import { Avatar, Box, Button, Chip, Typography } from '@mui/material'
const LiveHeaderInfo = ({ live, viewers }) => {
  const shop = live?.live_shopId

  return (
    <Box sx={{
      position: 'absolute', top: 0, left: 0, right: 0,
      background: 'linear-gradient(to bottom, rgba(0,0,0,0.72) 0%, rgba(0,0,0,0.1) 80%, transparent 100%)',
      px: 1.5, pt: 2.5, pb: 3,
      display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      pointerEvents: 'none',
      zIndex: 2
    }}>
      {/* Left: avatar + shop name + follow */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, minWidth: 0, flex: 1 }}>
        <Avatar
          src={shop?.shop_logo}
          sx={{ width: 36, height: 36, border: '2px solid rgba(255,255,255,0.75)', flexShrink: 0 }}
        >
          {shop?.shop_name?.[0]?.toUpperCase()}
        </Avatar>
        <Box sx={{ minWidth: 0 }}>
          <Typography sx={{
            color: 'white', fontWeight: 700, fontSize: '0.85rem', lineHeight: 1.2,
            textShadow: '0 1px 4px rgba(0,0,0,0.8)'
          }} noWrap>
            {shop?.shop_name ?? 'Shop'}
          </Typography>
          {live?.live_title && (
            <Typography sx={{
              color: 'rgba(255,255,255,0.72)', fontSize: '0.68rem',
              textShadow: '0 1px 3px rgba(0,0,0,0.7)'
            }} noWrap>
              {live.live_title}
            </Typography>
          )}
        </Box>
        <Button
          size="small"
          variant="outlined"
          sx={{
            color: 'white', borderColor: 'rgba(255,255,255,0.65)',
            fontSize: '0.7rem', fontWeight: 600, borderRadius: '20px',
            px: 1.5, py: 0.25, minWidth: 0, flexShrink: 0,
            pointerEvents: 'auto',
            '&:hover': { borderColor: 'white', bgcolor: 'rgba(255,255,255,0.12)' }
          }}
        >
          Theo dõi
        </Button>
      </Box>

      {/* Right: viewer count + Live badge */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, ml: 1, flexShrink: 0 }}>
        <Chip
          icon={<VisibilityIcon sx={{ fontSize: '13px !important', color: 'rgba(255,255,255,0.9) !important' }} />}
          label={viewers ?? 0}
          size="small"
          sx={{
            bgcolor: 'rgba(0,0,0,0.48)', color: 'white',
            fontSize: '0.72rem', fontWeight: 600,
            height: 24,
            '& .MuiChip-label': { px: 0.75 },
            '& .MuiChip-icon': { ml: 0.75 }
          }}
        />
        <Chip
          label='live'
          size='small'
          sx={{
            p: 1,
            bgcolor: '#ff2929ff',
            color: '#fff',
            fontWeight: 600,
            fontSize: '0.7rem',
            animation: 'livePulse 1.5s ease-in-out infinite',
            '@keyframes livePulse': {
              '0%, 100%': { opacity: 1 },
              '50%': { opacity: 0.55 }
            }
          }}
        />
      </Box>
    </Box>
  )
}

export default LiveHeaderInfo
