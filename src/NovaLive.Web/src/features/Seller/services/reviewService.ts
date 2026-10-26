import { request } from './transport'
import { allSellerProducts } from './productService'
import { mockProductReviews, mockReplyReview } from '../mocks/review'
import type { PagedResult, Review } from '../types/contracts'
import type { SellerReview } from '../types/viewModels'
import { REVIEW_PAGE_SIZE } from '../constants'

// Aggregate every product/review page; never apply filters only to a partial page.
// Replace with GET /seller/reviews once backend provides shop-scoped pagination.
export async function getSellerReviews(): Promise<SellerReview[]> {
  const products = await allSellerProducts()
  const rows: SellerReview[] = []
  for (const product of products) {
    let page = 1
    while (true) {
      const result = await request<PagedResult<Review>>('Đánh giá', { url: `products/${product.id}/reviews`, params: { page, size: REVIEW_PAGE_SIZE } }, () => mockProductReviews(product.id, page))
      rows.push(...result.items.map(review => ({ ...review, productName: product.name })))
      if (!result.items.length || page >= result.totalPages) break
      page++
    }
  }
  return rows.sort((a, b) => b.createdAt.localeCompare(a.createdAt))
}
export const replyReview = (id: string, replyText: string) => request<unknown>('Đánh giá', { method: 'POST', url: `seller/reviews/${id}/reply`, data: { replyText } }, () => mockReplyReview(id, replyText))
