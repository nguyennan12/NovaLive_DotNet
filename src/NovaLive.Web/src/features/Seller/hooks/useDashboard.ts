import * as service from '../services/dashboardService'
import { useSellerMutation, useSellerQuery } from './useSellerQuery'
import type { DateRange, PageRequest } from '../types/contracts'

export const useDashboard = (range: DateRange) => useSellerQuery(['dashboard', range], () => service.getDashboard(range))
export const useNotifications = (params: PageRequest & { isRead?: boolean }) => useSellerQuery(['notifications', params], () => service.getNotifications(params))
export const useUnreadCount = () => useSellerQuery(['unread-count'], async () => (await service.getNotifications({ page: 1, size: 1, isRead: false })).total)
export const useReadNotification = () => useSellerMutation(service.readNotification, 'Đã đánh dấu đã đọc.')
export const useReadAllNotifications = () => useSellerMutation(() => service.readAllNotifications(), 'Đã đánh dấu tất cả thông báo đã đọc.')
export const useSellerLives = (params: PageRequest) => useSellerQuery(['lives', params], () => service.getSellerLives(params))
