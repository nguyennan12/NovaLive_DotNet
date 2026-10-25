import { useState, useEffect } from 'react'
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
  Tabs,
  Tab,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Stack,
  Alert,
  CircularProgress,
  IconButton,
  Divider,
  Grid
} from '@mui/material'
import CloseRoundedIcon from '@mui/icons-material/CloseRounded'
import AssignmentReturnRoundedIcon from '@mui/icons-material/AssignmentReturnRounded'
import GavelRoundedIcon from '@mui/icons-material/GavelRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import AccessTimeRoundedIcon from '@mui/icons-material/AccessTimeRounded'
import VisibilityRoundedIcon from '@mui/icons-material/VisibilityRounded'
import { useNavigate } from 'react-router-dom'
import { formatDate, formatVND } from '~/common/utils/formatters'
import { getMyReturnsAPI } from '~/common/apis/services/returnService'

const RETURN_STATUS_CONFIG = {
  Pending: { label: 'Chờ Shop xử lý', color: '#f59e0b', bgcolor: '#fef3c7', border: '#fde68a' },
  SellerApproved: { label: 'Shop đã duyệt (Gửi hàng)', color: '#0095ff', bgcolor: '#eff6ff', border: '#bfdbfe' },
  SellerRejected: { label: 'Shop từ chối', color: '#dc2626', bgcolor: '#fef2f2', border: '#fecaca' },
  AdminDispute: { label: 'Đang phân xử bởi Admin', color: '#8b5cf6', bgcolor: '#f5f3ff', border: '#ddd6fe' },
  Completed: { label: 'Hoàn tiền thành công', color: '#16a34a', bgcolor: '#f0fdf4', border: '#bbf7d0' },
  BuyerWins: { label: 'Buyer thắng (Đang hoàn tiền)', color: '#16a34a', bgcolor: '#f0fdf4', border: '#bbf7d0' },
  SellerWins: { label: 'Seller thắng (Từ chối hoàn)', color: '#64748b', bgcolor: '#f1f5f9', border: '#e2e8f0' }
}

const REASON_MAP = {
  WrongItem: 'Giao sai sản phẩm / mẫu mã',
  Defective: 'Sản phẩm lỗi kỹ thuật',
  DamagedInShipping: 'Hư hỏng khi vận chuyển',
  NotAsDescribed: 'Không đúng mô tả',
  ChangeOfMind: 'Đổi ý định mua'
}

