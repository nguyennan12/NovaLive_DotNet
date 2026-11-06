import GridViewRounded from '@mui/icons-material/GridViewRounded'
import ShoppingBagOutlined from '@mui/icons-material/ShoppingBagOutlined'
import Inventory2Outlined from '@mui/icons-material/Inventory2Outlined'
import WarehouseOutlined from '@mui/icons-material/WarehouseOutlined'
import LocalOfferOutlined from '@mui/icons-material/LocalOfferOutlined'
import PeopleAltOutlined from '@mui/icons-material/PeopleAltOutlined'
import LiveTvOutlined from '@mui/icons-material/LiveTvOutlined'
import AccountBalanceOutlined from '@mui/icons-material/AccountBalanceOutlined'
import LocalShippingOutlined from '@mui/icons-material/LocalShippingOutlined'
import NotificationsNoneRounded from '@mui/icons-material/NotificationsNoneRounded'
import SettingsOutlined from '@mui/icons-material/SettingsOutlined'
import type { SvgIconComponent } from '@mui/icons-material'

export interface SellerNavItem {
  path: string
  label: string
  icon: SvgIconComponent
  permission?: string
  aliases?: string[]
}

export interface SellerNavSection {
  label: string
  items: SellerNavItem[]
}

export const sellerNavigation: SellerNavSection[] = [
  {
    label: 'GENERAL',
    items: [
      { path: '', label: 'Dashboard', icon: GridViewRounded, aliases: ['dashboard'] },
      { path: 'orders', label: 'Orders', icon: ShoppingBagOutlined, aliases: ['returns'] },
      { path: 'products', label: 'Products', icon: Inventory2Outlined },
      { path: 'inventory', label: 'Inventory', icon: WarehouseOutlined },
      { path: 'customers', label: 'Customers', icon: PeopleAltOutlined }
    ]
  },
  {
    label: 'TOOLS',
    items: [
      { path: 'live', label: 'Live', icon: LiveTvOutlined },
      { path: 'discounts', label: 'Discounts', icon: LocalOfferOutlined },
      { path: 'wallet', label: 'Finance', icon: AccountBalanceOutlined, aliases: ['finance'] },
      { path: 'shipping', label: 'Shipping & Fulfillment', icon: LocalShippingOutlined }
    ]
  },
  {
    label: 'PROFILE',
    items: [
      { path: 'notifications', label: 'Notifications', icon: NotificationsNoneRounded },
      { path: 'profile', label: 'Settings', icon: SettingsOutlined, aliases: ['settings'] }
    ]
  }
]

export const legacySellerPaths: Record<string, string> = {
  finance: 'wallet',
  settings: 'profile',
  messages: 'notifications'
}
