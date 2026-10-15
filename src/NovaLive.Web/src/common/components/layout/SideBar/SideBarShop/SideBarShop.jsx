import KeyboardDoubleArrowLeftRoundedIcon from '@mui/icons-material/KeyboardDoubleArrowLeftRounded'
import KeyboardDoubleArrowRightRoundedIcon from '@mui/icons-material/KeyboardDoubleArrowRightRounded'
import PlayArrowIcon from '@mui/icons-material/PlayArrow'
import { Box, IconButton, Typography, useMediaQuery, useTheme } from '@mui/material'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { NAV_SECTIONS } from '~/common/configs/sidebar.config'
import { gradientText } from '~/theme'
import SidebarProfile from './components/SidebarProfile'
import SidebarSection from './components/SidebarSection'

const SIDEBAR_EXPANDED = 240
const SIDEBAR_COLLAPSED = 60

const Sidebar = () => {
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))

  const [collapsed, setCollapsed] = useState(isMobile)
  const [prevIsMobile, setPrevIsMobile] = useState(isMobile)
  const [activeKey, setActiveKey] = useState('dashboard')

  // Set state during render (React pattern) thay vì useEffect để tránh cascade render
  if (prevIsMobile !== isMobile) {
    setPrevIsMobile(isMobile)
    setCollapsed(isMobile)
  }

  const width = collapsed ? SIDEBAR_COLLAPSED : SIDEBAR_EXPANDED
  const navigate = useNavigate()

  return (
    <Box
      component='nav'
      sx={{
        width,
        minWidth: width,
        height: '100%',
        bgcolor: 'primary.main',
        borderRight: '1px solid',
        borderColor: 'divider',
        display: 'flex',
        flexDirection: 'column',
        transition: 'width 0.2s ease',
        overflow: 'hidden',
        borderRadius: '12px'
      }}
    >
      {/* Header */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: collapsed ? 'center' : 'space-between',
          px: collapsed ? 1 : 2,
          py: 1.5,
          borderBottom: '1px solid',
          borderColor: 'divider',
          minHeight: 56,
          cursor: 'pointer'
        }}
      >
        <Box
          onClick={() => navigate('/')}
          sx={{ flexShrink: 0, cursor: 'pointer', userSelect: 'none', display: 'flex', alignItems: 'center', gap: 0.75 }}
        >
          <Box sx={{
            width: 28, height: 28, borderRadius: '8px',
            background: 'linear-gradient(90deg, #69bef7ff, #53e6eeff)',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            boxShadow: '0 2px 8px rgba(52,133,247,0.3)'
          }}>
            <PlayArrowIcon sx={{ color: '#fff', fontSize: '1.25rem' }} />
          </Box>
          {!collapsed && (
            <Typography variant="h6" sx={{
              fontWeight: 800,
              fontSize: { xs: '1rem', sm: '1.1rem' },
              ...gradientText,
              letterSpacing: '-0.4px',
              lineHeight: 1
            }}>
              NovaLive
            </Typography>
          )}
        </Box>

        <IconButton
          size='small'
          onClick={() => setCollapsed((c) => !c)}
          sx={{
            color: 'primary.contrastText',
            opacity: 0.5,
            '&:hover': { opacity: 1, bgcolor: 'action.hover' }
          }}
        >
          {collapsed
            ? <KeyboardDoubleArrowRightRoundedIcon sx={{ fontSize: 16 }} />
            : <KeyboardDoubleArrowLeftRoundedIcon sx={{ fontSize: 16 }} />
          }
        </IconButton>
      </Box>

      {/* Nav sections */}
      <Box sx={{ flex: 1, overflowY: 'auto', px: collapsed ? 1 : 1.5, py: 2 }}>
        {NAV_SECTIONS.map((section) => (
          <SidebarSection
            key={section.label}
            section={section}
            collapsed={collapsed}
            onItemClick={setActiveKey}
          />
        ))}
      </Box>

      {/* Profile */}
      <Box sx={{ px: collapsed ? 1 : 1.5, pb: 1.5 }}>
        <SidebarProfile
          activeKey={activeKey}
          collapsed={collapsed}
          onItemClick={setActiveKey}
        />
      </Box>
    </Box >
  )
}

export default Sidebar