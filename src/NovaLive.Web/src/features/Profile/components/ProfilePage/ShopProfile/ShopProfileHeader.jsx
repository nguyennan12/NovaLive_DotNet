import styled from '@emotion/styled'
import CameraAltIcon from '@mui/icons-material/CameraAlt'
import EditIcon from '@mui/icons-material/Edit'
import GroupIcon from '@mui/icons-material/Group'
import InventoryIcon from '@mui/icons-material/Inventory'
import LocalMallIcon from '@mui/icons-material/LocalMall'
import StarIcon from '@mui/icons-material/Star'
import StorefrontIcon from '@mui/icons-material/Storefront'
import VerifiedIcon from '@mui/icons-material/Verified'
import {
  Avatar, Box, Button, Chip, CircularProgress, Dialog, DialogActions,
  DialogContent, DialogTitle, IconButton, Skeleton, Stack, TextField, Tooltip, Typography
} from '@mui/material'
import { useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useSelector } from 'react-redux'
import { uploadShopLogoAPI } from '~/common/apis/services/uploadService'
import { useUpload } from '~/common/hooks/useUpload'
import { useShop, useShopMutation } from '~/features/Shop/hooks/useShop'
import { selectCurrentUser } from '~/store/user/userSlice'
import { glassSx } from '~/theme'

const Banner = styled(Box)({
  height: 160,
  background: 'linear-gradient(135deg, #0095ff 0%, #14c3eb 40%, #8b5cf6 100%)',
  position: 'relative',
  borderRadius: '12px 12px 0 0',
  overflow: 'hidden',
  '@media (min-width: 600px)': { height: 200 },
  '&::after': {
    content: '""', position: 'absolute', inset: 0,
    background: 'rgba(0,0,0,0.1)'
  }
})

const HeaderStat = ({ icon: Icon, label, value, color }) => (
  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
    <Icon sx={{ fontSize: 14, color }} />
    <Typography sx={{ fontWeight: 700, fontSize: '0.83rem', color: 'primary.contrastText', lineHeight: 1 }}>
      {value}
    </Typography>
    <Typography sx={{ fontSize: '0.72rem', color: 'text.secondary', lineHeight: 1 }}>
      {label}
    </Typography>
  </Box>
)

const EditShopDialog = ({ open, onClose, shop, onSave, isSaving }) => {
  const { register, handleSubmit, reset, formState: { errors } } = useForm({
    defaultValues: {
      shop_name: shop?.shop_name || '',
      contact_phone: shop?.shop_contact?.phone || '',
      contact_email: shop?.shop_contact?.email || ''
    }
  })

  useEffect(() => {
    if (open) reset({
      shop_name: shop?.shop_name || '',
      contact_phone: shop?.shop_contact?.phone || '',
      contact_email: shop?.shop_contact?.email || ''
    })
  }, [open, shop, reset])

  const onSubmit = ({ shop_name, contact_phone, contact_email }) => {
    onSave(
      { shopId: shop._id, data: { shop_name, shop_contact: { phone: contact_phone, email: contact_email } } },
      { onSuccess: onClose }
    )
  }

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="xs">
      <form onSubmit={handleSubmit(onSubmit)}>
        <DialogTitle sx={{ fontWeight: 700, fontSize: '1rem' }}>Chỉnh sửa cửa hàng</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 1.5 }}>
            <TextField
              fullWidth size="small" label="Tên cửa hàng"
              error={!!errors.shop_name} helperText={errors.shop_name?.message}
              {...register('shop_name', {
                required: 'Vui lòng nhập tên cửa hàng',
                minLength: { value: 2, message: 'Tối thiểu 2 ký tự' }
              })}
            />
            <TextField
              fullWidth size="small" label="SĐT liên hệ"
              error={!!errors.contact_phone} helperText={errors.contact_phone?.message}
              {...register('contact_phone', {
                pattern: { value: /^(0[3-9]\d{8})?$/, message: 'SĐT không hợp lệ' }
              })}
            />
            <TextField
              fullWidth size="small" label="Email liên hệ"
              error={!!errors.contact_email} helperText={errors.contact_email?.message}
              {...register('contact_email', {
                pattern: { value: /^\S+@\S+\.\S+$/, message: 'Email không hợp lệ' }
              })}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} color="inherit" disabled={isSaving}>Hủy</Button>
          <Button
            type="submit" variant="contained" disabled={isSaving}
            startIcon={isSaving ? <CircularProgress size={14} color="inherit" /> : null}
            sx={{ background: 'linear-gradient(90deg,#0095ff,#14c3eb)', color: '#fff' }}
          >
            Lưu
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

function ShopHeaderSkeleton() {
  return (
    <Box sx={{ borderRadius: 3, overflow: 'hidden', bgcolor: 'primary.main', boxShadow: '0 2px 16px rgba(0,0,0,0.06)' }}>
      <Skeleton variant="rectangular" sx={{ height: { xs: 160, sm: 200 } }} />
      <Box sx={{ px: 3, pb: 3 }}>
        <Skeleton variant="circular" width={96} height={96} sx={{ mt: '-48px', mb: 1 }} />
        <Skeleton width={180} height={32} />
        <Skeleton width={130} height={20} sx={{ mt: 0.5 }} />
        <Skeleton width={260} height={18} sx={{ mt: 1 }} />
      </Box>
    </Box>
  )
}

