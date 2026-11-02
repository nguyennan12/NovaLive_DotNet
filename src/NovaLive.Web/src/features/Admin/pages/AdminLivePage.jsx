import { useState, useEffect } from 'react'
import {
  Avatar,
  Box,
  Button,
  Card,
  CardMedia,
  CardContent,
  Chip,
  Grid,
  Stack,
  Typography
} from '@mui/material'
import LiveTvRoundedIcon from '@mui/icons-material/LiveTvRounded'
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import FiberManualRecordIcon from '@mui/icons-material/FiberManualRecord'
import { Link } from 'react-router-dom'
import { getAdminActiveLivestreamsAPI } from '~/common/apis/services/adminService'
import { PageHeader } from '~/features/Seller/components/shared/PageHeader'
import { SellerCard } from '~/features/Seller/components/shared/SellerCard'
import { QueryState } from '~/features/Seller/components/shared/QueryState'

export const AdminLivePage = () => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [streams, setStreams] = useState([])

  const loadStreams = async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await getAdminActiveLivestreamsAPI({ page: 1, size: 30 })
      const list = res?.items || (Array.isArray(res) ? res : res?.data || [])
      setStreams(list)
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadStreams()
  }, [])

  return (
    <>
      <PageHeader
        title="Giám sát Livestream Realtime"
        description="Theo dõi các luồng phát trực tiếp đang phát sóng trên nền tảng NovaLive."
        action={
          <Button
            variant="outlined"
            size="small"
            startIcon={<RefreshRoundedIcon fontSize="small" />}
            onClick={loadStreams}
            disabled={loading}
            sx={{
              borderColor: 'divider',
              color: 'text.secondary',
              textTransform: 'none',
              borderRadius: 2,
              '&:hover': { bgcolor: 'rgba(0,0,0,0.04)' }
            }}
          >
            Làm mới
          </Button>
        }
      />

      <QueryState
        loading={loading}
        error={error}
        empty={!loading && streams.length === 0}
        onRetry={loadStreams}
        emptyText="Hiện tại không có phiên livestream nào đang phát sóng."
      >
        <Grid container spacing={2.5}>
          {streams.map((stream) => (
            <Grid key={stream.id} size={{ xs: 12, sm: 6, md: 4 }}>
              <SellerCard>
                <Box sx={{ position: 'relative', borderRadius: 2, overflow: 'hidden', mb: 1.5 }}>
                  {stream.thumbnailUrl ? (
                    <Box
                      component="img"
                      src={stream.thumbnailUrl}
                      alt={stream.title}
                      sx={{ width: '100%', height: 180, objectFit: 'cover' }}
                    />
                  ) : (
                    <Box
                      sx={{
                        width: '100%',
                        height: 180,
                        bgcolor: '#0f172a',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        color: '#64748b'
                      }}
                    >
                      <LiveTvRoundedIcon sx={{ fontSize: 48 }} />
                    </Box>
                  )}
                  <Chip
                    icon={<FiberManualRecordIcon sx={{ fontSize: '10px !important', color: '#fff !important' }} />}
                    label="LIVE"
                    size="small"
                    color="error"
                    sx={{
                      position: 'absolute',
                      top: 10,
                      left: 10,
                      fontWeight: 800,
                      fontSize: '0.7rem'
                    }}
                  />
                  <Box
                    sx={{
                      position: 'absolute',
                      bottom: 8,
                      right: 8,
                      bgcolor: 'rgba(0,0,0,0.65)',
                      color: '#ffffff',
                      px: 1,
                      py: 0.3,
                      borderRadius: 1,
                      display: 'flex',
                      alignItems: 'center',
                      gap: 0.5,
                      fontSize: '0.75rem',
                      fontWeight: 600
                    }}
                  >
                    <VisibilityRoundedIcon sx={{ fontSize: 14 }} />
                    {Number(stream.viewerCount || stream.viewers || 0).toLocaleString('vi-VN')}
                  </Box>
                </Box>

                <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 0.5 }} noWrap>
                  {stream.title}
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1.5 }} noWrap>
                  Gian hàng: {stream.shopName || stream.shop || 'Cửa hàng'}
                </Typography>

                <Button
                  component={Link}
                  to={`/live?session=${stream.id}`}
                  fullWidth
                  variant="outlined"
                  color="secondary"
                  size="small"
                  sx={{ textTransform: 'none', borderRadius: 2 }}
                >
                  Xem luồng phát
                </Button>
              </SellerCard>
            </Grid>
          ))}
        </Grid>
      </QueryState>
    </>
  )
}

export default AdminLivePage
