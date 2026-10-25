import { useState } from 'react'
import {
  Box,
  Grid,
  Card,
  Typography,
  Chip,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  ButtonGroup,
  LinearProgress,
  Stack
} from '@mui/material'
import TrendingUpRoundedIcon from '@mui/icons-material/TrendingUpRounded'
import AccountBalanceWalletRoundedIcon from '@mui/icons-material/AccountBalanceWalletRounded'
import GavelRoundedIcon from '@mui/icons-material/GavelRounded'
import PeopleAltRoundedIcon from '@mui/icons-material/PeopleAltRounded'
import WhatshotRoundedIcon from '@mui/icons-material/WhatshotRounded'
import {
  AreaChart,
  Area,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip as RechartsTooltip,
  ResponsiveContainer,
  Legend
} from 'recharts'
import { useNavigate } from 'react-router-dom'
import { adminBtnStyles } from '../../theme/adminButtonStyles'

// 7-day revenue data
const REVENUE_DATA_7D = [
  { date: '17/10 (T6)', gmv: 145000000, fee: 7250000, orders: 430, liveGmv: 98000000 },
  { date: '18/10 (T7)', gmv: 182000000, fee: 9100000, orders: 512, liveGmv: 125000000 },
  { date: '19/10 (CN)', gmv: 168000000, fee: 8400000, orders: 480, liveGmv: 114000000 },
  { date: '20/10 (T2)', gmv: 215000000, fee: 10750000, orders: 620, liveGmv: 152000000 },
  { date: '21/10 (T3)', gmv: 190000000, fee: 9500000, orders: 540, liveGmv: 131000000 },
  { date: '22/10 (T4)', gmv: 240000000, fee: 12000000, orders: 690, liveGmv: 178000000 },
  { date: '23/10 (Hôm nay)', gmv: 280000000, fee: 14000000, orders: 780, liveGmv: 210000000 }
]

// 30-day revenue data
const REVENUE_DATA_30D = [
  { date: 'Tuần 1 (01-07)', gmv: 890000000, fee: 44500000, orders: 2540, liveGmv: 590000000 },
  { date: 'Tuần 2 (08-14)', gmv: 1050000000, fee: 52500000, orders: 3100, liveGmv: 720000000 },
  { date: 'Tuần 3 (15-21)', gmv: 1240000000, fee: 62000000, orders: 3680, liveGmv: 880000000 },
  { date: 'Tuần 4 (22-28)', gmv: 1420000000, fee: 71000000, orders: 4052, liveGmv: 1008000000 }
]

// Category breakdown data with comprehensive matrix attributes
const CATEGORY_DETAILS = [
  {
    name: 'Thời Trang & Phụ Kiện',
    value: 42,
    color: '#0095ff', // Brand secondary blue
    revenue: '596,400,000 đ',
    orders: '1,701 đơn',
    growth: '+18.2%',
    aov: '350,600 đ',
    liveShare: '76%',
    status: 'Tăng trưởng nóng',
    statusColor: '#10b981'
  },
  {
    name: 'Điện Thoại & Phụ Kiện',
    value: 28,
    color: '#14c3eb', // Brand gradient cyan
    revenue: '397,600,000 đ',
    orders: '1,134 đơn',
    growth: '+12.5%',
    aov: '350,600 đ',
    liveShare: '64%',
    status: 'Ổn định cao',
    statusColor: '#0095ff'
  },
  {
    name: 'Thiết Bị Điện Tử & Gia Dụng',
    value: 18,
    color: '#8b5cf6', // Third purple
    revenue: '255,600,000 đ',
    orders: '729 đơn',
    growth: '+9.8%',
    aov: '350,600 đ',
    liveShare: '58%',
    status: 'Tiềm năng lớn',
    statusColor: '#8b5cf6'
  },
  {
    name: 'Mỹ Phẩm & Chăm Sóc Da',
    value: 12,
    color: '#10b981', // Emerald green
    revenue: '170,400,000 đ',
    orders: '486 đơn',
    growth: '+15.4%',
    aov: '350,600 đ',
    liveShare: '82%',
    status: 'Đang mở rộng',
    statusColor: '#f59e0b'
  }
]

