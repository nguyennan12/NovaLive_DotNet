import FiberManualRecordIcon from '@mui/icons-material/FiberManualRecord'
import { Box, FormControl, MenuItem, Paper, Select, Typography } from '@mui/material'
import { useFormContext, Controller } from 'react-hook-form'

const StatusCard = () => {
  // Lấy control và watch từ context của form cha
  const { control, watch } = useFormContext()
  const currentStatus = watch('status')

  const statusColor = {
    published: '#22c55e',
    draft: '#f59e0b',
    archived: '#ef4444'
  }

  return (
    <Paper variant="outlined" sx={{ p: 2.5, borderRadius: 3, border: '1px solid secondary.contrastText', mt: 2 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
        <Typography variant="subtitle1" fontWeight={700}>Status</Typography>
        <FiberManualRecordIcon
          sx={{ color: statusColor[currentStatus] || '#22c55e', fontSize: 14 }}
        />
      </Box>
      <Typography variant="caption" color="primary.contractText" display="block" mb={1}>
        Set Status
      </Typography>

      <FormControl fullWidth size="small">
        <Controller
          name="status"
          control={control}
          defaultValue="published"
          render={({ field }) => (
            <Select
              {...field}
              sx={{
                borderRadius: 2.5,
                background: 'primary.contrastText',
                '& .MuiInputBase-input': { color: 'gray' },
                '& .MuiOutlinedInput-notchedOutline': { borderColor: 'secondary.main' },
                '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'secondary.main' },
                '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: 'secondary.main' }
              }}
            >
              <MenuItem value="published" >Published</MenuItem>
              <MenuItem value="draft">Draft</MenuItem>
              <MenuItem value="archived">Archived</MenuItem>
            </Select>
          )}
        />
      </FormControl>
    </Paper>
  )
}

export default StatusCard