import AddIcon from '@mui/icons-material/Add'
import ShoppingBagRoundedIcon from '@mui/icons-material/ShoppingBagRounded'
import { Box, Button, Drawer, Typography } from '@mui/material'
import { useState } from 'react'
import AddProductModal from './AddProductModal'
import LiveProductItem from './LiveProductItem'

const LiveProductMobileDrawer = ({
  open,
  onClose,
  liveProducts = [],
  onAddProduct,
  onRemoveProduct,
  isAdding
}) => {
  const [modalOpen, setModalOpen] = useState(false)

  return (
    <>
      <Drawer
        anchor="bottom"
        open={open}
        onClose={onClose}
        PaperProps={{
          sx: {
            borderRadius: '20px 20px 0 0',
            bgcolor: '#1a1a1a',
            maxHeight: '75vh',
            display: 'flex',
            flexDirection: 'column'
          }
        }}
      >
        {/* Handle drag indicator */}
        <Box
          sx={{ pt: 1.5, pb: 0.5, display: 'flex', justifyContent: 'center', flexShrink: 0, cursor: 'pointer' }}
          onClick={onClose}
        >
          <Box sx={{
            width: 36, height: 4,
            borderRadius: 2,
            bgcolor: 'rgba(255,255,255,0.15)'
          }} />
        </Box>

        {/* Header */}
        <Box sx={{
          px: 2.5,
          pt: 0.5,
          pb: 1.5,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          borderBottom: '1px solid rgba(255,255,255,0.08)',
          flexShrink: 0
        }}>
          <Box>
            <Typography sx={{ color: '#fff', fontWeight: 700, fontSize: '0.95rem' }}>
              Sản phẩm trong phiên live
            </Typography>
            <Typography sx={{ color: '#777', fontSize: '0.72rem', mt: 0.2 }}>
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
              fontSize: '0.78rem',
              '&:hover': { bgcolor: '#1a6de0' }
            }}
          >
            Thêm
          </Button>
        </Box>

        {/* Product list */}
        <Box sx={{
          flex: 1,
          overflowY: 'auto',
          px: 2,
          py: 1.5,
          '&::-webkit-scrollbar': { display: 'none' },
          scrollbarWidth: 'none'
        }}>
          {liveProducts.length === 0 ? (
            <Box sx={{ py: 5, textAlign: 'center' }}>
              <ShoppingBagRoundedIcon sx={{
                fontSize: 40,
                color: 'rgba(255,255,255,0.08)',
                mb: 1.5,
                display: 'block',
                mx: 'auto'
              }} />
              <Typography sx={{ color: '#555', fontSize: '0.82rem' }}>
                Chưa có sản phẩm nào
              </Typography>
            </Box>
          ) : (
            liveProducts.map((product) => (
              <LiveProductItem
                key={product.productId?.toString()}
                product={product}
                onRemove={(id) => { onRemoveProduct(id) }}
              />
            ))
          )}
        </Box>
      </Drawer>

      <AddProductModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        liveProducts={liveProducts}
        onAddProduct={onAddProduct}
        isAdding={isAdding}
      />
    </>
  )
}

export default LiveProductMobileDrawer
