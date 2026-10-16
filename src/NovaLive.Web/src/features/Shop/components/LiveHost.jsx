import ShoppingBagIcon from '@mui/icons-material/ShoppingBag'
import Badge from '@mui/material/Badge'
import Box from '@mui/material/Box'
import IconButton from '@mui/material/IconButton'
import { useState } from 'react'
import { useSelector } from 'react-redux'
import { useLiveHost } from '~/features/Shop/hooks/useLiveHost'
import { selectCurrentUser } from '~/store/user/userSlice'
import EndCofirmDialog from './LiveHost/EndCofirmDialog'
import LiveControls from './LiveHost/LiveControls'
import LiveEndScreen from './LiveHost/LiveEndScreen'
import LiveHeader from './LiveHost/LiveHeader'
import LiveStartScreen from './LiveHost/LiveStartScreen'

const LiveHost = ({ liveId, liveProducts = [], onOpenProducts }) => {
  const currentUser = useSelector(selectCurrentUser)
  const [confirmEnd, setConfirmEnd] = useState(false)

  const {
    status, isMicOn,
    isCamOn, duration,
    videoContainerRef,
    startLive, endLive,
    toggleMic, toggleCam
  } = useLiveHost({ liveId, userId: currentUser?._id })

  return (
    <Box sx={{
      position: 'relative',
      width: '100%',
      maxWidth: 480,
      height: '100vh',
      mx: 'auto',
      bgcolor: '#1e1e1e',
      overflow: 'hidden',
      display: 'flex',
      flexDirection: 'column'
    }}>
      <Box ref={videoContainerRef} sx={{ flex: 1, width: '100%', bgcolor: '#111', position: 'relative' }} />

      {status === 'idle' && <LiveStartScreen startLive={startLive} />}

      {status === 'live' && (
        <>
          <LiveHeader duration={duration} />
          <LiveControls
            toggleMic={toggleMic} isMicOn={isMicOn}
            setConfirmEnd={setConfirmEnd}
            toggleCam={toggleCam} isCamOn={isCamOn}
          />

          {/* FAB mở product drawer — chỉ hiện trên mobile */}
          {onOpenProducts && (
            <IconButton
              onClick={onOpenProducts}
              sx={{
                display: { xs: 'flex', md: 'none' },
                position: 'absolute',
                top: 56,
                right: 12,
                width: 44, height: 44,
                bgcolor: 'rgba(52,133,247,0.88)',
                backdropFilter: 'blur(8px)',
                WebkitBackdropFilter: 'blur(8px)',
                color: '#fff',
                zIndex: 10,
                '&:hover': { bgcolor: 'rgba(52,133,247,1)' }
              }}
            >
              <Badge
                badgeContent={liveProducts.length || null}
                color="error"
                sx={{ '& .MuiBadge-badge': { fontSize: '0.6rem', minWidth: 16, height: 16 } }}
              >
                <ShoppingBagIcon sx={{ fontSize: 22 }} />
              </Badge>
            </IconButton>
          )}
        </>
      )}

      {status === 'ended' && <LiveEndScreen duration={duration} />}

      <EndCofirmDialog confirmEnd={confirmEnd} setConfirmEnd={setConfirmEnd} endLive={endLive} />
    </Box>
  )
}

export default LiveHost
