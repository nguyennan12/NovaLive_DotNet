import { Box, Button, TextField } from '@mui/material'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useCustomers } from '../hooks/useOrders'
import { useDebouncedValue } from '../hooks/useSellerQuery'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { DataTable } from '../components/shared/DataTable'
import { DateText } from '../components/shared/DateText'
import { MoneyText } from '../components/shared/MoneyText'
import { PAGE_SIZE } from '../constants'

export default function CustomersPage() {
  const query = useCustomers(); const [keyword, setKeyword] = useState(''); const [page, setPage] = useState(1)
  const search = useDebouncedValue(keyword).toLowerCase()
  const rows = (query.data || []).filter(item => `${item.name} ${item.phone}`.toLowerCase().includes(search))
  return <><PageHeader title='Khách hàng' description='Tổng hợp người nhận từ đơn hàng thuộc Shop. Giá trị đơn chưa bao gồm hủy và chờ thanh toán; đây không phải doanh thu quyết toán.' /><Box sx={{ mb: 2 }}><TextField size='small' label='Tìm tên / Số điện thoại' value={keyword} onChange={event => { setKeyword(event.target.value); setPage(1) }} /></Box><SellerCard><DataTable rows={rows.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)} rowKey={row => row.id} loading={query.isLoading} error={query.error} onRetry={query.refetch} page={page} totalPages={Math.ceil(rows.length / PAGE_SIZE)} total={rows.length} onPage={setPage} emptyAction={<Button component={Link} to='/seller/orders'>Xem đơn hàng</Button>} columns={[
    { key: 'name', label: 'Người nhận', render: row => row.name }, { key: 'phone', label: 'Điện thoại', render: row => row.phone }, { key: 'count', label: 'Số đơn', render: row => row.orderCount }, { key: 'spent', label: 'Giá trị đơn hợp lệ', render: row => <MoneyText value={row.totalSpent} /> }, { key: 'last', label: 'Đơn gần nhất', render: row => <DateText value={row.lastOrderAt} /> }, { key: 'action', label: 'Thao tác', render: row => <Button component={Link} to={`/seller/orders/${row.lastOrderId}`}>Xem đơn gần nhất</Button> }
  ]} /></SellerCard></>
}
