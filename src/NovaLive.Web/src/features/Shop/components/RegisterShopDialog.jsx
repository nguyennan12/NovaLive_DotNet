import StorefrontIcon from '@mui/icons-material/Storefront'
import { Box, Button, CircularProgress, Dialog, DialogContent, DialogTitle, Divider, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'react-toastify'
import { uploadKycDocumentAPI } from '~/common/apis/services/uploadService'
import { glassSx } from '~/theme'
import { useShopMutation } from '../hooks/useShop'

const documentFields = [
  { name: 'idCardFront', label: 'Ảnh giấy tờ tùy thân (mặt trước)' },
  { name: 'idCardBack', label: 'Ảnh giấy tờ tùy thân (mặt sau)' }
]

const RegisterShopDialog = ({ open, onClose }) => {
  const { registerShop, isRegistering } = useShopMutation()
  const [uploads, setUploads] = useState({})
  const [uploadingField, setUploadingField] = useState(null)
  const { register, handleSubmit, reset, formState: { errors } } = useForm({
    defaultValues: { shopName: '', description: '', phone: '', email: '', bankAccount: '', bankName: '', bankBranch: '' }
  })

  const handleClose = () => {
    setUploads({})
    reset()
    onClose()
  }

  const handleDocumentUpload = async (field, file) => {
    if (!file) return
    setUploadingField(field)
    try {
      const result = await uploadKycDocumentAPI(file)
      setUploads(current => ({ ...current, [field]: result?.url || '' }))
      if (!result?.url) toast.error('Tải giấy tờ lên thất bại')
    } catch (error) {
      toast.error(error?.response?.data?.message || 'Tải giấy tờ lên thất bại')
    } finally {
      setUploadingField(null)
    }
  }

  const onSubmit = (form) => registerShop({
    ...form,
    idCardFront: uploads.idCardFront,
    idCardBack: uploads.idCardBack
  }, { onSuccess: handleClose })

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm" PaperProps={{
      sx: { ...glassSx, bgcolor: 'primary.main', borderRadius: 3, border: '1px solid', borderColor: 'divider', m: { xs: 1.5, sm: 3 } }
    }}>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1, pb: 1.25 }}>
        <StorefrontIcon sx={{ color: '#8b5cf6', fontSize: 22 }} />
        <Typography sx={{ fontSize: '1rem', fontWeight: 700 }}>Đăng ký cửa hàng</Typography>
      </DialogTitle>
      <Divider sx={{ borderColor: 'divider' }} />
      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogContent sx={{ pt: 2.5, pb: 3, display: 'flex', flexDirection: 'column', gap: 2 }}>
          <TextField label="Tên cửa hàng" size="small" error={!!errors.shopName} helperText={errors.shopName?.message}
            {...register('shopName', { required: 'Vui lòng nhập tên cửa hàng', maxLength: { value: 100, message: 'Tối đa 100 ký tự' } })} />
          <TextField label="Mô tả cửa hàng" size="small" multiline minRows={2} error={!!errors.description} helperText={errors.description?.message}
            {...register('description', { required: 'Vui lòng nhập mô tả cửa hàng' })} />
          <Box sx={{ display: 'flex', gap: 2 }}>
            <TextField label="Số điện thoại" size="small" fullWidth error={!!errors.phone} helperText={errors.phone?.message}
              {...register('phone', { required: 'Vui lòng nhập số điện thoại', pattern: { value: /^(0[3-9]\d{8})$/, message: 'Số điện thoại không hợp lệ' } })} />
            <TextField label="Email" size="small" fullWidth error={!!errors.email} helperText={errors.email?.message}
              {...register('email', { required: 'Vui lòng nhập email', pattern: { value: /^\S+@\S+\.\S+$/, message: 'Email không hợp lệ' } })} />
          </Box>

          <Divider sx={{ borderColor: 'divider', opacity: 0.5 }}>Thông tin xác minh</Divider>
          {documentFields.map(({ name, label }) => (
            <Box key={name}>
              <Typography component="label" htmlFor={name} sx={{ display: 'block', mb: 0.75, fontSize: '0.8rem' }}>{label}</Typography>
              <Button component="label" variant="outlined" size="small" disabled={uploadingField === name}>
                {uploadingField === name ? <CircularProgress size={16} /> : uploads[name] ? 'Đã tải lên' : 'Chọn ảnh'}
                <input id={name} hidden type="file" accept="image/*" onChange={event => handleDocumentUpload(name, event.target.files?.[0])} />
              </Button>
              {!uploads[name] && <Typography component="span" sx={{ ml: 1, color: 'error.main', fontSize: '0.75rem' }}>Bắt buộc</Typography>}
            </Box>
          ))}

          <Divider sx={{ borderColor: 'divider', opacity: 0.5 }}>Thông tin ngân hàng</Divider>
          <TextField label="Số tài khoản" size="small" error={!!errors.bankAccount} helperText={errors.bankAccount?.message}
            {...register('bankAccount', { required: 'Vui lòng nhập số tài khoản' })} />
          <TextField label="Tên ngân hàng" size="small" error={!!errors.bankName} helperText={errors.bankName?.message}
            {...register('bankName', { required: 'Vui lòng nhập tên ngân hàng' })} />
          <TextField label="Chi nhánh (không bắt buộc)" size="small" {...register('bankBranch')} />

          <Box sx={{ display: 'flex', gap: 1.5, justifyContent: 'flex-end', mt: 0.5 }}>
            <Button variant="outlined" onClick={handleClose} disabled={isRegistering}>Hủy</Button>
            <Button type="submit" variant="contained" disabled={isRegistering || uploadingField !== null || !uploads.idCardFront || !uploads.idCardBack}
              startIcon={isRegistering ? <CircularProgress size={14} color="inherit" /> : <StorefrontIcon sx={{ fontSize: '15px !important' }} />}
              sx={{ borderRadius: 2, fontWeight: 700, px: 2.5, background: 'linear-gradient(90deg,#0095ff,#14c3eb)', color: '#fff' }}>
              {isRegistering ? 'Đang đăng ký...' : 'Đăng ký ngay'}
            </Button>
          </Box>
        </DialogContent>
      </form>
    </Dialog>
  )
}

export default RegisterShopDialog
