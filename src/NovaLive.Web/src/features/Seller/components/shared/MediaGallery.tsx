import { Box, Button, Dialog, DialogActions, DialogContent } from '@mui/material'
import { useState } from 'react'

export function MediaGallery({ urls }: { urls: string[] }) {
  const [selected, setSelected] = useState<string | null>(null)
  const video = (url: string) => /\.(mp4|webm|mov)(\?|$)/i.test(url) || url.startsWith('data:video/')
  return <><Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>{urls.map((url, i) => <Button key={`${url}-${i}`} aria-label={`Xem bằng chứng ${i + 1}`} onClick={() => setSelected(url)} sx={{ p: 0.5, minWidth: 64, border: 1, borderColor: 'divider' }}>{video(url) ? '▶ Video' : <Box component='img' src={url} alt={`Bằng chứng ${i + 1}`} sx={{ width: 64, height: 64, objectFit: 'cover', borderRadius: 1 }} />}</Button>)}</Box>
    <Dialog open={Boolean(selected)} onClose={() => setSelected(null)} fullWidth maxWidth='md' aria-label='Xem tệp bằng chứng'><DialogContent>{selected && (video(selected) ? <Box component='video' controls src={selected} sx={{ width: '100%' }} /> : <Box component='img' src={selected} alt='Bằng chứng phóng to' sx={{ width: '100%', maxHeight: '75vh', objectFit: 'contain' }} />)}</DialogContent><DialogActions><Button onClick={() => setSelected(null)}>Đóng</Button></DialogActions></Dialog>
  </>
}
