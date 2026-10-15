import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import Typography from '@mui/material/Typography'
import Overlay from '../Overlay/Overlay'
import { useLivePlayer } from '../../hooks/useLivePlayer'
const LivePlayer = ({ liveId, userId }) => {

  const { videoRef, status } = useLivePlayer({ liveId, userId })

  return (
    <Box sx={{ position: 'relative', width: '100%', height: '100%', bgcolor: 'black' }}>
      <Box ref={videoRef} sx={{ width: '100%', height: '100%' }} />

      {status === 'connecting' && (
        <Overlay>
          <CircularProgress size={36} sx={{ color: '#fff' }} />
          <Typography variant="body2" color="white">Đang kết nối...</Typography>
        </Overlay>
      )}
      {status === 'ended' && (
        <Overlay>
          <Typography variant="body1" color="white" fontWeight={500}>Live đã kết thúc</Typography>
        </Overlay>
      )}
      {status === 'error' && (
        <Overlay>
          <Typography variant="body1" color="white" fontWeight={500}>Không thể kết nối</Typography>
        </Overlay>
      )}
    </Box>
  )
}

export default LivePlayer