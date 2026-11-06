import Logout from '@mui/icons-material/Logout'
import PersonIcon from '@mui/icons-material/Person'
import PersonOutlineRoundedIcon from '@mui/icons-material/PersonOutlineRounded'
import Settings from '@mui/icons-material/Settings'
import AdminPanelSettingsRoundedIcon from '@mui/icons-material/AdminPanelSettingsRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import ShoppingBagOutlinedIcon from '@mui/icons-material/ShoppingBagOutlined'
import Avatar from '@mui/material/Avatar'
import Box from '@mui/material/Box'
import Divider from '@mui/material/Divider'
import IconButton from '@mui/material/IconButton'
import ListItemIcon from '@mui/material/ListItemIcon'
import Menu from '@mui/material/Menu'
import MenuItem from '@mui/material/MenuItem'
import Tooltip from '@mui/material/Tooltip'
import Chip from '@mui/material/Chip'
import Typography from '@mui/material/Typography'
import { useConfirm } from 'material-ui-confirm'
import React from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { logoutUserAPI, selectCurrentUser } from '~/store/user/userSlice'
import { resolveMediaUrl } from '~/common/utils/formatters'

export const iconBoxSx = {
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  width: 36,
  height: 36,
  minWidth: 36,
  bgcolor: 'rgba(83,155,255,0.10)',
  border: '1px solid rgba(83,155,255,0.25)',
  borderRadius: '50%',
  color: 'secondary.main',
  transition: 'all 0.2s ease',
  '&:hover': {
    bgcolor: 'rgba(83,155,255,0.18)',
    borderColor: 'rgba(83,155,255,0.45)'
  }
}

