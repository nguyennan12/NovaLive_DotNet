import { useState, useEffect } from 'react'
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  IconButton,
  Stack,
  TextField,
  ToggleButton,
  ToggleButtonGroup,
  Typography
} from '@mui/material'
import AddRoundedIcon from '@mui/icons-material/AddRounded'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import DeleteRoundedIcon from '@mui/icons-material/DeleteRounded'
import DiscountRoundedIcon from '@mui/icons-material/DiscountRounded'
import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import PercentRoundedIcon from '@mui/icons-material/PercentRounded'
import AttachMoneyRoundedIcon from '@mui/icons-material/AttachMoneyRounded'
import { toast } from 'react-toastify'
import {
  getAdminDiscountsAPI,
  createAdminDiscountAPI,
  deleteAdminDiscountAPI
} from '~/common/apis/services/adminService'
import { PageHeader } from '~/features/Seller/components/shared/PageHeader'
import { SellerCard } from '~/features/Seller/components/shared/SellerCard'
import { DataTable } from '~/features/Seller/components/shared/DataTable'
import { MoneyText } from '~/features/Seller/components/shared/MoneyText'

const formatDiscountValue = (discount) => {
  const type = String(discount.discountType ?? discount.discount_type ?? '')
  const value = Number(discount.discountValue ?? discount.discount_value ?? 0)
  const money = `${new Intl.NumberFormat('vi-VN').format(value)} ₫`

  switch (type) {
    case 'PercentCart': return `Giảm ${value}%`
    case 'FixedCart': return `Giảm ${money}`
    case 'PercentShip': return `Giảm phí ship ${value}%`
    case 'FreeShip': return 'Miễn phí vận chuyển'
    default: return `Giảm ${money}`
  }
}

