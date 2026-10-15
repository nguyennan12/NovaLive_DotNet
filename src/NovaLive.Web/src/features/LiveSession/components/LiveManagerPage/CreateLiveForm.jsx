import { Box, Button, TextField } from '@mui/material'
import { Controller, useForm } from 'react-hook-form'
import { toast } from 'react-toastify'
import SectionCard from '~/features/Inventory/components/shared/SectionCard'
import { useCreateSession, useUpdateSession } from '../../hooks/useLiveSessions'

const DEFAULT_VALUES = { title: '', scheduledAt: '', description: '' }

const CreateLiveForm = ({ editData, onCancelEdit }) => {
  const isEditing = !!editData

  const { control, handleSubmit, reset, formState: { errors } } = useForm({
    values: isEditing
      ? {
        title: editData.live_title,
        scheduledAt: editData.live_scheduled_at ? editData.live_scheduled_at.slice(0, 16) : '',
        description: editData.live_description || ''
      }
      : DEFAULT_VALUES
  })

  const createSession = useCreateSession()
  const updateSession = useUpdateSession()

  const onSubmit = async (data) => {
    const payload = { ...data, scheduledAt: new Date(data.scheduledAt).toISOString() }
    if (isEditing) {
      await toast.promise(
        updateSession.mutateAsync({ sessionId: editData._id, data: payload }),
        { pending: 'Updating...', success: 'Live session updated successfully!', error: 'Update failed' }
      )
      onCancelEdit?.()
    } else {
      await toast.promise(
        createSession.mutateAsync(payload),
        { pending: 'Scheduling...', success: 'Live session scheduled successfully!', error: 'Scheduling failed' }
      )
    }
    reset(DEFAULT_VALUES)
  }

  const handleCancel = () => {
    onCancelEdit?.()
    reset(DEFAULT_VALUES)
  }

  return (
    <SectionCard
      title={isEditing ? 'Edit Live Session' : 'Schedule New Live'}
      subtitle='Fill in the details for your livestream'
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

        <Controller
          name='scheduledAt'
          control={control}
          rules={{ required: 'Start time is required' }}
          render={({ field }) => (
            <TextField
              {...field}
              label='Start time *'
              type='datetime-local'
              fullWidth
              size='small'
              slotProps={{ inputLabel: { shrink: true } }}
              error={!!errors.scheduledAt}
              helperText={errors.scheduledAt?.message}
            />
          )}
        />

        <Controller
          name='description'
          control={control}
          render={({ field }) => (
            <TextField
              {...field}
              label='Description (optional)'
              fullWidth
              size='small'
              multiline
              rows={3}
            />
          )}
        />

        <Box sx={{ display: 'flex', gap: 1.5, justifyContent: 'flex-end', mt: 0.5 }}>
          {isEditing && (
            <Button variant='outlined' color='inherit' onClick={handleCancel} sx={{ borderRadius: '8px' }}>
              Cancel
            </Button>
          )}
          <Button
            type='submit'
            variant='contained'
            disableElevation
            disabled={createSession.isPending || updateSession.isPending}
            sx={{
              background: 'linear-gradient(90deg, #74a2ffff, #69aedc, #8acdde)',
              borderRadius: '8px',
              fontWeight: 600,
              px: 3,
              color: '#fff'
            }}
          >
            {isEditing ? 'Save Changes' : 'Schedule Live'}
          </Button>
        </Box>
      </Box>
    </SectionCard>
  )
}

export default CreateLiveForm