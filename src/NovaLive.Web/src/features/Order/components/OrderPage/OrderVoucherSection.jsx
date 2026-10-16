import { Box } from '@mui/material'
import GlobalDiscountSection from '~/features/Discount/components/DIscountSelect/GlobalDiscountSection'
import { useApplyDiscounts } from '~/features/Discount/hook/useApplyDiscounts'


function OrderVoucherSection({ subtotal = 0 }) {
  const {
    appliedProductVoucher, setProductVoucher, clearProductVoucher,
    appliedFreeshipVoucher, setFreeshipVoucher, clearFreeshipVoucher
  } = useApplyDiscounts()

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.25 }}>

      <GlobalDiscountSection
        appliedProductVoucher={appliedProductVoucher}
        setProductVoucher={setProductVoucher}
        clearProductVoucher={clearProductVoucher}
        appliedFreeshipVoucher={appliedFreeshipVoucher}
        setFreeshipVoucher={setFreeshipVoucher}
        clearFreeshipVoucher={clearFreeshipVoucher}
        subtotal={subtotal}
      />
    </Box>
  )
}

export default OrderVoucherSection
