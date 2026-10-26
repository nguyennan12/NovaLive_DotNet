# Frontend Style Guide

> Reference doc cho AI và dev khi làm UI. Đọc file này trước khi tạo component mới.

---

## Theme màu (src/theme.js)

| Token | Hex | Dùng cho |
|---|---|---|
| `primary.main` | `#fafafa` | Nền card, nền section |
| `primary.contrastText` | `#2d2d2d` | Text chính |
| `secondary.main` | `#3485f7` | Nhấn xanh — nút, giá, focus border |
| `third.main` | `#8b5cf6` | Nhấn tím — icon "đề xuất" |
| `fourth.main` | `#f59e0b` | Nhấn vàng — best seller, badge |
| `divider` | `#eeeeee` | Border nhạt |

---

## Nền trang — WaterDropBackground (hiện tại đã bọc ở layout chung)

Dùng `@emotion/styled(Box)` khi cần `::before`/`::after` hoặc keyframe (MUI `sx` không đủ).

```js
const WaterDropBackground = styled(Box)({
  background: 'linear-gradient(135deg, #f8faff 0%, #f0f4ff 50%, #f5f8ff 100%)',
  position: 'relative', overflow: 'hidden',
  '&::before': {
    content: '""', position: 'absolute', inset: 0,
    backgroundImage: 'url(...)', backgroundSize: 'cover',
    opacity: 0.6, zIndex: 0
  },
  '& > *': { position: 'relative', zIndex: 1 }
})
```

---

## Glassmorphism section container

Dùng nhất quán cho các section nổi bật (Flash Sale, Best Seller, Grid):

```js
sx={{
  background: 'rgba(255, 255, 255, 0.1)',
  backdropFilter: 'blur(10px)',
  WebkitBackdropFilter: 'blur(10px)',
  borderRadius: 3,                            // = 24px
  boxShadow: '0 4px 30px rgba(0,0,0,0.05)',
  p: 2
}}
có thể dùng ...glassSX
```

---

## Section header

```jsx
// Accent bar (thanh màu dọc trái)
<Box sx={{ width: 3.5, height: 22, borderRadius: '2px', bgcolor: accentColor, flexShrink: 0 }} />

// Gradient title (dùng cho Best Seller / Flash Sale)
<Typography sx={{
  background: 'linear-gradient(to right, #FFD700 0%, #FFA500 100%)',
  WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent',
  fontSize: { xs: '1.5rem', sm: '2.5rem' }, fontWeight: 900
}}>
  BEST SELLER
</Typography>

// "Xem tất cả" link chuẩn
<Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, cursor: 'pointer', opacity: 0.75, '&:hover': { opacity: 1 } }}>
  <Typography sx={{ fontSize: '0.78rem', color: 'secondary.main', fontWeight: 600 }}>Xem tất cả</Typography>
  <ArrowForwardIosRoundedIcon sx={{ fontSize: 11, color: 'secondary.main' }} />
</Box>
```

---

## Product card — consumer view (HomeProductCard)

File: `src/features/Home/components/HomeProductCard.jsx`

| Variant | Dùng ở |
|---|---|
| `portrait` (mặc định) | Scroll section, grid |
| `landscape` | Best seller 2-col |

- **Không** có edit/delete (đó là admin card ở `ProductCard.jsx`)
- Hiển thị: ảnh, tên, giá `formatVND`, `total_sold` ("lượt mua") với `ShoppingBasketRoundedIcon`
- **Không** hiện stock badge
- Wishlist: `FavoriteBorderIcon` góc trên phải ảnh
- Cart: `ShoppingCartOutlinedIcon` góc dưới phải
- Hover: `translateY(-4px)` + tăng `boxShadow`
- Skeleton: `<HomeProductCardSkeleton variant="portrait|landscape" />`

### Admin card (ProductCard.jsx — dashboard)
- Có edit/delete hidden action
- Có stock badge (in/low/out)
- Navigate to `/products/form/:id`

---

## Category cards (CategorySection)

- Grid: `repeat(4,1fr)` xs → `repeat(5,1fr)` sm → `repeat(10,1fr)` md = **2 hàng trên desktop**
- Background: gradient từ bảng 20 màu blue/cyan tones
- Icon white + text white + `textShadow`
- Hover: `translateY(-4px) scale(1.04)`
- Click → filter sản phẩm theo `cat_slug`

---

## Banner (BannerHomePage)

- Layout: **65% carousel** + **35% ảnh tĩnh** cạnh phải
- Carousel auto-play `4000ms`, prev/next arrow (`backdropFilter: blur(6px)`), dot indicator
- Height: `{ xs: 250, sm: 350, md: 500 }`
- Export thêm: `PosterFirst`, `PosterSecond` — ảnh ngang full-width dạng banner phụ

---

## Filter bar (HomeFilterBar)

- Chỉ có: **select danh mục** + **select sắp xếp** (không có ô search text)
- Style select: `bgcolor: #f5f5f5`, `borderRadius: 10px`, `height: 38`
- Khi có danh mục active: hiện pill inline (click để xóa)
- Logic: chỉ gọi `queryProductAPI` khi `category !== 'all'`; mặc định dùng `getAllProductsAPI`

---

## Responsive grid chuẩn

| Breakpoint | Products grid | Category grid |
|---|---|---|
| xs `< 600px` | 2 cols | 4 cols |
| sm `600–900px` | 3 cols | 5 cols |
| md `900–1200px` | 4 cols | 10 cols |
| lg `> 1200px` | 5 cols | 10 cols |

---

## Utility hay dùng

```js
import { formatVND, formatDate, formatLiveDurationClock } from '~/common/utils/formatters'
import { buildQueryParamsProducts } from '~/common/utils/builder'

formatVND(1500000)              // "1.500.000 ₫"
buildQueryParamsProducts(filters)       // → object params → new URLSearchParams(params).toString()

// Scroll section scroll button
const scrollRef = useRef(null)
scrollRef.current?.scrollBy({ left: ±460, behavior: 'smooth' })
```

---

## SPU model fields (backend)

```
spu_name, spu_code, spu_thumb, spu_description, spu_slug
spu_price, spu_quantity, spu_category[], total_sold
spu_ratingsAvg, spu_attributes[], spu_variations[]
isDraft, isPublished, isDeleted
```

Field quan trọng cho consumer UI: `spu_price`, `spu_thumb`, `spu_name`, `total_sold`, `spu_quantity`
