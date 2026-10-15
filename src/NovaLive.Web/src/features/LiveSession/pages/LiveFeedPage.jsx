import { Box } from '@mui/material'
import { useSelector } from 'react-redux'
import { selectCurrentUser } from '~/store/user/userSlice'
import { LiveFeed } from '../components/LiveFeedPage/LiveFeed'

const LiveFeedPage = () => {
  const currentUser = useSelector(selectCurrentUser)
  return (
    <Box sx={{
      display: 'grid',
      gridTemplateColumns: { xs: '1fr', md: '1fr 400px 1fr' },
      height: '100vh',
      bgcolor: '#ffffffff',
      overflow: 'hidden'
    }}>
      {/* Left - empty sidebar (future) */}
      <Box sx={{ display: { xs: 'none', md: 'block' } }} />

      {/* Center - live feed */}
      <Box sx={{ height: '100vh', position: 'relative' }}>
        <LiveFeed
          userId={currentUser._id}
        />
      </Box>


    </Box>
  )
}

export default LiveFeedPage
