import styled from '@emotion/styled'
import {
  Box
} from '@mui/material'

export const WaterDropBackground = styled(Box)({
  maxWidth: 1440,
  margin: '0 auto',
  minHeight: '100vh',
  background: 'linear-gradient(135deg, #f8faff 0%, #f0f4ff 50%, #f5f8ff 100%)',
  position: 'relative',
  overflow: 'hidden',
  '&::before': {
    content: '""',
    position: 'absolute',
    top: 0, left: 0, width: '100%', height: '100%',
    backgroundImage: 'url(https://res.cloudinary.com/nguyennan12/image/upload/v1777444709/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/rigceoxnr4fnqcs1bkpd.png)',
    backgroundSize: 'cover',
    opacity: 0.6,
    zIndex: 0
  },
  '& > *': { position: 'relative', zIndex: 1 }
})