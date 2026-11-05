import {
  Alert,
  Box,
  Breadcrumbs,
  Button,
  Divider,
  FormControl,
  Grid,
  IconButton,
  MenuItem,
  Select,
  Stack,
  Switch,
  TextField,
  Tooltip,
  Typography
} from '@mui/material'
import CloseRounded from '@mui/icons-material/CloseRounded'
import SaveRounded from '@mui/icons-material/SaveRounded'
import Inventory2Outlined from '@mui/icons-material/Inventory2Outlined'
import DeleteOutlineRounded from '@mui/icons-material/DeleteOutlineRounded'
import ImageOutlined from '@mui/icons-material/ImageOutlined'
import FormatBoldRounded from '@mui/icons-material/FormatBoldRounded'
import FormatItalicRounded from '@mui/icons-material/FormatItalicRounded'
import FormatListBulletedRounded from '@mui/icons-material/FormatListBulletedRounded'
import InsertLinkRounded from '@mui/icons-material/InsertLinkRounded'
import AddRounded from '@mui/icons-material/AddRounded'
import CloudUploadOutlined from '@mui/icons-material/CloudUploadOutlined'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { resolveMediaUrl } from '~/common/utils/formatters'
import type { CreateProductRequest, CreateSkuRequest, ProductDetail } from '../../types/contracts'
import { useCategories, useCreateProduct, useSaveProductChanges } from '../../hooks/useProducts'
import { useShopPermissions } from '../../hooks/useShopPermissions'
import { useUploadMedia } from '../../hooks/useShop'
import { buildSkuMatrix, flattenCategories, productSchema } from '../../utils/product'
import { SellerCard } from '../shared/SellerCard'
import { errorMessage } from '../../utils/errors'

interface DraftSku extends CreateSkuRequest {
  id?: string
  isActive: boolean
}

