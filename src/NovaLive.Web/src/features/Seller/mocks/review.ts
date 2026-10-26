import { db, paginate, required, saveMock } from './database'
import type { PagedResult, Review } from '../types/contracts'
import { REVIEW_PAGE_SIZE } from '../constants'

export function mockProductReviews(spuId: string, page: number): PagedResult<Review> { return paginate(db().reviews.filter(item => item.spuId === spuId), { page, size: REVIEW_PAGE_SIZE }) }
export function mockReplyReview(id: string, replyText: string): unknown {
  const item = required(db().reviews.find(item => item.id === id), 'Đánh giá')
  if (item.sellerReply) throw new Error('Bạn chỉ được phản hồi đánh giá một lần.')
  item.sellerReply = replyText; item.repliedAt = new Date().toISOString(); saveMock(); return structuredClone(item)
}
