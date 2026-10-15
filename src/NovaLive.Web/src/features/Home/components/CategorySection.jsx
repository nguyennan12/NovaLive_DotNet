import BuildRoundedIcon from '@mui/icons-material/BuildRounded'
import CategoryRoundedIcon from '@mui/icons-material/CategoryRounded'
import CheckroomRoundedIcon from '@mui/icons-material/CheckroomRounded'
import ChildCareRoundedIcon from '@mui/icons-material/ChildCareRounded'
import ComputerRoundedIcon from '@mui/icons-material/ComputerRounded'
import DiamondRoundedIcon from '@mui/icons-material/DiamondRounded'
import DirectionsCarRoundedIcon from '@mui/icons-material/DirectionsCarRounded'
import FaceRetouchingNaturalRoundedIcon from '@mui/icons-material/FaceRetouchingNaturalRounded'
import HeadsetRoundedIcon from '@mui/icons-material/HeadsetRounded'
import HomeRoundedIcon from '@mui/icons-material/HomeRounded'
import KitchenRoundedIcon from '@mui/icons-material/KitchenRounded'
import LocalGroceryStoreRoundedIcon from '@mui/icons-material/LocalGroceryStoreRounded'
import LocalPharmacyRoundedIcon from '@mui/icons-material/LocalPharmacyRounded'
import MenuBookRoundedIcon from '@mui/icons-material/MenuBookRounded'
import PetsRoundedIcon from '@mui/icons-material/PetsRounded'
import PhotoCameraRoundedIcon from '@mui/icons-material/PhotoCameraRounded'
import RestaurantRoundedIcon from '@mui/icons-material/RestaurantRounded'
import ShoppingBagRoundedIcon from '@mui/icons-material/ShoppingBagRounded'
import SmartphoneRoundedIcon from '@mui/icons-material/SmartphoneRounded'
import SpaRoundedIcon from '@mui/icons-material/SpaRounded'
import SportsEsportsRoundedIcon from '@mui/icons-material/SportsEsportsRounded'
import SportsSoccerRoundedIcon from '@mui/icons-material/SportsSoccerRounded'
import TvRoundedIcon from '@mui/icons-material/TvRounded'
import WatchRoundedIcon from '@mui/icons-material/WatchRounded'
import { Box, Typography } from '@mui/material'
import { useSelector } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { selectCategories } from '~/store/product/categorySlice'

const GRADIENTS = [
  'linear-gradient(135deg, #6DD5FA 0%, #E0F7FA 100%)',
  'linear-gradient(135deg, #89F7FE 0%, #66A6FF 100%)',
  'linear-gradient(135deg, #4FACFE 0%, #00F2FE 100%)',
  'linear-gradient(135deg, #48C6EF 0%, #EAF8FD 100%)',
  'linear-gradient(135deg, #8FD3F4 0%, #D0EEF9 100%)',
  'linear-gradient(135deg, #00D2FF 0%, #E0F6FF 100%)',
  'linear-gradient(135deg, #70D6FF 0%, #D9F5FF 100%)',
  'linear-gradient(135deg, #87CEFA 0%, #F0F8FF 100%)',
  'linear-gradient(135deg, #58CFFB 0%, #C8F0FF 100%)',
  'linear-gradient(135deg, #66C2FF 0%, #E6F5FF 100%)',
  'linear-gradient(135deg, #A1C4FD 0%, #C2E9FB 100%)',
  'linear-gradient(135deg, #5CE1E6 0%, #EBFBFC 100%)',
  'linear-gradient(135deg, #80DFFF 0%, #E6FAFF 100%)',
  'linear-gradient(135deg, #4DB8FF 0%, #D6EFFF 100%)',
  'linear-gradient(135deg, #84CEEB 0%, #E3F4FE 100%)',
  'linear-gradient(135deg, #5AB9EA 0%, #C1E8FF 100%)',
  'linear-gradient(135deg, #63A4FF 0%, #D4E7FE 100%)',
  'linear-gradient(135deg, #90CAF9 0%, #E3F2FD 100%)',
  'linear-gradient(135deg, #81D4FA 0%, #E1F5FE 100%)',
  'linear-gradient(135deg, #4FC3F7 0%, #E1F5FE 100%)'
]

