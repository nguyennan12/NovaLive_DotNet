import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  cancelLiveSessionAPI,
  createLiveSessionAPI,
  getLiveHistoryAPI,
  getLiveRevenueChartAPI,
  getLiveStatsAPI,
  getUpommingLiveSessionsAPI,
  scheduleLiveSessionAPI,
  startLiveAPI,
  startScheduledLiveAPI,
  updateLiveSessionAPI
} from '~/common/apis/services/liveService'

export const useLiveStats = () =>
  useQuery({
    queryKey: ['live', 'stats'],
    queryFn: getLiveStatsAPI,
    staleTime: 30 * 1000
  })

export const useUpcomingSession = () =>
  useQuery({
    queryKey: ['live', 'upcoming'],
    queryFn: async () => getUpommingLiveSessionsAPI(),
    staleTime: 30 * 1000
  })

export const useSessionHistory = (params) => {

  const { data = [] } = useQuery({
    queryKey: ['live', 'history', params],
    queryFn: () => getLiveHistoryAPI(params),
    staleTime: 20 * 1000
  })
  const sessions = data?.lives || []
  const totalPages = data?.totalPages || 1
  return { sessions, totalPages }
}

export const useLiveRevenueChart = (period) =>
  useQuery({
    queryKey: ['live', 'revenue', period],
    queryFn: () => getLiveRevenueChartAPI(period),
    staleTime: 30 * 1000
  })

export const useCreateSession = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: createLiveSessionAPI,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['live'] })
  })
}

export const useScheduleSession = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: scheduleLiveSessionAPI,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['live'] })
  })
}

export const useUpdateSession = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ sessionId, data }) => updateLiveSessionAPI(sessionId, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['live'] })
  })
}

export const useCancelSession = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: cancelLiveSessionAPI,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['live'] })
  })
}

export const useStartSession = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: startLiveAPI,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['live'] })
  })
}

export const useStartScheduledSession = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: startScheduledLiveAPI,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['live'] })
  })
}
