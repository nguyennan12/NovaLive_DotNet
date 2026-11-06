import AccountBalanceOutlinedIcon from '@mui/icons-material/AccountBalanceOutlined'
import DashboardRoundedIcon from '@mui/icons-material/DashboardRounded'
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined'
import WarehouseOutlinedIcon from '@mui/icons-material/WarehouseOutlined'
import LocalOfferOutlinedIcon from '@mui/icons-material/LocalOfferOutlined'
import BoltRoundedIcon from '@mui/icons-material/BoltRounded'
import MailOutlineRoundedIcon from '@mui/icons-material/MailOutlineRounded'
import NotificationsNoneRoundedIcon from '@mui/icons-material/NotificationsNoneRounded'
import PeopleAltOutlinedIcon from '@mui/icons-material/PeopleAltOutlined'
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined'
import ShoppingBagOutlinedIcon from '@mui/icons-material/ShoppingBagOutlined'
import LiveTvOutlinedIcon from '@mui/icons-material/LiveTvOutlined'

export const NAV_SECTIONS = [
  {
    label: 'General',
    items: [
      { key: 'dashboard', label: 'Dashboard', icon: DashboardRoundedIcon, badge: null, path: '' },
      { key: 'orders', label: 'Orders', icon: ShoppingBagOutlinedIcon, badge: null, path: '/orders' },
      { key: 'products', label: 'Products', icon: Inventory2OutlinedIcon, badge: null, path: '/products' },
      { key: 'inventory', label: 'Inventory', icon: WarehouseOutlinedIcon, badge: null, path: '/inventory' },
      { key: 'customers', label: 'Customers', icon: PeopleAltOutlinedIcon, badge: null, path: '/customers' }
    ]
  },
  {
    label: 'Tools',
    items: [
      { key: 'live', label: 'Live', icon: LiveTvOutlinedIcon, badge: null, path: '/live' },
      { key: 'discounts', label: 'Discounts', icon: LocalOfferOutlinedIcon, badge: null, path: '/discounts' },
      { key: 'flash-sales', label: 'Flash Sales', icon: BoltRoundedIcon, badge: null, path: '/flash-sales' },
      { key: 'finance', label: 'Finance', icon: AccountBalanceOutlinedIcon, badge: null, path: '/finance' }
    ]
  }
]

export const PROFILE_ITEMS = [
  { key: 'messages', label: 'Messages', icon: MailOutlineRoundedIcon, badge: null },
  { key: 'notifications', label: 'Notifications', icon: NotificationsNoneRoundedIcon, badge: null },
  { key: 'settings', label: 'Settings', icon: SettingsOutlinedIcon, badge: null }
]
