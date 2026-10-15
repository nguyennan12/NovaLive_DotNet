export const authCardSx = {
  width: '100%',
  maxWidth: '420px',
  borderRadius: '20px',
  boxShadow: '0 20px 40px -15px rgba(15, 23, 42, 0.08), 0 1px 3px rgba(15, 23, 42, 0.04)',
  border: '1px solid #eef2f6',
  p: { xs: 3, sm: 4 },
  boxSizing: 'border-box',
  bgcolor: '#ffffff'
}

export const authInputSx = {
  '& .MuiOutlinedInput-root': {
    borderRadius: '10px',
    backgroundColor: '#f8fafc',
    fontSize: '0.875rem',
    transition: 'all 0.2s ease',
    '& fieldset': {
      borderColor: '#e2e8f0'
    },
    '&:hover fieldset': {
      borderColor: '#cbd5e1'
    },
    '&.Mui-focused': {
      backgroundColor: '#ffffff',
      boxShadow: '0 0 0 3px rgba(52, 101, 200, 0.12)',
      '& fieldset': {
        borderColor: '#3465c8',
        borderWidth: '1.5px'
      }
    }
  },
  '& .MuiInputLabel-root': {
    color: '#64748b',
    fontSize: '0.875rem',
    '&.Mui-focused': {
      color: '#3465c8',
      fontWeight: 600
    }
  }
}

export const authButtonSx = {
  background: 'linear-gradient(90deg, #3465c8 0%, #4f8ee2 50%, #69bef7 100%)',
  borderRadius: '10px',
  py: 1.25,
  fontSize: '0.95rem',
  fontWeight: 700,
  textTransform: 'none',
  color: '#ffffff',
  boxShadow: '0 4px 14px rgba(52, 101, 200, 0.35)',
  transition: 'all 0.25s ease',
  '&:hover': {
    background: 'linear-gradient(90deg, #2b55ab 0%, #3f79c2 50%, #55a8df 100%)',
    boxShadow: '0 6px 18px rgba(52, 101, 200, 0.45)',
    transform: 'translateY(-1px)'
  },
  '&:active': {
    transform: 'translateY(0)'
  },
  '&:disabled': {
    background: '#94a3b8',
    color: '#ffffff',
    boxShadow: 'none'
  }
}

export const authLogoBoxSx = {
  width: 34,
  height: 34,
  borderRadius: '9px',
  background: 'linear-gradient(135deg, #69bef7ff, #53e6eeff)',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  boxShadow: '0 2px 8px rgba(52,133,247,0.3)'
}
