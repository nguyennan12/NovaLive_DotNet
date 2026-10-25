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
  Grid,
  Stack,
  TextField,
  InputAdornment,
  Avatar,
  Divider
} from '@mui/material'
import AccessTimeRoundedIcon from '@mui/icons-material/AccessTimeRounded'
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import CancelRoundedIcon from '@mui/icons-material/CancelRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import WhatshotRoundedIcon from '@mui/icons-material/WhatshotRounded'
import FlashOnRoundedIcon from '@mui/icons-material/FlashOnRounded'
import { toast } from 'react-toastify'
import { adminBtnStyles } from '../../theme/adminButtonStyles'

const FLASH_SALE_SLOTS = [
  { id: 'slot-1', title: '00:00 - 09:00', label: 'Cú Đêm Săn Sale', registeredCount: 48, status: 'Completed', color: 'default' },
  { id: 'slot-2', title: '12:00 - 15:00', label: 'Deal Trưa Giảm Sâu', registeredCount: 86, status: 'Completed', color: 'default' },
  { id: 'slot-3', title: '20:00 - 24:00', label: 'Giờ Vàng Live Shopper', registeredCount: 142, status: 'Active', color: 'error' }
]

const INITIAL_FLASH_PRODUCTS = [
  {
    id: 'fp-1',
    productName: 'Tai Nghe Bluetooth True Wireless TWS Bass Chống Ồn',
    image: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=200&auto=format&fit=crop&q=80',
    shopName: 'TechLab Gaming Gears',
    originalPrice: '590,000 đ',
    flashPrice: '299,000 đ',
    discountPercent: 49,
    quantity: 100,
    slotTitle: '20:00 - 24:00 (Giờ Vàng Live Shopper)',
    status: 'Pending'
  },
  {
    id: 'fp-2',
    productName: 'Áo Thun Unisex Cotton 100% Co Giãn 4 Chiều In Họa Tiết',
    image: 'https://images.unsplash.com/photo-1521572267360-ee0c2909d518?w=200&auto=format&fit=crop&q=80',
    shopName: 'Fashion Forward Store',
    originalPrice: '250,000 đ',
    flashPrice: '129,000 đ',
    discountPercent: 48,
    quantity: 250,
    slotTitle: '20:00 - 24:00 (Giờ Vàng Live Shopper)',
    status: 'Pending'
  },
  {
    id: 'fp-3',
    productName: 'Bộ Nồi Inox 5 Đáy Nắp Kính Cường Lực Cao Cấp',
    image: 'https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?w=200&auto=format&fit=crop&q=80',
    shopName: 'Gia Dụng Nova VIP',
    originalPrice: '1,200,000 đ',
    flashPrice: '690,000 đ',
    discountPercent: 42,
    quantity: 80,
    slotTitle: '20:00 - 24:00 (Giờ Vàng Live Shopper)',
    status: 'Approved'
  },
  {
    id: 'fp-4',
    productName: 'Son Kem Lì Mịn Môi Không Khô Dưỡng Ẩm 24h',
    image: 'https://images.unsplash.com/photo-1586495777744-4413f21062fa?w=200&auto=format&fit=crop&q=80',
    shopName: 'Mỹ Phẩm Glow & Care',
    originalPrice: '380,000 đ',
    flashPrice: '340,000 đ',
    discountPercent: 10,
    quantity: 50,
    slotTitle: '20:00 - 24:00 (Giờ Vàng Live Shopper)',
    status: 'Rejected',
    rejectReason: 'Mức giảm chưa đạt tiêu chuẩn sàn (Tối thiểu 20%)'
  }
]

