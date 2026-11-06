import { useState } from 'react'
import { Box } from '@mui/material'

export default function OrderThumbnail({ src, alt = 'Ảnh sản phẩm', width = 68, height = 68, sx = {} }) {
  const [failedUrl, setFailedUrl] = useState('')
  const showImage = Boolean(src) && failedUrl !== src

  return (
    <Box
      sx={{
        width,
        height,
        flexShrink: 0,
        overflow: 'hidden',
        borderRadius: 1.5,
        border: '1px solid',
        borderColor: 'divider',
        bgcolor: '#f1f3f5',
        ...sx
      }}
    >
      {showImage && (
        <Box
          component="img"
          src={src}
          alt={alt}
          onError={() => setFailedUrl(src)}
          sx={{ width: '100%', height: '100%', display: 'block', objectFit: 'cover' }}
        />
      )}
    </Box>
  )
}
