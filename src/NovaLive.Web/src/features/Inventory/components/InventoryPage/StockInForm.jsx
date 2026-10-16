import AddRoundedIcon from '@mui/icons-material/AddRounded'
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined'
import { Autocomplete, Box, Button, FormControl, FormHelperText, InputLabel, MenuItem, Select, TextField, Typography } from '@mui/material'
import { Controller, FormProvider, useForm } from 'react-hook-form'
import { toast } from 'react-toastify'
import { addInventoryAPI } from '~/common/apis/services/inventoryService'

const WAREHOUSES = ['Kho Hồ Chí Minh', 'Kho Hà Nội', 'Kho Đà Nẵng', 'Kho Cần Thơ']
import SectionCard from '../shared/SectionCard'
import { useQueryClient } from '@tanstack/react-query'

const fieldSx = {
  '& .MuiOutlinedInput-root': {
    fontSize: '0.85rem',
    borderRadius: '8px',
    bgcolor: '#fafafa',
    '& fieldset': { borderColor: '#eeeeee' },
    '&:hover fieldset': { borderColor: 'secondary.main' },
    '&.Mui-focused fieldset': { borderColor: 'secondary.main', borderWidth: '1.5px' }
  },
  '& label': { fontSize: '0.8rem' },
  '& label.Mui-focused': { color: 'secondary.main' }
}


const StockInForm = ({ skus, setParams }) => {
  const queryClient = useQueryClient()
  const methods = useForm({
    mode: 'onBlur',
    defaultValues: {
      product: null,
      stock: '',
      location: '',
      note: '',
      type: 'IN'
    }
  })
  const { handleSubmit, reset, control } = methods
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
      success: 'Stock adding successfully!',
      error: 'Failed to add stock'
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
        title='Stock In'
        subtitle='Record incoming inventory'
        icon={<Inventory2OutlinedIcon />}
      >
        <Box
          component="form"
          onSubmit={handleSubmit(onSubmit, onError)}
          sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>

          {/* Product select */}
          <Controller
            name="product"
            control={control}
            rules={{ required: 'Vui lòng chọn sản phẩm' }}
            render={({ field: { onChange, value, ...field }, fieldState: { error } }) => {
              return <Autocomplete
                {...field}
                options={skus}
                getOptionLabel={(o) => {
                  return `${o.spu_name} — ${o.sku_id}`
                }}
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
                      <Box>
                        <Typography sx={{ fontSize: '0.82rem', fontWeight: 600 }}>
                          {option.spu_name}
                        </Typography>
                        <Typography sx={{ fontSize: '0.7rem', color: '#9ca3af' }}>
                          {option.sku_id}
                        </Typography>
                      </Box>
                    </Box>
                  )
                }}
              />
            }}
          />

          {/* Quantity */}
          <Controller
            name='stock'
            control={control}
            rules={{
              required: 'Quantity is required!',
              min: { value: 0, message: 'Stock cannot be negative' }
            }}
            render={({ field, fieldState: { error } }) => {
              return (<TextField
                {...field}
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
              )
            }}
          />


          {/* Warehouse */}
          <Controller
            name='location'
            control={control}
            rules={{ required: 'Warehouse is required' }}
            render={({ field, fieldState: { error } }) => (
              <FormControl size="small" error={!!error}>
                <InputLabel
                  sx={{
                    fontSize: '0.8rem',
                    '&.Mui-focused': { color: 'secondary.main' }
                  }}
                >
                  Warehouse Location *
                </InputLabel>
                <Select
                  {...field}
                  label="Warehouse Location *"
                  sx={{
                    fontSize: '0.85rem',
                    borderRadius: '8px',
                    bgcolor: '#fafafa',
                    '& fieldset': { borderColor: '#eeeeee' },
                    '&:hover fieldset': { borderColor: 'secondary.main' },
                    '&.Mui-focused fieldset': {
                      borderColor: 'secondary.main',
                      borderWidth: '1.5px'
                    }
                  }}
                >
                  {WAREHOUSES.map((w) => (
                    <MenuItem key={w} value={w} sx={{ fontSize: '0.82rem' }}>
                      {w}
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
            render={({ field }) => {
              return (
                <TextField
                  label='Note'
                  size='small'
                  multiline
                  rows={2}
                  {...field}
                  placeholder='e.g. Received from supplier, batch #...'
                  sx={fieldSx}
                />
              )
            }}
          />

          <Button
            type="submit"
            variant='contained'
            startIcon={<AddRoundedIcon />}
            onClick={handleSubmit}
            sx={{
              bgcolor: 'secondary.main',
              color: '#fff',
              fontWeight: 700,
              fontSize: '0.85rem',
              borderRadius: '8px',
              py: 1,
              boxShadow: '0 2px 8px rgba(52,133,247,0.35)',
              '&:hover': { bgcolor: '#2471e0', boxShadow: '0 4px 12px rgba(52,133,247,0.4)' }
            }}
          >
            Add Stock
          </Button>
        </Box>
      </SectionCard>
    </FormProvider>
  )
}

export default StockInForm