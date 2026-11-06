import type { SxProps, Theme } from '@mui/material'

export const sellerBtnStyles: Record<string, SxProps<Theme>> = {
  // 1. Primary Action (Brand Blue -> Cyan transition, White text & icon)
  primary: {
    background: 'linear-gradient(90deg, #0095ff 0%, #14c3eb 100%)',
    color: '#ffffff !important',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    boxShadow: '0 2px 10px rgba(0, 149, 255, 0.25)',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '& .MuiButton-startIcon': {
      color: '#ffffff !important'
    },
    '& .MuiSvgIcon-root': {
      color: '#ffffff !important'
    },
    '&:hover': {
      background: 'linear-gradient(90deg, #0080db 0%, #0fb0d4 100%)',
      boxShadow: '0 4px 14px rgba(0, 149, 255, 0.38)',
      transform: 'translateY(-1px)'
    },
    '&:active': {
      transform: 'translateY(0)'
    },
    '&.Mui-disabled': {
      background: '#e2e8f0 !important',
      color: '#94a3b8 !important',
      boxShadow: 'none',
      '& .MuiButton-startIcon': {
        color: '#94a3b8 !important'
      },
      '& .MuiSvgIcon-root': {
        color: '#94a3b8 !important'
      }
    }
  },

  // 2. Secondary / Outlined Translucent (Brand Blue-Cyan translucent)
  transSecondary: {
    background: 'linear-gradient(90deg, rgba(0, 149, 255, 0.08) 0%, rgba(20, 195, 235, 0.08) 100%)',
    border: '1.5px solid rgba(0, 149, 255, 0.35)',
    color: '#0080db !important',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '& .MuiButton-startIcon': {
      color: '#0080db !important'
    },
    '& .MuiSvgIcon-root': {
      color: '#0080db !important'
    },
    '&:hover': {
      background: 'linear-gradient(90deg, rgba(0, 149, 255, 0.16) 0%, rgba(20, 195, 235, 0.16) 100%)',
      borderColor: '#0095ff',
      boxShadow: '0 2px 8px rgba(0, 149, 255, 0.18)',
      transform: 'translateY(-1px)'
    },
    '&:active': {
      transform: 'translateY(0)'
    },
    '&.Mui-disabled': {
      background: '#f1f5f9 !important',
      borderColor: '#e2e8f0 !important',
      color: '#94a3b8 !important',
      '& .MuiButton-startIcon': {
        color: '#94a3b8 !important'
      },
      '& .MuiSvgIcon-root': {
        color: '#94a3b8 !important'
      }
    }
  },

  // 3. Success (Emerald -> Cyan transition)
  success: {
    background: 'linear-gradient(90deg, #10b981 0%, #14c3eb 100%)',
    color: '#ffffff !important',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    boxShadow: '0 2px 10px rgba(16, 185, 129, 0.25)',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '& .MuiButton-startIcon': {
      color: '#ffffff !important'
    },
    '& .MuiSvgIcon-root': {
      color: '#ffffff !important'
    },
    '&:hover': {
      background: 'linear-gradient(90deg, #059669 0%, #0fb0d4 100%)',
      boxShadow: '0 4px 14px rgba(16, 185, 129, 0.38)',
      transform: 'translateY(-1px)'
    },
    '&:active': {
      transform: 'translateY(0)'
    }
  },

  // 4. Danger Translucent (Red-Rose translucent)
  transDanger: {
    background: 'linear-gradient(90deg, rgba(239, 68, 68, 0.08) 0%, rgba(244, 63, 94, 0.08) 100%)',
    border: '1.5px solid rgba(239, 68, 68, 0.35)',
    color: '#dc2626 !important',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '& .MuiButton-startIcon': {
      color: '#dc2626 !important'
    },
    '& .MuiSvgIcon-root': {
      color: '#dc2626 !important'
    },
    '&:hover': {
      background: 'linear-gradient(90deg, rgba(239, 68, 68, 0.16) 0%, rgba(244, 63, 94, 0.16) 100%)',
      borderColor: '#ef4444',
      boxShadow: '0 2px 8px rgba(239, 68, 68, 0.2)',
      transform: 'translateY(-1px)'
    },
    '&:active': {
      transform: 'translateY(0)'
    }
  },

  // 5. Warning (Amber -> Orange transition)
  warning: {
    background: 'linear-gradient(90deg, #f59e0b 0%, #ea580c 100%)',
    color: '#ffffff !important',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    boxShadow: '0 2px 10px rgba(245, 158, 11, 0.25)',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '& .MuiButton-startIcon': {
      color: '#ffffff !important'
    },
    '& .MuiSvgIcon-root': {
      color: '#ffffff !important'
    },
    '&:hover': {
      background: 'linear-gradient(90deg, #d97706 0%, #c2410c 100%)',
      boxShadow: '0 4px 14px rgba(245, 158, 11, 0.38)',
      transform: 'translateY(-1px)'
    }
  }
}

export default sellerBtnStyles
