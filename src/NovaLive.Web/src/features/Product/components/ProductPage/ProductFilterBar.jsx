import { Box, TextField, Select, MenuItem, InputAdornment, Typography } from '@mui/material'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import FilterListRoundedIcon from '@mui/icons-material/FilterListRounded'
import { useSelector } from 'react-redux'
import { selectCategories } from '~/store/product/categorySlice'
import { useFormContext, Controller } from 'react-hook-form'

const selectSx = {
  fontSize: '0.8rem',
  borderRadius: '8px',
  height: 36,
  minWidth: 130,
  color: 'primary.contrastText',
  '& .MuiOutlinedInput-notchedOutline': {
    borderColor: 'divider',
    borderWidth: '1px !important'
  },
  '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'secondary.main' },
  '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: 'secondary.main' },
  '& .MuiSelect-icon': { color: 'primary.contrastText', opacity: 0.45 }
}

const menuItemSx = { fontSize: '0.8rem' }

const ProductFilterBar = () => {
  const categories = useSelector(selectCategories)
  const { control, register } = useFormContext()

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 1.5,
        mb: 2,
        flexWrap: 'wrap'
      }}
    >
      {/* Search */}
      <TextField
        {...register('keyword')}
        placeholder='Search by name or SKU...'
        size='small'
        slotProps={{
          input: {
            startAdornment: (
              <InputAdornment position="start">
                <SearchRoundedIcon sx={{ fontSize: 16, opacity: 0.4 }} />
              </InputAdornment>
            )
          }
        }}
        sx={{
          flex: 1,
          minWidth: 200,
          '& .MuiOutlinedInput-root': {
            height: 36,
            fontSize: '0.8rem',
            borderRadius: '8px',
            '& fieldset': { borderColor: 'divider' }
          },
          '& input': { color: 'primary.contrastText' },
          '& input::placeholder': { opacity: 0.4 }
        }}
      />

      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, opacity: 0.45 }}>
        <FilterListRoundedIcon sx={{ fontSize: 15, color: 'primary.contrastText' }} />
        <Typography sx={{ fontSize: '0.75rem', color: 'primary.contrastText', fontWeight: 500 }}>
          Filter
        </Typography>
      </Box>

      {/* Category */}
      <Controller
        name="category"
        control={control}
        render={({ field }) => (
          <Select {...field} sx={selectSx}>
            <MenuItem value='all' sx={menuItemSx}>All Categories</MenuItem>
            {categories?.map((cate) => (
              <MenuItem key={cate.id || cate._id} value={cate.cat_slug} sx={menuItemSx}>
                {cate.cat_name}
              </MenuItem>
            ))}
          </Select>
        )}
      />

      {/* Stock status */}
      <Controller
        name="stock"
        control={control}
        render={({ field }) => (
          <Select {...field} sx={selectSx}>
            <MenuItem value='all' sx={menuItemSx}>All Stock</MenuItem>
            <MenuItem value='in' sx={menuItemSx}>In Stock</MenuItem>
            <MenuItem value='low' sx={menuItemSx}>Low Stock</MenuItem>
            <MenuItem value='out' sx={menuItemSx}>Out of Stock</MenuItem>
          </Select>
        )}
      />

      {/* Status */}
      <Controller
        name="status"
        control={control}
        render={({ field }) => (
          <Select {...field} sx={selectSx}>
            <MenuItem value='all' sx={menuItemSx}>Status</MenuItem>
            <MenuItem value='published' sx={menuItemSx}>Publish</MenuItem>
            <MenuItem value='draft' sx={menuItemSx}>Draft</MenuItem>
            <MenuItem value='sold out' sx={menuItemSx}>Sold out</MenuItem>
          </Select>
        )}
      />

      {/* Sort */}
      <Controller
        name="sort"
        control={control}
        render={({ field }) => (
          <Select {...field} sx={{ ...selectSx, minWidth: 140 }}>
            <MenuItem value='newest' sx={menuItemSx}>Newest First</MenuItem>
            <MenuItem value='name_az' sx={menuItemSx}>Name A → Z</MenuItem>
            <MenuItem value='price_asc' sx={menuItemSx}>Price ↑</MenuItem>
            <MenuItem value='price_desc' sx={menuItemSx}>Price ↓</MenuItem>
          </Select>
        )}
      />
    </Box>
  )
}

export default ProductFilterBar
