import { useQuery } from '@tanstack/react-query'
import { getAllUserAddressAPI, getUserAddressAPI } from '~/common/apis/services/addressService'

export const useAddress = (user) => {
  const addressId = user?.default_address_id
  const userId = user?._id
  const ownerType = user?.user_shop ? 'shop' : 'user'
  // const ownerType = 'user'

  const { data: addressUser } = useQuery({
    queryKey: ['address_user', addressId],
    queryFn: () => getUserAddressAPI(addressId),
    enabled: !!addressId,
    staleTime: 1000 * 30
  })

  const { data: allAddressUser = [] } = useQuery({
    queryKey: ['all_address_user', userId],
    queryFn: () => getAllUserAddressAPI(ownerType),
    enabled: !!userId,
    staleTime: 1000 * 30
  })
  return {
    addressUser,
    allAddressUser
  }
}