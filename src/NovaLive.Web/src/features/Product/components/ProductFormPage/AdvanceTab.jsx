import { Box, Paper, Switch, Typography } from '@mui/material'
import { Controller, useFormContext } from 'react-hook-form'

export default function AdvanceTab() {
  const { control } = useFormContext()

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, mt: 2 }}>

      {/* Options */}
      <Paper variant="outlined" sx={{ p: 3, borderRadius: 3, border: '1px solid secondary.contrastText' }}>
        <Typography variant="h6" fontWeight={700} mb={2}>Product Options</Typography>

        {[
          { key: 'taxable', label: 'Taxable', desc: 'Apply tax to this product' },
          { key: 'freeShipping', label: 'Free Shipping', desc: 'Offer free shipping' },
          { key: 'returnAllowed', label: 'Returns Allowed', desc: 'Allow product returns' },
          { key: 'featured', label: 'Featured Product', desc: 'Show in featured section' }
        ].map(({ key, label, desc }) => (
          <Box
            key={key}
            sx={{
              display: 'flex', justifyContent: 'space-between', alignItems: 'center',
              py: 1.5, borderBottom: '1px solid secondary.contrastText', '&:last-child': { borderBottom: 'none' }
            }}
          >
            <Box>
              <Typography variant="body2" fontWeight={600}>{label}</Typography>
              <Typography variant="caption" color="text.secondary">{desc}</Typography>
            </Box>

            <Controller
              name={key}
              control={control}
              render={({ field }) => (
                <Switch
                  {...field}
                  checked={!!field.value}
                  onChange={(e) => field.onChange(e.target.checked)}
                  sx={{
                    '& .MuiSwitch-switchBase.Mui-checked': { color: 'secondary.main' },
                    '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { backgroundColor: 'secondary.main' }
                  }}
                />
              )}
            />
          </Box>
        ))}
      </Paper>
    </Box>
  )
}