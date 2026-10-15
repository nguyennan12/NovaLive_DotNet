import LockIcon from '@mui/icons-material/Lock'
import { Visibility, VisibilityOff } from '@mui/icons-material'
import {
  Box, Button, CircularProgress, Dialog, DialogActions,
  DialogContent, DialogTitle, IconButton, InputAdornment, TextField
} from '@mui/material'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { PASSWORD_RULE } from '~/common/utils/validators'


const ChangePasswordDialog = ({ open, onClose, onSubmit, isSaving }) => {
  const [show, setShow] = useState({ current: false, new: false, confirm: false })

  const { register, handleSubmit, getValues, reset, formState: { errors } } = useForm({
    defaultValues: { currentPassword: '', newPassword: '', confirmPassword: '' }
  })

  useEffect(() => {
    if (open) reset({ currentPassword: '', newPassword: '', confirmPassword: '' })
  }, [open, reset])

  const toggleShow = (key) => setShow(s => ({ ...s, [key]: !s[key] }))

  const onFormSubmit = ({ currentPassword, newPassword }) => {
    onSubmit({ currentPassword, newPassword }, { onSuccess: onClose })
  }

  const eyeAdornment = (key) => (
    <InputAdornment position="end">
      <IconButton size="small" onClick={() => toggleShow(key)} edge="end">
        {show[key] ? <VisibilityOff fontSize="small" /> : <Visibility fontSize="small" />}
      </IconButton>
    </InputAdornment>
  )

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs" slotProps={{ sx: { borderRadius: 3 } }}>
      <DialogTitle sx={{ fontWeight: 700, fontSize: '1rem', display: 'flex', alignItems: 'center', gap: 1 }}>
        <LockIcon sx={{ color: 'secondary.main', fontSize: 20 }} />
        Đổi mật khẩu
      </DialogTitle>

      <form onSubmit={handleSubmit(onFormSubmit)}>
        <DialogContent>
          <Box sx={{ pt: 1, display: 'flex', flexDirection: 'column', gap: 2 }}>
            <TextField
              fullWidth
              label="Mật khẩu hiện tại"
              type={show.current ? 'text' : 'password'}
              error={!!errors.currentPassword}
              helperText={errors.currentPassword?.message}
              slotProps={{ endAdornment: eyeAdornment('current') }}
              {...register('currentPassword', { required: 'Vui lòng nhập mật khẩu hiện tại' })}
            />
            <TextField
              fullWidth
              label="Mật khẩu mới"
              type={show.new ? 'text' : 'password'}
              error={!!errors.newPassword}
              helperText={errors.newPassword?.message}
              slotProps={{ endAdornment: eyeAdornment('new') }}
              {...register('newPassword', {
                required: 'Vui lòng nhập mật khẩu mới',
                pattern: {
                  value: PASSWORD_RULE,
                  message: 'Mật khẩu phải có ít nhất 8 ký tự, gồm chữ và số'
                }
              })}
            />
            <TextField
              fullWidth
              label="Xác nhận mật khẩu mới"
              type={show.confirm ? 'text' : 'password'}
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword?.message}
              slotProps={{ endAdornment: eyeAdornment('confirm') }}
              {...register('confirmPassword', {
                required: 'Vui lòng xác nhận mật khẩu',
                validate: (val) => val === getValues('newPassword') || 'Mật khẩu xác nhận không khớp'
              })}
            />
          </Box>
        </DialogContent>

        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={onClose} size="small" color="inherit" disabled={isSaving}>Hủy</Button>
          <Button
            type="submit"
            size="small"
            variant="contained"
            disabled={isSaving}
            startIcon={isSaving ? <CircularProgress size={14} sx={{ color: '#fff' }} /> : null}
            sx={{ background: 'linear-gradient(90deg,#0095ff,#14c3eb)', color: '#fff' }}
          >
            Đổi mật khẩu
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default ChangePasswordDialog
