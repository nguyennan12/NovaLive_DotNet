import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import { Avatar, Box, Checkbox, Chip, CircularProgress, Divider, InputAdornment, List, ListItemButton, ListItemIcon, ListItemText, Paper, TextField, Typography } from '@mui/material'
import { Controller } from 'react-hook-form'

export const SelectProductForForm = ({
  control,
  errors,
  watchAppliesTo,
  productSearch,
  setProductSearch,
  filteredProducts,
  isLoading,
  cardSx,
  sectionLabel,
  lastProductRef
}) => {
  return (
    <>
      {watchAppliesTo === 'specific' && (
        <Box sx={{ gridColumn: { md: '1 / -1' } }}>
          <Paper sx={cardSx}>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 0.5 }}>
              <Typography sx={sectionLabel}>Specific products</Typography>
              <Controller name="discount_product_ids" control={control}
                render={({ field }) => (
                  <Typography sx={{ fontSize: '0.72rem', color: 'secondary.main', fontWeight: 600 }}>
                    {(field.value?.length || 0)} selected
                  </Typography>
                )}
              />
            </Box>
            <Divider sx={{ borderStyle: 'dashed', mb: 1.5 }} />

            <Controller
              name="discount_product_ids"
              control={control}
              rules={{
                validate: (val) =>
                  watchAppliesTo !== 'specific' || (val && val.length > 0) || 'Select at least one product'
              }}
              render={({ field }) => {
                const selectedIds = field.value || []

                const handleToggle = (spuCode) => {
                  const next = selectedIds.includes(spuCode)
                    ? selectedIds.filter(id => id !== spuCode)
                    : [...selectedIds, spuCode]
                  field.onChange(next)
                }

                return (
                  <Box>
                    {/* Search input */}
                    <TextField
                      size="small" fullWidth
                      placeholder="Search by product name or code..."
                      value={productSearch}
                      onChange={e => setProductSearch(e.target.value)}
                      slotProps={{
                        input: {
                          startAdornment: (
                            <InputAdornment position="start">
                              <SearchRoundedIcon sx={{ fontSize: 16, color: 'text.disabled' }} />
                            </InputAdornment>
                          )
                        }
                      }}
                      sx={{ mb: 1.5, '& .MuiOutlinedInput-root': { borderRadius: '8px', fontSize: '0.82rem' } }}
                    />

                    {/* Selected product chips */}
                    {selectedIds.length > 0 && (
                      <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mb: 1.5 }}>
                        {selectedIds.map(id => {
                          const product = filteredProducts.find(p => (p.spu_code ?? p._id) === id)
                          const label = product ? (product.product_name ?? product.spu_name ?? id) : id
                          return (
                            <Chip
                              key={id} label={label} size="small"
                              onDelete={() => handleToggle(id)}
                              sx={{
                                fontSize: '0.7rem', height: 22,
                                bgcolor: '#e6efff', color: 'secondary.main', fontWeight: 600,
                                '& .MuiChip-deleteIcon': { fontSize: 14 }
                              }}
                            />
                          )
                        })}
                      </Box>
                    )}

                    {/* Product list */}
                    {isLoading ? (
                      <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                        <CircularProgress size={24} sx={{ color: 'secondary.main' }} />
                      </Box>
                    ) : filteredProducts.length === 0 ? (
                      <Box sx={{ py: 4, textAlign: 'center' }}>
                        <Typography sx={{ fontSize: '0.78rem', color: 'text.disabled' }}>
                          {filteredProducts.length === 0 ? 'No products found' : 'No matching products'}
                        </Typography>
                      </Box>
                    ) : (
                      <List dense disablePadding sx={{
                        maxHeight: 300, overflow: 'auto',
                        border: '1px solid', borderColor: 'divider', borderRadius: 2
                      }}>
                        {filteredProducts.map((product, idx) => {
                          const code = product.spu_code ?? product._id
                          const name = product.product_name ?? product.spu_name ?? 'Unnamed'
                          const thumb = product.product_thumb ?? product.spu_thumb
                          const isSelected = selectedIds.includes(code)
                          const isLast = filteredProducts.length === idx + 1

                          return (
                            <ListItemButton
                              key={code ?? idx} dense selected={isSelected}
                              ref={isLast ? lastProductRef : null}
                              onClick={() => handleToggle(code)}
                              sx={{
                                px: 1.5, py: 0.75,
                                borderBottom: idx < filteredProducts.length - 1 ? '1px solid' : 'none',
                                borderColor: 'divider',
                                '&.Mui-selected': { bgcolor: 'rgba(52,133,247,0.06)' },
                                '&.Mui-selected:hover': { bgcolor: 'rgba(52,133,247,0.1)' }
                              }}
                            >
                              <ListItemIcon sx={{ minWidth: 34 }}>
                                <Checkbox
                                  edge="start" checked={isSelected}
                                  tabIndex={-1} disableRipple size="small"
                                  sx={{ p: 0, color: 'divider', '&.Mui-checked': { color: 'secondary.main' } }}
                                />
                              </ListItemIcon>
                              {thumb && (
                                <Avatar
                                  variant="rounded" src={thumb}
                                  sx={{ width: 34, height: 34, mr: 1.25, flexShrink: 0, border: '1px solid', borderColor: 'divider' }}
                                />
                              )}
                              <ListItemText
                                primary={name}
                                secondary={code}
                                primaryTypographyProps={{ fontSize: '0.78rem', fontWeight: 600, noWrap: true, color: '#1a1a1a' }}
                                secondaryTypographyProps={{ fontSize: '0.65rem', fontFamily: 'monospace', noWrap: true, color: '#9ca3af' }}
                              />
                            </ListItemButton>
                          )
                        })}
                      </List>
                    )}
                    <Box sx={{ display: 'flex', justifyContent: 'center', p: 1 }}>
                      <CircularProgress size={20} />
                    </Box>

                    {errors.discount_product_ids && (
                      <Typography sx={{ fontSize: '0.7rem', color: 'error.main', mt: 0.75 }}>
                        {errors.discount_product_ids.message}
                      </Typography>
                    )}
                  </Box>
                )
              }}
            />
          </Paper>
        </Box>
      )}
    </>

  )
}