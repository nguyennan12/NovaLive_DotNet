import { db, mockDashboard, paginate, required, saveMock } from './database'
import type { DateRange, PagedResult, PageRequest } from '../types/contracts'
import type { SellerDashboard, SellerLive, SellerNotification } from '../types/viewModels'

export function mockGetDashboard(params: DateRange): SellerDashboard | Promise<SellerDashboard> { return mockDashboard(params.from, params.to) }

export function mockGetNotifications(params: PageRequest & { isRead?: boolean }): PagedResult<SellerNotification> | Promise<PagedResult<SellerNotification>> { return paginate(db().notifications.filter(item => params.isRead === undefined || item.isRead === params.isRead), params) }

export function mockReadNotification(id: string): unknown | Promise<unknown> { required(db().notifications.find(item => item.id === id), 'Thông báo').isRead = true; saveMock(); return {} }

export function mockReadAllNotifications(): unknown | Promise<unknown> { db().notifications.forEach(item => { item.isRead = true }); saveMock(); return {} }

export function mockGetSellerLives(params: PageRequest): PagedResult<SellerLive> | Promise<PagedResult<SellerLive>> { return paginate(db().lives, params) }
