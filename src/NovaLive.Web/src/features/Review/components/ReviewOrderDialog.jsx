import StarRoundedIcon from '@mui/icons-material/StarRounded'
import { Avatar, Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, Rating, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import { formatVND } from '~/common/utils/formatters'
import { useCreateReview } from '../hooks/useCreateReview'

const ReviewOrderDialog = ({ open, onClose, items = [] }) => {
  const [activeItem, setActiveItem] = useState(null)
  const [rating, setRating] = useState(5)
  const [title, setTitle] = useState('')
  const [content, setContent] = useState('')
  const [reviewedIds, setReviewedIds] = useState([])
  const createReview = useCreateReview()

  const closeDialog = () => {
    setActiveItem(null)
    setRating(5)
    setTitle('')
    setContent('')
    onClose()
  }

  const chooseItem = (item) => {
    setActiveItem(item)
    setRating(5)
    setTitle('')
    setContent('')
  }

  const submit = () => {
    if (!activeItem?.id) return
    createReview.mutate({
      orderItemId: activeItem.id,
      rating,
      title: title.trim() || null,
      content: content.trim() || null,
      mediaUrls: []
    }, {
      onSuccess: () => {
        setReviewedIds(ids => [...ids, String(activeItem.id)])
        setActiveItem(null)
        setTitle('')
        setContent('')
      }
    })
  }

  return (
    <Dialog open={open} onClose={createReview.isPending ? undefined : closeDialog} fullWidth maxWidth="sm">
      <DialogTitle sx={{ fontWeight: 800 }}>{activeItem ? 'Viết đánh giá' : 'Đánh giá sản phẩm đã mua'}</DialogTitle>
      <DialogContent dividers>
        {activeItem ? (
          <Stack spacing={2}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, p: 1.5, borderRadius: 2, bgcolor: 'rgba(52,133,247,0.06)' }}>
              <Avatar variant="rounded" src={activeItem.productId?.spu_thumb || activeItem.thumbnailUrl} sx={{ width: 54, height: 54 }}>
                {(activeItem.productId?.spu_name || activeItem.spuName || 'S').charAt(0)}
              </Avatar>
              <Box sx={{ minWidth: 0 }}>
                <Typography sx={{ fontWeight: 700 }}>{activeItem.productId?.spu_name || activeItem.spuName || 'Sản phẩm'}</Typography>
                <Typography variant="caption" color="text.secondary">SL: {activeItem.quantity || 1}</Typography>
              </Box>
            </Box>
            <Box>
              <Typography sx={{ fontWeight: 700, mb: 0.5 }}>Chất lượng sản phẩm</Typography>
              <Rating value={rating} onChange={(_, value) => value && setRating(value)} size="large" />
              <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>{rating}/5</Typography>
            </Box>
            <TextField label="Tiêu đề (không bắt buộc)" value={title} onChange={e => setTitle(e.target.value)} inputProps={{ maxLength: 200 }} helperText={`${title.length}/200`} fullWidth />
            <TextField label="Chia sẻ trải nghiệm của bạn" value={content} onChange={e => setContent(e.target.value)} multiline minRows={4} inputProps={{ maxLength: 2000 }} helperText={`${content.length}/2000`} fullWidth />
            <Typography variant="caption" color="text.secondary">Đánh giá chỉ gửi được cho sản phẩm đã giao và chưa trả hàng.</Typography>
          </Stack>
        ) : (
          <Stack spacing={1.25}>
            <Typography variant="body2" color="text.secondary">Chọn sản phẩm trong đơn để đánh giá.</Typography>
            {items.map((item, index) => {
              const itemId = String(item.id || '')
              const reviewed = reviewedIds.includes(itemId)
              return (
                <Box key={itemId || index} sx={{ display: 'flex', alignItems: 'center', gap: 1.25, p: 1.25, border: '1px solid', borderColor: 'divider', borderRadius: 2 }}>
                  <Avatar variant="rounded" src={item.productId?.spu_thumb || item.thumbnailUrl} sx={{ width: 48, height: 48 }}>
                    {(item.productId?.spu_name || item.spuName || 'S').charAt(0)}
                  </Avatar>
                  <Box sx={{ flex: 1, minWidth: 0 }}>
                    <Typography sx={{ fontSize: '0.9rem', fontWeight: 700 }} noWrap>{item.productId?.spu_name || item.spuName || 'Sản phẩm'}</Typography>
                    <Typography variant="caption" color="text.secondary">x{item.quantity || 1}{item.price ? ` · ${formatVND(item.price)}` : ''}</Typography>
                  </Box>
                  {reviewed ? <Typography sx={{ fontSize: '0.8rem', color: 'success.main', fontWeight: 700 }}>Đã gửi</Typography> : (
                    <Button size="small" variant="outlined" onClick={() => chooseItem(item)} disabled={!itemId}>Đánh giá</Button>
                  )}
                </Box>
              )
            })}
            {!items.length && <Typography color="text.secondary">Không tìm thấy sản phẩm trong đơn.</Typography>}
          </Stack>
        )}
      </DialogContent>
      <DialogActions sx={{ p: 2 }}>
        {activeItem ? <>
          <Button onClick={() => setActiveItem(null)} disabled={createReview.isPending}>Quay lại</Button>
          <Button variant="contained" startIcon={<StarRoundedIcon />} onClick={submit} disabled={createReview.isPending || !activeItem?.id}>
            {createReview.isPending ? 'Đang gửi…' : 'Gửi đánh giá'}
          </Button>
        </> : <Button onClick={closeDialog}>Đóng</Button>}
      </DialogActions>
    </Dialog>
  )
}

export default ReviewOrderDialog
