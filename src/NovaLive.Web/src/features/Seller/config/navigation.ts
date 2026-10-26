import DashboardRounded from '@mui/icons-material/DashboardRounded'
import StorefrontOutlined from '@mui/icons-material/StorefrontOutlined'
import Inventory2Outlined from '@mui/icons-material/Inventory2Outlined'
import WarehouseOutlined from '@mui/icons-material/WarehouseOutlined'
import ShoppingBagOutlined from '@mui/icons-material/ShoppingBagOutlined'
import AssignmentReturnOutlined from '@mui/icons-material/AssignmentReturnOutlined'
import LocalOfferOutlined from '@mui/icons-material/LocalOfferOutlined'
import FlashOnOutlined from '@mui/icons-material/FlashOnOutlined'
import StarOutlineRounded from '@mui/icons-material/StarOutlineRounded'
import AccountBalanceWalletOutlined from '@mui/icons-material/AccountBalanceWalletOutlined'
import NotificationsNoneRounded from '@mui/icons-material/NotificationsNoneRounded'
import LiveTvOutlined from '@mui/icons-material/LiveTvOutlined'
import PeopleAltOutlined from '@mui/icons-material/PeopleAltOutlined'
import BarChartOutlined from '@mui/icons-material/BarChartOutlined'
import LocalShippingOutlined from '@mui/icons-material/LocalShippingOutlined'
import CampaignOutlined from '@mui/icons-material/CampaignOutlined'

export const sellerNavigation = [
  { label: 'Cửa hàng', items: [
    { path: '', label: 'Tổng quan', icon: DashboardRounded, permission: 'Seller' },
    { path: 'orders', label: 'Đơn hàng', icon: ShoppingBagOutlined, permission: 'Seller' },
    { path: 'products', label: 'Sản phẩm', icon: Inventory2Outlined, permission: 'Seller' },
    { path: 'inventory', label: 'Kho hàng', icon: WarehouseOutlined, permission: 'Seller' },
    { path: 'returns', label: 'Trả hàng / Hoàn tiền', icon: AssignmentReturnOutlined, permission: 'Seller' },
    { path: 'shipping', label: 'Vận chuyển', icon: LocalShippingOutlined, permission: 'Seller' },
    { path: 'customers', label: 'Khách hàng', icon: PeopleAltOutlined, permission: 'Seller' }
  ] },
  { label: 'Tăng trưởng', items: [
    { path: 'marketing', label: 'Tiếp thị', icon: CampaignOutlined, permission: 'Seller' },
    { path: 'discounts', label: 'Voucher của Shop', icon: LocalOfferOutlined, permission: 'Seller' },
    { path: 'flash-sales', label: 'Flash Sale', icon: FlashOnOutlined, permission: 'Seller' },
    { path: 'reviews', label: 'Đánh giá', icon: StarOutlineRounded, permission: 'Seller' },
    { path: 'live', label: 'Livestream', icon: LiveTvOutlined, permission: 'Seller' },
    { path: 'analytics', label: 'Phân tích', icon: BarChartOutlined, permission: 'Seller' }
  ] },
  { label: 'Quản lý', items: [
    { path: 'wallet', label: 'Ví & Rút tiền', icon: AccountBalanceWalletOutlined, permission: 'Seller' },
    { path: 'notifications', label: 'Thông báo', icon: NotificationsNoneRounded, permission: 'Seller' },
    { path: 'profile', label: 'Hồ sơ & Kho của Shop', icon: StorefrontOutlined, permission: 'Seller' }
  ] }
]
export const legacySellerPaths: Record<string, string> = { finance: 'wallet', settings: 'profile', messages: 'notifications' }
