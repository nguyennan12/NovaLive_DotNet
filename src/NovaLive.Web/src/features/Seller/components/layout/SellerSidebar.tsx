import { Box, Divider, IconButton, ListItemButton, ListItemIcon, ListItemText, Tooltip, Typography } from '@mui/material'
import PlayArrowRounded from '@mui/icons-material/PlayArrowRounded'
import KeyboardDoubleArrowLeftRounded from '@mui/icons-material/KeyboardDoubleArrowLeftRounded'
import KeyboardDoubleArrowRightRounded from '@mui/icons-material/KeyboardDoubleArrowRightRounded'
import { Link, useLocation } from 'react-router-dom'
import { sellerNavigation, type SellerNavItem } from '../../config/navigation'

export function SellerSidebar({
  collapsed,
  onCollapse,
  onNavigate
}: {
  collapsed: boolean
  onCollapse: () => void
  onNavigate?: () => void
}) {
  const location = useLocation()

  const isItemActive = (item: SellerNavItem) => {
    const relPath = location.pathname.replace(/^\/seller\/?/, '')
    const firstSegment = relPath.split('/')[0] || ''
    if (!item.path) {
      return firstSegment === '' || firstSegment === 'dashboard'
    }
    if (firstSegment === item.path) return true
    if (item.aliases?.includes(firstSegment)) return true
    return false
  }

  return (
    <Box
      component='nav'
      aria-label='Menu quản lý cửa hàng'
      sx={{
        width: collapsed ? 72 : 240,
        height: '100%',
        bgcolor: '#ffffff',
        border: '1px solid #f1f5f9',
        borderRadius: '24px',
        boxShadow: '0 4px 24px -2px rgba(15, 23, 42, 0.05)',
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
        transition: 'width 0.25s cubic-bezier(0.4, 0, 0.2, 1)'
      }}
    >
      {/* Header / Brand */}
      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: collapsed ? 'center' : 'space-between',
          px: collapsed ? 1.5 : 2.5,
          py: 2,
          borderBottom: '1px solid #f1f5f9',
          minHeight: 68
        }}
      >
        {!collapsed ? (
          <Box
            component={Link}
            to='/'
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1.25,
              textDecoration: 'none'
            }}
          >
            <Box
              sx={{
                width: 34,
                height: 34,
                borderRadius: '10px',
                background: 'linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                boxShadow: '0 4px 10px rgba(59, 130, 246, 0.3)'
              }}
            >
              <PlayArrowRounded sx={{ color: '#fff', fontSize: 20 }} />
            </Box>
            <Typography
              sx={{
                fontSize: '1.25rem',
                fontWeight: 700,
                background: 'linear-gradient(135deg, #38bdf8 0%, #2563eb 100%)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
                letterSpacing: '-0.02em'
              }}
            >
              NovaLive
            </Typography>
          </Box>
        ) : (
          <Box
            component={Link}
            to='/'
            sx={{
              width: 34,
              height: 34,
              borderRadius: '10px',
              background: 'linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              textDecoration: 'none'
            }}
          >
            <PlayArrowRounded sx={{ color: '#fff', fontSize: 20 }} />
          </Box>
        )}

        <IconButton
          onClick={onCollapse}
          size='small'
          aria-label={collapsed ? 'Mở rộng menu' : 'Thu gọn menu'}
          sx={{
            color: '#94a3b8',
            p: 0.75,
            borderRadius: '8px',
            '&:hover': { color: '#475569', bgcolor: '#f1f5f9' },
            ...(collapsed ? { display: 'none' } : {})
          }}
        >
          <KeyboardDoubleArrowLeftRounded fontSize='small' />
        </IconButton>
      </Box>

      {/* When collapsed, small expand button below logo */}
      {collapsed && (
        <Box sx={{ display: 'flex', justifyContent: 'center', pt: 1 }}>
          <IconButton
            onClick={onCollapse}
            size='small'
            aria-label='Mở rộng menu'
            sx={{
              color: '#94a3b8',
              p: 0.5,
              borderRadius: '8px',
              '&:hover': { color: '#475569', bgcolor: '#f1f5f9' }
            }}
          >
            <KeyboardDoubleArrowRightRounded fontSize='small' />
          </IconButton>
        </Box>
      )}

      {/* Nav List */}
      <Box
        sx={{
          overflowY: 'auto',
          flex: 1,
          py: 1.5,
          px: 1.5,
          '&::-webkit-scrollbar': { width: 4 },
          '&::-webkit-scrollbar-thumb': { bgcolor: '#e2e8f0', borderRadius: 4 }
        }}
      >
        {sellerNavigation.map((section, idx) => (
          <Box key={section.label} sx={{ mb: idx === sellerNavigation.length - 1 ? 0 : 1 }}>
            {section.label === 'PROFILE' && (
              <Divider sx={{ my: 1.5, borderColor: '#f1f5f9' }} />
            )}

            {!collapsed && (
              <Typography
                sx={{
                  px: 1.5,
                  pt: idx === 0 ? 0.5 : 1,
                  pb: 0.75,
                  color: '#94a3b8',
                  fontSize: '0.6875rem',
                  fontWeight: 700,
                  textTransform: 'uppercase',
                  letterSpacing: '0.08em'
                }}
              >
                {section.label}
              </Typography>
            )}

            {section.items.map(item => {
              const path = `/seller${item.path ? `/${item.path}` : ''}`
              const active = isItemActive(item)
              const Icon = item.icon

              return (
                <Tooltip
                  key={item.label}
                  title={collapsed ? item.label : ''}
                  placement='right'
                >
                  <ListItemButton
                    component={Link}
                    to={path}
                    onClick={onNavigate}
                    selected={active}
                    aria-current={active ? 'page' : undefined}
                    sx={{
                      borderRadius: '12px',
                      mb: 0.5,
                      px: collapsed ? 1.5 : 2,
                      py: 1,
                      minHeight: 42,
                      justifyContent: collapsed ? 'center' : 'flex-start',
                      transition: 'all 0.15s ease-in-out',
                      bgcolor: active ? '#5b8bf7 !important' : 'transparent',
                      color: active ? '#ffffff' : '#334155',
                      boxShadow: active ? '0 4px 12px rgba(91, 139, 247, 0.28)' : 'none',
                      '&:hover': {
                        bgcolor: active ? '#4f80ee !important' : '#f8fafc',
                        color: active ? '#ffffff' : '#0f172a'
                      }
                    }}
                  >
                    <ListItemIcon
                      sx={{
                        minWidth: collapsed ? 'auto' : 32,
                        color: 'inherit',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center'
                      }}
                    >
                      <Icon sx={{ fontSize: 20, color: 'inherit' }} />
                    </ListItemIcon>
                    {!collapsed && (
                      <ListItemText
                        primary={item.label}
                        slotProps={{
                          primary: {
                            sx: {
                              fontSize: '0.875rem',
                              fontWeight: active ? 600 : 500,
                              color: 'inherit'
                            }
                          }
                        }}
                      />
                    )}
                  </ListItemButton>
                </Tooltip>
              )
            })}
          </Box>
        ))}
      </Box>
    </Box>
  )
}