export const FlashSaleManagementTab = () => {
  const [products, setProducts] = useState(INITIAL_FLASH_PRODUCTS)
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState('ALL')

  const handleApprove = (id, name) => {
    setProducts(prev => prev.map(p => p.id === id ? { ...p, status: 'Approved' } : p))
    toast.success(`Đã duyệt sản phẩm tham gia Flash Sale: ${name}`)
  }

  const handleReject = (id, name) => {
    setProducts(prev => prev.map(p => p.id === id ? { ...p, status: 'Rejected', rejectReason: 'Không đạt tiêu chí giảm sâu của chiến dịch' } : p))
    toast.error(`Đã từ chối duyệt Flash Sale: ${name}`)
  }

  const filteredProducts = products.filter(p => {
    if (statusFilter === 'PENDING' && p.status !== 'Pending') return false
    if (statusFilter === 'APPROVED' && p.status !== 'Approved') return false
    if (statusFilter === 'REJECTED' && p.status !== 'Rejected') return false
    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase()
      return p.productName.toLowerCase().includes(q) || p.shopName.toLowerCase().includes(q)
    }
    return true
  })

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, width: '100%' }}>
      {/* 3 Flash Sale Slots Header */}
      <Grid container spacing={2.5}>
        {FLASH_SALE_SLOTS.map((slot) => (
          <Grid item xs={12} md={4} key={slot.id}>
            <Card
              sx={{
                p: 2.5,
                borderRadius: '16px',
                border: slot.status === 'Active' ? '2px solid #ef4444' : '1px solid #e2e8f0',
                bgcolor: '#ffffff',
                boxShadow: '0 2px 10px rgba(0,0,0,0.02)',
                position: 'relative',
                overflow: 'hidden'
              }}
            >
              {slot.status === 'Active' && (
                <Box sx={{ position: 'absolute', top: 0, right: 0, bgcolor: '#ef4444', color: '#fff', px: 1.5, py: 0.3, fontSize: '0.65rem', fontWeight: 800, borderBottomLeftRadius: '8px', whiteSpace: 'nowrap' }}>
                  ĐANG DIỄN RA
                </Box>
              )}
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1 }}>
                <Box sx={{ p: 1, borderRadius: '10px', bgcolor: slot.status === 'Active' ? 'rgba(239, 68, 68, 0.1)' : '#f1f5f9', color: slot.status === 'Active' ? '#ef4444' : '#64748b', flexShrink: 0 }}>
                  <AccessTimeRoundedIcon />
                </Box>
                <Box>
                  <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>{slot.title}</Typography>
                  <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>{slot.label}</Typography>
                </Box>
              </Box>
              <Divider sx={{ my: 1.5 }} />
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>Sản phẩm đã duyệt:</Typography>
                <Typography variant="body2" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>{slot.registeredCount} sản phẩm</Typography>
              </Box>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Product Approval Table */}
      <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)', width: '100%' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2.5, flexWrap: 'wrap', gap: 1.5 }}>
          <Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <FlashOnRoundedIcon sx={{ color: '#ef4444', fontSize: 24 }} />
              <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
                Duyệt Sản Phẩm Đăng Ký Flash Sale Toàn Sàn
              </Typography>
            </Box>
            <Typography variant="caption" sx={{ color: '#64748b' }}>
              Kiểm tra mức giá chiết khấu (tối thiểu giảm từ 20% trở lên) và số lượng cam kết trong kho của Shop
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ alignItems: 'center' }}>
            {[
              { key: 'ALL', label: `Tất cả (${products.length})` },
              { key: 'PENDING', label: `Chờ duyệt (${products.filter(p => p.status === 'Pending').length})`, isPending: true },
              { key: 'APPROVED', label: `Đã duyệt (${products.filter(p => p.status === 'Approved').length})` },
              { key: 'REJECTED', label: `Từ chối (${products.filter(p => p.status === 'Rejected').length})` }
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

        {/* Search */}
        <Box sx={{ mb: 2 }}>
          <TextField
            size="small"
            placeholder="Tìm theo tên sản phẩm, tên gian hàng..."
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
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Sản Phẩm & Shop</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Khung Giờ</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Giá Gốc / Giá Flash</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Mức Giảm (%)</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Số Lượng Cam Kết</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Trạng Thái</TableCell>
                <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Thao Tác</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {filteredProducts.map((p) => (
                <TableRow key={p.id} hover>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                      <Avatar variant="rounded" src={p.image} sx={{ width: 44, height: 44, borderRadius: '8px', border: '1px solid #e2e8f0' }} />
                      <Box>
                        <Typography
                          variant="body2"
                          sx={{
                            fontWeight: 700,
                            color: '#0f172a',
                            maxWidth: 260,
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            whiteSpace: 'nowrap'
                          }}
                          title={p.productName}
                        >
                          {p.productName}
                        </Typography>
                        <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>
                          Shop: {p.shopName}
                        </Typography>
                      </Box>
                    </Box>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}><Typography variant="caption" sx={{ fontWeight: 600, color: '#475569', whiteSpace: 'nowrap' }}>{p.slotTitle}</Typography></TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ textDecoration: 'line-through', color: '#94a3b8', fontSize: '0.8rem', whiteSpace: 'nowrap' }}>
                      {p.originalPrice}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 800, color: '#ef4444', whiteSpace: 'nowrap' }}>
                      {p.flashPrice}
                    </Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Chip
                      icon={<WhatshotRoundedIcon sx={{ fontSize: '14px !important', color: '#ef4444' }} />}
                      label={`-${p.discountPercent}%`}
                      size="small"
                      sx={{ fontWeight: 800, bgcolor: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', height: 24, fontSize: '0.75rem', whiteSpace: 'nowrap' }}
                    />
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}><Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{p.quantity} slot</Typography></TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Chip
                      label={p.status === 'Approved' ? 'Đã duyệt' : p.status === 'Pending' ? 'Chờ duyệt' : 'Từ chối'}
                      size="small"
                      sx={{
                        fontWeight: 700,
                        height: 24,
                        fontSize: '0.72rem',
                        whiteSpace: 'nowrap',
                        bgcolor: p.status === 'Approved' ? '#f0fdf4' : p.status === 'Pending' ? '#fffbeb' : '#fef2f2',
                        color: p.status === 'Approved' ? '#16a34a' : p.status === 'Pending' ? '#d97706' : '#dc2626',
                        border: p.status === 'Approved' ? '1px solid #bbf7d0' : p.status === 'Pending' ? '1px solid #fde68a' : '1px solid #fecaca'
                      }}
                    />
                  </TableCell>
                  <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                    {p.status === 'Pending' ? (
                      <Stack direction="row" spacing={1} justifyContent="flex-end">
                        <Button
                          size="small"
                          variant="contained"
                          startIcon={<CheckCircleRoundedIcon sx={{ fontSize: '16px !important' }} />}
                          onClick={() => handleApprove(p.id, p.productName)}
                          sx={{
                            ...adminBtnStyles.success,
                            textTransform: 'none',
                            whiteSpace: 'nowrap'
                          }}
                        >
                          Duyệt
                        </Button>
                        <Button
                          size="small"
                          variant="outlined"
                          startIcon={<CancelRoundedIcon sx={{ fontSize: '16px !important' }} />}
                          onClick={() => handleReject(p.id, p.productName)}
                          sx={{
                            ...adminBtnStyles.transDanger,
                            textTransform: 'none',
                            whiteSpace: 'nowrap'
                          }}
                        >
                          Từ chối
                        </Button>
                      </Stack>
                    ) : (
                      <Typography variant="caption" sx={{ color: p.status === 'Approved' ? '#16a34a' : '#ef4444', fontWeight: 700, whiteSpace: 'nowrap' }}>
                        {p.status === 'Approved' ? 'Đã lên sóng' : 'Đã bác bỏ'}
                      </Typography>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>
    </Box>
  )
}

export default FlashSaleManagementTab
