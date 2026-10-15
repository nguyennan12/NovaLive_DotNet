import ShopAddressCard from './ShopProfile/ShopAddressCard'
import UserAddressCard from './UserProfile/UserAddressCard'

const AddressCard = ({ isShop }) => {
  if (isShop) return <ShopAddressCard />
  return <UserAddressCard />
}

export default AddressCard
