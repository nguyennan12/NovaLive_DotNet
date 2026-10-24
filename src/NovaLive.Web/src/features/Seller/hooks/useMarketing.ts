import * as service from '../services/marketingService'
import { useSellerMutation, useSellerQuery } from './useSellerQuery'
import type { FlashRegistrationRequest, PageRequest, UpdateDiscountRequest } from '../types/contracts'

export const useDiscounts = (params: PageRequest & { status?: string }) => useSellerQuery(['discounts', params], () => service.getDiscounts(params))
export const useCreateDiscount = () => useSellerMutation(service.createDiscount, 'Đã tạo voucher.')
export const useUpdateDiscount = () => useSellerMutation(({ id, data }: { id: string; data: UpdateDiscountRequest }) => service.updateDiscount(id, data), 'Đã cập nhật voucher.')
export const useCampaigns = () => useSellerQuery(['campaigns'], service.getCampaigns)
export const useRegistrations = (params: PageRequest & { status?: string }) => useSellerQuery(['flash-registrations', params], () => service.getRegistrations(params))
export const useRegisterFlashSale = () => useSellerMutation(({ campaignId, data }: { campaignId: string; data: FlashRegistrationRequest }) => service.registerFlashSale(campaignId, data), 'Đã gửi đăng ký Flash Sale.')
