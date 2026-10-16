import { Box, Typography, Avatar } from '@mui/material'
import KeyboardArrowDownRoundedIcon from '@mui/icons-material/KeyboardArrowDownRounded'

const SidebarStoreSelect = ({ collapsed }) => (
  <Box
    sx={{
      display: 'flex',
      alignItems: 'center',
      gap: collapsed ? 0 : 1,
      px: collapsed ? 0 : 1,
      py: 0.75,
      borderRadius: '8px',
      cursor: 'pointer',
      border: '1px solid',
      borderColor: 'divider',
      justifyContent: collapsed ? 'center' : 'space-between',
      transition: 'border-color 0.15s',
      '&:hover': { borderColor: 'secondary.main' }
    }}
  >
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
      <Avatar
        sx={{
          width: 28,
          height: 28,
          bgcolor: '#3465c8',
          fontSize: '0.75rem',
          fontWeight: 700,
          borderRadius: '6px',
          flexShrink: 0
        }}
      >
        C
      </Avatar>

      {!collapsed && (
        <Typography
          sx={{
            fontSize: '0.875rem',
            fontWeight: 500,
            color: 'primary.contrastText'
          }}
        >
          Capstore
        </Typography>
      )}
    </Box>

    {!collapsed && (
      <KeyboardArrowDownRoundedIcon
        sx={{ fontSize: 18, color: 'primary.contrastText', opacity: 0.5 }}
      />
    )}
  </Box>
)

export default SidebarStoreSelect