import FiberManualRecordIcon from '@mui/icons-material/FiberManualRecord'
import Box from '@mui/material/Box'
import Chip from '@mui/material/Chip'

const LiveHeader = ({ duration }) => {
  return (
    <Box sx={{
      position: 'absolute', top: 0, left: 0, right: 0,
      display: 'flex', alignItems: 'center',
      justifyContent: 'space-between',
      px: 2, py: 1.5,
      background: 'linear-gradient(to bottom, rgba(0,0,0,0.7), transparent)'
    }}>
      <Chip
        icon={<FiberManualRecordIcon sx={{ color: '#ff3b3b !important', fontSize: '10px !important' }} />}
        label="LIVE"
        size="small"
        sx={{
          bgcolor: 'rgba(255,59,59,0.2)',
          color: '#fff',
          border: '1px solid rgba(255,59,59,0.5)',
          fontWeight: 700,
          letterSpacing: 1
        }}
      />
      <Chip
        label={duration}
        size="small"
        sx={{
          bgcolor: 'rgba(0,0,0,0.5)',
          color: '#fff',
          fontFamily: 'monospace',
          fontSize: 13
        }}
      />
    </Box>
  )
}

export default LiveHeader