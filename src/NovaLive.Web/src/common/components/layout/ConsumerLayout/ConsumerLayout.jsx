import Box from '@mui/material/Box'
import { useTheme } from '@mui/material/styles'
import { Outlet } from 'react-router-dom'
import { WaterDropBackground } from '../../common/style/WaterDropBackground'
import AppBar from '../AppBar/AppBar'
import BottomActionBar from '../BottomActionBar/BottomActionBar'
import Footer from '../Footer/Footer'

function ConsumerLayout() {
  const theme = useTheme()
  return (
    <Box sx={{ minHeight: '100vh' }}>
      <AppBar />

      <WaterDropBackground>
        <Box component="main" sx={{ pt: theme.app.appBarHeight }}>
          <Outlet />
        </Box>
        <Footer />
      </WaterDropBackground>

      <BottomActionBar />
    </Box>
  )
}

export default ConsumerLayout
