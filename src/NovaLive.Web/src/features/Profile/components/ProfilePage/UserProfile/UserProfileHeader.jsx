import styled from '@emotion/styled'
import CameraAltIcon from '@mui/icons-material/CameraAlt'
import { Avatar, Box, IconButton, Skeleton, Stack, Tooltip, Typography } from '@mui/material'
import { useQueryClient } from '@tanstack/react-query'
import { useDispatch, useSelector } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { uploadAvatarAPI } from '~/common/apis/services/uploadService'
import { updateMyProfileAPI } from '~/common/apis/services/userService'
import { useUpload } from '~/common/hooks/useUpload'
import { resolveMediaUrl } from '~/common/utils/formatters'
import { useShop } from '~/features/Shop/hooks/useShop'
import { selectCurrentUser, setCurrentUser } from '~/store/user/userSlice'
import { glassSx } from '~/theme'
import ProfileActionButton from '../../shared/renderActionButton'

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

function UserHeaderSkeleton() {
  return (
    <Box sx={{ borderRadius: 3, overflow: 'hidden', bgcolor: 'primary.main', boxShadow: '0 2px 16px rgba(0,0,0,0.06)' }}>
      <Skeleton variant="rectangular" sx={{ height: { xs: 160, sm: 200 } }} />
      <Box sx={{ px: 3, pb: 3 }}>
        <Skeleton variant="circular" width={96} height={96} sx={{ mt: '-48px', mb: 1 }} />
        <Skeleton width={180} height={32} />
        <Skeleton width={130} height={20} sx={{ mt: 0.5 }} />
      </Box>
    </Box>
  )
}

const UserProfileHeader = ({ profile, isLoading }) => {
  const navigate = useNavigate()
  const dispatch = useDispatch()
  const currentUser = useSelector(selectCurrentUser)
  const { shop } = useShop()
  const queryClient = useQueryClient()

  const shopStatus = shop?.shop_status?.toLowerCase()
  const roles = Array.isArray(currentUser?.roles) ? currentUser.roles : [currentUser?.role].filter(Boolean)
  const isShop = roles.some(role => String(role).toLowerCase() === 'seller')
  const isPendingApproval = shopStatus === 'pending'

  const uploadAvatar = useUpload({
    api: async (file) => {
      const uploaded = await uploadAvatarAPI(file)
      const updatedUrl = uploaded?.url || uploaded?.Url || (typeof uploaded === 'string' ? uploaded : null)
      const updatedProfile = await updateMyProfileAPI({
        fullName: profile?.user_name || currentUser?.user_name || '',
        phone: profile?.user_phone || currentUser?.user_phone || null,
        birthday: profile?.user_birthday || currentUser?.user_birthday || null,
        gender: profile?.user_gender || currentUser?.user_gender || null,
        avatarUrl: updatedUrl
      })

      if (currentUser && updatedUrl) {
        dispatch(setCurrentUser({
          ...currentUser,
          user_avatar: updatedUrl,
          avatarUrl: updatedUrl
        }))
      }

      return updatedProfile
    },
    afterUpload: () => {
      queryClient.invalidateQueries({ queryKey: ['my_profile'] })
    },
    pendingMsg: 'Đang tải lên ảnh đại diện...',
    successMsg: 'Cập nhật ảnh đại diện thành công!',
    errorMsg: 'Cập nhật ảnh đại diện thất bại!'
  })

  if (isLoading) return <UserHeaderSkeleton />

  const avatarSrc = resolveMediaUrl(profile?.user_avatar || currentUser?.user_avatar || '')

  return (
    <Box sx={{ borderRadius: 3, overflow: 'hidden', boxShadow: '0 2px 16px rgba(0,0,0,0.06)', ...glassSx }}>
      <Banner />
      <Box sx={{ px: 3, pb: 3, position: 'relative' }}>
        <Box sx={{ position: 'relative', display: 'inline-block', mt: '-48px', mb: 1 }}>
          <Avatar
            src={avatarSrc}
            sx={{
              width: 96, height: 96,
              border: '4px solid #fff',
              boxShadow: '0 4px 16px rgba(0,0,0,0.15)',
              fontSize: '2.2rem',
              background: 'linear-gradient(135deg,#0095ff,#8b5cf6)'
            }}
          >
            {profile?.user_name?.[0]?.toUpperCase()}
          </Avatar>
          <input {...uploadAvatar.fileInputProps} />
          <Tooltip title="Đổi ảnh đại diện">
            <IconButton
              size="small"
              sx={{
                position: 'absolute', bottom: 4, right: 0,
                background: 'rgba(255,255,255,0.95)',
                border: '1px solid', borderColor: 'divider',
                width: 26, height: 26,
                '&:hover': { background: '#fff' }
              }}
              onClick={uploadAvatar.openFileDialog}
            >
              <CameraAltIcon sx={{ fontSize: 13 }} />
            </IconButton>
          </Tooltip>
        </Box>

        <Stack sx={{ flexDirection: { xs: 'column', sm: 'row' }, justifyContent: 'space-between', gap: 1 }}>
          <Box>
            <Typography variant="h5" fontWeight={700} sx={{ color: 'primary.contrastText', lineHeight: 1.2 }}>
              {profile?.user_name || 'Người dùng'}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
              {profile?.user_email}
            </Typography>
          </Box>
          <Stack sx={{ gap: 1, flexDirection: 'row', alignItems: 'flex-start', pt: { xs: 0, sm: 0.5 } }}>
            <ProfileActionButton
              currentUser={currentUser}
              isShop={isShop}
              isPendingApproval={isPendingApproval}
              navigate={navigate}
            />
          </Stack>
        </Stack>
      </Box>
    </Box>
  )
}

export default UserProfileHeader
