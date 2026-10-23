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
  Stack,
  TextField,
  InputAdornment,
  Avatar,
  Rating
} from '@mui/material'
import RateReviewRoundedIcon from '@mui/icons-material/RateReviewRounded'
import VisibilityOffRoundedIcon from '@mui/icons-material/VisibilityOffRounded'
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import FlagRoundedIcon from '@mui/icons-material/FlagRounded'
import { toast } from 'react-toastify'
import { adminBtnStyles } from '../../theme/adminButtonStyles'

const INITIAL_REVIEWS = [
  {
    id: 'rev-1',
    productName: 'Tai Nghe Chống Ồn Wireless Pro Max - Màu Đen',
    productImage: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=100&auto=format&fit=crop&q=80',
    buyerName: 'nguyen_an99',
    shopName: 'TechLab Gaming Gears',
    rating: 1,
    comment: 'Shop lừa đảo, hàng nhận bị nứt màn hình kêu không đổi trả. Đừng ai mua ở đây lũ ăn cắp!',
    flagReason: 'Ngôn từ xúc phạm, thóa mạ uy tín',
    createdAt: '23/10/2026 10:20',
    status: 'Flagged'
  },
  {
    id: 'rev-2',
    productName: 'Áo Khoác Bomber Nam Da Lộn - Size XL',
    productImage: 'https://images.unsplash.com/photo-1551028719-00167b16eac5?w=100&auto=format&fit=crop&q=80',
    buyerName: 'tran_duc_vip',
    shopName: 'Fashion Forward Store',
    rating: 1,
    comment: 'Ai muốn mua áo đẹp qua Zalo 0988xxx giá chỉ 200k, vào link bit.ly/san-deal-re nhận quà',
    flagReason: 'Spam link lừa đảo ngoài sàn',
    createdAt: '22/10/2026 15:40',
    status: 'Flagged'
  },
  {
    id: 'rev-3',
    productName: 'Nồi Cơm Điện Cao Tần Dung Tích 1.8L',
    productImage: 'https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?w=100&auto=format&fit=crop&q=80',
    buyerName: 'mai_phuong_91',
    shopName: 'Gia Dụng Nova VIP',
    rating: 5,
    comment: 'Nồi nấu cơm rất dẻo và ngon, giao hàng GHN nhanh trong 2 ngày, đóng gói cẩn thận 10 điểm!',
    flagReason: '',
    createdAt: '21/10/2026 18:00',
    status: 'Approved'
  },
  {
    id: 'rev-4',
    productName: 'Kem Dưỡng Da Mặt Ban Đêm Cấp Ẩm Chuyên Sâu',
    productImage: 'https://images.unsplash.com/photo-1522337360788-8b13dee7a37e?w=100&auto=format&fit=crop&q=80',
    buyerName: 'hoang_yen_gl',
    shopName: 'Mỹ Phẩm Glow & Care',
    rating: 2,
    comment: 'Hàng xài bị kích ứng nổi mụn, đề nghị shop kiểm tra lại thành phần',
    flagReason: '',
    createdAt: '20/10/2026 09:30',
    status: 'Approved'
  }
]

