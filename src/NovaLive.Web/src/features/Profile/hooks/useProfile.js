import { useQuery } from '@tanstack/react-query'
import { useSelector } from 'react-redux'
import { getMyProfileAPI } from '~/common/apis/services/userService'
import { selectCurrentUser } from '~/store/user/userSlice'

// Fetches enriched profile from server; falls back to Redux user if API fails
export const useProfile = () => {
  const currentUser = useSelector(selectCurrentUser)

  const { data: profile, isLoading, refetch } = useQuery({
    queryKey: ['my_profile', currentUser?._id],
    queryFn: getMyProfileAPI,
    staleTime: 5 * 60 * 1000,
    enabled: !!currentUser,
    retry: false
  })

  return {
    profile: profile ?? currentUser,
    isLoading: isLoading && !!currentUser,
    refetch
  }
}
