/* eslint-disable react-hooks/incompatible-library */
import RemoveCircleOutlineRoundedIcon from '@mui/icons-material/RemoveCircleOutlineRounded'
import { Autocomplete, Box, Button, Chip, FormControl, FormHelperText, InputLabel, MenuItem, Select, TextField, Typography } from '@mui/material'
import { Controller, FormProvider, useForm } from 'react-hook-form'
import { toast } from 'react-toastify'
import { addInventoryAPI } from '~/common/apis/services/inventoryService'
import SectionCard from '../shared/SectionCard'
import { useQueryClient } from '@tanstack/react-query'

const fieldSx = {
  '& .MuiOutlinedInput-root': {
    fontSize: '0.85rem',
    borderRadius: '8px',
    bgcolor: '#fafafa',
    '& fieldset': { borderColor: '#eeeeee' },
    '&:hover fieldset': { borderColor: 'third.main' },
    '&.Mui-focused fieldset': { borderColor: 'third.main', borderWidth: '1.5px' }
  },
  '& label': { fontSize: '0.8rem' },
  '& label.Mui-focused': { color: 'third.main' }
}

const REASON_META = {
  order: { label: 'Order Fulfillment', color: '#3485f7' },
  damaged: { label: 'Damaged / Write-off', color: '#ef4444' },
  transfer: { label: 'Warehouse Transfer', color: '#f59e0b' },
  adjustment: { label: 'Stock Adjustment', color: '#9ca3af' }
}

const OUT_REASONS = ['order', 'damaged', 'transfer', 'adjustment']

