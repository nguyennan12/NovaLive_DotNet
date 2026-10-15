import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'

const LiveStartScreen = ({ startLive }) => {
  return (
    <Box sx={{
      position: 'absolute', inset: 0,
      display: 'flex', flexDirection: 'column',
      alignItems: 'center', justifyContent: 'center',
      gap: 3, bgcolor: 'rgba(0,0,0,0.85)'
    }}>

      <>
        <Typography color="white" variant="h5" fontWeight={600}>Sẵn sàng Live?</Typography>
        <Button
          onClick={startLive}
          variant="contained"
          size="large"
          startIcon={<PlayArrowIcon />}
          sx={{
            fontWeight: '600',
            background: 'linear-gradient(90deg, #3465c8, #69aedc, #8acdde)',
            color: '#ffffff'
          }}
        >
          Phát Live
        </Button>
      </>

    </Box>
  )
}

export default LiveStartScreen