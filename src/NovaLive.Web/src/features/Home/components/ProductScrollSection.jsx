import ArrowForwardIosRoundedIcon from '@mui/icons-material/ArrowForwardIosRounded'
import ChevronLeftRoundedIcon from '@mui/icons-material/ChevronLeftRounded'
import ChevronRightRoundedIcon from '@mui/icons-material/ChevronRightRounded'
import { Box, IconButton, Typography } from '@mui/material'
import { useRef } from 'react'
import { HomeProductCard, HomeProductCardSkeleton } from './HomeProductCard'
import { FlashSaleCountdown } from '~/common/components/common/countdown/CoundownBox'
import BoltRoundedIcon from '@mui/icons-material/BoltRounded'

const FlashSaleHeader = ({ endTime, onViewAll }) => {
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2.5 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: { xs: 1.5, sm: 2 } }}>
        <Box sx={{ width: { xs: 4, sm: 6 }, height: { xs: 28, sm: 36 }, borderRadius: '4px', bgcolor: '#e8472a', flexShrink: 0 }} />

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <Typography sx={{
            fontSize: { xs: '1.5rem', sm: '2.5rem' },
            fontWeight: 900,
            color: '#e8472a',
            letterSpacing: '-0.02em',
            lineHeight: 1
          }}>
            FL
          </Typography>
          <BoltRoundedIcon sx={{ fontSize: { xs: 24, sm: 36 }, color: '#e8472a', mx: '-4px' }} />
          <Typography sx={{
            fontSize: { xs: '1.5rem', sm: '2.5rem' },
            fontWeight: 900,
            color: '#e8472a',
            letterSpacing: '-0.02em',
            lineHeight: 1
          }}>
            SH
          </Typography>
          <Typography sx={{
            fontSize: { xs: '1.5rem', sm: '2.5rem' },
            fontWeight: 900,
            color: 'text.primary',
            letterSpacing: '-0.02em',
            lineHeight: 1,
            ml: 1
          }}>
            SALE
          </Typography>
        </Box>

        <FlashSaleCountdown endTime={endTime} />
      </Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, cursor: 'pointer', opacity: 0.75, '&:hover': { opacity: 1 } }}
        onClick={onViewAll}>
        <Typography sx={{ fontSize: { xs: '0.85rem', sm: '1rem' }, color: 'secondary.main', fontWeight: 600 }}>Xem tất cả</Typography>
        <ArrowForwardIosRoundedIcon sx={{ fontSize: { xs: 12, sm: 14 }, color: 'secondary.main' }} />
      </Box>
    </Box>
  )
}


const SCROLL_AMOUNT = 460

const ProductScrollSection = ({ products = [], isLoading, campaign, showFlashBadge = false }) => {
  const scrollRef = useRef(null)
  const endTime = campaign?.end_time ? new Date(campaign.end_time) : null

  const scroll = (dir) => {
    scrollRef.current?.scrollBy({ left: dir * SCROLL_AMOUNT, behavior: 'smooth' })
  }

  return (
    <Box
      sx={{
        background: 'rgba(255, 255, 255, 0.1)',
        backdropFilter: 'blur(10px)',
        WebkitBackdropFilter: 'blur(10px)',
        borderRadius: 3,
        boxShadow: '0 4px 30px rgba(0, 0, 0, 0.05)',
        p: 2, mb: { xs: 3.5, md: 4 }
      }}>
      <FlashSaleHeader endTime={endTime} />

      <Box sx={{ position: 'relative' }}>
        <IconButton
          onClick={() => scroll(-1)}
          sx={{
            display: { xs: 'none', md: 'flex' },
            position: 'absolute', left: -18, top: '50%', transform: 'translateY(-50%)',
            zIndex: 2, bgcolor: 'primary.main', boxShadow: '0 2px 10px rgba(0,0,0,0.14)',
            width: 34, height: 34,
            '&:hover': { bgcolor: '#f0f0f0' }
          }}
        >
          <ChevronLeftRoundedIcon sx={{ fontSize: 20 }} />
        </IconButton>

        {isLoading ? (
          <Box sx={{ display: 'flex', gap: 1.5, overflow: 'hidden' }}>
            {Array.from({ length: 5 }).map((_, i) => (
              <Box key={i} sx={{ flexShrink: 0, width: { xs: 150, sm: 185, md: 210 } }}>
                <HomeProductCardSkeleton variant="portrait" />
              </Box>
            ))}
          </Box>
        ) : (
          <Box
            ref={scrollRef}
            sx={{
              display: 'flex', gap: 1.5,
              overflowX: 'auto', pb: 0.75,
              scrollSnapType: 'x mandatory',
              '&::-webkit-scrollbar': { height: 3 },
              '&::-webkit-scrollbar-thumb': { bgcolor: 'divider', borderRadius: 4 }
            }}
          >
            {products.map((product) => (
              <Box
                key={product.spu_code || product.mongo_id || product._id}
                sx={{ flexShrink: 0, width: { xs: 150, sm: 185, md: 210 }, scrollSnapAlign: 'start' }}
              >
                <HomeProductCard product={product} variant="portrait" showFlashBadge={showFlashBadge} />
              </Box>
            ))}
            {products.length === 0 && (
              <Typography sx={{ fontSize: '0.85rem', color: '#bbb', py: 3 }}>
                Không có sản phẩm nào
              </Typography>
            )}
          </Box>
        )}

        <IconButton
          onClick={() => scroll(1)}
          sx={{
            display: { xs: 'none', md: 'flex' },
            position: 'absolute', right: -18, top: '50%', transform: 'translateY(-50%)',
            zIndex: 2, bgcolor: 'primary.main', boxShadow: '0 2px 10px rgba(0,0,0,0.14)',
            width: 34, height: 34,
            '&:hover': { bgcolor: '#f0f0f0' }
          }}
        >
          <ChevronRightRoundedIcon sx={{ fontSize: 20 }} />
        </IconButton>
      </Box>
    </Box>
  )
}

export default ProductScrollSection
