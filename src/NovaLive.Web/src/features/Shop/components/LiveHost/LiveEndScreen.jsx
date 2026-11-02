import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import { useNavigate } from 'react-router-dom'

const LiveEndScreen = ({ duration }) => {
  const navigate = useNavigate()
  return (
    <Box sx={{
      position: 'absolute',
      inset: 0,
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      justifyContent: 'center',
      gap: 2,
      bgcolor: 'rgba(15, 23, 42, 0.92)',
      backdropFilter: 'blur(12px)',
      zIndex: 10,
      p: 3,
      textAlign: 'center'
    }}>
      <Box
        sx={{
          width: 64,
          height: 64,
          borderRadius: '50%',
          bgcolor: 'rgba(34, 197, 94, 0.15)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: '#22c55e',
          mb: 1
        }}
      >
        <CheckCircleRoundedIcon sx={{ fontSize: 36 }} />
      </Box>

      <Typography color="white" variant="h5" fontWeight={700}>
        Phiên Live đã kết thúc
      </Typography>

      <Typography sx={{ color: '#94a3b8', fontSize: '0.9rem' }}>
        Thời lượng phát sóng: <strong style={{ color: '#ffffff' }}>{duration || '00:00'}</strong>
      </Typography>

      <Button
        onClick={() => navigate('/seller/live')}
        variant="contained"
        sx={{
          bgcolor: '#2563eb',
          color: '#fff',
          borderRadius: '9999px',
          px: 3.5,
          py: 1,
          mt: 2,
          fontWeight: 600,
          textTransform: 'none',
          '&:hover': { bgcolor: '#1d4ed8' }
        }}
      >
        Về trang quản lý Live
      </Button>
    </Box>
  )
}

export default LiveEndScreen
