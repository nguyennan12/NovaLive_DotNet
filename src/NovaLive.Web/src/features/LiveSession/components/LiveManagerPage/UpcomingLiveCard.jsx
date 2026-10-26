import { useState } from 'react'
import { Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, Skeleton, TextField, Typography, Divider } from '@mui/material'
import CalendarMonthRoundedIcon from '@mui/icons-material/CalendarMonthRounded'
import CancelRoundedIcon from '@mui/icons-material/CancelRounded'
import EditRoundedIcon from '@mui/icons-material/EditRounded'
import PlayArrowRoundedIcon from '@mui/icons-material/PlayArrowRounded'
import { useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import ConfirmDialog from '~/common/components/common/form/ConfirmDialog'
import SectionCard from '~/features/Inventory/components/shared/SectionCard'
import LiveStatusBadge from '../shared/LiveStatusBadge'
import { useCancelSession, useStartScheduledSession, useUpcomingSession, useUpdateSession } from '../../hooks/useLiveSessions'
import { formatDate } from '~/common/utils/formatters'

const toLocalDateTime = (value) => {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return ''
  return new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().slice(0, 16)
}

const UpcomingLiveCard = () => {
  const navigate = useNavigate()
  const [sessionToCancel, setSessionToCancel] = useState(null)
  const [sessionToEdit, setSessionToEdit] = useState(null)
  const [editTitle, setEditTitle] = useState('')
  const [editScheduledAt, setEditScheduledAt] = useState('')

  const { data: sessions = [], isLoading } = useUpcomingSession()
  const cancelSession = useCancelSession()
  const updateSession = useUpdateSession()
  const startSession = useStartScheduledSession()

  const handleStart = async (session) => {
    if (!session) return
    const started = await toast.promise(
      startSession.mutateAsync(session._id),
      { pending: 'Đang bắt đầu phiên live...', success: 'Phiên live đã bắt đầu.', error: 'Không thể bắt đầu phiên live.' }
    )
    if (started?.id || started?._id) navigate(`/shop/live/${started.id || started._id}`)
  }

  const openEdit = (session) => {
    setSessionToEdit(session)
    setEditTitle(session.live_title || '')
    setEditScheduledAt(toLocalDateTime(session.live_schedule_at))
  }

  const handleSaveEdit = async () => {
    if (!sessionToEdit) return
    const scheduledAt = new Date(editScheduledAt)
    if (editTitle.trim().length < 3 || Number.isNaN(scheduledAt.getTime()) || scheduledAt <= new Date()) {
      toast.error('Nhập tiêu đề và thời gian hợp lệ trong tương lai.')
      return
    }
    await toast.promise(
      updateSession.mutateAsync({
        sessionId: sessionToEdit._id,
        data: { title: editTitle.trim(), scheduledAt: scheduledAt.toISOString() }
      }),
      { pending: 'Đang cập nhật...', success: 'Đã cập nhật lịch live.', error: 'Cập nhật thất bại.' }
    )
    setSessionToEdit(null)
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
                onClick={() => openEdit(session)}
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

      <Dialog open={Boolean(sessionToEdit)} onClose={() => setSessionToEdit(null)} fullWidth maxWidth='sm'>
        <DialogTitle>Sửa lịch livestream</DialogTitle>
        <DialogContent sx={{ display: 'grid', gap: 2, pt: '12px !important' }}>
          <TextField
            label='Tên phiên live'
            value={editTitle}
            onChange={(event) => setEditTitle(event.target.value)}
            inputProps={{ maxLength: 150 }}
            fullWidth
          />
          <TextField
            label='Thời gian bắt đầu'
            type='datetime-local'
            value={editScheduledAt}
            onChange={(event) => setEditScheduledAt(event.target.value)}
            inputProps={{ min: toLocalDateTime(new Date()) }}
            InputLabelProps={{ shrink: true }}
            fullWidth
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setSessionToEdit(null)}>Đóng</Button>
          <Button variant='contained' onClick={handleSaveEdit} disabled={updateSession.isPending}>Lưu thay đổi</Button>
        </DialogActions>
      </Dialog>
    </SectionCard>
  )
}

export default UpcomingLiveCard
