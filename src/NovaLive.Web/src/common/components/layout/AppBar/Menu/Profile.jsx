import Logout from '@mui/icons-material/Logout'
import PersonIcon from '@mui/icons-material/Person'
import Settings from '@mui/icons-material/Settings'
import Avatar from '@mui/material/Avatar'
import Box from '@mui/material/Box'
import Divider from '@mui/material/Divider'
import IconButton from '@mui/material/IconButton'
import ListItemIcon from '@mui/material/ListItemIcon'
import Menu from '@mui/material/Menu'
import MenuItem from '@mui/material/MenuItem'
import Tooltip from '@mui/material/Tooltip'
import { useConfirm } from 'material-ui-confirm'
import React from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { logoutUserAPI, selectCurrentUser } from '~/store/user/userSlice'

function Profiles() {
  const [anchorEl, setAnchorEl] = React.useState(null)
  const open = Boolean(anchorEl)

  const dispatch = useDispatch()
  const currentUser = useSelector(selectCurrentUser)
  const confirmLogout = useConfirm()
  const navigate = useNavigate()

  const handleLogout = async () => {
    const { confirmed } = await confirmLogout({
      title: 'Log out of your account?',
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
  return (
    <Box>
      <Tooltip title="Account settings">
        <IconButton
          onClick={handleClick}
          size="small"
          sx={{ padding: 0 }}
          aria-controls={open ? 'basic-menu-profiles' : undefined}
          aria-haspopup="true"
          aria-expanded={open ? 'true' : undefined}
        >
          <Avatar sx={{ width: 30, height: 30 }} src={currentUser?.user_avatar} alt="avatar user" />
        </IconButton>
      </Tooltip>

      <Menu
        id="basic-menu-recent"
        anchorEl={anchorEl}
        open={open}
        onClose={handleClose}
        onClick={handleClose}
        MenuListProps={{
          'aria-labelledby': 'basic-button-recent'
        }}
      >
        <MenuItem onClick={() => { handleClose(); navigate('/profile') }}>
          <Avatar sx={{ width: '28px', height: '28px', mr: 2 }} src={currentUser?.user_avatar} />
          {currentUser?.user_name || 'Hồ sơ của tôi'}
        </MenuItem>
        <Divider />
        <MenuItem onClick={() => { handleClose(); navigate('/profile') }}>
          <ListItemIcon>
            <PersonIcon fontSize="small" />
          </ListItemIcon>
          Hồ sơ cá nhân
        </MenuItem>
        <MenuItem>
          <ListItemIcon>
            <Settings fontSize="small" />
          </ListItemIcon>
          Cài đặt
        </MenuItem>
        <MenuItem onClick={handleLogout} sx={{
          '&:hover': {
            color: '#d91515',
            '& .logout-icon': {
              color: '#d91515'
            }
          }
        }}>
          <ListItemIcon>
            <Logout className='logout-icon' fontSize="small" />
          </ListItemIcon>
          Logout
        </MenuItem>


      </Menu>
    </Box>
  )
}

export default Profiles