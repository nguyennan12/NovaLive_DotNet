import AddLocationAltRoundedIcon from '@mui/icons-material/AddLocationAltRounded'
import HomeIcon from '@mui/icons-material/Home'
import LocationOnRoundedIcon from '@mui/icons-material/LocationOnRounded'
import { Box, Button, Chip, Divider, Paper, Typography } from '@mui/material'
import { useState } from 'react'
import { useSelector } from 'react-redux'
import { useAddress } from '~/features/Address/hooks/useAddresss'
import AddressModal from '~/features/Order/components/OrderPage/AddressModal'
import { selectCurrentUser } from '~/store/user/userSlice'
import { glassSx } from '~/theme'
import SectionTitle from '../../shared/SectionTitle'

const AddressItem = ({ addr, isDefault }) => (
  <Box
    sx={{
      display: 'flex', alignItems: 'flex-start', gap: 1.5,
      p: 1.5, borderRadius: 2,
      border: '1px solid',
      borderColor: isDefault ? 'secondary.main' : 'divider',
      bgcolor: isDefault ? 'rgba(52,133,247,0.04)' : 'transparent'
    }}
  >
    <LocationOnRoundedIcon sx={{ fontSize: 16, color: 'secondary.main', mt: 0.25, flexShrink: 0 }} />
    <Box sx={{ flex: 1, minWidth: 0 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap', mb: 0.3 }}>
        <Typography sx={{ fontSize: '0.84rem', fontWeight: 700, color: 'primary.contrastText' }}>
          {addr.owner_name}
        </Typography>
        <Typography sx={{ fontSize: '0.78rem', color: 'text.secondary' }}>
          {addr.owner_phone}
        </Typography>
        {isDefault && (
          <Chip
            label="Mặc định" size="small"
            sx={{
              height: 17, fontSize: '0.6rem', fontWeight: 700,
              bgcolor: 'rgba(52,133,247,0.1)', color: 'secondary.main',
              border: '1px solid rgba(52,133,247,0.25)',
              '& .MuiChip-label': { px: 0.7 }
            }}
          />
        )}
      </Box>
      <Typography sx={{ fontSize: '0.78rem', color: 'text.secondary', lineHeight: 1.55, wordBreak: 'break-word' }}>
        {addr.fullAddress || addr.fullAdress || `${addr.street}, ${addr.ward}, ${addr.district}, ${addr.province}`}
      </Typography>
    </Box>
  </Box>
)

const UserAddressCard = () => {
  const [modalOpen, setModalOpen] = useState(false)
  const currentUser = useSelector(selectCurrentUser)
  const { allAddressUser } = useAddress(currentUser)
  const defaultAddressId = currentUser?.default_address_id

  return (
    <>
      <Paper elevation={0} sx={{ borderRadius: 3, p: { xs: 2, sm: 2.5 }, bgcolor: 'primary.main', border: '1px solid', borderColor: 'divider', ...glassSx }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <SectionTitle title="Địa chỉ của tôi" icon={HomeIcon} accentColor="#f59e0b" />
          <Button
            size="small"
            startIcon={<AddLocationAltRoundedIcon sx={{ fontSize: 15 }} />}
            onClick={() => setModalOpen(true)}
            sx={{
              fontSize: '0.78rem', fontWeight: 600, color: 'secondary.main',
              textTransform: 'none', borderRadius: 2, px: 1.25, mt: -0.5,
              '&:hover': { bgcolor: 'rgba(52,133,247,0.08)' }
            }}
          >
            Thêm địa chỉ
          </Button>
        </Box>

        {allAddressUser.length === 0 ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', py: 3, gap: 1, color: 'text.secondary' }}>
            <LocationOnRoundedIcon sx={{ fontSize: 36, opacity: 0.3 }} />
            <Typography sx={{ fontSize: '0.84rem' }}>Bạn chưa có địa chỉ nào</Typography>
            <Button
              size="small" variant="outlined" onClick={() => setModalOpen(true)}
              sx={{ mt: 0.5, fontSize: '0.78rem', borderRadius: 2, textTransform: 'none' }}
            >
              Thêm ngay
            </Button>
          </Box>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            {allAddressUser.map((addr, idx) => (
              <Box key={addr._id}>
                <AddressItem addr={addr} isDefault={addr._id === defaultAddressId || addr.is_default} />
                {idx < allAddressUser.length - 1 && <Divider sx={{ my: 0.75, opacity: 0.35 }} />}
              </Box>
            ))}
          </Box>
        )}
      </Paper>

      <AddressModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onSelect={() => setModalOpen(false)}
        selectedAddress={null}
        userId={currentUser?._id}
        addresses={allAddressUser}
        ownerType="user"
      />
    </>
  )
}

export default UserAddressCard
