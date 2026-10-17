import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { toast } from 'react-toastify'
import { addProductAPI, getProductDetailAPI, updateProductAPI } from '~/common/apis/services/productService'

export const useProductForm = (productId) => {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const isUpdate = Boolean(productId)

  const methods = useForm({
    mode: 'onBlur',
    defaultValues: {
      spu_name: '',
      spu_price: '',
      spu_quantity: '',
      spu_thumb: '',
      spu_category: '',
      spu_description: '',
      status: 'active',
      sku_list: [],
      spu_attributes: []
    }
  })

  const { reset } = methods

  // Fetch product detail when in update mode
  const { data: product, isLoading: isFetchingProduct } = useQuery({
    queryKey: ['product_detail', productId],
    queryFn: () => getProductDetailAPI(productId),
    enabled: isUpdate,
    staleTime: 60 * 1000
  })

  useEffect(() => {
    if (product) {
      reset({
        spu_name: product.spu_name || product.name || '',
        spu_price: product.spu_price ?? product.minPrice ?? '',
        spu_quantity: product.spu_quantity ?? product.totalStock ?? '',
        spu_thumb: product.spu_thumb || product.thumbnailUrl || '',
        spu_category: product.spu_category || product.categoryId || '',
        spu_description: product.spu_description || product.description || '',
        spu_variations: product.spu_variations || [],
        sku_list: product.sku_list || product.skus || [],
        spu_attributes: product.spu_attributes || product.attributes || [],
        status: product.status === 'draft' ? 'draft' : 'active'
      })
    }
  }, [product, reset])

  // Mutation for creating / updating product
  const mutation = useMutation({
    mutationFn: async (formData) => {
      const isPublished = formData.status !== 'draft'
      const payload = {
        name: formData.spu_name?.trim(),
        description: formData.spu_description?.trim() || '',
        categoryId: formData.spu_category,
        thumbnailUrl: formData.spu_thumb,
        isPublished,
        status: isPublished ? 'active' : 'draft',
        spu_name: formData.spu_name?.trim(),
        spu_description: formData.spu_description?.trim() || '',
        spu_thumb: formData.spu_thumb,
        spu_category: formData.spu_category,
        spu_price: Number(formData.spu_price || 0),
        spu_quantity: Number(formData.spu_quantity || 0),
        sku_list: formData.sku_list || [],
        spu_attributes: formData.spu_attributes || []
      }

      if (isUpdate) {
        return await updateProductAPI(product?._id || product?.id || productId, payload)
      } else {
        return await addProductAPI(payload)
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      queryClient.invalidateQueries({ queryKey: ['product_detail', productId] })
      queryClient.invalidateQueries({ queryKey: ['products_stats'] })
      toast.success(isUpdate ? 'Cập nhật sản phẩm thành công!' : 'Tạo sản phẩm mới thành công!')
      navigate('/dashboard/shop/products')
    },
    onError: (error) => {
      const message = error.response?.data?.detail || error.response?.data?.message || error.message || 'Lỗi khi lưu sản phẩm'
      toast.error(message)
    }
  })

  const onSubmit = (formData) => {
    mutation.mutate(formData)
  }

  const onError = (errors) => {
    const firstError = Object.values(errors)[0]
    toast.error(firstError?.message || 'Vui lòng kiểm tra lại các trường bắt buộc!')
  }

  const handleCancel = () => {
    reset()
    navigate('/dashboard/shop/products')
  }

  return {
    methods,
    isUpdate,
    isLoading: isFetchingProduct,
    isSubmitting: mutation.isPending,
    onSubmit,
    onError,
    handleCancel
  }
}
