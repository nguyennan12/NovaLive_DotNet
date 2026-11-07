import { useState, useEffect } from 'react'
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
  Typography
} from '@mui/material'
import AddRoundedIcon from '@mui/icons-material/AddRounded'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import FlashOnRoundedIcon from '@mui/icons-material/FlashOnRounded'
import { toast } from 'react-toastify'
import {
  getAdminFlashSalesAPI,
  createAdminFlashSaleAPI
} from '~/common/apis/services/adminService'
import { PageHeader } from '~/features/Seller/components/shared/PageHeader'
import { SellerCard } from '~/features/Seller/components/shared/SellerCard'
import { DataTable } from '~/features/Seller/components/shared/DataTable'

export const AdminFlashSalesPage = () => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [campaigns, setCampaigns] = useState([])

  const [createModalOpen, setCreateModalOpen] = useState(false)
  const [name, setName] = useState('')
  const [startTime, setStartTime] = useState('')
  const [endTime, setEndTime] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const loadFlashSales = async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await getAdminFlashSalesAPI()
      const list = Array.isArray(res) ? res : res?.items || res?.data || []
      setCampaigns(list)
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadFlashSales()
  }, [])

  const handleCreate = async (e) => {
    e.preventDefault()
    if (!name.trim() || !startTime || !endTime) {
      toast.error('Vui lòng điền đầy đủ thông tin chiến dịch')
      return
    }

    setIsSubmitting(true)
    try {
      await createAdminFlashSaleAPI({
        name: name.trim(),
        startAt: new Date(startTime).toISOString(),
        endAt: new Date(endTime).toISOString(),
        bannerUrl: null
      })
      toast.success('Đã tạo khung giờ Flash Sale mới!')
      setCreateModalOpen(false)
      setName('')
      setStartTime('')
      setEndTime('')
      loadFlashSales()
    } catch (err) {
      toast.error(err?.response?.data?.detail || 'Tạo Flash Sale thất bại')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <>
      <PageHeader
        title="Chiến dịch Flash Sale sàn"
        description="Tạo và kiểm duyệt các khung giờ săn deal Flash Sale toàn sàn."
        action={
          <Stack direction="row" spacing={1}>
            <Button
              variant="outlined"
              size="small"
              startIcon={<RefreshRoundedIcon fontSize="small" />}
              onClick={loadFlashSales}
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
              Tạo chiến dịch
            </Button>
          </Stack>
        }
      />

      <SellerCard>
        <DataTable
          rows={campaigns}
          rowKey={(row) => row.id}
          loading={loading}
          error={error}
          onRetry={loadFlashSales}
          emptyText="Hiện không có chiến dịch Flash Sale nào đang diễn ra."
          columns={[
            {
              key: 'name',
              label: 'Tên chiến dịch',
              render: (row) => (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <FlashOnRoundedIcon color="warning" fontSize="small" />
                  <Typography variant="body2" sx={{ fontWeight: 700 }}>
                    {row.name}
                  </Typography>
                </Box>
              )
            },
            {
              key: 'startAt',
              label: 'Bắt đầu',
              render: (row) => (
                <Typography variant="body2" color="text.secondary">
                  {row.startAt ? new Date(row.startAt).toLocaleString('vi-VN') : '—'}
                </Typography>
              )
            },
            {
              key: 'endAt',
              label: 'Kết thúc',
              render: (row) => (
                <Typography variant="body2" color="text.secondary">
                  {row.endAt ? new Date(row.endAt).toLocaleString('vi-VN') : '—'}
                </Typography>
              )
            },
            {
              key: 'status',
              label: 'Trạng thái',
              render: (row) => {
                const now = new Date()
                const start = new Date(row.startAt)
                const end = new Date(row.endAt)
                const hasValidSchedule = Number.isFinite(start.getTime()) && Number.isFinite(end.getTime())
                const isActive = hasValidSchedule && now >= start && now <= end
                const statusLabel = hasValidSchedule
                  ? (isActive ? 'Đang diễn ra' : now < start ? 'Sắp mở' : 'Đã kết thúc')
                  : (row.status || 'Không xác định')
                return (
                  <Chip
                    label={statusLabel}
                    size="small"
                    color={isActive ? 'error' : row.status === 'Scheduled' ? 'info' : 'default'}
                    sx={{ fontWeight: 600, borderRadius: 1.5 }}
                  />
                )
              }
            }
          ]}
        />
      </SellerCard>

      {/* Create Campaign Modal */}
      <Dialog
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        maxWidth="xs"
        fullWidth
        slotProps={{ paper: { sx: { borderRadius: 3 } } }}
      >
        <form onSubmit={handleCreate}>
          <DialogTitle sx={{ fontWeight: 800 }}>Tạo khung giờ Flash Sale mới</DialogTitle>
          <DialogContent dividers>
            <Stack spacing={2} sx={{ pt: 1 }}>
              <TextField
                required
                fullWidth
                size="small"
                label="Tên chiến dịch"
                placeholder="VD: Flash Sale Giờ Vàng 20:00 - 24:00"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
              <TextField
                required
                fullWidth
                size="small"
                type="datetime-local"
                label="Thời gian bắt đầu"
                slotProps={{ inputLabel: { shrink: true } }}
                value={startTime}
                onChange={(e) => setStartTime(e.target.value)}
              />
              <TextField
                required
                fullWidth
                size="small"
                type="datetime-local"
                label="Thời gian kết thúc"
                slotProps={{ inputLabel: { shrink: true } }}
                value={endTime}
                onChange={(e) => setEndTime(e.target.value)}
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
              disabled={isSubmitting || !name.trim()}
              sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
            >
              Tạo chiến dịch
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </>
  )
}

export default AdminFlashSalesPage
