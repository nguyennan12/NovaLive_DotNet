import CalendarMonthRoundedIcon from '@mui/icons-material/CalendarMonthRounded'
import InventoryRoundedIcon from '@mui/icons-material/InventoryRounded'
import LocalOfferRoundedIcon from '@mui/icons-material/LocalOfferRounded'
import TrendingUpRoundedIcon from '@mui/icons-material/TrendingUpRounded'
import WarningAmberRoundedIcon from '@mui/icons-material/WarningAmberRounded'
import { Box, Button, Grid, Typography } from '@mui/material'
import { useNavigate } from 'react-router-dom'

const cardData = [
  {
    key: 'total',
    icon: InventoryRoundedIcon,
    iconBg: 'linear-gradient(135deg, #3465c8 0%, #4f83e3 100%)',
    iconColor: '#fff',
    label: 'Total Products',
    value: '1,245',
    sub: '+12 added this week',
    subColor: '#22c55e',
    subIcon: TrendingUpRoundedIcon,
    btnLabel: '+ Add Product',
    btnVariant: 'outlined',
    btnBorderColor: '#3465c8',
    btnColor: '#3465c8',
    path: '/products/form'
  },
  {
    key: 'stock',
    icon: WarningAmberRoundedIcon,
    iconBg: 'linear-gradient(135deg, #8b5cf6 0%, #a78bfa 100%)',
    iconColor: '#fff',
    label: 'Stock Inventory',
    value: '14 Items',
    valueSub: 'Low Stock',
    sub: '3 out of stock',
    subColor: '#ef4444',
    btnLabel: 'Manage Stock',
    btnVariant: 'outlined',
    btnBorderColor: '#8b5cf6',
    btnColor: '#8b5cf6',
    path: '/manager/inventory'
  },
  {
    key: 'discount',
    icon: LocalOfferRoundedIcon,
    iconBg: 'linear-gradient(135deg, #f59e0b 0%, #fbbf24 100%)',
    iconColor: '#fff',
    label: 'Active Discounts',
    value: '3',
    valueSub: 'Campaigns',
    sub: '2 ending this week',
    subColor: '#f59e0b',
    btnLabel: '+ New Discount',
    btnVariant: 'outlined',
    btnBorderColor: '#f59e0b',
    btnColor: '#f59e0b',
    path: '/manager/discount'
  },
  {
    key: 'schedule',
    icon: CalendarMonthRoundedIcon,
    iconBg: 'linear-gradient(135deg, #10b981 0%, #34d399 100%)',
    iconColor: '#fff',
    label: 'Upcoming Schedule',
    value: 'Oct 25',
    valueSub: 'Next Flash Sale',
    sub: '5 days remaining',
    subColor: '#10b981',
    btnLabel: 'View Calendar',
    btnVariant: 'outlined',
    btnBorderColor: '#10b981',
    btnColor: '#10b981'
  }
]

const ActionCard = ({ card }) => {
  const Icon = card.icon
  const SubIcon = card.subIcon
  const navigate = useNavigate()


  return (
    <Box
      sx={{
        bgcolor: 'primary.main',
        borderRadius: '12px',
        p: 2.5,
        boxShadow: '0 1px 4px rgba(0,0,0,0.06), 0 0 0 1px rgba(0,0,0,0.04)',
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
        height: '100%',
        transition: 'transform 0.18s ease, box-shadow 0.18s ease',
        '&:hover': {
          transform: 'translateY(-2px)',
          boxShadow: '0 6px 20px rgba(0,0,0,0.08), 0 0 0 1px rgba(0,0,0,0.04)'
        }
      }}
    >
      {/* Header row */}
      <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
        <Box
          sx={{
            width: 40,
            height: 40,
            borderRadius: '10px',
            background: card.iconBg,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flexShrink: 0
          }}
        >
          <Icon sx={{ fontSize: 20, color: card.iconColor }} />
        </Box>

        <Typography
          sx={{
            fontSize: '0.7rem',
            fontWeight: 500,
            color: 'primary.contrastText',
            opacity: 0.45,
            textTransform: 'uppercase',
            letterSpacing: '0.07em',
            mt: 0.5
          }}
        >
          {card.label}
        </Typography>
      </Box>

      {/* Value */}
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'baseline', gap: 0.75 }}>
          <Typography
            sx={{
              fontSize: '1.75rem',
              fontWeight: 700,
              color: 'primary.contrastText',
              lineHeight: 1,
              letterSpacing: '-0.03em'
            }}
          >
            {card.value}
          </Typography>
          {card.valueSub && (
            <Typography
              sx={{
                fontSize: '0.8rem',
                fontWeight: 500,
                color: 'primary.contrastText',
                opacity: 0.5,
                lineHeight: 1
              }}
            >
              {card.valueSub}
            </Typography>
          )}
        </Box>

        {/* Sub info */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.75 }}>
          {SubIcon && <SubIcon sx={{ fontSize: 13, color: card.subColor }} />}
          <Typography sx={{ fontSize: '0.75rem', color: card.subColor, fontWeight: 500 }}>
            {card.sub}
          </Typography>
        </Box>
      </Box>

      {/* Button */}
      <Button
        size='small'
        variant={card.btnVariant}
        onClick={() => navigate(card.path)}
        sx={{
          mt: 'auto',
          fontSize: '0.78rem',
          fontWeight: 600,
          borderRadius: '8px',
          py: 0.75,
          ...(card.btnVariant === 'contained'
            ? { bgcolor: card.btnBg, color: card.btnColor, '&:hover': { bgcolor: card.btnBg, opacity: 0.9 } }
            : { borderColor: card.btnBorderColor, color: card.btnColor, '&:hover': { borderColor: card.btnBorderColor, bgcolor: `${card.btnBorderColor}12` } }
          )
        }}
      >
        {card.btnLabel}
      </Button>
    </Box>
  )
}

const ProductActionCards = () => (
  <Grid container spacing={2} sx={{ mb: 3 }}>
    {cardData.map((card) => (
      <Grid key={card.key} size={{ xs: 12, sm: 6, lg: 3 }}>
        <ActionCard card={card} />
      </Grid>
    ))}
  </Grid>
)

export default ProductActionCards