import { Autocomplete, Box, Chip, CircularProgress, MenuItem, Paper, Select, TextField, Typography } from '@mui/material'
import { useCallback, useEffect, useMemo, useRef } from 'react'
import { Controller, useFormContext, useWatch } from 'react-hook-form'
import { useDispatch, useSelector } from 'react-redux'
import {
  fetchAttributesBySlugAPI,
  fetchCategoriesAPI,
  selectAttributesBySlug,
  selectCategories,
  selectCategoryLoading,
  setAttributesBySlug
} from '~/store/product/categorySlice'

export default function ProductDetailsCard() {
  const dispatch = useDispatch()
  const categories = useSelector(selectCategories)
  const attributesBySlug = useSelector(selectAttributesBySlug)
  const loading = useSelector(selectCategoryLoading)

  const { control, setValue, getValues } = useFormContext()

  // category slugs
  const rawSlugs = useWatch({ control, name: 'spu_category' })
  const selectedSlugs = useMemo(() => (Array.isArray(rawSlugs) ? rawSlugs : rawSlugs ? [rawSlugs] : []), [rawSlugs])
  const selectedKey = useMemo(() => selectedSlugs.join('|'), [selectedSlugs])

  useEffect(() => {
    if (!categories?.length) dispatch(fetchCategoriesAPI())
  }, [dispatch, categories?.length])

  const categoriesBySlug = useMemo(() => new Map((categories || []).map((c) => [c.cat_slug, c])), [categories])

  // Build spu_attributes array from attributes object
  const rebuildSpuAttributes = useCallback(() => {
    const obj = getValues('attributes') || {}

    const spu_attributes = Object.entries(obj)
      .map(([attr_id, v]) => {
        if (v == null) return null

        // multi-select nếu bạn lưu array thì join, nếu lưu text thì trim
        const attr_value = Array.isArray(v)
          ? v.map((x) => String(x ?? '').trim()).filter(Boolean).join(',')
          : String(v ?? '').trim()

        if (!attr_value) return null
        return { attr_id, attr_value }
      })
      .filter(Boolean)

    setValue('spu_attributes', spu_attributes, { shouldDirty: true, shouldValidate: false })
  }, [getValues, setValue])

  // prevent loop
  const prevSelectedKeyRef = useRef('___init___')
  useEffect(() => {
    if (prevSelectedKeyRef.current === selectedKey) return
    prevSelectedKeyRef.current = selectedKey

    if (!selectedSlugs.length) {
      dispatch(setAttributesBySlug([]))
      setValue('attributes', {}, { shouldDirty: true, shouldValidate: false })
      setValue('spu_attributes', [], { shouldDirty: true, shouldValidate: false })
      return
    }

    setValue('attributes', {}, { shouldDirty: true, shouldValidate: false })
    setValue('spu_attributes', [], { shouldDirty: true, shouldValidate: false })
    dispatch(fetchAttributesBySlugAPI(selectedSlugs))
  }, [dispatch, selectedKey, selectedSlugs, setValue])

  const sortedAttrs = useMemo(() => {
    const arr = [...(attributesBySlug || [])]
    return arr.sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0))
  }, [attributesBySlug])

  const showAttributes = selectedSlugs.length > 0 && sortedAttrs.length > 0

  return (
    <Paper variant="outlined" sx={{ p: 2.5, borderRadius: 3, border: '1px solid secondary.contrastText', mt: 2 }}>
      <Typography variant="subtitle1" fontWeight={700} mb={2}>
        Categories & Attributes
      </Typography>

      {/* Categories */}
      <Box sx={{ mt: 2 }}>
        <Typography variant="caption" color="primary.contrastText" display="block" mb={1}>
          Categories
        </Typography>

        <Controller
          name="spu_category"
          control={control}
          render={({ field }) => {
            const currentSlugs = Array.isArray(field.value) ? field.value : field.value ? [field.value] : []
            const selectedObjects = currentSlugs.map((slug) => categoriesBySlug.get(slug)).filter(Boolean)

            return (
              <Autocomplete
                multiple
                size="small"
                options={categories || []}
                value={selectedObjects || []}
                isOptionEqualToValue={(opt, val) => opt.cat_slug === val.cat_slug}
                getOptionLabel={(opt) => opt?.cat_name || ''}
                onChange={(_, newObjects) => field.onChange((newObjects || []).map((c) => c.cat_slug))}
                renderTags={(val, getTagProps) =>
                  val.map((cat, index) => (
                    <Chip
                      {...getTagProps({ index })}
                      key={cat.cat_slug}
                      label={cat.cat_name}
                      size="small"
                      sx={{
                        background: 'linear-gradient(135deg, #eef2ff, #dbeafe)',
                        color: 'primary.main',
                        fontWeight: 600,
                        fontSize: '0.72rem'
                      }}
                    />
                  ))
                }
                renderInput={(params) => (
                  <TextField
                    {...params}
                    inputRef={field.ref}
                    placeholder="Add categories..."
                    sx={{ '& .MuiOutlinedInput-root': { borderRadius: 2.5, background: 'primary.contrastText' } }}
                  />
                )}
              />
            )
          }}
        />
      </Box>

      {/* Loading attributes */}
      {loading && selectedSlugs.length > 0 && (
        <Box sx={{ display: 'flex', alignItems: 'center' }} gap={1} mt={2}>
          <CircularProgress size={16} />
          <Typography variant="caption" color="text.secondary">
            Loading attributes...
          </Typography>
        </Box>
      )}

      {/* Attributes */}
      {showAttributes && !loading && (
        <Box sx={{ mt: 2 }}>
          <Typography variant="caption" fontWeight={700} color="primary.main" display="block" mb={1}>
            Attributes
          </Typography>

          <Box sx={{ p: 1, background: 'primary.contrastText', borderRadius: 2.5, border: '2px solid secondary.contrastText' }}>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'space-evenly', gap: 1 }}>
              {sortedAttrs.map((attr) => (
                <Box key={attr.attr_id}>
                  <Box sx={{ width: { xs: '100px', sm: '200px' }, display: 'flex', flexDirection: 'column', gap: 0.5, justifyContent: 'space-between' }}>
                    <Typography variant="caption" color="text.secondary" display="block" mb={0.75}>
                      {attr.attr_name} {attr.isRequired ? '*' : ''}
                    </Typography>

                    <Controller
                      // giữ attributes.<id> để validate dễ
                      name={`attributes.${attr.attr_id}`}
                      control={control}
                      rules={{
                        validate: (v) => {
                          if (!attr.isRequired) return true
                          if (attr.attr_type === 'multi-select') {
                            return Array.isArray(v) ? v.length > 0 : !!String(v || '').trim()
                          }
                          return !!String(v || '').trim()
                        }
                      }}
                      render={({ field, fieldState }) => {
                        const commonProps = {
                          fullWidth: true,
                          size: 'small',
                          error: !!fieldState.error,
                          helperText: fieldState.error?.message || '',
                          sx: { '& .MuiOutlinedInput-root': { borderRadius: 2, background: 'primary.contrastText' } }
                        }

                        if (attr.attr_type === 'select') {
                          return (
                            <Select
                              {...field}
                              fullWidth
                              size="small"
                              displayEmpty
                              value={field.value || ''}
                              onChange={(e) => {
                                field.onChange(e.target.value)
                                rebuildSpuAttributes()
                              }}
                              sx={{

                                borderRadius: 2,
                                background: 'primary.contrastText',
                                '& .MuiOutlinedInput-notchedOutline': { borderColor: 'secondary.main' },
                                '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'secondary.main' },
                                '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: 'secondary.main' }
                              }}
                            >
                              <MenuItem value="" disabled>
                                Select {attr.attr_name}
                              </MenuItem>
                              {(attr.attr_options || []).map((opt) => (
                                <MenuItem key={opt} value={opt}>
                                  {opt}
                                </MenuItem>
                              ))}
                            </Select>
                          )
                        }

                        if (attr.attr_type === 'number') {
                          return (
                            <TextField
                              {...field}
                              {...commonProps}
                              type="number"
                              value={field.value ?? ''}
                              placeholder={attr.attr_unit ? `(${attr.attr_unit})` : ''}
                              onChange={(e) => {
                                field.onChange(e.target.value)
                                rebuildSpuAttributes()
                              }}
                            />
                          )
                        }

                        if (attr.attr_type === 'multi-select') {
                          return (
                            <TextField
                              {...field}
                              {...commonProps}
                              value={field.value ?? ''}
                              onChange={(e) => {
                                field.onChange(e.target.value)
                                rebuildSpuAttributes()
                              }}
                            />
                          )
                        }

                        return (
                          <TextField
                            {...field}
                            {...commonProps}
                            type="text"
                            value={field.value ?? ''}
                            onChange={(e) => {
                              field.onChange(e.target.value)
                              rebuildSpuAttributes()
                            }}
                          />
                        )
                      }}
                    />
                  </Box>

                </Box>
              ))}
            </Box>
          </Box>
        </Box>
      )
      }
    </Paper >
  )
}