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
  Grid,
  Stack,
  TextField,
  InputAdornment,
  Avatar
} from '@mui/material'
import FiberManualRecordIcon from '@mui/icons-material/FiberManualRecord'
import LiveTvRoundedIcon from '@mui/icons-material/LiveTvRounded'
import BlockRoundedIcon from '@mui/icons-material/BlockRounded'
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded'
import WarningAmberRoundedIcon from '@mui/icons-material/WarningAmberRounded'
import NetworkCheckRoundedIcon from '@mui/icons-material/NetworkCheckRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import SendRoundedIcon from '@mui/icons-material/SendRounded'
import { toast } from 'react-toastify'
import { adminBtnStyles } from '../../theme/adminButtonStyles'

const INITIAL_STREAMS = [
  {
    id: 'live-101',
    title: 'Đại tiệc Flash Sale 50% - Gia Dụng Thông Minh VIP',
    shop: 'Gia Dụng Nova VIP',
    streamer: 'Ngọc Mai Host',
    avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=100&auto=format&fit=crop&q=80',
    thumbnail: 'https://images.unsplash.com/photo-1556911220-e15b29be8c8f?w=600&auto=format&fit=crop&q=80',
    category: 'Gia Dụng',
    viewers: 4820,
    orders: 312,
    gmv: '78,400,000 đ',
    bitrate: '2,400 kbps',
    health: 'Ổn định',
    violationStatus: 'Safe',
    startedAt: '19:30'
  },
  {
    id: 'live-102',
    title: 'Thời Trang Nam Nữ Thu Đông - Giảm Sâu Cực Sốc',
    shop: 'Fashion Forward Store',
    streamer: 'Đức Minh',
    avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100&auto=format&fit=crop&q=80',
    thumbnail: 'https://images.unsplash.com/photo-1489987707025-afc232f7ea0f?w=600&auto=format&fit=crop&q=80',
    category: 'Thời Trang',
    viewers: 3650,
    orders: 245,
    gmv: '54,200,000 đ',
    bitrate: '2,100 kbps',
    health: 'Ổn định',
    violationStatus: 'Safe',
    startedAt: '20:00'
  },
  {
    id: 'live-103',
    title: 'Xả Kho Bán Lỗ Tai Nghe Không Dây Siêu Rẻ (Reported)',
    shop: 'TechLab Gaming Gears',
    streamer: 'Quang Tèo Review',
    avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=100&auto=format&fit=crop&q=80',
    thumbnail: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&auto=format&fit=crop&q=80',
    category: 'Điện Tử',
    viewers: 2890,
    orders: 118,
    gmv: '29,500,000 đ',
    bitrate: '1,850 kbps',
    health: 'Drop Frame 8%',
    violationStatus: 'Warning',
    startedAt: '20:15'
  },
  {
    id: 'live-104',
    title: 'Mỹ Phẩm Serum Cấp Ẩm Da Mùa Hanh Khô',
    shop: 'Mỹ Phẩm Glow & Care',
    streamer: 'Thanh Thảo',
    avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100&auto=format&fit=crop&q=80',
    thumbnail: 'https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=600&auto=format&fit=crop&q=80',
    category: 'Mỹ Phẩm',
    viewers: 1980,
    orders: 164,
    gmv: '32,100,000 đ',
    bitrate: '2,350 kbps',
    health: 'Ổn định',
    violationStatus: 'Safe',
    startedAt: '20:30'
  }
]

