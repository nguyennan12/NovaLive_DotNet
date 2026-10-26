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
  Stepper,
  Step,
  StepLabel,
  StepContent,
  TextField,
  InputAdornment
} from '@mui/material'
import GavelRoundedIcon from '@mui/icons-material/GavelRounded'
import PersonRoundedIcon from '@mui/icons-material/PersonRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import VideoCameraBackRoundedIcon from '@mui/icons-material/VideoCameraBackRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import { toast } from 'react-toastify'
import { adminBtnStyles } from '../../theme/adminButtonStyles'

export const DisputesArbitrationTab = ({ disputes, setDisputes }) => {
  const [selectedDispute, setSelectedDispute] = useState(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState('ALL')

  const handleResolveDispute = (disputeId, verdict) => {
    setDisputes(prev => prev.map(d => d.id === disputeId ? { ...d, status: verdict } : d))
    setSelectedDispute(null)
    if (verdict === 'BuyerWins') {
      toast.success(`Phán quyết [Buyer Wins]: Đã duyệt hoàn tiền 100% cho Người mua, trừ ví Shop vi phạm!`)
    } else {
      toast.info(`Phán quyết [Seller Wins]: Đã bác bỏ khiếu nại, giữ nguyên doanh thu giải ngân cho Shop!`)
    }
  }

  const filteredDisputes = disputes.filter(d => {
    if (statusFilter === 'PENDING' && d.status !== 'Pending') return false
    if (statusFilter === 'BUYER_WINS' && d.status !== 'BuyerWins') return false
    if (statusFilter === 'SELLER_WINS' && d.status !== 'SellerWins') return false
    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase()
      return (
        d.id.toLowerCase().includes(q) ||
        d.orderId.toLowerCase().includes(q) ||
        d.productName.toLowerCase().includes(q) ||
        d.buyerName.toLowerCase().includes(q) ||
        d.shopName.toLowerCase().includes(q)
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
            <GavelRoundedIcon sx={{ color: '#ef4444', fontSize: 24 }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
              Admin Dispute Arbitration (Phân Xử Tranh Chấp & Hoàn Tiền)
            </Typography>
          </Box>
          <Typography variant="caption" sx={{ color: '#64748b' }}>
            Màn hình trọng tài phân xử đối sánh bằng chứng giữa Người mua và Người bán, kiểm tra timeline GHN
          </Typography>
        </Box>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ alignItems: 'center' }}>
          {[
            { key: 'ALL', label: `Tất cả (${disputes.length})` },
            { key: 'PENDING', label: `Chờ Phán Quyết (${disputes.filter(d => d.status === 'Pending').length})`, isAlert: true },
            { key: 'BUYER_WINS', label: `Buyer Thắng (${disputes.filter(d => d.status === 'BuyerWins').length})` },
            { key: 'SELLER_WINS', label: `Seller Thắng (${disputes.filter(d => d.status === 'SellerWins').length})` }
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
                  bgcolor: isActive ? (f.isAlert ? '#ef4444' : '#0095ff') : '#f1f5f9',
                  color: isActive ? '#ffffff' : (f.isAlert ? '#dc2626' : '#475569'),
                  border: f.isAlert && !isActive ? '1px solid #fecaca' : 'none',
                  '&:hover': {
                    bgcolor: isActive ? (f.isAlert ? '#dc2626' : '#0080db') : '#e2e8f0'
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
          placeholder="Tìm mã vụ việc, mã đơn hàng, sản phẩm, người mua, shop..."
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
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Mã Vụ Việc</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Sản Phẩm Tranh Chấp</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Giá Trị Hoàn Trả</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Người Mua (Buyer)</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Gian Hàng (Seller)</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Trạng Thái</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Phiên Phán Quyết</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredDisputes.map((d) => (
              <TableRow key={d.id} hover>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Typography variant="body2" sx={{ fontWeight: 700, color: '#0095ff', whiteSpace: 'nowrap' }}>{d.id}</Typography>
                  <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>Đơn: {d.orderId}</Typography>
                </TableCell>
                <TableCell sx={{ maxWidth: 280, whiteSpace: 'nowrap' }}>
                  <Typography
                    variant="body2"
                    sx={{
                      fontWeight: 600,
                      color: '#0f172a',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      whiteSpace: 'nowrap'
                    }}
                    title={d.productName}
                  >
                    {d.productName}
                  </Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Typography variant="body2" sx={{ fontWeight: 800, color: '#ef4444', whiteSpace: 'nowrap' }}>{d.amount}</Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Typography variant="body2" sx={{ color: '#334155', whiteSpace: 'nowrap' }}>{d.buyerName}</Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Typography variant="body2" sx={{ color: '#334155', fontWeight: 600, whiteSpace: 'nowrap' }}>{d.shopName}</Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Chip
                    label={d.status === 'Pending' ? 'Chờ Phán Quyết' : d.status === 'BuyerWins' ? 'Buyer Thắng (Hoàn Tiền)' : 'Seller Thắng (Bác Bỏ)'}
                    size="small"
                    sx={{
                      height: 24,
                      fontSize: '0.72rem',
                      fontWeight: 700,
                      whiteSpace: 'nowrap',
                      bgcolor: d.status === 'Pending' ? '#fef2f2' : d.status === 'BuyerWins' ? '#f0fdf4' : '#eff6ff',
                      color: d.status === 'Pending' ? '#dc2626' : d.status === 'BuyerWins' ? '#16a34a' : '#0095ff',
                      border: d.status === 'Pending' ? '1px solid #fecaca' : d.status === 'BuyerWins' ? '1px solid #bbf7d0' : '1px solid #bfdbfe'
                    }}
                  />
                </TableCell>
                <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                  <Button
                    size="small"
                    startIcon={<GavelRoundedIcon sx={{ fontSize: '16px !important' }} />}
                    onClick={() => setSelectedDispute(d)}
                    sx={d.status === 'Pending' ? adminBtnStyles.danger : adminBtnStyles.transSecondary}
                  >
                    {d.status === 'Pending' ? 'Mở Phiên Phán Quyết' : 'Xem Lại Hồ Sơ'}
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* MODAL: MÀN HÌNH ĐỐI SÁNH BẰNG CHỨNG TRANH CHẤP (SIDE-BY-SIDE) */}
      <Dialog
        open={Boolean(selectedDispute)}
        onClose={() => setSelectedDispute(null)}
        maxWidth="lg"
        fullWidth
        PaperProps={{ sx: { borderRadius: '16px', p: 1 } }}
      >
        <DialogTitle sx={{ fontWeight: 800, display: 'flex', justifyContent: 'space-between', alignItems: 'center', whiteSpace: 'nowrap' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <GavelRoundedIcon sx={{ color: '#dc2626' }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
              Phiên Trọng Tài: Hồ Sơ Tranh Chấp #{selectedDispute?.id}
            </Typography>
          </Box>
          <Chip
            label={`Giá Trị Tranh Chấp: ${selectedDispute?.amount}`}
            color="error"
            sx={{ fontWeight: 800, fontSize: '0.85rem', whiteSpace: 'nowrap' }}
          />
        </DialogTitle>
        <DialogContent dividers>
          {selectedDispute && (
            <Stack spacing={3}>
              {/* Product & Orders summary banner */}
              <Card sx={{ p: 2, bgcolor: '#f8fafc', border: '1px solid #e2e8f0', borderRadius: '12px' }}>
                <Grid container spacing={2}>
                  <Grid item xs={12} sm={4}>
                    <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>SẢN PHẨM TRANH CHẤP</Typography>
                    <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{selectedDispute.productName}</Typography>
                  </Grid>
                  <Grid item xs={12} sm={4}>
                    <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>NGƯỜI MUA (BUYER)</Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600, whiteSpace: 'nowrap' }}>{selectedDispute.buyerName}</Typography>
                  </Grid>
                  <Grid item xs={12} sm={4}>
                    <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>GIAN HÀNG (SELLER)</Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600, whiteSpace: 'nowrap' }}>{selectedDispute.shopName}</Typography>
                  </Grid>
                </Grid>
              </Card>

              {/* Side-by-side Evidence Comparison */}
              <Grid container spacing={2.5}>
                {/* Column Left: Buyer Claim & Evidence */}
                <Grid item xs={12} md={6}>
                  <Card sx={{ p: 2.5, height: '100%', border: '1.5px solid #fed7aa', borderRadius: '14px', bgcolor: '#fffbf5' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
                      <PersonRoundedIcon sx={{ color: '#ea580c' }} />
                      <Typography variant="subtitle1" sx={{ fontWeight: 800, color: '#9a3412', whiteSpace: 'nowrap' }}>
                        Bằng Chứng Người Mua (Buyer Claim)
                      </Typography>
                    </Box>
                    <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', mb: 0.5 }}>
                      Lý do: {selectedDispute.buyerClaim.reason}
                    </Typography>
                    <Typography
                      variant="body2"
                      sx={{ color: '#475569', mb: 2, bgcolor: '#ffffff', p: 1.5, borderRadius: '8px', border: '1px solid #fed7aa' }}
                    >
                      "{selectedDispute.buyerClaim.detail}"
                    </Typography>

                    <Typography variant="caption" sx={{ fontWeight: 700, color: '#64748b', display: 'block', mb: 1, whiteSpace: 'nowrap' }}>
                      HÌNH ẢNH MỞ HỘP CỦA KHÁCH:
                    </Typography>
                    <Grid container spacing={1}>
                      {selectedDispute.buyerClaim.photos.map((src, i) => (
                        <Grid item xs={6} key={i}>
                          <Box
                            component="img"
                            src={src}
                            sx={{ width: '100%', height: 130, objectFit: 'cover', borderRadius: '8px', border: '1px solid #fed7aa' }}
                          />
                        </Grid>
                      ))}
                    </Grid>
                    <Box sx={{ mt: 2, p: 1, bgcolor: '#fff', borderRadius: '8px', display: 'flex', alignItems: 'center', gap: 1 }}>
                      <VideoCameraBackRoundedIcon sx={{ color: '#ea580c' }} />
                      <Typography variant="caption" sx={{ fontWeight: 600, whiteSpace: 'nowrap' }}>{selectedDispute.buyerClaim.videoProof}</Typography>
                    </Box>
                  </Card>
                </Grid>

                {/* Column Right: Seller Evidence */}
                <Grid item xs={12} md={6}>
                  <Card sx={{ p: 2.5, height: '100%', border: '1.5px solid #bfdbfe', borderRadius: '14px', bgcolor: '#f8faff' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
                      <StorefrontRoundedIcon sx={{ color: '#0095ff' }} />
                      <Typography variant="subtitle1" sx={{ fontWeight: 800, color: '#0095ff', whiteSpace: 'nowrap' }}>
                        Bằng Chứng Người Bán (Seller Evidence)
                      </Typography>
                    </Box>
                    <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', mb: 0.5 }}>
                      Giải trình: {selectedDispute.sellerEvidence.reason}
                    </Typography>
                    <Typography
                      variant="body2"
                      sx={{ color: '#475569', mb: 2, bgcolor: '#ffffff', p: 1.5, borderRadius: '8px', border: '1px solid #bfdbfe' }}
                    >
                      "{selectedDispute.sellerEvidence.detail}"
                    </Typography>

                    <Typography variant="caption" sx={{ fontWeight: 700, color: '#64748b', display: 'block', mb: 1, whiteSpace: 'nowrap' }}>
                      HÌNH ẢNH ĐÓNG GÓI TẠI KHO SHOP:
                    </Typography>
                    <Grid container spacing={1}>
                      {selectedDispute.sellerEvidence.photos.length > 0 ? (
                        selectedDispute.sellerEvidence.photos.map((src, i) => (
                          <Grid item xs={6} key={i}>
                            <Box
                              component="img"
                              src={src}
                              sx={{ width: '100%', height: 130, objectFit: 'cover', borderRadius: '8px', border: '1px solid #bfdbfe' }}
                            />
                          </Grid>
                        ))
                      ) : (
                        <Grid item xs={12}>
                          <Box sx={{ height: 130, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: '#f1f5f9', borderRadius: '8px' }}>
                            <Typography variant="caption" sx={{ color: '#94a3b8', whiteSpace: 'nowrap' }}>Shop không bổ sung thêm ảnh đóng gói</Typography>
                          </Box>
                        </Grid>
                      )}
                    </Grid>
                    <Box sx={{ mt: 2, p: 1, bgcolor: '#fff', borderRadius: '8px', display: 'flex', alignItems: 'center', gap: 1 }}>
                      <VideoCameraBackRoundedIcon sx={{ color: '#0095ff' }} />
                      <Typography variant="caption" sx={{ fontWeight: 600, whiteSpace: 'nowrap' }}>{selectedDispute.sellerEvidence.videoProof}</Typography>
                    </Box>
                  </Card>
                </Grid>
              </Grid>

              {/* Timeline Vận Chuyển GHN */}
              <Card sx={{ p: 2.5, border: '1px solid #e2e8f0', borderRadius: '14px' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <LocalShippingRoundedIcon sx={{ color: '#16a34a' }} />
                  <Typography variant="subtitle2" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
                    Timeline Vận Chuyển GHN & Lịch Sử Leo Thang Tranh Chấp
                  </Typography>
                </Box>
                <Stepper orientation="vertical">
                  {selectedDispute.ghnTimeline.map((step, idx) => (
                    <Step key={idx} active={true} completed={true}>
                      <StepLabel
                        StepIconComponent={() => (
                          <Box
                            sx={{
                              width: 10,
                              height: 10,
                              borderRadius: '50%',
                              bgcolor: idx === selectedDispute.ghnTimeline.length - 1 ? '#dc2626' : '#0095ff'
                            }}
                          />
                        )}
                      >
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{step.label}</Typography>
                          <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>({step.time})</Typography>
                        </Box>
                      </StepLabel>
                      <StepContent>
                        <Typography variant="caption" sx={{ color: '#475569' }}>{step.detail}</Typography>
                      </StepContent>
                    </Step>
                  ))}
                </Stepper>
              </Card>
            </Stack>
          )}
        </DialogContent>
        <DialogActions sx={{ p: 2.5, justifyContent: 'space-between', bgcolor: '#f8fafc' }}>
          <Button onClick={() => setSelectedDispute(null)} sx={{ textTransform: 'none', color: '#64748b', fontWeight: 600, whiteSpace: 'nowrap' }}>Đóng lại</Button>
          {selectedDispute?.status === 'Pending' && (
            <Stack direction="row" spacing={1.5} flexWrap="wrap">
              <Button
                onClick={() => handleResolveDispute(selectedDispute.id, 'BuyerWins')}
                sx={adminBtnStyles.success}
              >
                Phán Quyết: Buyer Wins (Hoàn tiền 100% cho Người mua)
              </Button>
              <Button
                onClick={() => handleResolveDispute(selectedDispute.id, 'SellerWins')}
                sx={adminBtnStyles.primary}
              >
                Phán Quyết: Seller Wins (Bác bỏ khiếu nại, giữ tiền Shop)
              </Button>
            </Stack>
          )}
        </DialogActions>
      </Dialog>
    </Card>
  )
}

export default DisputesArbitrationTab
