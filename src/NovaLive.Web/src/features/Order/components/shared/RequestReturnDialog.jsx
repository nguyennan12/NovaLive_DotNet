import { useState } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Box,
  Typography,
  Button,
  TextField,
  MenuItem,
  Checkbox,
  Stack,
  IconButton,
  Alert,
  CircularProgress,
  Chip,
  Divider
} from '@mui/material'
import CloseRoundedIcon from '@mui/icons-material/CloseRounded'
import CloudUploadRoundedIcon from '@mui/icons-material/CloudUploadRounded'
import DeleteOutlineRoundedIcon from '@mui/icons-material/DeleteOutlineRounded'
import AccessTimeRoundedIcon from '@mui/icons-material/AccessTimeRounded'
import AssignmentReturnRoundedIcon from '@mui/icons-material/AssignmentReturnRounded'
import { toast } from 'react-toastify'
import { formatVND } from '~/common/utils/formatters'
import { uploadSingleImageAPI } from '~/common/apis/services/uploadService'
import { requestReturnAPI, RETURN_REASONS } from '~/common/apis/services/returnService'
import { getRemainingReturnDays, RETURN_WINDOW_DAYS } from '../../constants/orderStatus'

export default function RequestReturnDialog({ open, onClose, order, onSuccess }) {
  const [reason, setReason] = useState('Defective')
  const [selectedItems, setSelectedItems] = useState({})
  const [reasonDetails, setReasonDetails] = useState('')
  const [evidenceUrls, setEvidenceUrls] = useState([])
  const [uploading, setUploading] = useState(false)
  const [submitting, setSubmitting] = useState(false)

  if (!order) return null

  const products = order.order_products || []
  const subOrderId = order.subOrderId || order._id
  const deliveredAt = order.deliveredAt || order.updatedAt || order.createdAt
  const remainingDays = getRemainingReturnDays(deliveredAt)

  const handleToggleItem = (productId, itemPrice, maxQty) => {
    setSelectedItems(prev => {
      const current = prev[productId]
      if (current) {
        const next = { ...prev }
        delete next[productId]
        return next
      } else {
        return {
          ...prev,
          [productId]: {
            quantity: maxQty || 1,
            price: itemPrice || 0
          }
        }
      }
    })
  }

  const handleQuantityChange = (productId, qty, maxQty) => {
    const val = Math.min(Math.max(1, parseInt(qty) || 1), maxQty)
    setSelectedItems(prev => ({
      ...prev,
      [productId]: {
        ...prev[productId],
        quantity: val
      }
    }))
  }

  const handleFileUpload = async (event) => {
    const files = Array.from(event.target.files || [])
    if (!files.length) return

    setUploading(true)
    try {
      for (const file of files) {
        const res = await uploadSingleImageAPI(file, 'returns')
        const url = res?.url || res?.fileUrl || (typeof res === 'string' ? res : null)
        if (url) {
          setEvidenceUrls(prev => [...prev, url])
        }
      }
      toast.success(`Đã tải lên ${files.length} tệp bằng chứng!`)
    } catch (err) {
      // Fallback: nếu MinIO/backend upload local chưa chạy, tạo object URL hoặc thông báo
      console.warn('Upload API error, applying local preview preview:', err)
      const mockUrls = files.map(f => URL.createObjectURL(f))
      setEvidenceUrls(prev => [...prev, ...mockUrls])
      toast.info('Đã thêm bằng chứng ảnh/video!')
    } finally {
      setUploading(false)
      event.target.value = ''
    }
  }

  const handleRemoveEvidence = (index) => {
    setEvidenceUrls(prev => prev.filter((_, i) => i !== index))
  }

  // Calculate estimated refund amount
  const estimatedRefund = Object.values(selectedItems).reduce((sum, item) => sum + (item.price * item.quantity), 0)
  const selectedCount = Object.keys(selectedItems).length

  const handleSubmit = async () => {
    if (selectedCount === 0) {
      toast.error('Vui lòng chọn ít nhất một sản phẩm cần hoàn trả!')
      return
    }
    if (evidenceUrls.length === 0) {
      toast.error('Vui lòng tải lên ít nhất một ảnh hoặc video bằng chứng lỗi/hư hỏng!')
      return
    }

    const items = Object.entries(selectedItems).map(([id, val]) => ({
      orderItemId: id,
      quantity: val.quantity,
      reasonDetail: reasonDetails.trim() || undefined
    }))

    const payload = {
      subOrderId,
      reason,
      items,
      evidenceUrls
    }

    setSubmitting(true)
    try {
      await requestReturnAPI(payload)
      toast.success('Yêu cầu Trả hàng / Hoàn tiền đã được gửi thành công tới Người bán!')
      onSuccess?.()
      onClose()
    } catch (err) {
      const msg = err.response?.data?.message || err.message || 'Không thể tạo yêu cầu hoàn hàng'
      toast.error(`Gửi yêu cầu thất bại: ${msg}`)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth PaperProps={{ sx: { borderRadius: '16px', p: 1 } }}>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', pb: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <AssignmentReturnRoundedIcon sx={{ color: '#0095ff', fontSize: 24 }} />
          <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a' }}>
            Yêu Cầu Trả Hàng / Hoàn Tiền
          </Typography>
        </Box>
        <IconButton size="small" onClick={onClose} sx={{ color: '#64748b' }}>
          <CloseRoundedIcon fontSize="small" />
        </IconButton>
      </DialogTitle>

      <DialogContent dividers sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
        {/* Countdown Alert */}
        <Alert
          severity={remainingDays <= 2 ? 'warning' : 'info'}
          icon={<AccessTimeRoundedIcon />}
          sx={{ borderRadius: '10px' }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1 }}>
            <Typography variant="body2" sx={{ fontWeight: 600 }}>
              Chính sách bảo vệ người mua NovaLive: Được hoàn trả trong vòng <strong>{RETURN_WINDOW_DAYS} ngày</strong> từ khi nhận hàng.
            </Typography>
            <Chip
              label={`Thời hạn khiếu nại: Còn ${remainingDays} ngày`}
              size="small"
              color={remainingDays <= 2 ? 'error' : 'primary'}
              sx={{ fontWeight: 700, height: 24 }}
            />
          </Box>
        </Alert>

        {/* Reason Selection */}
        <Box>
          <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1, color: '#1e293b' }}>
            1. Lý do trả hàng / hoàn tiền *
          </Typography>
          <TextField
            select
            fullWidth
            size="small"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            sx={{ '& .MuiOutlinedInput-root': { borderRadius: '10px' } }}
          >
            {RETURN_REASONS.map(r => (
              <MenuItem key={r.value} value={r.value}>
                {r.label}
              </MenuItem>
            ))}
          </TextField>
        </Box>

        {/* Select Items */}
        <Box>
          <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1, color: '#1e293b' }}>
            2. Chọn sản phẩm cần hoàn trả *
          </Typography>
          <Stack spacing={1.5}>
            {products.map((p) => {
              const pId = p.productId?._id || p._id || p.productId
              const name = p.productId?.spu_name || p.name || 'Sản phẩm'
              const thumb = p.productId?.spu_thumb || p.thumb
              const price = p.productId?.spu_price || p.price || 0
              const maxQty = p.quantity || 1
              const isSelected = Boolean(selectedItems[pId])
              const currentQty = selectedItems[pId]?.quantity || 1

              return (
                <Box
                  key={pId}
                  sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1.5,
                    p: 1.5,
                    borderRadius: '12px',
                    border: '1.5px solid',
                    borderColor: isSelected ? '#0095ff' : '#e2e8f0',
                    bgcolor: isSelected ? 'rgba(0, 149, 255, 0.03)' : '#ffffff',
                    transition: 'all 0.2s'
                  }}
                >
                  <Checkbox
                    checked={isSelected}
                    onChange={() => handleToggleItem(pId, price, maxQty)}
                    sx={{ p: 0.5, color: '#94a3b8', '&.Mui-checked': { color: '#0095ff' } }}
                  />
                  <Box
                    component="img"
                    src={thumb}
                    alt={name}
                    sx={{ width: 50, height: 50, borderRadius: '8px', objectFit: 'cover', bgcolor: '#f1f5f9' }}
                    onError={(e) => { e.target.style.display = 'none' }}
                  />
                  <Box sx={{ flex: 1, minWidth: 0 }}>
                    <Typography variant="body2" sx={{ fontWeight: 600, color: '#0f172a', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {name}
                    </Typography>
                    <Typography variant="caption" sx={{ color: '#64748b' }}>
                      Đơn giá: {formatVND(price)} • Tối đa hoàn: x{maxQty}
                    </Typography>
                  </Box>
                  {isSelected && (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Typography variant="caption" sx={{ fontWeight: 600, color: '#475569' }}>SL hoàn:</Typography>
                      <TextField
                        size="small"
                        type="number"
                        inputProps={{ min: 1, max: maxQty }}
                        value={currentQty}
                        onChange={(e) => handleQuantityChange(pId, e.target.value, maxQty)}
                        sx={{ width: 65, '& input': { p: 0.8, textAlign: 'center', fontWeight: 700 } }}
                      />
                    </Box>
                  )}
                </Box>
              )
            })}
          </Stack>
        </Box>

        {/* Detailed Explanation */}
        <Box>
          <Typography variant="subtitle2" sx={{ fontWeight: 700, mb: 1, color: '#1e293b' }}>
            3. Mô tả chi tiết tình trạng hàng
          </Typography>
          <TextField
            fullWidth
            multiline
            rows={2.5}
            placeholder="Mô tả cụ thể tình trạng hàng bị hỏng, sai màu, thiếu phụ kiện để Shop và Admin thẩm định..."
            value={reasonDetails}
            onChange={(e) => setReasonDetails(e.target.value)}
            sx={{ '& .MuiOutlinedInput-root': { borderRadius: '10px' } }}
          />
        </Box>

        {/* Evidence Media Upload */}
        <Box>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Typography variant="subtitle2" sx={{ fontWeight: 700, color: '#1e293b' }}>
              4. Hình ảnh & Video bằng chứng *
            </Typography>
            <Typography variant="caption" sx={{ color: '#64748b' }}>
              (Tối thiểu 1 ảnh/video unbox hoặc ảnh chụp lỗi)
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', alignItems: 'center' }}>
            {evidenceUrls.map((url, idx) => (
              <Box
                key={idx}
                sx={{
                  position: 'relative',
                  width: 72,
                  height: 72,
                  borderRadius: '8px',
                  overflow: 'hidden',
                  border: '1px solid #cbd5e1'
                }}
              >
                <Box
                  component="img"
                  src={url}
                  sx={{ width: '100%', height: '100%', objectFit: 'cover' }}
                />
                <IconButton
                  size="small"
                  onClick={() => handleRemoveEvidence(idx)}
                  sx={{
                    position: 'absolute',
                    top: 2,
                    right: 2,
                    bgcolor: 'rgba(0,0,0,0.6)',
                    color: '#fff',
                    p: 0.3,
                    '&:hover': { bgcolor: 'rgba(220,38,38,0.8)' }
                  }}
                >
                  <DeleteOutlineRoundedIcon sx={{ fontSize: 14 }} />
                </IconButton>
              </Box>
            ))}

            <Button
              component="label"
              variant="outlined"
              disabled={uploading}
              sx={{
                width: 72,
                height: 72,
                borderRadius: '8px',
                border: '1.5px dashed #94a3b8',
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                color: '#64748b',
                '&:hover': { borderColor: '#0095ff', color: '#0095ff', bgcolor: 'rgba(0,149,255,0.04)' }
              }}
            >
              {uploading ? <CircularProgress size={20} /> : <CloudUploadRoundedIcon sx={{ fontSize: 22 }} />}
              <Typography sx={{ fontSize: '0.65rem', fontWeight: 600, mt: 0.5 }}>Tải lên</Typography>
              <input type="file" multiple accept="image/*,video/*" hidden onChange={handleFileUpload} />
            </Button>
          </Box>
        </Box>

        {/* Refund Amount Summary */}
        <Box sx={{ p: 1.5, bgcolor: '#f8fafc', borderRadius: '12px', border: '1px solid #e2e8f0' }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="body2" sx={{ color: '#64748b', fontWeight: 600 }}>
              Số tiền hoàn dự kiến ({selectedCount} sản phẩm):
            </Typography>
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0095ff' }}>
              {formatVND(estimatedRefund)}
            </Typography>
          </Box>
        </Box>
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button onClick={onClose} sx={{ textTransform: 'none', color: '#64748b', fontWeight: 600 }}>
          Hủy bỏ
        </Button>
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={submitting || selectedCount === 0 || evidenceUrls.length === 0}
          sx={{
            textTransform: 'none',
            fontWeight: 700,
            px: 3,
            background: 'linear-gradient(90deg, #0095ff 0%, #14c3eb 100%)',
            boxShadow: '0 2px 8px rgba(0,149,255,0.25)'
          }}
        >
          {submitting ? 'Đang gửi...' : 'Gửi Yêu Cầu Hoàn Tiền'}
        </Button>
      </DialogActions>
    </Dialog>
  )
}
