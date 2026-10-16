import AllInclusiveRoundedIcon from '@mui/icons-material/AllInclusiveRounded'
import AttachMoneyRoundedIcon from '@mui/icons-material/AttachMoneyRounded'
import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import PercentRoundedIcon from '@mui/icons-material/PercentRounded'
import TuneRoundedIcon from '@mui/icons-material/TuneRounded'
import { Box, Divider, FormControl, InputLabel, MenuItem, Paper, Select, TextField, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material'
import { useTheme } from '@mui/material/styles'
import { useEffect, useState } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { useSelector } from 'react-redux'
import { useInfiniteScroll } from '~/common/hooks/useScroll'
import { selectCurrentUser } from '~/store/user/userSlice'
import { toApiPayloadDiscount, toDefaultValuesDiscount } from '~/common/utils/converter'
import { useHomeProducts } from '~/features/Home/hooks/useHomeProducts'
import { SelectProductForForm } from '../shared/SelectProductForForm'


const cardSx = {
  boxShadow: 'none',
  border: '1px solid',
  borderColor: 'divider',
  borderRadius: '12px',
  p: 2,
  bgcolor: '#fff'
}

const toggleSx = (activeColor = 'secondary.main', activeBg = '#e6efff') => ({
  width: '100%', gap: 1,
  '& .MuiToggleButton-root': {
    flex: 1, py: 0.5, fontSize: '0.75rem', fontWeight: 600,
    gap: 0.5, textTransform: 'none', color: 'text.secondary',
    border: '1px solid', borderColor: 'divider',
    '&.Mui-selected': { bgcolor: activeBg, color: activeColor, borderColor: activeColor },
    '&.Mui-selected:hover': { filter: 'brightness(0.95)' }
  }
})

const DEFAULT_VALUES = {
  discount_name: '',
  discount_description: '',
  discount_code: '',
  discount_start_date: '',
  discount_end_date: '',
  discount_is_active: true,
  discount_target: 'product',
  discount_type: 'percentage',
  discount_applies_to: 'all',
  discount_scope: 'global',
  discount_shopId: null,
  discount_min_value: '',
  discount_value: '',
  discount_max_value: '',
  discount_max_uses: '',
  discount_uses_count: 0,
  discount_users_used: [],
  discount_max_uses_per_user: 1,
  discount_product_ids: []
}

export const DiscountForm = ({ onSubmit, editData }) => {
  const theme = useTheme()
  const inputSx = theme.customStyles?.discountForm?.inputSx || {}
  const dateSx = theme.customStyles?.discountForm?.dateSx || {}
  const sectionLabel = theme.customStyles?.discountForm?.sectionLabel || {}
  const user = useSelector(selectCurrentUser)

  const [productSearch, setProductSearch] = useState('')
  const { control, handleSubmit, reset, formState: { errors } } = useForm({ defaultValues: DEFAULT_VALUES })

  const watchType = useWatch({ control, name: 'discount_type' })
  const watchStart = useWatch({ control, name: 'discount_start_date' })
  const watchAppliesTo = useWatch({ control, name: 'discount_applies_to' })

  const filters = { shopId: user.user_shop, keyword: productSearch }
  const { isFiltering, filteredProducts, isLoading, fetchNextPage, hasNextPage, isFetchingNextPage } = useHomeProducts(filters)
  const lastProductRef = useInfiniteScroll({ isFiltering, isFetchingNextPage, hasNextPage, fetchNextPage })

  useEffect(() => {
    reset(editData ? { ...DEFAULT_VALUES, ...toDefaultValuesDiscount(editData) } : DEFAULT_VALUES)
  }, [editData, reset])

  const onFormSubmit = (data) => {
    const payload = {
      ...data,
      discount_shopId: user.user_shop,
      discount_product_ids: data.discount_applies_to === 'all' ? [] : data.discount_product_ids
    }
    onSubmit(toApiPayloadDiscount(payload))
    reset(DEFAULT_VALUES)
  }

  return (
    <Box
      component="form"
      id="discount-form"
      onSubmit={handleSubmit(onFormSubmit)}
      sx={{
        display: 'grid',
        gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' },
        gap: 2,
        width: '100%',
        alignItems: 'start'
      }}
    >
      {/* ══ CỘT TRÁI: General Info + Apply Conditions ══ */}
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>

        {/* ── GENERAL INFORMATION ── */}
        <Paper sx={cardSx}>
          <Typography sx={sectionLabel}>General information</Typography>
          <Divider sx={{ borderStyle: 'dashed', mb: 1.5 }} />
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
            <Controller
              name="discount_name" control={control}
              rules={{ required: 'Required', minLength: { value: 3, message: 'Minimum 3 characters' } }}
              render={({ field }) => (
                <TextField {...field} label="Program name *" size="small" fullWidth
                  error={!!errors.discount_name} helperText={errors.discount_name?.message}
                  placeholder="e.g. Summer Flash Sale" sx={inputSx} />
              )}
            />
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1.5 }}>
              <Controller
                name="discount_code" control={control}
                render={({ field }) => (
                  <TextField {...field} label="Discount code" size="small" fullWidth
                    placeholder="SUMMER24"
                    onChange={(e) => field.onChange(e.target.value.toUpperCase())}
                    slotProps={{ style: { fontFamily: 'monospace', fontWeight: 800, letterSpacing: '0.08em' } }}
                    helperText="Optional" sx={inputSx} />
                )}
              />
              <Controller
                name="discount_max_uses_per_user" control={control}
                rules={{ required: 'Required', min: { value: 1, message: '≥ 1' } }}
                render={({ field }) => (
                  <TextField {...field} label="Uses / user *" size="small" type="number"
                    fullWidth placeholder="1"
                    error={!!errors.discount_max_uses_per_user}
                    helperText={errors.discount_max_uses_per_user?.message} sx={inputSx} />
                )}
              />
            </Box>
          </Box>
        </Paper>

        {/* ── APPLY CONDITIONS ── */}
        <Paper sx={cardSx}>
          <Typography sx={sectionLabel}>Apply conditions</Typography>
          <Divider sx={{ borderStyle: 'dashed', mb: 1.5 }} />
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1.5 }}>
              <Controller name="discount_start_date" control={control} rules={{ required: 'Required' }}
                render={({ field }) => (
                  <Box>
                    <Typography sx={{ fontSize: '0.68rem', color: 'text.secondary', fontWeight: 700, mb: 0.5 }}>Start date *</Typography>
                    <TextField {...field} size="small" type="date" fullWidth
                      error={!!errors.discount_start_date} helperText={errors.discount_start_date?.message}
                      sx={dateSx} />
                  </Box>
                )}
              />
              <Controller name="discount_end_date" control={control}
                rules={{ required: 'Required', validate: v => !watchStart || v >= watchStart || 'Must be after start date' }}
                render={({ field }) => (
                  <Box>
                    <Typography sx={{ fontSize: '0.68rem', color: 'text.secondary', fontWeight: 700, mb: 0.5 }}>End date *</Typography>
                    <TextField {...field} size="small" type="date" fullWidth
                      error={!!errors.discount_end_date} helperText={errors.discount_end_date?.message}
                      sx={dateSx} />
                  </Box>
                )}
              />
            </Box>
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1.5 }}>
              <Controller name="discount_is_active" control={control}
                render={({ field }) => (
                  <FormControl size="small" fullWidth sx={inputSx}>
                    <InputLabel>Active status *</InputLabel>
                    <Select
                      {...field}
                      value={field.value ? 'true' : 'false'}
                      onChange={(e) => field.onChange(e.target.value === 'true')}
                      label="Active status *"
                    >
                      <MenuItem value="true">Active</MenuItem>
                      <MenuItem value="false">Inactive</MenuItem>
                    </Select>
                  </FormControl>
                )}
              />
              {/* Applies to — replaces Scope select */}
              <Box>
                <Controller name="discount_applies_to" control={control}
                  render={({ field }) => (
                    <ToggleButtonGroup exclusive size="small" value={field.value}
                      onChange={(_, v) => v && field.onChange(v)}
                      sx={toggleSx('#0d9488', '#f0fdf4')}
                    >
                      <ToggleButton value="all">
                        <AllInclusiveRoundedIcon sx={{ fontSize: 13 }} /> All
                      </ToggleButton>
                      <ToggleButton value="specific">
                        <TuneRoundedIcon sx={{ fontSize: 13 }} /> Specific
                      </ToggleButton>
                    </ToggleButtonGroup>
                  )}
                />
              </Box>
            </Box>
          </Box>
        </Paper>

      </Box>

      {/* ══ CỘT PHẢI: Discount Value + Notes ══ */}
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>

        {/* ── DISCOUNT VALUE ── */}
        <Paper sx={cardSx}>
          <Typography sx={sectionLabel}>Discount value</Typography>
          <Divider sx={{ borderStyle: 'dashed', mb: 1.5 }} />
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1.5 }}>
              <Box>
                <Typography sx={{ fontSize: '0.68rem', color: 'text.secondary', fontWeight: 700, mb: 0.5 }}>Category</Typography>
                <Controller name="discount_target" control={control}
                  render={({ field }) => (
                    <ToggleButtonGroup exclusive size="small" value={field.value}
                      onChange={(_, v) => v && field.onChange(v)} sx={toggleSx('#7c3aed', '#f5f3ff')}>
                      <ToggleButton value="product"><LocalOfferRoundedIcon sx={{ fontSize: 13 }} /> Product</ToggleButton>
                      <ToggleButton value="shipping"><LocalShippingRoundedIcon sx={{ fontSize: 13 }} /> Free ship</ToggleButton>
                    </ToggleButtonGroup>
                  )}
                />
              </Box>
              <Box>
                <Typography sx={{ fontSize: '0.68rem', color: 'text.secondary', fontWeight: 700, mb: 0.5 }}>Discount type</Typography>
                <Controller name="discount_type" control={control}
                  render={({ field }) => (
                    <ToggleButtonGroup exclusive size="small" value={field.value}
                      onChange={(_, v) => v && field.onChange(v)} sx={toggleSx()}>
                      <ToggleButton value="percentage"><PercentRoundedIcon sx={{ fontSize: 13 }} /> Percentage</ToggleButton>
                      <ToggleButton value="fixed_amount"><AttachMoneyRoundedIcon sx={{ fontSize: 13 }} /> Fixed</ToggleButton>
                    </ToggleButtonGroup>
                  )}
                />
              </Box>
            </Box>

            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1.5 }}>
              <Controller name="discount_value" control={control}
                rules={{
                  validate: (value) => {
                    if (!value) return 'Required'
                    if (value < 1) return '> 0'
                    if (watchType !== 'fixed_amount' && value > 100) { return '≤ 100' } return true
                  }
                }}
                render={({ field }) => (
                  <TextField {...field} size="small" type="number" fullWidth
                    label={watchType === 'percentage' ? 'Discount rate (%) *' : 'Discount amount (₫) *'}
                    error={!!errors.discount_value} helperText={errors.discount_value?.message}
                    placeholder={watchType === 'percentage' ? '20' : '50000'} sx={inputSx} />
                )}
              />
              <Controller name="discount_max_value" control={control}
                rules={{ required: 'Required', min: { value: 1, message: '≥ 1' } }}
                render={({ field }) => (
                  <TextField {...field} size="small" type="number" fullWidth
                    label="Max discount (₫) *" placeholder="100000"
                    error={!!errors.discount_max_value} helperText={errors.discount_max_value?.message} sx={inputSx} />
                )}
              />
              <Controller name="discount_min_value" control={control}
                rules={{ required: 'Required', min: { value: 0, message: '≥ 0' } }}
                render={({ field }) => (
                  <TextField {...field} size="small" type="number" fullWidth
                    label="Min order (₫) *" placeholder="0"
                    error={!!errors.discount_min_value} helperText={errors.discount_min_value?.message} sx={inputSx} />
                )}
              />
              <Controller name="discount_max_uses" control={control}
                rules={{ required: 'Required', min: { value: 1, message: '≥ 1' } }}
                render={({ field }) => (
                  <TextField {...field} size="small" type="number" fullWidth
                    label="Usage limit *" placeholder="500"
                    error={!!errors.discount_max_uses} helperText={errors.discount_max_uses?.message} sx={inputSx} />
                )}
              />
            </Box>
          </Box>
        </Paper>

        {/* ── NOTES ── */}
        <Paper sx={cardSx}>
          <Typography sx={sectionLabel}>Notes</Typography>
          <Divider sx={{ borderStyle: 'dashed', mb: 1.5 }} />
          <Controller name="discount_description" control={control}
            render={({ field }) => (
              <TextField {...field} size="small" fullWidth multiline minRows={4}
                placeholder="Additional terms, internal notes..." sx={inputSx} />
            )}
          />
        </Paper>

      </Box>

      {/*  PRODUCT PICKER  */}
      <SelectProductForForm
        control={control}
        errors={errors}
        watchAppliesTo={watchAppliesTo}
        productSearch={productSearch}
        setProductSearch={setProductSearch}
        filteredProducts={filteredProducts}
        isLoading={isLoading}
        cardSx={cardSx}
        sectionLabel={sectionLabel}
        lastProductRef={lastProductRef}
      />
    </Box>
  )
}
