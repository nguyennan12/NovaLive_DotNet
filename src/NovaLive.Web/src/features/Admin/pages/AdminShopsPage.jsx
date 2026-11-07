import { useState, useEffect } from 'react'
import {
  Avatar,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Divider,
  Grid,
  IconButton,
  InputAdornment,
  Stack,
  TextField,
  Tooltip,
  Typography
} from '@mui/material'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import BlockRoundedIcon from '@mui/icons-material/BlockRounded'
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import LockOpenRoundedIcon from '@mui/icons-material/LockOpenRounded'
import { toast } from 'react-toastify'
import {
  getAdminShopsAPI,
  approveShopAPI,
  banShopAPI
} from '~/common/apis/services/adminService'
import { PageHeader } from '~/features/Seller/components/shared/PageHeader'
import { SellerCard } from '~/features/Seller/components/shared/SellerCard'
import { DataTable } from '~/features/Seller/components/shared/DataTable'
import { QueryState } from '~/features/Seller/components/shared/QueryState'

const STATUS_FILTERS = [
  { key: 'ALL', label: 'Tất cả' },
  { key: 'Pending', label: 'Chờ duyệt' },
  { key: 'Active', label: 'Hoạt động' },
  { key: 'Banned', label: 'Bị khóa' }
]

export const AdminShopsPage = () => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [shops, setShops] = useState([])
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)

  const [statusFilter, setStatusFilter] = useState('ALL')
  const [keyword, setKeyword] = useState('')
  const [searchQuery, setSearchQuery] = useState('')

  // KYC modal & Ban dialog
  const [viewKycShop, setViewKycShop] = useState(null)
  const [banModalShop, setBanModalShop] = useState(null)
  const [banReason, setBanReason] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const loadShops = async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await getAdminShopsAPI({
        status: statusFilter,
        keyword: searchQuery || undefined,
        page,
        size: 15
      })

      const items = res?.items || res?.data || []
      setShops(items)
      setTotalPages(res?.totalPages || res?.total_pages || 1)
      setTotalCount(res?.totalCount || res?.total || items.length)
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadShops()
  }, [statusFilter, searchQuery, page])

  const handleSearchSubmit = (e) => {
    e.preventDefault()
    setPage(1)
    setSearchQuery(keyword.trim())
  }

  const handleApprove = async (shop) => {
    setIsSubmitting(true)
    try {
      await approveShopAPI(shop.id, { note: 'Hồ sơ KYC hợp lệ' })
      toast.success(`Đã phê duyệt gian hàng: ${shop.shopName || shop.name}`)
      setViewKycShop(null)
      loadShops()
    } catch (err) {
      toast.error(err?.response?.data?.detail || 'Phê duyệt thất bại')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleConfirmBan = async () => {
    if (!banReason.trim()) {
      toast.error('Vui lòng nhập lý do')
      return
    }
    setIsSubmitting(true)
    try {
      await banShopAPI(banModalShop.id, { reason: banReason.trim() })
      toast.success(`Đã khóa gian hàng: ${banModalShop.shopName || banModalShop.name}`)
      setBanModalShop(null)
      setBanReason('')
      loadShops()
    } catch (err) {
      toast.error(err?.response?.data?.detail || 'Khóa gian hàng thất bại')
    } finally {
      setIsSubmitting(false)
    }
  }

  const renderStatusChip = (status) => {
    const s = String(status || '').toLowerCase()
    if (s === 'active') {
      return (
        <Chip
          label="Hoạt động"
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
        label={status || 'Khóa'}
        size="small"
        sx={{ bgcolor: '#fef2f2', color: '#b91c1c', border: '1px solid #fecaca', fontWeight: 600 }}
      />
    )
  }

  return (
    <>
      <PageHeader
        title="Quản trị và phê duyệt hồ sơ Shop"
        description="Kiểm tra hồ sơ định danh KYC, phê duyệt mở gian hàng và quản lý trạng thái hoạt động."
        action={
          <Button
            variant="outlined"
            size="small"
            startIcon={<RefreshRoundedIcon fontSize="small" />}
            onClick={loadShops}
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
        {/* Filters Bar */}
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          justifyContent="space-between"
          alignItems={{ xs: 'stretch', sm: 'center' }}
          gap={2}
          sx={{ mb: 2.5 }}
        >
          {/* Status Filter Tabs */}
          <Stack direction="row" spacing={1} flexWrap="wrap">
            {STATUS_FILTERS.map((f) => {
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

          {/* Search Box */}
          <Box component="form" onSubmit={handleSearchSubmit} sx={{ minWidth: { xs: '100%', sm: 260 } }}>
            <TextField
              size="small"
              fullWidth
              placeholder="Tìm theo tên shop..."
              value={keyword}
              onChange={(e) => setKeyword(e.target.value)}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchRoundedIcon fontSize="small" sx={{ color: 'text.secondary' }} />
                  </InputAdornment>
                )
              }}
              sx={{ bgcolor: '#ffffff', borderRadius: 2 }}
            />
          </Box>
        </Stack>

        {/* Data Table */}
        <DataTable
          rows={shops}
          rowKey={(row) => row.id}
          loading={loading}
          error={error}
          onRetry={loadShops}
          emptyText="Không có gian hàng nào phù hợp điều kiện lọc."
          page={page}
          totalPages={totalPages}
          total={totalCount}
          onPage={(p) => setPage(p)}
          columns={[
            {
              key: 'shop',
              label: 'Gian hàng',
              render: (row) => (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                  <Avatar
                    src={row.logoUrl || ''}
                    sx={{
                      width: 40,
                      height: 40,
                      bgcolor: 'rgba(0, 149, 255, 0.1)',
                      color: 'secondary.main',
                      fontWeight: 700
                    }}
                  >
                    {row.shopName?.[0]?.toUpperCase() || 'S'}
                  </Avatar>
                  <Box>
                    <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a' }}>
                      {row.shopName || row.name}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      @{row.slug || String(row.id).slice(0, 8)}
                    </Typography>
                  </Box>
                </Box>
              )
            },
            {
              key: 'contact',
              label: 'Liên hệ',
              render: (row) => (
                <Box>
                  <Typography variant="body2">{row.email || '—'}</Typography>
                  <Typography variant="caption" color="text.secondary">
                    {row.phone || '—'}
                  </Typography>
                </Box>
              )
            },
            {
              key: 'createdAt',
              label: 'Ngày gửi hồ sơ',
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
              key: 'kyc',
              label: 'Hồ sơ KYC',
              render: (row) => (
                <Button
                  size="small"
                  variant="outlined"
                  startIcon={<VisibilityRoundedIcon fontSize="small" />}
                  onClick={() => setViewKycShop(row)}
                  sx={{
                    textTransform: 'none',
                    borderRadius: 2,
                    fontSize: '0.78rem'
                  }}
                >
                  Xem hồ sơ
                </Button>
              )
            },
            {
              key: 'actions',
              label: 'Thao tác',
              align: 'right',
              render: (row) => {
                const s = String(row.status || '').toLowerCase()
                return (
                  <Stack direction="row" spacing={1} justifyContent="flex-end">
                    {s === 'pending' && (
                      <>
                        <Button
                          size="small"
                          variant="contained"
                          color="secondary"
                          startIcon={<CheckCircleRoundedIcon fontSize="small" />}
                          onClick={() => handleApprove(row)}
                          disabled={isSubmitting}
                          sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
                        >
                          Duyệt
                        </Button>
                        <Button
                          size="small"
                          variant="outlined"
                          color="error"
                          onClick={() => {
                            setBanModalShop(row)
                            setBanReason('')
                          }}
                          disabled={isSubmitting}
                          sx={{ textTransform: 'none', borderRadius: 2 }}
                        >
                          Từ chối
                        </Button>
                      </>
                    )}
                    {s === 'active' && (
                      <Button
                        size="small"
                        variant="outlined"
                        color="error"
                        startIcon={<BlockRoundedIcon fontSize="small" />}
                        onClick={() => {
                          setBanModalShop(row)
                          setBanReason('')
                        }}
                        disabled={isSubmitting}
                        sx={{ textTransform: 'none', borderRadius: 2 }}
                      >
                        Khóa shop
                      </Button>
                    )}
                  </Stack>
                )
              }
            }
          ]}
        />
      </SellerCard>

      {/* KYC Detail Dialog */}
      <Dialog
        open={Boolean(viewKycShop)}
        onClose={() => setViewKycShop(null)}
        maxWidth="md"
        fullWidth
        slotProps={{ paper: { sx: { borderRadius: 3 } } }}
      >
        <DialogTitle sx={{ fontWeight: 800 }}>
          Hồ sơ định danh KYC: {viewKycShop?.shopName || viewKycShop?.name}
        </DialogTitle>
        <DialogContent dividers>
          {viewKycShop && (
            <Stack spacing={2.5}>
              <Box>
                <Typography variant="subtitle2" color="text.secondary" sx={{ textTransform: 'uppercase', mb: 1, fontWeight: 700 }}>
                  Thông tin liên hệ & Đăng ký
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="body2" color="text.secondary">Tên shop:</Typography>
                    <Typography variant="body2" fontWeight={600}>{viewKycShop.shopName || viewKycShop.name}</Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="body2" color="text.secondary">Email liên hệ:</Typography>
                    <Typography variant="body2" fontWeight={600}>{viewKycShop.email || '—'}</Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="body2" color="text.secondary">Số điện thoại:</Typography>
                    <Typography variant="body2" fontWeight={600}>{viewKycShop.phone || '—'}</Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="body2" color="text.secondary">Trạng thái:</Typography>
                    {renderStatusChip(viewKycShop.status)}
                  </Grid>
                </Grid>
              </Box>

              <Divider />

              {/* Bank Details */}
              <Box>
                <Typography variant="subtitle2" color="text.secondary" sx={{ textTransform: 'uppercase', mb: 1, fontWeight: 700 }}>
                  Tài khoản ngân hàng thụ hưởng
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <Typography variant="body2" color="text.secondary">Ngân hàng:</Typography>
                    <Typography variant="body2" fontWeight={600}>{viewKycShop.bankName || '—'}</Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <Typography variant="body2" color="text.secondary">Số tài khoản:</Typography>
                    <Typography variant="body2" fontWeight={600}>{viewKycShop.bankAccount || '—'}</Typography>
                  </Grid>
                  <Grid size={{ xs: 12, sm: 4 }}>
                    <Typography variant="body2" color="text.secondary">Chi nhánh:</Typography>
                    <Typography variant="body2" fontWeight={600}>{viewKycShop.bankBranch || 'Mặc định'}</Typography>
                  </Grid>
                </Grid>
              </Box>

              <Divider />

              {/* ID Cards Photos */}
              <Box>
                <Typography variant="subtitle2" color="text.secondary" sx={{ textTransform: 'uppercase', mb: 1.5, fontWeight: 700 }}>
                  Hình ảnh Giấy tờ tùy thân (CCCD / Giấy phép kinh doanh)
                </Typography>
                <Grid container spacing={2}>
                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="body2" fontWeight={600} sx={{ mb: 1 }}>
                      Mặt trước CCCD:
                    </Typography>
                    {viewKycShop.idCardFrontUrl || viewKycShop.idCardFront ? (
                      <Box
                        component="img"
                        src={viewKycShop.idCardFrontUrl || viewKycShop.idCardFront}
                        alt="CCCD Front"
                        sx={{
                          width: '100%',
                          maxHeight: 240,
                          objectFit: 'contain',
                          border: '1px solid #e2e8f0',
                          borderRadius: 2,
                          bgcolor: '#f8fafc'
                        }}
                      />
                    ) : (
                      <Typography variant="body2" color="text.secondary">Chưa có ảnh mặt trước</Typography>
                    )}
                  </Grid>

                  <Grid size={{ xs: 12, sm: 6 }}>
                    <Typography variant="body2" fontWeight={600} sx={{ mb: 1 }}>
                      Mặt sau CCCD:
                    </Typography>
                    {viewKycShop.idCardBackUrl || viewKycShop.idCardBack ? (
                      <Box
                        component="img"
                        src={viewKycShop.idCardBackUrl || viewKycShop.idCardBack}
                        alt="CCCD Back"
                        sx={{
                          width: '100%',
                          maxHeight: 240,
                          objectFit: 'contain',
                          border: '1px solid #e2e8f0',
                          borderRadius: 2,
                          bgcolor: '#f8fafc'
                        }}
                      />
                    ) : (
                      <Typography variant="body2" color="text.secondary">Chưa có ảnh mặt sau</Typography>
                    )}
                  </Grid>
                </Grid>
              </Box>
            </Stack>
          )}
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setViewKycShop(null)} color="inherit" sx={{ textTransform: 'none' }}>
            Đóng
          </Button>
          {String(viewKycShop?.status || '').toLowerCase() === 'pending' && (
            <>
              <Button
                variant="outlined"
                color="error"
                onClick={() => {
                  setBanModalShop(viewKycShop)
                  setViewKycShop(null)
                }}
                sx={{ textTransform: 'none', borderRadius: 2 }}
              >
                Từ chối hồ sơ
              </Button>
              <Button
                variant="contained"
                color="secondary"
                onClick={() => handleApprove(viewKycShop)}
                disabled={isSubmitting}
                sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
              >
                Phê duyệt ngay
              </Button>
            </>
          )}
        </DialogActions>
      </Dialog>

      {/* Ban / Reject Reason Modal */}
      <Dialog
        open={Boolean(banModalShop)}
        onClose={() => setBanModalShop(null)}
        maxWidth="xs"
        fullWidth
        slotProps={{ paper: { sx: { borderRadius: 3 } } }}
      >
        <DialogTitle sx={{ fontWeight: 800 }}>
          {String(banModalShop?.status || '').toLowerCase() === 'pending' ? 'Từ chối duyệt Shop' : 'Khóa gian hàng'}
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Vui lòng nhập lý do để thông báo đến chủ cửa hàng:
          </Typography>
          <TextField
            autoFocus
            fullWidth
            size="small"
            label="Lý do"
            multiline
            rows={3}
            value={banReason}
            onChange={(e) => setBanReason(e.target.value)}
          />
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setBanModalShop(null)} color="inherit" sx={{ textTransform: 'none' }}>
            Hủy
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleConfirmBan}
            disabled={isSubmitting || !banReason.trim()}
            sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
          >
            Xác nhận
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}

export default AdminShopsPage
