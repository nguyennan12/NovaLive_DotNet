import { Alert, Box, Button, Stack, Typography } from '@mui/material'
import UploadOutlined from '@mui/icons-material/UploadOutlined'
import { useState } from 'react'
import { useUploadMedia } from '../../hooks/useShop'
import { MediaGallery } from './MediaGallery'

export function MediaUpload({ label, urls, onChange, multiple = false, video = false, disabled = false }: { label: string; urls: string[]; onChange: (urls: string[]) => void; multiple?: boolean; video?: boolean; disabled?: boolean }) {
  const upload = useUploadMedia()
  const [error, setError] = useState('')
  async function selected(files: FileList | null) {
    if (!files?.length) return
    setError('')
    const result: string[] = multiple ? [...urls] : []
    for (const file of Array.from(files)) {
      if (!file.type.startsWith('image/') && !(video && file.type.startsWith('video/'))) { setError('Chọn tệp ảnh hoặc video được hỗ trợ.'); return }
      const max = file.type.startsWith('video/') ? 100 : 10
      if (file.size > max * 1024 * 1024) { setError(`Tệp ${file.name} vượt quá giới hạn ${max} MB.`); return }
      try { result.push((await upload.mutateAsync({ file })).url) } catch { return }
    }
    onChange(result)
  }
  return <Stack spacing={1}><Typography sx={{ fontWeight: 600 }}>{label}</Typography><MediaGallery urls={urls} />
    <Box sx={{ display: 'flex', gap: 1 }}><Button component='label' variant='outlined' color='secondary' disabled={upload.isPending || disabled} startIcon={<UploadOutlined />}>{upload.isPending ? 'Đang tải…' : 'Chọn tệp'}<input hidden aria-label={label} type='file' accept={video ? 'image/*,video/mp4,video/webm' : 'image/*'} multiple={multiple} onChange={event => { void selected(event.target.files); event.target.value = '' }} /></Button>{urls.length > 0 && <Button disabled={upload.isPending || disabled} onClick={() => onChange([])}>Bỏ tệp đã chọn</Button>}</Box>
    <Typography variant='caption' color='text.secondary'>Ảnh tối đa 10 MB{video ? ', video tối đa 100 MB' : ''}.</Typography>{error && <Alert severity='error'>{error}</Alert>}
  </Stack>
}
