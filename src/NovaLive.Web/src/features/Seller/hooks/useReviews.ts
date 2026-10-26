import * as service from '../services/reviewService'
import { useSellerMutation, useSellerQuery } from './useSellerQuery'

export const useReviews = () => useSellerQuery(['reviews'], service.getSellerReviews)
export const useReplyReview = () => useSellerMutation(({ id, replyText }: { id: string; replyText: string }) => service.replyReview(id, replyText), 'Đã gửi phản hồi. Mỗi đánh giá chỉ được phản hồi một lần.')
