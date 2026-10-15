import { Box, Typography, Badge } from '@mui/material'

const SidebarNavItem = ({ item, active, collapsed, onClick }) => {
  const Icon = item.icon

  return (
    <Box
      onClick={() => onClick(item.key)}
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 1.5,
        px: collapsed ? 0 : 1.5,
        py: 1,
        borderRadius: '8px',
        cursor: 'pointer',
        justifyContent: collapsed ? 'center' : 'flex-start',
        transition: 'background 0.15s',
        bgcolor: active ? 'secondary.main' : 'transparent',
        '&:hover': {
          bgcolor: active ? 'secondary.main' : 'action.hover'
        }
      }}
    >
      <Badge
        badgeContent={collapsed ? item.badge : 0}
        color='error'
        variant='dot'
        invisible={!item.badge}
        sx={{ '& .MuiBadge-dot': { top: 2, right: 2 } }}
      >
        <Icon
          sx={{
            fontSize: 20,
            color: active ? 'secondary.contrastText' : 'primary.contrastText',
            flexShrink: 0
          }}
        />
      </Badge>

      {!collapsed && (
        <>
          <Typography
            noWrap
            sx={{
              flex: 1,
              fontSize: '0.875rem',
              fontWeight: active ? 600 : 400,
              color: active ? 'secondary.contrastText' : 'primary.contrastText',
              lineHeight: 1
            }}
          >
            {item.label}
          </Typography>

          {item.badge ? (
            <Box
              sx={{
                minWidth: 22,
                height: 22,
                borderRadius: '6px',
                bgcolor: active ? 'rgba(255,255,255,0.25)' : 'action.selected',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                px: 0.75
              }}
            >
              <Typography
                sx={{
                  fontSize: '0.7rem',
                  fontWeight: 600,
                  color: active ? 'secondary.contrastText' : 'primary.contrastText',
                  lineHeight: 1
                }}
              >
                {item.badge}
              </Typography>
            </Box>
          ) : null}
        </>
      )}
    </Box>
  )
}

export default SidebarNavItem