const StockOutForm = ({ skus, setParams }) => {
  const methods = useForm({
    mode: 'onBlur',
    defaultValues: {
      product: null,
      stock: '',
      reason: '',
      note: '',
      type: 'OUT'
    }
  })
  const queryClient = useQueryClient()
  const { handleSubmit, control, watch, reset } = methods
  const selectedProduct = watch('product')

  const onSubmit = async (data) => {
    const { product, ...rest } = data
    const payload = {
      productId: product.sku_spuId,
      skuId: product._id,
      shopId: '69e364dfdf24f31846f15580',
      ...rest
    }
    toast.promise(addInventoryAPI(payload), {
      pending: 'Processing...',
      success: 'Stock removed successfully!',
      error: 'Failed to remove stock'
    })
    await Promise.all([
      queryClient.refetchQueries({ queryKey: ['skus'], type: 'active' }),
      queryClient.refetchQueries({ queryKey: ['chart_inventory'], type: 'active' }),
      queryClient.refetchQueries({ queryKey: ['history_inventory'], type: 'active' })
    ])
    reset()
  }

  const onError = () => {
    toast.error('Please check the required fields!')
  }

  return (
    <FormProvider {...methods}>
      <SectionCard
        title='Stock Out'
        subtitle='Record outgoing inventory'
        icon={<RemoveCircleOutlineRoundedIcon sx={{ color: 'third.main' }} />}
      >
        <Box
          component="form"
          onSubmit={handleSubmit(onSubmit, onError)}
          sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}
        >
          {/* Product select */}
          <Controller
            name="product"
            control={control}
            rules={{ required: 'Please select a product' }}
            render={({ field: { onChange, value, ...field }, fieldState: { error } }) => (
              <Autocomplete
                {...field}
                options={skus}
                getOptionLabel={(o) => `${o.spu_name} — ${o.sku_id}`}
                onInputChange={(_, value, reason) => {
                  if (reason === 'input' || reason === 'clear') {
                    setParams(prev => ({ ...prev, keyword: value, page: 1 }))
                  }
                }}
                value={value ?? null}
                onChange={(_, data) => onChange(data)}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label='Product *'
                    size='small'
                    error={!!error}
                    helperText={error ? error.message : ''}
                    sx={fieldSx}
                  />
                )}
                renderOption={(props, option) => {
                  const { key, ...rest } = props
                  return (
                    <Box component="li" key={key} {...rest} sx={{ py: 1 }}>
                      <Box sx={{ flex: 1 }}>
                        <Typography sx={{ fontSize: '0.82rem', fontWeight: 600 }}>
                          {option.spu_name}
                        </Typography>
                        <Typography sx={{ fontSize: '0.7rem', color: '#9ca3af' }}>
                          {option.sku_id} · Available: {option.sku_stock}
                        </Typography>
                      </Box>
                      {option.sku_stock <= 0 && (
                        <Chip label='Out' size='small' sx={{ fontSize: '0.6rem', bgcolor: '#fef2f2', color: '#ef4444', height: 18 }} />
                      )}
                    </Box>
                  )
                }}
              />
            )}
          />

          {/* Stock indicator */}
          {selectedProduct && (
            <Box
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1,
                px: 1.5,
                py: 1,
                bgcolor: selectedProduct.sku_stock <= 10 ? '#fffbeb' : '#f0fdf4',
                borderRadius: '8px',
                border: '1px solid',
                borderColor: selectedProduct.sku_stock <= 10 ? '#fde68a' : '#bbf7d0'
              }}
            >
              <Box
                sx={{
                  width: 7,
                  height: 7,
                  borderRadius: '50%',
                  bgcolor: selectedProduct.sku_stock <= 10 ? '#f59e0b' : '#22c55e'
                }}
              />
              <Typography sx={{ fontSize: '0.75rem', color: '#4b5563', fontWeight: 500 }}>
                Current stock: <strong>{selectedProduct.sku_stock} units</strong>
              </Typography>
            </Box>
          )}

          {/* Quantity */}
          <Controller
            name='stock'
            control={control}
            rules={{
              required: 'Quantity is required!',
              min: { value: 1, message: 'Minimum 1 unit' },
              validate: (val) => !selectedProduct || val <= selectedProduct.sku_stock || `Exceeds available stock (${selectedProduct.sku_stock})`
            }}
            render={({ field, fieldState: { error } }) => (
              <TextField
                {...field}
                fullWidth
                label='Quantity *'
                size="small"
                type="number"
                placeholder="0"
                error={!!error}
                helperText={error?.message}
                onChange={(e) => {
                  const value = e.target.value
                  field.onChange(value === '' ? undefined : Number(value))
                }}
                sx={fieldSx}
              />
            )}
          />

          {/* Reason */}
          <Controller
            name='reason'
            control={control}
            rules={{ required: 'Reason is required' }}
            render={({ field, fieldState: { error } }) => (
              <FormControl size="small" error={!!error}>
                <InputLabel sx={{ fontSize: '0.8rem', '&.Mui-focused': { color: 'third.main' } }}>
                  Reason *
                </InputLabel>
                <Select
                  {...field}
                  label="Reason *"
                  sx={{
                    fontSize: '0.85rem',
                    borderRadius: '8px',
                    bgcolor: '#fafafa',
                    '& fieldset': { borderColor: '#eeeeee' },
                    '&:hover fieldset': { borderColor: 'third.main' },
                    '&.Mui-focused fieldset': { borderColor: 'third.main', borderWidth: '1.5px' }
                  }}
                >
                  {OUT_REASONS.map((r) => (
                    <MenuItem key={r} value={r} sx={{ fontSize: '0.82rem' }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <Box sx={{ width: 7, height: 7, borderRadius: '50%', bgcolor: REASON_META[r].color }} />
                        {REASON_META[r].label}
                      </Box>
                    </MenuItem>
                  ))}
                </Select>
                {error && <FormHelperText>{error.message}</FormHelperText>}
              </FormControl>
            )}
          />

          {/* Note */}
          <Controller
            name='note'
            control={control}
            render={({ field }) => (
              <TextField
                label='Note'
                size='small'
                multiline
                rows={2}
                {...field}
                placeholder='e.g. Order #8821, transfer to warehouse B...'
                sx={fieldSx}
              />
            )}
          />

          <Button
            type="submit"
            variant='contained'
            startIcon={<RemoveCircleOutlineRoundedIcon />}
            sx={{
              bgcolor: 'third.main',
              color: '#fff',
              fontWeight: 700,
              fontSize: '0.85rem',
              borderRadius: '8px',
              py: 1,
              boxShadow: '0 2px 8px rgba(124,58,237,0.3)',
              '&:hover': { bgcolor: 'third.main', boxShadow: '0 4px 12px rgba(124,58,237,0.4)' }
            }}
          >
            Export Stock
          </Button>
        </Box>
      </SectionCard>
    </FormProvider>
  )
}

export default StockOutForm
