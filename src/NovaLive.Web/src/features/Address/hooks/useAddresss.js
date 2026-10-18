import { useQuery } from '@tanstack/react-query'
import { getAllUserAddressAPI, getUserAddressAPI } from '~/common/apis/services/addressService'

export const useAddress = (user) => {
  const userId = user?.id || user?._id
  const addressId = user?.default_address_id

  const { data: allAddressUser = [] } = useQuery({
    queryKey: ['all_address_user', userId],
    queryFn: () => getAllUserAddressAPI(),
    enabled: !!userId,
    staleTime: 1000 * 30
  })

  const { data: addressUser } = useQuery({
    queryKey: ['address_user', addressId],
    queryFn: () => getUserAddressAPI(addressId),
    enabled: !!addressId,
    staleTime: 1000 * 30
  })

  const defaultFromList = allAddressUser.find((a) => a.isDefault || a.is_default) || allAddressUser[0] || null

  return {
    addressUser: addressUser || defaultFromList,
    allAddressUser
  }
}