// Hourly traffic & live orders
const HOURLY_TRAFFIC_DATA = [
  { hour: '00:00 - 04:00', ccu: 2100, orders: 85, liveSales: '12,500,000 đ' },
  { hour: '04:00 - 08:00', ccu: 1450, orders: 42, liveSales: '6,200,000 đ' },
  { hour: '08:00 - 12:00', ccu: 5800, orders: 240, liveSales: '38,000,000 đ' },
  { hour: '12:00 - 16:00 (Deal trưa)', ccu: 8900, orders: 410, liveSales: '64,500,000 đ' },
  { hour: '16:00 - 19:00', ccu: 7200, orders: 320, liveSales: '48,000,000 đ' },
  { hour: '19:00 - 21:00 (Giờ vàng Live)', ccu: 16800, orders: 850, liveSales: '142,000,000 đ' },
  { hour: '21:00 - 24:00 (Đỉnh điểm Live)', ccu: 14250, orders: 690, liveSales: '112,000,000 đ' }
]

export const DashboardOverviewTab = ({ metrics, payouts, onApprovePayout, onRejectPayout }) => {
  const navigate = useNavigate()
  const [timeRange, setTimeRange] = useState('7d')

  const chartData = timeRange === '7d' ? REVENUE_DATA_7D : REVENUE_DATA_30D

  // Real API metrics with fallback
  const displayGmv = metrics ? `${Number(metrics.totalGmv || 0).toLocaleString('vi-VN')} đ` : '1,420,000,000 đ'
  const displayFee = metrics ? `${Number(metrics.totalPlatformFeeRevenue || 0).toLocaleString('vi-VN')} đ` : '71,000,000 đ'
  const displayOrders = metrics ? `${metrics.totalOrdersCount || 0} đơn thanh toán` : '4,052 đơn thanh toán'
  const displayReturnRate = metrics ? `${metrics.returnRatePercent || 0}%` : '1.6%'
  const displayDisputeRate = metrics ? `Tranh chấp: ${metrics.disputeRatePercent || 0}%` : 'Tranh chấp: 0.4%'
  const displayNewBuyers = metrics ? `+${metrics.newBuyerCount || 0}` : '+1,248'
  const displayNewSellers = metrics ? `+${metrics.newSellerCount || 0} Shop mới` : '+35 Shop mới'
  const displayActiveShops = metrics ? `${metrics.totalActiveShopsCount || 0} Shop Active` : '142 Shop Active'

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3.5, width: '100%' }}>
      {/* 1. Header Filter Bar */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 1.5 }}>
        <Box>
          <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
            Chỉ Số Hiệu Suất Nền Tảng (Executive Performance Overview)
          </Typography>
          <Typography variant="caption" sx={{ color: '#64748b' }}>
            Dữ liệu tài chính, lưu lượng người xem livestream và cơ cấu doanh thu toàn sàn
          </Typography>
        </Box>
        <ButtonGroup size="small" sx={{ bgcolor: '#ffffff', borderRadius: '8px', p: 0.3, border: '1px solid #e2e8f0', boxShadow: '0 1px 4px rgba(0,0,0,0.04)' }}>
          <Button
            onClick={() => setTimeRange('7d')}
            sx={timeRange === '7d' ? adminBtnStyles.primary : adminBtnStyles.transSecondary}
          >
            7 ngày qua
          </Button>
          <Button
            onClick={() => setTimeRange('30d')}
            sx={timeRange === '30d' ? adminBtnStyles.primary : adminBtnStyles.transSecondary}
          >
            30 ngày qua (Theo tuần)
          </Button>
        </ButtonGroup>
      </Box>

      {/* 2. Top 4 Core Metric KPI Cards */}
      <Grid container spacing={2.5}>
        {/* Total GMV */}
        <Grid item xs={12} sm={6} lg={3}>
          <Card
            sx={{
              p: 2.5,
              borderRadius: '16px',
              border: '1px solid #e2e8f0',
              bgcolor: '#ffffff',
              height: '100%',
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'space-between',
              boxShadow: '0 2px 10px rgba(0,0,0,0.02)'
            }}
          >
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Tổng GMV Toàn Sàn
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 800, color: '#0f172a', mt: 0.5, whiteSpace: 'nowrap' }}>
                  {displayGmv}
                </Typography>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(0, 149, 255, 0.1)', color: '#0095ff', flexShrink: 0 }}>
                <TrendingUpRoundedIcon />
              </Box>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2, flexWrap: 'wrap' }}>
              <Chip label="+16.4%" size="small" sx={{ bgcolor: 'rgba(22, 163, 74, 0.1)', color: '#16a34a', fontWeight: 700, height: 22, whiteSpace: 'nowrap' }} />
              <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>{displayOrders}</Typography>
            </Box>
          </Card>
        </Grid>

        {/* Platform Net Revenue (Fee 5%) */}
        <Grid item xs={12} sm={6} lg={3}>
          <Card
            sx={{
              p: 2.5,
              borderRadius: '16px',
              border: '1px solid #e2e8f0',
              bgcolor: '#ffffff',
              height: '100%',
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'space-between',
              boxShadow: '0 2px 10px rgba(0,0,0,0.02)'
            }}
          >
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Doanh Thu Phí Sàn (Net)
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 800, color: '#10b981', mt: 0.5, whiteSpace: 'nowrap' }}>
                  {displayFee}
                </Typography>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(16, 185, 129, 0.1)', color: '#10b981', flexShrink: 0 }}>
                <AccountBalanceWalletRoundedIcon />
              </Box>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2, flexWrap: 'wrap' }}>
              <Chip label="5% Hoa hồng" size="small" sx={{ bgcolor: 'rgba(16, 185, 129, 0.1)', color: '#10b981', fontWeight: 700, height: 22, whiteSpace: 'nowrap' }} />
              <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>Thực thu sau hoàn hàng</Typography>
            </Box>
          </Card>
        </Grid>

        {/* Return Rate & Dispute Rate */}
        <Grid item xs={12} sm={6} lg={3}>
          <Card
            sx={{
              p: 2.5,
              borderRadius: '16px',
              border: '1px solid #e2e8f0',
              bgcolor: '#ffffff',
              height: '100%',
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'space-between',
              boxShadow: '0 2px 10px rgba(0,0,0,0.02)'
            }}
          >
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box sx={{ width: '100%', pr: 1 }}>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Tỷ Lệ Hoàn & Tranh Chấp
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: 0.5 }}>
                  <Typography variant="h5" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
                    {displayReturnRate}
                  </Typography>
                  <Chip
                    label={displayDisputeRate}
                    size="small"
                    sx={{ bgcolor: 'rgba(239, 68, 68, 0.08)', color: '#ef4444', fontWeight: 700, fontSize: '0.72rem', height: 22, whiteSpace: 'nowrap' }}
                  />
                </Box>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(245, 158, 11, 0.1)', color: '#f59e0b', flexShrink: 0 }}>
                <GavelRoundedIcon />
              </Box>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2, flexWrap: 'wrap' }}>
              <Chip label="Đạt chuẩn < 2%" size="small" sx={{ bgcolor: 'rgba(22, 163, 74, 0.1)', color: '#16a34a', fontWeight: 700, height: 22, whiteSpace: 'nowrap' }} />
              <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>Kiểm soát tốt rủi ro</Typography>
            </Box>
          </Card>
        </Grid>

        {/* New Users & Shops */}
        <Grid item xs={12} sm={6} lg={3}>
          <Card
            sx={{
              p: 2.5,
              borderRadius: '16px',
              border: '1px solid #e2e8f0',
              bgcolor: '#ffffff',
              height: '100%',
              display: 'flex',
              flexDirection: 'column',
              justifyContent: 'space-between',
              boxShadow: '0 2px 10px rgba(0,0,0,0.02)'
            }}
          >
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <Box sx={{ width: '100%', pr: 1 }}>
                <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 700, textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
                  Người Dùng & Gian Hàng Mới
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mt: 0.5 }}>
                  <Typography variant="h5" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
                    {displayNewBuyers}
                  </Typography>
                  <Chip
                    label={displayNewSellers}
                    size="small"
                    sx={{ bgcolor: 'rgba(0, 149, 255, 0.08)', color: '#0095ff', fontWeight: 700, fontSize: '0.72rem', height: 22, whiteSpace: 'nowrap' }}
                  />
                </Box>
              </Box>
              <Box sx={{ p: 1.2, borderRadius: '12px', bgcolor: 'rgba(139, 92, 246, 0.1)', color: '#8b5cf6', flexShrink: 0 }}>
                <PeopleAltRoundedIcon />
              </Box>
            </Box>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2, flexWrap: 'wrap' }}>
              <Chip label={displayActiveShops} size="small" sx={{ bgcolor: '#f1f5f9', color: '#475569', fontWeight: 600, height: 22, whiteSpace: 'nowrap' }} />
              <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>Tăng trưởng tuần +12%</Typography>
            </Box>
          </Card>
        </Grid>
      </Grid>

      {/* 3. BIỂU ĐỒ FULL-WIDTH 1: GMV & DOANH THU HOA HỒNG SÀN */}
      <Card sx={{ p: { xs: 2, sm: 3 }, borderRadius: '18px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 4px 20px rgba(0,0,0,0.03)', width: '100%' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 2, mb: 3 }}>
          <Box>
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', fontSize: '1.2rem', whiteSpace: 'nowrap' }}>
              Biểu Đồ GMV Toàn Sàn & Doanh Thu Hoa Hồng Thực Nhận
            </Typography>
            <Typography variant="body2" sx={{ color: '#64748b', mt: 0.3 }}>
              So sánh dòng tiền tổng giá trị hàng hóa giao dịch (GMV), doanh thu phí sàn 5% và doanh số phát sinh từ Livestream
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            <Chip
              label="GMV: 1,420,000,000 đ"
              size="small"
              sx={{ bgcolor: 'rgba(0, 149, 255, 0.08)', color: '#0095ff', fontWeight: 700, px: 0.5, whiteSpace: 'nowrap' }}
            />
            <Chip
              label="Phí sàn: 71,000,000 đ"
              size="small"
              sx={{ bgcolor: 'rgba(16, 185, 129, 0.1)', color: '#10b981', fontWeight: 700, px: 0.5, whiteSpace: 'nowrap' }}
            />
            <Chip
              label="Từ Live: 1,008,000,000 đ (71%)"
              size="small"
              sx={{ bgcolor: 'rgba(239, 68, 68, 0.1)', color: '#ef4444', fontWeight: 700, px: 0.5, whiteSpace: 'nowrap' }}
            />
          </Stack>
        </Box>

        {/* Full Width Area Chart Container */}
        <Box sx={{ width: '100%', height: 380 }}>
          <ResponsiveContainer width="100%" height="100%">
            <AreaChart data={chartData} margin={{ top: 15, right: 20, left: 15, bottom: 5 }}>
              <defs>
                <linearGradient id="fullGmvGradient" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#0095ff" stopOpacity={0.35} />
                  <stop offset="95%" stopColor="#0095ff" stopOpacity={0.01} />
                </linearGradient>
                <linearGradient id="fullLiveGradient" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#ef4444" stopOpacity={0.3} />
                  <stop offset="95%" stopColor="#ef4444" stopOpacity={0.01} />
                </linearGradient>
                <linearGradient id="fullFeeGradient" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#10b981" stopOpacity={0.35} />
                  <stop offset="95%" stopColor="#10b981" stopOpacity={0.01} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
              <XAxis dataKey="date" stroke="#64748b" fontSize={13} fontWeight={600} tickLine={false} dy={8} />
              <YAxis
                stroke="#64748b"
                fontSize={12}
                tickLine={false}
                tickFormatter={(v) => `${(v / 1000000).toFixed(0)} Triệu`}
              />
              <RechartsTooltip
                formatter={(value, name) => [
                  new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value),
                  name
                ]}
                contentStyle={{
                  backgroundColor: '#ffffff',
                  borderRadius: '12px',
                  boxShadow: '0 10px 25px rgba(0,0,0,0.1)',
                  border: '1px solid #e2e8f0',
                  padding: '10px 14px'
                }}
              />
              <Legend
                verticalAlign="top"
                align="right"
                height={36}
                iconType="circle"
                wrapperStyle={{ paddingBottom: '12px', fontSize: '0.875rem', fontWeight: 600 }}
              />
              <Area
                type="monotone"
                dataKey="gmv"
                name="Tổng GMV Toàn Sàn"
                stroke="#0095ff"
                strokeWidth={3}
                fillOpacity={1}
                fill="url(#fullGmvGradient)"
              />
              <Area
                type="monotone"
                dataKey="liveGmv"
                name="GMV Chốt Qua Livestream"
                stroke="#ef4444"
                strokeWidth={2.5}
                strokeDasharray="4 4"
                fillOpacity={1}
                fill="url(#fullLiveGradient)"
              />
              <Area
                type="monotone"
                dataKey="fee"
                name="Doanh Thu Hoa Hồng (5%)"
                stroke="#10b981"
                strokeWidth={2.5}
                fillOpacity={1}
                fill="url(#fullFeeGradient)"
              />
            </AreaChart>
          </ResponsiveContainer>
        </Box>
      </Card>

      {/* 4. BIỂU ĐỒ FULL-WIDTH 2: CƠ CẤU NGÀNH HÀNG & MA TRẬN ĐÓNG GÓP DOANH SỐ */}
      <Card sx={{ p: { xs: 2, sm: 3 }, borderRadius: '18px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 4px 20px rgba(0,0,0,0.03)', width: '100%' }}>
        {/* Header */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 2, mb: 3 }}>
          <Box>
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', fontSize: '1.2rem', whiteSpace: 'nowrap' }}>
              Phân Tích Cơ Cấu Ngành Hàng & Ma Trận Đóng Góp Doanh Số
            </Typography>
            <Typography variant="body2" sx={{ color: '#64748b', mt: 0.3 }}>
              Tỷ trọng doanh thu, đơn hàng thành công, giá trị đơn trung bình và tốc độ tăng trưởng của từng ngành hàng chính
            </Typography>
          </Box>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            <Chip
              label="Tổng GMV: 1,420,000,000 đ"
              size="small"
              sx={{ bgcolor: 'rgba(0, 149, 255, 0.08)', color: '#0095ff', fontWeight: 700, px: 0.5, whiteSpace: 'nowrap' }}
            />
            <Chip
              label="4 Ngành Hàng Trọng Điểm"
              size="small"
              sx={{ bgcolor: 'rgba(139, 92, 246, 0.1)', color: '#8b5cf6', fontWeight: 700, px: 0.5, whiteSpace: 'nowrap' }}
            />
            <Chip
              label="Tăng trưởng TB: +14.0% MoM"
              size="small"
              sx={{ bgcolor: 'rgba(16, 185, 129, 0.1)', color: '#10b981', fontWeight: 700, px: 0.5, whiteSpace: 'nowrap' }}
            />
          </Stack>
        </Box>

        {/* 1. Full-Width 4 Metric Cards for the 4 Categories */}
        <Grid container spacing={2.5} sx={{ mb: 3 }}>
          {CATEGORY_DETAILS.map((cat) => (
            <Grid item xs={12} sm={6} lg={3} key={cat.name}>
              <Box
                sx={{
                  p: 2.2,
                  borderRadius: '14px',
                  bgcolor: '#ffffff',
                  border: '1px solid #e2e8f0',
                  boxShadow: '0 2px 8px rgba(0,0,0,0.02)',
                  height: '100%',
                  display: 'flex',
                  flexDirection: 'column',
                  justifyContent: 'space-between',
                  position: 'relative',
                  overflow: 'hidden',
                  '&::before': {
                    content: '""',
                    position: 'absolute',
                    top: 0,
                    left: 0,
                    right: 0,
                    height: '4px',
                    bgcolor: cat.color
                  }
                }}
              >
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1.5 }}>
                  <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }} title={cat.name}>
                    {cat.name}
                  </Typography>
                  <Chip
                    label={`${cat.value}%`}
                    size="small"
                    sx={{
                      fontWeight: 800,
                      height: 22,
                      fontSize: '0.72rem',
                      whiteSpace: 'nowrap',
                      bgcolor: `${cat.color}15`,
                      color: cat.color,
                      border: `1px solid ${cat.color}35`
                    }}
                  />
                </Box>

                <Typography variant="h5" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap', mb: 0.5 }}>
                  {cat.revenue}
                </Typography>

                <LinearProgress
                  variant="determinate"
                  value={cat.value}
                  sx={{
                    height: 6,
                    borderRadius: 3,
                    bgcolor: '#f1f5f9',
                    my: 1.2,
                    '& .MuiLinearProgress-bar': {
                      bgcolor: cat.color,
                      borderRadius: 3
                    }
                  }}
                />

                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 0.5 }}>
                  <Typography variant="caption" sx={{ color: '#64748b', fontWeight: 600, whiteSpace: 'nowrap' }}>
                    {cat.orders}
                  </Typography>
                  <Chip
                    label={cat.growth}
                    size="small"
                    sx={{
                      height: 20,
                      fontSize: '0.7rem',
                      fontWeight: 700,
                      whiteSpace: 'nowrap',
                      bgcolor: 'rgba(22, 163, 74, 0.1)',
                      color: '#16a34a'
                    }}
                  />
                </Box>
              </Box>
            </Grid>
          ))}
        </Grid>

        {/* 2. Full-Width Market Share Visual Strip */}
        <Box sx={{ p: 2, borderRadius: '12px', bgcolor: '#f8fafc', border: '1px solid #e2e8f0', mb: 3 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1.2, flexWrap: 'wrap', gap: 1 }}>
            <Typography variant="caption" sx={{ fontWeight: 700, color: '#475569', textTransform: 'uppercase', whiteSpace: 'nowrap' }}>
              Tỷ Trọng Đóng Góp GMV Toàn Sàn (100% Thị Phần)
            </Typography>
            <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
              {CATEGORY_DETAILS.map(c => (
                <Box key={c.name} sx={{ display: 'flex', alignItems: 'center', gap: 0.8, whiteSpace: 'nowrap' }}>
                  <Box sx={{ width: 10, height: 10, borderRadius: '50%', bgcolor: c.color }} />
                  <Typography variant="caption" sx={{ fontWeight: 600, color: '#334155', whiteSpace: 'nowrap' }}>
                    {c.name}: <strong>{c.value}%</strong>
                  </Typography>
                </Box>
              ))}
            </Stack>
          </Box>
          <Box sx={{ display: 'flex', width: '100%', height: 12, borderRadius: 6, overflow: 'hidden', bgcolor: '#e2e8f0' }}>
            {CATEGORY_DETAILS.map(c => (
              <Box
                key={c.name}
                sx={{
                  width: `${c.value}%`,
                  bgcolor: c.color,
                  transition: 'width 0.6s ease'
                }}
                title={`${c.name}: ${c.value}% (${c.revenue})`}
              />
            ))}
          </Box>
        </Box>

        {/* 3. Full-Width Comprehensive Data Matrix Table */}
        <TableContainer sx={{ overflowX: 'auto', borderRadius: '12px', border: '1px solid #e2e8f0' }}>
          <Table size="small">
            <TableHead sx={{ bgcolor: '#f8fafc' }}>
              <TableRow>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Ngành Hàng Chính</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Doanh Thu GMV (đ)</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Tỷ Trọng (%)</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Số Đơn Thành Công</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Giá Trị TB Đơn (AOV)</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Tỷ Lệ Qua Live</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Tăng Trưởng (MoM)</TableCell>
                <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Hiệu Suất</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {CATEGORY_DETAILS.map((cat) => (
                <TableRow key={cat.name} hover>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                      <Box sx={{ width: 14, height: 14, borderRadius: '4px', bgcolor: cat.color, flexShrink: 0 }} />
                      <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>
                        {cat.name}
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
                      {cat.revenue}
                    </Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap', minWidth: 160 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                      <LinearProgress
                        variant="determinate"
                        value={cat.value}
                        sx={{
                          flex: 1,
                          height: 7,
                          borderRadius: 4,
                          bgcolor: '#f1f5f9',
                          '& .MuiLinearProgress-bar': { bgcolor: cat.color, borderRadius: 4 }
                        }}
                      />
                      <Typography variant="body2" sx={{ fontWeight: 800, color: cat.color, whiteSpace: 'nowrap', minWidth: 32 }}>
                        {cat.value}%
                      </Typography>
                    </Box>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ color: '#334155', fontWeight: 600, whiteSpace: 'nowrap' }}>{cat.orders}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>{cat.aov}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Chip
                      label={`${cat.liveShare} qua Live`}
                      size="small"
                      sx={{
                        fontWeight: 700,
                        height: 22,
                        fontSize: '0.72rem',
                        whiteSpace: 'nowrap',
                        bgcolor: 'rgba(239, 68, 68, 0.08)',
                        color: '#ef4444'
                      }}
                    />
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Chip
                      label={cat.growth}
                      size="small"
                      sx={{
                        fontWeight: 700,
                        height: 22,
                        fontSize: '0.72rem',
                        whiteSpace: 'nowrap',
                        bgcolor: 'rgba(22, 163, 74, 0.1)',
                        color: '#16a34a'
                      }}
                    />
                  </TableCell>
                  <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                    <Chip
                      label={cat.status}
                      size="small"
                      sx={{
                        fontWeight: 700,
                        height: 24,
                        fontSize: '0.72rem',
                        whiteSpace: 'nowrap',
                        bgcolor: `${cat.statusColor}15`,
                        color: cat.statusColor,
                        border: `1px solid ${cat.statusColor}35`
                      }}
                    />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>

      {/* 5. BIỂU ĐỒ FULL-WIDTH 3: LƯU LƯỢNG KHÁCH HÀNG & ĐƠN HÀNG LIVE THEO KHUNG GIỜ */}
      <Card sx={{ p: { xs: 2, sm: 3 }, borderRadius: '18px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 4px 20px rgba(0,0,0,0.03)', width: '100%' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 2, mb: 3 }}>
          <Box>
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', fontSize: '1.2rem', whiteSpace: 'nowrap' }}>
              Lưu Lượng Khách Hàng Đồng Thời (CCU) & Đơn Hàng Theo Khung Giờ Trong Ngày
            </Typography>
            <Typography variant="body2" sx={{ color: '#64748b', mt: 0.3 }}>
              Phân tích hành vi mua sắm trực tiếp qua livestream để tối ưu điều phối máy chủ và lịch Flash Sale
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, whiteSpace: 'nowrap' }}>
            <WhatshotRoundedIcon sx={{ color: '#ef4444' }} />
            <Typography variant="body2" sx={{ fontWeight: 700, color: '#ef4444', whiteSpace: 'nowrap' }}>
              Giờ Vàng (19h - 24h) chiếm 62% tổng doanh số sàn
            </Typography>
          </Box>
        </Box>

        <Box sx={{ width: '100%', height: 320 }}>
          <ResponsiveContainer width="100%" height="100%">
            <BarChart data={HOURLY_TRAFFIC_DATA} margin={{ top: 10, right: 20, left: 15, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
              <XAxis dataKey="hour" stroke="#64748b" fontSize={12} tickLine={false} dy={8} />
              <YAxis yAxisId="left" stroke="#64748b" fontSize={12} tickLine={false} tickFormatter={(v) => `${v.toLocaleString()} CCU`} />
              <YAxis yAxisId="right" orientation="right" stroke="#16a34a" fontSize={12} tickLine={false} tickFormatter={(v) => `${v} đơn`} />
              <RechartsTooltip
                formatter={(value, name) => [
                  name === 'Khán Giả Đồng Thời (CCU)' ? `${value.toLocaleString()} người xem` : `${value} đơn chốt`,
                  name
                ]}
                contentStyle={{
                  backgroundColor: '#ffffff',
                  borderRadius: '12px',
                  boxShadow: '0 10px 25px rgba(0,0,0,0.1)',
                  border: '1px solid #e2e8f0'
                }}
              />
              <Legend verticalAlign="top" align="right" height={36} wrapperStyle={{ paddingBottom: '10px', fontWeight: 600 }} />
              <Bar yAxisId="left" dataKey="ccu" name="Khán Giả Đồng Thời (CCU)" fill="#0095ff" radius={[6, 6, 0, 0]} maxBarSize={48} />
              <Bar yAxisId="right" dataKey="orders" name="Số Đơn Hàng Chốt Trực Tiếp" fill="#10b981" radius={[6, 6, 0, 0]} maxBarSize={48} />
            </BarChart>
          </ResponsiveContainer>
        </Box>
      </Card>

      {/* 6. Quick Payout Approval on Dashboard */}
      <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)', width: '100%' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2, flexWrap: 'wrap', gap: 1 }}>
          <Box>
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
              Duyệt Lệnh Rút Tiền Seller Payouts Chờ Xử Lý
            </Typography>
            <Typography variant="caption" sx={{ color: '#64748b' }}>
              Giải ngân trực tiếp vào tài khoản ngân hàng của đối tác gian hàng
            </Typography>
          </Box>
          <Button
            size="small"
            onClick={() => navigate('/admin/payouts')}
            sx={adminBtnStyles.transSecondary}
          >
            Xem tất cả lệnh rút tiền
          </Button>
        </Box>
        <TableContainer sx={{ overflowX: 'auto' }}>
          <Table size="small">
            <TableHead sx={{ bgcolor: '#f8fafc' }}>
              <TableRow>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Gian Hàng</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Chủ Tài Khoản</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Số Tiền Rút</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Ngân Hàng Thụ Hưởng</TableCell>
                <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Thời Gian</TableCell>
                <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Hành Động</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {payouts.slice(0, 3).map((pay) => (
                <TableRow key={pay.id} hover>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{pay.shopName}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ color: '#334155', fontWeight: 600, whiteSpace: 'nowrap' }}>{pay.holder}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ fontWeight: 800, color: '#16a34a', whiteSpace: 'nowrap' }}>{pay.amount}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ color: '#334155', whiteSpace: 'nowrap' }}>{pay.bank}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>{pay.requestedAt}</Typography>
                  </TableCell>
                  <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                    {pay.status === 'Pending' ? (
                      <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                        <Button
                          size="small"
                          onClick={() => onApprovePayout(pay.id, pay.amount, pay.shopName)}
                          sx={adminBtnStyles.success}
                        >
                          Duyệt Giải Ngân
                        </Button>
                        <Button
                          size="small"
                          onClick={() => onRejectPayout(pay.id, pay.shopName)}
                          sx={adminBtnStyles.transDanger}
                        >
                          Từ chối
                        </Button>
                      </Box>
                    ) : (
                      <Chip
                        label={pay.status === 'Completed' ? 'Đã chi trả' : 'Đã từ chối'}
                        color={pay.status === 'Completed' ? 'success' : 'error'}
                        size="small"
                        sx={{ fontWeight: 700, whiteSpace: 'nowrap' }}
                      />
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>
    </Box>
  )
}

export default DashboardOverviewTab
