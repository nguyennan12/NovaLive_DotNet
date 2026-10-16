import { useState } from 'react'
import { Box, Button, Skeleton, Typography, Divider } from '@mui/material'
import CalendarMonthRoundedIcon from '@mui/icons-material/CalendarMonthRounded'
import CancelRoundedIcon from '@mui/icons-material/CancelRounded'
import EditRoundedIcon from '@mui/icons-material/EditRounded'
import PlayArrowRoundedIcon from '@mui/icons-material/PlayArrowRounded'
import { useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import ConfirmDialog from '~/common/components/common/form/ConfirmDialog'
import SectionCard from '~/features/Inventory/components/shared/SectionCard'
import LiveStatusBadge from '../shared/LiveStatusBadge'
import { useCancelSession, useUpcomingSession } from '../../hooks/useLiveSessions'
import { formatDate } from '~/common/utils/formatters'

const UpcomingLiveCard = ({ onEdit }) => {
  const navigate = useNavigate()
  const [sessionToCancel, setSessionToCancel] = useState(null)

  const { data: sessions = [], isLoading } = useUpcomingSession()
  const cancelSession = useCancelSession()

  const handleStart = (session) => {
    if (!session) return
    navigate(`/shop/live/${session._id}`)
  }

  const handleConfirmCancel = async () => {
    if (!sessionToCancel) return
    await toast.promise(
      cancelSession.mutateAsync(sessionToCancel._id),
      { pending: 'Đang hủy...', success: 'Phiên live đã bị hủy', error: 'Hủy thất bại' }
    )
    setSessionToCancel(null)
  }

  if (isLoading) {
    return (
      <SectionCard title='Phiên Live Sắp Tới'>
        <Skeleton variant='rectangular' height={160} sx={{ borderRadius: 2 }} />
      </SectionCard>
    )
  }

  if (sessions.length === 0) {
    return (
      <SectionCard title='Phiên Live Sắp Tới' sx={{ height: '100%' }}>
        <Box sx={{ textAlign: 'center', py: 5 }}>
          <CalendarMonthRoundedIcon sx={{ fontSize: 52, color: '#e2e8f0', mb: 1.5 }} />
          <Typography sx={{ fontSize: '0.875rem', fontWeight: 600, color: '#94a3b8' }}>
            Chưa có phiên nào được lên lịch
          </Typography>
          <Typography sx={{ fontSize: '0.78rem', color: '#cbd5e1', mt: 0.5 }}>
            Hãy tạo phiên live đầu tiên của bạn!
          </Typography>
        </Box>
      </SectionCard>
    )
  }

  return (
    <SectionCard
      title='Phiên Live Sắp Tới'
      subtitle='Đã lên lịch'
      sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}
    >
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          gap: 3,
          overflowY: 'auto',
          maxHeight: '240px',
          pr: 1.5,
          pb: 1,
          '&::-webkit-scrollbar': {
            width: '6px'
          }
        }}
      >
        {sessions.map((session, index) => (
          <Box key={session._id} sx={{ bgcolor: 'background.paper', border: '0.5px solid', borderColor: 'divider', borderRadius: 2 }}>
            <Box sx={{ bgcolor: 'grey.50', px: 1.75, py: 1, display: 'flex', alignItems: 'center', justifyContent: 'space-between', borderBottom: '0.5px solid', borderColor: 'divider' }}>
              <Typography sx={{ fontSize: '0.68rem', fontWeight: 600, color: 'text.disabled', letterSpacing: '0.05em', textTransform: 'uppercase' }}>
                Sắp diễn ra
              </Typography>
              <LiveStatusBadge status={session.live_status} />
            </Box>

            {/* Title + description */}
            <Box sx={{ px: 1.75, pt: 1.75, pb: 1.5 }}>
              <Typography sx={{ fontWeight: 500, fontSize: '0.94rem', color: 'text.primary', lineHeight: 1.35, mb: 0.5 }}>
                {session.live_title}
              </Typography>
              {session.live_description && (
                <Typography sx={{ fontSize: '0.81rem', color: 'text.secondary', lineHeight: 1.5 }}>
                  {session.live_description}
                </Typography>
              )}
            </Box>

            {/* Date + live code */}
            <Box sx={{ px: 1.75, pb: 1.5, display: 'flex', alignItems: 'center', gap: 0.75, borderBottom: '0.5px solid', borderColor: 'divider' }}>
              <CalendarMonthRoundedIcon sx={{ fontSize: 13, color: 'text.disabled', flexShrink: 0 }} />
              <Typography sx={{ fontSize: '0.75rem', color: 'text.secondary' }}>
                {formatDate(session.live_schedule_at)}
              </Typography>
              <Typography sx={{ ml: 'auto', fontSize: '0.7rem', color: 'text.disabled', fontFamily: 'monospace' }}>
                {session.live_code}
              </Typography>
            </Box>

            {/* Actions */}
            <Box sx={{ px: 1.75, py: 1.25, display: 'flex', gap: 1 }}>
              <Button
                variant="contained" size="small" startIcon={<PlayArrowRoundedIcon />}
                onClick={() => handleStart(session)}
                disableElevation
                sx={{ flex: 1, background: 'linear-gradient(90deg, #74a2ffff, #69aedc, #8acdde)', fontWeight: 600, borderRadius: '8px', fontSize: '0.81rem', color: '#fff' }}
              >
                Bắt đầu
              </Button>
              <Button
                variant="outlined" size="small" startIcon={<EditRoundedIcon />}
                onClick={() => onEdit?.(session)}
                sx={{ borderColor: 'divider', color: 'text.secondary', fontWeight: 500, borderRadius: '8px', fontSize: '0.81rem' }}
              >
                Sửa
              </Button>
              <Button
                variant="outlined" size="small" startIcon={<CancelRoundedIcon />}
                onClick={() => setSessionToCancel(session)}
                sx={{ borderColor: 'error.light', color: 'error.main', fontWeight: 500, borderRadius: '8px', fontSize: '0.81rem' }}
              >
                Hủy
              </Button>
            </Box>
            {index !== sessions.length - 1 && (<Divider sx={{ mt: 3 }} />)}
          </Box>
        ))}
      </Box>

      <ConfirmDialog
        open={Boolean(sessionToCancel)}
        title='Hủy phiên live?'
        content={`Bạn có chắc muốn hủy phiên "${sessionToCancel?.live_title}"? Hành động này không thể hoàn tác.`}
        confirmText='Hủy phiên live'
        cancelText='Giữ lại'
        isLoading={cancelSession.isPending}
        onConfirm={handleConfirmCancel}
        onClose={() => setSessionToCancel(null)}
      />
    </SectionCard>
  )
}

export default UpcomingLiveCard