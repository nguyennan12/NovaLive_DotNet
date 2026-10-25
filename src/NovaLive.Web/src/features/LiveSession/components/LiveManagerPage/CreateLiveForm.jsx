import { Box, Button, TextField } from '@mui/material'
import { Controller, useForm } from 'react-hook-form'
import { toast } from 'react-toastify'
import { useNavigate } from 'react-router-dom'
import SectionCard from '~/features/Inventory/components/shared/SectionCard'
import { useCreateSession } from '../../hooks/useLiveSessions'

const DEFAULT_VALUES = { title: '' }

const CreateLiveForm = () => {
  const { control, handleSubmit, reset, formState: { errors } } = useForm({ defaultValues: DEFAULT_VALUES })

  const createSession = useCreateSession()
  const navigate = useNavigate()

  const onSubmit = async (data) => {
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
      title='Start New Live'
      subtitle='Tạo phiên live ngay bây giờ rồi mở Live Studio để bật camera và microphone.'
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
              label='Live session title *'
              fullWidth
              size='small'
              error={!!errors.title}
              helperText={errors.title?.message}
            />
          )}
        />

        <Box sx={{ display: 'flex', gap: 1.5, justifyContent: 'flex-end', mt: 0.5 }}>
          <Button
            type='submit'
            variant='contained'
            disableElevation
            disabled={createSession.isPending}
            sx={{
              background: 'linear-gradient(90deg, #74a2ffff, #69aedc, #8acdde)',
              borderRadius: '8px',
              fontWeight: 600,
              px: 3,
              color: '#fff'
            }}
          >
            Tạo phòng & mở Live Studio
          </Button>
        </Box>
      </Box>
    </SectionCard>
  )
}

export default CreateLiveForm
