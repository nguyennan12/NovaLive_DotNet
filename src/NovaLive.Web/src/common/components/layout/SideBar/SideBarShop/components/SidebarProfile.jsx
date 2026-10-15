import { Box, Typography } from '@mui/material'
import { PROFILE_ITEMS } from '~/common/configs/sidebar.config'
import SidebarNavItem from './SidebarNavItem'

const SidebarProfile = ({ activeKey, collapsed, onItemClick }) => (
  <Box sx={{ borderTop: '1px solid', borderColor: 'divider', pt: 1.5 }}>
    {!collapsed && (
      <Typography
        sx={{
          fontSize: '0.7rem',
          fontWeight: 500,
          color: 'primary.contrastText',
          opacity: 0.5,
          textTransform: 'uppercase',
          letterSpacing: '0.08em',
          px: 1.5,
          mb: 0.5
        }}
      >
        Profile
      </Typography>
    )}

    {PROFILE_ITEMS.map((item) => (
      <SidebarNavItem
        key={item.key}
        item={item}
        active={activeKey === item.key}
        collapsed={collapsed}
        onClick={onItemClick}
      />
    ))}


  </Box>
)

export default SidebarProfile