function Profiles() {
  const [anchorEl, setAnchorEl] = React.useState(null)
  const open = Boolean(anchorEl)

  const dispatch = useDispatch()
  const currentUser = useSelector(selectCurrentUser)
  const confirmLogout = useConfirm()
  const navigate = useNavigate()

  const handleLogout = async () => {
    const { confirmed } = await confirmLogout({
      title: 'Đăng xuất khỏi tài khoản?',
      description: 'Bạn có chắc chắn muốn đăng xuất không?',
      confirmationText: 'Đăng xuất',
      cancellationText: 'Hủy',
      allowClose: false
    })
    if (confirmed) {
      dispatch(logoutUserAPI())
    }
  }

  const handleClick = (event) => {
    setAnchorEl(event.currentTarget)
  }
  const handleClose = () => {
    setAnchorEl(null)
  }

  const displayName = currentUser?.user_name || currentUser?.fullName || 'Hồ sơ của tôi'
  const displayEmail = currentUser?.email || ''
  const avatarUrl = resolveMediaUrl(currentUser?.user_avatar || currentUser?.avatarUrl || '')
  const userRoles = Array.isArray(currentUser?.roles) ? currentUser.roles : [currentUser?.role].filter(Boolean)
  const normalizedRoles = userRoles.map(role => String(role).toLowerCase())
  const hasAdminRole = normalizedRoles.includes('admin')
  const hasSellerRole = normalizedRoles.includes('seller')
  const isBuyerOnly = !hasAdminRole && !hasSellerRole

  return (
    <Box>
      <Tooltip title={displayName}>
        <IconButton
          onClick={handleClick}
          size="small"
          sx={{ p: 0.25 }}
          aria-controls={open ? 'basic-menu-profiles' : undefined}
          aria-haspopup="true"
          aria-expanded={open ? 'true' : undefined}
        >
          {avatarUrl ? (
            <Avatar
              sx={{
                width: 36,
                height: 36,
                border: '1.5px solid rgba(83,155,255,0.3)',
                boxShadow: '0 2px 6px rgba(52,133,247,0.15)'
              }}
              src={avatarUrl}
              alt={displayName}
            />
          ) : (
            <Box sx={iconBoxSx}>
              <PersonOutlineRoundedIcon sx={{ fontSize: '1.25rem' }} />
            </Box>
          )}
        </IconButton>
      </Tooltip>

      <Menu
        id="basic-menu-recent"
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        onClick={handleClose}
        PaperProps={{
          sx: {
            mt: 1.5,
            borderRadius: '16px',
            border: '1px solid rgba(52,133,247,0.15)',
            boxShadow: '0 10px 30px rgba(52,133,247,0.12)',
            minWidth: 220
          }
        }}
        transformOrigin={{ horizontal: 'right', vertical: 'top' }}
        anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
        MenuListProps={{
          'aria-labelledby': 'basic-button-recent'
        }}
      >
        <MenuItem onClick={() => { handleClose(); navigate('/profile') }} sx={{ py: 1.25 }}>
          {avatarUrl ? (
            <Avatar sx={{ width: 32, height: 32, mr: 1.5 }} src={avatarUrl} />
          ) : (
            <Box sx={{ ...iconBoxSx, width: 32, height: 32, minWidth: 32, mr: 1.5 }}>
              <PersonOutlineRoundedIcon sx={{ fontSize: '1.1rem' }} />
            </Box>
          )}
          <Box sx={{ overflow: 'hidden' }}>
            <Typography sx={{ fontWeight: 700, fontSize: '0.85rem', color: 'primary.contrastText', lineHeight: 1.2 }}>
              {displayName}
            </Typography>
            {displayEmail && (
              <Typography variant="caption" sx={{ color: 'rgba(45,45,45,0.5)', display: 'block' }}>
                {displayEmail}
              </Typography>
            )}
          </Box>
        </MenuItem>

        <Divider />

        <MenuItem onClick={() => { handleClose(); navigate('/profile') }}>
          <ListItemIcon>
            <PersonIcon fontSize="small" sx={{ color: 'secondary.main' }} />
          </ListItemIcon>
          Hồ sơ cá nhân
        </MenuItem>

        <MenuItem onClick={() => { handleClose(); navigate('/orders') }}>
          <ListItemIcon>
            <ShoppingBagOutlinedIcon fontSize="small" sx={{ color: 'secondary.main' }} />
          </ListItemIcon>
          Đơn mua của tôi
        </MenuItem>

        {(hasAdminRole || hasSellerRole || isBuyerOnly) && <Divider />}

        {hasAdminRole && (
          <MenuItem onClick={() => { handleClose(); navigate('/admin/dashboard') }} sx={{ bgcolor: 'rgba(52, 101, 200, 0.04)' }}>
            <ListItemIcon sx={{ color: '#2563eb' }}>
              <AdminPanelSettingsRoundedIcon fontSize="small" />
            </ListItemIcon>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
              <span>Quản trị sàn</span>
              <Chip label="Admin" size="small" color="primary" sx={{ height: 18, fontSize: '0.65rem', fontWeight: 700, ml: 1 }} />
            </Box>
          </MenuItem>
        )}

        {hasSellerRole && (
          <MenuItem onClick={() => { handleClose(); navigate('/seller') }}>
            <ListItemIcon sx={{ color: '#8b5cf6' }}>
              <StorefrontRoundedIcon fontSize="small" />
            </ListItemIcon>
            Kênh Người Bán
          </MenuItem>
        )}

        {isBuyerOnly && (
          <MenuItem onClick={() => { handleClose(); navigate('/profile') }}>
            <ListItemIcon sx={{ color: '#8b5cf6' }}>
              <StorefrontRoundedIcon fontSize="small" />
            </ListItemIcon>
            Đăng ký bán hàng
          </MenuItem>
        )}

        <Divider />

        <MenuItem onClick={() => { handleClose(); navigate('/profile') }}>
          <ListItemIcon>
            <Settings fontSize="small" />
          </ListItemIcon>
          Cài đặt
        </MenuItem>

        <MenuItem
          onClick={handleLogout}
          sx={{
            color: '#dc2626',
            '&:hover': {
              bgcolor: 'rgba(220,38,38,0.06)'
            }
          }}
        >
          <ListItemIcon>
            <Logout fontSize="small" sx={{ color: '#dc2626' }} />
          </ListItemIcon>
          Đăng xuất
        </MenuItem>
      </Menu>
    </Box>
  )
}

export default Profiles
