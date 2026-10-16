import styled from '@emotion/styled'
import CameraAltIcon from '@mui/icons-material/CameraAlt'
import { Avatar, Box, IconButton, Skeleton, Stack, Tooltip, Typography } from '@mui/material'
import { useQueryClient } from '@tanstack/react-query'
import { useSelector } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { uploadAvatarAPI } from '~/common/apis/services/uploadService'
import { useUpload } from '~/common/hooks/useUpload'
import { selectCurrentUser } from '~/store/user/userSlice'
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
  const currentUser = useSelector(selectCurrentUser)
  const queryClient = useQueryClient()

  const uploadAvatar = useUpload({
    api: uploadAvatarAPI,
    afterUpload: () => queryClient.refetchQueries({ queryKey: ['my_profile', profile?._id] }),
    pendingMsg: 'Đang tải lên ảnh đại diện...',
    successMsg: 'Uploaded avatar!',
    errorMsg: 'Upload avatar failed!'
  })

  if (isLoading) return <UserHeaderSkeleton />

  return (
    <Box sx={{ borderRadius: 3, overflow: 'hidden', boxShadow: '0 2px 16px rgba(0,0,0,0.06)', ...glassSx }}>
      <Banner />
      <Box sx={{ px: 3, pb: 3, position: 'relative' }}>
        <Box sx={{ position: 'relative', display: 'inline-block', mt: '-48px', mb: 1 }}>
          <Avatar
            src={profile?.user_avatar || ''}
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
              <input {...uploadAvatar.fileInputProps} />
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
            <ProfileActionButton currentUser={currentUser} isShop={false} navigate={navigate} />
          </Stack>
        </Stack>
      </Box>
    </Box>
  )
}

export default UserProfileHeader
