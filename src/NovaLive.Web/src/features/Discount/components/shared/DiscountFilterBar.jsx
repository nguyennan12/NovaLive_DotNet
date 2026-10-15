import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import { Box, InputAdornment, MenuItem, Select, TextField } from '@mui/material'

const selectSx = {
  fontSize: '0.8rem',
  borderRadius: '10px',
  height: 40,
  minWidth: 140,
  bgcolor: '#f9fafb',
  '& .MuiOutlinedInput-notchedOutline': {
    borderColor: '#e5e7eb',
    borderWidth: '1px !important'
  },
  '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: '#3b82f6' },
  '&.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: '#3b82f6' },
  '& .MuiSelect-icon': { color: '#64748b' }
}

export const DiscountFilterBar = ({
  search, status, type, category,
  onSearchChange, onStatusChange, onTypeChange, onCategoryChange
}) => (
  <Box sx={{
    display: 'flex', alignItems: 'center', gap: 2, mb: 3,
    p: 1.5, bgcolor: '#fff', border: '1px solid #e5e7eb', borderRadius: '16px',
    boxShadow: '0 2px 12px rgba(0,0,0,0.03)',
    flexWrap: 'wrap'
  }}>
    {/* Search */}
    <TextField
      placeholder='Tìm theo tên hoặc mã...'
      value={search}
      onChange={(e) => onSearchChange(e.target.value)}
      size='small'
      slotProps={{
        startAdornment: (
          <InputAdornment position='start'>
            <SearchRoundedIcon sx={{ fontSize: 18, color: '#94a3b8' }} />
          </InputAdornment>
        )
      }}
      sx={{
        flex: 1, minWidth: 200,
        '& .MuiOutlinedInput-root': {
          height: 40, fontSize: '0.8rem', borderRadius: '10px', bgcolor: '#f9fafb',
          '& fieldset': { borderColor: '#e5e7eb' },
          '&:hover fieldset': { borderColor: '#3b82f6' },
          '&.Mui-focused fieldset': { borderColor: '#3b82f6' }
        }
      }}
    />

    {/* Category */}
    <Select value={category} onChange={(e) => onCategoryChange(e.target.value)} size='small' sx={{ ...selectSx }}>
      <MenuItem value='all' sx={{ fontSize: '0.8rem' }}>Tất cả loại</MenuItem>
      <MenuItem value='product' sx={{ fontSize: '0.8rem', display: 'flex', gap: 1 }}> <LocalOfferRoundedIcon sx={{ fontSize: '0.9rem', color: '#3b82f6' }} />Sản phẩm</MenuItem>
      <MenuItem value='freeship' sx={{ fontSize: '0.8rem', display: 'flex', gap: 1 }}>< LocalShippingRoundedIcon sx={{ fontSize: '0.9rem', color: '#f59e0b' }} /> Free Ship</MenuItem>
    </Select>

    {/* Status */}
    <Select value={status} onChange={(e) => onStatusChange(e.target.value)} size='small' sx={{ ...selectSx, minWidth: 160 }}>
      <MenuItem value='all' sx={{ fontSize: '0.8rem' }}>Tất cả trạng thái</MenuItem>
      <MenuItem value='active' sx={{ fontSize: '0.8rem' }}>Đang chạy</MenuItem>
      <MenuItem value='draft' sx={{ fontSize: '0.8rem' }}>Bản nháp</MenuItem>
      <MenuItem value='expired' sx={{ fontSize: '0.8rem' }}>Hết hạn</MenuItem>
    </Select>

    {/* Type */}
    <Select value={type} onChange={(e) => onTypeChange(e.target.value)} size='small' sx={{ ...selectSx }}>
      <MenuItem value='all' sx={{ fontSize: '0.8rem' }}>Hình thức giảm</MenuItem>
      <MenuItem value='percentage' sx={{ fontSize: '0.8rem' }}>Phần trăm (%)</MenuItem>
      <MenuItem value='fixed' sx={{ fontSize: '0.8rem' }}>Số tiền (VNĐ)</MenuItem>
    </Select>
  </Box>
)

