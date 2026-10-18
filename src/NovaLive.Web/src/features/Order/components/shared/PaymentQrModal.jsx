import { useState, useEffect, useMemo, useCallback } from 'react'
import {
  Dialog,
  DialogContent,
  Box,
  Typography,
  Button,
  IconButton,
  CircularProgress,
  Tooltip,
  Divider,
  Alert
} from '@mui/material'
import CloseRoundedIcon from '@mui/icons-material/CloseRounded'
import ContentCopyRoundedIcon from '@mui/icons-material/ContentCopyRounded'
import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import QrCode2RoundedIcon from '@mui/icons-material/QrCode2Rounded'
import AccountBalanceWalletRoundedIcon from '@mui/icons-material/AccountBalanceWalletRounded'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import TimerOutlinedIcon from '@mui/icons-material/TimerOutlined'
import { useNavigate } from 'react-router-dom'
import { toast } from 'react-toastify'
import { formatVND } from '~/common/utils/formatters'
import { generateVietQrAPI, createMomoPaymentAPI, getPaymentStatusAPI } from '~/common/apis/services/orderService'
import { usePaymentHub } from '~/common/hooks/useSignalR'
import { gradientText } from '~/theme'

const QR_COUNTDOWN_SECONDS = 15 * 60 // 15 mins

