import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import { useNavigate } from 'react-router-dom'


const LiveEndScreen = ({ duration }) => {
  const navigate = useNavigate()
  return (
    <Box sx={{
      position: 'absolute', inset: 0,
      display: 'flex', flexDirection: 'column',
      alignItems: 'center', justifyContent: 'center',
      gap: 2, bgcolor: 'rgba(0,0,0,0.9)'
    }}>
      <Typography color="white" variant="h5" fontWeight={600}>Live đã kết thúc</Typography>
      <Typography color="rgba(255,255,255,0.5)">Thời gian: {duration}</Typography>
      <Button
        onClick={() => navigate('/')} //ve trang quan ly cua shop
        variant="outlined"
        sx={{ color: '#fff', borderColor: 'rgba(255,255,255,0.3)', mt: 2 }}
      >
        Về trang quản lý
      </Button>
    </Box>
  )
}

export default LiveEndScreen