const ICON_MAP = [
  { keys: ['dien-thoai', 'phone', 'mobile', 'smartphone'], Icon: SmartphoneRoundedIcon },
  { keys: ['laptop', 'may-tinh', 'computer', 'pc', 'tablet'], Icon: ComputerRoundedIcon },
  { keys: ['tai-nghe', 'audio', 'headset', 'headphone', 'am-thanh'], Icon: HeadsetRoundedIcon },
  { keys: ['camera', 'anh', 'photo'], Icon: PhotoCameraRoundedIcon },
  { keys: ['tv', 'tivi', 'man-hinh'], Icon: TvRoundedIcon },
  { keys: ['thoi-trang', 'quan-ao', 'ao', 'quan', 'fashion', 'clothing'], Icon: CheckroomRoundedIcon },
  { keys: ['sach', 'book', 'van-phong-pham', 'stationery'], Icon: MenuBookRoundedIcon },
  { keys: ['game', 'gaming', 'esport'], Icon: SportsEsportsRoundedIcon },
  { keys: ['my-pham', 'beauty', 'lam-dep', 'trang-diem', 'cosmetic'], Icon: FaceRetouchingNaturalRoundedIcon },
  { keys: ['nha-cua', 'noi-that', 'home', 'furniture', 'gia-dung'], Icon: HomeRoundedIcon },
  { keys: ['o-to', 'xe', 'auto', 'car', 'motor', 'xe-may'], Icon: DirectionsCarRoundedIcon },
  { keys: ['the-thao', 'sport', 'fitness', 'gym', 'outdoor'], Icon: SportsSoccerRoundedIcon },
  { keys: ['thuc-pham', 'food', 'an-uong', 'do-an', 'restaurant'], Icon: RestaurantRoundedIcon },
  { keys: ['thu-cung', 'pet', 'cho', 'meo'], Icon: PetsRoundedIcon },
  { keys: ['tre-em', 'baby', 'do-choi', 'toy', 'kid'], Icon: ChildCareRoundedIcon },
  { keys: ['suc-khoe', 'health', 'thuoc', 'y-te', 'pharmacy'], Icon: LocalPharmacyRoundedIcon },
  { keys: ['cong-cu', 'tool', 'dung-cu', 'may-moc', 'dien'], Icon: BuildRoundedIcon },
  { keys: ['spa', 'cham-soc', 'duong-da', 'skincare'], Icon: SpaRoundedIcon },
  { keys: ['tui-xach', 'bag', 'accessories', 'phu-kien', 'balo'], Icon: ShoppingBagRoundedIcon },
  { keys: ['dong-ho', 'watch', 'jewelry', 'trang-suc', 'vong'], Icon: WatchRoundedIcon },
  { keys: ['bep', 'kitchen', 'nau-an', 'noi'], Icon: KitchenRoundedIcon },
  { keys: ['sieu-thi', 'grocery', 'tap-hoa', 'hang-tieu-dung'], Icon: LocalGroceryStoreRoundedIcon },
  { keys: ['trang-suc', 'jewelry', 'vang', 'bac', 'kim-cuong'], Icon: DiamondRoundedIcon }
]

const getIcon = (slug = '', name = '') => {
  const target = (slug + ' ' + name).toLowerCase()
  for (const { keys, Icon } of ICON_MAP) {
    if (keys.some(k => target.includes(k))) return Icon
  }
  return CategoryRoundedIcon
}

const CategoryCard = ({ category, gradient, onClick, Icon }) => {
  return (
    <Box
      onClick={onClick}
      sx={{
        background: gradient,
        borderRadius: { xs: '14px', sm: '16px' },
        p: { xs: 1.25, sm: 1.5 },
        display: 'flex', flexDirection: 'column',
        alignItems: 'center', justifyContent: 'center',
        gap: { xs: 0.6, sm: 0.75 },
        cursor: 'pointer',
        height: { xs: 80, sm: 96, md: 104 },
        transition: 'transform 0.18s ease, box-shadow 0.18s ease',
        boxShadow: '0 2px 10px rgba(0,0,0,0.12)',
        '&:hover': {
          transform: 'translateY(-4px) scale(1.04)',
          boxShadow: '0 8px 22px rgba(0,0,0,0.2)'
        },
        '&:active': { transform: 'scale(0.97)' }
      }}
    >
      <Icon sx={{
        fontSize: { xs: 24, sm: 28, md: 30 },
        color: 'white',
        filter: 'drop-shadow(0 1px 3px rgba(31, 30, 32, 0.25))'
      }} />
      <Typography sx={{
        fontSize: { xs: '0.64rem', sm: '0.7rem', md: '0.74rem' },
        fontWeight: 700,
        color: 'white',
        textAlign: 'center',
        lineHeight: 1.25,
        textShadow: '0 1px 3px rgba(20, 20, 21, 0.22)',
        display: '-webkit-box',
        WebkitLineClamp: 2,
        WebkitBoxOrient: 'vertical',
        overflow: 'hidden',
        px: 0.25
      }}>
        {category.cat_name}
      </Typography>
    </Box>
  )
}

const CategorySection = () => {
  const navigate = useNavigate()
  const categories = useSelector(selectCategories)
  const popular = (categories || []).slice(0, 20)

  if (!popular.length) return null

  return (
    <Box sx={{ my: { xs: 2.5, md: 3 } }}>
      <Box sx={{
        display: 'grid',
        gridTemplateColumns: {
          xs: 'repeat(4, 1fr)',
          sm: 'repeat(5, 1fr)',
          md: 'repeat(10, 1fr)'
        },
        gap: { xs: 1, sm: 1.25 }
      }}>
        {popular.map((cat, idx) => {
          const Icon = getIcon(cat.cat_slug, cat.cat_name)
          return (
            <CategoryCard
              key={cat._id || cat.id || cat.cat_slug}
              category={cat}
              gradient={GRADIENTS[idx % GRADIENTS.length]}
              Icon={Icon}
              onClick={() => navigate(`/products?category=${cat.cat_slug}`)}
            />
          )
        })}
      </Box>
    </Box>
  )
}

export default CategorySection
