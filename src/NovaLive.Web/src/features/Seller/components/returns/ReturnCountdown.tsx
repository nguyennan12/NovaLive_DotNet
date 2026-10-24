import { useEffect, useState } from 'react'
import { Typography } from '@mui/material'
import { returnDeadline } from '../../utils/format'

export function useReturnDeadline(createdAt: string) {
  const [now, setNow] = useState(Date.now)
  useEffect(() => { const timer = setInterval(() => setNow(Date.now()), 1000); return () => clearInterval(timer) }, [])
  return Math.max(0, returnDeadline(createdAt) - now)
}
export function ReturnCountdown({ createdAt }: { createdAt: string }) {
  const remaining = useReturnDeadline(createdAt)
  const totalSeconds = Math.floor(remaining / 1000)
  const hours = Math.floor(totalSeconds / 3600)
  const minutes = Math.floor(totalSeconds % 3600 / 60)
  const seconds = totalSeconds % 60
  return <Typography variant='caption' color={remaining < 86_400_000 ? 'error.main' : 'text.secondary'} role='timer'>{remaining > 0 ? `Còn ${hours}h ${String(minutes).padStart(2, '0')}m ${String(seconds).padStart(2, '0')}s${remaining < 86_400_000 ? ' · Sắp hết hạn' : ''}` : 'Hết hạn phản hồi · chờ hệ thống chuyển phân xử'}</Typography>
}
