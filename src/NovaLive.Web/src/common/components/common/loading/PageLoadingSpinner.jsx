import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'

function PageLoadingSpinner() {
  return (
    <Box sx={{ width: '100vw', height: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <CircularProgress />
    </Box>
  )
}

export default PageLoadingSpinner