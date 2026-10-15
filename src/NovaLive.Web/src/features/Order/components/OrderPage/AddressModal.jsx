import AddLocationAltRoundedIcon from '@mui/icons-material/AddLocationAltRounded'
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import ExpandLessRoundedIcon from '@mui/icons-material/ExpandLessRounded'
import LocationOnRoundedIcon from '@mui/icons-material/LocationOnRounded'
import {
  Autocomplete,
  Box, Button, Chip, CircularProgress, Collapse,
  Dialog, DialogContent, DialogTitle,
  Divider, TextField, Typography
} from '@mui/material'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useAddressMutations } from '~/features/Address/hooks/useAddressMutation'
import { useDistricts, useProvinces, useWards } from '~/features/Address/hooks/useShipping'
import { glassSx } from '~/theme'

function AddressModal({ open, onClose, onSelect, selectedAddress, userId, addresses = [], ownerType = 'user' }) {
  const isShop = ownerType === 'shop'

  const [showFormExplicit, setShowFormExplicit] = useState(null)
  const showCreateForm = showFormExplicit !== null ? showFormExplicit : addresses.length === 0

  const [selectedProvince, setSelectedProvince] = useState(null)
  const [selectedDistrict, setSelectedDistrict] = useState(null)
  const [selectedWard, setSelectedWard] = useState(null)

  const { provinces, isLoadingProvinces } = useProvinces()
  const { districts, isLoadingDistricts } = useDistricts(selectedProvince?.ProvinceID)
  const { wards, isLoadingWards } = useWards(selectedDistrict?.DistrictID)
  const { createAddress, isCreating } = useAddressMutations(userId)

  const { register, control, handleSubmit, setValue, reset, formState: { errors } } = useForm({
    defaultValues: { owner_name: '', owner_phone: '', province: '', district: '', ward: '', street: '' }
  })

  const resetForm = () => {
    setSelectedProvince(null)
    setSelectedDistrict(null)
    setSelectedWard(null)
    reset()
  }

  const handleClose = () => {
    setShowFormExplicit(null)
    resetForm()
    onClose()
  }

  const handleToggleForm = () => {
    if (showCreateForm) resetForm()
    setShowFormExplicit(!showCreateForm)
  }

  const handleSelectExisting = (addr) => {
    onSelect(addr)
    handleClose()
  }

  const handleProvinceChange = (val) => {
    setSelectedProvince(val)
    setSelectedDistrict(null)
    setSelectedWard(null)
    setValue('province', val?.ProvinceName || '', { shouldValidate: true })
    setValue('district', '', { shouldValidate: false })
    setValue('ward', '', { shouldValidate: false })
  }

  const handleDistrictChange = (val) => {
    setSelectedDistrict(val)
    setSelectedWard(null)
    setValue('district', val?.DistrictName || '', { shouldValidate: true })
    setValue('ward', '', { shouldValidate: false })
  }

  const handleWardChange = (val) => {
    setSelectedWard(val)
    setValue('ward', val?.WardName || '', { shouldValidate: true })
  }

  const onSubmit = (formData) => {
    const payload = {
      owner_type: ownerType,
      ...(formData.owner_name && { owner_name: formData.owner_name }),
      ...(formData.owner_phone && { owner_phone: formData.owner_phone }),
      province: selectedProvince.ProvinceName,
      province_id: selectedProvince.ProvinceID,
      district: selectedDistrict.DistrictName,
      district_id: selectedDistrict.DistrictID,
      ward: selectedWard.WardName,
      ward_code: String(selectedWard.WardCode),
      street: formData.street,
      fullAddress: `${formData.street}, ${selectedWard.WardName}, ${selectedDistrict.DistrictName}, ${selectedProvince.ProvinceName}`
    }
    createAddress(payload, {
      onSuccess: (created) => {
        onSelect(created)
        handleClose()
      }
    })
  }

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      fullWidth
      maxWidth="sm"
      PaperProps={{
        sx: {
          ...glassSx,
          bgcolor: 'primary.main',
          borderRadius: 3,
          border: '1px solid',
          borderColor: 'divider',
          m: { xs: 1.5, sm: 3 }
        }
      }}
    >
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1, pb: 1.25 }}>
        <LocationOnRoundedIcon sx={{ color: isShop ? '#8b5cf6' : 'secondary.main', fontSize: 20 }} />
        <Typography sx={{ fontSize: '1rem', fontWeight: 700 }}>
          {isShop ? 'Địa chỉ cửa hàng' : 'Địa chỉ giao hàng'}
        </Typography>
      </DialogTitle>

      <Divider sx={{ borderColor: 'divider' }} />

      <DialogContent sx={{ pt: 2, pb: 2.5, display: 'flex', flexDirection: 'column', gap: 0, maxHeight: '72vh', overflowY: 'auto' }}>

        {/*  Danh sách địa chỉ đã lưu  */}
        {addresses.length > 0 && (
          <>
            <Typography sx={{ fontSize: '0.78rem', fontWeight: 700, color: 'rgba(45,45,45,0.5)', textTransform: 'uppercase', letterSpacing: '0.06em', mb: 1.25 }}>
              Địa chỉ đã lưu
            </Typography>

            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1, mb: 1.5 }}>
              {addresses.map((addr) => {
                const isSelected = selectedAddress?._id === addr._id
                return (
                  <Box
                    key={addr._id}
                    onClick={() => handleSelectExisting(addr)}
                    sx={{
                      display: 'flex', alignItems: 'flex-start', gap: 1.25,
                      p: 1.5, borderRadius: 2, cursor: 'pointer',
                      border: '1px solid',
                      borderColor: isSelected ? 'secondary.main' : 'divider',
                      bgcolor: isSelected ? 'rgba(52,133,247,0.05)' : 'transparent',
                      transition: 'all 0.15s ease',
                      '&:hover': {
                        borderColor: 'secondary.main',
                        bgcolor: 'rgba(52,133,247,0.05)',
                        '& .select-icon': { opacity: 1 }
                      }
                    }}
                  >
                    <LocationOnRoundedIcon sx={{ fontSize: 16, color: 'secondary.main', mt: 0.2, flexShrink: 0 }} />

                    <Box sx={{ flex: 1, minWidth: 0 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap', mb: 0.35 }}>
                        <Typography sx={{ fontSize: '0.84rem', fontWeight: 700, color: 'primary.contrastText' }}>
                          {addr.owner_name}
                        </Typography>
                        <Typography sx={{ fontSize: '0.78rem', color: 'rgba(45,45,45,0.5)' }}>
                          {addr.owner_phone}
                        </Typography>
                        {addr.is_default && (
                          <Chip
                            label="Mặc định"
                            size="small"
                            sx={{
                              height: 17, fontSize: '0.6rem', fontWeight: 700,
                              bgcolor: 'rgba(52,133,247,0.1)', color: 'secondary.main',
                              border: '1px solid rgba(52,133,247,0.25)',
                              '& .MuiChip-label': { px: 0.7 }
                            }}
                          />
                        )}
                      </Box>
                      <Typography sx={{ fontSize: '0.78rem', color: 'rgba(45,45,45,0.62)', lineHeight: 1.5, wordBreak: 'break-word' }}>
                        {addr.fullAddress || `${addr.street}, ${addr.ward}, ${addr.district}, ${addr.province}`}
                      </Typography>
                    </Box>

                    <CheckCircleRoundedIcon
                      className="select-icon"
                      sx={{
                        fontSize: 17,
                        color: 'secondary.main',
                        flexShrink: 0,
                        opacity: isSelected ? 1 : 0,
                        transition: 'opacity 0.15s'
                      }}
                    />
                  </Box>
                )
              })}
            </Box>

            <Divider sx={{ borderColor: 'divider', mb: 1.5 }} />
          </>
        )}

        {/* ── Toggle button thêm địa chỉ mới ── */}
        <Button
          size="small"
          onClick={handleToggleForm}
          startIcon={
            showCreateForm
              ? <ExpandLessRoundedIcon sx={{ fontSize: 16 }} />
              : <AddLocationAltRoundedIcon sx={{ fontSize: 16 }} />
          }
          sx={{
            alignSelf: 'flex-start',
            fontSize: '0.8rem', fontWeight: 600,
            color: showCreateForm ? 'rgba(45,45,45,0.5)' : 'secondary.main',
            textTransform: 'none', py: 1, px: 1.25, borderRadius: 2,
            '&:hover': { bgcolor: 'rgba(52,133,247,0.06)' }
          }}
        >
          {showCreateForm ? 'Thu gọn' : 'Thêm địa chỉ mới'}
        </Button>

        {/* ── Form tạo địa chỉ mới (collapsible) ── */}
        <Collapse in={showCreateForm} unmountOnExit>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>

            {/* Họ tên + SĐT — ẩn khi tạo địa chỉ shop */}
            {!isShop && (
              <Box sx={{ display: 'flex', gap: 2 }}>
                <TextField
                  label="Họ tên người nhận"
                  size="small" fullWidth
                  error={!!errors.owner_name}
                  helperText={errors.owner_name?.message}
                  {...register('owner_name', { required: 'Vui lòng nhập tên người nhận' })}
                />
                <TextField
                  label="Số điện thoại"
                  size="small" fullWidth
                  error={!!errors.owner_phone}
                  helperText={errors.owner_phone?.message}
                  {...register('owner_phone', {
                    required: 'Vui lòng nhập SĐT',
                    pattern: { value: /^[0-9]{9,11}$/, message: 'SĐT không hợp lệ' }
                  })}
                />
              </Box>
            )}

            {/* Tỉnh / Thành phố */}
            <Controller
              name="province" control={control}
              rules={{ required: 'Vui lòng chọn tỉnh/thành phố' }}
              render={({ field }) => (
                <Autocomplete
                  options={provinces}
                  getOptionLabel={(opt) => opt.ProvinceName || ''}
                  loading={isLoadingProvinces}
                  loadingText="Đang tải..."
                  noOptionsText="Không tìm thấy"
                  value={selectedProvince}
                  onChange={(_, val) => handleProvinceChange(val)}
                  isOptionEqualToValue={(opt, val) => opt.ProvinceID === val?.ProvinceID}
                  renderInput={(params) => (
                    <TextField {...params} inputRef={field.ref}
                      label="Tỉnh / Thành phố" size="small"
                      error={!!errors.province} helperText={errors.province?.message}
                    />
                  )}
                />
              )}
            />

            {/* Quận / Huyện */}
            <Controller
              name="district" control={control}
              rules={{ required: 'Vui lòng chọn quận/huyện' }}
              render={({ field }) => (
                <Autocomplete
                  options={districts}
                  getOptionLabel={(opt) => opt.DistrictName || ''}
                  loading={isLoadingDistricts}
                  loadingText="Đang tải..."
                  noOptionsText="Không tìm thấy"
                  disabled={!selectedProvince}
                  value={selectedDistrict}
                  onChange={(_, val) => handleDistrictChange(val)}
                  isOptionEqualToValue={(opt, val) => opt.DistrictID === val?.DistrictID}
                  renderInput={(params) => (
                    <TextField {...params} inputRef={field.ref}
                      label="Quận / Huyện" size="small"
                      error={!!errors.district}
                      helperText={errors.district?.message || (!selectedProvince ? 'Chọn tỉnh/thành trước' : '')}
                    />
                  )}
                />
              )}
            />

            {/* Phường / Xã */}
            <Controller
              name="ward" control={control}
              rules={{ required: 'Vui lòng chọn phường/xã' }}
              render={({ field }) => (
                <Autocomplete
                  options={wards}
                  getOptionLabel={(opt) => opt.WardName || ''}
                  loading={isLoadingWards}
                  loadingText="Đang tải..."
                  noOptionsText="Không tìm thấy"
                  disabled={!selectedDistrict}
                  value={selectedWard}
                  onChange={(_, val) => handleWardChange(val)}
                  isOptionEqualToValue={(opt, val) => opt.WardCode === val?.WardCode}
                  renderInput={(params) => (
                    <TextField {...params} inputRef={field.ref}
                      label="Phường / Xã" size="small"
                      error={!!errors.ward}
                      helperText={errors.ward?.message || (!selectedDistrict ? 'Chọn quận/huyện trước' : '')}
                    />
                  )}
                />
              )}
            />

            {/* Địa chỉ chi tiết */}
            <TextField
              label="Địa chỉ chi tiết (số nhà, tên đường...)"
              size="small" fullWidth multiline rows={2}
              error={!!errors.street}
              helperText={errors.street?.message}
              {...register('street', { required: 'Vui lòng nhập địa chỉ chi tiết' })}
            />

            {/* Actions */}
            <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5, justifyContent: 'flex-end' }}>
              <Button
                variant="outlined"
                onClick={handleClose}
                disabled={isCreating}
                sx={{ borderRadius: 2, borderColor: 'divider', color: 'primary.contrastText', px: 2.5 }}
              >
                Hủy
              </Button>
              <Button
                variant="contained"
                onClick={handleSubmit(onSubmit)}
                disabled={isCreating}
                startIcon={isCreating ? <CircularProgress size={14} color="inherit" /> : null}
                sx={{
                  borderRadius: 2, fontWeight: 700, px: 2.5,
                  background: 'linear-gradient(90deg, #568dfb, #69aedc)',
                  color: '#fff',
                  '&:hover': { boxShadow: '0 4px 16px rgba(52,133,247,0.35)' }
                }}
              >
                {isCreating ? 'Đang lưu...' : 'Lưu địa chỉ mới'}
              </Button>
            </Box>

          </Box>
        </Collapse>

      </DialogContent>
    </Dialog>
  )
}

export default AddressModal
