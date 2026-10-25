import * as mocks from '../mocks/return'
import { request } from './transport'

import type { OrderReturn, PagedResult, PageRequest } from '../types/contracts'

export const getReturns = (params: PageRequest & { status?: string }) => request<PagedResult<OrderReturn>>('Trả hàng', { url: 'seller/returns', params }, () => mocks.mockGetReturns(params))
export type ReturnDecision = { action: 'approve'; note: string } | { action: 'reject'; reason: string; evidenceUrls: string[] } | { action: 'received' }
export const decideReturn = (id: string, decision: ReturnDecision) => request<unknown>('Trả hàng', { method: 'PUT', url: `seller/returns/${id}/${decision.action}`, data: decision.action === 'approve' ? { note: decision.note || null } : decision.action === 'reject' ? { reason: decision.reason, evidenceUrls: decision.evidenceUrls } : undefined }, () => mocks.mockDecideReturn(id, decision))
