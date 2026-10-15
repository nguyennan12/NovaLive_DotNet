import AddIcon from '@mui/icons-material/Add'
import DeleteOutlineOutlinedIcon from '@mui/icons-material/DeleteOutlineOutlined'
import { Autocomplete, Box, Button, Chip, Grid, IconButton, InputAdornment, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, TextField, Tooltip, Typography } from '@mui/material'
import { useEffect } from 'react'
import { Controller, useFieldArray, useFormContext } from 'react-hook-form'
import { generateCombinations } from '~/common/utils/generator'

export default function ProductVariantsCard() {

  const { control, watch, setValue, getValues } = useFormContext()
  const variations = watch('spu_variations') || []
  const { fields: variationFields, append: appendVariation, remove: removeVariation } = useFieldArray({ control, name: 'spu_variations' })//danh sách variations
  const { fields: skuFields } = useFieldArray({ control, name: 'sku_list' })//danh sách skus

  //khi variations thay đổi (thêm mới) thì generate ra sku
  useEffect(() => {
    const currentSkus = getValues('sku_list') || []
    if (variations.length === 0) {
      if (currentSkus.length > 0) return
      return
    }

    const combinations = generateCombinations(variations)

    const newSkuList = combinations.map(comb => {
      const tierIdx = comb.indices
      const existingSku = currentSkus.find(sku =>
        JSON.stringify(sku.sku_tier_idx) === JSON.stringify(tierIdx)
      )
      if (existingSku) { return { ...existingSku, tier_options: comb.options } }
      return {
        sku_tier_idx: tierIdx,
        tier_options: comb.options,
        sku_price: '',
        sku_stock: '',
        sku_weight: ''
      }
    })

    const isDifferent =
      newSkuList.length !== currentSkus.length ||
      newSkuList.some((sku, i) => JSON.stringify(sku.sku_tier_idx) !== JSON.stringify(currentSkus[i]?.sku_tier_idx))

    if (isDifferent) { setValue('sku_list', newSkuList) }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [JSON.stringify(variations)])


  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, mt: 3 }}>
      {/* VARIATIONS */}
      <Paper variant="outlined" sx={{ p: 3, borderRadius: 3, border: '1px solid secondary.contrastText' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Box>
            <Typography variant="h6" fontWeight={700}>Variations</Typography>
            <Typography variant="caption" color="text.secondary">Add product variations like Color or Size</Typography>
          </Box>
          <Button
            variant="contained"
            startIcon={<AddIcon sx={{ fontWeight: 550 }} />}
            size="small"
            onClick={() => appendVariation({ name: '', options: [] })}
            sx={{
              borderRadius: 2, textTransform: 'none', fontWeight: 550,
              background: 'linear-gradient(90deg, #75a3ff, #7fcbfd, #8acdde)',
              boxShadow: 'none',
              color: '#fff'
            }}
          >
            Add
          </Button>
        </Box>

        {variationFields.length === 0 && (
          <Box sx={{ p: 4, textAlign: 'center', background: 'primary.contrastText', borderRadius: 2, border: '1px dashed #ccc' }}>
            <Typography variant="body2" color="primary.contrastText">No variation added yet. Click Add to create one.</Typography>
          </Box>
        )}

        {variationFields.map((item, index) => (
          <Box
            key={item.id}
            sx={{
              p: 2, mb: 2, borderRadius: 2, background: 'primary.contrastText', border: '1px solid secondary.contrastText',
              position: 'relative'
            }}
          >
            <Tooltip title="Remove this attribute">
              <IconButton
                size="small" color="error" onClick={() => removeVariation(index)}
                sx={{ position: 'absolute', top: 0, right: 8, background: 'primary.main', border: '1px solid #fee2e2' }}
              >
                <DeleteOutlineOutlinedIcon fontSize="small" />
              </IconButton>
            </Tooltip>

            <Grid container spacing={2} sx={{ alignItems: 'center' }}>
              <Grid size={{ xs: 12, sm: 4 }}>
                {/* VARIATION */}
                <Typography variant="caption" fontWeight={600} display="block" mb={0.5}>Variation Name</Typography>
                <Controller
                  name={`spu_variations.${index}.name`}
                  control={control}
                  render={({ field }) => (
                    <TextField
                      {...field} fullWidth size="small" placeholder="e.g. Color, Size"
                      sx={{ '& .MuiOutlinedInput-root': { background: 'primary.contrastText' } }}
                    />
                  )}
                />
              </Grid>

              <Grid size={{ xs: 12, sm: 8 }} >
                {/* OPTIONS */}
                <Typography variant="caption" fontWeight={600} display="block" mb={0.5}>Options (Press Enter to add)</Typography>
                <Controller
                  name={`spu_variations.${index}.options`}
                  control={control}
                  render={({ field: { onChange, value } }) => (
                    <Autocomplete
                      multiple freeSolo clearIcon={false}
                      options={[]}
                      value={value || []}
                      onChange={(_, newValue) => onChange(newValue)}
                      renderTags={(val, getTagProps) =>
                        val.map((option, idx) => (
                          <Chip
                            {...getTagProps({ index: idx })} key={idx} label={option} size="small"
                            sx={{ background: 'primary.main', color: 'primary.contrastText', fontWeight: 600 }}
                          />
                        ))
                      }
                      renderInput={(params) => (
                        <TextField
                          {...params} size="small" placeholder="Add options..."
                          sx={{ '& .MuiOutlinedInput-root': { background: 'primary.contrastText' } }}
                        />
                      )}
                    />
                  )}
                />
              </Grid>
            </Grid>
          </Box>
        ))}
      </Paper>
      {/* GENERATE SKU LIST */}
      {skuFields.length > 0 && (
        <Paper variant="outlined" sx={{ p: 3, borderRadius: 3, border: '1px solid secondary.contrastText' }}>
          <Typography variant="h6" fontWeight={700} mb={0.5}> SKU List</Typography>
          <Typography variant="caption" color="text.secondary" display="block" mb={2}>
            Set prices, stock, and SKU numbers for each combination.
          </Typography>

          <TableContainer sx={{ border: '1px solid secondary.contrastText', borderRadius: 2 }}>
            <Table size="small">
              <TableHead sx={{ background: 'secondary.contrastText' }}>
                <TableRow>
                  <TableCell><Typography variant="caption" fontWeight={700}>Variant</Typography></TableCell>
                  <TableCell><Typography variant="caption" fontWeight={700}>Price<span style={{ color: 'red' }}>*</span></Typography></TableCell>
                  <TableCell><Typography variant="caption" fontWeight={700}>Stock <span style={{ color: 'red' }}>*</span></Typography></TableCell>
                  <TableCell><Typography variant="caption" fontWeight={700}>Weight</Typography></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {skuFields.map((item, index) => (
                  <TableRow key={item.id} hover>

                    {/* VARIATION */}
                    <TableCell>
                      {item.tier_options.map((opt, i) => (
                        <Chip
                          key={i} label={opt} size="small"
                          sx={{ mr: 0.5, borderRadius: 1, background: 'primary.main', color: 'primary.contrastText', fontWeight: 500, fontSize: '11px' }}
                        />
                      ))}
                    </TableCell>

                    {/* PRICE */}
                    <TableCell sx={{ minWidth: 140 }}>
                      <Controller
                        name={`sku_list.${index}.sku_price`}
                        control={control}
                        rules={{ required: 'Price is required', min: { value: 0, message: 'Price invalid' } }}
                        render={({ field, fieldState: { error } }) => (
                          <TextField
                            {...field} fullWidth size="small" type="number" placeholder="0"
                            error={!!error} helperText={error?.message}
                            onChange={(e) => {
                              const value = e.target.value
                              field.onChange(value === '' ? undefined : Number(value))
                            }}
                            slotProps={{
                              formHelperText: { sx: { mx: 0, mt: 0.5 } },
                              startAdornment: <InputAdornment position="start"><Typography variant="body2">$</Typography></InputAdornment>
                            }}
                          />
                        )}
                      />
                    </TableCell>

                    {/* Stock */}
                    <TableCell sx={{ minWidth: 120 }}>
                      <Controller
                        name={`sku_list.${index}.sku_stock`}
                        control={control}
                        rules={{ required: 'Stock is required', min: { value: 0, message: 'Stock invalid' } }}
                        render={({ field, fieldState: { error } }) => (
                          <TextField {...field} fullWidth size="small" type="number" placeholder="0"
                            error={!!error} helperText={error?.message}
                            onChange={(e) => {
                              const value = e.target.value
                              field.onChange(value === '' ? undefined : Number(value))
                            }}
                            slotProps={{ formHelperText: { sx: { mx: 0, mt: 0.5 } } }}
                          />
                        )}
                      />
                    </TableCell>

                    {/* WEIGHT */}
                    <TableCell sx={{ minWidth: 160 }}>
                      <Controller
                        name={`sku_list.${index}.sku_weight`}
                        control={control}
                        rules={{ required: 'Weight is required', min: { value: 0, message: 'Weight invalid' } }}
                        render={({ field, fieldState: { error } }) => (
                          <TextField {...field} fullWidth size="small" type="number" placeholder="gam"
                            error={!!error} helperText={error?.message}
                            onChange={(e) => {
                              const value = e.target.value
                              field.onChange(value === '' ? undefined : Number(value))
                            }}
                            slotProps={{
                              formHelperText: { sx: { mx: 0, mt: 0.5 } },
                              startAdornment: <InputAdornment position="start"><Typography variant="body2">$</Typography></InputAdornment>
                            }}
                          />
                        )}
                      />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      )}
    </Box>
  )
}