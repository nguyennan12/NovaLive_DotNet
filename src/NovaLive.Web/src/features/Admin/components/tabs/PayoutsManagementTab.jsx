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
  Stack,
  Grid,
  TextField,
  InputAdornment
} from '@mui/material'
import AccountBalanceWalletRoundedIcon from '@mui/icons-material/AccountBalanceWalletRounded'
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import PaymentRoundedIcon from '@mui/icons-material/PaymentRounded'
import QrCode2RoundedIcon from '@mui/icons-material/QrCode2Rounded'
import { adminBtnStyles } from '../../theme/adminButtonStyles'

export const PayoutsManagementTab = ({ payouts, onApprovePayout, onRejectPayout }) => {
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState('ALL')
  const [confirmPayoutModal, setConfirmPayoutModal] = useState(null)
  const [transferRefCode, setTransferRefCode] = useState('FT261023' + Math.floor(1000 + Math.random() * 9000))

  const handleOpenConfirm = (pay) => {
    setConfirmPayoutModal(pay)
    setTransferRefCode('FT261023' + Math.floor(1000 + Math.random() * 9000))
  }

  const handleExecuteApproval = () => {
    if (!confirmPayoutModal) return
    onApprovePayout(confirmPayoutModal.id, confirmPayoutModal.amount, confirmPayoutModal.shopName)
    setConfirmPayoutModal(null)
  }

  const filteredPayouts = payouts.filter(p => {
    if (statusFilter === 'PENDING' && p.status !== 'Pending') return false
    if (statusFilter === 'COMPLETED' && p.status !== 'Completed') return false
    if (statusFilter === 'REJECTED' && p.status !== 'Rejected') return false
    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase()
      return (
        p.shopName.toLowerCase().includes(q) ||
        p.holder.toLowerCase().includes(q) ||
        p.bank.toLowerCase().includes(q)
      )
    }
    return true
  })

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, width: '100%' }}>
      {/* 3 Stat Cards */}
      <Grid container spacing={2.5}>
        <Grid item xs={12} sm={4}>
          <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Tổng Tiền Chờ Giải Ngân
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 800, color: '#f59e0b', mt: 0.5, whiteSpace: 'nowrap' }}>
                  59,700,000 đ
                </Typography>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b', flexShrink: 0 }}>
                <AccountBalanceWalletRoundedIcon />
              </Box>
            </Box>
            <Typography variant="caption" sx={{ color: '#64748b', mt: 1.5, display: 'block', whiteSpace: 'nowrap' }}>
              2 lệnh rút đang chờ Admin xác nhận
            </Typography>
          </Card>
        </Grid>

        <Grid item xs={12} sm={4}>
          <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Đã Chi Trả Tháng Này
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 800, color: '#10b981', mt: 0.5, whiteSpace: 'nowrap' }}>
                  385,400,000 đ
                </Typography>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(16, 185, 129, 0.1)', color: '#10b981', flexShrink: 0 }}>
                <CheckCircleRoundedIcon />
              </Box>
            </Box>
            <Typography variant="caption" sx={{ color: '#64748b', mt: 1.5, display: 'block', whiteSpace: 'nowrap' }}>
              Giải ngân tự động qua VietQR & NAPAS 247
            </Typography>
          </Card>
        </Grid>

        <Grid item xs={12} sm={4}>
          <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Phí Rút Tiền Hộ Đối Tác
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 800, color: '#0095ff', mt: 0.5, whiteSpace: 'nowrap' }}>
                  0 đ (Miễn Phí)
                </Typography>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(0, 149, 255, 0.1)', color: '#0095ff', flexShrink: 0 }}>
                <PaymentRoundedIcon />
              </Box>
            </Box>
            <Typography variant="caption" sx={{ color: '#64748b', mt: 1.5, display: 'block', whiteSpace: 'nowrap' }}>
              NovaLive tài trợ 100% phí chuyển khoản ngân hàng
            </Typography>
          </Card>
        </Grid>
      </Grid>

      {/* Table Card */}
      <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)', width: '100%' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2.5, flexWrap: 'wrap', gap: 1.5 }}>
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <AccountBalanceWalletRoundedIcon sx={{ color: '#10b981', fontSize: 24 }} />
              <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
                Duyệt Lệnh Rút Tiền Ví Sàn (Seller Wallet Payouts)
              </Typography>
            </Box>
            <Typography variant="caption" sx={{ color: '#64748b' }}>
              Xác nhận chuyển khoản ngân hàng qua VietQR hoặc MoMo cho người bán
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ alignItems: 'center' }}>
            {[
              { key: 'ALL', label: `Tất cả (${payouts.length})` },
              { key: 'PENDING', label: `Chờ giải ngân (${payouts.filter(p => p.status === 'Pending').length})`, isAlert: true },
              { key: 'COMPLETED', label: `Đã chi trả (${payouts.filter(p => p.status === 'Completed').length})` }
            ].map(f => {
              const isActive = statusFilter === f.key
              return (
                <Chip
                  key={f.key}
                  label={f.label}
                  onClick={() => setStatusFilter(f.key)}
                  sx={{
                    fontWeight: 700,
                    fontSize: '0.78rem',
                    whiteSpace: 'nowrap',
                    bgcolor: isActive ? (f.isAlert ? '#f59e0b' : '#0095ff') : '#f1f5f9',
                    color: isActive ? '#ffffff' : (f.isAlert ? '#d97706' : '#475569'),
                    border: f.isAlert && !isActive ? '1px solid #fde68a' : 'none',
                    '&:hover': {
                      bgcolor: isActive ? (f.isAlert ? '#d97706' : '#0080db') : '#e2e8f0'
                    }
                  }}
                />
              )
            })}
          </Stack>
        </Box>

        <Box sx={{ mb: 2 }}>
          <TextField
            size="small"
            placeholder="Tìm theo gian hàng, chủ tài khoản, ngân hàng..."
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
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Gian Hàng Yêu Cầu</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Chủ Tài Khoản</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Số Tiền Yêu Cầu</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Ngân Hàng & Số Tài Khoản</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Thời Gian Yêu Cầu</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Trạng Thái</TableCell>
                <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Xử Lý Chi Trả</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredPayouts.map((pay) => (
                <TableRow key={pay.id} hover>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{pay.shopName}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ color: '#334155', fontWeight: 600, whiteSpace: 'nowrap' }}>{pay.holder}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ fontWeight: 800, color: '#16a34a', whiteSpace: 'nowrap' }}>{pay.amount}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ color: '#334155', whiteSpace: 'nowrap' }}>{pay.bank}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>{pay.requestedAt}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Chip
                      label={pay.status === 'Completed' ? 'Đã chi trả' : pay.status === 'Pending' ? 'Chờ giải ngân' : 'Từ chối'}
                      size="small"
                      sx={{
                        fontWeight: 700,
                        height: 24,
                        fontSize: '0.72rem',
                        whiteSpace: 'nowrap',
                        bgcolor: pay.status === 'Completed' ? '#f0fdf4' : pay.status === 'Pending' ? '#fffbeb' : '#fef2f2',
                        color: pay.status === 'Completed' ? '#16a34a' : pay.status === 'Pending' ? '#d97706' : '#dc2626',
                        border: pay.status === 'Completed' ? '1px solid #bbf7d0' : pay.status === 'Pending' ? '1px solid #fde68a' : '1px solid #fecaca'
                      }}
                    />
                  </TableCell>
                  <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                    {pay.status === 'Pending' ? (
                      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                        <Button
                          size="small"
                          onClick={() => handleOpenConfirm(pay)}
                          sx={adminBtnStyles.success}
                        >
                          Duyệt VietQR
                        </Button>
                        <Button
                          size="small"
                          onClick={() => onRejectPayout(pay.id, pay.shopName)}
                          sx={adminBtnStyles.transDanger}
                        >
                          Từ chối
                        </Button>
                      </Box>
                    ) : (
                      <Typography variant="caption" sx={{ color: '#16a34a', fontWeight: 700, whiteSpace: 'nowrap' }}>
                        Đã hoàn tất
                      </Typography>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>

      {/* Confirmation & VietQR Dialog */}
      <Dialog
        open={Boolean(confirmPayoutModal)}
        onClose={() => setConfirmPayoutModal(null)}
        maxWidth="xs"
        fullWidth
        PaperProps={{ sx: { borderRadius: '16px', p: 1 } }}
      >
        <DialogTitle sx={{ fontWeight: 800, display: 'flex', alignItems: 'center', gap: 1, whiteSpace: 'nowrap' }}>
          <QrCode2RoundedIcon sx={{ color: '#10b981' }} />
          <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
            Xác Nhận Giải Ngân VietQR
          </Typography>
        </DialogTitle>
        <DialogContent dividers>
          {confirmPayoutModal && (
            <Stack spacing={2}>
              <Box sx={{ p: 2, bgcolor: '#f0fdf4', borderRadius: '10px', border: '1px solid #bbf7d0', textAlign: 'center' }}>
                <Typography variant="caption" sx={{ color: '#15803d', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>SỐ TIỀN GIẢI NGÂN</Typography>
                <Typography variant="h5" sx={{ fontWeight: 800, color: '#15803d', mt: 0.5, whiteSpace: 'nowrap' }}>
                  {confirmPayoutModal.amount}
                </Typography>
              </Box>

              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>GIAN HÀNG THỤ HƯỞNG</Typography>
                <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{confirmPayoutModal.shopName}</Typography>
              </Box>

              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>CHỦ TÀI KHOẢN & NGÂN HÀNG</Typography>
                <Typography variant="body2" sx={{ fontWeight: 600, color: '#334155', whiteSpace: 'nowrap' }}>{confirmPayoutModal.holder} - {confirmPayoutModal.bank}</Typography>
              </Box>

              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>MÃ GIAO DỊCH / BÚT TOÁN NAPAS</Typography>
                <TextField
                  fullWidth
                  size="small"
                  value={transferRefCode}
                  onChange={(e) => setTransferRefCode(e.target.value)}
                  sx={{ mt: 0.5, '& .MuiOutlinedInput-root': { borderRadius: '10px' } }}
                />
              </Box>
            </Stack>
          )}
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setConfirmPayoutModal(null)} sx={{ textTransform: 'none', color: '#64748b', fontWeight: 600, whiteSpace: 'nowrap' }}>Hủy</Button>
          <Button
            onClick={handleExecuteApproval}
            sx={adminBtnStyles.primary}
          >
            Xác Nhận Chi Trả
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default PayoutsManagementTab
