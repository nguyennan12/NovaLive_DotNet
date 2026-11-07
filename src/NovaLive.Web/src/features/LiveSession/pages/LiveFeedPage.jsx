import { useMemo, useState } from 'react'
import { useSelector } from 'react-redux'
import { Box } from '@mui/material'
import { selectCurrentUser } from '~/store/user/userSlice'
import { LiveFeed } from '../components/LiveFeedPage/LiveFeed'
import { LiveSidebar } from '../components/LiveFeedPage/LiveSidebar'
import { LiveUpcomingScheduleCard } from '../components/LiveFeedPage/LiveUpcomingScheduleCard'
import { useLiveFeed } from '../hooks/useLiveFeed'

const LiveFeedPage = () => {
  const currentUser = useSelector(selectCurrentUser)
  const userId = useMemo(() => {
    if (currentUser?.id || currentUser?._id) {
      return String(currentUser.id || currentUser._id)
    }
    return `guest-${Math.random().toString(36).substring(2, 9)}`
  }, [currentUser])

  const {
    lives,
    loading,
    currentIndex,
    goNext,
    goPrev,
    selectLiveById,
    loadingMore,
    handleTouchEnd,
    handleTouchStart,
    handleWheel
  } = useLiveFeed()

  const currentLive = lives[currentIndex] || null
  const [activeTab, setActiveTab] = useState('live') // 'live' | 'schedule'

  return (
    <Box
      sx={{
        width: '100vw',
        height: '100vh',
        bgcolor: '#f0f5fc',
        p: 2,
        boxSizing: 'border-box',
        display: 'flex',
        alignItems: 'center',
        gap: 2.5,
        overflow: 'hidden'
      }}
    >
      {/* 1. Left Sidebar: Sát bên trái */}
      <Box
        sx={{
          display: { xs: 'none', lg: 'flex' },
          width: 260,
          height: '100%',
          flexShrink: 0
        }}
      >
        <LiveSidebar
          onSelectLive={(liveId) => {
            selectLiveById(liveId)
            setActiveTab('live')
          }}
        />
      </Box>

      {/* 2. Center Column: Chiếm phần còn lại và căn giữa LiveFeed */}
      <Box
        sx={{
          flex: 1,
          height: '100%',
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          minWidth: 0
        }}
      >
        <Box
          sx={{
            width: { xs: '100%', sm: 420, md: 440 },
            height: '100%',
            position: 'relative',
            flexShrink: 0
          }}
        >
          <LiveFeed
            userId={userId}
            lives={lives}
            loading={loading}
            currentIndex={currentIndex}
            goNext={goNext}
            goPrev={goPrev}
            loadingMore={loadingMore}
            handleTouchStart={handleTouchStart}
            handleTouchEnd={handleTouchEnd}
            handleWheel={handleWheel}
          />
        </Box>
      </Box>

      {/* 3. Right Schedule Card: Tự co ngắn lại khi chưa có live (không fullheight) */}
      <Box
        sx={{
          display: { xs: 'none', md: 'flex' },
          width: 340,
          alignSelf: 'flex-start',
          flexShrink: 0
        }}
      >
        <LiveUpcomingScheduleCard
          activeLiveCount={lives.length}
          currentLive={currentLive}
          activeTab={activeTab}
          onTabChange={setActiveTab}
        />
      </Box>
    </Box>
  )
}

export default LiveFeedPage
