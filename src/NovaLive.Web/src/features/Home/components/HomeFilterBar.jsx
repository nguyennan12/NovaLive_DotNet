import TuneRoundedIcon from '@mui/icons-material/TuneRounded'
import { Box, MenuItem, Select, Slider, Typography } from '@mui/material'
import { useState } from 'react'
import { formatVND } from '~/common/utils/formatters'
import { PRICE_SLIDER_MAX, PRICE_SLIDER_MIN, SORT_OPTIONS, PRICE_SLIDER_STEP } from '~/common/utils/constant'
import { glassSx } from '~/theme'


const sliderLabel = (v) => {
  if (v === 0) return '0'
  if (v >= 1_000_000) {
    const m = v / 1_000_000
    return `${Number.isInteger(m) ? m : m.toFixed(1)}M`
  }
  return `${v / 1_000}k`
}

const HomeFilterBar = ({ filters, onFilterChange }) => {
  const [localRange, setLocalRange] = useState(filters.priceRange)
  const [prevPriceRange, setPrevPriceRange] = useState(filters.priceRange)

  if (prevPriceRange !== filters.priceRange) {
    setPrevPriceRange(filters.priceRange)
    setLocalRange(filters.priceRange)
  }

  const handleSliderChange = (_, newValue) => setLocalRange(newValue)
  const handleSliderCommit = (_, newValue) => onFilterChange('priceRange', newValue)

  const isDefaultRange = localRange[0] === PRICE_SLIDER_MIN && localRange[1] === PRICE_SLIDER_MAX

  return (
    <Box sx={{
      bgcolor: 'primary.main',
      borderRadius: { xs: '14px', sm: '16px' },
      px: { xs: 1.5, sm: 2, md: 2.5 },
      py: { xs: 1, sm: 1.5 },
      boxShadow: '0 1px 12px rgba(79,79,79,0.11)',
      my: 3,
      display: 'flex',
      ...glassSx,
      alignItems: 'center',
      justifyContent: 'center',
      gap: { xs: 2, sm: 3 },
      flexWrap: 'wrap'
    }}>
      {/* Label "Lọc" */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.6, opacity: 0.4 }}>
        <TuneRoundedIcon sx={{ fontSize: 15 }} />
        <Typography sx={{ fontSize: '0.75rem', fontWeight: 700 }}>Lọc</Typography>
      </Box>

      {/* Price range slider */}
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5, minWidth: { xs: 200, sm: 260, md: 320 }, flex: 1, maxWidth: 400 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Typography sx={{ fontSize: '0.72rem', color: 'text.secondary', fontWeight: 600 }}>Khoảng giá</Typography>
          {!isDefaultRange && (
            <Typography
              onClick={() => {
                const reset = [PRICE_SLIDER_MIN, PRICE_SLIDER_MAX]
                setLocalRange(reset)
                onFilterChange('priceRange', reset)
              }}
              sx={{ fontSize: '0.68rem', color: 'secondary.main', cursor: 'pointer', fontWeight: 600, '&:hover': { opacity: 0.7 } }}
            >
              Xóa lọc
            </Typography>
          )}
        </Box>

        <Slider
          value={localRange}
          onChange={handleSliderChange}
          onChangeCommitted={handleSliderCommit}
          min={PRICE_SLIDER_MIN}
          max={PRICE_SLIDER_MAX}
          step={PRICE_SLIDER_STEP}
          valueLabelDisplay="auto"
          valueLabelFormat={sliderLabel}
          disableSwap
          sx={{
            color: 'secondary.main',
            height: 4,
            '& .MuiSlider-thumb': {
              width: 16, height: 16,
              '&:hover, &.Mui-focusVisible': { boxShadow: '0 0 0 6px rgba(52,133,247,0.16)' }
            },
            '& .MuiSlider-valueLabel': {
              fontSize: '0.7rem', py: 0.4, px: 0.8,
              background: 'rgba(52,133,247,0.9)', borderRadius: '6px'
            },
            '& .MuiSlider-rail': { opacity: 0.2 }
          }}
        />

        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
          <Typography sx={{ fontSize: '0.7rem', color: 'secondary.main', fontWeight: 600 }}>
            {formatVND(localRange[0])}
          </Typography>
          <Typography sx={{ fontSize: '0.7rem', color: 'secondary.main', fontWeight: 600 }}>
            {localRange[1] === PRICE_SLIDER_MAX ? `${sliderLabel(PRICE_SLIDER_MAX)}+` : formatVND(localRange[1])}
          </Typography>
        </Box>
      </Box>

      {/* Sort */}
      <Select
        value={filters.sort}
        onChange={(e) => onFilterChange('sort', e.target.value)}
        sx={{
          bgcolor: 'rgba(52, 133, 247, 0.04)',
          borderRadius: 2,
          color: 'primary.contrastText',
          fontSize: '0.8rem',
          fontWeight: 500,
          '.MuiOutlinedInput-notchedOutline': { borderColor: 'rgba(83, 155, 255, 0.3)' },
          '&:hover .MuiOutlinedInput-notchedOutline': { borderColor: 'secondary.main' },
          '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
            borderColor: 'secondary.main',
            borderWidth: '1px'
          },
          '.MuiSvgIcon-root': { color: 'secondary.main' }
        }}
      >
        {SORT_OPTIONS.map(opt => (
          <MenuItem key={opt.value} value={opt.value} sx={{
            fontSize: '0.8rem'
          }}>{opt.label}</MenuItem>
        ))}
      </Select>
    </Box>
  )
}

export default HomeFilterBar
