import * as mocks from '../mocks/dashboard'
import { request } from './transport'

import type { DateRange, PagedResult, PageRequest } from '../types/contracts'
import type { SellerDashboard, SellerLive, SellerNotification } from '../types/viewModels'

export const getDashboard = (params: DateRange) => request<SellerDashboard>('Báo cáo', { url: 'seller/reports/dashboard', params }, () => mocks.mockGetDashboard(params))
export const getNotifications = (params: PageRequest & { isRead?: boolean }) => request<PagedResult<SellerNotification>>('Thông báo', { url: 'notifications', params }, () => mocks.mockGetNotifications(params))
export const readNotification = (id: string) => request<unknown>('Thông báo', { method: 'PUT', url: `notifications/${id}/read` }, () => mocks.mockReadNotification(id))
export const readAllNotifications = () => request<unknown>('Thông báo', { method: 'PUT', url: 'notifications/read-all' }, () => mocks.mockReadAllNotifications())
// Proposed list endpoint: absent in backend; real mode displays the standard error state.
export const getSellerLives = (params: PageRequest) => request<PagedResult<SellerLive>>('Phiên livestream', { url: 'seller/livestreams', params }, () => mocks.mockGetSellerLives(params))
