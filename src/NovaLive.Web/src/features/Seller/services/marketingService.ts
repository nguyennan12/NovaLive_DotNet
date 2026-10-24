import * as mocks from '../mocks/marketing'
import { request } from './transport'

import type { Campaign, CreateDiscountRequest, Discount, FlashRegistration, FlashRegistrationRequest, PagedResult, PageRequest, UpdateDiscountRequest } from '../types/contracts'

export const getDiscounts = (params: PageRequest & { status?: string }) => request<PagedResult<Discount>>('Voucher', { url: 'seller/discounts', params: { ...params, status: params.status === 'Scheduled' ? 'upcoming' : params.status } }, () => mocks.mockGetDiscounts(params))
export const createDiscount = (data: CreateDiscountRequest) => request<Discount>('Voucher', { method: 'POST', url: 'seller/discounts', data }, () => mocks.mockCreateDiscount(data))
export const updateDiscount = (id: string, data: UpdateDiscountRequest) => request<Discount>('Voucher', { method: 'PUT', url: `seller/discounts/${id}`, data }, () => mocks.mockUpdateDiscount(id, data))
export const getCampaigns = () => request<Campaign[]>('Chiến dịch Flash Sale', { url: 'flash-sales/active' }, () => mocks.mockGetCampaigns())
export const getRegistrations = (params: PageRequest & { status?: string }) => request<PagedResult<FlashRegistration>>('Đăng ký Flash Sale', { url: 'seller/flash-sales/registrations', params }, () => mocks.mockGetRegistrations(params))
export const registerFlashSale = (campaignId: string, data: FlashRegistrationRequest) => request<FlashRegistration>('Đăng ký Flash Sale', { method: 'POST', url: `seller/flash-sales/${campaignId}/register`, data }, () => mocks.mockRegisterFlashSale(campaignId, data))
