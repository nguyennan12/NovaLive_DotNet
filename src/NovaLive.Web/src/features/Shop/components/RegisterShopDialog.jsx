import StorefrontIcon from '@mui/icons-material/Storefront'
import {
  Autocomplete, Box, Button, CircularProgress,
  Dialog, DialogContent, DialogTitle,
  Divider, TextField, Typography
} from '@mui/material'
import { useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useDistricts, useProvinces, useWards } from '~/features/Address/hooks/useShipping'
import { glassSx } from '~/theme'
import { useShopMutation } from '../hooks/useShop'

const RegisterShopDialog = ({ open, onClose }) => {
  const { registerShop, isRegistering } = useShopMutation()

  const [selectedProvince, setSelectedProvince] = useState(null)
  const [selectedDistrict, setSelectedDistrict] = useState(null)
  const [selectedWard, setSelectedWard] = useState(null)

  const { provinces, isLoadingProvinces } = useProvinces()
  const { districts, isLoadingDistricts } = useDistricts(selectedProvince?.ProvinceID)
  const { wards, isLoadingWards } = useWards(selectedDistrict?.DistrictID)

  const { register, control, handleSubmit, setValue, reset, formState: { errors } } = useForm({
    defaultValues: { name: '', contact: '', province: '', district: '', ward: '', street: '' }
  })

  const handleClose = () => {
    setSelectedProvince(null)
    setSelectedDistrict(null)
    setSelectedWard(null)
    reset()
    onClose()
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
      name: formData.name,
      contact: formData.contact || null,
      address: {
        street: formData.street,
        province: selectedProvince.ProvinceName,
        province_id: selectedProvince.ProvinceID,
        district: selectedDistrict.DistrictName,
        district_id: selectedDistrict.DistrictID,
        ward: selectedWard.WardName,
        ward_code: String(selectedWard.WardCode),
        fullAddress: `${formData.street}, ${selectedWard.WardName}, ${selectedDistrict.DistrictName}, ${selectedProvince.ProvinceName}`
      }
    }
    registerShop(payload, { onSuccess: handleClose })
  }

  return (
    <Dialog
      open={open}
      onClose={handleClose}
      fullWidth
      maxWidth="sm"
      PaperProps={{
        sx: { ...glassSx, bgcolor: 'primary.main', borderRadius: 3, border: '1px solid', borderColor: 'divider', m: { xs: 1.5, sm: 3 } }
      }}
    >
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1, pb: 1.25 }}>
        <StorefrontIcon sx={{ color: '#8b5cf6', fontSize: 22 }} />
        <Typography sx={{ fontSize: '1rem', fontWeight: 700 }}>Đăng ký cửa hàng</Typography>
      </DialogTitle>

      <Divider sx={{ borderColor: 'divider' }} />

      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogContent sx={{ pt: 2.5, pb: 3, display: 'flex', flexDirection: 'column', gap: 2.5 }}>

          {/* Tên shop + SĐT liên hệ */}
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField
              label="Tên cửa hàng"
              size="small"
              fullWidth
              error={!!errors.name}
              helperText={errors.name?.message}
              {...register('name', {
                required: 'Vui lòng nhập tên cửa hàng',
                minLength: { value: 2, message: 'Tối thiểu 2 ký tự' },
                maxLength: { value: 100, message: 'Tối đa 100 ký tự' }
              })}
            />
            <TextField
              label="Số điện thoại liên hệ"
              size="small"
              fullWidth
              error={!!errors.contact}
              helperText={errors.contact?.message}
              {...register('contact', {
                pattern: { value: /^(0[3-9]\d{8})?$/, message: 'SĐT không hợp lệ' }
              })}
            />
          </Box>

          <Divider sx={{ borderColor: 'divider', opacity: 0.5 }}>
            <Typography sx={{ fontSize: '0.72rem', color: 'text.secondary', px: 1 }}>Địa chỉ cửa hàng</Typography>
          </Divider>

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
            label="Số nhà, tên đường..."
            size="small"
            fullWidth
            multiline
            rows={2}
            error={!!errors.street}
            helperText={errors.street?.message}
            {...register('street', { required: 'Vui lòng nhập địa chỉ chi tiết' })}
          />

          {/* Actions */}
          <Box sx={{ display: 'flex', gap: 1.5, justifyContent: 'flex-end', mt: 0.5 }}>
            <Button
              variant="outlined"
              onClick={handleClose}
              disabled={isRegistering}
              sx={{ borderRadius: 2, borderColor: 'divider', color: 'primary.contrastText', px: 2.5 }}
            >
              Hủy
            </Button>
            <Button
              type="submit"
              variant="contained"
              disabled={isRegistering}
              startIcon={isRegistering ? <CircularProgress size={14} color="inherit" /> : <StorefrontIcon sx={{ fontSize: '15px !important' }} />}
              sx={{
                borderRadius: 2, fontWeight: 700, px: 2.5,
                background: 'linear-gradient(90deg,#0095ff,#14c3eb)', color: '#fff'
              }}
            >
              {isRegistering ? 'Đang đăng ký...' : 'Đăng ký ngay'}
            </Button>
          </Box>
        </DialogContent>
      </form>
    </Dialog >
  )
}

export default RegisterShopDialog
