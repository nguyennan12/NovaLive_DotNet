import ShopProfileHeader from './ShopProfile/ShopProfileHeader'
import UserProfileHeader from './UserProfile/UserProfileHeader'

const ProfileHeader = ({ profile, isLoading, isShop }) => {
  if (isShop) return <ShopProfileHeader />
  return <UserProfileHeader profile={profile} isLoading={isLoading} />
}

export default ProfileHeader
