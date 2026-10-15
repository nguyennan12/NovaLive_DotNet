import EditRoundedIcon from '@mui/icons-material/EditRounded'
import LocationOnRoundedIcon from '@mui/icons-material/LocationOnRounded'
import { Box, Button, Chip, Divider, Typography } from '@mui/material'
import { useState } from 'react'
import { useSelector } from 'react-redux'
import { selectCurrentUser } from '~/store/user/userSlice'
import { glassSx, gradientText } from '~/theme'
import AddressModal from './AddressModal'

function OrderAddressSection({ selectedAddress, onAddressChange, allAddressUser }) {
  const [open, setOpen] = useState(false)
  const user = useSelector(selectCurrentUser)

  const handleSelect = (addr) => {
    onAddressChange(addr)
    setOpen(false)
  }

  return (
    <Box sx={{
      bgcolor: 'primary.main', ...glassSx,
      borderRadius: 3, p: 2.5,
      border: '1px solid', borderColor: 'divider'
    }}>
      {/* Section header */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.75 }}>
        <Box sx={{ width: 3.5, height: 22, borderRadius: '2px', bgcolor: 'secondary.main', flexShrink: 0 }} />
        <Typography sx={{ fontWeight: 700, fontSize: '0.93rem', ...gradientText }}>
          Địa chỉ nhận hàng
        </Typography>
      </Box>

      {selectedAddress ? (
        <>
          <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.2 }}>
            <LocationOnRoundedIcon sx={{ fontSize: 18, color: 'secondary.main', mt: 0.25, flexShrink: 0 }} />
            <Box sx={{ flex: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap', mb: 0.4 }}>
                <Typography sx={{ fontSize: '0.875rem', fontWeight: 700, color: 'primary.contrastText' }}>
                  {selectedAddress.owner_name || user?.usr_name || 'Người nhận'}
                </Typography>
                <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.55)' }}>
                  | {selectedAddress.owner_phone}
                </Typography>
                {!selectedAddress && (
                  <Chip
                    label="Mặc định"
                    size="small"
                    sx={{
                      height: 18, fontSize: '0.62rem', fontWeight: 700,
                      bgcolor: 'rgba(52,133,247,0.1)', color: 'secondary.main',
                      border: '1px solid rgba(52,133,247,0.25)',
                      '& .MuiChip-label': { px: 0.75 }
                    }}
                  />
                )}
              </Box>
              <Typography sx={{ fontSize: '0.81rem', color: 'rgba(45,45,45,0.65)', lineHeight: 1.55 }}>
                {selectedAddress.fullAddress || 'Chưa có địa chỉ chi tiết'}
              </Typography>
            </Box>
          </Box>

          <Divider sx={{ my: 1.5, borderColor: 'divider' }} />

          <Button
            size="small"
            startIcon={<EditRoundedIcon sx={{ fontSize: 14 }} />}
            onClick={() => setOpen(true)}
            sx={{
              fontSize: '0.78rem', fontWeight: 600,
              color: 'secondary.main', textTransform: 'none',
              py: 0.4, px: 1.25, borderRadius: 2,
              '&:hover': { bgcolor: 'rgba(52,133,247,0.06)' }
            }}
          >
            Chọn địa chỉ khác
          </Button>
        </>
      ) : (
        <Box>
          <Typography sx={{ fontSize: '0.8rem', color: 'rgba(45,45,45,0.5)', mb: 1.5 }}>
            Bạn chưa có địa chỉ giao hàng. Vui lòng thêm địa chỉ để tiếp tục.
          </Typography>
          <Button
            variant="outlined"
            size="small"
            startIcon={<LocationOnRoundedIcon sx={{ fontSize: 14 }} />}
            onClick={() => setOpen(true)}
            sx={{
              fontSize: '0.78rem', fontWeight: 600, borderRadius: 2,
              borderColor: 'secondary.main', color: 'secondary.main',
              textTransform: 'none', py: 0.6, px: 1.5
            }}
          >
            Thêm địa chỉ giao hàng
          </Button>
        </Box>
      )}

      <AddressModal
        open={open}
        onClose={() => setOpen(false)}
        onSelect={handleSelect}
        userId={user?.id}
        selectedAddress={selectedAddress}
        addresses={allAddressUser}
      />
    </Box>
  )
}

export default OrderAddressSection
