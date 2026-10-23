import { useState } from 'react'
import {
  Box,
  Card,
  Typography,
  Chip,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Divider,
  Stack,
  TextField,
  InputAdornment,
  Grid
} from '@mui/material'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded'
import BlockRoundedIcon from '@mui/icons-material/BlockRounded'
import LockOpenRoundedIcon from '@mui/icons-material/LockOpenRounded'
import BusinessRoundedIcon from '@mui/icons-material/BusinessRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import CancelRoundedIcon from '@mui/icons-material/CancelRounded'
import { toast } from 'react-toastify'
import { adminBtnStyles } from '../../theme/adminButtonStyles'

export const ShopsManagementTab = ({ shops, setShops }) => {
  const [shopFilter, setShopFilter] = useState('ALL')
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedKycShop, setSelectedKycShop] = useState(null)
  const [rejectReasonOpen, setRejectReasonOpen] = useState(false)
  const [rejectReason, setRejectReason] = useState('')

  const handleApproveKyc = (id, name) => {
    setShops(prev => prev.map(s => s.id === id ? { ...s, status: 'Active' } : s))
    setSelectedKycShop(null)
    toast.success(`Đã phê duyệt hồ sơ KYC & kích hoạt gian hàng: ${name}`)
  }

  const handleConfirmRejectKyc = () => {
    if (!rejectReason.trim()) {
      toast.error('Vui lòng nhập lý do từ chối hồ sơ')
      return
    }
    setShops(prev => prev.map(s => s.id === selectedKycShop.id ? { ...s, status: 'Rejected' } : s))
    setRejectReasonOpen(false)
    const name = selectedKycShop.name
    setSelectedKycShop(null)
    toast.info(`Đã từ chối hồ sơ Shop ${name}. Lý do: ${rejectReason}`)
    setRejectReason('')
  }

  const handleToggleBanShop = (id, name, currentStatus) => {
    const newStatus = currentStatus === 'Active' ? 'Suspended' : 'Active'
    setShops(prev => prev.map(s => s.id === id ? { ...s, status: newStatus } : s))
    if (newStatus === 'Suspended') {
      toast.error(`Đã khóa gian hàng: ${name}`)
    } else {
      toast.success(`Đã mở khóa và khôi phục hoạt động cho Shop: ${name}`)
    }
  }

  const filteredShops = shops.filter(s => {
    if (shopFilter === 'PENDING' && s.status !== 'Pending') return false
    if (shopFilter === 'ACTIVE' && s.status !== 'Active') return false
    if (shopFilter === 'SUSPENDED' && s.status !== 'Suspended') return false
    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase()
      return (
        s.name.toLowerCase().includes(q) ||
        s.owner.toLowerCase().includes(q) ||
        s.taxId.toLowerCase().includes(q) ||
        s.phone.includes(q)
      )
    }
    return true
  })

  return (
    <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)' }}>
      {/* Header & Filter */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2.5, flexWrap: 'wrap', gap: 1.5 }}>
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <StorefrontRoundedIcon sx={{ color: '#8b5cf6', fontSize: 24 }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
              Admin Operations: Duyệt Shop Onboarding & Quản Trị Gian Hàng
            </Typography>
          </Box>
          <Typography variant="caption" sx={{ color: '#64748b' }}>
            Thẩm định hồ sơ KYC pháp lý, kích hoạt shop mới, xử lý khóa vi phạm hoặc mở khóa gian hàng
          </Typography>
        </Box>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ alignItems: 'center' }}>
          {[
            { key: 'ALL', label: `Tất cả (${shops.length})` },
            { key: 'PENDING', label: `Chờ duyệt KYC (${shops.filter(s => s.status === 'Pending').length})`, colorKey: 'info' },
            { key: 'ACTIVE', label: `Đang hoạt động (${shops.filter(s => s.status === 'Active').length})`, colorKey: 'success' },
            { key: 'SUSPENDED', label: `Đã khóa (${shops.filter(s => s.status === 'Suspended').length})`, colorKey: 'error' }
          ].map(f => {
            const isActive = shopFilter === f.key
            return (
              <Chip
                key={f.key}
                label={f.label}
                onClick={() => setShopFilter(f.key)}
                sx={{
                  fontWeight: 700,
                  fontSize: '0.78rem',
                  whiteSpace: 'nowrap',
                  bgcolor: isActive ? '#0095ff' : '#f1f5f9',
                  color: isActive ? '#ffffff' : '#475569',
                  '&:hover': {
                    bgcolor: isActive ? '#0080db' : '#e2e8f0'
                  }
                }}
              />
            )
          })}
        </Stack>
      </Box>

      {/* Search Bar */}
      <Box sx={{ mb: 2 }}>
        <TextField
          size="small"
          placeholder="Tìm kiếm shop theo tên, người đại diện, MST, số điện thoại..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          sx={{
            width: { xs: '100%', sm: 380 },
            '& .MuiOutlinedInput-root': {
              borderRadius: '10px'
            }
          }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchRoundedIcon sx={{ color: '#94a3b8', fontSize: 20 }} />
              </InputAdornment>
            )
          }}
        />
      </Box>

      <TableContainer sx={{ overflowX: 'auto' }}>
        <Table size="small">
          <TableHead sx={{ bgcolor: '#f8fafc' }}>
            <TableRow>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Tên Cửa Hàng</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Chủ Gian Hàng</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Mã Số Thuế / CCCD</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Số Điện Thoại & Email</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Trạng Thái</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Hồ Sơ KYC</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Hành Động Quản Trị</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredShops.map((shop) => (
              <TableRow key={shop.id} hover>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <StorefrontRoundedIcon sx={{ color: '#8b5cf6', fontSize: '1.2rem', flexShrink: 0 }} />
                    <Box>
                      <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{shop.name}</Typography>
                      <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>Đăng ký: {shop.createdAt}</Typography>
                    </Box>
                  </Box>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Typography variant="body2" sx={{ color: '#334155', fontWeight: 600, whiteSpace: 'nowrap' }}>{shop.owner}</Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Typography variant="body2" sx={{ fontWeight: 700, color: '#0095ff', whiteSpace: 'nowrap' }}>{shop.taxId}</Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Typography variant="body2" sx={{ whiteSpace: 'nowrap', color: '#334155' }}>{shop.phone}</Typography>
                  <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>{shop.email}</Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Chip
                    label={shop.status === 'Active' ? 'Hoạt Động' : shop.status === 'Pending' ? 'Chờ Duyệt KYC' : shop.status === 'Suspended' ? 'Bị Tạm Khóa' : 'Từ Chối'}
                    size="small"
                    sx={{
                      height: 24,
                      fontSize: '0.72rem',
                      fontWeight: 700,
                      whiteSpace: 'nowrap',
                      bgcolor: shop.status === 'Active' ? '#f0fdf4' : shop.status === 'Pending' ? '#eff6ff' : '#fef2f2',
                      color: shop.status === 'Active' ? '#16a34a' : shop.status === 'Pending' ? '#0095ff' : '#dc2626',
                      border: shop.status === 'Active' ? '1px solid #bbf7d0' : shop.status === 'Pending' ? '1px solid #bfdbfe' : '1px solid #fecaca'
                    }}
                  />
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Button
                    size="small"
                    startIcon={<VisibilityRoundedIcon sx={{ fontSize: '15px !important' }} />}
                    onClick={() => setSelectedKycShop(shop)}
                    sx={adminBtnStyles.transSecondary}
                  >
                    Xem Hồ Sơ
                  </Button>
                </TableCell>
                <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                  <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                    {shop.status === 'Pending' && (
                      <>
                        <Button
                          size="small"
                          onClick={() => handleApproveKyc(shop.id, shop.name)}
                          sx={adminBtnStyles.success}
                        >
                          Duyệt
                        </Button>
                        <Button
                          size="small"
                          onClick={() => { setSelectedKycShop(shop); setRejectReasonOpen(true); }}
                          sx={adminBtnStyles.transDanger}
                        >
                          Từ Chối
                        </Button>
                      </>
                    )}
                    {shop.status === 'Active' && (
                      <Button
                        size="small"
                        startIcon={<BlockRoundedIcon sx={{ fontSize: '15px !important' }} />}
                        onClick={() => handleToggleBanShop(shop.id, shop.name, shop.status)}
                        sx={adminBtnStyles.transDanger}
                      >
                        Khóa Shop
                      </Button>
                    )}
                    {shop.status === 'Suspended' && (
                      <Button
                        size="small"
                        startIcon={<LockOpenRoundedIcon sx={{ fontSize: '15px !important' }} />}
                        onClick={() => handleToggleBanShop(shop.id, shop.name, shop.status)}
                        sx={adminBtnStyles.primary}
                      >
                        Mở Khóa
                      </Button>
                    )}
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* MODAL: XEM CHI TIẾT HỒ SƠ KYC GIAN HÀNG */}
      <Dialog
        open={Boolean(selectedKycShop)}
        onClose={() => setSelectedKycShop(null)}
        maxWidth="md"
        fullWidth
        PaperProps={{ sx: { borderRadius: '16px', p: 1 } }}
      >
        <DialogTitle sx={{ fontWeight: 800, display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <BusinessRoundedIcon sx={{ color: '#0095ff' }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
              Hồ Sơ Xác Minh KYC: {selectedKycShop?.name}
            </Typography>
          </Box>
          <Chip
            label={selectedKycShop?.status === 'Pending' ? 'Đang Chờ Duyệt' : 'Đã Xác Minh'}
            size="small"
            sx={{
              fontWeight: 700,
              whiteSpace: 'nowrap',
              bgcolor: selectedKycShop?.status === 'Pending' ? '#eff6ff' : '#f0fdf4',
              color: selectedKycShop?.status === 'Pending' ? '#0095ff' : '#16a34a'
            }}
          />
        </DialogTitle>
        <DialogContent dividers>
          {selectedKycShop && (
            <Stack spacing={2.5}>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>TÊN GIAN HÀNG</Typography>
                  <Typography variant="body1" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{selectedKycShop.name}</Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>NGƯỜI ĐẠI DIỆN PHÁP LUẬT</Typography>
                  <Typography variant="body1" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{selectedKycShop.owner}</Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>MÃ SỐ THUẾ / CCCD</Typography>
                  <Typography variant="body1" sx={{ fontWeight: 700, color: '#0095ff', whiteSpace: 'nowrap' }}>{selectedKycShop.taxId}</Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>SỐ ĐIỆN THOẠI & EMAIL</Typography>
                  <Typography variant="body2" sx={{ whiteSpace: 'nowrap' }}>{selectedKycShop.phone} • {selectedKycShop.email}</Typography>
                </Grid>
                <Grid item xs={12}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>ĐỊA CHỈ KHO HÀNG LẤY HÀNG (GHN PICKUP)</Typography>
                  <Typography variant="body2">{selectedKycShop.address}</Typography>
                </Grid>
              </Grid>

              <Divider />

              <Typography variant="subtitle2" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
                Hình Ảnh Tài Liệu Pháp Lý Đính Kèm
              </Typography>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={4}>
                  <Card sx={{ p: 1.5, border: '1px solid #e2e8f0', borderRadius: '10px' }}>
                    <Typography variant="caption" sx={{ fontWeight: 700, display: 'block', mb: 1, whiteSpace: 'nowrap' }}>1. Mặt Trước CCCD</Typography>
                    <Box
                      component="img"
                      src={selectedKycShop.idCardFront || 'https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600&auto=format&fit=crop&q=80'}
                      sx={{ width: '100%', height: 130, objectFit: 'cover', borderRadius: '6px' }}
                    />
                  </Card>
                </Grid>
                <Grid item xs={12} sm={4}>
                  <Card sx={{ p: 1.5, border: '1px solid #e2e8f0', borderRadius: '10px' }}>
                    <Typography variant="caption" sx={{ fontWeight: 700, display: 'block', mb: 1, whiteSpace: 'nowrap' }}>2. Mặt Sau CCCD</Typography>
                    <Box
                      component="img"
                      src={selectedKycShop.idCardBack || 'https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600&auto=format&fit=crop&q=80'}
                      sx={{ width: '100%', height: 130, objectFit: 'cover', borderRadius: '6px' }}
                    />
                  </Card>
                </Grid>
                <Grid item xs={12} sm={4}>
                  <Card sx={{ p: 1.5, border: '1px solid #e2e8f0', borderRadius: '10px' }}>
                    <Typography variant="caption" sx={{ fontWeight: 700, display: 'block', mb: 1, whiteSpace: 'nowrap' }}>3. Giấy Phép ĐKKD</Typography>
                    <Box
                      component="img"
                      src={selectedKycShop.businessLicense || 'https://images.unsplash.com/photo-1450133064473-71024230f91b?w=600&auto=format&fit=crop&q=80'}
                      sx={{ width: '100%', height: 130, objectFit: 'cover', borderRadius: '6px' }}
                    />
                  </Card>
                </Grid>
              </Grid>
            </Stack>
          )}
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setSelectedKycShop(null)} sx={{ textTransform: 'none', color: '#64748b', fontWeight: 600 }}>Đóng</Button>
          {selectedKycShop?.status === 'Pending' && (
            <>
              <Button
                onClick={() => setRejectReasonOpen(true)}
                sx={adminBtnStyles.transDanger}
              >
                Từ Chối Hồ Sơ
              </Button>
              <Button
                onClick={() => handleApproveKyc(selectedKycShop.id, selectedKycShop.name)}
                sx={adminBtnStyles.primary}
              >
                Chấp Thuận & Kích Hoạt Shop
              </Button>
            </>
          )}
        </DialogActions>
      </Dialog>

      {/* Reject Reason Dialog */}
      <Dialog
        open={rejectReasonOpen}
        onClose={() => setRejectReasonOpen(false)}
        maxWidth="xs"
        fullWidth
        PaperProps={{ sx: { borderRadius: '14px' } }}
      >
        <DialogTitle sx={{ fontWeight: 800 }}>Lý Do Từ Chối Hồ Sơ KYC</DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ color: '#64748b', mb: 2 }}>
            Vui lòng nêu rõ lý do để gửi email thông báo cho đối tác bổ sung:
          </Typography>
          <TextField
            autoFocus
            fullWidth
            multiline
            rows={3}
            placeholder="Ví dụ: Ảnh chụp CCCD bị lóa mờ số định danh, vui lòng chụp lại rõ nét..."
            value={rejectReason}
            onChange={(e) => setRejectReason(e.target.value)}
          />
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setRejectReasonOpen(false)} sx={{ textTransform: 'none', color: '#64748b', fontWeight: 600 }}>Hủy</Button>
          <Button onClick={handleConfirmRejectKyc} sx={adminBtnStyles.danger}>
            Xác Nhận Từ Chối
          </Button>
        </DialogActions>
      </Dialog>
    </Card>
  )
}

export default ShopsManagementTab
