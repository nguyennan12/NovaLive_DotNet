import { useState } from 'react'
import { Box, Skeleton } from '@mui/material'

const THUMB_COUNT = 4

const ProductImageGallery = ({ thumbUrl, productName }) => {
  const [activeIdx, setActiveIdx] = useState(0)
  const [mainThumb, setMainThumb] = useState(thumbUrl)

  if (!thumbUrl) {
    return (
      <Box>
        <Skeleton variant="rectangular" height={460} sx={{ borderRadius: '16px' }} />
        <Box sx={{ display: 'flex', gap: 1, mt: 1.5, px: 0.5 }}>
          {Array.from({ length: THUMB_COUNT }).map((_, i) => (
            <Skeleton key={i} variant="rounded" width={64} height={64} sx={{ borderRadius: '10px' }} />
          ))}
        </Box>
      </Box>
    )
  }

  return (
    <Box >
      <Box sx={{
        height: { xs: 280, sm: 380, md: 460 },
        borderRadius: '16px',
        overflow: 'hidden',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center'
      }}>
        <Box
          component="img"
          src={mainThumb}
          alt={productName}
          sx={{ width: '100%', height: '100%', objectFit: 'contain' }}
        />
      </Box>

      <Box sx={{ display: 'flex', gap: 1, mt: 1.5, px: 0.5 }}>
        {Array.from({ length: THUMB_COUNT }).map((_, i) => (
          <Box
            key={i}
            onClick={() => setActiveIdx(i)}
            sx={{
              width: 64, height: 64,
              borderRadius: '10px',
              overflow: 'hidden',
              border: activeIdx === i ? '2px solid' : '2px solid transparent',
              borderColor: activeIdx === i ? 'secondary.main' : 'transparent',
              cursor: 'pointer',
              opacity: i === 0 ? 1 : 0.35,
              transition: 'opacity 0.15s, border-color 0.15s',
              flexShrink: 0
            }}
          >
            <Box
              component="img"
              src={thumbUrl}
              onClick={() => setMainThumb(thumbUrl)}
              alt={`${productName} ${i + 1}`}
              sx={{ width: '100%', height: '100%', objectFit: 'cover' }}
            />
          </Box>
        ))}
      </Box>
    </Box>
  )
}

export default ProductImageGallery
