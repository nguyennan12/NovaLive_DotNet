import { z } from 'zod'
import type { Campaign, Discount } from '../types/contracts'
import { utcDate } from './format'

export function canRegisterCampaign(campaign: Campaign, now = Date.now()): boolean {
  return campaign.status === 'Scheduled' && utcDate(campaign.startAt).getTime() > now
}

export function discountStatus(discount: Discount) {
  if (!discount.isActive) return 'Inactive'
  if (new Date(discount.validTo).getTime() < Date.now()) return 'Expired'
  if (new Date(discount.validFrom).getTime() > Date.now()) return 'Scheduled'
  return 'Active'
}
export const discountSchema = z.object({
  code: z.string().trim().min(3, 'Mã voucher cần ít nhất 3 ký tự.').regex(/^[A-Za-z0-9_-]+$/, 'Mã chỉ gồm chữ, số, dấu gạch ngang/gạch dưới.'), name: z.string().trim().min(2, 'Nhập tên voucher.'), discountType: z.enum(['PercentCart', 'FixedCart']), discountValue: z.number().positive('Mức giảm phải lớn hơn 0.'), minOrderAmount: z.number().min(0, 'Đơn tối thiểu không được âm.'), maxDiscountAmount: z.number().positive('Mức giảm tối đa phải lớn hơn 0.').nullable(), maxUses: z.number().int().positive('Tổng lượt dùng phải lớn hơn 0.').nullable(), perUserLimit: z.number().int().positive('Mỗi người dùng phải được sử dụng ít nhất 1 lần.'), validFrom: z.iso.datetime(), validTo: z.iso.datetime(), isPublic: z.boolean(), appliesTo: z.string(), targetIds: z.array(z.string()).nullable()
}).superRefine((value, context) => {
  if (value.validTo <= value.validFrom) context.addIssue({ code: 'custom', message: 'Ngày kết thúc phải sau ngày bắt đầu.', path: ['validTo'] })
  if (value.discountType === 'PercentCart') {
    if (value.discountValue > 100) context.addIssue({ code: 'custom', message: 'Phần trăm giảm không vượt quá 100%.', path: ['discountValue'] })
    if (!value.maxDiscountAmount) context.addIssue({ code: 'custom', message: 'Voucher phần trăm bắt buộc có mức giảm tối đa lớn hơn 0.', path: ['maxDiscountAmount'] })
  }
})
export function validateFlashRegistration(flashPrice: number, sellPrice: number, quantity: number, availableQty: number, perUserLimit: number): string | null {
  if (!Number.isFinite(flashPrice) || flashPrice <= 0) return 'Giá Flash Sale phải lớn hơn 0.'
  if (flashPrice > sellPrice * 0.9) return 'Giá Flash Sale phải giảm tối thiểu 10% so với giá bán hiện tại.'
  if (!Number.isInteger(quantity) || quantity <= 0 || quantity > availableQty) return 'Số lượng đăng ký phải là số nguyên dương và không vượt tồn khả dụng.'
  if (!Number.isInteger(perUserLimit) || perUserLimit < 1 || perUserLimit > quantity) return 'Giới hạn mỗi người phải từ 1 đến số lượng đăng ký.'
  return null
}
