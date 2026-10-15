import AccountBalanceWalletRoundedIcon from '@mui/icons-material/AccountBalanceWalletRounded'
import CreditCardRoundedIcon from '@mui/icons-material/CreditCardRounded'
import LocalAtmRoundedIcon from '@mui/icons-material/LocalAtmRounded'
import PaymentsRoundedIcon from '@mui/icons-material/PaymentsRounded'
import { Box, RadioGroup, Typography } from '@mui/material'
import { Controller } from 'react-hook-form'
import { glassSx, gradientText } from '~/theme'
import { LOGO } from '~/common/utils/constant'

const PAYMENT_METHODS = [
  {
    value: 'cod',
    label: 'Thanh toán khi nhận hàng (COD)',
    desc: 'Thanh toán bằng tiền mặt khi nhận hàng',
    Icon: LocalAtmRoundedIcon,
    color: '#16a34a',
    logoSrc: LOGO.COD
  },
  {
    value: 'vnpay',
    label: 'VNPay',
    desc: 'Thanh toán qua cổng VNPay (ATM / Internet Banking / QR)',
    Icon: CreditCardRoundedIcon,
    color: '#0d47a1',
    logoSrc: LOGO.VNPAY
  },
  {
    value: 'momo',
    label: 'Ví MoMo',
    desc: 'Thanh toán qua ví điện tử MoMo (quét QR hoặc deeplink)',
    Icon: AccountBalanceWalletRoundedIcon,
    color: '#ae2d68',
    logoSrc: LOGO.MOMO
  }
]

function OrderPaymentMethod({ control }) {
  return (
    <Box sx={{
      bgcolor: 'primary.main', ...glassSx,
      borderRadius: 3, p: 2.5,
      border: '1px solid', borderColor: 'divider'
    }}>
      {/* Section header */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.75 }}>
        <Box sx={{ width: 3.5, height: 22, borderRadius: '2px', bgcolor: 'secondary.main', flexShrink: 0 }} />
        <PaymentsRoundedIcon sx={{ fontSize: 17, color: 'secondary.main' }} />
        <Typography sx={{ fontWeight: 700, fontSize: '0.93rem', ...gradientText }}>
          Phương thức thanh toán
        </Typography>
      </Box>

      <Controller
        name="paymentMethod"
        control={control}
        defaultValue="cod"
        render={({ field }) => (
          <RadioGroup {...field} sx={{ gap: 1 }}>
            {PAYMENT_METHODS.map(({ value, label, desc, Icon, color, logoSrc }) => {
              const selected = field.value === value
              return (
                <Box
                  key={value}
                  onClick={() => field.onChange(value)}
                  sx={{
                    display: 'flex', alignItems: 'center', gap: 1.5,
                    px: 1.75, py: 1.25, borderRadius: 2, cursor: 'pointer',
                    border: '1px solid',
                    // Đổi màu viền và nền dựa trên màu đặc trưng (color)
                    borderColor: selected ? color : 'divider',
                    bgcolor: selected ? `${color}1A` : 'transparent', // 1A tương đương ~10% opacity
                    transition: 'border-color 0.18s, background 0.18s',
                    '&:hover': {
                      bgcolor: selected ? `${color}1A` : `${color}0D`, // 0D tương đương ~5% opacity
                      borderColor: selected ? color : `${color}66`, // 66 tương đương ~40% opacity
                      // Thêm hiệu ứng cho icon bên trong khi hover
                      '& .payment-icon': {
                        color: color
                      }
                    }
                  }}
                >
                  {/* Custom radio circle */}
                  <Box sx={{
                    width: 18, height: 18, borderRadius: '50%', flexShrink: 0,
                    border: '2px solid',
                    // Dấu tick chọn cũng đổi theo màu brand cho đồng bộ
                    borderColor: selected ? color : 'rgba(0,0,0,0.28)',
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                    transition: 'border-color 0.18s'
                  }}>
                    {selected && (
                      <Box sx={{
                        width: 9, height: 9, borderRadius: '50%',
                        bgcolor: color
                      }} />
                    )}
                  </Box>

                  {/* Logo / Icon */}
                  <Box sx={{
                    width: 34, height: 34, borderRadius: 1.5, flexShrink: 0,
                    bgcolor: `${color}12`,
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                    overflow: 'hidden' // Giúp logo bo góc gọn gàng nếu viền ảnh bị vuông
                  }}>
                    {logoSrc ? (
                      <Box
                        component="img"
                        src={logoSrc}
                        alt={label}
                        sx={{ width: '100%', height: '100%', objectFit: 'contain' }}
                      />
                    ) : (
                      <Icon
                        className="payment-icon"
                        sx={{
                          fontSize: 19,
                          color: selected ? color : 'rgba(45,45,45,0.45)',
                          transition: 'color 0.18s'
                        }}
                      />
                    )}
                  </Box>

                  {/* Label + desc */}
                  <Box sx={{ flex: 1, minWidth: 0 }}>
                    <Typography sx={{
                      fontSize: '0.83rem', fontWeight: selected ? 700 : 600,
                      color: selected ? 'primary.contrastText' : 'rgba(45,45,45,0.75)',
                      lineHeight: 1.3
                    }}>
                      {label}
                    </Typography>
                    <Typography sx={{ fontSize: '0.71rem', color: 'rgba(45,45,45,0.48)', mt: 0.25, lineHeight: 1.4 }}>
                      {desc}
                    </Typography>
                  </Box>
                </Box>
              )
            })}
          </RadioGroup>
        )}
      />
    </Box>
  )
}

export default OrderPaymentMethod