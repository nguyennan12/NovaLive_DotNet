export const API_ROOT = 'http://localhost:3031/v1/api'

export const LIMIT_PRODUCTS = 30

export const LIMIT = {
  PRODUCTS: 30,
  INVENTORY: 10,
  DISCOUNT: 20,
  ORDER: 20
}

export const PRICE_SLIDER_MIN = 0
export const PRICE_SLIDER_MAX = 50_000_000
export const PRICE_SLIDER_STEP = 500_000

export const SORT_OPTIONS = [
  { value: 'newest', label: 'Mới nhất' },
  { value: 'price_asc', label: 'Giá: Thấp → Cao' },
  { value: 'price_desc', label: 'Giá: Cao → Thấp' },
  { value: 'name_az', label: 'Tên A → Z' }
]

export const DEFAULT_FILTERS = { priceRange: [PRICE_SLIDER_MIN, PRICE_SLIDER_MAX], sort: 'newest' }

export const LOGO = {
  COD: 'https://res.cloudinary.com/nguyennan12/image/upload/v1777652319/Livestream-ecommerce/uwsohe8omck56ehjt7d1.jpg',
  VNPAY: 'https://res.cloudinary.com/nguyennan12/image/upload/v1777652132/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/pdbnoctkqricctef7ath.jpg',
  MOMO: 'https://res.cloudinary.com/nguyennan12/image/upload/v1777651847/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/lqcgllzp6rid7dbucflk.png',
  GHN: 'https://res.cloudinary.com/nguyennan12/image/upload/v1777652145/Livestream-ecommerce/Shops/69e364dfdf24f31846f15580/Products/swfs1pjidv8hcyqpvpm8.png'
}