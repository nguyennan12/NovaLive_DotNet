import { Box } from '@mui/material'
import { Outlet } from 'react-router-dom'
import Sidebar from '../SideBar/SideBarShop/SideBarShop'

const DashboardShop = () => {
  return (
    <Box sx={{ background: 'linear-gradient(135deg, #f8faff 0%, #f0f4ff 50%, #f5f8ff 100%)', minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <Box sx={{ display: 'flex', flex: 1 }}>

        {/* Wrapper của Sidebar */}
        <Box
          sx={{
            p: { xs: 1, sm: 2 },
            height: '100dvh',
            position: 'sticky',
            top: 0,
            display: 'flex',
            flexDirection: 'column'
          }}
        >
          <Sidebar />
        </Box>

        {/* Main Content */}
        <Box
          component='main'
          sx={{
            flex: 1,
            p: { xs: 2, sm: 3 }
          }}
        >
          <Outlet />
        </Box>

      </Box>
    </Box >
  )
}

export default DashboardShop