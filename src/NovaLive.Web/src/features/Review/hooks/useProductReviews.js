// Swap MOCK_* with real API calls when backend ready
// Expected API: getProductReviewsAPI(productId, { page, limit })

const MOCK_REVIEWS = [
  {
    _id: '1',
    user: { name: 'Nguyễn Minh Tuấn', avatar: '' },
    rating: 5,
    content: 'Sản phẩm chất lượng tốt, đóng gói cẩn thận. Giao hàng nhanh, đúng như mô tả. Sẽ ủng hộ shop lần sau!',
    createdAt: new Date(Date.now() - 2 * 24 * 3600 * 1000).toISOString(),
    likes: 12
  },
  {
    _id: '2',
    user: { name: 'Trần Thị Hoa', avatar: '' },
    rating: 4,
    content: 'Màu sắc đẹp, khớp với ảnh. Chỉ cần ship thêm nhanh hơn nữa thì hoàn hảo.',
    createdAt: new Date(Date.now() - 5 * 24 * 3600 * 1000).toISOString(),
    likes: 5
  },
  {
    _id: '3',
    user: { name: 'Lê Văn Bình', avatar: '' },
    rating: 5,
    content: 'Ổn, mình đã mua lần 2 rồi. Chất lượng ổn định, không bị hàng lỗi hay thiếu phụ kiện gì.',
    createdAt: new Date(Date.now() - 10 * 24 * 3600 * 1000).toISOString(),
    likes: 8
  }
]

const MOCK_SUMMARY = {
  avg: 4.7,
  total: 128,
  distribution: { 5: 89, 4: 24, 3: 10, 2: 3, 1: 2 }
}

// eslint-disable-next-line no-unused-vars
export const useProductReviews = (_productId) => {
  return {
    reviews: MOCK_REVIEWS,
    summary: MOCK_SUMMARY,
    isLoading: false
  }
}
