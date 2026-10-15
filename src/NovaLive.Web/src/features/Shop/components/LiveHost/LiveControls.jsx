import CallEndIcon from '@mui/icons-material/CallEnd'
import MicIcon from '@mui/icons-material/Mic'
import MicOffIcon from '@mui/icons-material/MicOff'
import VideocamIcon from '@mui/icons-material/Videocam'
import VideocamOffIcon from '@mui/icons-material/VideocamOff'
import Box from '@mui/material/Box'
import IconButton from '@mui/material/IconButton'

const LiveControls = ({ toggleMic, isMicOn, setConfirmEnd, toggleCam, isCamOn }) => {
  return (
    <Box sx={{
      position: 'absolute', bottom: 0, left: 0, right: 0,
      display: 'flex', alignItems: 'center',
      justifyContent: 'center', gap: 2,
      px: 3, py: 3,
      background: 'linear-gradient(to top, rgba(0,0,0,0.8), transparent)'
    }}>

      <IconButton
        onClick={toggleMic}
        sx={{
          width: 52, height: 52,
          bgcolor: isMicOn ? 'rgba(255,255,255,0.15)' : 'rgba(255,59,59,0.8)',
          color: '#fff',
          '&:hover': {
            bgcolor: isMicOn ? 'rgba(255,255,255,0.25)' : 'rgba(255,59,59,1)'
          }
        }}
      >
        {isMicOn ? <MicIcon /> : <MicOffIcon />}
      </IconButton>

      <IconButton
        onClick={() => setConfirmEnd(true)}
        sx={{
          width: 64, height: 64,
          bgcolor: '#ff3b3b',
          color: '#fff',
          '&:hover': { bgcolor: '#e02020' }
        }}
      >
        <CallEndIcon sx={{ fontSize: 28 }} />
      </IconButton>

      <IconButton
        onClick={toggleCam}
        sx={{
          width: 52, height: 52,
          bgcolor: isCamOn ? 'rgba(255,255,255,0.15)' : 'rgba(255,59,59,0.8)',
          color: '#fff',
          '&:hover': {
            bgcolor: isCamOn ? 'rgba(255,255,255,0.25)' : 'rgba(255,59,59,1)'
          }
        }}
      >
        {isCamOn ? <VideocamIcon /> : <VideocamOffIcon />}
      </IconButton>
    </Box>
  )
}

export default LiveControls