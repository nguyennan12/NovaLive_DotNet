import { Box, Button, TextField } from '@mui/material'
import { Controller, useForm } from 'react-hook-form'
import { toast } from 'react-toastify'
import { useNavigate } from 'react-router-dom'
import SectionCard from '~/features/Inventory/components/shared/SectionCard'
import { useCreateSession, useScheduleSession } from '../../hooks/useLiveSessions'

const DEFAULT_VALUES = { title: '', scheduledAt: '' }

const toLocalDateTime = (date) => {
  const offset = date.getTimezoneOffset() * 60000
  return new Date(date.getTime() - offset).toISOString().slice(0, 16)
}

const CreateLiveForm = () => {
  const { control, handleSubmit, reset, formState: { errors } } = useForm({ defaultValues: DEFAULT_VALUES })

  const createSession = useCreateSession()
  const scheduleSession = useScheduleSession()
  const navigate = useNavigate()

  const onSubmit = async (data) => {
    if (data.scheduledAt) {
      const scheduledAt = new Date(data.scheduledAt)
      if (Number.isNaN(scheduledAt.getTime()) || scheduledAt <= new Date()) {
        toast.error('Thời gian lên lịch phải ở tương lai.')
        return
      }
      await toast.promise(
        scheduleSession.mutateAsync({ title: data.title, scheduledAt: scheduledAt.toISOString() }),
        { pending: 'Đang lên lịch live...', success: 'Đã lên lịch phiên live.', error: 'Không thể lên lịch phiên live.' }
      )
      reset(DEFAULT_VALUES)
      return
    }

    const session = await toast.promise(
      createSession.mutateAsync({ title: data.title }),
      { pending: 'Đang tạo phòng live...', success: 'Đã tạo phòng live.', error: 'Không thể tạo phòng live.' }
    )
    if (!session?.id) {
      toast.error('API tạo live không trả về mã phiên.')
      return
    }
    reset(DEFAULT_VALUES)
    navigate(`/shop/live/${session.id}`)
  }

  return (
    <SectionCard
      title='Tạo phiên livestream'
      subtitle='Bắt đầu ngay hoặc chọn thời gian để lên lịch phiên live.'
    >
      <Box
        component='form'
        onSubmit={handleSubmit(onSubmit)}
        sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}
      >
        <Controller
          name='title'
          control={control}
          rules={{ required: 'Title is required', minLength: { value: 3, message: 'Minimum 3 characters' } }}
          render={({ field }) => (
            <TextField
              {...field}
              label='Tên phiên live *'
              fullWidth
              size='small'
              error={!!errors.title}
              helperText={errors.title?.message}
            />
          )}
        />

        <Controller
          name='scheduledAt'
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              type='datetime-local'
              label='Lên lịch (để trống để bắt đầu ngay)'
              fullWidth
              size='small'
              InputLabelProps={{ shrink: true }}
              inputProps={{ min: toLocalDateTime(new Date()) }}
              helperText='Thời gian được tính theo múi giờ thiết bị.'
            />
          )}
        />

        <Box sx={{ display: 'flex', gap: 1.5, justifyContent: 'flex-end', mt: 0.5 }}>
          <Button
            type='submit'
            variant='contained'
            disableElevation
            disabled={createSession.isPending || scheduleSession.isPending}
            sx={{
              background: 'linear-gradient(90deg, #74a2ffff, #69aedc, #8acdde)',
              borderRadius: '8px',
              fontWeight: 600,
              px: 3,
              color: '#fff'
            }}
          >
            {scheduleSession.isPending ? 'Đang lên lịch…' : 'Tạo phiên live'}
          </Button>
        </Box>
      </Box>
    </SectionCard>
  )
}

export default CreateLiveForm
