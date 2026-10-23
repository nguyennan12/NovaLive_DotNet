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
  TextField,
  InputAdornment,
  Grid
} from '@mui/material'
import ReportProblemRoundedIcon from '@mui/icons-material/ReportProblemRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded'
import RemoveCircleOutlineRoundedIcon from '@mui/icons-material/RemoveCircleOutlineRounded'
import BlockRoundedIcon from '@mui/icons-material/BlockRounded'
import { toast } from 'react-toastify'
import { adminBtnStyles } from '../../theme/adminButtonStyles'

const INITIAL_REPORTS = [
  {
    id: 'REP-501',
    type: 'Lừa đảo thanh toán ngoài sàn',
    targetType: 'Shop',
    targetName: 'Fake Brands Warning Store',
    reporterName: 'Đặng Quốc Huy (huy.dang@gmail.com)',
    reason: 'Shop gửi tin nhắn chat ép chuyển cọc qua Zalo riêng để nhận giá ưu đãi 50%',
    evidenceText: 'Shop gửi mã QR ngân hàng cá nhân qua Zalo và dọa hủy đơn trên sàn nếu không chuyển cọc 200k trước.',
    evidenceImg: 'https://images.unsplash.com/photo-1616077168079-7e09a677fb2c?w=600&auto=format&fit=crop&q=80',
    severity: 'Khẩn Cấp',
    createdAt: '23/10/2026 15:10',
    status: 'Pending'
  },
  {
    id: 'REP-502',
    type: 'Nghi vấn hàng giả, nhái nhãn hiệu',
    targetType: 'Product',
    targetName: 'Nước hoa Chanel Bleu 100ml - Giá 199.000đ',
    shopName: 'Shop Nước Hoa Giá Rẻ',
    reporterName: 'Vũ Hải Đăng (dang.vu@gmail.com)',
    reason: 'Sản phẩm nhái thương hiệu trắng trợn, bán giá 199k so với giá niêm yết 3.8 triệu',
    evidenceText: 'Vỏ hộp in sai chính tả tiếng Pháp, nắp chai lỏng lẻo, mùi cồn nồng nặc.',
    evidenceImg: 'https://images.unsplash.com/photo-1541643600914-78b084683601?w=600&auto=format&fit=crop&q=80',
    severity: 'Cao',
    createdAt: '23/10/2026 13:45',
    status: 'Pending'
  },
  {
    id: 'REP-503',
    type: 'Ngôn từ xúc phạm trên Livestream',
    targetType: 'User',
    targetName: 'user_troll_99',
    reporterName: 'Fashion Forward Store',
    reason: 'Người xem liên tục spam từ ngữ thô tục quấy rối host livestream và người mua khác',
    evidenceText: 'Chat log có 15 tin nhắn chửi bới trong vòng 2 phút.',
    evidenceImg: '',
    severity: 'Trung Bình',
    createdAt: '22/10/2026 21:00',
    status: 'Pending'
  },
  {
    id: 'REP-504',
    type: 'Hàng cấm / Thực phẩm chức năng không rõ nguồn gốc',
    targetType: 'Product',
    targetName: 'Thuốc giảm cân cấp tốc 7 ngày không rõ thành phần',
    shopName: 'Gia Dụng & Sức Khỏe Fast',
    reporterName: 'Hệ thống tự động quét AI',
    reason: 'Từ khóa cấm: "giảm cân cấp tốc", không có giấy phép công bố Cục An toàn Thực phẩm',
    evidenceText: 'Không có số đăng ký xác nhận công bố phù hợp quy định an toàn thực phẩm.',
    evidenceImg: 'https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=600&auto=format&fit=crop&q=80',
    severity: 'Khẩn Cấp',
    createdAt: '21/10/2026 16:30',
    status: 'Resolved'
  }
]

