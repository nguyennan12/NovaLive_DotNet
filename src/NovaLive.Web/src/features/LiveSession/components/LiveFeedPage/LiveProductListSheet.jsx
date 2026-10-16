import ShoppingBagRoundedIcon from '@mui/icons-material/ShoppingBagRounded'
import ShoppingCartOutlinedIcon from '@mui/icons-material/ShoppingCartOutlined'
import { Box, Typography } from '@mui/material'
import Badge from '@mui/material/Badge'
import { useNavigate } from 'react-router-dom'
import { useCart } from '~/features/Cart/hooks/useCart'
import { ProductLiveCard } from '../shared/ProductLiveCard'


const LiveProductListSheet = ({ liveShop, open, onClose, products = [] }) => {
  const navigate = useNavigate()
  const { cartCount } = useCart()
  const handleClick = (product) => {
    navigate(`/product/${product.code}`)
  }

  return (
    <>
      <Box
        onClick={onClose}
        sx={{
          position: 'absolute', inset: 0,
          bgcolor: 'rgba(0,0,0,0.45)',
          zIndex: 29,
          opacity: open ? 1 : 0,
          pointerEvents: open ? 'auto' : 'none',
          transition: 'opacity 0.28s ease'
        }}
      />

      <Box
        onWheel={(e) => e.stopPropagation()}
        onTouchStart={(e) => e.stopPropagation()}
        onTouchMove={(e) => e.stopPropagation()}
        onTouchEnd={(e) => e.stopPropagation()}
        sx={{
          background: 'linear-gradient(135deg, #f8faff 0%, #f0f4ff 50%, #f5f8ff 100%)',
          position: 'absolute', bottom: 0, left: 0, right: 0,
          height: '68%',
          borderRadius: '20px 20px 0 0',
          transform: open ? 'translateY(0)' : 'translateY(100%)',
          transition: 'transform 0.32s cubic-bezier(0.4,0,0.2,1)',
          zIndex: 30,
          display: 'flex', flexDirection: 'column',
          overflow: 'hidden'
        }}
      >
        {/* Handle */}
        <Box sx={{ pt: 1.25, pb: 0.75, display: 'flex', justifyContent: 'center', flexShrink: 0 }}>
          <Box
            onClick={onClose}
            sx={{
              width: 38, height: 4, borderRadius: 2,
              bgcolor: 'rgba(52,133,247,0.3)',
              cursor: 'pointer'
            }}
          />
        </Box>

        {/* Header */}
        <Box sx={{
          px: 2.5, pb: 1, flexShrink: 0,
          borderBottom: '1px solid rgba(52,133,247,0.12)',
          display: 'flex', justifyContent: 'space-between'
        }}>
          <Box>
            <Typography sx={{ color: '#2d2d2d', fontWeight: 700, fontSize: '0.92rem' }}>
              Sản phẩm trong live
            </Typography>
            <Typography sx={{ color: '#3485f7', fontSize: '0.72rem', fontWeight: 500 }}>
              {products.length} sản phẩm
            </Typography>
          </Box>
          <Box>
            <Badge
              badgeContent={cartCount}
              color="error"
              invisible={!cartCount}
              sx={{ '& .MuiBadge-badge': { fontSize: '0.6rem', minWidth: 16, height: 16, p: '0 3px' } }}
            >
              <Box
                onClick={() => navigate('/cart')}
                sx={{
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  minWidth: 36,
                  height: 36,
                  bgcolor: 'rgba(83,155,255,0.10)',
                  border: '1px solid rgba(83,155,255,0.25)',
                  borderRadius: '50%',
                  color: 'secondary.main',
                  cursor: 'pointer'
                }}>
                <ShoppingCartOutlinedIcon sx={{ fontSize: '1.2rem' }} />
              </Box>
            </Badge>
          </Box>
        </Box>

        {/* List — stop propagation để scroll list không trigger chuyển live */}
        <Box
          onWheel={(e) => e.stopPropagation()}
          onTouchStart={(e) => e.stopPropagation()}
          onTouchMove={(e) => e.stopPropagation()}
          onTouchEnd={(e) => e.stopPropagation()}
          sx={{
            flex: 1, overflowY: 'auto', pt: 1,
            '&::-webkit-scrollbar': { display: 'none' },
            scrollbarWidth: 'none'
          }}
        >
          {products.length === 0 ? (
            <Box sx={{ py: 5, textAlign: 'center' }}>
              <ShoppingBagRoundedIcon sx={{ fontSize: 38, color: 'rgba(52,133,247,0.2)', mb: 1 }} />
              <Typography sx={{ color: '#aaa', fontSize: '0.82rem' }}>
                Chưa có sản phẩm nào
              </Typography>
            </Box>
          ) : products.map((p, idx) => (
            <ProductLiveCard liveShop={liveShop} key={idx} product={p} onClick={handleClick} />
          ))}
        </Box>
      </Box>
    </>
  )
}

export default LiveProductListSheet
