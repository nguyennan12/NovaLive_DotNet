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
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import CancelRoundedIcon from '@mui/icons-material/CancelRounded'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import { toast } from 'react-toastify'
import {
  getAdminPayoutsAPI,
  approvePayoutAPI,
  rejectPayoutAPI
} from '~/common/apis/services/adminService'
import { PageHeader } from '~/features/Seller/components/shared/PageHeader'
import { SellerCard } from '~/features/Seller/components/shared/SellerCard'
import { DataTable } from '~/features/Seller/components/shared/DataTable'
import { MoneyText } from '~/features/Seller/components/shared/MoneyText'

const PAYOUT_STATUS_FILTERS = [
  { key: 'ALL', label: 'Tất cả' },
  { key: 'Pending', label: 'Chờ giải ngân' },
  { key: 'Approved', label: 'Đã giải ngân' },
  { key: 'Rejected', label: 'Đã từ chối' }
]

export const AdminPayoutsPage = () => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [payouts, setPayouts] = useState([])
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [statusFilter, setStatusFilter] = useState('ALL')

  // Action dialogs
  const [confirmApprove, setConfirmApprove] = useState(null)
  const [confirmReject, setConfirmReject] = useState(null)
  const [rejectReason, setRejectReason] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const loadPayouts = async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await getAdminPayoutsAPI({
        status: statusFilter,
        page,
        size: 15
      })

      const items = res?.items || res?.data || []
      setPayouts(items)
      setTotalPages(res?.totalPages || res?.total_pages || 1)
      setTotalCount(res?.totalCount || res?.total || items.length)
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadPayouts()
  }, [statusFilter, page])

  const handleExecuteApprove = async () => {
    if (!confirmApprove) return
    setIsSubmitting(true)
    try {
      await approvePayoutAPI(confirmApprove.id, { note: 'Xác nhận chuyển khoản thành công' })
      toast.success('Đã xác nhận giải ngân thành công!')
      setConfirmApprove(null)
      loadPayouts()
    } catch (err) {
      toast.error(err?.response?.data?.detail || 'Duyệt giải ngân thất bại')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleExecuteReject = async () => {
    if (!confirmReject) return
    if (!rejectReason.trim()) {
      toast.error('Vui lòng nhập lý do từ chối')
      return
    }
    setIsSubmitting(true)
    try {
      await rejectPayoutAPI(confirmReject.id, { reason: rejectReason.trim() })
      toast.info('Đã từ chối lệnh giải ngân ví')
      setConfirmReject(null)
      setRejectReason('')
      loadPayouts()
    } catch (err) {
      toast.error(err?.response?.data?.detail || 'Từ chối giải ngân thất bại')
    } finally {
      setIsSubmitting(false)
    }
  }

  const renderStatusChip = (status) => {
    const s = String(status || '').toLowerCase()
    if (s === 'approved' || s === 'completed') {
      return (
        <Chip
          label="Đã giải ngân"
          size="small"
          sx={{ bgcolor: '#ecfdf5', color: '#047857', border: '1px solid #d1fae5', fontWeight: 600 }}
        />
      )
    }
    if (s === 'pending') {
      return (
        <Chip
          label="Chờ duyệt"
          size="small"
          sx={{ bgcolor: '#fef3c7', color: '#b45309', border: '1px solid #fde68a', fontWeight: 600 }}
        />
      )
    }
    return (
      <Chip
        label="Đã từ chối"
        size="small"
        sx={{ bgcolor: '#fef2f2', color: '#b91c1c', border: '1px solid #fecaca', fontWeight: 600 }}
      />
    )
  }

  return (
    <>
      <PageHeader
        title="Duyệt giải ngân ví (Payouts)"
        description="Kiểm tra và xử lý các yêu cầu rút tiền từ số dư ví của các gian hàng."
        action={
          <Button
            variant="outlined"
            size="small"
            startIcon={<RefreshRoundedIcon fontSize="small" />}
            onClick={loadPayouts}
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
        }
      />

      <SellerCard>
        {/* Status Filters */}
        <Stack direction="row" spacing={1} flexWrap="wrap" sx={{ mb: 2.5 }}>
          {PAYOUT_STATUS_FILTERS.map((f) => {
            const active = statusFilter === f.key
            return (
              <Chip
                key={f.key}
                label={f.label}
                clickable
                onClick={() => {
                  setStatusFilter(f.key)
                  setPage(1)
                }}
                color={active ? 'secondary' : 'default'}
                variant={active ? 'filled' : 'outlined'}
                sx={{
                  fontWeight: active ? 700 : 500,
                  borderRadius: 2
                }}
              />
            )
          })}
        </Stack>

        {/* Data Table */}
        <DataTable
          rows={payouts}
          rowKey={(row) => row.id}
          loading={loading}
          error={error}
          onRetry={loadPayouts}
          emptyText="Không có yêu cầu rút tiền nào."
          page={page}
          totalPages={totalPages}
          total={totalCount}
          onPage={(p) => setPage(p)}
          columns={[
            {
              key: 'id',
              label: 'Mã yêu cầu',
              render: (row) => (
                <Typography variant="body2" sx={{ fontFamily: 'monospace', fontWeight: 600 }}>
                  {String(row.id).slice(0, 8)}...
                </Typography>
              )
            },
            {
              key: 'shop',
              label: 'Gian hàng',
              render: (row) => (
                <Box>
                  <Typography variant="body2" sx={{ fontWeight: 700 }}>
                    {row.shopName || row.shop || 'Cửa hàng'}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {row.ownerName || row.owner || ''}
                  </Typography>
                </Box>
              )
            },
            {
              key: 'amount',
              label: 'Số tiền rút',
              render: (row) => (
                <Typography variant="body2" sx={{ fontWeight: 800, color: 'secondary.main' }}>
                  <MoneyText value={row.amount || 0} />
                </Typography>
              )
            },
            {
              key: 'bank',
              label: 'Tài khoản thụ hưởng',
              render: (row) => (
                <Box>
                  <Typography variant="body2" fontWeight={600}>
                    {row.bankName || row.bank || 'Ngân hàng'}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {row.bankAccountNumber || row.bankAccount || '—'}
                  </Typography>
                </Box>
              )
            },
            {
              key: 'createdAt',
              label: 'Ngày yêu cầu',
              render: (row) => (
                <Typography variant="body2" color="text.secondary">
                  {row.createdAt ? new Date(row.createdAt).toLocaleDateString('vi-VN') : '—'}
                </Typography>
              )
            },
            {
              key: 'status',
              label: 'Trạng thái',
              render: (row) => renderStatusChip(row.status)
            },
            {
              key: 'actions',
              label: 'Thao tác',
              align: 'right',
              render: (row) => {
                const s = String(row.status || '').toLowerCase()
                if (s !== 'pending') return <Typography variant="caption" color="text.secondary">—</Typography>

                return (
                  <Stack direction="row" spacing={1} justifyContent="flex-end">
                    <Button
                      size="small"
                      variant="contained"
                      color="secondary"
                      startIcon={<CheckCircleRoundedIcon fontSize="small" />}
                      onClick={() => setConfirmApprove(row)}
                      disabled={isSubmitting}
                      sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
                    >
                      Duyệt
                    </Button>
                    <Button
                      size="small"
                      variant="outlined"
                      color="error"
                      startIcon={<CancelRoundedIcon fontSize="small" />}
                      onClick={() => {
                        setConfirmReject(row)
                        setRejectReason('')
                      }}
                      disabled={isSubmitting}
                      sx={{ textTransform: 'none', borderRadius: 2 }}
                    >
                      Từ chối
                    </Button>
                  </Stack>
                )
              }
            }
          ]}
        />
      </SellerCard>

      {/* Approve Dialog */}
      <Dialog
        open={Boolean(confirmApprove)}
        onClose={() => setConfirmApprove(null)}
        maxWidth="xs"
        fullWidth
        slotProps={{ paper: { sx: { borderRadius: 3 } } }}
      >
        <DialogTitle sx={{ fontWeight: 800 }}>Xác nhận giải ngân ví</DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 1.5 }}>
            Bạn có chắc chắn muốn duyệt và xác nhận đã chuyển khoản giải ngân số tiền:
          </Typography>
          <Typography variant="h5" sx={{ fontWeight: 800, color: 'secondary.main', textAlign: 'center', my: 2 }}>
            <MoneyText value={confirmApprove?.amount || 0} />
          </Typography>
          <Typography variant="caption" color="text.secondary">
            Cho gian hàng: <strong>{confirmApprove?.shopName || confirmApprove?.shop}</strong> ({confirmApprove?.bankName} - {confirmApprove?.bankAccountNumber})
          </Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setConfirmApprove(null)} color="inherit" sx={{ textTransform: 'none' }}>
            Hủy
          </Button>
          <Button
            variant="contained"
            color="secondary"
            onClick={handleExecuteApprove}
            disabled={isSubmitting}
            sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
          >
            Xác nhận giải ngân
          </Button>
        </DialogActions>
      </Dialog>

      {/* Reject Dialog */}
      <Dialog
        open={Boolean(confirmReject)}
        onClose={() => setConfirmReject(null)}
        maxWidth="xs"
        fullWidth
        slotProps={{ paper: { sx: { borderRadius: 3 } } }}
      >
        <DialogTitle sx={{ fontWeight: 800 }}>Từ chối yêu cầu rút tiền</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Vui lòng nhập lý do từ chối để hoàn trả lại số dư vào ví của Shop:
          </Typography>
          <TextField
            autoFocus
            fullWidth
            size="small"
            label="Lý do từ chối"
            multiline
            rows={3}
            value={rejectReason}
            onChange={(e) => setRejectReason(e.target.value)}
          />
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setConfirmReject(null)} color="inherit" sx={{ textTransform: 'none' }}>
            Hủy
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleExecuteReject}
            disabled={isSubmitting || !rejectReason.trim()}
            sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
          >
            Xác nhận từ chối
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}

export default AdminPayoutsPage
