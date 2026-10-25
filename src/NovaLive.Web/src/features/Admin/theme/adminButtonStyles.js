// Shared 2-Color Transition & Translucent Gradient Button Styles for Admin Dashboard
export const adminBtnStyles = {
  // 1. Primary Action (Brand Blue -> Cyan transition)
  primary: {
    background: 'linear-gradient(90deg, #0095ff 0%, #14c3eb 100%)',
    color: '#ffffff',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    boxShadow: '0 2px 10px rgba(0, 149, 255, 0.25)',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '&:hover': {
      background: 'linear-gradient(90deg, #0080db 0%, #0fb0d4 100%)',
      boxShadow: '0 4px 14px rgba(0, 149, 255, 0.38)',
      transform: 'translateY(-1px)'
    },
    '&:active': {
      transform: 'translateY(0)'
    }
  },

  // 2. Secondary / Outlined Translucent (2-color trans: Blue-Cyan translucent)
  transSecondary: {
    background: 'linear-gradient(90deg, rgba(0, 149, 255, 0.08) 0%, rgba(20, 195, 235, 0.08) 100%)',
    border: '1.5px solid rgba(0, 149, 255, 0.35)',
    color: '#0080db',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '&:hover': {
      background: 'linear-gradient(90deg, rgba(0, 149, 255, 0.16) 0%, rgba(20, 195, 235, 0.16) 100%)',
      borderColor: '#0095ff',
      boxShadow: '0 2px 8px rgba(0, 149, 255, 0.18)',
      transform: 'translateY(-1px)'
    },
    '&:active': {
      transform: 'translateY(0)'
    }
  },

  // 3. Success (Emerald -> Cyan transition)
  success: {
    background: 'linear-gradient(90deg, #10b981 0%, #14c3eb 100%)',
    color: '#ffffff',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    boxShadow: '0 2px 10px rgba(16, 185, 129, 0.25)',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '&:hover': {
      background: 'linear-gradient(90deg, #059669 0%, #0fb0d4 100%)',
      boxShadow: '0 4px 14px rgba(16, 185, 129, 0.38)',
      transform: 'translateY(-1px)'
    },
    '&:active': {
      transform: 'translateY(0)'
    }
  },

  // 4. Success Translucent (2-color trans: Green-Cyan translucent)
  transSuccess: {
    background: 'linear-gradient(90deg, rgba(16, 185, 129, 0.1) 0%, rgba(20, 195, 235, 0.1) 100%)',
    border: '1.5px solid rgba(16, 185, 129, 0.4)',
    color: '#059669',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '&:hover': {
      background: 'linear-gradient(90deg, rgba(16, 185, 129, 0.18) 0%, rgba(20, 195, 235, 0.18) 100%)',
      borderColor: '#10b981',
      boxShadow: '0 2px 8px rgba(16, 185, 129, 0.2)',
      transform: 'translateY(-1px)'
    }
  },

  // 5. Danger (Red -> Rose/Crimson transition)
  danger: {
    background: 'linear-gradient(90deg, #ef4444 0%, #dc2626 100%)',
    color: '#ffffff',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    boxShadow: '0 2px 10px rgba(239, 68, 68, 0.25)',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '&:hover': {
      background: 'linear-gradient(90deg, #dc2626 0%, #b91c1c 100%)',
      boxShadow: '0 4px 14px rgba(239, 68, 68, 0.38)',
      transform: 'translateY(-1px)'
    },
    '&:active': {
      transform: 'translateY(0)'
    }
  },

  // 6. Danger Translucent (2-color trans: Red-Orange translucent)
  transDanger: {
    background: 'linear-gradient(90deg, rgba(239, 68, 68, 0.08) 0%, rgba(244, 63, 94, 0.08) 100%)',
    border: '1.5px solid rgba(239, 68, 68, 0.35)',
    color: '#dc2626',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '&:hover': {
      background: 'linear-gradient(90deg, rgba(239, 68, 68, 0.16) 0%, rgba(244, 63, 94, 0.16) 100%)',
      borderColor: '#ef4444',
      boxShadow: '0 2px 8px rgba(239, 68, 68, 0.2)',
      transform: 'translateY(-1px)'
    }
  },

  // 7. Warning / Amber (Amber -> Orange transition)
  warning: {
    background: 'linear-gradient(90deg, #f59e0b 0%, #ea580c 100%)',
    color: '#ffffff',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    boxShadow: '0 2px 10px rgba(245, 158, 11, 0.25)',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '&:hover': {
      background: 'linear-gradient(90deg, #d97706 0%, #c2410c 100%)',
      boxShadow: '0 4px 14px rgba(245, 158, 11, 0.38)',
      transform: 'translateY(-1px)'
    }
  },

  // 8. Warning Translucent (2-color trans: Amber-Orange translucent)
  transWarning: {
    background: 'linear-gradient(90deg, rgba(245, 158, 11, 0.1) 0%, rgba(234, 88, 12, 0.1) 100%)',
    border: '1.5px solid rgba(245, 158, 11, 0.4)',
    color: '#d97706',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '&:hover': {
      background: 'linear-gradient(90deg, rgba(245, 158, 11, 0.18) 0%, rgba(234, 88, 12, 0.18) 100%)',
      borderColor: '#f59e0b',
      boxShadow: '0 2px 8px rgba(245, 158, 11, 0.2)',
      transform: 'translateY(-1px)'
    }
  },

  // 9. Purple (Purple -> Blue transition)
  purple: {
    background: 'linear-gradient(90deg, #8b5cf6 0%, #6366f1 100%)',
    color: '#ffffff',
    fontWeight: 700,
    textTransform: 'none',
    borderRadius: '8px',
    whiteSpace: 'nowrap',
    boxShadow: '0 2px 10px rgba(139, 92, 246, 0.25)',
    transition: 'all 0.25s cubic-bezier(0.4, 0, 0.2, 1)',
    '&:hover': {
      background: 'linear-gradient(90deg, #7c3aed 0%, #4f46e5 100%)',
      boxShadow: '0 4px 14px rgba(139, 92, 246, 0.38)',
      transform: 'translateY(-1px)'
    }
  }
}

export default adminBtnStyles