const ShopProfileHeader = () => {
  const currentUser = useSelector(selectCurrentUser)
  const queryClient = useQueryClient()
  const { shop, isLoading } = useShop()
  const { updateShop, isUpdatingShop } = useShopMutation()
  const [editOpen, setEditOpen] = useState(false)

  const uploadShopLogo = useUpload({
    api: uploadShopLogoAPI,
    afterUpload: () => queryClient.refetchQueries({ queryKey: ['my_shop', currentUser?._id] }),
    pendingMsg: 'Đang tải lên logo cửa hàng...',
    successMsg: 'Cập nhật logo thành công!',
    errorMsg: 'Upload logo thất bại!'
  })

  if (isLoading) return <ShopHeaderSkeleton />

  const metrics = shop?.shop_metrics || {}

  return (
    <>
      <Box sx={{ borderRadius: 3, overflow: 'hidden', boxShadow: '0 2px 16px rgba(0,0,0,0.06)', ...glassSx }}>
        <Banner />
        <Box sx={{ px: 3, pb: 3, position: 'relative' }}>
          <Box sx={{ position: 'relative', display: 'inline-block', mt: '-48px', mb: 1 }}>
            <Avatar
              src={shop?.shop_logo || ''}
              sx={{
                width: 96, height: 96,
                border: '4px solid #fff',
                boxShadow: '0 4px 16px rgba(0,0,0,0.15)',
                fontSize: '2.2rem',
                background: 'linear-gradient(135deg,#8b5cf6,#0095ff)'
              }}
            >
              {shop?.shop_name?.[0]?.toUpperCase()}
            </Avatar>
            <Tooltip title="Đổi logo cửa hàng">
              <IconButton
                size="small"
                sx={{
                  position: 'absolute', bottom: 4, right: 0,
                  background: 'rgba(255,255,255,0.95)',
                  border: '1px solid', borderColor: 'divider',
                  width: 26, height: 26,
                  '&:hover': { background: '#fff' }
                }}
                onClick={uploadShopLogo.openFileDialog}
              >
                <input {...uploadShopLogo.fileInputProps} />
                <CameraAltIcon sx={{ fontSize: 13 }} />
              </IconButton>
            </Tooltip>
          </Box>

          <Stack sx={{ flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', gap: 1 }}>
            <Box>
              <Stack direction="row" alignItems="center" gap={0.75}>
                <Typography variant="h5" fontWeight={700} sx={{ color: 'primary.contrastText', lineHeight: 1.2 }}>
                  {shop?.shop_name || 'Cửa hàng'}
                </Typography>
                <Tooltip title="Tài khoản Shop đã xác thực">
                  <VerifiedIcon sx={{ color: '#0095ff', fontSize: 22 }} />
                </Tooltip>
              </Stack>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
                @{shop?.shop_slug}
              </Typography>
              <Chip
                icon={<StorefrontIcon sx={{ fontSize: '14px !important', color: '#fff !important' }} />}
                label={shop?.shop_status === 'active' ? 'Đang hoạt động' : (shop?.shop_status || 'Cửa hàng')}
                size="small"
                sx={{
                  mt: 0.75,
                  background: shop?.shop_status === 'active'
                    ? 'linear-gradient(90deg,#10b981,#14c3eb)'
                    : 'linear-gradient(90deg,#0095ff,#14c3eb)',
                  color: '#fff', fontWeight: 600, fontSize: '0.72rem', height: 22
                }}
              />
              <Box sx={{ display: 'flex', gap: 2, mt: 1.25, flexWrap: 'wrap', alignItems: 'center' }}>
                <HeaderStat icon={StarIcon} label="đánh giá" value={metrics.rating_avg?.toFixed(1) || '0.0'} color="#f59e0b" />
                <HeaderStat icon={InventoryIcon} label="sản phẩm" value={metrics.total_products ?? 0} color="#8b5cf6" />
                <HeaderStat icon={LocalMallIcon} label="đã bán" value={metrics.total_sold ?? 0} color="#14c3eb" />
                <HeaderStat icon={GroupIcon} label="theo dõi" value={metrics.follower_count ?? 0} color="#10b981" />
              </Box>
            </Box>

            <Stack sx={{ gap: 1, flexDirection: 'row', alignItems: 'flex-start', pt: { xs: 0, sm: 0.5 } }}>
              <Button
                size="small" variant="outlined"
                startIcon={<EditIcon sx={{ fontSize: '15px !important' }} />}
                onClick={() => setEditOpen(true)}
                sx={{
                  borderColor: '#8b5cf6', color: '#8b5cf6', fontSize: '0.8rem', py: 0.6,
                  '&:hover': { background: 'rgba(139,92,246,0.08)' }
                }}
              >
                Chỉnh sửa shop
              </Button>
            </Stack>
          </Stack>
        </Box>
      </Box>

      <EditShopDialog
        open={editOpen}
        onClose={() => setEditOpen(false)}
        shop={shop}
        onSave={updateShop}
        isSaving={isUpdatingShop}
      />
    </>
  )
}

export default ShopProfileHeader
