import { useNavigate } from 'react-router-dom'
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown'
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp'
import LiveTvRoundedIcon from '@mui/icons-material/LiveTvRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import HomeRoundedIcon from '@mui/icons-material/HomeRounded'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import IconButton from '@mui/material/IconButton'
import Typography from '@mui/material/Typography'
import LivePlayer from '../shared/LivePlayer'
import LiveOverlay from './LiveOverlay'

export const LiveFeed = ({
  userId,
  lives = [],
  loading = false,
  currentIndex = 0,
  goNext,
  goPrev,
  loadingMore,
  handleTouchStart,
  handleTouchEnd,
  handleWheel
}) => {
  const navigate = useNavigate()

  if (loading) {
    return (
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          height: '100%',
          width: '100%',
          bgcolor: '#f8fafc',
          borderRadius: '24px'
        }}
      >
        <CircularProgress sx={{ color: '#2563eb' }} />
      </Box>
    )
  }

  if (lives.length === 0) {
    return (
      <Box
        sx={{
          position: 'relative',
          width: '100%',
          height: '100%',
          borderRadius: '24px',
          overflow: 'hidden',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          p: 3,
          background: 'radial-gradient(circle at 50% 32%, #dbeafe 0%, #eff6ff 40%, #ffffff 88%)',
          boxShadow: '0 8px 32px rgba(37, 99, 235, 0.06)',
          border: '1px solid rgba(226, 232, 240, 0.7)'
        }}
      >
        {/* Decorative radar/ripple rings behind the icon */}
        <Box
          sx={{
            position: 'relative',
            width: 320,
            height: 320,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            mb: 2
          }}
        >
          {/* Ring 3 */}
          <Box
            sx={{
              position: 'absolute',
              width: 310,
              height: 310,
              borderRadius: '50%',
              border: '1px solid rgba(59, 130, 246, 0.06)'
            }}
          />
          {/* Ring 2 */}
          <Box
            sx={{
              position: 'absolute',
              width: 220,
              height: 220,
              borderRadius: '50%',
              border: '1px solid rgba(59, 130, 246, 0.12)'
            }}
          />
          {/* Ring 1 */}
          <Box
            sx={{
              position: 'absolute',
              width: 140,
              height: 140,
              borderRadius: '50%',
              border: '1px solid rgba(59, 130, 246, 0.2)'
            }}
          />

          {/* Center Blue Circle Icon */}
          <Box
            sx={{
              width: 76,
              height: 76,
              borderRadius: '50%',
              bgcolor: '#3b82f6',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: '0 10px 25px rgba(59, 130, 246, 0.35)',
              zIndex: 2
            }}
          >
            <LiveTvRoundedIcon sx={{ fontSize: 38, color: '#ffffff' }} />
          </Box>
        </Box>

        {/* Heading & Subtitle */}
        <Typography
          sx={{
            fontSize: '1.25rem',
            fontWeight: 700,
            color: '#0f172a',
            textAlign: 'center',
            mb: 1.2
          }}
        >
          Chưa có phiên live nào đang phát
        </Typography>

        <Typography
          sx={{
            fontSize: '0.875rem',
            color: '#64748b',
            textAlign: 'center',
            maxWidth: 320,
            lineHeight: 1.6,
            mb: 4
          }}
        >
          Các shop sẽ sớm lên sóng với nhiều deal giảm giá và voucher độc quyền. Hãy quay lại sau nhé!
        </Typography>

        {/* Action Buttons */}
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5, width: '100%', maxWidth: 290 }}>
          <Button
            variant="contained"
            onClick={() => navigate('/products')}
            startIcon={<StorefrontRoundedIcon sx={{ fontSize: 20 }} />}
            sx={{
              py: 1.2,
              borderRadius: '9999px',
              bgcolor: '#3b82f6',
              color: '#ffffff',
              fontSize: '0.92rem',
              fontWeight: 600,
              textTransform: 'none',
              boxShadow: '0 4px 14px rgba(59, 130, 246, 0.3)',
              '&:hover': {
                bgcolor: '#2563eb',
                boxShadow: '0 6px 18px rgba(59, 130, 246, 0.4)'
              }
            }}
          >
            Khám phá sản phẩm
          </Button>

          <Button
            variant="outlined"
            onClick={() => navigate('/')}
            startIcon={<HomeRoundedIcon sx={{ fontSize: 20 }} />}
            sx={{
              py: 1.2,
              borderRadius: '9999px',
              bgcolor: '#ffffff',
              color: '#0f172a',
              border: '1.5px solid #e2e8f0',
              fontSize: '0.92rem',
              fontWeight: 600,
              textTransform: 'none',
              '&:hover': {
                bgcolor: '#f8fafc',
                border: '1.5px solid #cbd5e1'
              }
            }}
          >
            Về trang chủ
          </Button>
        </Box>
      </Box>
    )
  }

  const currentLive = lives[currentIndex]

  return (
    <Box
      onTouchStart={handleTouchStart}
      onTouchEnd={handleTouchEnd}
      onWheel={handleWheel}
      sx={{
        position: 'relative',
        width: '100%',
        height: '100%',
        bgcolor: '#0a0a0a',
        overflow: 'hidden',
        userSelect: 'none',
        borderRadius: '24px',
        boxShadow: '0 8px 32px rgba(0,0,0,0.15)'
      }}
    >
      {/* Video player */}
      <LivePlayer
        key={currentLive._id || currentLive.id}
        liveId={currentLive._id || currentLive.id}
        userId={userId}
      />

      <LiveOverlay
        key={`overlay-${currentLive._id || currentLive.id}`}
        live={currentLive}
      />

      {/* Nav arrows */}
      <Box
        sx={{
          position: 'absolute',
          right: 12,
          top: '50%',
          transform: 'translateY(-50%)',
          display: 'flex',
          flexDirection: 'column',
          gap: 1,
          zIndex: 20
        }}
      >
        <IconButton
          onClick={goPrev}
          disabled={currentIndex === 0}
          sx={{
            bgcolor: 'rgba(255,255,255,0.18)',
            color: 'white',
            '&:hover': { bgcolor: 'rgba(255,255,255,0.35)' },
            '&.Mui-disabled': { opacity: 0.3, color: 'white' }
          }}
        >
          <KeyboardArrowUpIcon />
        </IconButton>
        <IconButton
          onClick={goNext}
          disabled={currentIndex >= lives.length - 1}
          sx={{
            bgcolor: 'rgba(255,255,255,0.18)',
            color: 'white',
            '&:hover': { bgcolor: 'rgba(255,255,255,0.35)' },
            '&.Mui-disabled': { opacity: 0.3, color: 'white' }
          }}
        >
          <KeyboardArrowDownIcon />
        </IconButton>
      </Box>

      {/* Loading indicator */}
      {loadingMore && (
        <Box sx={{ position: 'absolute', bottom: 12, right: 12, zIndex: 20 }}>
          <CircularProgress size={16} sx={{ color: 'rgba(255,255,255,0.7)' }} />
        </Box>
      )}
    </Box>
  )
}