export const ReviewsModerationTab = () => {
  const [reviews, setReviews] = useState(INITIAL_REVIEWS)
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState('ALL')

  const handleHideReview = (r) => {
    setReviews(prev => prev.map(item => item.id === r.id ? { ...item, status: 'Hidden' } : item))
    toast.error(`Đã ẩn đánh giá vi phạm của người dùng: ${r.buyerName}`)
  }

  const handleApproveReview = (r) => {
    setReviews(prev => prev.map(item => item.id === r.id ? { ...item, status: 'Approved', flagReason: '' } : item))
    toast.success(`Đã phê duyệt / gỡ cờ cho đánh giá của ${r.buyerName}`)
  }

  const filteredReviews = reviews.filter(r => {
    if (statusFilter === 'FLAGGED' && r.status !== 'Flagged') return false
    if (statusFilter === 'HIDDEN' && r.status !== 'Hidden') return false
    if (statusFilter === 'APPROVED' && r.status !== 'Approved') return false
    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase()
      return (
        r.productName.toLowerCase().includes(q) ||
        r.buyerName.toLowerCase().includes(q) ||
        r.shopName.toLowerCase().includes(q) ||
        r.comment.toLowerCase().includes(q)
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
            <RateReviewRoundedIcon sx={{ color: '#0095ff', fontSize: 24 }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
              Kiểm Duyệt Đánh Giá Sản Phẩm (Review Moderation)
            </Typography>
          </Box>
          <Typography variant="caption" sx={{ color: '#64748b' }}>
            Lọc và xử lý các đánh giá chứa spam, link độc hại hoặc ngôn từ xúc phạm thóa mạ
          </Typography>
        </Box>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ alignItems: 'center' }}>
          {[
            { key: 'ALL', label: `Tất cả (${reviews.length})` },
            { key: 'FLAGGED', label: `Bị gắn cờ (${reviews.filter(r => r.status === 'Flagged').length})`, isAlert: true },
            { key: 'HIDDEN', label: `Đã ẩn (${reviews.filter(r => r.status === 'Hidden').length})` },
            { key: 'APPROVED', label: `Đã duyệt (${reviews.filter(r => r.status === 'Approved').length})` }
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
          placeholder="Tìm theo sản phẩm, người mua, shop, nội dung..."
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
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Sản Phẩm Đánh Giá</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Khách Hàng & Shop</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Số Sao</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Nội Dung Đánh Giá</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Cảnh Báo Vi Phạm</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Trạng Thái</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Thao Tác</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredReviews.map((r) => (
              <TableRow key={r.id} hover>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Avatar variant="rounded" src={r.productImage} sx={{ width: 42, height: 42, borderRadius: '8px', border: '1px solid #e2e8f0' }} />
                    <Box sx={{ maxWidth: 220 }}>
                      <Typography
                        variant="body2"
                        sx={{
                          fontWeight: 700,
                          color: '#0f172a',
                          overflow: 'hidden',
                          textOverflow: 'ellipsis',
                          whiteSpace: 'nowrap'
                        }}
                        title={r.productName}
                      >
                        {r.productName}
                      </Typography>
                      <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>{r.createdAt}</Typography>
                    </Box>
                  </Box>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{r.buyerName}</Typography>
                  <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>Shop: {r.shopName}</Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Rating value={r.rating} readOnly size="small" sx={{ color: '#f59e0b' }} />
                </TableCell>
                <TableCell sx={{ minWidth: 220, maxWidth: 320 }}>
                  <Typography
                    variant="body2"
                    sx={{
                      color: '#334155',
                      fontSize: '0.82rem',
                      display: '-webkit-box',
                      WebkitLineClamp: 2,
                      WebkitBoxOrient: 'vertical',
                      overflow: 'hidden',
                      textOverflow: 'ellipsis',
                      lineHeight: 1.4
                    }}
                    title={r.comment}
                  >
                    "{r.comment}"
                  </Typography>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  {r.flagReason ? (
                    <Chip
                      icon={<FlagRoundedIcon sx={{ fontSize: '14px !important', color: '#ef4444' }} />}
                      label={r.flagReason}
                      size="small"
                      sx={{
                        height: 24,
                        fontSize: '0.72rem',
                        fontWeight: 700,
                        whiteSpace: 'nowrap',
                        bgcolor: '#fef2f2',
                        color: '#dc2626',
                        border: '1px solid #fecaca'
                      }}
                    />
                  ) : (
                    <Typography variant="caption" sx={{ color: '#94a3b8', whiteSpace: 'nowrap' }}>Bình thường</Typography>
                  )}
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Chip
                    label={r.status === 'Approved' ? 'Hiển thị' : r.status === 'Flagged' ? 'Cần duyệt' : 'Đã ẩn'}
                    size="small"
                    sx={{
                      height: 24,
                      fontSize: '0.72rem',
                      fontWeight: 700,
                      whiteSpace: 'nowrap',
                      bgcolor: r.status === 'Approved' ? '#f0fdf4' : r.status === 'Flagged' ? '#fef2f2' : '#f1f5f9',
                      color: r.status === 'Approved' ? '#16a34a' : r.status === 'Flagged' ? '#dc2626' : '#64748b',
                      border: r.status === 'Approved' ? '1px solid #bbf7d0' : r.status === 'Flagged' ? '1px solid #fecaca' : '1px solid #e2e8f0'
                    }}
                  />
                </TableCell>
                <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                  <Stack direction="row" spacing={1} justifyContent="flex-end">
                    {r.status !== 'Approved' && (
                      <Button
                        size="small"
                        variant="contained"
                        startIcon={<CheckCircleRoundedIcon sx={{ fontSize: '16px !important' }} />}
                        onClick={() => handleApproveReview(r)}
                        sx={{
                          ...adminBtnStyles.success,
                          textTransform: 'none',
                          whiteSpace: 'nowrap'
                        }}
                      >
                        Duyệt
                      </Button>
                    )}
                    {r.status !== 'Hidden' && (
                      <Button
                        size="small"
                        variant="outlined"
                        startIcon={<VisibilityOffRoundedIcon sx={{ fontSize: '16px !important' }} />}
                        onClick={() => handleHideReview(r)}
                        sx={{
                          ...adminBtnStyles.transDanger,
                          textTransform: 'none',
                          whiteSpace: 'nowrap'
                        }}
                      >
                        Ẩn
                      </Button>
                    )}
                  </Stack>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Card>
  )
}

export default ReviewsModerationTab
