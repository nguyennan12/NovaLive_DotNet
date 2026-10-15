import LiveTvRoundedIcon from '@mui/icons-material/LiveTvRounded'
import { Box, Grid, Typography } from '@mui/material'
import { useState } from 'react'
import CreateLiveForm from '../components/LiveManagerPage/CreateLiveForm'
import LiveHistoryTable from '../components/LiveManagerPage/LiveHistoryTable'
import LiveRevenueChart from '../components/LiveManagerPage/LiveRevenueChart'
import LiveStatsBlock from '../components/LiveManagerPage/LiveStatsBlock'
import UpcomingLiveCard from '../components/LiveManagerPage/UpcomingLiveCard'

const LiveManagerPage = () => {
  const [editData, setEditData] = useState(null)

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, pb: 4 }}>
      {/* Page header */}

      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
        <Box
          sx={{
            width: 42,
            height: 42,
            borderRadius: '10px',
            bgcolor: '#eff6ff',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flexShrink: 0
          }}
        >
          <LiveTvRoundedIcon sx={{ fontSize: 22, color: '#3485f7' }} />
        </Box>
        <Box>
          <Typography
            sx={{
              fontSize: { xs: '1.15rem', sm: '1.35rem' },
              fontWeight: 800,
              color: '#1e293b',
              letterSpacing: '-0.02em',
              lineHeight: 1.2
            }}
          >
            Live Sessions
          </Typography>
          <Typography sx={{ fontSize: '0.8rem', color: '#94a3b8', mt: 0.2 }}>
            Quản lý và theo dõi các buổi livestream của bạn
          </Typography>
        </Box>
      </Box>

      {/* Stats cards */}
      <LiveStatsBlock />
      {/*Create Live Form */}
      <Grid container spacing={2.5} alignItems='stretch'>
        <Grid size={{ xs: 12, md: 7 }}>
          <CreateLiveForm editData={editData} onCancelEdit={() => setEditData(null)} />
        </Grid>
        <Grid size={{ xs: 12, md: 5 }}>
          <UpcomingLiveCard onEdit={setEditData} />
        </Grid>
      </Grid>

      {/* Revenue chart */}
      <LiveRevenueChart />

      {/* History table */}
      <LiveHistoryTable />
    </Box >
  )
}

export default LiveManagerPage
