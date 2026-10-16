import AccountBalanceOutlinedIcon from '@mui/icons-material/AccountBalanceOutlined'
import BarChartOutlinedIcon from '@mui/icons-material/BarChartOutlined'
import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded'
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined'
import LocalShippingOutlinedIcon from '@mui/icons-material/LocalShippingOutlined'
import MailOutlineRoundedIcon from '@mui/icons-material/MailOutlineRounded'
import NotificationsNoneRoundedIcon from '@mui/icons-material/NotificationsNoneRounded'
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined'
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined'
import ShoppingBagOutlinedIcon from '@mui/icons-material/ShoppingBagOutlined'
import TrackChangesOutlinedIcon from '@mui/icons-material/TrackChangesOutlined'
import LiveTvOutlinedIcon from '@mui/icons-material/LiveTvOutlined'

export const NAV_SECTIONS = [
  {
    label: 'General',
    items: [
      { key: 'dashboard', label: 'Dashboard', icon: DashboardRoundedIcon, badge: null, path: '' },
      { key: 'orders', label: 'Orders', icon: ShoppingBagOutlinedIcon, badge: null, path: '/orders' },
      { key: 'products', label: 'Products', icon: Inventory2OutlinedIcon, badge: null, path: '/products' },
      { key: 'customers', label: 'Customers', icon: PeopleAltOutlinedIcon, badge: null, path: '/customers' }
    ]
  },
  {
    label: 'Tools',
    items: [
      { key: 'live', label: 'Live', icon: LiveTvOutlinedIcon, badge: null, path: '/live' },
      { key: 'analytics', label: 'Analytics', icon: BarChartOutlinedIcon, badge: null, path: '/analytics' },
      { key: 'marketing', label: 'Marketing', icon: TrackChangesOutlinedIcon, badge: null, path: '/marketing' },
      { key: 'finance', label: 'Finance', icon: AccountBalanceOutlinedIcon, badge: null, path: '/finance' },
      { key: 'shipping', label: 'Shipping & Fulfillment', icon: LocalShippingOutlinedIcon, badge: null, path: '/shipping' }
    ]
  }
]

export const PROFILE_ITEMS = [
  { key: 'messages', label: 'Messages', icon: MailOutlineRoundedIcon, badge: null },
  { key: 'notifications', label: 'Notifications', icon: NotificationsNoneRoundedIcon, badge: null },
  { key: 'settings', label: 'Settings', icon: SettingsOutlinedIcon, badge: null }
]