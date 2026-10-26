import AddIcon from '@mui/icons-material/Add'
import CloseIcon from '@mui/icons-material/Close'
import InventoryIcon from '@mui/icons-material/Inventory'
import SaveOutlinedIcon from '@mui/icons-material/SaveOutlined'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import {
  Box,
  Breadcrumbs,
  Button,
  CircularProgress,
  Link,
  Paper,
  Tooltip,
  Typography
} from '@mui/material'
import { FormProvider } from 'react-hook-form'
import { Link as RouterLink, useParams } from 'react-router-dom'
import { useProductForm } from '../hooks/useProductForm'
import AdvanceTab from '../components/ProductFormPage/AdvanceTab'
import GeneralTab from '../components/ProductFormPage/GeneralTab'
import StatusCard from '../components/ProductFormPage/StatusCard'
import ThumbnailUpload from '../components/ProductFormPage/ThumbnailUpload'

const ProductFormPage = () => {
  const { id } = useParams()
  const {
    methods,
    isUpdate,
    isLoading,
    isSubmitting,
    onSubmit,
    onError,
    handleCancel
  } = useProductForm(id)

  if (isLoading) {
    return (
      <Box
        sx={{
          minHeight: '60vh',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          gap: 2
        }}
      >
        <CircularProgress size={36} sx={{ color: 'secondary.main' }} />
        <Typography variant="body2" sx={{ color: 'text.secondary' }}>
          Đang tải thông tin sản phẩm...
        </Typography>
      </Box>
    )
  }

  return (
    <FormProvider {...methods}>
      <Box
        component="form"
        onSubmit={methods.handleSubmit(onSubmit, onError)}
        sx={{
          minHeight: '100vh',
          background: 'linear-gradient(135deg, #f8faff 0%, #f0f4ff 50%, #f5f8ff 100%)',
          p: { xs: 2, md: 4 }
        }}
      >
        {/* Header Breadcrumbs & Action Buttons */}
        <Box sx={{ mb: 3 }}>
          <Breadcrumbs sx={{ mb: 1.5 }}>
            <Link
              component={RouterLink}
              to="/dashboard/shop/products"
              underline="hover"
              sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 0.5,
                fontSize: '0.82rem',
                color: 'primary.contrastText'
              }}
            >
              <InventoryIcon sx={{ fontSize: 14 }} /> Products
            </Link>

            {isUpdate ? (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, fontWeight: 600 }}>
                <EditOutlinedIcon sx={{ fontSize: 14 }} />
                Update
              </Box>
            ) : (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, fontWeight: 600 }}>
                <AddIcon sx={{ fontSize: 14 }} />
                Add New
              </Box>
            )}
          </Breadcrumbs>

          <Box
            sx={{
              display: 'flex',
              alignItems: 'flex-start',
              justifyContent: 'space-between',
              flexWrap: 'wrap',
              gap: 2
            }}
          >
            <Box>
              <Typography
                variant="h5"
                fontWeight={800}
                letterSpacing="-0.5px"
                sx={{ color: 'primary.contrastText' }}
              >
                {isUpdate ? 'Update Product' : 'Add New Product'}
              </Typography>

              <Typography variant="body2" color="text.secondary" mt={0.25}>
                {isUpdate
                  ? 'Chỉnh sửa thông tin chi tiết và biến thể của sản phẩm.'
                  : 'Điền đầy đủ thông tin bên dưới để đăng tải sản phẩm mới lên gian hàng.'}
              </Typography>
            </Box>

            <Box sx={{ display: 'flex', gap: 1.5 }}>
              <Tooltip title="Hủy các thay đổi và quay lại danh sách">
                <Button
                  variant="outlined"
                  startIcon={<CloseIcon />}
                  onClick={handleCancel}
                  disabled={isSubmitting}
                  sx={{
                    borderRadius: 2.5,
                    px: 2.5,
                    py: 1,
                    borderColor: 'divider',
                    color: 'text.secondary',
                    '&:hover': { borderColor: '#ccc', background: '#fafafa' }
                  }}
                >
                  Cancel
                </Button>
              </Tooltip>

              <Tooltip title={isUpdate ? 'Lưu cập nhật sản phẩm' : 'Lưu và xuất bản sản phẩm'}>
                <span>
                  <Button
                    type="submit"
                    variant="contained"
                    disabled={isSubmitting}
                    startIcon={
                      isSubmitting ? (
                        <CircularProgress size={18} color="inherit" />
                      ) : (
                        <SaveOutlinedIcon />
                      )
                    }
                    sx={{
                      borderRadius: 2.5,
                      px: 3,
                      py: 1,
                      fontWeight: 600,
                      background: 'linear-gradient(90deg, #4c83f0, #69aedc, #8acdde)',
                      color: '#ffffff',
                      '&:hover': {
                        filter: 'brightness(1.05)'
                      }
                    }}
                  >
                    {isSubmitting ? 'Saving...' : 'Save Product'}
                  </Button>
                </span>
              </Tooltip>
            </Box>
          </Box>
        </Box>

        {/* Form Body: Left sidebar + Right main content */}
        <Box
          sx={{
            display: 'flex',
            gap: 3,
            flexDirection: { xs: 'column', md: 'row' },
            alignItems: 'flex-start'
          }}
        >
          {/* LEFT: Thumbnail, Status, Specs */}
          <Box sx={{ width: { xs: '100%', md: 300 }, flexShrink: 0 }}>
            <Paper
              variant="outlined"
              sx={{
                p: 2.5,
                borderRadius: 3,
                border: '1px solid',
                borderColor: 'divider',
                mb: 2,
                bgcolor: 'background.paper'
              }}
            >
              <Typography variant="subtitle2" fontWeight={700} mb={1.5}>
                Thumbnail
              </Typography>
              <ThumbnailUpload />
            </Paper>
            <StatusCard />
            <AdvanceTab />
          </Box>

          {/* RIGHT: General details & SKU variations */}
          <Box sx={{ flex: 1, minWidth: 0, width: '100%' }}>
            <GeneralTab isUpdate={isUpdate} />
          </Box>
        </Box>
      </Box>
    </FormProvider>
  )
}

export default ProductFormPage