export default function PaymentQrModal({
  open,
  onClose,
  paymentData, // { paymentId, parentOrderId, orderCode, amount, paymentMethod }
  onPaymentSuccess
}) {
  const navigate = useNavigate()
  const { paymentId, parentOrderId, orderCode, amount = 0, paymentMethod = 'vietqr' } = paymentData || {}

  const [loading, setLoading] = useState(false)
  const [vietQrData, setVietQrData] = useState(null)
  const [momoData, setMomoData] = useState(null)
  const [isSuccess, setIsSuccess] = useState(false)
  const [timeLeft, setTimeLeft] = useState(QR_COUNTDOWN_SECONDS)
  const [copiedField, setCopiedField] = useState(null)

  // Countdown timer
  useEffect(() => {
    if (!open || isSuccess) return
    setTimeLeft(QR_COUNTDOWN_SECONDS)

    const timer = setInterval(() => {
      setTimeLeft((prev) => {
        if (prev <= 1) {
          clearInterval(timer)
          return 0
        }
        return prev - 1
      })
    }, 1000)

    return () => clearInterval(timer)
  }, [open, isSuccess])

  const formattedTimer = useMemo(() => {
    const minutes = Math.floor(timeLeft / 60)
    const seconds = timeLeft % 60
    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`
  }, [timeLeft])

  // Handle successful payment event
  const handleSuccess = useCallback((eventPayload) => {
    setIsSuccess(true)
    toast.success('🎉 Thanh toán thành công! Đơn hàng đã được xác nhận.')
    onPaymentSuccess?.(eventPayload)

    setTimeout(() => {
      onClose?.()
      navigate('/orders')
    }, 2200)
  }, [onPaymentSuccess, onClose, navigate])

  // SignalR Payment Hub connection
  usePaymentHub({
    paymentId,
    onPaymentSuccess: handleSuccess,
    enabled: open && !!paymentId && !isSuccess
  })

  // Fetch QR info when modal opens
  const fetchPaymentDetails = useCallback(async () => {
    if (!open || !orderCode) return
    setLoading(true)

    try {
      if (paymentMethod === 'vietqr') {
        const data = await generateVietQrAPI(orderCode)
        setVietQrData(data)
      } else if (paymentMethod === 'momo') {
        const data = await createMomoPaymentAPI({
          orderId: parentOrderId || orderCode,
          orderInfo: `Thanh toan don hang ${orderCode}`
        })
        setMomoData(data)
      }
    } catch (err) {
      console.error('Failed to load payment QR:', err)
      toast.error('Không thể tạo mã QR thanh toán. Bạn có thể kiểm tra lại trong lịch sử đơn.')
    } finally {
      setLoading(false)
    }
  }, [open, orderCode, parentOrderId, paymentMethod])

  useEffect(() => {
    if (open) {
      setIsSuccess(false)
      fetchPaymentDetails()
    }
  }, [open, fetchPaymentDetails])

  // Copy to clipboard helper
  const handleCopy = (text, fieldName) => {
    navigator.clipboard.writeText(String(text)).then(() => {
      setCopiedField(fieldName)
      toast.success(`Đã sao chép ${fieldName}!`)
      setTimeout(() => setCopiedField(null), 2000)
    })
  }

  // Manual status check fallback
  const handleCheckManual = async () => {
    if (!parentOrderId) return
    try {
      const res = await getPaymentStatusAPI(parentOrderId)
      if (res?.isPaid) {
        handleSuccess(res)
      } else {
        toast.info('Hệ thống chưa nhận được tiền. Nếu bạn đã chuyển, vui lòng chờ trong giây lát.')
      }
    } catch {
      toast.error('Kiểm tra trạng thái thất bại. Vui lòng thử lại sau.')
    }
  }

  return (
    <Dialog
      open={open}
      onClose={isSuccess ? undefined : onClose}
      maxWidth="sm"
      fullWidth
      PaperProps={{
        sx: {
          borderRadius: 3.5,
          overflow: 'hidden',
          boxShadow: '0 20px 60px rgba(0,0,0,0.25)',
          border: '1px solid',
          borderColor: 'divider',
          background: '#ffffff'
        }
      }}
    >
      {/* Header */}
      <Box sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        px: 3,
        py: 2,
        borderBottom: '1px solid',
        borderColor: 'divider',
        bgcolor: '#f8fafc'
      }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.25 }}>
          {paymentMethod === 'vietqr' ? (
            <QrCode2RoundedIcon sx={{ color: '#0284c7', fontSize: 26 }} />
          ) : (
            <AccountBalanceWalletRoundedIcon sx={{ color: '#ae2d68', fontSize: 26 }} />
          )}
          <Box>
            <Typography sx={{ fontWeight: 800, fontSize: '1.05rem', ...gradientText }}>
              {paymentMethod === 'vietqr' ? 'Thanh toán chuyển khoản VietQR' : 'Thanh toán ví điện tử MoMo'}
            </Typography>
            <Typography sx={{ fontSize: '0.76rem', color: 'text.secondary' }}>
              Mã đơn hàng: <strong>#{orderCode}</strong>
            </Typography>
          </Box>
        </Box>

        {!isSuccess && (
          <IconButton size="small" onClick={onClose} sx={{ color: 'text.secondary' }}>
            <CloseRoundedIcon fontSize="small" />
          </IconButton>
        )}
      </Box>

      <DialogContent sx={{ p: 3 }}>
        {isSuccess ? (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <CheckCircleRoundedIcon sx={{ fontSize: 72, color: '#16a34a', mb: 1.5, animation: 'bounce 1s' }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#16a34a', mb: 0.5 }}>
              Thanh toán thành công!
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Đơn hàng <strong>#{orderCode}</strong> đã được ghi nhận. Hệ thống đang chuyển đến trang đơn hàng...
            </Typography>
            <CircularProgress size={24} sx={{ mt: 3, color: '#16a34a' }} />
          </Box>
        ) : loading ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', py: 6, gap: 2 }}>
            <CircularProgress size={40} sx={{ color: paymentMethod === 'vietqr' ? '#0284c7' : '#ae2d68' }} />
            <Typography variant="body2" color="text.secondary">
              Đang tạo mã QR thanh toán động...
            </Typography>
          </Box>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            {/* Countdown Banner */}
            <Box sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 0.75,
              px: 2,
              py: 0.75,
              borderRadius: 2,
              bgcolor: timeLeft < 120 ? '#fee2e2' : '#f1f5f9',
              color: timeLeft < 120 ? '#dc2626' : '#475569',
              mb: 2.5
            }}>
              <TimerOutlinedIcon sx={{ fontSize: 18 }} />
              <Typography sx={{ fontSize: '0.82rem', fontWeight: 600 }}>
                Mã QR hết hạn sau: <strong>{formattedTimer}</strong>
              </Typography>
            </Box>

            {/* QR Code Container */}
            <Box sx={{
              p: 2,
              borderRadius: 3,
              border: '2px dashed',
              borderColor: paymentMethod === 'vietqr' ? '#0284c7' : '#ae2d68',
              bgcolor: '#ffffff',
              boxShadow: '0 8px 30px rgba(0,0,0,0.06)',
              mb: 2.5,
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center'
            }}>
              {paymentMethod === 'vietqr' && (
                vietQrData?.qrDataUrl || vietQrData?.qrCode ? (
                  <Box
                    component="img"
                    src={vietQrData.qrDataUrl || vietQrData.qrCode}
                    alt="VietQR Code"
                    sx={{ width: 220, height: 220, objectFit: 'contain' }}
                  />
                ) : (
                  <Box sx={{ width: 220, height: 220, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Typography variant="caption" color="text.secondary">Không tìm thấy mã QR</Typography>
                  </Box>
                )
              )}

              {paymentMethod === 'momo' && (
                momoData?.qrCodeUrl || momoData?.payUrl ? (
                  <Box
                    component="img"
                    src={momoData.qrCodeUrl || `https://api.qrserver.com/v1/create-qr-code/?size=220x220&data=${encodeURIComponent(momoData.payUrl)}`}
                    alt="MoMo QR Code"
                    sx={{ width: 220, height: 220, objectFit: 'contain' }}
                  />
                ) : (
                  <Box sx={{ width: 220, height: 220, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                    <Typography variant="caption" color="text.secondary">Đang tải MoMo QR...</Typography>
                  </Box>
                )
              )}

              <Typography sx={{ fontSize: '0.75rem', color: 'text.secondary', mt: 1 }}>
                Mở ứng dụng ngân hàng hoặc MoMo để quét mã
              </Typography>
            </Box>

            {/* Transaction Details (VietQR) */}
            {paymentMethod === 'vietqr' && vietQrData && (
              <Box sx={{
                width: '100%',
                bgcolor: '#f8fafc',
                borderRadius: 2.5,
                p: 2,
                border: '1px solid #e2e8f0',
                mb: 2.5
              }}>
                <DetailRow
                  label="Ngân hàng"
                  value={vietQrData.bankBin ? `BIN: ${vietQrData.bankBin}` : 'VietQR Auto Bank'}
                />
                <DetailRow
                  label="Số tài khoản"
                  value={vietQrData.accountNumber}
                  copyable
                  onCopy={() => handleCopy(vietQrData.accountNumber, 'Số tài khoản')}
                  copied={copiedField === 'Số tài khoản'}
                />
                <DetailRow
                  label="Chủ tài khoản"
                  value={vietQrData.accountName}
                />
                <DetailRow
                  label="Số tiền"
                  value={formatVND(vietQrData.amount || amount)}
                  highlight
                  copyable
                  onCopy={() => handleCopy(vietQrData.amount || amount, 'Số tiền')}
                  copied={copiedField === 'Số tiền'}
                />
                <DetailRow
                  label="Nội dung CK"
                  value={vietQrData.description || orderCode}
                  highlight
                  copyable
                  onCopy={() => handleCopy(vietQrData.description || orderCode, 'Nội dung chuyển khoản')}
                  copied={copiedField === 'Nội dung chuyển khoản'}
                />
              </Box>
            )}

            {/* MoMo deeplink action button */}
            {paymentMethod === 'momo' && momoData?.payUrl && (
              <Button
                variant="contained"
                fullWidth
                onClick={() => window.open(momoData.payUrl, '_blank')}
                sx={{
                  bgcolor: '#ae2d68',
                  color: '#fff',
                  fontWeight: 700,
                  py: 1.25,
                  borderRadius: 2,
                  mb: 2,
                  '&:hover': { bgcolor: '#912154' }
                }}
              >
                Mở ứng dụng MoMo thanh toán
              </Button>
            )}

            {/* Realtime waiting indicator */}
            <Box sx={{
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: 1,
              width: '100%',
              py: 1,
              borderRadius: 2,
              bgcolor: 'rgba(2, 132, 199, 0.06)'
            }}>
              <CircularProgress size={14} sx={{ color: '#0284c7' }} />
              <Typography sx={{ fontSize: '0.78rem', color: '#0284c7', fontWeight: 600 }}>
                Đang chờ tín hiệu thanh toán qua SignalR realtime...
              </Typography>
            </Box>

            {/* Actions */}
            <Box sx={{ display: 'flex', gap: 1.5, width: '100%', mt: 2.5 }}>
              <Button
                variant="outlined"
                fullWidth
                size="medium"
                startIcon={<RefreshRoundedIcon />}
                onClick={handleCheckManual}
                sx={{
                  borderRadius: 2,
                  fontWeight: 600,
                  fontSize: '0.82rem',
                  textTransform: 'none',
                  borderColor: 'divider',
                  color: 'text.primary'
                }}
              >
                Tôi đã chuyển khoản
              </Button>
              <Button
                variant="contained"
                fullWidth
                size="medium"
                onClick={() => {
                  onClose?.()
                  navigate('/orders')
                }}
                sx={{
                  borderRadius: 2,
                  fontWeight: 700,
                  fontSize: '0.82rem',
                  textTransform: 'none',
                  background: 'linear-gradient(90deg, #568dfb, #69aedc)',
                  color: '#fff'
                }}
              >
                Xem đơn hàng
              </Button>
            </Box>
          </Box>
        )}
      </DialogContent>
    </Dialog>
  )
}

function DetailRow({ label, value, highlight = false, copyable = false, onCopy, copied = false }) {
  if (!value) return null
  return (
    <Box sx={{
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      py: 0.6
    }}>
      <Typography sx={{ fontSize: '0.8rem', color: 'text.secondary' }}>
        {label}:
      </Typography>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <Typography sx={{
          fontSize: '0.84rem',
          fontWeight: highlight ? 700 : 600,
          color: highlight ? 'secondary.main' : 'text.primary',
          fontFamily: highlight ? 'monospace' : 'inherit'
        }}>
          {value}
        </Typography>
        {copyable && (
          <Tooltip title={copied ? 'Đã sao chép' : 'Sao chép'}>
            <IconButton size="small" onClick={onCopy} sx={{ p: 0.25, color: copied ? '#16a34a' : 'text.secondary' }}>
              <ContentCopyRoundedIcon sx={{ fontSize: 14 }} />
            </IconButton>
          </Tooltip>
        )}
      </Box>
    </Box>
  )
}
