import ShopActivityStatsCard from './ShopProfile/ShopActivityStatsCard'
import UserActivityStatsCard from './UserProfile/UserActivityStatsCard'

const ActivityStatsCard = ({ totalOrders, totalSpent, pendingOrders, isLoading, isShop }) => {
  if (isShop) return <ShopActivityStatsCard />
  return <UserActivityStatsCard totalOrders={totalOrders} totalSpent={totalSpent} pendingOrders={pendingOrders} isLoading={isLoading} />
}

export default ActivityStatsCard
