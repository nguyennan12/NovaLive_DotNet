import { useState } from 'react'
import {
  Alert,
  Avatar,
  Box,
  Button,
  Chip,
  Divider,
  Grid,
  IconButton,
  Paper,
  Stack,
  Tooltip,
  Typography
} from '@mui/material'
import ArrowBackRounded from '@mui/icons-material/ArrowBackRounded'
import ContentCopyRounded from '@mui/icons-material/ContentCopyRounded'
import CalendarTodayRounded from '@mui/icons-material/CalendarTodayRounded'
import LocalShippingRounded from '@mui/icons-material/LocalShippingRounded'
import WarehouseRounded from '@mui/icons-material/WarehouseRounded'
import ReceiptLongRounded from '@mui/icons-material/ReceiptLongRounded'
import PlaceRounded from '@mui/icons-material/PlaceRounded'
import PhoneRounded from '@mui/icons-material/PhoneRounded'
import CheckCircleRounded from '@mui/icons-material/CheckCircleRounded'
import CancelOutlined from '@mui/icons-material/CancelOutlined'
import AssignmentReturnRounded from '@mui/icons-material/AssignmentReturnRounded'
import AltRouteRounded from '@mui/icons-material/AltRouteRounded'
import LiveTvRounded from '@mui/icons-material/LiveTvRounded'
import ShoppingBagRounded from '@mui/icons-material/ShoppingBagRounded'
import TimelineRounded from '@mui/icons-material/TimelineRounded'
import InfoOutlined from '@mui/icons-material/InfoOutlined'
import StorefrontRounded from '@mui/icons-material/StorefrontRounded'
import { Link, useParams } from 'react-router-dom'
import { toast } from 'react-toastify'
import { useOrder } from '../hooks/useOrders'
import { useShopPermissions } from '../hooks/useShopPermissions'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { QueryState } from '../components/shared/QueryState'
import { StatusBadge } from '../components/shared/StatusBadge'
import { DateText } from '../components/shared/DateText'
import { MoneyText } from '../components/shared/MoneyText'
import { OrderActionDialogs } from '../components/orders/OrderActionDialogs'
import { TrackingPanel } from '../components/orders/TrackingPanel'
import { snapshot } from '../utils/format'
import { sellerBtnStyles } from '../theme/sellerBtnStyles'

