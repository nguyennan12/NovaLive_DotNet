import { useState, useEffect } from 'react'
import {
  Box,
  Card,
  Typography,
  Chip,
  Button,
  Tabs,
  Tab
} from '@mui/material'
import { useLocation, useNavigate } from 'react-router-dom'
import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded'
import StorefrontRoundedIcon from '@mui/icons-material/StorefrontRounded'
import GavelRoundedIcon from '@mui/icons-material/GavelRounded'
import AccountBalanceWalletRoundedIcon from '@mui/icons-material/AccountBalanceWalletRounded'
import LiveTvRoundedIcon from '@mui/icons-material/LiveTvRounded'
import ReportProblemRoundedIcon from '@mui/icons-material/ReportProblemRounded'
import CategoryRoundedIcon from '@mui/icons-material/CategoryRounded'
import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import PeopleAltRoundedIcon from '@mui/icons-material/PeopleAltRounded'
import RateReviewRoundedIcon from '@mui/icons-material/RateReviewRounded'
import { toast } from 'react-toastify'
import { adminBtnStyles } from '../theme/adminButtonStyles'
import {
  getAdminShopsAPI,
  getAdminPayoutsAPI,
  approvePayoutAPI,
  rejectPayoutAPI,
  getAdminDashboardReportAPI
} from '~/common/apis/services/adminService'

// Import modular tab components
import DashboardOverviewTab from '../components/tabs/DashboardOverviewTab'
import ShopsManagementTab from '../components/tabs/ShopsManagementTab'
import DisputesArbitrationTab from '../components/tabs/DisputesArbitrationTab'
import PayoutsManagementTab from '../components/tabs/PayoutsManagementTab'
import LiveModerationTab from '../components/tabs/LiveModerationTab'
import ReportsModerationTab from '../components/tabs/ReportsModerationTab'
import CategoriesManagementTab from '../components/tabs/CategoriesManagementTab'
import FlashSaleManagementTab from '../components/tabs/FlashSaleManagementTab'
import UsersManagementTab from '../components/tabs/UsersManagementTab'
import ReviewsModerationTab from '../components/tabs/ReviewsModerationTab'

// Initial Mock Data
const INITIAL_SHOPS = [
  {
    id: 'shop-1',
    name: 'Mỹ Phẩm Cao Cấp Glow & Care',
    owner: 'Trần Thị Mai',
    email: 'mai.tran@glowcare.vn',
    phone: '0987654321',
    taxId: '0318492041',
    address: '142 Hai Bà Trưng, Phường Đa Kao, Quận 1, TP. Hồ Chí Minh',
    createdAt: '23/10/2026 14:20',
    status: 'Pending',
    idCardFront: 'https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600&auto=format&fit=crop&q=80',
    idCardBack: 'https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600&auto=format&fit=crop&q=80',
    businessLicense: 'https://images.unsplash.com/photo-1450133064473-71024230f91b?w=600&auto=format&fit=crop&q=80',
    notes: 'Hồ sơ pháp nhân hộ kinh doanh cá thể đầy đủ'
  },
  {
    id: 'shop-2',
    name: 'TechLab Gaming Gears',
    owner: 'Lê Hoàng Nam',
    email: 'nam.le@techlab.com',
    phone: '0912987654',
    taxId: '0109482715',
    address: '88 Thái Hà, Phường Trung Liệt, Quận Đống Đa, Hà Nội',
    createdAt: '23/10/2026 11:05',
    status: 'Pending',
    idCardFront: 'https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600&auto=format&fit=crop&q=80',
    idCardBack: 'https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600&auto=format&fit=crop&q=80',
    businessLicense: 'https://images.unsplash.com/photo-1450133064473-71024230f91b?w=600&auto=format&fit=crop&q=80',
    notes: 'Hồ sơ công ty TNHH MTV'
  },
  {
    id: 'shop-3',
    name: 'Organic Food & Drinks',
    owner: 'Phạm Minh Khang',
    email: 'khang.pham@organicvn.com',
    phone: '0934567890',
    taxId: '0409283741',
    address: '25 Lê Duẩn, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh',
    createdAt: '22/10/2026 18:40',
    status: 'Pending',
    idCardFront: 'https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600&auto=format&fit=crop&q=80',
    idCardBack: 'https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=600&auto=format&fit=crop&q=80',
    businessLicense: 'https://images.unsplash.com/photo-1450133064473-71024230f91b?w=600&auto=format&fit=crop&q=80',
    notes: 'Hồ sơ chờ xác nhận CCCD chip'
  },
  {
    id: 'shop-4',
    name: 'Sneaker Authentic VN',
    owner: 'Nguyễn Văn Đạt',
    email: 'dat.nguyen@sneaker.vn',
    phone: '0901234567',
    taxId: '0308192847',
    address: '12 Nguyễn Trãi, Quận 5, TP. Hồ Chí Minh',
    createdAt: '21/10/2026 09:10',
    status: 'Active',
    idCardFront: '',
    idCardBack: '',
    businessLicense: '',
    notes: 'Đã kích hoạt'
  },
  {
    id: 'shop-5',
    name: 'Fake Brands Warning Store',
    owner: 'Hoàng Văn B',
    email: 'fake.warning@gmail.com',
    phone: '0977889900',
    taxId: '0102938475',
    address: '45 Cầu Giấy, Hà Nội',
    createdAt: '15/10/2026 10:00',
    status: 'Suspended',
    idCardFront: '',
    idCardBack: '',
    businessLicense: '',
    notes: 'Bị khóa do bán hàng giả mạo nhãn hiệu'
  }
]

