import { Box, Typography } from '@mui/material'
import SidebarNavItem from './SidebarNavItem'
import { useNavigate, useLocation } from 'react-router-dom'

const SidebarSection = ({ section, collapsed, onItemClick }) => {
  const navigate = useNavigate()
  const location = useLocation()
  const handleItemClick = (item) => {
    const fullPath = `/dashboard/shop${item.path}`
    navigate(fullPath)
    onItemClick?.(item)
  }
  const isActive = (item) => item.path === ''
    ? location.pathname === '/dashboard/shop'
    : location.pathname.startsWith(`/dashboard/shop${item.path}`)

  return <Box sx={{ mb: 1 }}>
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
        {section.label}
      </Typography>
    )}

    {collapsed && (
      <Box sx={{ height: '1px', bgcolor: 'divider', mx: 1, mb: 1, opacity: 0.6 }} />
    )}

    {section.items.map((item) => (
      <SidebarNavItem
        key={item.key}
        item={item}
        active={isActive(item)}
        collapsed={collapsed}
        onClick={() => handleItemClick(item)}
      />
    ))}
  </Box>
}

export default SidebarSection