export default function OrderDetailPage() {
  const { id } = useParams()
  const query = useOrder(id)
  const order = query.data
  const [action, setAction] = useState<'confirm' | 'cancel' | 'ship' | 'deliver' | null>(null)
  const [tracking, setTracking] = useState(false)
  const { canOperate } = useShopPermissions()

  const shipped = Boolean(
    order && ['Shipping', 'Delivered', 'Completed', 'ReturnRequested', 'Returned', 'Refunded'].includes(order.status)
  )
  const paid = Boolean(
    order && (order.paymentMethod === 'COD' || ['Paid', 'Success'].includes(order.paymentStatus))
  )

  const copyCode = () => {
    if (order?.code) {
      navigator.clipboard.writeText(order.code)
      toast.success('Đã sao chép mã đơn hàng!')
    }
  }

  const roleLabel = (role?: string) => {
    const map: Record<string, string> = {
      Seller: 'Shop',
      Buyer: 'Người mua',
      System: 'Hệ thống',
      Admin: 'Quản trị viên'
    }
    return map[role || ''] || role || 'Hệ thống'
  }

  return (
    <>
      <PageHeader
        title={order ? `Đơn hàng #${order.code}` : 'Chi tiết đơn hàng'}
        description="Quản lý chi tiết đơn hàng, trạng thái xử lý và vận chuyển."
        action={
          <Button
            component={Link}
            to="/seller/orders"
            variant="outlined"
            size="small"
            startIcon={<ArrowBackRounded fontSize="small" />}
            sx={{
              ...sellerBtnStyles.transSecondary,
              px: 2,
              py: 0.6,
              fontSize: '0.82rem'
            }}
          >
            Về danh sách
          </Button>
        }
      />

      <QueryState loading={query.isLoading} error={query.error} onRetry={query.refetch}>
        {order && (
          <Stack spacing={2.5}>
            {/* Top Order Status & Action Toolbar Card */}
            <SellerCard>
              <Box
                sx={{
                  display: 'flex',
                  flexWrap: 'wrap',
                  alignItems: 'center',
                  justifyContent: 'space-between',
                  gap: 2
                }}
              >
                {/* Left: Status & Metadata Badges */}
                <Stack direction="row" spacing={1.5} alignItems="center" flexWrap="wrap" sx={{ gap: 1 }}>
                  <StatusBadge status={order.status} />

                  <Chip
                    icon={<ContentCopyRounded sx={{ fontSize: '13px !important' }} />}
                    label={order.code}
                    onClick={copyCode}
                    size="small"
                    sx={{
                      fontFamily: 'monospace',
                      fontWeight: 800,
                      letterSpacing: '0.05em',
                      cursor: 'pointer',
                      bgcolor: '#f1f5f9',
                      color: '#0f172a',
                      borderRadius: '8px',
                      '&:hover': { bgcolor: '#e2e8f0' }
                    }}
                  />

                  <Chip
                    label={order.paymentMethod === 'COD' ? 'Thanh toán COD' : order.paymentMethod}
                    size="small"
                    sx={{
                      fontWeight: 700,
                      fontSize: '0.75rem',
                      bgcolor: order.paymentMethod === 'COD' ? '#fef3c7' : '#eff6ff',
                      color: order.paymentMethod === 'COD' ? '#b45309' : '#1d4ed8',
                      borderRadius: '8px'
                    }}
                  />

                  {order.orderSource === 'Livestream' ? (
                    <Chip
                      icon={<LiveTvRounded sx={{ fontSize: '14px !important', color: '#ef4444 !important' }} />}
                      label="Livestream"
                      size="small"
                      sx={{
                        fontWeight: 700,
                        fontSize: '0.75rem',
                        bgcolor: '#fef2f2',
                        color: '#b91c1c',
                        borderRadius: '8px'
                      }}
                    />
                  ) : (
                    <Chip
                      label="Trực tuyến"
                      size="small"
                      sx={{
                        fontWeight: 600,
                        fontSize: '0.75rem',
                        bgcolor: '#f8fafc',
                        color: '#64748b',
                        borderRadius: '8px'
                      }}
                    />
                  )}

                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: '#64748b', fontSize: '0.8rem' }}>
                    <CalendarTodayRounded sx={{ fontSize: 14 }} />
                    <DateText value={order.createdAt} />
                  </Box>
                </Stack>

                {/* Right: Operational Action Buttons */}
                <Stack direction="row" spacing={1} flexWrap="wrap" sx={{ gap: 1 }}>
                  {order.status === 'ReturnRequested' && (
                    <Button
                      component={Link}
                      to={`/seller/returns?subOrderId=${order.id}`}
                      variant="contained"
                      size="small"
                      startIcon={<AssignmentReturnRounded fontSize="small" />}
                      sx={{
                        ...sellerBtnStyles.warning,
                        px: 2,
                        py: 0.75,
                        fontSize: '0.85rem'
                      }}
                    >
                      Mở yêu cầu trả hàng
                    </Button>
                  )}

                  {order.status === 'Confirmed' && (
                    <Button
                      variant="outlined"
                      size="small"
                      disabled={!canOperate}
                      onClick={() => setAction('confirm')}
                      startIcon={<WarehouseRounded fontSize="small" />}
                      sx={{
                        ...sellerBtnStyles.transSecondary,
                        px: 2,
                        py: 0.75,
                        fontSize: '0.85rem'
                      }}
                    >
                      {order.warehouseAddressId ? 'Đổi kho lấy hàng' : 'Xác nhận & Chọn kho'}
                    </Button>
                  )}

                  {order.status === 'Confirmed' && paid && (
                    <Button
                      variant="contained"
                      size="small"
                      disabled={!canOperate || !order.warehouseAddressId}
                      onClick={() => setAction('ship')}
                      startIcon={<LocalShippingRounded fontSize="small" />}
                      sx={{
                        ...sellerBtnStyles.primary,
                        px: 2.2,
                        py: 0.75,
                        fontSize: '0.85rem'
                      }}
                    >
                      Giao hàng
                    </Button>
                  )}

                  {order.status === 'Shipping' && (
                    <Button
                      variant="contained"
                      size="small"
                      disabled={!canOperate}
                      onClick={() => setAction('deliver')}
                      startIcon={<CheckCircleRounded fontSize="small" />}
                      sx={{
                        ...sellerBtnStyles.success,
                        px: 2.2,
                        py: 0.75,
                        fontSize: '0.85rem'
                      }}
                    >
                      Xác nhận đã giao
                    </Button>
                  )}

                  {['Delivered', 'ReturnRequested'].includes(order.status) && order.paymentMethod === 'COD' && (
                    <Button
                      variant="outlined"
                      size="small"
                      disabled={!canOperate}
                      onClick={() => setAction('deliver')}
                      sx={{
                        ...sellerBtnStyles.transSecondary,
                        px: 2,
                        py: 0.75,
                        fontSize: '0.85rem'
                      }}
                    >
                      Đối soát COD đã giao
                    </Button>
                  )}

                  {shipped && (
                    <Button
                      variant="outlined"
                      size="small"
                      onClick={() => setTracking(!tracking)}
                      startIcon={<AltRouteRounded fontSize="small" />}
                      sx={{
                        ...sellerBtnStyles.transSecondary,
                        px: 2,
                        py: 0.75,
                        fontSize: '0.85rem'
                      }}
                    >
                      {tracking ? 'Ẩn hành trình' : 'Xem hành trình'}
                    </Button>
                  )}

                  {['PendingPayment', 'Confirmed', 'Processing'].includes(order.status) && (
                    <Button
                      variant="outlined"
                      size="small"
                      onClick={() => setAction('cancel')}
                      startIcon={<CancelOutlined fontSize="small" />}
                      sx={{
                        ...sellerBtnStyles.transDanger,
                        px: 2,
                        py: 0.75,
                        fontSize: '0.85rem'
                      }}
                    >
                      Hủy đơn
                    </Button>
                  )}
                </Stack>
              </Box>

              {/* Status Specific Alerts */}
              {order.status === 'ReturnRequested' && (
                <Alert severity="warning" sx={{ mt: 2, borderRadius: '10px' }}>
                  Người mua đã gửi yêu cầu trả hàng. Mở yêu cầu để xem bằng chứng, duyệt hoặc từ chối; sau khi duyệt,
                  xác nhận nhận hàng hoàn tại cùng màn hình.
                </Alert>
              )}
              {order.status === 'PendingPayment' && (
                <Alert severity="info" sx={{ mt: 2, borderRadius: '10px' }}>
                  Đơn hàng thanh toán trực tuyến chưa thanh toán thành công. Chưa thể chọn kho lấy hàng hoặc bàn giao
                  vận chuyển.
                </Alert>
              )}
              {order.cancelReason && (
                <Alert severity="error" sx={{ mt: 2, borderRadius: '10px' }}>
                  Đơn hàng đã hủy. Lý do hủy: <strong>{order.cancelReason}</strong>
                </Alert>
              )}
            </SellerCard>

            {/* Tracking Panel Accordion/View */}
            {tracking && <TrackingPanel id={order.id} />}

            {/* Order Items Table Card */}
            <SellerCard
              title={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <ShoppingBagRounded sx={{ color: '#2563eb', fontSize: 20 }} />
                  <span>Sản phẩm trong đơn</span>
                  <Chip
                    label={`${order.items.length} mặt hàng`}
                    size="small"
                    sx={{ height: 22, fontSize: '0.72rem', fontWeight: 700, bgcolor: '#eff6ff', color: '#2563eb' }}
                  />
                </Box>
              }
            >
              <Box sx={{ overflowX: 'auto' }}>
                <Box
                  component="table"
                  sx={{
                    width: '100%',
                    borderCollapse: 'collapse',
                    textAlign: 'left',
                    '& th': {
                      py: 1.5,
                      px: 2,
                      fontSize: '0.75rem',
                      fontWeight: 700,
                      color: '#64748b',
                      textTransform: 'uppercase',
                      letterSpacing: '0.05em',
                      borderBottom: '1px solid #e2e8f0',
                      bgcolor: '#f8fafc'
                    },
                    '& td': {
                      py: 2,
                      px: 2,
                      borderBottom: '1px solid #f1f5f9',
                      fontSize: '0.875rem'
                    }
                  }}
                >
                  <thead>
                    <tr>
                      <th style={{ borderRadius: '8px 0 0 8px' }}>Sản phẩm (Bản chụp lúc mua)</th>
                      <th style={{ textAlign: 'center', width: 120 }}>Số lượng</th>
                      <th style={{ textAlign: 'right', width: 150 }}>Đơn giá</th>
                      <th style={{ textAlign: 'right', width: 160, borderRadius: '0 8px 8px 0' }}>Thành tiền</th>
                    </tr>
                  </thead>
                  <tbody>
                    {order.items.map((row) => {
                      const item = snapshot(row.skuSnapshotJson)
                      return (
                        <tr key={row.id}>
                          <td>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                              <Avatar
                                src={item.image}
                                variant="rounded"
                                alt={item.name}
                                sx={{
                                  width: 58,
                                  height: 58,
                                  borderRadius: '10px',
                                  border: '1px solid #e2e8f0',
                                  bgcolor: '#f1f5f9',
                                  flexShrink: 0
                                }}
                              >
                                {item.name[0]}
                              </Avatar>
                              <Box sx={{ minWidth: 0 }}>
                                <Typography
                                  sx={{
                                    fontWeight: 700,
                                    color: '#0f172a',
                                    fontSize: '0.92rem',
                                    lineHeight: 1.4,
                                    mb: 0.5
                                  }}
                                >
                                  {item.name}
                                </Typography>
                                {item.variant && (
                                  <Typography
                                    variant="caption"
                                    sx={{
                                      display: 'inline-block',
                                      px: 1,
                                      py: 0.2,
                                      bgcolor: '#f1f5f9',
                                      color: '#475569',
                                      borderRadius: '6px',
                                      fontFamily: 'monospace',
                                      fontSize: '0.75rem',
                                      fontWeight: 600
                                    }}
                                  >
                                    {item.variant}
                                  </Typography>
                                )}
                              </Box>
                            </Box>
                          </td>
                          <td style={{ textAlign: 'center' }}>
                            <Chip
                              label={`x${row.quantity}`}
                              size="small"
                              sx={{
                                fontWeight: 800,
                                fontSize: '0.8rem',
                                bgcolor: '#f8fafc',
                                color: '#1e293b'
                              }}
                            />
                          </td>
                          <td style={{ textAlign: 'right', color: '#475569', fontWeight: 600 }}>
                            <MoneyText value={row.unitPrice} />
                          </td>
                          <td style={{ textAlign: 'right', fontWeight: 800, color: '#0f172a' }}>
                            <MoneyText value={row.lineTotal} />
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </Box>
              </Box>
            </SellerCard>

            {/* Two Column Grid: Recipient Info & Payment Details */}
            <Grid container spacing={2.5}>
              {/* Left: Shipping & Recipient Card */}
              <Grid size={{ xs: 12, md: 6 }}>
                <SellerCard
                  title={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <LocalShippingRounded sx={{ color: '#2563eb', fontSize: 20 }} />
                      <span>Thông tin nhận hàng</span>
                    </Box>
                  }
                >
                  <Stack spacing={2}>
                    {/* Recipient box */}
                    <Box
                      sx={{
                        p: 2,
                        borderRadius: '12px',
                        bgcolor: '#f8fafc',
                        border: '1px solid #e2e8f0'
                      }}
                    >
                      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
                        <Typography sx={{ fontWeight: 800, color: '#0f172a', fontSize: '0.95rem' }}>
                          {order.recipient.recipientName}
                        </Typography>
                        <Chip
                          icon={<PhoneRounded sx={{ fontSize: '12px !important', color: '#2563eb !important' }} />}
                          label={order.recipient.phone}
                          size="small"
                          sx={{
                            fontWeight: 700,
                            fontFamily: 'monospace',
                            bgcolor: '#eff6ff',
                            color: '#1d4ed8'
                          }}
                        />
                      </Box>
                      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1, color: '#475569' }}>
                        <PlaceRounded sx={{ fontSize: 18, color: '#64748b', mt: 0.2, flexShrink: 0 }} />
                        <Typography sx={{ fontSize: '0.875rem', lineHeight: 1.5 }}>
                          {[
                            order.recipient.detailAddress,
                            order.recipient.wardName,
                            order.recipient.districtName,
                            order.recipient.provinceName
                          ]
                            .filter(Boolean)
                            .join(', ')}
                        </Typography>
                      </Box>
                    </Box>

                    {/* Warehouse & Delivery Status */}
                    {order.warehouseAddressId ? (
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, p: 1.5, borderRadius: '8px', bgcolor: '#f0fdf4', border: '1px solid #bbf7d0' }}>
                        <StorefrontRounded sx={{ fontSize: 18, color: '#16a34a' }} />
                        <Typography sx={{ fontSize: '0.8rem', color: '#15803d', fontWeight: 600 }}>
                          Đã chọn kho xuất hàng cho đơn này
                        </Typography>
                      </Box>
                    ) : (
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, p: 1.5, borderRadius: '8px', bgcolor: '#fffbeb', border: '1px solid #fde68a' }}>
                        <WarehouseRounded sx={{ fontSize: 18, color: '#d97706' }} />
                        <Typography sx={{ fontSize: '0.8rem', color: '#b45309', fontWeight: 600 }}>
                          Chưa phân bổ kho lấy hàng. Vui lòng bấm &quot;Xác nhận &amp; Chọn kho&quot;.
                        </Typography>
                      </Box>
                    )}
                  </Stack>
                </SellerCard>
              </Grid>

              {/* Right: Payment Breakdown Card */}
              <Grid size={{ xs: 12, md: 6 }}>
                <SellerCard
                  title={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <ReceiptLongRounded sx={{ color: '#2563eb', fontSize: 20 }} />
                      <span>Chi tiết thanh toán</span>
                    </Box>
                  }
                >
                  <Stack spacing={1.5}>
                    {[
                      { label: 'Tiền hàng', value: order.itemAmount, isDiscount: false },
                      { label: 'Phí vận chuyển', value: order.shippingFee, isDiscount: false },
                      { label: 'Voucher Shop', value: -order.shopDiscountAmount, isDiscount: true },
                      { label: 'Voucher sàn', value: -order.platformDiscountAmount, isDiscount: true },
                      { label: 'Giảm phí vận chuyển', value: -order.shippingDiscountAmount, isDiscount: true }
                    ].map((row) => {
                      const num = Number(row.value)
                      return (
                        <Box
                          key={row.label}
                          sx={{
                            display: 'flex',
                            justifyContent: 'space-between',
                            alignItems: 'center',
                            py: 0.25
                          }}
                        >
                          <Typography sx={{ fontSize: '0.875rem', color: '#64748b' }}>
                            {row.label}
                          </Typography>
                          <Typography
                            component="div"
                            sx={{
                              fontSize: '0.875rem',
                              fontWeight: 600,
                              color: row.isDiscount && num < 0 ? '#16a34a' : '#0f172a'
                            }}
                          >
                            <MoneyText value={num} />
                          </Typography>
                        </Box>
                      )
                    })}

                    <Divider sx={{ my: 1, borderColor: '#e2e8f0' }} />

                    {/* Total Grand Amount */}
                    <Box
                      sx={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'center',
                        pt: 0.5
                      }}
                    >
                      <Typography sx={{ fontWeight: 800, fontSize: '1rem', color: '#0f172a' }}>
                        Tổng thanh toán
                      </Typography>
                      <Typography
                        component="div"
                        sx={{
                          fontSize: '1.35rem',
                          fontWeight: 900,
                          color: '#2563eb',
                          letterSpacing: '-0.02em'
                        }}
                      >
                        <MoneyText value={order.subTotal} />
                      </Typography>
                    </Box>

                    {/* Notice box */}
                    <Box
                      sx={{
                        display: 'flex',
                        alignItems: 'flex-start',
                        gap: 1.25,
                        p: 1.5,
                        mt: 1,
                        borderRadius: '10px',
                        bgcolor: '#f0f9ff',
                        border: '1px solid #bae6fd'
                      }}
                    >
                      <InfoOutlined sx={{ fontSize: 18, color: '#0284c7', mt: 0.2, flexShrink: 0 }} />
                      <Typography sx={{ fontSize: '0.78rem', color: '#0369a1', lineHeight: 1.5 }}>
                        Tiền Shop thực nhận chưa có trong API chi tiết đơn. Doanh thu được ghi nhận ở sổ ví khi đủ điều
                        kiện thanh toán.
                      </Typography>
                    </Box>
                  </Stack>
                </SellerCard>
              </Grid>
            </Grid>

            {/* Timeline Card */}
            <SellerCard
              title={
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <TimelineRounded sx={{ color: '#2563eb', fontSize: 20 }} />
                  <span>Lịch sử trạng thái</span>
                </Box>
              }
            >
              <Stack spacing={2} sx={{ position: 'relative', pl: 1 }}>
                {order.timeline.map((item, index) => (
                  <Box
                    key={index}
                    sx={{
                      display: 'flex',
                      gap: 2,
                      position: 'relative',
                      pb: index !== order.timeline.length - 1 ? 2 : 0,
                      '&::before': index !== order.timeline.length - 1 ? {
                        content: '""',
                        position: 'absolute',
                        left: 11,
                        top: 24,
                        bottom: 0,
                        width: 2,
                        bgcolor: '#e2e8f0'
                      } : {}
                    }}
                  >
                    {/* Node circle */}
                    <Box
                      sx={{
                        width: 24,
                        height: 24,
                        borderRadius: '50%',
                        bgcolor: index === 0 ? '#eff6ff' : '#f1f5f9',
                        border: '2px solid',
                        borderColor: index === 0 ? '#3b82f6' : '#94a3b8',
                        display: 'grid',
                        placeItems: 'center',
                        flexShrink: 0,
                        zIndex: 1
                      }}
                    >
                      <Box
                        sx={{
                          width: 8,
                          height: 8,
                          borderRadius: '50%',
                          bgcolor: index === 0 ? '#2563eb' : '#94a3b8'
                        }}
                      />
                    </Box>

                    {/* Content */}
                    <Box sx={{ flex: 1, minWidth: 0, pt: 0.2 }}>
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap', mb: 0.5 }}>
                        <DateText value={item.changedAt} />
                        <Typography variant="caption" sx={{ color: '#94a3b8' }}>•</Typography>
                        <StatusBadge status={item.toStatus} />
                        <Typography variant="caption" sx={{ color: '#94a3b8' }}>•</Typography>
                        <Chip
                          label={roleLabel(item.changedByRole)}
                          size="small"
                          sx={{
                            height: 20,
                            fontSize: '0.68rem',
                            fontWeight: 700,
                            bgcolor: '#f1f5f9',
                            color: '#475569'
                          }}
                        />
                      </Box>
                      {item.note && (
                        <Typography sx={{ fontSize: '0.85rem', color: '#475569', mt: 0.5 }}>
                          {item.note}
                        </Typography>
                      )}
                    </Box>
                  </Box>
                ))}
              </Stack>
            </SellerCard>

            {/* Action Dialogs */}
            {action && (
              <OrderActionDialogs
                key={action}
                order={order}
                action={action}
                onClose={() => setAction(null)}
              />
            )}
          </Stack>
        )}
      </QueryState>
    </>
  )
}
