import AddRoundedIcon from '@mui/icons-material/AddRounded'
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined'
import { Autocomplete, Box, Button, TextField, Typography } from '@mui/material'
import { Controller, FormProvider, useForm } from 'react-hook-form'
import { toast } from 'react-toastify'
import SectionCard from '../shared/SectionCard'
import { useStockAdjustment } from '../../hooks/useInventory'

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

const StockInForm = ({ skus }) => {
  const { adjustStock, isSubmitting } = useStockAdjustment()
  const methods = useForm({
    mode: 'onBlur',
    defaultValues: {
      product: null,
      stock: '',
      note: ''
    }
  })
  const { handleSubmit, reset, control } = methods

  const onSubmit = async (data) => {
    const { product } = data
    if (!product) {
      toast.error('Vui lòng chọn sản phẩm!')
      return
    }
    const skuId = product.skuId || product.id || product._id || product.sku_spuId
    await adjustStock({
      skuId,
      qtyChange: Number(data.stock),
      changeType: 'Import',
      note: data.note || 'Nhập kho hàng hóa'
    })
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
          sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>

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
              min: { value: 1, message: 'Enter at least 1 unit' }
            }}
            render={({ field, fieldState: { error } }) => {
              return (<TextField
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
              />
              )
            }}
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
            disabled={isSubmitting}
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
