import { Button, Grid, Typography } from '@mui/material'
import { Link } from 'react-router-dom'
import { useCampaigns, useDiscounts } from '../hooks/useMarketing'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { QueryState } from '../components/shared/QueryState'

export default function MarketingPage() {
  const discounts = useDiscounts({ page: 1, size: 1 }); const campaigns = useCampaigns()
  return <><PageHeader title='Tiếp thị cửa hàng' description='Quản lý ưu đãi của Shop và các chiến dịch Flash Sale.' /><QueryState loading={discounts.isLoading || campaigns.isLoading} error={discounts.error || campaigns.error} onRetry={() => Promise.all([discounts.refetch(), campaigns.refetch()])}>
    <Grid container spacing={2.5}><Grid size={{ xs: 12, md: 6 }}><SellerCard title='Voucher của Shop'><Typography sx={{ mb: 2 }}>{discounts.data?.total || 0} voucher đã tạo. Tạo mã ưu đãi, giới hạn lượt dùng và bật/tắt theo nhu cầu.</Typography><Button component={Link} to='/seller/discounts' variant='contained' color='secondary'>Quản lý voucher</Button></SellerCard></Grid><Grid size={{ xs: 12, md: 6 }}><SellerCard title='Flash Sale'><Typography sx={{ mb: 2 }}>{campaigns.data?.length || 0} chiến dịch được công bố. Đăng ký SKU với giá giảm từ 10% và theo dõi tiến độ bán.</Typography><Button component={Link} to='/seller/flash-sales' variant='outlined' color='secondary'>Xem & Đăng ký chiến dịch</Button></SellerCard></Grid></Grid>
  </QueryState></>
}
