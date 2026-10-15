import StoreIcon from '@mui/icons-material/Store'
import { Button } from '@mui/material'
import { useState } from 'react'
import RegisterShopDialog from '~/features/Shop/components/RegisterShopDialog'

const ProfileActionButton = ({ currentUser, isShop, navigate }) => {
  const [registerOpen, setRegisterOpen] = useState(false)

  if (!currentUser) {
    return (
      <Button
        size="small"
        variant="contained"
        onClick={() => navigate('/login')}
        sx={{ background: 'linear-gradient(90deg,#0095ff,#14c3eb)', color: '#fff', fontSize: '0.8rem', py: 0.6 }}
      >
        Đăng nhập
      </Button>
    )
  }

  if (isShop) {
    return (
      <></>
    )
  }

  return (
    <>
      <Button
        variant="outlined"
        size="small"
        startIcon={<StoreIcon sx={{ fontSize: '15px !important' }} />}
        onClick={() => setRegisterOpen(true)}
        sx={{
          borderColor: '#8b5cf6', color: '#8b5cf6', fontSize: '0.8rem', py: 0.6,
          '&:hover': { background: 'rgba(139,92,246,0.08)' }
        }}
      >
        Mở cửa hàng
      </Button>
      <RegisterShopDialog open={registerOpen} onClose={() => setRegisterOpen(false)} />
    </>
  )
}

export default ProfileActionButton