export const LiveModerationTab = () => {
  const [streams, setStreams] = useState(INITIAL_STREAMS)
  const [searchTerm, setSearchTerm] = useState('')
  const [activeStreamModal, setActiveStreamModal] = useState(null)
  const [warningMessage, setWarningMessage] = useState('')
  const [killConfirmModal, setKillConfirmModal] = useState(null)

  const handleKillSwitch = (stream) => {
    setStreams(prev => prev.filter(s => s.id !== stream.id))
    setKillConfirmModal(null)
    setActiveStreamModal(null)
    toast.error(`ĐÃ KÍCH HOẠT KILL-SWITCH: Dừng phòng livestream "${stream.title}" khẩn cấp!`)
  }

  const handleSendWarning = (stream) => {
    if (!warningMessage.trim()) {
      toast.warning('Vui lòng nhập nội dung cảnh cáo')
      return
    }
    toast.info(`Đã gửi cảnh báo đỏ tới phòng live của ${stream.shop}: "${warningMessage}"`)
    setWarningMessage('')
  }

  const filteredStreams = streams.filter(s => {
    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase()
      return (
        s.title.toLowerCase().includes(q) ||
        s.shop.toLowerCase().includes(q) ||
        s.streamer.toLowerCase().includes(q) ||
        s.category.toLowerCase().includes(q)
      )
    }
    return true
  })

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, width: '100%' }}>
      {/* 4 Metrics Header */}
      <Grid container spacing={2.5}>
        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Đang Phát Trực Tiếp
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 800, color: '#ef4444', mt: 0.5, whiteSpace: 'nowrap' }}>
                  {streams.length} Phòng Live
                </Typography>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', flexShrink: 0 }}>
                <FiberManualRecordIcon />
              </Box>
            </Box>
            <Typography variant="caption" sx={{ color: '#16a34a', mt: 1.5, display: 'block', fontWeight: 600, whiteSpace: 'nowrap' }}>
              • Tất cả luồng Agora RTC hoạt động
            </Typography>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Tổng Khán Giả Đồng Thời (CCU)
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 800, color: '#0f172a', mt: 0.5, whiteSpace: 'nowrap' }}>
                  {streams.reduce((acc, s) => acc + s.viewers, 0).toLocaleString()} CCU
                </Typography>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(0, 149, 255, 0.1)', color: '#0095ff', flexShrink: 0 }}>
                <LiveTvRoundedIcon />
              </Box>
            </Box>
            <Typography variant="caption" sx={{ color: '#64748b', mt: 1.5, display: 'block', whiteSpace: 'nowrap' }}>
              Đỉnh điểm: 16,800 CCU lúc 20:45
            </Typography>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Băng Thông Trung Bình
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 800, color: '#10b981', mt: 0.5, whiteSpace: 'nowrap' }}>
                  2,250 kbps
                </Typography>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(16, 185, 129, 0.1)', color: '#10b981', flexShrink: 0 }}>
                <NetworkCheckRoundedIcon />
              </Box>
            </Box>
            <Typography variant="caption" sx={{ color: '#64748b', mt: 1.5, display: 'block', whiteSpace: 'nowrap' }}>
              Độ trễ trung bình: 180ms Ultra Low Latency
            </Typography>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} lg={3}>
          <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)' }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Cảnh Báo Nội Dung
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 800, color: '#f59e0b', mt: 0.5, whiteSpace: 'nowrap' }}>
                  1 Cảnh Báo
                </Typography>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b', flexShrink: 0 }}>
                <WarningAmberRoundedIcon />
              </Box>
            </Box>
            <Typography variant="caption" sx={{ color: '#ef4444', mt: 1.5, display: 'block', fontWeight: 700, whiteSpace: 'nowrap' }}>
              Cần kiểm tra: TechLab Gaming Gears
            </Typography>
          </Card>
        </Grid>
      </Grid>

      {/* Main Table */}
      <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)', width: '100%' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2.5, flexWrap: 'wrap', gap: 1.5 }}>
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <LiveTvRoundedIcon sx={{ color: '#0095ff', fontSize: 24 }} />
              <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
                Bảng Điều Phối Giám Sát Livestream Trực Tiếp (Realtime Moderation)
              </Typography>
            </Box>
            <Typography variant="caption" sx={{ color: '#64748b' }}>
              Theo dõi chất lượng luồng, doanh số trực tiếp và can thiệp bảo vệ quy chuẩn cộng đồng
            </Typography>
          </Box>
          <Box sx={{ width: { xs: '100%', sm: 320 } }}>
            <TextField
              size="small"
              fullWidth
              placeholder="Tìm theo tiêu đề, tên shop, streamer..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              sx={{ '& .MuiOutlinedInput-root': { borderRadius: '10px' } }}
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchRoundedIcon sx={{ color: '#94a3b8', fontSize: 20 }} />
                  </InputAdornment>
                )
              }}
            />
          </Box>
        </Box>

        <TableContainer sx={{ overflowX: 'auto' }}>
          <Table size="small">
            <TableHead sx={{ bgcolor: '#f8fafc' }}>
              <TableRow>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Phiên Live & Host</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Ngành Hàng</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Khán Giả (CCU)</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Đơn Hàng / GMV</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Chất Lượng Luồng</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>An Toàn Nội Dung</TableCell>
                <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Hành Động Khẩn</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredStreams.map((s) => (
                <TableRow key={s.id} hover>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                      <Avatar src={s.avatar} sx={{ width: 40, height: 40, border: '2px solid #ef4444' }} />
                      <Box>
                        <Typography
                          variant="body2"
                          sx={{
                            fontWeight: 700,
                            color: '#0f172a',
                            maxWidth: 280,
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            whiteSpace: 'nowrap'
                          }}
                          title={s.title}
                        >
                          {s.title}
                        </Typography>
                        <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>
                          Shop: {s.shop} • Bắt đầu: {s.startedAt}
                        </Typography>
                      </Box>
                    </Box>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Chip label={s.category} size="small" sx={{ fontWeight: 700, bgcolor: '#f1f5f9', color: '#475569', height: 24, fontSize: '0.75rem', whiteSpace: 'nowrap' }} />
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                      <FiberManualRecordIcon sx={{ fontSize: 10, color: '#ef4444' }} />
                      <Typography variant="body2" sx={{ fontWeight: 800, color: '#ef4444', whiteSpace: 'nowrap' }}>
                        {s.viewers.toLocaleString()} CCU
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ fontWeight: 800, color: '#16a34a', whiteSpace: 'nowrap' }}>{s.gmv}</Typography>
                    <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>{s.orders} đơn chốt</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ fontWeight: 600, color: '#0f172a', whiteSpace: 'nowrap' }}>{s.bitrate}</Typography>
                    <Typography variant="caption" sx={{ color: s.health === 'Ổn định' ? '#16a34a' : '#ef4444', fontWeight: 600, whiteSpace: 'nowrap', display: 'block' }}>
                      {s.health}
                    </Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Chip
                      label={s.violationStatus === 'Safe' ? 'An toàn' : 'Có Báo Cáo'}
                      size="small"
                      sx={{
                        fontWeight: 700,
                        height: 24,
                        fontSize: '0.72rem',
                        whiteSpace: 'nowrap',
                        bgcolor: s.violationStatus === 'Safe' ? '#f0fdf4' : '#fffbeb',
                        color: s.violationStatus === 'Safe' ? '#16a34a' : '#d97706',
                        border: s.violationStatus === 'Safe' ? '1px solid #bbf7d0' : '1px solid #fde68a'
                      }}
                    />
                  </TableCell>
                  <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                    <Stack direction="row" spacing={1} justifyContent="flex-end">
                      <Button
                        size="small"
                        startIcon={<VisibilityRoundedIcon sx={{ fontSize: '16px !important' }} />}
                        onClick={() => setActiveStreamModal(s)}
                        sx={adminBtnStyles.transSecondary}
                      >
                        Giám Sát
                      </Button>
                      <Button
                        size="small"
                        startIcon={<BlockRoundedIcon sx={{ fontSize: '16px !important' }} />}
                        onClick={() => setKillConfirmModal(s)}
                        sx={adminBtnStyles.danger}
                      >
                        Kill-Switch
                      </Button>
                    </Stack>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>

      {/* MODAL 1: GIÁM SÁT PHÒNG LIVE REALTIME & GỬI CẢNH BÁO */}
      <Dialog
        open={Boolean(activeStreamModal)}
        onClose={() => setActiveStreamModal(null)}
        maxWidth="md"
        fullWidth
        PaperProps={{ sx: { borderRadius: '16px', p: 1 } }}
      >
        <DialogTitle sx={{ fontWeight: 800, display: 'flex', justifyContent: 'space-between', alignItems: 'center', whiteSpace: 'nowrap' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, overflow: 'hidden', textOverflow: 'ellipsis' }}>
            <LiveTvRoundedIcon sx={{ color: '#ef4444' }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
              Giám Sát: {activeStreamModal?.title}
            </Typography>
          </Box>
          <Chip label={`${activeStreamModal?.viewers.toLocaleString()} CCU`} color="error" size="small" sx={{ fontWeight: 700, whiteSpace: 'nowrap' }} />
        </DialogTitle>
        <DialogContent dividers>
          {activeStreamModal && (
            <Grid container spacing={2.5}>
              {/* Stream Video Preview */}
              <Grid item xs={12} md={7}>
                <Box sx={{ position: 'relative', borderRadius: '12px', overflow: 'hidden', bgcolor: '#000', height: 280 }}>
                  <Box
                    component="img"
                    src={activeStreamModal.thumbnail}
                    sx={{ width: '100%', height: '100%', objectFit: 'cover', opacity: 0.85 }}
                  />
                  <Box sx={{ position: 'absolute', top: 12, left: 12, display: 'flex', gap: 1 }}>
                    <Chip label="LIVE STREAMING" color="error" size="small" sx={{ fontWeight: 800, height: 22, whiteSpace: 'nowrap' }} />
                    <Chip label={activeStreamModal.bitrate} size="small" sx={{ bgcolor: 'rgba(0,0,0,0.6)', color: '#fff', height: 22, whiteSpace: 'nowrap' }} />
                  </Box>
                  <Box sx={{ position: 'absolute', bottom: 12, left: 12, right: 12, p: 1.5, bgcolor: 'rgba(0,0,0,0.7)', borderRadius: '8px', color: '#fff' }}>
                    <Typography variant="body2" sx={{ fontWeight: 700, whiteSpace: 'nowrap' }}>Host: {activeStreamModal.streamer}</Typography>
                    <Typography variant="caption" sx={{ color: '#cbd5e1', whiteSpace: 'nowrap' }}>Gian hàng: {activeStreamModal.shop}</Typography>
                  </Box>
                </Box>
              </Grid>

              {/* Chat & Moderation Controls */}
              <Grid item xs={12} md={5}>
                <Stack spacing={2} sx={{ height: '100%' }}>
                  <Box sx={{ p: 1.5, bgcolor: '#f8fafc', borderRadius: '10px', border: '1px solid #e2e8f0' }}>
                    <Typography variant="caption" sx={{ fontWeight: 700, color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>THÔNG SỐ PHIÊN LIVE</Typography>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 1 }}>
                      <Typography variant="body2" sx={{ whiteSpace: 'nowrap' }}>Doanh thu chốt:</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 800, color: '#16a34a', whiteSpace: 'nowrap' }}>{activeStreamModal.gmv}</Typography>
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 0.5 }}>
                      <Typography variant="body2" sx={{ whiteSpace: 'nowrap' }}>Số đơn thành công:</Typography>
                      <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{activeStreamModal.orders} đơn</Typography>
                    </Box>
                  </Box>

                  {/* Send In-stream Warning Message */}
                  <Box>
                    <Typography variant="caption" sx={{ fontWeight: 700, color: '#ef4444', whiteSpace: 'nowrap', display: 'block' }}>GỬI CẢNH BÁO TRỰC TIẾP TỚI HOST</Typography>
                    <TextField
                      fullWidth
                      size="small"
                      multiline
                      rows={2}
                      placeholder="Nhập nội dung cảnh báo vi phạm quy chuẩn hiển thị trên màn hình host..."
                      value={warningMessage}
                      onChange={(e) => setWarningMessage(e.target.value)}
                      sx={{ mt: 1, '& .MuiOutlinedInput-root': { borderRadius: '10px' } }}
                    />
                    <Button
                      fullWidth
                      startIcon={<SendRoundedIcon />}
                      onClick={() => handleSendWarning(activeStreamModal)}
                      sx={adminBtnStyles.warning}
                    >
                      Bắn Cảnh Báo Vi Phạm
                    </Button>
                  </Box>
                </Stack>
              </Grid>
            </Grid>
          )}
        </DialogContent>
        <DialogActions sx={{ p: 2, justifyContent: 'space-between' }}>
          <Button onClick={() => setActiveStreamModal(null)} sx={{ textTransform: 'none', color: '#64748b', fontWeight: 600, whiteSpace: 'nowrap' }}>Đóng</Button>
          <Button
            startIcon={<BlockRoundedIcon />}
            onClick={() => {
              setKillConfirmModal(activeStreamModal)
            }}
            sx={adminBtnStyles.danger}
          >
            Ngắt Phòng Khẩn Cấp (Kill-Switch)
          </Button>
        </DialogActions>
      </Dialog>

      {/* MODAL 2: CONFIRM KILL SWITCH */}
      <Dialog
        open={Boolean(killConfirmModal)}
        onClose={() => setKillConfirmModal(null)}
        maxWidth="xs"
        fullWidth
        PaperProps={{ sx: { borderRadius: '16px', p: 1 } }}
      >
        <DialogTitle sx={{ fontWeight: 800, color: '#ef4444', display: 'flex', alignItems: 'center', gap: 1, whiteSpace: 'nowrap' }}>
          <BlockRoundedIcon />
          <span>Kích Hoạt Kill-Switch?</span>
        </DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ color: '#475569' }}>
            Hành động này sẽ <strong>ngắt kết nối luồng WebRTC lập tức</strong>, đẩy toàn bộ người xem ra khỏi phòng và tạm khóa tính năng phát trực tiếp của shop <strong>{killConfirmModal?.shop}</strong>.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setKillConfirmModal(null)} sx={{ textTransform: 'none', fontWeight: 600, color: '#64748b', whiteSpace: 'nowrap' }}>Hủy</Button>
          <Button
            onClick={() => handleKillSwitch(killConfirmModal)}
            sx={adminBtnStyles.danger}
          >
            Xác Nhận Ngắt Ngay
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default LiveModerationTab
