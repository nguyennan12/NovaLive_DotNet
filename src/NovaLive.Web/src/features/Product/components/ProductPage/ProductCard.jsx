import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import { Box, IconButton, Tooltip, Typography } from '@mui/material'
import { formatVND } from '~/common/utils/formatters'
import { getStockStatus } from '~/common/utils/formatters'
import { useNavigate } from 'react-router-dom'
import DeleteOutlineOutlinedIcon from '@mui/icons-material/DeleteOutlineOutlined'
import ArrowLeftIcon from '@mui/icons-material/ArrowLeft'
import { toast } from 'react-toastify'
import { deleteProductAPI } from '~/common/apis/services/productService'
import ConfirmDialog from '~/common/components/common/form/ConfirmDialog'
import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'


const stockConfig = {
  in: { label: 'In Stock', color: '#4ade80' },
  low: { label: 'Low', color: '#fbbf24' },
  out: { label: 'Out', color: '#f87171' }
}


const ProductCard = ({ product }) => {
  const navigate = useNavigate()
  const [openDialog, setOpenDialog] = useState(false)
  const [selectedId, setSelectedId] = useState(null)
  const stock = stockConfig[getStockStatus(product.spu_quantity)] || stockConfig['in']
  const queryClient = useQueryClient()

  const handleDelete = async () => {
    await toast.promise(deleteProductAPI(selectedId), { pending: 'Deleting...' })
    setOpenDialog(false)
    setSelectedId(null)
    await queryClient.refetchQueries({
      queryKey: ['products'],
      type: 'active'
    })
  }
  return (
    <Box
      sx={{
        bgcolor: 'primary.main',
        borderRadius: { xs: '14px', sm: '18px' },
        overflow: 'hidden',
        boxShadow: '0 1px 10px rgba(79, 79, 79, 0.28)',
        '&:hover': {
          transform: 'translateY(-2px)'
        }
      }}
    >
      {/* Image area */}
      <Box sx={{ p: { xs: 1, sm: 1.5 }, pb: { xs: 0.35, sm: 0.5 } }}>
        <Box
          sx={{
            position: 'relative',
            borderRadius: { xs: '10px', sm: '12px' },
            height: { xs: 118, sm: 160, md: 180 },
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            overflow: 'hidden'
          }}
        >
          <Box
            component="img"
            src={product.spu_thumb}
            alt={product.spu_name}
            sx={{
              width: '100%',
              height: '100%',
              objectFit: 'cover',
              borderRadius: { xs: '8px', sm: '10px' }
            }}
          />

          {/* Edit */}
          <Box
            sx={{
              position: 'absolute',
              top: { xs: 6, sm: 8 },
              right: { xs: 6, sm: 8 },
              display: 'flex',
              alignItems: 'center',
              bgcolor: 'background.paper',
              borderRadius: '50px',
              boxShadow: '0 2px 8px rgba(0,0,0,0.3)',
              overflow: 'hidden',
              width: { xs: 28, sm: 32 },
              height: { xs: 28, sm: 32 },
              transition: 'width 0.3s ease-in-out',
              '&:hover': {
                width: { xs: 84, sm: 100 }
              }
            }}
          >
            <IconButton
              size='small'
              sx={{
                width: { xs: 28, sm: 32 },
                height: { xs: 28, sm: 32 },
                flexShrink: 0,
                borderRadius: '50%',
                '&:hover': { bgcolor: '#c6ecff' }
              }}
            >
              <ArrowLeftIcon sx={{ fontSize: { xs: 17, sm: 20 }, color: 'secondary.main' }} />
            </IconButton>

            <Tooltip title='update' placement="top">
              <IconButton
                size='small'
                onClick={() => {
                  return navigate(`/products/form/${product.spu_code}`)
                }}
                sx={{
                  width: { xs: 28, sm: 32 },
                  height: { xs: 28, sm: 32 },
                  flexShrink: 0,
                  borderRadius: '50%',
                  '&:hover': { bgcolor: '#c6ecff' }
                }}
              >
                <EditOutlinedIcon sx={{ fontSize: { xs: 14, sm: 16 }, color: 'secondary.main' }} />
              </IconButton>
            </Tooltip>
            <Tooltip title='Delete' placement="top">
              <IconButton
                size='small'
                onClick={() => {
                  setSelectedId(product.mongo_id)
                  setOpenDialog(true)
                }}
                sx={{
                  width: { xs: 28, sm: 32 },
                  height: { xs: 28, sm: 32 },
                  flexShrink: 0,
                  borderRadius: '50%',
                  '&:hover': { bgcolor: '#ffcdd2' }
                }}
              >
                <DeleteOutlineOutlinedIcon sx={{ fontSize: { xs: 14, sm: 16 }, color: 'error.main' }} />
              </IconButton>
            </Tooltip>
          </Box>
        </Box>
      </Box>

      {/* Info */}
      <Box sx={{ px: { xs: 1.25, sm: 2 }, pt: { xs: 0.9, sm: 1.25 }, pb: { xs: 1.1, sm: 1.75 } }}>
        <Typography
          noWrap
          sx={{ fontSize: { xs: '0.82rem', sm: '0.95rem' }, fontWeight: 700, color: 'primary.contractText', letterSpacing: '-0.02em' }}
        >
          {product.spu_name}
        </Typography>
        {/* <Typography sx={{ fontSize: '0.75rem', color: '#777', mt: 0.25 }}>
          {product.brand || product.category}
        </Typography> */}

        {/* Price + Stock pill */}
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: { xs: 1, sm: 1.5 } }}>
          <Typography sx={{ fontSize: { xs: '1.02rem', sm: '1.35rem' }, fontWeight: 800, color: 'primary.contractText', letterSpacing: '-0.03em', lineHeight: 1 }}>
            {formatVND(product.spu_price)}
          </Typography>

          <Box
            sx={{
              bgcolor: '#f4f4f4',
              borderRadius: { xs: '8px', sm: '10px' },
              px: { xs: 0.8, sm: 1.25 },
              py: { xs: 0.5, sm: 0.75 },
              display: 'flex',
              alignItems: 'center',
              gap: { xs: 0.45, sm: 0.6 },
              minWidth: { xs: 46, sm: 54 }
            }}
          >
            <Box
              sx={{
                width: { xs: 5, sm: 6 },
                height: { xs: 5, sm: 6 },
                borderRadius: '50%',
                bgcolor: stock.color,
                flexShrink: 0,
                boxShadow: `0 0 5px ${stock.color}`
              }}
            />
            <Typography sx={{ fontSize: { xs: '0.55rem', sm: '0.6rem' }, fontWeight: 700, color: '#333', lineHeight: 1, whiteSpace: 'nowrap' }}>
              {stock.label}
            </Typography>
          </Box>
        </Box>

      </Box>

      <ConfirmDialog
        open={openDialog}
        title="Xác nhận xóa sản phẩm"
        content="Bạn có chắc chắn muốn xóa sản phẩm này không? Hành động này không thể hoàn tác."
        confirmText="Xóa sản phẩm"
        onConfirm={handleDelete}
        onClose={() => setOpenDialog(false)}
      />
    </Box >
  )
}

export default ProductCard