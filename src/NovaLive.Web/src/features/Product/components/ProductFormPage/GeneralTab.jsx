import FormatBoldIcon from '@mui/icons-material/FormatBold'
import FormatItalicIcon from '@mui/icons-material/FormatItalic'
import FormatListBulletedIcon from '@mui/icons-material/FormatListBulleted'
import ImageIcon from '@mui/icons-material/Image'
import LinkIcon from '@mui/icons-material/Link'
import { Box, Divider, Paper, TextField, Typography } from '@mui/material'
import { Controller, useFormContext } from 'react-hook-form'
import ProductVariantsCard from './ProductVariantsCard'
import ProductDetailsCard from './ProductDetailsCard'

export default function GeneralTab({ isUpdate }) {
  const { control } = useFormContext()
  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
      <Paper variant="outlined" sx={{ p: 3, borderRadius: 3, border: '1px solid secondary.contrastText' }}>
        <Typography variant="h6" fontWeight={700} mb={2.5}>General</Typography>

        <Box mb={2.5}>
          <Typography variant="caption" color="text.secondary" fontWeight={600} display="block" mb={0.75}>
            Product Name <span style={{ color: '#ef4444' }}>*</span>
          </Typography>

          <Controller
            name="spu_name"
            control={control}
            rules={{ required: 'Product name is required!' }}
            render={({ field, fieldState: { error } }) => (
              <>
                <TextField
                  {...field}
                  value={field.value ?? ''}
                  fullWidth
                  size="small"
                  placeholder="Enter product name"
                  error={!!error}
                  helperText={error?.message || 'A product name is required and recommended to be unique.'}
                />
              </>
            )}
          />
        </Box>

        <Box sx={{ display: 'flex', gap: 2 }}>

          {/* Cột Price */}
          <Box sx={{ flex: 1 }}>
            <Typography variant="caption" color="text.secondary" fontWeight={600} display="block" mb={0.75}>
              Price <span style={{ color: '#ef4444' }}>*</span>
            </Typography>
            <Controller
              name="spu_price"
              control={control}
              rules={{
                required: 'Price is required!',
                min: { value: 0, message: 'Price cannot be negative' }
              }}
              render={({ field, fieldState: { error } }) => (
                <TextField
                  {...field}
                  value={field.value ?? ''}
                  fullWidth
                  size="small"
                  type="number"
                  placeholder="0.00"
                  error={!!error}
                  helperText={error?.message}
                  onChange={(e) => {
                    const value = e.target.value
                    field.onChange(value === '' ? undefined : Number(value))
                  }}
                />
              )}
            />
          </Box>

          {/* Cột quantity */}
          <Box sx={{ flex: 1 }}>
            <Typography variant="caption" color="text.secondary" fontWeight={600} display="block" mb={0.75}>
              Quantity <span style={{ color: '#ef4444' }}>*</span>
            </Typography>
            <Controller
              name="spu_quantity"
              control={control}
              rules={{
                required: 'Quantity is required!',
                min: { value: 0, message: 'Stock cannot be negative' }
              }}
              render={({ field, fieldState: { error } }) => (
                <TextField
                  {...field}
                  value={field.value ?? ''}
                  fullWidth
                  size="small"
                  type="number"
                  placeholder="0"
                  error={!!error}
                  helperText={error?.message}
                  onChange={(e) => {
                    const value = e.target.value
                    field.onChange(value === '' ? undefined : Number(value))
                  }}
                />
              )}
            />
          </Box>

        </Box>

        <Box>
          <Typography variant="caption" color="text.secondary" fontWeight={600} display="block" mb={0.75}>
            Description
          </Typography>
          <Paper variant="outlined" sx={{ borderRadius: 2.5, overflow: 'hidden', border: '1px solid secondary.contrastText' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, px: 1.5, py: 1, borderBottom: '1px solid primary.main', background: 'secondary.contrastText' }}>
              {[
                { icon: <FormatBoldIcon fontSize="small" />, label: 'Bold' },
                { icon: <FormatItalicIcon fontSize="small" />, label: 'Italic' },
                { icon: <FormatListBulletedIcon fontSize="small" />, label: 'List' },
                { icon: <LinkIcon fontSize="small" />, label: 'Link' },
                { icon: <ImageIcon fontSize="small" />, label: 'Image' }
              ].map(({ icon, label }) => (
                <Box
                  key={label}
                  title={label}
                  sx={{
                    p: 0.75, borderRadius: 1.5, cursor: 'pointer', color: '#666',
                    '&:hover': { background: '#eef2ff', color: 'secondary.main' },
                    transition: 'all 0.15s',
                    display: 'flex', alignItems: 'center'
                  }}
                >
                  {icon}
                </Box>
              ))}
              <Divider orientation="vertical" flexItem sx={{ mx: 0.5 }} />
              <Typography variant="caption" color="text.secondary" sx={{ px: 1 }}>Normal</Typography>
            </Box>

            <Controller
              name="spu_description"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  fullWidth
                  multiline
                  rows={4}
                  placeholder="Describe your product in detail..."
                  sx={{
                    '& .MuiOutlinedInput-root': {
                      borderRadius: 0,
                      '& fieldset': { border: 'none' }
                    }
                  }}
                />
              )}
            />
          </Paper>
          <Typography variant="caption" color="text.secondary" mt={0.5} display="block">
            Set a description for better visibility.
          </Typography>
        </Box>
      </Paper>

      <ProductVariantsCard />

      {isUpdate ? <Box></Box> : <ProductDetailsCard />}

    </Box>
  )
}