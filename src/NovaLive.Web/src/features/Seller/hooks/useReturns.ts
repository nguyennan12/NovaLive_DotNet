import * as service from '../services/returnService'
import { useSellerMutation, useSellerQuery } from './useSellerQuery'
import type { PageRequest } from '../types/contracts'

export const useReturns = (params: PageRequest & { status?: string }) => useSellerQuery(['returns', params], () => service.getReturns(params))
export const useDecideReturn = () => useSellerMutation(({ id, decision }: { id: string; decision: service.ReturnDecision }) => service.decideReturn(id, decision), 'Đã cập nhật yêu cầu hoàn hàng.')
