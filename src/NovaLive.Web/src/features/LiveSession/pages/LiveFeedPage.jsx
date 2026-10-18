import { Box } from '@mui/material'
import { useSelector } from 'react-redux'
import { useMemo } from 'react'
import { selectCurrentUser } from '~/store/user/userSlice'
import { LiveFeed } from '../components/LiveFeedPage/LiveFeed'

const LiveFeedPage = () => {
  const currentUser = useSelector(selectCurrentUser)
  const userId = useMemo(() => {
    if (currentUser?.id || currentUser?._id) {
      return String(currentUser.id || currentUser._id)
    }
    return `guest-${Math.random().toString(36).substring(2, 9)}`
  }, [currentUser])

  return (
    <Box sx={{
      display: 'grid',
      gridTemplateColumns: { xs: '1fr', md: '1fr 400px 1fr' },
      height: '100vh',
      bgcolor: '#ffffff',
      overflow: 'hidden'
    }}>
      {/* Left - empty sidebar */}
      <Box sx={{ display: { xs: 'none', md: 'block' } }} />

      {/* Center - live feed */}
      <Box sx={{ height: '100vh', position: 'relative' }}>
        <LiveFeed userId={userId} />
      </Box>
    </Box>
  )
}

export default LiveFeedPage
