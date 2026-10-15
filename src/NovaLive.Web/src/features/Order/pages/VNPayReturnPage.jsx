import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import ErrorRoundedIcon from '@mui/icons-material/ErrorRounded'
import HomeRoundedIcon from '@mui/icons-material/HomeRounded'
import { Box, Button, CircularProgress, Container, Typography } from '@mui/material'
import { useEffect, useRef, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { verifyVNPayReturnAPI } from '~/common/apis/services/orderService'
import { formatVND } from '~/common/utils/formatters'
import { glassSx, gradientText } from '~/theme'

function VNPayReturnPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [status, setStatus] = useState('loading')
  const [result, setResult] = useState(null)
  const calledRef = useRef(false)

  useEffect(() => {

    if (calledRef.current) return
    calledRef.current = true

    const params = Object.fromEntries(searchParams.entries())
    verifyVNPayReturnAPI(params)
      .then(data => {
        setResult(data)
        setStatus(data?.code === '00' ? 'success' : 'fail')
      })
      .catch(() => {
        setStatus('fail')
        setResult({ message: 'Xác minh thanh toán thất bại. Vui lòng liên hệ hỗ trợ.' })
      })
  }, [searchParams])

  return (
    <Container maxWidth="sm" sx={{ pt: 10, pb: 6, display: 'flex', justifyContent: 'center' }}>
      <Box sx={{
        width: '100%',
        bgcolor: 'primary.main', ...glassSx,
        borderRadius: 3, p: { xs: 3, sm: 4.5 },
        border: '1px solid', borderColor: 'divider',
        display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2.5, textAlign: 'center'
      }}>

        {status === 'loading' && (
          <>
            <CircularProgress size={52} sx={{ color: 'secondary.main' }} />
            <Typography sx={{ fontSize: '1rem', fontWeight: 600, color: 'primary.contrastText' }}>
              Đang xác nhận thanh toán...
            </Typography>
            <Typography sx={{ fontSize: '0.82rem', color: 'rgba(45,45,45,0.5)' }}>
              Vui lòng không đóng trang này
            </Typography>
          </>
        )}

        {status === 'success' && (
          <>
            <Box sx={{
              width: 72, height: 72, borderRadius: '50%',
              bgcolor: 'rgba(22,163,74,0.1)',
              display: 'flex', alignItems: 'center', justifyContent: 'center'
            }}>
              <CheckCircleRoundedIcon sx={{ fontSize: 42, color: '#16a34a' }} />
            </Box>

            <Box>
              <Typography sx={{ fontSize: '1.2rem', fontWeight: 800, ...gradientText, mb: 0.5 }}>
                Thanh toán thành công!
              </Typography>
              <Typography sx={{ fontSize: '0.82rem', color: 'rgba(45,45,45,0.55)' }}>
                Đơn hàng của bạn đã được xác nhận và đang được xử lý
              </Typography>
            </Box>

            {result?.amount > 0 && (
              <Box sx={{
                bgcolor: 'rgba(22,163,74,0.06)', borderRadius: 2,
                border: '1px solid rgba(22,163,74,0.2)',
                px: 3, py: 1.5, width: '100%'
              }}>
                <Typography sx={{ fontSize: '0.76rem', color: 'rgba(45,45,45,0.5)', mb: 0.25 }}>
                  Số tiền thanh toán
                </Typography>
                <Typography sx={{ fontSize: '1.3rem', fontWeight: 800, color: '#16a34a' }}>
                  {formatVND(result.amount)}
                </Typography>
              </Box>
            )}

            {result?.orderId && (
              <Typography sx={{ fontSize: '0.75rem', color: 'rgba(45,45,45,0.4)' }}>
                Mã đơn hàng: <strong style={{ color: 'rgba(45,45,45,0.65)' }}>{result.orderId}</strong>
              </Typography>
            )}

            <Button
              variant="contained"
              startIcon={<HomeRoundedIcon />}
              onClick={() => navigate('/')}
              sx={{
                mt: 0.5, fontWeight: 700, borderRadius: 2, px: 3, py: 1.1,
                background: 'linear-gradient(90deg, #568dfb, #69aedc)',
                color: '#fff',
                '&:hover': { boxShadow: '0 4px 16px rgba(52,133,247,0.35)' }
              }}
            >
              Về trang chủ
            </Button>
          </>
        )}

        {status === 'fail' && (
          <>
            <Box sx={{
              width: 72, height: 72, borderRadius: '50%',
              bgcolor: 'rgba(220,38,38,0.1)',
              display: 'flex', alignItems: 'center', justifyContent: 'center'
            }}>
              <ErrorRoundedIcon sx={{ fontSize: 42, color: '#dc2626' }} />
            </Box>

            <Box>
              <Typography sx={{ fontSize: '1.2rem', fontWeight: 800, color: '#dc2626', mb: 0.5 }}>
                Thanh toán thất bại
              </Typography>
              <Typography sx={{ fontSize: '0.82rem', color: 'rgba(45,45,45,0.55)' }}>
                {result?.message || 'Giao dịch không thành công. Đơn hàng đã bị hủy.'}
              </Typography>
            </Box>

            <Button
              variant="contained"
              startIcon={<HomeRoundedIcon />}
              onClick={() => navigate('/')}
              sx={{
                mt: 0.5, fontWeight: 700, borderRadius: 2, px: 3, py: 1.1,
                bgcolor: '#dc2626', color: '#fff',
                '&:hover': { bgcolor: '#b91c1c' }
              }}
            >
              Về trang chủ
            </Button>
          </>
        )}
      </Box>
    </Container>
  )
}

export default VNPayReturnPage
