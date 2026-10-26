import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import type { SellerReview } from '../../types/viewModels'
import { useReplyReview } from '../../hooks/useReviews'
import { REVIEW_REPLY_MAX } from '../../constants'

export function ReviewReplyDialog({ review, onClose }: { review: SellerReview; onClose: () => void }) {
  const [text, setText] = useState(''); const reply = useReplyReview()
  return <Dialog open fullWidth maxWidth='sm' onClose={reply.isPending ? undefined : onClose} aria-labelledby='reply-title'><DialogTitle id='reply-title'>Phản hồi đánh giá của {review.buyerName}</DialogTitle><DialogContent><Stack spacing={2}><Typography>{review.content}</Typography><Alert severity='warning'>Shop chỉ được phản hồi một lần. Vui lòng kiểm tra nội dung trước khi gửi.</Alert><TextField autoFocus label='Nội dung phản hồi' multiline minRows={4} value={text} onChange={event => setText(event.target.value)} helperText={`${text.length}/${REVIEW_REPLY_MAX.toLocaleString('vi-VN')} ký tự`} /></Stack></DialogContent><DialogActions><Button disabled={reply.isPending} onClick={onClose}>Quay lại</Button><Button variant='contained' color='secondary' disabled={reply.isPending || !text.trim() || text.length > REVIEW_REPLY_MAX || Boolean(review.sellerReply) || review.isHidden} onClick={() => reply.mutate({ id: review.id, replyText: text.trim() }, { onSuccess: onClose })}>{reply.isPending ? 'Đang gửi…' : 'Gửi phản hồi'}</Button></DialogActions></Dialog>
}
