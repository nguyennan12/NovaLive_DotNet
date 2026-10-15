import AddIcon from '@mui/icons-material/Add'
import ShoppingBagRoundedIcon from '@mui/icons-material/ShoppingBagRounded'
import { Box, Button, Typography } from '@mui/material'
import { useState } from 'react'
import AddProductModal from './AddProductModal'
import LiveProductItem from './LiveProductItem'

const LiveProductPanel = ({ liveProducts = [], onAddProduct, onRemoveProduct, isAdding }) => {
  const [modalOpen, setModalOpen] = useState(false)

  return (
    <Box sx={{
      width: '100%',
      height: '100vh',
      display: 'flex',
      flexDirection: 'column',
      bgcolor: '#161616',
      borderLeft: '1px solid rgba(255,255,255,0.06)'
    }}>
      {/* Header */}
      <Box sx={{
        px: 2.5,
        pt: 3,
        pb: 2,
        borderBottom: '1px solid rgba(255,255,255,0.06)',
        display: 'flex',
        alignItems: 'flex-start',
        justifyContent: 'space-between',
        gap: 1,
        flexShrink: 0
      }}>
        <Box>
          <Typography sx={{ color: '#fff', fontWeight: 700, fontSize: '0.95rem', lineHeight: 1.3 }}>
            Sản phẩm trong phiên live
          </Typography>
          <Typography sx={{ color: '#777', fontSize: '0.72rem', mt: 0.3 }}>
            {liveProducts.length} sản phẩm
          </Typography>
        </Box>

        <Button
          variant="contained"
          size="small"
          startIcon={<AddIcon sx={{ fontSize: '16px !important' }} />}
          onClick={() => setModalOpen(true)}
          sx={{
            bgcolor: '#3485f7',
            borderRadius: 2,
            textTransform: 'none',
            fontWeight: 600,
            color: '#fff',
            fontSize: '0.78rem',
            whiteSpace: 'nowrap',
            flexShrink: 0,
            '&:hover': { bgcolor: '#1a6de0' }
          }}
        >
          Thêm sản phẩm
        </Button>
      </Box>

      {/* Product list */}
      <Box sx={{
        flex: 1,
        overflowY: 'auto',
        px: 2,
        py: 1.5,
        '&::-webkit-scrollbar': { width: 4 },
        '&::-webkit-scrollbar-track': { bgcolor: 'transparent' },
        '&::-webkit-scrollbar-thumb': {
          bgcolor: 'rgba(255,255,255,0.1)',
          borderRadius: 2,
          '&:hover': { bgcolor: 'rgba(255,255,255,0.2)' }
        }
      }}>
        {liveProducts.length === 0 ? (
          <Box sx={{ py: 8, textAlign: 'center' }}>
            <ShoppingBagRoundedIcon sx={{
              fontSize: 48,
              color: 'rgba(255,255,255,0.08)',
              mb: 1.5,
              display: 'block',
              mx: 'auto'
            }} />
            <Typography sx={{ color: '#555', fontSize: '0.85rem' }}>
              Chưa có sản phẩm nào
            </Typography>
            <Typography sx={{ color: '#444', fontSize: '0.75rem', mt: 0.5 }}>
              Bấm &quot;Thêm sản phẩm&quot; để bắt đầu
            </Typography>
          </Box>
        ) : (
          liveProducts.map((product) => (
            <LiveProductItem
              key={product.productId?.toString()}
              product={product}
              onRemove={onRemoveProduct}
            />
          ))
        )}
      </Box>

      <AddProductModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        liveProducts={liveProducts}
        onAddProduct={onAddProduct}
        isAdding={isAdding}
      />
    </Box>
  )
}

export default LiveProductPanel