export function ProductEditor({ initial }: { initial?: ProductDetail }) {
  const [form, setForm] = useState<CreateProductRequest>(
    initial
      ? {
          name: initial.name,
          description: initial.description,
          categoryId: initial.categoryId,
          brand: initial.brand,
          thumbnailUrl: initial.thumbnailUrl,
          attributesConfigJson: initial.attributesConfigJson,
          attributes: initial.attributes,
          skus: []
        }
      : {
          name: '',
          description: '',
          categoryId: '',
          brand: '',
          thumbnailUrl: '',
          attributesConfigJson: null,
          attributes: [],
          skus: []
        }
  )

  const [skus, setSkus] = useState<DraftSku[]>(
    initial
      ? initial.skus.map(sku => ({ ...sku, initialStock: 0 }))
      : [{ ...buildSkuMatrix([], 'NOVA')[0], isActive: true }]
  )

  const [axes, setAxes] = useState<{ name: string; text: string }[]>([])
  const [prefix, setPrefix] = useState('NOVA')
  const [price, setPrice] = useState(initial?.skus[0]?.sellPrice ? String(initial.skus[0].sellPrice) : '')
  const [quantity, setQuantity] = useState('')
  const [status, setStatus] = useState(initial?.status === 'Banned' ? 'Banned' : 'Published')

  // Product options
  const [taxable, setTaxable] = useState(true)
  const [freeShipping, setFreeShipping] = useState(false)
  const [returnsAllowed, setReturnsAllowed] = useState(true)
  const [featuredProduct, setFeaturedProduct] = useState(false)

  const [error, setError] = useState('')
  const [dragOver, setDragOver] = useState(false)

  const categories = useCategories()
  const create = useCreateProduct()
  const update = useSaveProductChanges()
  const upload = useUploadMedia()
  const navigate = useNavigate()
  const { canOperate } = useShopPermissions()

  const pending = create.isPending || update.isPending
  const disabled = pending || (!initial && !canOperate) || initial?.status === 'Banned'

  const changeSku = (index: number, patch: Partial<DraftSku>) => {
    setSkus(rows => rows.map((row, i) => (i === index ? { ...row, ...patch } : row)))
  }

  const handlePriceChange = (val: string) => {
    setPrice(val)
    const num = Number(val) || 0
    if (skus.length === 1) {
      changeSku(0, { sellPrice: num, originalPrice: num })
    }
  }

  const handleQuantityChange = (val: string) => {
    setQuantity(val)
    const num = Number(val) || 0
    if (skus.length === 1 && !initial) {
      changeSku(0, { initialStock: num })
    }
  }

  const addVariation = () => {
    setAxes(prev => [...prev, { name: '', text: '' }])
  }

  const removeVariation = (index: number) => {
    const next = axes.filter((_, i) => i !== index)
    setAxes(next)
    if (next.length === 0) {
      const defaultSku = buildSkuMatrix([], prefix)[0]
      const numPrice = Number(price) || 0
      const numStock = Number(quantity) || 0
      setSkus([{ ...defaultSku, sellPrice: numPrice, originalPrice: numPrice, initialStock: numStock, isActive: true }])
      setForm(f => ({ ...f, attributesConfigJson: null }))
    } else {
      generateMatrix(next)
    }
  }

  const generateMatrix = (variationAxes = axes) => {
    try {
      const normalized = variationAxes
        .filter(axis => axis.name.trim())
        .map(axis => ({
          name: axis.name.trim(),
          values: axis.text.split(',').map(v => v.trim()).filter(Boolean)
        }))

      if (normalized.length === 0) {
        return
      }

      const numPrice = Number(price) || 0
      const numStock = Number(quantity) || 0
      const generated = buildSkuMatrix(normalized, prefix).map(sku => ({
        ...sku,
        sellPrice: sku.sellPrice || numPrice,
        originalPrice: sku.originalPrice || numPrice,
        initialStock: sku.initialStock || numStock,
        isActive: true
      }))

      setSkus(generated)
      setForm(f => ({ ...f, attributesConfigJson: JSON.stringify(normalized) }))
      setError('')
    } catch (err) {
      setError(errorMessage(err))
    }
  }

  const handleFileDrop = async (e: React.DragEvent) => {
    e.preventDefault()
    setDragOver(false)
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      await uploadFile(e.dataTransfer.files[0])
    }
  }

  const handleFileInput = async (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      await uploadFile(e.target.files[0])
      e.target.value = ''
    }
  }

  const uploadFile = async (file: File) => {
    if (!file.type.startsWith('image/')) {
      setError('Vui lòng chọn tệp hình ảnh (.png, .jpg, .jpeg)')
      return
    }
    setError('')
    const localBlob = URL.createObjectURL(file)
    setForm(f => ({ ...f, thumbnailUrl: localBlob }))

    try {
      const res = await upload.mutateAsync({ file })
      const remoteUrl = res.url || localBlob
      setForm(f => ({ ...f, thumbnailUrl: remoteUrl }))
    } catch (err) {
      setError(errorMessage(err))
    }
  }

  const submit = async (event: React.FormEvent) => {
    event.preventDefault()
    setError('')

    const submitSkus = skus.map(sku => ({
      ...sku,
      sellPrice: sku.sellPrice || Number(price) || 0,
      originalPrice: sku.originalPrice || sku.sellPrice || Number(price) || 0,
      initialStock: sku.initialStock !== undefined ? sku.initialStock : Number(quantity) || 0
    }))

    const parsed = productSchema.safeParse({ ...form, skus: submitSkus })
    if (!parsed.success) {
      setError(parsed.error.issues.map(issue => `${issue.path.join('.')} · ${issue.message}`).join('\n'))
      return
    }

    try {
      if (initial) {
        const { skus: _skus, ...data } = parsed.data
        void _skus
        await update.mutateAsync({
          id: initial.id,
          data,
          skus: skus
            .filter(sku => sku.id)
            .map(sku => ({
              id: sku.id!,
              data: {
                sellPrice: sku.sellPrice,
                originalPrice: sku.originalPrice,
                weightGram: sku.weightGram,
                isActive: sku.isActive
              }
            }))
        })
      } else {
        await create.mutateAsync(parsed.data)
      }
      navigate('/seller/products')
    } catch (err) {
      setError(errorMessage(err))
    }
  }

  return (
    <Box component='form' onSubmit={event => void submit(event)}>
      {/* Top Header: Breadcrumbs, Title & Actions */}
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'flex-start',
          flexWrap: 'wrap',
          gap: 2,
          mb: 3
        }}
      >
        <Box>
          <Breadcrumbs
            aria-label='breadcrumb'
            sx={{
              fontSize: '0.85rem',
              color: 'text.secondary',
              mb: 0.5,
              '& .MuiBreadcrumbs-separator': { mx: 1 }
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <Inventory2Outlined sx={{ fontSize: 16, color: '#64748b' }} />
              <Link
                to='/seller/products'
                style={{ textDecoration: 'none', color: '#64748b', fontWeight: 500 }}
              >
                Products
              </Link>
            </Box>
            <Typography sx={{ fontSize: '0.85rem', color: '#0f172a', fontWeight: 500 }}>
              {initial ? 'Edit Product' : '+ Add New'}
            </Typography>
          </Breadcrumbs>
          <Typography variant='h5' sx={{ fontWeight: 700, color: '#0f172a' }}>
            {initial ? 'Edit Product' : 'Add New Product'}
          </Typography>
          <Typography variant='body2' sx={{ color: '#64748b', mt: 0.5 }}>
            Fill in all details below to publish a new product listing.
          </Typography>
        </Box>

        <Stack direction='row' spacing={1.5}>
          <Button
            component={Link}
            to='/seller/products'
            variant='outlined'
            startIcon={<CloseRounded />}
            disabled={pending}
            sx={{
              textTransform: 'none',
              borderRadius: '10px',
              px: 2.5,
              py: 1,
              borderColor: '#e2e8f0',
              color: '#475569',
              bgcolor: '#ffffff',
              fontWeight: 500,
              '&:hover': { bgcolor: '#f8fafc', borderColor: '#cbd5e1' }
            }}
          >
            Cancel
          </Button>
          <Button
            type='submit'
            variant='contained'
            startIcon={<SaveRounded />}
            disabled={disabled}
            sx={{
              textTransform: 'none',
              borderRadius: '10px',
              px: 3,
              py: 1,
              bgcolor: '#5b8bf7',
              color: '#ffffff',
              fontWeight: 600,
              boxShadow: '0 4px 14px rgba(91, 139, 247, 0.35)',
              '&:hover': { bgcolor: '#4a7be6' }
            }}
          >
            {pending ? 'Saving…' : 'Save'}
          </Button>
        </Stack>
      </Box>

      {error && (
        <Alert severity='error' sx={{ mb: 3, whiteSpace: 'pre-line', borderRadius: 2 }}>
          {error}
        </Alert>
      )}

      {initial?.status === 'Banned' && (
        <Alert severity='warning' sx={{ mb: 3, borderRadius: 2 }}>
          Sản phẩm đã bị khóa. Vui lòng liên hệ bộ phận hỗ trợ.
        </Alert>
      )}

      {/* 2-Column Grid Layout matching mockup */}
      <Box component='fieldset' disabled={disabled} sx={{ border: 0, p: 0, m: 0, minWidth: 0 }}>
        <Grid container spacing={3}>
          {/* Left Column (Thumbnail, Status, Product Options) */}
          <Grid size={{ xs: 12, md: 4, lg: 3.5 }}>
            <Stack spacing={3}>
              {/* Card 1: Thumbnail */}
              <SellerCard title='Thumbnail'>
                <Box
                  onDragOver={e => {
                    e.preventDefault()
                    setDragOver(true)
                  }}
                  onDragLeave={() => setDragOver(false)}
                  onDrop={handleFileDrop}
                  sx={{
                    border: '1.5px dashed',
                    borderColor: dragOver ? '#3b82f6' : '#bfdbfe',
                    bgcolor: dragOver ? '#eff6ff' : '#f8faff',
                    borderRadius: '16px',
                    p: 3,
                    textAlign: 'center',
                    cursor: 'pointer',
                    position: 'relative',
                    transition: 'all 0.2s ease-in-out',
                    '&:hover': { borderColor: '#60a5fa', bgcolor: '#eff6ff' }
                  }}
                  component='label'
                >
                  <input
                    hidden
                    type='file'
                    accept='image/png,image/jpeg,image/jpg'
                    onChange={handleFileInput}
                  />

                  {form.thumbnailUrl ? (
                    <Box sx={{ position: 'relative' }}>
                      <Box
                        component='img'
                        src={resolveMediaUrl(form.thumbnailUrl)}
                        alt='Product thumbnail'
                        sx={{
                          width: '100%',
                          height: 180,
                          objectFit: 'contain',
                          borderRadius: '12px'
                        }}
                      />
                      <Typography
                        variant='caption'
                        sx={{
                          display: 'block',
                          mt: 1,
                          color: '#3b82f6',
                          fontWeight: 500
                        }}
                      >
                        Click or drop to replace image
                      </Typography>
                    </Box>
                  ) : (
                    <Box sx={{ py: 2 }}>
                      <Box
                        sx={{
                          width: 48,
                          height: 48,
                          borderRadius: '12px',
                          bgcolor: '#eff6ff',
                          color: '#3b82f6',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          mx: 'auto',
                          mb: 1.5
                        }}
                      >
                        {upload.isPending ? (
                          <CloudUploadOutlined sx={{ fontSize: 28 }} />
                        ) : (
                          <ImageOutlined sx={{ fontSize: 28 }} />
                        )}
                      </Box>
                      <Typography sx={{ fontSize: '0.9rem', fontWeight: 600, color: '#334155', mb: 0.5 }}>
                        {upload.isPending ? 'Uploading...' : 'Click or drop image here'}
                      </Typography>
                      <Typography variant='caption' sx={{ color: '#94a3b8' }}>
                        Only *.png, *.jpg and *.jpeg accepted
                      </Typography>
                    </Box>
                  )}
                </Box>
              </SellerCard>

              {/* Card 2: Status */}
              <SellerCard
                title='Status'
                action={
                  <Box
                    sx={{
                      width: 10,
                      height: 10,
                      borderRadius: '50%',
                      bgcolor: status === 'Published' ? '#22c55e' : '#f59e0b'
                    }}
                  />
                }
              >
                <Typography variant='caption' sx={{ color: '#64748b', fontWeight: 600, display: 'block', mb: 1 }}>
                  Set Status
                </Typography>
                <FormControl fullWidth size='small'>
                  <Select
                    value={status}
                    onChange={e => setStatus(e.target.value)}
                    sx={{ borderRadius: '10px' }}
                  >
                    <MenuItem value='Published'>Published</MenuItem>
                    <MenuItem value='Draft'>Draft</MenuItem>
                    {initial?.status === 'Banned' && <MenuItem value='Banned'>Banned</MenuItem>}
                  </Select>
                </FormControl>
              </SellerCard>

              {/* Card 3: Product Options */}
              <SellerCard title='Product Options'>
                <Stack spacing={2}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Box>
                      <Typography sx={{ fontSize: '0.875rem', fontWeight: 600, color: '#1e293b' }}>
                        Taxable
                      </Typography>
                      <Typography variant='caption' sx={{ color: '#64748b' }}>
                        Apply tax to this product
                      </Typography>
                    </Box>
                    <Switch
                      checked={taxable}
                      onChange={e => setTaxable(e.target.checked)}
                      color='primary'
                    />
                  </Box>

                  <Divider sx={{ borderColor: '#f1f5f9' }} />

                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Box>
                      <Typography sx={{ fontSize: '0.875rem', fontWeight: 600, color: '#1e293b' }}>
                        Free Shipping
                      </Typography>
                      <Typography variant='caption' sx={{ color: '#64748b' }}>
                        Offer free shipping
                      </Typography>
                    </Box>
                    <Switch
                      checked={freeShipping}
                      onChange={e => setFreeShipping(e.target.checked)}
                      color='primary'
                    />
                  </Box>

                  <Divider sx={{ borderColor: '#f1f5f9' }} />

                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Box>
                      <Typography sx={{ fontSize: '0.875rem', fontWeight: 600, color: '#1e293b' }}>
                        Returns Allowed
                      </Typography>
                      <Typography variant='caption' sx={{ color: '#64748b' }}>
                        Allow product returns
                      </Typography>
                    </Box>
                    <Switch
                      checked={returnsAllowed}
                      onChange={e => setReturnsAllowed(e.target.checked)}
                      color='primary'
                    />
                  </Box>

                  <Divider sx={{ borderColor: '#f1f5f9' }} />

                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                    <Box>
                      <Typography sx={{ fontSize: '0.875rem', fontWeight: 600, color: '#1e293b' }}>
                        Featured Product
                      </Typography>
                      <Typography variant='caption' sx={{ color: '#64748b' }}>
                        Show in featured section
                      </Typography>
                    </Box>
                    <Switch
                      checked={featuredProduct}
                      onChange={e => setFeaturedProduct(e.target.checked)}
                      color='primary'
                    />
                  </Box>
                </Stack>
              </SellerCard>
            </Stack>
          </Grid>

          {/* Right Column (General, Variations, Categories & Attributes) */}
          <Grid size={{ xs: 12, md: 8, lg: 8.5 }}>
            <Stack spacing={3}>
              {/* Card 1: General */}
              <SellerCard title='General'>
                <Stack spacing={2.5}>
                  <Box>
                    <Typography
                      variant='caption'
                      sx={{ color: '#334155', fontWeight: 600, display: 'block', mb: 0.75 }}
                    >
                      Product Name <span style={{ color: '#ef4444' }}>*</span>
                    </Typography>
                    <TextField
                      fullWidth
                      size='small'
                      placeholder='Enter product name'
                      value={form.name}
                      onChange={e => setForm({ ...form, name: e.target.value })}
                      slotProps={{
                        input: { sx: { borderRadius: '10px' } }
                      }}
                      helperText='A product name is required and recommended to be unique.'
                    />
                  </Box>

                  <Grid container spacing={2}>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <Typography
                        variant='caption'
                        sx={{ color: '#334155', fontWeight: 600, display: 'block', mb: 0.75 }}
                      >
                        Price <span style={{ color: '#ef4444' }}>*</span>
                      </Typography>
                      <TextField
                        fullWidth
                        type='number'
                        size='small'
                        placeholder='0.00'
                        value={price}
                        onChange={e => handlePriceChange(e.target.value)}
                        slotProps={{
                          input: { sx: { borderRadius: '10px' } }
                        }}
                      />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                      <Typography
                        variant='caption'
                        sx={{ color: '#334155', fontWeight: 600, display: 'block', mb: 0.75 }}
                      >
                        Quantity <span style={{ color: '#ef4444' }}>*</span>
                      </Typography>
                      <TextField
                        fullWidth
                        type='number'
                        size='small'
                        placeholder='0'
                        value={quantity}
                        onChange={e => handleQuantityChange(e.target.value)}
                        slotProps={{
                          input: { sx: { borderRadius: '10px' } }
                        }}
                      />
                    </Grid>
                  </Grid>

                  <Box>
                    <Typography
                      variant='caption'
                      sx={{ color: '#334155', fontWeight: 600, display: 'block', mb: 0.75 }}
                    >
                      Description
                    </Typography>
                    {/* Mock WYSIWYG Toolbar */}
                    <Box
                      sx={{
                        border: '1px solid #e2e8f0',
                        borderBottom: 0,
                        borderRadius: '10px 10px 0 0',
                        p: 1,
                        bgcolor: '#f8fafc',
                        display: 'flex',
                        alignItems: 'center',
                        gap: 1
                      }}
                    >
                      <IconButton size='small' sx={{ p: 0.5 }}>
                        <FormatBoldRounded fontSize='small' />
                      </IconButton>
                      <IconButton size='small' sx={{ p: 0.5 }}>
                        <FormatItalicRounded fontSize='small' />
                      </IconButton>
                      <IconButton size='small' sx={{ p: 0.5 }}>
                        <FormatListBulletedRounded fontSize='small' />
                      </IconButton>
                      <IconButton size='small' sx={{ p: 0.5 }}>
                        <InsertLinkRounded fontSize='small' />
                      </IconButton>
                      <IconButton size='small' sx={{ p: 0.5 }}>
                        <ImageOutlined fontSize='small' />
                      </IconButton>
                      <Typography variant='caption' sx={{ color: '#64748b', ml: 1 }}>
                        Normal
                      </Typography>
                    </Box>
                    <TextField
                      fullWidth
                      multiline
                      minRows={5}
                      placeholder='Describe your product in detail...'
                      value={form.description}
                      onChange={e => setForm({ ...form, description: e.target.value })}
                      slotProps={{
                        input: {
                          sx: {
                            borderRadius: '0 0 10px 10px',
                            '& fieldset': { borderTopLeftRadius: 0, borderTopRightRadius: 0 }
                          }
                        }
                      }}
                      helperText='Set a description for better visibility.'
                    />
                  </Box>
                </Stack>
              </SellerCard>

              {/* Card 2: Variations */}
              <SellerCard
                title='Variations'
                action={
                  <Button
                    onClick={addVariation}
                    variant='contained'
                    size='small'
                    startIcon={<AddRounded />}
                    sx={{
                      borderRadius: '8px',
                      textTransform: 'none',
                      px: 2,
                      bgcolor: '#5b8bf7',
                      '&:hover': { bgcolor: '#4a7be6' }
                    }}
                  >
                    Add
                  </Button>
                }
              >
                <Typography variant='caption' sx={{ color: '#64748b', display: 'block', mb: 2 }}>
                  Add product variations like Color or Size
                </Typography>

                {axes.length === 0 ? (
                  <Box
                    sx={{
                      border: '1px dashed #cbd5e1',
                      borderRadius: '12px',
                      p: 4,
                      textAlign: 'center',
                      color: '#94a3b8',
                      fontSize: '0.875rem'
                    }}
                  >
                    No variation added yet. Click Add to create one.
                  </Box>
                ) : (
                  <Stack spacing={2}>
                    {axes.map((axis, index) => (
                      <Box
                        key={index}
                        sx={{
                          p: 2,
                          border: '1px solid #f1f5f9',
                          borderRadius: '12px',
                          bgcolor: '#f8fafc'
                        }}
                      >
                        <Grid container spacing={2} sx={{ alignItems: 'flex-end' }}>
                          <Grid size={{ xs: 12, sm: 5 }}>
                            <Typography
                              variant='caption'
                              sx={{ color: '#334155', fontWeight: 600, display: 'block', mb: 0.5 }}
                            >
                              Variation Name
                            </Typography>
                            <TextField
                              fullWidth
                              size='small'
                              placeholder='e.g. Color, Size'
                              value={axis.name}
                              onChange={e => {
                                const next = axes.map((item, i) =>
                                  i === index ? { ...item, name: e.target.value } : item
                                )
                                setAxes(next)
                              }}
                              slotProps={{ input: { sx: { borderRadius: '8px', bgcolor: '#fff' } } }}
                            />
                          </Grid>
                          <Grid size={{ xs: 10, sm: 6 }}>
                            <Typography
                              variant='caption'
                              sx={{ color: '#334155', fontWeight: 600, display: 'block', mb: 0.5 }}
                            >
                              Options (Press Enter or comma to add)
                            </Typography>
                            <TextField
                              fullWidth
                              size='small'
                              placeholder='e.g. Red, Blue, Green'
                              value={axis.text}
                              onChange={e => {
                                const next = axes.map((item, i) =>
                                  i === index ? { ...item, text: e.target.value } : item
                                )
                                setAxes(next)
                              }}
                              slotProps={{ input: { sx: { borderRadius: '8px', bgcolor: '#fff' } } }}
                            />
                          </Grid>
                          <Grid size={{ xs: 2, sm: 1 }} sx={{ textAlign: 'right' }}>
                            <Tooltip title='Xóa biến thể'>
                              <IconButton
                                color='error'
                                size='small'
                                onClick={() => removeVariation(index)}
                                sx={{ p: 1 }}
                              >
                                <DeleteOutlineRounded />
                              </IconButton>
                            </Tooltip>
                          </Grid>
                        </Grid>
                      </Box>
                    ))}

                    <Box sx={{ display: 'flex', gap: 1.5, justifyContent: 'flex-end', pt: 1 }}>
                      <TextField
                        size='small'
                        label='Tiền tố SKU'
                        value={prefix}
                        onChange={e => setPrefix(e.target.value)}
                        sx={{ width: 140 }}
                      />
                      <Button
                        variant='outlined'
                        size='small'
                        onClick={() => generateMatrix()}
                        sx={{ borderRadius: '8px', textTransform: 'none' }}
                      >
                        Tạo ma trận SKU
                      </Button>
                    </Box>
                  </Stack>
                )}

                {/* SKU Config Cards if multiple SKUs exist */}
                {skus.length > 1 && (
                  <Box sx={{ mt: 3, pt: 2, borderTop: '1px solid #f1f5f9' }}>
                    <Typography sx={{ fontWeight: 600, fontSize: '0.9rem', mb: 1.5 }}>
                      SKU List ({skus.length})
                    </Typography>
                    <Stack spacing={1.5}>
                      {skus.map((sku, index) => (
                        <Box
                          key={sku.id || index}
                          sx={{
                            p: 1.5,
                            border: '1px solid #e2e8f0',
                            borderRadius: '10px',
                            bgcolor: '#ffffff'
                          }}
                        >
                          <Typography sx={{ fontWeight: 600, fontSize: '0.85rem', color: '#1e293b', mb: 1 }}>
                            {Object.values(JSON.parse(sku.attributesJson || '{}') as Record<string, string>).join(' / ') ||
                              `SKU #${index + 1}`}
                          </Typography>
                          <Grid container spacing={1}>
                            <Grid size={{ xs: 12, sm: 4 }}>
                              <TextField
                                fullWidth
                                size='small'
                                label='Mã SKU'
                                value={sku.skuCode}
                                disabled={Boolean(initial)}
                                onChange={e => changeSku(index, { skuCode: e.target.value })}
                              />
                            </Grid>
                            <Grid size={{ xs: 6, sm: 3 }}>
                              <TextField
                                fullWidth
                                type='number'
                                size='small'
                                label='Giá bán (₫)'
                                value={sku.sellPrice}
                                onChange={e => changeSku(index, { sellPrice: Number(e.target.value) })}
                              />
                            </Grid>
                            <Grid size={{ xs: 6, sm: 3 }}>
                              <TextField
                                fullWidth
                                type='number'
                                size='small'
                                label='Tồn kho'
                                value={sku.initialStock}
                                disabled={Boolean(initial)}
                                onChange={e => changeSku(index, { initialStock: Number(e.target.value) })}
                              />
                            </Grid>
                            <Grid size={{ xs: 6, sm: 2 }}>
                              <TextField
                                fullWidth
                                type='number'
                                size='small'
                                label='Khối lượng (g)'
                                value={sku.weightGram}
                                onChange={e => changeSku(index, { weightGram: Number(e.target.value) })}
                              />
                            </Grid>
                          </Grid>
                        </Box>
                      ))}
                    </Stack>
                  </Box>
                )}
              </SellerCard>

              {/* Card 3: Categories & Attributes */}
              <SellerCard title='Categories & Attributes'>
                <Stack spacing={2.5}>
                  <Box>
                    <Typography
                      variant='caption'
                      sx={{ color: '#334155', fontWeight: 600, display: 'block', mb: 0.75 }}
                    >
                      Categories <span style={{ color: '#ef4444' }}>*</span>
                    </Typography>
                    <FormControl fullWidth size='small'>
                      <Select
                        displayEmpty
                        value={form.categoryId}
                        onChange={e => setForm({ ...form, categoryId: e.target.value })}
                        sx={{ borderRadius: '10px' }}
                      >
                        <MenuItem value=''>
                          <span style={{ color: '#94a3b8' }}>Add categories...</span>
                        </MenuItem>
                        {flattenCategories(categories.data || []).map(category => (
                          <MenuItem key={category.id} value={category.id}>
                            {category.name}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Box>

                  <Box>
                    <Typography
                      variant='caption'
                      sx={{ color: '#334155', fontWeight: 600, display: 'block', mb: 0.75 }}
                    >
                      Brand
                    </Typography>
                    <TextField
                      fullWidth
                      size='small'
                      placeholder='e.g. Sony, Apple, Nike'
                      value={form.brand || ''}
                      onChange={e => setForm({ ...form, brand: e.target.value || null })}
                      slotProps={{ input: { sx: { borderRadius: '10px' } } }}
                    />
                  </Box>

                  <Box>
                    <Box
                      sx={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        mb: 1
                      }}
                    >
                      <Typography variant='caption' sx={{ color: '#334155', fontWeight: 600 }}>
                        Technical Attributes
                      </Typography>
                      <Button
                        size='small'
                        onClick={() =>
                          setForm({ ...form, attributes: [...form.attributes, { name: '', value: '' }] })
                        }
                        sx={{ textTransform: 'none', fontSize: '0.8rem' }}
                      >
                        + Add attribute
                      </Button>
                    </Box>
                    <Stack spacing={1}>
                      {form.attributes.map((attr, index) => (
                        <Box key={index} sx={{ display: 'flex', gap: 1 }}>
                          <TextField
                            size='small'
                            placeholder='Tên (ví dụ: Xuất xứ)'
                            value={attr.name}
                            onChange={e =>
                              setForm({
                                ...form,
                                attributes: form.attributes.map((item, i) =>
                                  i === index ? { ...item, name: e.target.value } : item
                                )
                              })
                            }
                            sx={{ flex: 1 }}
                            slotProps={{ input: { sx: { borderRadius: '8px' } } }}
                          />
                          <TextField
                            size='small'
                            placeholder='Giá trị (ví dụ: Việt Nam)'
                            value={attr.value}
                            onChange={e =>
                              setForm({
                                ...form,
                                attributes: form.attributes.map((item, i) =>
                                  i === index ? { ...item, value: e.target.value } : item
                                )
                              })
                            }
                            sx={{ flex: 1 }}
                            slotProps={{ input: { sx: { borderRadius: '8px' } } }}
                          />
                          <IconButton
                            color='error'
                            size='small'
                            onClick={() =>
                              setForm({
                                ...form,
                                attributes: form.attributes.filter((_, i) => i !== index)
                              })
                            }
                          >
                            <DeleteOutlineRounded fontSize='small' />
                          </IconButton>
                        </Box>
                      ))}
                    </Stack>
                  </Box>
                </Stack>
              </SellerCard>
            </Stack>
          </Grid>
        </Grid>
      </Box>
    </Box>
  )
}
