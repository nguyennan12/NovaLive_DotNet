import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown'
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp'
import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import IconButton from '@mui/material/IconButton'
import { Typography } from '@mui/material'
import LivePlayer from '../shared/LivePlayer'
import LiveOverlay from './LiveOverlay'
import { useLiveFeed } from '../../hooks/useLiveFeed'

export const LiveFeed = ({ userId }) => {
  const {
    lives, loading, currentIndex,
    goNext, goPrev, loadingMore,
    handleTouchEnd, handleTouchStart, handleWheel
  } = useLiveFeed()


  if (loading) return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%', bgcolor: '#0a0a0a' }}>
      <CircularProgress sx={{ color: 'white' }} />
    </Box>
  )

  if (lives.length === 0) return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '100%', bgcolor: '#0a0a0a', gap: 1 }}>
      <Typography color="white" variant="h6">Chưa có live nào đang phát</Typography>
      <Typography sx={{ color: 'rgba(255,255,255,0.5)', fontSize: '0.85rem' }}>Quay lại sau nhé!</Typography>
    </Box>
  )

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
        bgcolor: '#101010',
        overflow: 'hidden',
        userSelect: 'none',
        borderRadius: { md: '18px' }
      }}
    >
      {/* Video player — key để unmount/remount khi đổi live */}
      <LivePlayer
        key={currentLive._id}
        liveId={currentLive._id}
        userId={userId}
      />

      <LiveOverlay
        key={`overlay-${currentLive._id}`}
        live={currentLive}
      />

      {/* Nav arrows — z:20 để ở trên overlay */}
      <Box sx={{
        position: 'absolute', right: 12, top: '50%',
        transform: 'translateY(-50%)',
        display: 'flex', flexDirection: 'column', gap: 1,
        zIndex: 20
      }}>
        <IconButton
          onClick={goPrev}
          disabled={currentIndex === 0}
          sx={{ bgcolor: 'rgba(255,255,255,0.15)', color: 'white', '&:hover': { bgcolor: 'rgba(255,255,255,0.3)' } }}
        >
          <KeyboardArrowUpIcon />
        </IconButton>
        <IconButton
          onClick={goNext}
          disabled={currentIndex >= lives.length - 1}
          sx={{ bgcolor: 'rgba(255,255,255,0.15)', color: 'white', '&:hover': { bgcolor: 'rgba(255,255,255,0.3)' } }}
        >
          <KeyboardArrowDownIcon />
        </IconButton>
      </Box>

      {/* Loading indicator khi tải thêm live */}
      {loadingMore && (
        <Box sx={{ position: 'absolute', bottom: 10, right: 10, zIndex: 20 }}>
          <CircularProgress size={14} sx={{ color: 'rgba(255,255,255,0.6)' }} />
        </Box>
      )}
    </Box>
  )
}
