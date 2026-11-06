import {
  Alert,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  IconButton,
  Stack,
  Typography
} from '@mui/material'
import CloseRoundedIcon from '@mui/icons-material/CloseRounded'
import AssignmentReturnRoundedIcon from '@mui/icons-material/AssignmentReturnRounded'
import GavelRoundedIcon from '@mui/icons-material/GavelRounded'
import LocalShippingRoundedIcon from '@mui/icons-material/LocalShippingRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import { formatDate, formatVND, resolveMediaUrl } from '~/common/utils/formatters'
import { REASON_MAP, RETURN_STATUS_CONFIG } from './ReturnHistoryCard'

export default function ReturnDetailModal({ open, onClose, returnItem }) {
  if (!returnItem) return null

  const cfg = RETURN_STATUS_CONFIG[returnItem.status] || {
    label: returnItem.status,
    color: '#64748b',
    bgColor: 'rgba(100,116,139,0.1)',
    borderColor: 'rgba(100,116,139,0.25)'
  }

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: {
          borderRadius: 3,
          p: 0.5,
          boxShadow: '0 10px 40px rgba(0,0,0,0.12)'
        }
      }}
    >
      <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', pb: 1.5 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <AssignmentReturnRoundedIcon sx={{ color: 'secondary.main', fontSize: 24 }} />
          <Typography variant="h6" sx={{ fontWeight: 800, fontSize: '1.05rem', color: 'primary.contrastText' }}>
            Chi Tiết Trả Hàng: #{String(returnItem.id).substring(0, 8)}
          </Typography>
        </Box>
        <IconButton size="small" onClick={onClose} sx={{ color: 'rgba(45,45,45,0.4)' }}>
          <CloseRoundedIcon fontSize="small" />
        </IconButton>
      </DialogTitle>

      <DialogContent dividers sx={{ borderColor: 'divider', py: 2.5 }}>
        <Stack spacing={2.5}>
          {/* Status Banner */}
          <Box sx={{
            p: 2,
            borderRadius: 2.5,
            bgcolor: 'rgba(83,155,255,0.04)',
            border: '1px solid',
            borderColor: 'divider',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            flexWrap: 'wrap',
            gap: 1.5
          }}>
            <Box>
              <Typography variant="caption" sx={{ color: 'rgba(45,45,45,0.5)', fontWeight: 700, display: 'block', mb: 0.5 }}>
                TRẠNG THÁI TIẾN TRÌNH
              </Typography>
              <Chip
                label={cfg.label}
                size="small"
                sx={{
                  fontWeight: 700,
                  fontSize: '0.76rem',
                  color: cfg.color,
                  bgcolor: cfg.bgColor,
                  border: '1px solid',
                  borderColor: cfg.borderColor
                }}
              />
            </Box>

            <Box sx={{ textAlign: { xs: 'left', sm: 'right' } }}>
              <Typography variant="caption" sx={{ color: 'rgba(45,45,45,0.5)', fontWeight: 700, display: 'block', mb: 0.5 }}>
                TIỀN YÊU CẦU HOÀN
              </Typography>
              <Typography variant="h6" sx={{ fontWeight: 800, color: '#ef4444', fontSize: '1.15rem' }}>
                {formatVND(returnItem.requestedRefundAmount || 0)}
              </Typography>
            </Box>
          </Box>

          {/* Admin Dispute Alert */}
          {returnItem.status === 'AdminDispute' && (
            <Alert
              severity="warning"
              icon={<GavelRoundedIcon />}
              sx={{ borderRadius: 2, fontSize: '0.82rem' }}
            >
              Yêu cầu này đang được <strong>Bộ phận Phân xử Tranh chấp NovaLive</strong> xem xét đối soát bằng chứng giữa Người mua và Shop.
            </Alert>
          )}

          {/* Warehouse Address for return shipment if approved */}
          {returnItem.returnAddress && (
            <Box sx={{
              p: 2,
              borderRadius: 2.5,
              border: '1px solid rgba(52,133,247,0.3)',
              bgcolor: 'rgba(52,133,247,0.05)'
            }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <LocalShippingRoundedIcon sx={{ color: 'secondary.main', fontSize: 18 }} />
                <Typography variant="subtitle2" sx={{ fontWeight: 800, color: 'secondary.main', fontSize: '0.85rem' }}>
                  Địa Chỉ Kho Nhận Hàng Trả Lại (Shop đã duyệt)
                </Typography>
              </Box>
              <Typography variant="body2" sx={{ fontWeight: 600, color: 'primary.contrastText', fontSize: '0.82rem' }}>
                Người nhận: {returnItem.returnAddress.contactName} • SĐT: {returnItem.returnAddress.contactPhone}
              </Typography>
              <Typography variant="body2" sx={{ color: 'rgba(45,45,45,0.65)', mt: 0.5, fontSize: '0.8rem' }}>
                {returnItem.returnAddress.warehouseName ? `Kho: ${returnItem.returnAddress.warehouseName} - ` : ''}
                {returnItem.returnAddress.detailAddress}, {returnItem.returnAddress.wardName}, {returnItem.returnAddress.districtName}, {returnItem.returnAddress.provinceName}
              </Typography>
            </Box>
          )}

          {/* Seller Feedback if any */}
          {returnItem.sellerResponse && (
            <Box sx={{
              p: 2,
              borderRadius: 2.5,
              border: '1px solid',
              borderColor: 'divider',
              bgcolor: 'background.paper'
            }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                <StorefrontRoundedIcon sx={{ fontSize: 16, color: 'rgba(45,45,45,0.5)' }} />
                <Typography variant="subtitle2" sx={{ fontWeight: 700, color: 'primary.contrastText', fontSize: '0.82rem' }}>
                  Phản hồi từ Shop:
                </Typography>
              </Box>
              <Typography variant="body2" sx={{ color: 'rgba(45,45,45,0.7)', fontStyle: 'italic', fontSize: '0.82rem' }}>
                &ldquo;{returnItem.sellerResponse}&rdquo;
              </Typography>
            </Box>
          )}

          {/* Products List */}
          <Box>
            <Typography variant="subtitle2" sx={{ fontWeight: 800, mb: 1.25, color: 'primary.contrastText', fontSize: '0.85rem' }}>
              Sản Phẩm Yêu Cầu Trả Hàng
            </Typography>
            <Stack spacing={1}>
              {(returnItem.items || []).map((prod, i) => (
                <Box
                  key={i}
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    p: 1.5,
                    borderRadius: 2,
                    border: '1px solid',
                    borderColor: 'divider',
                    bgcolor: 'rgba(83,155,255,0.02)'
                  }}
                >
                  <Box sx={{ pr: 2 }}>
                    <Typography variant="body2" sx={{ fontWeight: 700, fontSize: '0.82rem', color: 'primary.contrastText' }}>
                      {prod.spuName}
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'rgba(45,45,45,0.5)' }}>
                      Mã SKU: {prod.skuCode || 'Mặc định'} • Số lượng: x{prod.quantity}
                    </Typography>
                  </Box>
                  <Typography variant="body2" sx={{ fontWeight: 800, color: '#ef4444', fontSize: '0.85rem', flexShrink: 0 }}>
                    {formatVND((prod.refundPrice || 0) * (prod.quantity || 1))}
                  </Typography>
                </Box>
              ))}
            </Stack>
          </Box>

          {/* Evidence media */}
          {returnItem.evidenceUrls?.length > 0 && (
            <Box>
              <Typography variant="subtitle2" sx={{ fontWeight: 800, mb: 1, color: 'primary.contrastText', fontSize: '0.85rem' }}>
                Hình Ảnh / Video Bằng Chứng
              </Typography>
              <Grid container spacing={1.5}>
                {returnItem.evidenceUrls.map((rawUrl, i) => {
                  const url = resolveMediaUrl(rawUrl)
                  return (
                    <Grid item xs={4} sm={3} key={i}>
                      {/\.(mp4|webm|mov|m4v)(\?|$)/i.test(url)
                        ? <Box component="video" src={url} controls sx={{ width: '100%', height: 90, borderRadius: 2, objectFit: 'cover', border: '1px solid', borderColor: 'divider' }} />
                        : <Box component="img" src={url} alt={`Evidence ${i + 1}`} onError={(event) => { event.currentTarget.style.display = 'none' }} sx={{ width: '100%', height: 90, borderRadius: 2, objectFit: 'cover', border: '1px solid', borderColor: 'divider' }} />}
                    </Grid>
                  )
                })}
              </Grid>
            </Box>
          )}

          {/* Time & Reason info */}
          <Box sx={{ display: 'flex', justifyContent: 'space-between', flexWrap: 'wrap', gap: 1, pt: 1 }}>
            <Typography variant="caption" sx={{ color: 'rgba(45,45,45,0.45)' }}>
              Lý do: <strong>{REASON_MAP[returnItem.reason] || returnItem.reason}</strong>
            </Typography>
            <Typography variant="caption" sx={{ color: 'rgba(45,45,45,0.45)' }}>
              Tạo lúc: {formatDate(returnItem.createdAt)}
            </Typography>
          </Box>
        </Stack>
      </DialogContent>

      <DialogActions sx={{ p: 2 }}>
        <Button
          onClick={onClose}
          sx={{
            textTransform: 'none',
            fontWeight: 600,
            fontSize: '0.82rem',
            color: 'rgba(45,45,45,0.6)'
          }}
        >
          Đóng
        </Button>
      </DialogActions>
    </Dialog>
  )
}
