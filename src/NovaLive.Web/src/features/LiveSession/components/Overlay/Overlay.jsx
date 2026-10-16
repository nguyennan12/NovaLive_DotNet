import { Box } from '@mui/material'

const Overlay = ({ children }) => {
  return (
    <Box sx={{
      position: 'absolute', inset: 0,
      display: 'flex', flexDirection: 'column',
      alignItems: 'center', justifyContent: 'center',
      gap: 1.5, bgcolor: 'rgba(0,0,0,0.6)'
    }}>
      {children}
    </Box>
  )
}

export default Overlay