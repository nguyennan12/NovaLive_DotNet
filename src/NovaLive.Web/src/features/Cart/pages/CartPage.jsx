import { Container, Grid } from '@mui/material'
import CartList from '../components/CartList'
import CartSummary from '../components/CartSummary'

function CartPage() {
  return (
    <Container
      maxWidth="lg"
      sx={{ pt: { xs: 2.5, md: 4 }, pb: { xs: '110px', md: '100px' } }}
    >

      <Grid container columns={10} spacing={{ xs: 2, md: 3 }}>
        <Grid size={{ xs: 10, md: 7 }}>
          <CartList />
        </Grid>

        <Grid size={{ xs: 10, md: 3 }}>
          <CartSummary />
        </Grid>
      </Grid>
    </Container>
  )
}

export default CartPage
