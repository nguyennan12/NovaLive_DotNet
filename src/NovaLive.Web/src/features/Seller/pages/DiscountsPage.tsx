import AddRounded from '@mui/icons-material/AddRounded'
import CalendarMonthOutlined from '@mui/icons-material/CalendarMonthOutlined'
import ContentCopyRounded from '@mui/icons-material/ContentCopyRounded'
import LocalOfferOutlined from '@mui/icons-material/LocalOfferOutlined'
import SearchRounded from '@mui/icons-material/SearchRounded'
import SellOutlined from '@mui/icons-material/SellOutlined'
import TaskAltRounded from '@mui/icons-material/TaskAltRounded'
import { Box, Button, Chip, IconButton, InputAdornment, LinearProgress, MenuItem, Stack, Switch, TextField, Tooltip, Typography } from '@mui/material'
import { useMemo, useState, type ReactNode } from 'react'
import { toast } from 'react-toastify'
import type { Discount } from '../types/contracts'
import { useDiscounts, useUpdateDiscount } from '../hooks/useMarketing'
import { PageHeader } from '../components/shared/PageHeader'
import { SellerCard } from '../components/shared/SellerCard'
import { DateText } from '../components/shared/DateText'
import { MoneyText } from '../components/shared/MoneyText'
import { StatusBadge } from '../components/shared/StatusBadge'
import { QueryState } from '../components/shared/QueryState'
import { DiscountDialog } from '../components/marketing/DiscountDialog'
import { discountStatus } from '../utils/marketing'
import { PAGE_SIZE } from '../constants'

function StatCard({ icon, label, value, color }: { icon: ReactNode; label: string; value: number; color: string }) {
  return <SellerCard><Stack direction='row' spacing={1.5} sx={{ alignItems: 'center' }}><Box sx={{ width: 48, height: 48, borderRadius: 2.5, display: 'grid', placeItems: 'center', bgcolor: `${color}14`, color }}>{icon}</Box><Box><Typography color='text.secondary' variant='body2'>{label}</Typography><Typography variant='h5' sx={{ fontWeight: 800 }}>{value.toLocaleString('vi-VN')}</Typography></Box></Stack></SellerCard>
}

function VoucherCard({ item, pending, onEdit, onToggle }: { item: Discount; pending: boolean; onEdit: () => void; onToggle: (checked: boolean) => void }) {
  const status = discountStatus(item)
  const usedPercent = item.maxUses ? Math.min(100, item.usedCount / item.maxUses * 100) : 0
  return <SellerCard>
    <Stack spacing={1.5}>
      <Stack direction='row' spacing={1} sx={{ justifyContent: 'space-between', alignItems: 'flex-start' }}>
        <Box><Chip size='small' icon={<SellOutlined />} label={item.discountType === 'PercentCart' ? 'Giảm phần trăm' : 'Giảm trực tiếp'} color={item.discountType === 'PercentCart' ? 'primary' : 'warning'} variant='outlined' /><Typography sx={{ fontWeight: 800, mt: 1, fontSize: '1.05rem' }}>{item.name}</Typography></Box>
        <StatusBadge status={status} />
      </Stack>
      <Stack direction='row' spacing={0.5} sx={{ alignItems: 'center' }}><Typography sx={{ fontFamily: 'monospace', fontWeight: 800, letterSpacing: 1, bgcolor: 'action.hover', px: 1, py: 0.5, borderRadius: 1 }}>{item.code}</Typography><Tooltip title='Sao chép mã'><IconButton size='small' onClick={() => { void navigator.clipboard.writeText(item.code); toast.success('Đã sao chép mã voucher.') }}><ContentCopyRounded fontSize='small' /></IconButton></Tooltip></Stack>
      <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1 }}><Box><Typography variant='caption' color='text.secondary'>Giá trị</Typography><Typography sx={{ fontWeight: 800 }}>{item.discountType === 'PercentCart' ? `${item.discountValue}%` : <MoneyText value={item.discountValue} />}</Typography></Box><Box><Typography variant='caption' color='text.secondary'>Đơn tối thiểu</Typography><Typography sx={{ fontWeight: 700 }}><MoneyText value={item.minOrderAmount} /></Typography></Box></Box>
      <Stack direction='row' spacing={1} sx={{ alignItems: 'center' }}><CalendarMonthOutlined fontSize='small' color='action' /><Typography variant='body2'><DateText value={item.validFrom} /> — <DateText value={item.validTo} /></Typography></Stack>
      <Box><Stack direction='row' sx={{ justifyContent: 'space-between' }}><Typography variant='caption' color='text.secondary'>Đã sử dụng</Typography><Typography variant='caption' sx={{ fontWeight: 700 }}>{item.usedCount}/{item.maxUses ?? '∞'}</Typography></Stack><LinearProgress variant='determinate' value={usedPercent} sx={{ mt: 0.75, height: 6, borderRadius: 99 }} /></Box>
      <Stack direction='row' sx={{ justifyContent: 'space-between', alignItems: 'center' }}><Stack direction='row' sx={{ alignItems: 'center' }}><Switch size='small' checked={item.isActive} disabled={pending} onChange={(_, checked) => onToggle(checked)} /><Typography variant='body2'>{item.isActive ? 'Đang bật' : 'Đã tắt'}</Typography></Stack><Button size='small' onClick={onEdit}>Chỉnh sửa</Button></Stack>
    </Stack>
  </SellerCard>
}