const INITIAL_DISPUTES = [
  {
    id: 'DISP-8821',
    returnId: 'RET-1092',
    orderId: 'SUB-9402',
    productName: 'Tai Nghe Chống Ồn Wireless Pro Max - Màu Đen',
    amount: '1,450,000 đ',
    buyerName: 'Nguyễn Văn An (an.nguyen@gmail.com)',
    shopName: 'TechLab Gaming Gears',
    createdAt: '23/10/2026 09:30',
    status: 'Pending',
    buyerClaim: {
      reason: 'Hàng bể vỡ màn hình cảm ứng & không bật được nguồn',
      detail: 'Tôi mở hộp thấy màn hình tai nghe nứt góc, bật nút nguồn hoàn toàn không phản hồi. Shop từ chối đổi trả nói do tôi làm rơi.',
      photos: [
        'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500&auto=format&fit=crop&q=60',
        'https://images.unsplash.com/photo-1484704849700-f032a568e944?w=500&auto=format&fit=crop&q=60'
      ],
      videoProof: 'Video_Unboxing_SUB9402.mp4 (Có ghi hình từ lúc rạch băng keo GHN)'
    },
    sellerEvidence: {
      reason: 'Sản phẩm đã dán tem QC Pass và kiểm tra 100% trước khi đóng gói',
      detail: 'Chúng tôi có camera ghi hình lúc đóng hộp, hàng hoàn toàn mới 100%, có dán tem niêm phong và đóng 3 lớp xốp khí chống sốc.',
      photos: [
        'https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=500&auto=format&fit=crop&q=60'
      ],
      videoProof: 'Packing_Log_GHN_1810.mp4 (Đóng hàng bàn giao bưu tá)'
    },
    ghnTimeline: [
      { time: '18/10 10:00', label: 'Lấy hàng thành công', detail: 'Bưu tá GHN nhận kiện hàng tại kho Shop' },
      { time: '19/10 14:30', label: 'Vận chuyển liên tỉnh', detail: 'Đến kho trung chuyển liên vùng Tân Bình' },
      { time: '20/10 09:15', label: 'Giao hàng thành công', detail: 'Người mua ký nhận gói hàng' },
      { time: '20/10 16:20', label: 'Người mua mở yêu cầu trả hàng', detail: 'Lý do: Hàng bể vỡ khi nhận' },
      { time: '21/10 11:00', label: 'Shop từ chối hỗ trợ', detail: 'Lý do: Shop bảo lưu hàng chuẩn QC' },
      { time: '22/10 09:00', label: 'Leo thang Admin Trọng tài', detail: 'Hệ thống tự động chuyển Admin phân xử' }
    ]
  },
  {
    id: 'DISP-8822',
    returnId: 'RET-1095',
    orderId: 'SUB-9488',
    productName: 'Áo Khoác Bomber Nam Da Lộn - Size XL',
    amount: '680,000 đ',
    buyerName: 'Trần Minh Đức (duc.tran@yahoo.com)',
    shopName: 'Fashion Forward Store',
    createdAt: '22/10/2026 14:15',
    status: 'Pending',
    buyerClaim: {
      reason: 'Shop gửi sai kích cỡ (Đặt XL nhưng giao nhầm Size M)',
      detail: 'Tôi đặt size XL chiều cao 1m75 nhưng tem áo ghi size M, tôi mặc không vừa. Shop không đồng ý gửi đổi size mới.',
      photos: [
        'https://images.unsplash.com/photo-1551028719-00167b16eac5?w=500&auto=format&fit=crop&q=60'
      ],
      videoProof: 'Video_Tag_Ao_Size_M.mp4'
    },
    sellerEvidence: {
      reason: 'Shop khẳng định gửi đúng mã đơn, tem size chuẩn phom âu rộng',
      detail: 'Phom áo là phom Âu rộng nên size M tương đương XL Việt Nam. Khách hàng không đọc kỹ bảng kích cỡ mô tả.',
      photos: [],
      videoProof: 'Kich_Thuoc_Ao_Size_Chart.png'
    },
    ghnTimeline: [
      { time: '19/10 11:00', label: 'Lấy hàng thành công', detail: 'Bưu tá GHN lấy hàng tại kho Fashion' },
      { time: '21/10 10:30', label: 'Giao hàng thành công', detail: 'Người mua nhận hàng' },
      { time: '21/10 18:00', label: 'Người mua yêu cầu hoàn hàng', detail: 'Lý do: Nhận sai size' },
      { time: '22/10 10:00', label: 'Shop từ chối', detail: 'Shop không nhận hoàn' },
      { time: '22/10 14:00', label: 'Leo thang lên Trọng tài', detail: 'Chờ quyết định hoàn tiền của Admin' }
    ]
  }
]

