import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { toast } from 'react-toastify'
import { SELLER_QUERY_KEY } from '../constants'
import { errorMessage } from '../utils/errors'
import { useSelector } from 'react-redux'
import type { SellerRootState } from '../types/viewModels'

export function useSellerQuery<T>(key: readonly unknown[], queryFn: () => Promise<T>, enabled = true) {
  const owner = useSelector((state: SellerRootState) => state.user.currentUser?.id || state.user.currentUser?._id || state.user.currentUser?.accessToken || 'anonymous')
  return useQuery({ queryKey: [...SELLER_QUERY_KEY, owner, ...key], queryFn, enabled, staleTime: 20_000, retry: false })
}
export function useSellerMutation<TVariables, TData = unknown>(mutationFn: (variables: TVariables) => Promise<TData>, successMessage: string, onSuccess?: (data: TData) => void) {
  const client = useQueryClient()
  return useMutation({
    mutationFn,
    onSuccess: async data => {
      await client.invalidateQueries({ queryKey: SELLER_QUERY_KEY })
      toast.success(successMessage)
      onSuccess?.(data)
    },
    onError: (error: unknown) => toast.error(errorMessage(error))
  })
}
export function useDebouncedValue<T>(value: T, delay = 350) {
  const [result, setResult] = useState(value)
  useEffect(() => { const timer = setTimeout(() => setResult(value), delay); return () => clearTimeout(timer) }, [value, delay])
  return result
}
