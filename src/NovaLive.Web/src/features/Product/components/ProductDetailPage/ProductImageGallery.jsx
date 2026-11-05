import { useEffect, useState } from 'react'
import ImageOutlinedIcon from '@mui/icons-material/ImageOutlined'
import { Box, Typography } from '@mui/material'

const THUMB_COUNT = 4

const ProductImageGallery = ({ thumbUrl, productName }) => {
  const [activeIdx, setActiveIdx] = useState(0)
  const [mainThumb, setMainThumb] = useState(thumbUrl)
  const [imageFailed, setImageFailed] = useState(false)

  useEffect(() => {
    setMainThumb(thumbUrl || '')
    setActiveIdx(0)
    setImageFailed(false)
  }, [thumbUrl])

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
        {mainThumb ? (
          <Box
            component="img"
            src={mainThumb}
            onError={() => { setMainThumb(''); setImageFailed(true) }}
            alt={productName}
            sx={{ width: '100%', height: '100%', objectFit: 'contain' }}
          />
        ) : (
          <Box sx={{ width: '100%', height: '100%', bgcolor: '#eef0f3', color: '#9aa1aa', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 1 }}>
            <ImageOutlinedIcon sx={{ fontSize: 54 }} />
            <Typography variant="body2">Chưa có ảnh sản phẩm</Typography>
          </Box>
        )}
      </Box>

      <Box sx={{ display: 'flex', gap: 1, mt: 1.5, px: 0.5 }}>
        {thumbUrl && !imageFailed && Array.from({ length: THUMB_COUNT }).map((_, i) => (
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
              onError={() => setImageFailed(true)}
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