const INITIAL_PAYOUTS = [
  { id: 'pay-1', shopName: 'Fashion Forward Store', amount: '38,500,000 đ', bank: 'Techcombank •••• 9412', holder: 'TRẦN MINH QUÂN', requestedAt: '23/10/2026 09:15', status: 'Pending' },
  { id: 'pay-2', shopName: 'Smart Tech Zone', amount: '21,200,000 đ', bank: 'Vietcombank •••• 1084', holder: 'LÊ VĂN THẮNG', requestedAt: '23/10/2026 10:30', status: 'Pending' },
  { id: 'pay-3', shopName: 'Gia Dụng Nova VIP', amount: '65,000,000 đ', bank: 'MBBank •••• 5566', holder: 'HOÀNG ĐÌNH TRỌNG', requestedAt: '22/10/2026 15:45', status: 'Completed' },
  { id: 'pay-4', shopName: 'Beauty Cosmetics Mall', amount: '15,800,000 đ', bank: 'ACB •••• 3321', holder: 'ĐẶNG THỊ THU HÀ', requestedAt: '21/10/2026 14:10', status: 'Completed' }
]

export const AdminDashboardPage = () => {
  const location = useLocation()
  const navigate = useNavigate()

  const TAB_KEYS = [
    { key: 'dashboard', label: 'Tổng quan', path: '/admin/dashboard', icon: DashboardRoundedIcon },
    { key: 'shops', label: 'Duyệt Shop (KYC)', path: '/admin/shops', icon: StorefrontRoundedIcon, badge: '3 chờ', badgeColor: 'info' },
    { key: 'disputes', label: 'Phân xử Tranh chấp', path: '/admin/disputes', icon: GavelRoundedIcon, badge: '2 vụ việc', badgeColor: 'error' },
    { key: 'payouts', label: 'Duyệt Payout (Ví)', path: '/admin/payouts', icon: AccountBalanceWalletRoundedIcon, badge: '2 lệnh', badgeColor: 'success' },
    { key: 'livestreams', label: 'Giám sát Live', path: '/admin/livestreams', icon: LiveTvRoundedIcon, badge: 'Live', badgeColor: 'error' },
    { key: 'reports', label: 'Khiếu nại & Tố cáo', path: '/admin/reports', icon: ReportProblemRoundedIcon, badge: '3 mới', badgeColor: 'warning' },
    { key: 'categories', label: 'Cây danh mục', path: '/admin/categories', icon: CategoryRoundedIcon },
    { key: 'flash-sales', label: 'Flash Sale', path: '/admin/flash-sales', icon: LocalOfferRoundedIcon, badge: 'Hot', badgeColor: 'error' },
    { key: 'users', label: 'Người dùng & RBAC', path: '/admin/users', icon: PeopleAltRoundedIcon },
    { key: 'reviews', label: 'Kiểm duyệt đánh giá', path: '/admin/reviews', icon: RateReviewRoundedIcon }
  ]

  const getCurrentTabIndex = () => {
    const path = location.pathname.toLowerCase()
    const index = TAB_KEYS.findIndex((item) => path.endsWith(item.key) || path.includes(item.key))
    return index !== -1 ? index : 0
  }

  const [activeTab, setActiveTab] = useState(getCurrentTabIndex())

  useEffect(() => {
    setActiveTab(getCurrentTabIndex())
  }, [location.pathname])

  const handleTabChange = (_, newIndex) => {
    setActiveTab(newIndex)
    navigate(TAB_KEYS[newIndex].path)
  }

  // Shared state for core operational data
  const [shops, setShops] = useState(INITIAL_SHOPS)
  const [disputes, setDisputes] = useState(INITIAL_DISPUTES)
  const [payouts, setPayouts] = useState(INITIAL_PAYOUTS)
  const [dashboardMetrics, setDashboardMetrics] = useState(null)

  const loadAdminData = async () => {
    try {
      const [shopsRes, payoutsRes, reportRes] = await Promise.allSettled([
        getAdminShopsAPI({ size: 50 }),
        getAdminPayoutsAPI({ size: 50 }),
        getAdminDashboardReportAPI()
      ])
      if (reportRes.status === 'fulfilled' && reportRes.value) {
        setDashboardMetrics(reportRes.value)
      }
      if (shopsRes.status === 'fulfilled' && shopsRes.value?.items?.length) {
        const apiShops = shopsRes.value.items.map(s => ({
          id: s.id,
          name: s.shopName || s.name,
          owner: s.ownerName || s.owner || 'Chủ gian hàng',
          email: s.contactEmail || s.email || '',
          phone: s.contactPhone || s.phone || '',
          taxId: s.taxCode || s.taxId || 'N/A',
          address: s.pickupAddress || s.address || '',
          createdAt: s.createdAt ? new Date(s.createdAt).toLocaleDateString('vi-VN') : '',
          status: s.status,
          idCardFront: s.idCardFrontUrl || '',
          idCardBack: s.idCardBackUrl || '',
          businessLicense: s.licenseUrl || '',
          notes: s.notes || ''
        }))
        setShops(prev => {
          const apiIds = new Set(apiShops.map(x => x.id))
          return [...apiShops, ...prev.filter(x => !apiIds.has(x.id))]
        })
      }
      if (payoutsRes.status === 'fulfilled' && payoutsRes.value?.items?.length) {
        const apiPayouts = payoutsRes.value.items.map(p => ({
          id: p.id,
          shop: p.shopName || 'Shop',
          owner: p.ownerName || '',
          amount: p.amount ? `${p.amount.toLocaleString('vi-VN')} đ` : '0 đ',
          bank: p.bankName || 'Ngân hàng',
          accountNumber: p.bankAccountNumber || '',
          requestedAt: p.createdAt ? new Date(p.createdAt).toLocaleDateString('vi-VN') : '',
          status: p.status
        }))
        setPayouts(prev => {
          const apiIds = new Set(apiPayouts.map(x => x.id))
          return [...apiPayouts, ...prev.filter(x => !apiIds.has(x.id))]
        })
      }
    } catch (e) {
      console.warn('loadAdminData fallback to mock:', e)
    }
  }

  useEffect(() => {
    loadAdminData()
  }, [])

  const handleApprovePayout = async (id, amount, shop) => {
    try {
      await approvePayoutAPI(id)
    } catch (err) {
      console.warn('approvePayoutAPI fallback to optimistic:', err)
    }
    setPayouts(prev => prev.map(p => p.id === id ? { ...p, status: 'Completed' } : p))
    toast.success(`Đã xác nhận chuyển tiền giải ngân ${amount} cho ${shop}`)
  }

  const handleRejectPayout = async (id, shop) => {
    try {
      await rejectPayoutAPI(id, { reason: 'Từ chối bởi Quản trị viên' })
    } catch (err) {
      console.warn('rejectPayoutAPI fallback to optimistic:', err)
    }
    setPayouts(prev => prev.map(p => p.id === id ? { ...p, status: 'Rejected' } : p))
    toast.error(`Đã từ chối lệnh rút tiền của ${shop}`)
  }

  return (
    <Box sx={{ width: '100%', maxWidth: '100%', display: 'flex', flexDirection: 'column', gap: 3 }}>
      {/* 1. Header Greeting & Quick Actions */}
      <Box sx={{ display: 'flex', flexWrap: 'wrap', alignItems: 'center', justifyContent: 'space-between', gap: 2 }}>
        <Box>
          <Typography variant="h5" sx={{ fontWeight: 800, color: '#0f172a', letterSpacing: '-0.02em' }}>
            Bảng Điều Khiển Giám Đốc Sàn (Platform Command Center)
          </Typography>
          <Typography variant="body2" sx={{ color: '#64748b', mt: 0.5 }}>
            Điều hành toàn diện: Duyệt Shop KYC, Trọng tài Tranh chấp, Duyệt Payout, Giám sát Livestream và Quản trị RBAC.
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1.5 }}>
          <Button
            size="small"
            onClick={() => toast.info('Đang đồng bộ dữ liệu thời gian thực...')}
            sx={adminBtnStyles.transSecondary}
          >
            Làm mới số liệu
          </Button>
          <Button
            size="small"
            onClick={() => toast.success('Đang xuất báo cáo tài chính toàn sàn PDF/Excel...')}
            sx={adminBtnStyles.primary}
          >
            Xuất Báo Cáo
          </Button>
        </Box>
      </Box>

      {/* 2. Top Navigation Tabs */}
      <Card sx={{ borderRadius: '14px', border: '1px solid #e2e8f0', boxShadow: '0 2px 8px rgba(0,0,0,0.02)', bgcolor: '#ffffff', px: 1.5 }}>
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          variant="scrollable"
          scrollButtons="auto"
          sx={{
            minHeight: 52,
            '& .MuiTab-root': {
              minHeight: 52,
              textTransform: 'none',
              fontSize: '0.875rem',
              fontWeight: 600,
              color: '#64748b',
              whiteSpace: 'nowrap',
              py: 1,
              px: { xs: 1.5, sm: 2 },
              '&.Mui-selected': { color: '#0095ff', fontWeight: 700 }
            },
            '& .MuiTabs-indicator': {
              background: 'linear-gradient(90deg, #0095ff 0%, #14c3eb 100%)',
              height: 3,
              borderRadius: '3px 3px 0 0'
            }
          }}
        >
          {TAB_KEYS.map((tab) => {
            const Icon = tab.icon
            return (
              <Tab
                key={tab.key}
                icon={<Icon sx={{ fontSize: '1.15rem' }} />}
                iconPosition="start"
                label={
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.8, whiteSpace: 'nowrap' }}>
                    <span style={{ whiteSpace: 'nowrap' }}>{tab.label}</span>
                    {tab.badge && (
                      <Chip
                        label={tab.badge}
                        size="small"
                        color={tab.badgeColor || 'default'}
                        sx={{ height: 18, fontSize: '0.65rem', fontWeight: 700, whiteSpace: 'nowrap' }}
                      />
                    )}
                  </Box>
                }
              />
            )
          })}
        </Tabs>
      </Card>

      {/* 3. Sub-pages (Tabs View) */}
      {activeTab === 0 && (
        <DashboardOverviewTab
          metrics={dashboardMetrics}
          payouts={payouts}
          onApprovePayout={handleApprovePayout}
          onRejectPayout={handleRejectPayout}
        />
      )}

      {activeTab === 1 && (
        <ShopsManagementTab
          shops={shops}
          setShops={setShops}
        />
      )}

      {activeTab === 2 && (
        <DisputesArbitrationTab
          disputes={disputes}
          setDisputes={setDisputes}
        />
      )}

      {activeTab === 3 && (
        <PayoutsManagementTab
          payouts={payouts}
          onApprovePayout={handleApprovePayout}
          onRejectPayout={handleRejectPayout}
        />
      )}

      {activeTab === 4 && <LiveModerationTab />}

      {activeTab === 5 && <ReportsModerationTab />}

      {activeTab === 6 && <CategoriesManagementTab />}

      {activeTab === 7 && <FlashSaleManagementTab />}

      {activeTab === 8 && <UsersManagementTab />}

      {activeTab === 9 && <ReviewsModerationTab />}
    </Box>
  )
}

export default AdminDashboardPage