export const ReportsModerationTab = () => {
  const [reports, setReports] = useState(INITIAL_REPORTS)
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState('ALL')
  const [selectedReport, setSelectedReport] = useState(null)

  const handleTakedownProduct = (report) => {
    setReports(prev => prev.map(r => r.id === report.id ? { ...r, status: 'Penalized' } : r))
    setSelectedReport(null)
    toast.success(`Đã gỡ bỏ sản phẩm vi phạm: "${report.targetName}" khỏi toàn bộ nền tảng sàn!`)
  }

  const handleBanTarget = (report) => {
    setReports(prev => prev.map(r => r.id === report.id ? { ...r, status: 'Penalized' } : r))
    setSelectedReport(null)
    toast.error(`Đã áp dụng chế tài: Khóa tài khoản đối tượng "${report.targetName}" 30 ngày!`)
  }

  const handleDismissReport = (report) => {
    setReports(prev => prev.map(r => r.id === report.id ? { ...r, status: 'Dismissed' } : r))
    setSelectedReport(null)
    toast.info(`Đã bác bỏ tố cáo ${report.id} do không đủ chứng cứ vi phạm`)
  }

  const filteredReports = reports.filter(r => {
    if (statusFilter === 'PENDING' && r.status !== 'Pending') return false
    if (statusFilter === 'PENALIZED' && r.status !== 'Penalized') return false
    if (statusFilter === 'DISMISSED' && r.status !== 'Dismissed') return false
    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase()
      return (
        r.id.toLowerCase().includes(q) ||
        r.type.toLowerCase().includes(q) ||
        r.targetName.toLowerCase().includes(q) ||
        r.reporterName.toLowerCase().includes(q)
      )
    }
    return true
  })

  return (
    <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)', width: '100%' }}>
      {/* Header & Filter */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2.5, flexWrap: 'wrap', gap: 1.5 }}>
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <ReportProblemRoundedIcon sx={{ color: '#ef4444', fontSize: 24 }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
              Xử Lý Khiếu Nại & Tố Cáo Vi Phạm (Violation Reports)
            </Typography>
          </Box>
          <Typography variant="caption" sx={{ color: '#64748b' }}>
            Tiếp nhận tố cáo hàng giả, gian lận thanh toán ngoài sàn, vi phạm quy chuẩn cộng đồng
          </Typography>
        </Box>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ alignItems: 'center' }}>
          {[
            { key: 'ALL', label: `Tất cả (${reports.length})` },
            { key: 'PENDING', label: `Chờ xử lý (${reports.filter(r => r.status === 'Pending').length})`, isAlert: true },
            { key: 'PENALIZED', label: `Đã xử phạt (${reports.filter(r => r.status === 'Penalized' || r.status === 'Resolved').length})` },
            { key: 'DISMISSED', label: `Đã bác bỏ (${reports.filter(r => r.status === 'Dismissed').length})` }
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
          placeholder="Tìm mã tố cáo, hành vi vi phạm, đối tượng, người gửi..."
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
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Mã Tố Cáo</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Hành Vi Vi Phạm</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Đối Tượng Bị Tố Cáo</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Người Tố Cáo</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Mức Độ</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Trạng Thái</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Xử Lý</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredReports.map((r) => (
              <TableRow key={r.id} hover>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Typography variant="body2" sx={{ fontWeight: 700, color: '#0095ff', whiteSpace: 'nowrap' }}>{r.id}</Typography>
                  <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>{r.createdAt}</Typography>
                </TableCell>
                <TableCell sx={{ maxWidth: 220 }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#dc2626', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{r.type}</Typography>
                  <Typography variant="caption" sx={{ color: '#64748b', display: 'block', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{r.reason}</Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap', maxWidth: 200 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.8 }}>
                    <Chip label={r.targetType} size="small" sx={{ fontSize: '0.68rem', height: 22, fontWeight: 700, whiteSpace: 'nowrap', bgcolor: '#f1f5f9', color: '#475569' }} />
                    <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }} title={r.targetName}>
                      {r.targetName}
                    </Typography>
                  </Box>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Typography variant="body2" sx={{ color: '#334155', whiteSpace: 'nowrap' }}>{r.reporterName}</Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Chip
                    label={r.severity}
                    size="small"
                    sx={{
                      fontWeight: 700,
                      height: 24,
                      fontSize: '0.72rem',
                      whiteSpace: 'nowrap',
                      bgcolor: r.severity === 'Khẩn Cấp' ? '#fef2f2' : r.severity === 'Cao' ? '#fffbeb' : '#f1f5f9',
                      color: r.severity === 'Khẩn Cấp' ? '#dc2626' : r.severity === 'Cao' ? '#d97706' : '#475569',
                      border: r.severity === 'Khẩn Cấp' ? '1px solid #fecaca' : r.severity === 'Cao' ? '1px solid #fde68a' : '1px solid #e2e8f0'
                    }}
                  />
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Chip
                    label={r.status === 'Pending' ? 'Chờ thụ lý' : r.status === 'Penalized' || r.status === 'Resolved' ? 'Đã chế tài' : 'Bác bỏ'}
                    size="small"
                    sx={{
                      fontWeight: 700,
                      height: 24,
                      fontSize: '0.72rem',
                      whiteSpace: 'nowrap',
                      bgcolor: r.status === 'Pending' ? '#fef2f2' : r.status === 'Penalized' || r.status === 'Resolved' ? '#f0fdf4' : '#f1f5f9',
                      color: r.status === 'Pending' ? '#dc2626' : r.status === 'Penalized' || r.status === 'Resolved' ? '#16a34a' : '#64748b',
                      border: r.status === 'Pending' ? '1px solid #fecaca' : r.status === 'Penalized' || r.status === 'Resolved' ? '1px solid #bbf7d0' : '1px solid #e2e8f0'
                    }}
                  />
                </TableCell>
                <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                  <Button
                    size="small"
                    startIcon={<VisibilityRoundedIcon sx={{ fontSize: '16px !important' }} />}
                    onClick={() => setSelectedReport(r)}
                    sx={adminBtnStyles.primary}
                  >
                    Xem Chi Tiết
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* MODAL: CHI TIẾT TỐ CÁO & ÁP DỤNG CHẾ TÀI */}
      <Dialog
        open={Boolean(selectedReport)}
        onClose={() => setSelectedReport(null)}
        maxWidth="md"
        fullWidth
        PaperProps={{ sx: { borderRadius: '16px', p: 1 } }}
      >
        <DialogTitle sx={{ fontWeight: 800, display: 'flex', justifyContent: 'space-between', alignItems: 'center', whiteSpace: 'nowrap' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <ReportProblemRoundedIcon sx={{ color: '#dc2626' }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
              Hồ Sơ Tố Cáo Vi Phạm: #{selectedReport?.id}
            </Typography>
          </Box>
          <Chip
            label={selectedReport?.severity}
            size="small"
            sx={{
              fontWeight: 800,
              whiteSpace: 'nowrap',
              bgcolor: selectedReport?.severity === 'Khẩn Cấp' ? '#fef2f2' : '#fffbeb',
              color: selectedReport?.severity === 'Khẩn Cấp' ? '#dc2626' : '#d97706',
              border: selectedReport?.severity === 'Khẩn Cấp' ? '1px solid #fecaca' : '1px solid #fde68a'
            }}
          />
        </DialogTitle>
        <DialogContent dividers>
          {selectedReport && (
            <Stack spacing={2.5}>
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>LOẠI HÀNH VI VI PHẠM</Typography>
                  <Typography variant="body1" sx={{ fontWeight: 700, color: '#dc2626', whiteSpace: 'nowrap' }}>{selectedReport.type}</Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>ĐỐI TƯỢNG BỊ TỐ CÁO</Typography>
                  <Typography variant="body1" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{selectedReport.targetName} ({selectedReport.targetType})</Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>NGƯỜI GỬI TỐ CÁO</Typography>
                  <Typography variant="body2" sx={{ whiteSpace: 'nowrap' }}>{selectedReport.reporterName}</Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>THỜI ĐIỂM GỬI</Typography>
                  <Typography variant="body2" sx={{ whiteSpace: 'nowrap' }}>{selectedReport.createdAt}</Typography>
                </Grid>
                <Grid item xs={12}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, whiteSpace: 'nowrap', display: 'block' }}>MÔ TẢ CHI TIẾT SỰ VIỆC</Typography>
                  <Box sx={{ p: 1.5, bgcolor: '#fef2f2', borderRadius: '8px', border: '1px solid #fecaca', mt: 0.5 }}>
                    <Typography variant="body2" sx={{ color: '#991b1b', fontWeight: 600 }}>{selectedReport.reason}</Typography>
                    <Typography variant="body2" sx={{ color: '#475569', mt: 0.5 }}>"{selectedReport.evidenceText}"</Typography>
                  </Box>
                </Grid>
              </Grid>

              {selectedReport.evidenceImg && (
                <Box>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, display: 'block', mb: 1, whiteSpace: 'nowrap' }}>
                    HÌNH ẢNH / TÀI LIỆU BẰNG CHỨNG ĐÍNH KÈM
                  </Typography>
                  <Box
                    component="img"
                    src={selectedReport.evidenceImg}
                    sx={{ width: '100%', maxHeight: 220, objectFit: 'cover', borderRadius: '10px', border: '1px solid #e2e8f0' }}
                  />
                </Box>
              )}
            </Stack>
          )}
        </DialogContent>
        <DialogActions sx={{ p: 2, justifyContent: 'space-between', bgcolor: '#f8fafc' }}>
          <Button onClick={() => setSelectedReport(null)} sx={{ textTransform: 'none', color: '#64748b', fontWeight: 600, whiteSpace: 'nowrap' }}>Đóng</Button>
          {selectedReport?.status === 'Pending' && (
            <Stack direction="row" spacing={1.5}>
              <Button
                onClick={() => handleDismissReport(selectedReport)}
                sx={adminBtnStyles.transSecondary}
              >
                Bác Bỏ (Không Vi Phạm)
              </Button>
              {selectedReport.targetType === 'Product' && (
                <Button
                  startIcon={<RemoveCircleOutlineRoundedIcon />}
                  onClick={() => handleTakedownProduct(selectedReport)}
                  sx={adminBtnStyles.warning}
                >
                  Hạ Sản Phẩm Ngay
                </Button>
              )}
              <Button
                startIcon={<BlockRoundedIcon />}
                onClick={() => handleBanTarget(selectedReport)}
                sx={adminBtnStyles.danger}
              >
                Khóa Tài Khoản Vi Phạm
              </Button>
            </Stack>
          )}
        </DialogActions>
      </Dialog>
    </Card>
  )
}

export default ReportsModerationTab
