import { Alert, Box, Button, Checkbox, FormControlLabel, MenuItem, Rating, Stack, TextField, Typography } from '@mui/material'
import { useMemo, useState } from 'react'
import type { SellerReview } from '../types/viewModels'
import { useReviews } from '../hooks/useReviews'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { DataTable } from '../components/shared/DataTable'
import { DateText } from '../components/shared/DateText'
import { MediaGallery } from '../components/shared/MediaGallery'
import { ReviewReplyDialog } from '../components/reviews/ReviewReplyDialog'
import { PAGE_SIZE } from '../constants'

export default function ReviewsPage() {
  const query = useReviews(); const [rating, setRating] = useState(''); const [unanswered, setUnanswered] = useState(false); const [page, setPage] = useState(1); const [target, setTarget] = useState<SellerReview | null>(null)
  const filtered = useMemo(() => (query.data || []).filter(item => (!rating || item.rating === Number(rating)) && (!unanswered || !item.sellerReply)), [query.data, rating, unanswered])
  return <><PageHeader title='Đánh giá sản phẩm' description='Lắng nghe khách hàng và phản hồi mỗi đánh giá một lần.' /><Alert severity='info' sx={{ mb: 2 }}>Đánh giá được tổng hợp theo từng sản phẩm. Đánh giá đã bị ẩn chỉ xuất hiện nếu dữ liệu nguồn cho phép Shop xem.</Alert>
    <Box sx={{ display: 'flex', gap: 2, mb: 2, flexWrap: 'wrap' }}><TextField select label='Số sao' size='small' value={rating} onChange={event => { setRating(event.target.value); setPage(1) }} sx={{ minWidth: 150 }}><MenuItem value=''>Tất cả</MenuItem>{[5, 4, 3, 2, 1].map(value => <MenuItem key={value} value={String(value)}>{value} sao</MenuItem>)}</TextField><FormControlLabel label='Chưa phản hồi' control={<Checkbox checked={unanswered} onChange={(_, checked) => { setUnanswered(checked); setPage(1) }} />} /></Box>
    <SellerCard><DataTable rows={filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)} rowKey={row => row.id} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={Math.ceil(filtered.length / PAGE_SIZE)} total={filtered.length} onPage={setPage} rowMuted={row => Boolean(row.isHidden)} columns={[
      { key: 'buyer', label: 'Người mua / Sản phẩm', render: row => <>{row.buyerName}<Typography component='div' variant='caption'>{row.productName}</Typography><DateText value={row.createdAt} /></> },
      { key: 'rating', label: 'Số sao', render: row => <Rating value={row.rating} readOnly size='small' aria-label={`${row.rating} trên 5 sao`} /> },
      { key: 'content', label: 'Đánh giá', render: row => <Stack spacing={1}><Typography>{row.content || row.title || 'Không có nội dung văn bản.'}</Typography>{row.isHidden && <Typography variant='caption'>Đánh giá đã bị ẩn</Typography>}<MediaGallery urls={row.mediaUrls} /></Stack> },
      { key: 'reply', label: 'Phản hồi của Shop', render: row => row.sellerReply ? <><Typography>{row.sellerReply}</Typography><Typography variant='caption'>Đã phản hồi · không thể sửa</Typography></> : <Button disabled={row.isHidden} onClick={() => setTarget(row)}>Phản hồi một lần</Button> }
    ]} /></SellerCard>{target && <ReviewReplyDialog review={target} onClose={() => setTarget(null)} />}
  </>
}