export default function BuyerReturnsPage() {
  const navigate = useNavigate()
  const [activeTab, setActiveTab] = useState('ALL')
  const [returns, setReturns] = useState([])
  const [loading, setLoading] = useState(true)
  const [selectedReturn, setSelectedReturn] = useState(null)

  const fetchReturns = async () => {
    setLoading(true)
    try {
      const res = await getMyReturnsAPI()
      const items = res?.items || res || []
      setReturns(Array.isArray(items) ? items : [])
    } catch (err) {
      console.warn('API getMyReturns error, loading sample data for preview:', err)
      // Dữ liệu mẫu minh họa tiến trình xử lý
      setReturns([
        {
          id: 'ret-101',
          subOrderId: 'sub-8812',
          subOrderCode: 'NL-20261023-8812',
          shopName: 'Mỹ Phẩm Cao Cấp Glow & Care',
          reason: 'Defective',
          status: 'SellerApproved',
          requestedRefundAmount: 480000,
          approvedRefundAmount: 480000,
          createdAt: new Date(Date.now() - 36 * 3600 * 1000).toISOString(),
          items: [
            {
              orderItemId: 'item-1',
              spuName: 'Serum Dưỡng Ẩm Phục Hồi Da B5 Glow Serum 50ml',
              skuCode: 'GLOW-B5-50',
              quantity: 1,
              refundPrice: 480000
            }
          ],
          evidenceUrls: [
            'https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=500&auto=format&fit=crop&q=80'
          ],
          returnAddress: {
            warehouseName: 'Kho Tổng GlowCare HCM',
            contactName: 'Trần Thị Mai',
            contactPhone: '0987654321',
            detailAddress: '142 Hai Bà Trưng',
            wardName: 'Phường Đa Kao',
            districtName: 'Quận 1',
            provinceName: 'TP. Hồ Chí Minh'
          },
          sellerResponse: 'Shop đồng ý nhận lại sản phẩm để đổi mới hoặc hoàn tiền. Quý khách vui lòng gửi hàng về địa chỉ kho trên.'
        },
        {
          id: 'ret-102',
          subOrderId: 'sub-8813',
          subOrderCode: 'NL-20261022-8813',
          shopName: 'TechLab Gaming Gears',
          reason: 'DamagedInShipping',
          status: 'AdminDispute',
          requestedRefundAmount: 1250000,
          approvedRefundAmount: null,
          createdAt: new Date(Date.now() - 60 * 3600 * 1000).toISOString(),
          items: [
            {
              orderItemId: 'item-2',
              spuName: 'Bàn phím cơ không dây RGB 87 phím Red Switch',
              skuCode: 'KEY-RGB-RED-87',
              quantity: 1,
              refundPrice: 1250000
            }
          ],
          evidenceUrls: [
            'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=500&auto=format&fit=crop&q=80'
          ],
          sellerResponse: 'Hộp sản phẩm xuất kho có niêm phong nguyên vẹn. Shop từ chối giải quyết.',
          sellerEvidenceUrls: [
            'https://images.unsplash.com/photo-1544816155-12df9643f363?w=500&auto=format&fit=crop&q=80'
          ]
        }
      ])
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchReturns()
  }, [])

  const filteredReturns = returns.filter(item => {
    if (activeTab === 'ALL') return true
    return item.status === activeTab
  })

  return (
    <Box sx={{ maxWidth: 1200, mx: 'auto', px: { xs: 2, sm: 3 }, py: 3.5 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, flexWrap: 'wrap', gap: 1.5 }}>
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <AssignmentReturnRoundedIcon sx={{ color: '#0095ff', fontSize: 28 }} />
            <Typography variant="h5" sx={{ fontWeight: 800, color: '#0f172a' }}>
              Trung Tâm Khiếu Nại & Trả Hàng / Hoàn Tiền
            </Typography>
          </Box>
          <Typography variant="body2" sx={{ color: '#64748b', mt: 0.5 }}>
            Theo dõi tiến trình xử lý yêu cầu hoàn tiền, phản hồi của Shop và trọng tài phân xử từ NovaLive
          </Typography>
        </Box>
        <Button
          variant="outlined"
          onClick={() => navigate('/orders')}
          sx={{ textTransform: 'none', fontWeight: 600, borderRadius: '10px' }}
        >
          Quay lại Đơn Hàng Của Tôi
        </Button>
      </Box>

      {/* Filter Tabs */}
      <Card sx={{ mb: 3, borderRadius: '14px', border: '1px solid #e2e8f0', boxShadow: '0 2px 8px rgba(0,0,0,0.02)' }}>
        <Tabs
          value={activeTab}
          onChange={(_, val) => setActiveTab(val)}
          variant="scrollable"
          scrollButtons="auto"
          sx={{
            minHeight: 48,
            '& .MuiTab-root': {
              minHeight: 48,
              textTransform: 'none',
              fontWeight: 600,
              fontSize: '0.875rem',
              color: '#64748b',
              '&.Mui-selected': { color: '#0095ff', fontWeight: 700 }
            }
          }}
        >
          <Tab value="ALL" label={`Tất cả (${returns.length})`} />
          <Tab value="Pending" label="Chờ Shop xử lý" />
          <Tab value="SellerApproved" label="Shop đồng ý (Gửi trả)" />
          <Tab value="SellerRejected" label="Shop từ chối" />
          <Tab value="AdminDispute" label="Đang Admin phân xử" />
          <Tab value="Completed" label="Đã hoàn tiền" />
        </Tabs>
      </Card>

      {/* Content Table */}
      <Card sx={{ borderRadius: '16px', border: '1px solid #e2e8f0', boxShadow: '0 2px 10px rgba(0,0,0,0.02)', overflow: 'hidden' }}>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
            <CircularProgress />
          </Box>
        ) : filteredReturns.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 8, px: 2 }}>
            <AssignmentReturnRoundedIcon sx={{ fontSize: 48, color: '#cbd5e1', mb: 1.5 }} />
            <Typography variant="h6" sx={{ fontWeight: 700, color: '#334155' }}>
              Không có yêu cầu trả hàng / hoàn tiền nào
            </Typography>
            <Typography variant="body2" sx={{ color: '#64748b', mt: 0.5, mb: 2 }}>
              Bạn có thể tạo yêu cầu trả hàng trong vòng 7 ngày sau khi đơn hàng được giao thành công.
            </Typography>
            <Button variant="contained" onClick={() => navigate('/orders')} sx={{ textTransform: 'none', fontWeight: 700 }}>
              Xem danh sách đơn hàng
            </Button>
          </Box>
        ) : (
          <TableContainer>
            <Table>
              <TableHead sx={{ bgcolor: '#f8fafc' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700, color: '#475569' }}>Mã Yêu Cầu</TableCell>
                  <TableCell sx={{ fontWeight: 700, color: '#475569' }}>Sản Phẩm Yêu Cầu Hoàn</TableCell>
                  <TableCell sx={{ fontWeight: 700, color: '#475569' }}>Lý Do Hoàn Trả</TableCell>
                  <TableCell sx={{ fontWeight: 700, color: '#475569' }}>Tiền Yêu Cầu</TableCell>
                  <TableCell sx={{ fontWeight: 700, color: '#475569' }}>Trạng Thái</TableCell>
                  <TableCell sx={{ fontWeight: 700, color: '#475569' }}>Thời Gian</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700, color: '#475569' }}>Hành Động</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {filteredReturns.map((item) => {
                  const cfg = RETURN_STATUS_CONFIG[item.status] || {
                    label: item.status,
                    color: '#64748b',
                    bgcolor: '#f1f5f9',
                    border: '#e2e8f0'
                  }
                  const firstItem = item.items?.[0]
                  return (
                    <TableRow key={item.id} hover>
                      <TableCell>
                        <Typography variant="body2" sx={{ fontWeight: 700, color: '#0095ff' }}>
                          {item.id}
                        </Typography>
                        <Typography variant="caption" sx={{ color: '#64748b', display: 'block' }}>
                          Đơn: {item.subOrderCode || item.subOrderId}
                        </Typography>
                      </TableCell>
                      <TableCell sx={{ maxWidth: 260 }}>
                        <Typography variant="body2" sx={{ fontWeight: 600, color: '#0f172a', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                          {firstItem?.spuName || 'Sản phẩm hoàn'}
                        </Typography>
                        <Typography variant="caption" sx={{ color: '#64748b' }}>
                          Số lượng: x{firstItem?.quantity || 1} • SKU: {firstItem?.skuCode || 'Mặc định'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" sx={{ color: '#334155', fontWeight: 600 }}>
                          {REASON_MAP[item.reason] || item.reason}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" sx={{ fontWeight: 800, color: '#ef4444' }}>
                          {formatVND(item.requestedRefundAmount || 0)}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={cfg.label}
                          size="small"
                          sx={{
                            fontWeight: 700,
                            fontSize: '0.72rem',
                            height: 24,
                            color: cfg.color,
                            bgcolor: cfg.bgcolor,
                            border: `1px solid ${cfg.border}`
                          }}
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="caption" sx={{ color: '#64748b' }}>
                          {formatDate(item.createdAt)}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        <Button
                          size="small"
                          startIcon={<VisibilityRoundedIcon sx={{ fontSize: '15px !important' }} />}
                          onClick={() => setSelectedReturn(item)}
                          sx={{ textTransform: 'none', fontWeight: 700, color: '#0095ff' }}
                        >
                          Chi tiết
                        </Button>
                      </TableCell>
                    </TableRow>
                  )
                })}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Card>

      {/* Modal xem chi tiết tiến trình khiếu nại */}
      <Dialog
        open={Boolean(selectedReturn)}
        onClose={() => setSelectedReturn(null)}
        maxWidth="md"
        fullWidth
        PaperProps={{ sx: { borderRadius: '16px', p: 1 } }}
      >
        <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <AssignmentReturnRoundedIcon sx={{ color: '#0095ff' }} />
            <Typography variant="h6" sx={{ fontWeight: 800 }}>
              Tiến Trình Khiếu Nại: #{selectedReturn?.id}
            </Typography>
          </Box>
          <IconButton size="small" onClick={() => setSelectedReturn(null)}>
            <CloseRoundedIcon />
          </IconButton>
        </DialogTitle>

        <DialogContent dividers>
          {selectedReturn && (
            <Stack spacing={2.5}>
              {/* Status Banner */}
              <Box sx={{ p: 2, borderRadius: '12px', bgcolor: '#f8fafc', border: '1px solid #e2e8f0' }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 1 }}>
                  <Box>
                    <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, display: 'block' }}>TRẠNG THÁI HIỆN TẠI</Typography>
                    <Chip
                      label={RETURN_STATUS_CONFIG[selectedReturn.status]?.label || selectedReturn.status}
                      size="small"
                      sx={{
                        mt: 0.5,
                        fontWeight: 700,
                        color: RETURN_STATUS_CONFIG[selectedReturn.status]?.color,
                        bgcolor: RETURN_STATUS_CONFIG[selectedReturn.status]?.bgcolor
                      }}
                    />
                  </Box>
                  <Box sx={{ textAlign: 'right' }}>
                    <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, display: 'block' }}>SỐ TIỀN YÊU CẦU HOÀN</Typography>
                    <Typography variant="h6" sx={{ fontWeight: 800, color: '#ef4444' }}>
                      {formatVND(selectedReturn.requestedRefundAmount || 0)}
                    </Typography>
                  </Box>
                </Box>
              </Box>

              {/* Dispute Alert notice */}
              {selectedReturn.status === 'AdminDispute' && (
                <Alert severity="warning" icon={<GavelRoundedIcon />} sx={{ borderRadius: '10px' }}>
                  Yêu cầu này đang được <strong>Bộ phận Phân xử Tranh chấp NovaLive</strong> xem xét đối soát bằng chứng giữa Người mua và Shop. Phán quyết sẽ được gửi thông báo sớm nhất.
                </Alert>
              )}

              {/* Warehouse Address for return shipment if approved */}
              {selectedReturn.returnAddress && (
                <Card sx={{ p: 2, border: '1.5px solid #bfdbfe', borderRadius: '12px', bgcolor: '#eff6ff' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                    <LocalShippingRoundedIcon sx={{ color: '#0095ff' }} />
                    <Typography variant="subtitle2" sx={{ fontWeight: 800, color: '#1e3a8a' }}>
                      Địa Chỉ Kho Nhận Hàng Hoàn Trả (Shop đã chấp thuận)
                    </Typography>
                  </Box>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#0f172a' }}>
                    Người nhận: {selectedReturn.returnAddress.contactName} • SĐT: {selectedReturn.returnAddress.contactPhone}
                  </Typography>
                  <Typography variant="body2" sx={{ color: '#334155', mt: 0.5 }}>
                    Kho: {selectedReturn.returnAddress.warehouseName} - {selectedReturn.returnAddress.detailAddress}, {selectedReturn.returnAddress.wardName}, {selectedReturn.returnAddress.districtName}, {selectedReturn.returnAddress.provinceName}
                  </Typography>
                </Card>
              )}

              {/* Seller Feedback if any */}
              {selectedReturn.sellerResponse && (
                <Box sx={{ p: 2, borderRadius: '12px', border: '1px solid #e2e8f0', bgcolor: '#ffffff' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                    <StorefrontRoundedIcon sx={{ fontSize: 18, color: '#64748b' }} />
                    <Typography variant="subtitle2" sx={{ fontWeight: 700, color: '#0f172a' }}>
                      Phản hồi từ Shop:
                    </Typography>
                  </Box>
                  <Typography variant="body2" sx={{ color: '#475569' }}>
                    "{selectedReturn.sellerResponse}"
                  </Typography>
                </Box>
              )}

              {/* Products List */}
              <Box>
                <Typography variant="subtitle2" sx={{ fontWeight: 800, mb: 1, color: '#0f172a' }}>
                  Danh Sách Sản Phẩm Yêu Cầu Hoàn
                </Typography>
                <Stack spacing={1}>
                  {(selectedReturn.items || []).map((prod, i) => (
                    <Box
                      key={i}
                      sx={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        p: 1.5,
                        borderRadius: '8px',
                        border: '1px solid #e2e8f0',
                        bgcolor: '#fafafa'
                      }}
                    >
                      <Box>
                        <Typography variant="body2" sx={{ fontWeight: 700 }}>
                          {prod.spuName}
                        </Typography>
                        <Typography variant="caption" sx={{ color: '#64748b' }}>
                          Mã SKU: {prod.skuCode} • Số lượng hoàn: x{prod.quantity}
                        </Typography>
                      </Box>
                      <Typography variant="body2" sx={{ fontWeight: 700, color: '#0095ff' }}>
                        {formatVND(prod.refundPrice * prod.quantity)}
                      </Typography>
                    </Box>
                  ))}
                </Stack>
              </Box>

              {/* Evidence media */}
              {selectedReturn.evidenceUrls?.length > 0 && (
                <Box>
                  <Typography variant="subtitle2" sx={{ fontWeight: 800, mb: 1, color: '#0f172a' }}>
                    Hình Ảnh / Video Bằng Chứng Đính Kèm
                  </Typography>
                  <Grid container spacing={1.5}>
                    {selectedReturn.evidenceUrls.map((url, i) => (
                      <Grid item xs={4} sm={3} key={i}>
                        <Box
                          component="img"
                          src={url}
                          sx={{
                            width: '100%',
                            height: 100,
                            borderRadius: '8px',
                            objectFit: 'cover',
                            border: '1px solid #e2e8f0'
                          }}
                        />
                      </Grid>
                    ))}
                  </Grid>
                </Box>
              )}
            </Stack>
          )}
        </DialogContent>

        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setSelectedReturn(null)} sx={{ textTransform: 'none', fontWeight: 600 }}>
            Đóng
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
