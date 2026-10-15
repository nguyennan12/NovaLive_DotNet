import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Grid, MenuItem, TextField } from '@mui/material'
import { useTheme } from '@mui/material/styles'
import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'

const GENDER_OPTIONS = [
  { label: 'Nam', value: 'male' },
  { label: 'Nữ', value: 'female' },
  { label: 'Khác', value: 'other' }
]

const toDefaultValues = (profile) => ({
  user_name: profile?.user_name || '',
  user_phone: profile?.user_phone || '',
  user_gender: profile?.user_gender || 'other',
  user_birthday: profile?.user_birthday ? profile.user_birthday.slice(0, 10) : ''
})

const FullEditProfileDialog = ({ open, onClose, profile, onSave, isSaving }) => {
  const theme = useTheme()
  const dateSx = theme.customStyles?.discountForm?.dateSx || {}

  const { register, handleSubmit, control, reset, formState: { errors } } = useForm({
    defaultValues: toDefaultValues(profile)
  })


  useEffect(() => {
    if (open) reset(toDefaultValues(profile))
  }, [open, profile, reset])

  const onSubmit = (data) => {
    onSave(data, { onSuccess: onClose })
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogTitle>Chỉnh sửa thông tin cá nhân</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ pt: 3 }}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Họ và tên"
                size="small"
                variant="outlined"
                error={!!errors.user_name}
                helperText={errors.user_name?.message}
                {...register('user_name', { required: 'Vui lòng nhập họ và tên' })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label="Số điện thoại"
                size="small"
                variant="outlined"
                type="tel"
                error={!!errors.user_phone}
                helperText={errors.user_phone?.message}
                {...register('user_phone', {
                  pattern: {
                    value: /^(0[3-9]\d{8})?$/,
                    message: 'Số điện thoại không hợp lệ'
                  }
                })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <Controller
                name="user_gender"
                control={control}
                render={({ field }) => (
                  <TextField
                    select
                    fullWidth
                    label="Giới tính"
                    size="small"
                    variant="outlined"
                    {...field}
                  >
                    {GENDER_OPTIONS.map(opt => (
                      <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>
                    ))}
                  </TextField>
                )}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                type="date"
                size="small"
                variant="outlined"
                sx={dateSx}
                slotProps={{ shrink: true }}
                inputProps={{ max: new Date().toISOString().slice(0, 10) }}
                error={!!errors.user_birthday}
                helperText={errors.user_birthday?.message}
                {...register('user_birthday')}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} color="inherit" disabled={isSaving}>Hủy</Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isSaving}
            sx={{ background: 'linear-gradient(90deg,#0095ff,#14c3eb)', color: '#fff' }}
          >
            Lưu thay đổi
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default FullEditProfileDialog
