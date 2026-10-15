/* eslint-disable react-hooks/exhaustive-deps */
import AddIcon from '@mui/icons-material/Add'
import CloseIcon from '@mui/icons-material/Close'
import InventoryIcon from '@mui/icons-material/Inventory'
import SaveOutlinedIcon from '@mui/icons-material/SaveOutlined'
import { Box, Breadcrumbs, Button, Link, Paper, Typography } from '@mui/material'
import { FormProvider, useForm } from 'react-hook-form'
import { Link as RouterLink } from 'react-router-dom'
import { toast } from 'react-toastify'
import { addProductAPI, getProductDetailAPI, updateProductAPI } from '~/common/apis/services/productService'
import AdvanceTab from '../components/ProductFormPage/AdvanceTab'
import GeneralTab from '../components/ProductFormPage/GeneralTab'
import StatusCard from '../components/ProductFormPage/StatusCard'
import ThumbnailUpload from '../components/ProductFormPage/ThumbnailUpload'
import { useParams } from 'react-router-dom'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import { useQuery } from '@tanstack/react-query'
import { useEffect } from 'react'


const ProductFormPage = () => {
  const methods = useForm({ mode: 'onBlur' })
  const { handleSubmit, reset } = methods

  const { id } = useParams()
  const isUpdate = !!id
  const { data: product } = useQuery({
    queryKey: ['product', id],
    queryFn: () => getProductDetailAPI(id),
    enabled: !!id
  })
  useEffect(() => {
    if (product) {
      reset({
        spu_name: product.spu_name,
        spu_price: product.spu_price,
        spu_quantity: product.spu_quantity,
        spu_thumb: product.spu_thumb,
        spu_category: product.spu_category,
        spu_variations: product.spu_variations,
        sku_list: product.sku_list,
        spu_attributes: product.spu_attributes
      })
    }
  }, [product])

  const onSubmit = (data) => {
    const published = data.status === 'draft' ? false : true
    const payload = {
      ...data,
      isPublished: published,
      spu_shopId: '69e364dfdf24f31846f15580'
    }
    isUpdate === true
      ? toast.promise(updateProductAPI(product._id, payload), { pending: 'Updating...' })
      : toast.promise(addProductAPI(payload), { pending: 'Creating...' })
  }


  const onError = () => {
    toast.error('Please check the required fields!')
  }

  const handleCancel = () => {
    reset()
    toast.info('Changes discarded.')
  }

  return (
    <FormProvider {...methods}>
      <Box
        component="form"
        onSubmit={handleSubmit(onSubmit, onError)}
        sx={{
          minHeight: '100vh',
          background: 'linear-gradient(135deg, #f8faff 0%, #f0f4ff 50%, #f5f8ff 100%)',
          p: { xs: 2, md: 4 }
        }}
      >
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

          <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2 }}>
            <Box>
              {isUpdate ? <Typography variant="h5" fontWeight={800} letterSpacing="-0.5px" sx={{ color: 'primary.contrastText' }}>
                Update Product
              </Typography> : <Typography variant="h5" fontWeight={800} letterSpacing="-0.5px" sx={{ color: 'primary.contrastText' }}>
                Add New Product
              </Typography>}

              <Typography variant="body2" color="primary.contrastText" mt={0.25}>
                Fill in all details below to publish a new product listing.
              </Typography>
            </Box>
            <Box sx={{ display: 'flex', gap: 1.5 }}>
              <Button
                variant="outlined"
                startIcon={<CloseIcon />}
                onClick={handleCancel}
                sx={{ borderRadius: 2.5, px: 2.5, py: 1, borderColor: 'secondary.main', color: '#555', '&:hover': { borderColor: '#ccc', background: '#fafafa' } }}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                startIcon={<SaveOutlinedIcon />}
                sx={{
                  borderRadius: 2.5, px: 3, py: 1, fontWeight: '500',
                  background: 'linear-gradient(90deg, #4c83f0, #69aedc, #8acdde)',
                  color: '#ffffff'
                }}
              >
                Save
              </Button>
            </Box>
          </Box>
        </Box>

        <Box sx={{ display: 'flex', gap: 3, flexDirection: { xs: 'column', md: 'row' }, alignItems: 'flex-start' }}>
          {/* LEFT main content */}
          <Box sx={{ width: { xs: '100%', md: 300 }, flexShrink: 0 }}>
            <Paper variant="outlined" sx={{ p: 2.5, borderRadius: 3, border: '1px solid secondary.contrastText', mb: 0 }}>
              <Typography variant="subtitle1" fontWeight={700} mb={1.5}>Thumbnail</Typography>
              <ThumbnailUpload />
            </Paper>
            <StatusCard />
            <AdvanceTab />
          </Box>

          {/* RIGHT main content */}
          <Box sx={{ flex: 1, minWidth: 0, width: '100%' }}>
            <GeneralTab isUpdate={isUpdate} />
          </Box>
        </Box>

      </Box>
    </FormProvider>
  )
}

export default ProductFormPage