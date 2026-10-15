import { Box, Grid, Skeleton } from '@mui/material'
import { glassSx } from '~/theme'


export const PageSkeleton = () => (
  <Grid container spacing={2.5}>
    <Grid size={{ xs: 12, md: 5 }}>
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <Box sx={glassSx}>
          <Skeleton variant="rectangular" height={380} sx={{ borderRadius: '12px' }} />
          <Box sx={{ display: 'flex', gap: 1, mt: 1.5 }}>
            {[0, 1, 2, 3].map((i) => (
              <Skeleton key={i} variant="rounded" width={64} height={64} sx={{ borderRadius: '10px' }} />
            ))}
          </Box>
        </Box>
        <Box sx={glassSx}><Skeleton variant="rounded" height={160} /></Box>
      </Box>
    </Grid>
    <Grid size={{ xs: 12, md: 7 }}>
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <Box sx={glassSx}>
          <Skeleton variant="text" width="75%" height={40} />
          <Skeleton variant="text" width="40%" height={22} sx={{ mt: 0.75 }} />
          <Skeleton variant="rounded" height={60} sx={{ mt: 2, borderRadius: 2 }} />
        </Box>
        <Box sx={glassSx}>
          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2,1fr)', gap: 1 }}>
            {[0, 1, 2, 3].map((i) => <Skeleton key={i} variant="rounded" height={52} sx={{ borderRadius: '10px' }} />)}
          </Box>
        </Box>
        <Box sx={glassSx}><Skeleton variant="rounded" height={44} sx={{ borderRadius: '12px' }} /></Box>
      </Box>
    </Grid>
    <Grid size={12}><Skeleton variant="rounded" height={100} sx={{ borderRadius: 3 }} /></Grid>
    <Grid size={12}><Skeleton variant="rounded" height={120} sx={{ borderRadius: 3 }} /></Grid>
    <Grid size={12}><Skeleton variant="rounded" height={200} sx={{ borderRadius: 3 }} /></Grid>
  </Grid>
)