export default function DiscountsPage() {
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState('')
  const [kind, setKind] = useState('')
  const [keyword, setKeyword] = useState('')
  const [editor, setEditor] = useState<{ initial: Discount | null } | null>(null)
  const query = useDiscounts({ page, size: PAGE_SIZE, status: status || undefined })
  const overview = useDiscounts({ page: 1, size: 100 })
  const update = useUpdateDiscount()
  const all = useMemo(() => overview.data?.items || [], [overview.data?.items])
  const stats = useMemo(() => all.reduce((acc, item) => { const state = discountStatus(item); acc.total += 1; if (state === 'Active') acc.active += 1; if (state === 'Scheduled') acc.scheduled += 1; if (state === 'Expired') acc.expired += 1; return acc }, { total: 0, active: 0, scheduled: 0, expired: 0 }), [all])
  const visible = useMemo(() => (query.data?.items || []).filter(item => (!kind || item.discountType === kind) && (!keyword.trim() || `${item.code} ${item.name}`.toLowerCase().includes(keyword.trim().toLowerCase()))), [query.data?.items, kind, keyword])

  return <>
    <PageHeader title='Quản lý ưu đãi' description='Tạo và theo dõi voucher dành riêng cho khách hàng của Shop.' action={<Button variant='contained' color='secondary' startIcon={<AddRounded />} onClick={() => setEditor({ initial: null })}>Tạo voucher</Button>} />

    <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', xl: 'repeat(4, 1fr)' }, gap: 2, mb: 2.5 }}>
      <StatCard icon={<LocalOfferOutlined />} label='Tổng voucher' value={overview.data?.total ?? stats.total} color='#568dfb' />
      <StatCard icon={<TaskAltRounded />} label='Đang hoạt động' value={stats.active} color='#22a06b' />
      <StatCard icon={<CalendarMonthOutlined />} label='Sắp diễn ra' value={stats.scheduled} color='#8067dc' />
      <StatCard icon={<SellOutlined />} label='Đã hết hạn' value={stats.expired} color='#df5267' />
    </Box>

    <SellerCard title='Danh sách voucher' action={<Typography color='text.secondary' variant='body2'>{query.data?.total || 0} voucher</Typography>}>
      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'minmax(260px, 1fr) 190px 190px' }, gap: 1.5, mb: 2.5 }}>
        <TextField size='small' placeholder='Tìm theo tên hoặc mã voucher...' value={keyword} onChange={event => setKeyword(event.target.value)} slotProps={{ input: { startAdornment: <InputAdornment position='start'><SearchRounded fontSize='small' /></InputAdornment> } }} />
        <TextField select size='small' label='Hiệu lực' value={status} onChange={event => { setStatus(event.target.value); setPage(1) }}><MenuItem value=''>Tất cả trạng thái</MenuItem><MenuItem value='Active'>Đang hoạt động</MenuItem><MenuItem value='Scheduled'>Sắp diễn ra</MenuItem><MenuItem value='Expired'>Đã hết hạn</MenuItem><MenuItem value='Inactive'>Đã tắt</MenuItem></TextField>
        <TextField select size='small' label='Hình thức giảm' value={kind} onChange={event => setKind(event.target.value)}><MenuItem value=''>Tất cả hình thức</MenuItem><MenuItem value='PercentCart'>Giảm phần trăm</MenuItem><MenuItem value='FixedCart'>Giảm trực tiếp</MenuItem></TextField>
      </Box>

      <QueryState loading={query.isLoading} error={query.error} empty={!query.isLoading && visible.length === 0} onRetry={query.refetch} emptyText='Không có voucher phù hợp.' emptyAction={<Button onClick={() => setEditor({ initial: null })}>Tạo voucher đầu tiên</Button>}>
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', lg: 'repeat(2, minmax(0, 1fr))' }, gap: 2 }}>
          {visible.map(item => <VoucherCard key={item.id} item={item} pending={update.isPending} onEdit={() => setEditor({ initial: item })} onToggle={checked => update.mutate({ id: item.id, data: { name: item.name, maxUses: item.maxUses, validTo: item.validTo, isActive: checked } })} />)}
        </Box>
      </QueryState>
      {query.data && query.data.totalPages > 1 && <Stack direction='row' spacing={1} sx={{ mt: 2.5, justifyContent: 'center' }}><Button disabled={page <= 1} onClick={() => setPage(value => value - 1)}>Trang trước</Button><Chip label={`Trang ${page}/${query.data.totalPages}`} /><Button disabled={page >= query.data.totalPages} onClick={() => setPage(value => value + 1)}>Trang sau</Button></Stack>}
    </SellerCard>

    {editor && <DiscountDialog initial={editor.initial} onClose={() => setEditor(null)} />}
  </>
}