export const AdminDiscountsPage = () => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [discounts, setDiscounts] = useState([])

  // Modal State
  const [createModalOpen, setCreateModalOpen] = useState(false)
  const [category, setCategory] = useState('product') // 'product' | 'shipping'
  const [productType, setProductType] = useState('PercentCart') // 'PercentCart' | 'FixedCart'
  const [shippingType, setShippingType] = useState('FreeShip') // 'FreeShip' | 'PercentShip'

  const [code, setCode] = useState('')
  const [name, setName] = useState('')
  const [discountValue, setDiscountValue] = useState('15')
  const [maxDiscountAmount, setMaxDiscountAmount] = useState('50000')
  const [minOrderAmount, setMinOrderAmount] = useState('0')
  const [startDate, setStartDate] = useState(new Date().toISOString().slice(0, 10))
  const [endDate, setEndDate] = useState(new Date(Date.now() + 30 * 86400000).toISOString().slice(0, 10))
  const [maxUses, setMaxUses] = useState('300')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const loadDiscounts = async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await getAdminDiscountsAPI()
      const list = res?.items || (Array.isArray(res) ? res : res?.data || [])
      setDiscounts(list)
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadDiscounts()
  }, [])

  const handleCreate = async (e) => {
    e.preventDefault()
    if (!code.trim()) {
      toast.error('Vui lòng nhập mã giảm giá')
      return
    }

    const discountType = category === 'shipping' ? shippingType : productType
    let numValue = Number(discountValue)
    if (discountType === 'FreeShip') {
      numValue = 1
    } else if (isNaN(numValue) || numValue <= 0) {
      toast.error('Giá trị giảm giá phải lớn hơn 0')
      return
    }

    let numMaxDiscount = null
    if (discountType === 'PercentCart' || discountType === 'PercentShip') {
      if (numValue > 100) {
        toast.error('Tỷ lệ giảm phần trăm không được vượt quá 100%')
        return
      }
      numMaxDiscount = Number(maxDiscountAmount)
      if (!numMaxDiscount || numMaxDiscount <= 0) {
        toast.error('Voucher giảm theo % bắt buộc phải có mức giảm tối đa lớn hơn 0')
        return
      }
    }

    const payload = {
      code: code.trim().toUpperCase(),
      name: name.trim() || (category === 'shipping' ? `Miễn phí vận chuyển ${code.trim().toUpperCase()}` : `Ưu đãi ${code.trim().toUpperCase()}`),
      discountType,
      discountValue: numValue,
      minOrderAmount: Number(minOrderAmount) || 0,
      maxDiscountAmount: numMaxDiscount,
      maxUses: maxUses ? Number(maxUses) : null,
      perUserLimit: 1,
      appliesTo: 'AllProducts',
      targetIds: null,
      validFrom: startDate ? new Date(`${startDate}T00:00:00.000Z`).toISOString() : new Date().toISOString(),
      validTo: endDate ? new Date(`${endDate}T23:59:59.999Z`).toISOString() : new Date(Date.now() + 30 * 86400000).toISOString(),
      isPublic: true
    }

    setIsSubmitting(true)
    try {
      await createAdminDiscountAPI(payload)
      toast.success(`Đã phát hành voucher sàn: ${code.toUpperCase()}`)
      setCreateModalOpen(false)
      setCode('')
      setName('')
      loadDiscounts()
    } catch (err) {
      toast.error(err?.response?.data?.detail || err?.response?.data?.message || 'Tạo voucher thất bại')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDelete = async (id, voucherCode) => {
    if (!window.confirm(`Bạn có chắc chắn muốn xóa voucher ${voucherCode}?`)) return
    try {
      await deleteAdminDiscountAPI(id)
      toast.success(`Đã xóa voucher ${voucherCode}`)
      loadDiscounts()
    } catch (err) {
      toast.error(err?.response?.data?.detail || 'Xóa voucher thất bại')
    }
  }

  return (
    <>
      <PageHeader
        title="Mã giảm giá toàn sàn"
        description="Phát hành và quản lý các voucher khuyến mãi áp dụng trên toàn hệ thống NovaLive."
        action={
          <Stack direction="row" spacing={1}>
            <Button
              variant="outlined"
              size="small"
              startIcon={<RefreshRoundedIcon fontSize="small" />}
              onClick={loadDiscounts}
              disabled={loading}
              sx={{
                borderColor: 'divider',
                color: 'text.secondary',
                textTransform: 'none',
                borderRadius: 2,
                '&:hover': { bgcolor: 'rgba(0,0,0,0.04)' }
              }}
            >
              Làm mới
            </Button>
            <Button
              variant="contained"
              size="small"
              color="secondary"
              startIcon={<AddRoundedIcon fontSize="small" />}
              onClick={() => setCreateModalOpen(true)}
              sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
            >
              Tạo voucher sàn
            </Button>
          </Stack>
        }
      />

      <SellerCard>
        <DataTable
          rows={discounts}
          rowKey={(row) => row.id}
          loading={loading}
          error={error}
          onRetry={loadDiscounts}
          emptyText="Chưa có mã giảm giá toàn sàn nào."
          columns={[
            {
              key: 'code',
              label: 'MÃ VOUCHER',
              render: (row) => (
                <Stack direction="row" alignItems="center" spacing={1}>
                  <DiscountRoundedIcon fontSize="small" color="secondary" />
                  <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 800, color: 'text.primary' }}>
                    {row.code}
                  </Typography>
                </Stack>
              )
            },
            {
              key: 'name',
              label: 'TÊN CHƯƠNG TRÌNH',
              render: (row) => (
                <Typography variant="body2" sx={{ fontWeight: 600 }}>
                  {row.name || '—'}
                </Typography>
              )
            },
            {
              key: 'discountValue',
              label: 'MỨC GIẢM',
              render: (row) => (
                <Chip
                  label={formatDiscountValue(row)}
                  size="small"
                  sx={{
                    bgcolor: String(row.discountType).includes('Ship') ? '#fef3c7' : '#e0f2fe',
                    color: String(row.discountType).includes('Ship') ? '#b45309' : '#0284c7',
                    fontWeight: 700,
                    borderRadius: 1.5
                  }}
                />
              )
            },
            {
              key: 'minOrderAmount',
              label: 'ĐƠN TỐI THIỂU',
              render: (row) => <MoneyText value={row.minOrderAmount ?? row.min_order_amount ?? 0} />
            },
            {
              key: 'maxDiscountAmount',
              label: 'GIẢM TỐI ĐA',
              render: (row) =>
                row.maxDiscountAmount || row.max_discount_amount ? (
                  <MoneyText value={row.maxDiscountAmount || row.max_discount_amount} />
                ) : (
                  <Typography variant="body2" color="text.secondary">
                    Không giới hạn
                  </Typography>
                )
            },
            {
              key: 'validity',
              label: 'THỜI HẠN',
              render: (row) => (
                <Typography variant="body2" color="text.secondary">
                  {row.validTo || row.valid_to
                    ? new Date(row.validTo || row.valid_to).toLocaleDateString('vi-VN')
                    : 'Vô thời hạn'}
                </Typography>
              )
            },
            {
              key: 'actions',
              label: 'THAO TÁC',
              align: 'right',
              render: (row) => (
                <IconButton size="small" color="error" onClick={() => handleDelete(row.id, row.code)}>
                  <DeleteRoundedIcon fontSize="small" />
                </IconButton>
              )
            }
          ]}
        />
      </SellerCard>

      {/* Create Modal */}
      <Dialog
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        maxWidth="sm"
        fullWidth
        slotProps={{ paper: { sx: { borderRadius: 3 } } }}
      >
        <form onSubmit={handleCreate}>
          <DialogTitle sx={{ fontWeight: 800 }}>Tạo Voucher Sàn Mới</DialogTitle>
          <DialogContent dividers>
            <Stack spacing={2.5} sx={{ pt: 1 }}>
              {/* Category Selector: Product vs Shipping */}
              <Box>
                <Typography sx={{ fontSize: '0.75rem', fontWeight: 700, color: 'text.secondary', mb: 1 }}>
                  PHÂN LOẠI VOUCHER (CATEGORY)
                </Typography>
                <ToggleButtonGroup
                  exclusive
                  fullWidth
                  size="small"
                  value={category}
                  onChange={(_, val) => val && setCategory(val)}
                  sx={{
                    '& .MuiToggleButton-root': {
                      py: 1,
                      fontWeight: 700,
                      textTransform: 'none',
                      gap: 1,
                      '&.Mui-selected': {
                        bgcolor: category === 'shipping' ? '#fffbeb' : '#eff6ff',
                        color: category === 'shipping' ? '#b45309' : '#2563eb',
                        borderColor: category === 'shipping' ? '#f59e0b' : '#3b82f6'
                      }
                    }
                  }}
                >
                  <ToggleButton value="product">
                    <LocalOfferRoundedIcon sx={{ fontSize: 18 }} /> Voucher Sản phẩm / Đơn hàng
                  </ToggleButton>
                  <ToggleButton value="shipping">
                    <LocalShippingRoundedIcon sx={{ fontSize: 18 }} /> Voucher Vận chuyển (Freeship)
                  </ToggleButton>
                </ToggleButtonGroup>
              </Box>

              {/* Sub-type selector based on Category */}
              {category === 'product' ? (
                <Box>
                  <Typography sx={{ fontSize: '0.75rem', fontWeight: 700, color: 'text.secondary', mb: 1 }}>
                    HÌNH THỨC GIẢM GIÁ
                  </Typography>
                  <ToggleButtonGroup
                    exclusive
                    fullWidth
                    size="small"
                    value={productType}
                    onChange={(_, val) => val && setProductType(val)}
                    sx={{
                      '& .MuiToggleButton-root': {
                        py: 0.8,
                        fontWeight: 600,
                        textTransform: 'none',
                        gap: 1
                      }
                    }}
                  >
                    <ToggleButton value="PercentCart">
                      <PercentRoundedIcon sx={{ fontSize: 16 }} /> Giảm theo phần trăm (%)
                    </ToggleButton>
                    <ToggleButton value="FixedCart">
                      <AttachMoneyRoundedIcon sx={{ fontSize: 16 }} /> Giảm số tiền cố định (₫)
                    </ToggleButton>
                  </ToggleButtonGroup>
                </Box>
              ) : (
                <Box>
                  <Typography sx={{ fontSize: '0.75rem', fontWeight: 700, color: 'text.secondary', mb: 1 }}>
                    HÌNH THỨC MIỄN GIẢM VẬN CHUYỂN
                  </Typography>
                  <ToggleButtonGroup
                    exclusive
                    fullWidth
                    size="small"
                    value={shippingType}
                    onChange={(_, val) => val && setShippingType(val)}
                    sx={{
                      '& .MuiToggleButton-root': {
                        py: 0.8,
                        fontWeight: 600,
                        textTransform: 'none',
                        gap: 1
                      }
                    }}
                  >
                    <ToggleButton value="FreeShip">
                      <LocalShippingRoundedIcon sx={{ fontSize: 16 }} /> Miễn phí vận chuyển 100% (FreeShip)
                    </ToggleButton>
                    <ToggleButton value="PercentShip">
                      <PercentRoundedIcon sx={{ fontSize: 16 }} /> Giảm phí ship theo %
                    </ToggleButton>
                  </ToggleButtonGroup>
                </Box>
              )}

              <Divider sx={{ borderStyle: 'dashed' }} />

              {/* Basic Info */}
              <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
                <TextField
                  required
                  fullWidth
                  size="small"
                  label="Mã giảm giá (Code) *"
                  placeholder="VD: DEMO-FREESHIP"
                  value={code}
                  onChange={(e) => setCode(e.target.value.toUpperCase())}
                  slotProps={{
                    htmlInput: { style: { fontFamily: 'monospace', fontWeight: 700, letterSpacing: '0.05em' } }
                  }}
                />
                <TextField
                  required
                  fullWidth
                  size="small"
                  label="Tên chương trình *"
                  placeholder="VD: Miễn phí vận chuyển toàn sàn"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                />
              </Box>

              {/* Value Fields */}
              <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
                {category === 'shipping' && shippingType === 'FreeShip' ? (
                  <TextField
                    disabled
                    fullWidth
                    size="small"
                    label="Mức giảm"
                    value="Miễn phí 100% cước vận chuyển"
                  />
                ) : (
                  <TextField
                    required
                    fullWidth
                    type="number"
                    size="small"
                    label={
                      (category === 'product' && productType === 'PercentCart') ||
                      (category === 'shipping' && shippingType === 'PercentShip')
                        ? 'Tỷ lệ giảm (%) *'
                        : 'Số tiền giảm (₫) *'
                    }
                    placeholder={
                      (category === 'product' && productType === 'PercentCart') ||
                      (category === 'shipping' && shippingType === 'PercentShip')
                        ? 'VD: 20'
                        : 'VD: 50000'
                    }
                    value={discountValue}
                    onChange={(e) => setDiscountValue(e.target.value)}
                  />
                )}

                {((category === 'product' && productType === 'PercentCart') ||
                  (category === 'shipping' && shippingType === 'PercentShip')) && (
                  <TextField
                    required
                    fullWidth
                    type="number"
                    size="small"
                    label="Giảm tối đa (₫) *"
                    placeholder="VD: 50000"
                    value={maxDiscountAmount}
                    onChange={(e) => setMaxDiscountAmount(e.target.value)}
                  />
                )}
              </Box>

              {/* Conditions */}
              <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
                <TextField
                  fullWidth
                  type="number"
                  size="small"
                  label="Đơn hàng tối thiểu (₫)"
                  placeholder="0"
                  value={minOrderAmount}
                  onChange={(e) => setMinOrderAmount(e.target.value)}
                />
                <TextField
                  fullWidth
                  type="number"
                  size="small"
                  label="Số lượt dùng tối đa"
                  placeholder="300"
                  value={maxUses}
                  onChange={(e) => setMaxUses(e.target.value)}
                />
              </Box>

              {/* Dates */}
              <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
                <TextField
                  fullWidth
                  size="small"
                  type="date"
                  label="Ngày bắt đầu"
                  slotProps={{ inputLabel: { shrink: true } }}
                  value={startDate}
                  onChange={(e) => setStartDate(e.target.value)}
                />
                <TextField
                  fullWidth
                  size="small"
                  type="date"
                  label="Ngày kết thúc"
                  slotProps={{ inputLabel: { shrink: true } }}
                  value={endDate}
                  onChange={(e) => setEndDate(e.target.value)}
                />
              </Box>
            </Stack>
          </DialogContent>
          <DialogActions sx={{ p: 2 }}>
            <Button onClick={() => setCreateModalOpen(false)} color="inherit" sx={{ textTransform: 'none' }}>
              Hủy
            </Button>
            <Button
              type="submit"
              variant="contained"
              color="secondary"
              disabled={isSubmitting || !code.trim()}
              sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
            >
              Phát hành voucher sàn
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </>
  )
}

export default AdminDiscountsPage
