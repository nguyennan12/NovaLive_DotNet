import { useState, useEffect } from 'react'
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Stack,
  TextField,
  Typography
} from '@mui/material'
import AddRoundedIcon from '@mui/icons-material/AddRounded'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import DeleteRoundedIcon from '@mui/icons-material/DeleteRounded'
import DiscountRoundedIcon from '@mui/icons-material/DiscountRounded'
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

export const AdminDiscountsPage = () => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [discounts, setDiscounts] = useState([])

  const [createModalOpen, setCreateModalOpen] = useState(false)
  const [code, setCode] = useState('')
  const [discountPercent, setDiscountPercent] = useState('')
  const [maxDiscountAmount, setMaxDiscountAmount] = useState('')
  const [minOrderAmount, setMinOrderAmount] = useState('')
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [usageLimit, setUsageLimit] = useState(100)
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

    setIsSubmitting(true)
    try {
      await createAdminDiscountAPI({
        code: code.trim().toUpperCase(),
        discountPercent: Number(discountPercent) || 10,
        maxDiscountAmount: Number(maxDiscountAmount) || 50000,
        minOrderAmount: Number(minOrderAmount) || 100000,
        startDate: startDate ? new Date(startDate).toISOString() : new Date().toISOString(),
        endDate: endDate ? new Date(endDate).toISOString() : new Date(Date.now() + 30 * 86400000).toISOString(),
        usageLimit: Number(usageLimit) || 100
      })
      toast.success(`Đã tạo mã giảm giá: ${code.toUpperCase()}`)
      setCreateModalOpen(false)
      setCode('')
      loadDiscounts()
    } catch (err) {
      toast.error(err?.response?.data?.detail || 'Tạo voucher thất bại')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDelete = async (id, voucherCode) => {
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
              label: 'Mã voucher',
              render: (row) => (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <DiscountRoundedIcon color="secondary" fontSize="small" />
                  <Typography variant="body2" sx={{ fontWeight: 800, fontFamily: 'monospace' }}>
                    {row.code}
                  </Typography>
                </Box>
              )
            },
            {
              key: 'discount',
              label: 'Mức giảm',
              render: (row) => (
                <Chip
                  label={`Giảm ${row.discountPercent || row.discount_percent || 0}%`}
                  size="small"
                  color="secondary"
                  sx={{ fontWeight: 700, borderRadius: 1.5 }}
                />
              )
            },
            {
              key: 'minOrder',
              label: 'Đơn tối thiểu',
              render: (row) => <MoneyText value={row.minOrderAmount || row.min_order_amount || 0} />
            },
            {
              key: 'maxDiscount',
              label: 'Giảm tối đa',
              render: (row) => <MoneyText value={row.maxDiscountAmount || row.max_discount_amount || 0} />
            },
            {
              key: 'validity',
              label: 'Thời hạn',
              render: (row) => (
                <Typography variant="body2" color="text.secondary">
                  {row.endDate ? new Date(row.endDate).toLocaleDateString('vi-VN') : 'Vô thời hạn'}
                </Typography>
              )
            },
            {
              key: 'actions',
              label: 'Thao tác',
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
        maxWidth="xs"
        fullWidth
        slotProps={{ paper: { sx: { borderRadius: 3 } } }}
      >
        <form onSubmit={handleCreate}>
          <DialogTitle sx={{ fontWeight: 800 }}>Tạo Voucher Sàn Mới</DialogTitle>
          <DialogContent dividers>
            <Stack spacing={2} sx={{ pt: 1 }}>
              <TextField
                required
                fullWidth
                size="small"
                label="Mã giảm giá (Code)"
                placeholder="VD: NOVALIVE50K"
                value={code}
                onChange={(e) => setCode(e.target.value.toUpperCase())}
              />
              <TextField
                required
                fullWidth
                type="number"
                size="small"
                label="Tỷ lệ giảm (%)"
                placeholder="VD: 15"
                value={discountPercent}
                onChange={(e) => setDiscountPercent(e.target.value)}
              />
              <TextField
                fullWidth
                type="number"
                size="small"
                label="Giảm tối đa (VNĐ)"
                placeholder="VD: 50000"
                value={maxDiscountAmount}
                onChange={(e) => setMaxDiscountAmount(e.target.value)}
              />
              <TextField
                fullWidth
                type="number"
                size="small"
                label="Đơn hàng tối thiểu (VNĐ)"
                placeholder="VD: 150000"
                value={minOrderAmount}
                onChange={(e) => setMinOrderAmount(e.target.value)}
              />
              <TextField
                fullWidth
                size="small"
                type="datetime-local"
                label="Ngày hết hạn"
                slotProps={{ inputLabel: { shrink: true } }}
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
              />
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
              Phát hành voucher
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </>
  )
}

export default AdminDiscountsPage
