import StoreIcon from '@mui/icons-material/Store'
import AdminPanelSettingsRoundedIcon from '@mui/icons-material/AdminPanelSettingsRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import { Button, Stack } from '@mui/material'
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

  return (
    <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
      {/* Admin Dashboard shortcut button */}
      <Button
        variant="contained"
        size="small"
        startIcon={<AdminPanelSettingsRoundedIcon sx={{ fontSize: '16px !important' }} />}
        onClick={() => navigate('/admin/dashboard')}
        sx={{
          background: 'linear-gradient(90deg, #3465c8 0%, #1e40af 100%)',
          color: '#fff',
          fontSize: '0.8rem',
          fontWeight: 700,
          py: 0.6,
          px: 1.5,
          borderRadius: '8px',
          boxShadow: '0 2px 8px rgba(37, 99, 235, 0.3)',
          textTransform: 'none',
          '&:hover': {
            background: 'linear-gradient(90deg, #2563eb 0%, #1d4ed8 100%)'
          }
        }}
      >
        Quản trị sàn (Admin)
      </Button>

      {isShop ? (
        <Button
          variant="outlined"
          size="small"
          startIcon={<StorefrontRoundedIcon sx={{ fontSize: '16px !important' }} />}
          onClick={() => navigate('/dashboard/shop')}
          sx={{
            borderColor: '#8b5cf6',
            color: '#8b5cf6',
            fontSize: '0.8rem',
            fontWeight: 600,
            py: 0.6,
            px: 1.5,
            borderRadius: '8px',
            textTransform: 'none',
            '&:hover': { background: 'rgba(139,92,246,0.08)', borderColor: '#7c3aed' }
          }}
        >
          Kênh Người Bán
        </Button>
      ) : (
        <>
          <Button
            variant="outlined"
            size="small"
            startIcon={<StoreIcon sx={{ fontSize: '15px !important' }} />}
            onClick={() => setRegisterOpen(true)}
            sx={{
              borderColor: '#8b5cf6',
              color: '#8b5cf6',
              fontSize: '0.8rem',
              fontWeight: 600,
              py: 0.6,
              px: 1.5,
              borderRadius: '8px',
              textTransform: 'none',
              '&:hover': { background: 'rgba(139,92,246,0.08)' }
            }}
          >
            Mở cửa hàng
          </Button>
          <RegisterShopDialog open={registerOpen} onClose={() => setRegisterOpen(false)} />
        </>
      )}
    </Stack>
  )
}

export default ProfileActionButton
