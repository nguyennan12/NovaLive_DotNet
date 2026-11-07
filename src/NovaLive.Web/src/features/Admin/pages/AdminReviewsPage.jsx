import { useState } from 'react'
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
  Typography
} from '@mui/material'
import BlockRoundedIcon from '@mui/icons-material/BlockRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import { toast } from 'react-toastify'
import { hideReviewAPI } from '~/common/apis/services/adminService'
import authorizedAxiosInstance from '~/common/apis/custom/authorizeAxios'
import { PageHeader } from '~/features/Seller/components/shared/PageHeader'
import { SellerCard } from '~/features/Seller/components/shared/SellerCard'
import { DataTable } from '~/features/Seller/components/shared/DataTable'

export const AdminReviewsPage = () => {
  const [spuId, setSpuId] = useState('')
  const [loading, setLoading] = useState(false)
  const [reviews, setReviews] = useState([])
  const [searched, setSearched] = useState(false)

  const [hideDialogReview, setHideDialogReview] = useState(null)
  const [hideReason, setHideReason] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const handleSearchReviews = async (e) => {
    e.preventDefault()
    if (!spuId.trim()) {
      toast.error('Vui lòng nhập mã sản phẩm SPU')
      return
    }

    setLoading(true)
    setSearched(true)
    try {
      const res = await authorizedAxiosInstance.get(`products/${spuId.trim()}/reviews`)
      const list = res?.items || (Array.isArray(res) ? res : res?.data || [])
      setReviews(list)
    } catch (err) {
      setReviews([])
      toast.error('Không tìm thấy đánh giá cho sản phẩm này')
    } finally {
      setLoading(false)
    }
  }

  const handleExecuteHide = async () => {
    if (!hideDialogReview) return
    if (!hideReason.trim()) {
      toast.error('Vui lòng nhập lý do ẩn đánh giá')
      return
    }

    setIsSubmitting(true)
    try {
      await hideReviewAPI(hideDialogReview.id, { reason: hideReason.trim() })
      toast.success('Đã ẩn đánh giá vi phạm thành công!')
      setReviews((prev) => prev.filter((r) => r.id !== hideDialogReview.id))
      setHideDialogReview(null)
      setHideReason('')
    } catch (err) {
      toast.error(err?.response?.data?.detail || 'Thao tác ẩn đánh giá thất bại')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <>
      <PageHeader
        title="Kiểm duyệt đánh giá sản phẩm"
        description="Tra cứu và ẩn các bài đánh giá vi phạm tiêu chuẩn cộng đồng theo mã sản phẩm (SPU)."
      />

      <SellerCard title="Tra cứu đánh giá theo mã sản phẩm">
        <Box component="form" onSubmit={handleSearchReviews} sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mb: 3 }}>
          <TextField
            size="small"
            placeholder="Nhập mã sản phẩm (SPU ID)..."
            value={spuId}
            onChange={(e) => setSpuId(e.target.value)}
            sx={{ flex: 1, minWidth: 260 }}
          />
          <Button
            type="submit"
            variant="contained"
            color="secondary"
            startIcon={<SearchRoundedIcon fontSize="small" />}
            disabled={loading || !spuId.trim()}
            sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
          >
            Tìm đánh giá
          </Button>
        </Box>

        <DataTable
          rows={reviews}
          rowKey={(row) => row.id}
          loading={loading}
          error={null}
          onRetry={() => {}}
          emptyText={searched ? 'Sản phẩm này chưa có bài đánh giá nào.' : 'Nhập mã sản phẩm (SPU ID) ở trên để tra cứu danh sách đánh giá.'}
          columns={[
            {
              key: 'user',
              label: 'Người đánh giá',
              render: (row) => (
                <Typography variant="body2" sx={{ fontWeight: 600 }}>
                  {row.userName || row.user || 'Khách hàng'}
                </Typography>
              )
            },
            {
              key: 'rating',
              label: 'Điểm sao',
              render: (row) => (
                <Typography variant="body2" sx={{ fontWeight: 700, color: '#f59e0b' }}>
                  {row.rating || 5} ★
                </Typography>
              )
            },
            {
              key: 'content',
              label: 'Nội dung nhận xét',
              render: (row) => (
                <Typography variant="body2" color="text.secondary">
                  {row.content || row.comment || '—'}
                </Typography>
              )
            },
            {
              key: 'createdAt',
              label: 'Thời gian',
              render: (row) => (
                <Typography variant="body2" color="text.secondary">
                  {row.createdAt ? new Date(row.createdAt).toLocaleDateString('vi-VN') : '—'}
                </Typography>
              )
            },
            {
              key: 'actions',
              label: 'Thao tác',
              align: 'right',
              render: (row) => (
                <Button
                  size="small"
                  variant="outlined"
                  color="error"
                  startIcon={<BlockRoundedIcon fontSize="small" />}
                  onClick={() => {
                    setHideDialogReview(row)
                    setHideReason('')
                  }}
                  sx={{ textTransform: 'none', borderRadius: 2 }}
                >
                  Ẩn vi phạm
                </Button>
              )
            }
          ]}
        />
      </SellerCard>

      {/* Hide Reason Dialog */}
      <Dialog
        open={Boolean(hideDialogReview)}
        onClose={() => setHideDialogReview(null)}
        maxWidth="xs"
        fullWidth
        slotProps={{ paper: { sx: { borderRadius: 3 } } }}
      >
        <DialogTitle sx={{ fontWeight: 800 }}>Ẩn đánh giá vi phạm</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Vui lòng nhập lý do ẩn bài đánh giá này khỏi sàn:
          </Typography>
          <TextField
            autoFocus
            fullWidth
            size="small"
            label="Lý do ẩn"
            multiline
            rows={3}
            value={hideReason}
            onChange={(e) => setHideReason(e.target.value)}
          />
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setHideDialogReview(null)} color="inherit" sx={{ textTransform: 'none' }}>
            Hủy
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleExecuteHide}
            disabled={isSubmitting || !hideReason.trim()}
            sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
          >
            Xác nhận ẩn
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}

export default AdminReviewsPage
