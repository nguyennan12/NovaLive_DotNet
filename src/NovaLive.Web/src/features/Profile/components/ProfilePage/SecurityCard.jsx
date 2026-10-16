import ChevronRightIcon from '@mui/icons-material/ChevronRight'
import LinkIcon from '@mui/icons-material/Link'
import LockIcon from '@mui/icons-material/Lock'
import LogoutIcon from '@mui/icons-material/Logout'
import SecurityIcon from '@mui/icons-material/Security'
import { Box, Chip, Divider, List, ListItemButton, ListItemIcon, ListItemText, Paper, Typography } from '@mui/material'
import { useState } from 'react'
import SectionTitle from '../shared/SectionTitle'
import ChangePasswordDialog from './ChangePasswordDialog'
import { glassSx } from '~/theme'

const DISABLED_ITEMS = [
  {
    icon: <SecurityIcon sx={{ color: '#8b5cf6', fontSize: 20 }} />,
    label: 'Xác thực 2 bước (2FA)',
    desc: 'Tăng cường bảo mật tài khoản',
    badge: 'Sắp ra mắt'
  },
  {
    icon: <LinkIcon sx={{ color: '#14c3eb', fontSize: 20 }} />,
    label: 'Tài khoản liên kết',
    desc: 'Google, GitHub, Facebook...',
    badge: 'Sắp ra mắt'
  }
]

const SecurityCard = ({ onChangePassword, isChangingPassword, onLogoutAll }) => {
  const [pwDialogOpen, setPwDialogOpen] = useState(false)

  const activeItems = [
    {
      icon: <LockIcon sx={{ color: 'secondary.main', fontSize: 20 }} />,
      label: 'Đổi mật khẩu',
      desc: 'Cập nhật mật khẩu đăng nhập',
      onClick: () => setPwDialogOpen(true)
    },
    {
      icon: <LogoutIcon sx={{ color: '#f59e0b', fontSize: 20 }} />,
      label: 'Đăng xuất tất cả thiết bị',
      desc: 'Kết thúc mọi phiên đăng nhập khác',
      onClick: onLogoutAll
    }
  ]

  const allItems = [...activeItems, ...DISABLED_ITEMS]

  return (
    <>
      <Paper
        elevation={0}
        sx={{ borderRadius: 3, p: { xs: 2, sm: 2.5 }, bgcolor: 'primary.main', border: '1px solid', borderColor: 'divider', height: '100%', minWidth: '380px', ...glassSx }}
      >
        <SectionTitle title="Bảo mật" icon={SecurityIcon} accentColor="#8b5cf6" />
        <List dense disablePadding>
          {allItems.map((item, idx) => {
            const isDisabled = !item.onClick
            return (
              <Box key={item.label}>
                <ListItemButton
                  disabled={isDisabled}
                  onClick={item.onClick}
                  sx={{ borderRadius: 2, px: 1, opacity: isDisabled ? 0.55 : 1 }}
                >
                  <ListItemIcon sx={{ minWidth: 36 }}>{item.icon}</ListItemIcon>
                  <ListItemText
                    primary={
                      <Typography sx={{ fontSize: '0.87rem', fontWeight: 600 }}>
                        {item.label}
                      </Typography>
                    }
                    secondary={
                      <Typography sx={{ fontSize: '0.74rem', color: 'text.secondary' }}>
                        {item.desc}
                      </Typography>
                    }
                  />
                  {item.badge ? (
                    <Chip label={item.badge} size="small" variant="outlined" sx={{ fontSize: '0.63rem', height: 18 }} />
                  ) : (
                    <ChevronRightIcon sx={{ fontSize: 18, color: 'text.disabled' }} />
                  )}
                </ListItemButton>
                {idx < allItems.length - 1 && <Divider sx={{ my: 0.25, opacity: 0.4 }} />}
              </Box>
            )
          })}
        </List>
      </Paper>

      <ChangePasswordDialog
        open={pwDialogOpen}
        onClose={() => setPwDialogOpen(false)}
        onSubmit={onChangePassword}
        isSaving={isChangingPassword}
      />
    </>
  )
}

export default SecurityCard
