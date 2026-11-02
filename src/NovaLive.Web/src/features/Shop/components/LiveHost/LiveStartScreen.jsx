import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import LiveTvRoundedIcon from '@mui/icons-material/LiveTvRounded'
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'

const LiveStartScreen = ({ startLive }) => {
  return (
    <Box sx={{
      position: 'absolute',
      inset: 0,
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 2.5,
      bgcolor: 'rgba(15, 23, 42, 0.88)',
      backdropFilter: 'blur(12px)',
      zIndex: 10,
      p: 3,
      textAlign: 'center'
    }}>
      <Box
        sx={{
          width: 72,
          height: 72,
          borderRadius: '50%',
          bgcolor: 'rgba(37, 99, 235, 0.15)',
          border: '2px solid rgba(37, 99, 235, 0.4)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: '#3b82f6',
          boxShadow: '0 0 24px rgba(37, 99, 235, 0.3)'
        }}
      >
        <LiveTvRoundedIcon sx={{ fontSize: 36 }} />
      </Box>

      <Box>
        <Typography color="white" variant="h5" fontWeight={700}>
          Sẵn sàng bắt đầu Live?
        </Typography>
        <Typography sx={{ color: '#94a3b8', fontSize: '0.875rem', mt: 0.5, maxWidth: 300 }}>
          Kiểm tra camera và micro trước khi bắt đầu phát sóng trực tiếp tới người mua.
        </Typography>
      </Box>

      <Button
        onClick={startLive}
        variant="contained"
        size="large"
        startIcon={<PlayArrowIcon />}
        sx={{
          fontWeight: 700,
          bgcolor: '#2563eb',
          color: '#ffffff',
          borderRadius: '9999px',
          px: 4,
          py: 1.2,
          textTransform: 'none',
          fontSize: '1rem',
          boxShadow: '0 6px 20px rgba(37, 99, 235, 0.4)',
          '&:hover': {
            bgcolor: '#1d4ed8',
            boxShadow: '0 8px 25px rgba(37, 99, 235, 0.5)'
          }
        }}
      >
        Bắt đầu phát sóng
      </Button>
    </Box>
  )
}

export default LiveStartScreen
