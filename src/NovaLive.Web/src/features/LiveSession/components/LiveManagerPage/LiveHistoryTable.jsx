import KeyboardArrowDownRoundedIcon from '@mui/icons-material/KeyboardArrowDownRounded'
import KeyboardArrowUpRoundedIcon from '@mui/icons-material/KeyboardArrowUpRounded'
import PeopleOutlineRoundedIcon from '@mui/icons-material/PeopleOutlineRounded'
import ShoppingBagOutlinedIcon from '@mui/icons-material/ShoppingBagOutlined'
import { Box, Collapse, IconButton, MenuItem, Pagination, Select, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Typography } from '@mui/material'
import { useState } from 'react'
import { formatDate, formatLiveDurationClock, formatVND } from '~/common/utils/formatters'
import SectionCard from '~/features/Inventory/components/shared/SectionCard'
import { useSessionHistory } from '../../hooks/useLiveSessions'
import LiveStatusBadge from '../shared/LiveStatusBadge'
import { LIMIT } from '~/common/utils/constant'

const ExpandedDetail = ({ session }) => {
  return (
    <TableRow>
      <TableCell colSpan={7} sx={{ p: 0, border: 0 }}>
        <Collapse in unmountOnExit>
          <Box
            sx={{
              px: 3,
              py: 2,
              bgcolor: '#f8faff',
              borderBottom: '1px solid #eeeeee',
              display: 'flex',
              gap: 4,
              flexWrap: 'wrap',
              alignItems: 'flex-start'
            }}
          >
            {session.description && (
              <Box sx={{ flex: 1, minWidth: 160 }}>
                <Typography sx={{ fontSize: '0.72rem', color: '#94a3b8', fontWeight: 700, mb: 0.5, textTransform: 'uppercase', letterSpacing: '0.04em' }}>
                  Mô tả
                </Typography>
                <Typography sx={{ fontSize: '0.82rem', color: '#64748b', lineHeight: 1.5 }}>
                  {session.live_description}
                </Typography>
              </Box>
            )}
            <Box sx={{ display: 'flex', gap: 3 }}>
              <Box sx={{ textAlign: 'center' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, justifyContent: 'center' }}>
                  <PeopleOutlineRoundedIcon sx={{ fontSize: 16, color: '#8b5cf6' }} />
                  <Typography sx={{ fontSize: '1.1rem', fontWeight: 800, color: '#8b5cf6' }}>
                    {session?.live_metrics?.peak_viewers}
                  </Typography>
                </Box>
                <Typography sx={{ fontSize: '0.7rem', color: '#94a3b8' }}>Lượt xem</Typography>
              </Box>
              <Box sx={{ textAlign: 'center' }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, justifyContent: 'center' }}>
                  <ShoppingBagOutlinedIcon sx={{ fontSize: 16, color: '#f59e0b' }} />
                  <Typography sx={{ fontSize: '1.1rem', fontWeight: 800, color: '#f59e0b' }}>
                    {session?.live_metrics?.total_orders}
                  </Typography>
                </Box>
                <Typography sx={{ fontSize: '0.7rem', color: '#94a3b8' }}>Đơn hàng</Typography>
              </Box>
            </Box>
          </Box>
        </Collapse>
      </TableCell>
    </TableRow>
  )
}

const SessionRow = ({ session }) => {
  const [open, setOpen] = useState(false)
  return (
    <>
      <TableRow
        sx={{
          '&:hover': { bgcolor: '#f8faff' },
          transition: 'background 0.15s',
          cursor: 'pointer'
        }}
      >
        <TableCell sx={{ width: 40, p: '4px 8px' }}>
          <IconButton size='small' onClick={() => setOpen((o) => !o)} sx={{ p: 0.5 }}>
            {open
              ? <KeyboardArrowUpRoundedIcon sx={{ fontSize: 18 }} />
              : <KeyboardArrowDownRoundedIcon sx={{ fontSize: 18 }} />}
          </IconButton>
        </TableCell>
        <TableCell>
          <Box>
            <Typography sx={{
              fontWeight: 600, color: 'secondary.main', fontSize: '0.82rem', maxWidth: 180, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap'
            }}>{session.live_title}</Typography>
            <Typography sx={{
              fontWeight: 500, color: '#a5a5a5ff', fontSize: '0.7rem', maxWidth: 180, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap'
            }}>{session.live_code}</Typography>

          </Box>

        </TableCell>
        <TableCell sx={{ fontSize: '0.8rem', color: '#64748b', whiteSpace: 'nowrap' }}>
          {formatDate(session.live_schedule_at)}
        </TableCell>
        <TableCell sx={{ fontSize: '0.8rem', color: '#64748b' }}>
          {formatLiveDurationClock(session.live_actual_start, session.live_actual_end)}
        </TableCell>
        <TableCell sx={{ fontSize: '0.8rem', color: '#64748b', textAlign: 'center' }}>
          {session?.live_metrics?.peak_viewers || 0}
        </TableCell>
        <TableCell sx={{ fontWeight: 700, color: '#3485f7', fontSize: '0.82rem', whiteSpace: 'nowrap' }}>
          {session?.live_metrics?.total_revenue ? formatVND(session?.live_metrics?.total_revenue) : '—'}
        </TableCell>
        <TableCell>
          <LiveStatusBadge status={session.live_status} />
        </TableCell>
      </TableRow >
      {open && <ExpandedDetail session={session} />
      }
    </>
  )
}

const HEADER_CELLS = [
  { label: '' },
  { label: 'Tiêu đề' },
  { label: 'Ngày' },
  { label: 'Thời lượng' },
  { label: 'Lượt xem', align: 'center' },
  { label: 'Doanh thu' },
  { label: 'Trạng thái' }
]

const LiveHistoryTable = () => {
  const [page, setPage] = useState(1)
  const [status, setStatus] = useState('all')

  const handlePageChange = (event, value) => {
    setPage(value)
  }
  const params = { page, limt: LIMIT.INVENTORY, status }

  const queryString = new URLSearchParams(params).toString()
  const { sessions, totalPages } = useSessionHistory(queryString)

  return (
    <SectionCard title='Lịch Sử Phiên Live' subtitle='Danh sách các buổi live đã diễn ra'
      action={
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Select
            value={status}
            onChange={(e) => setStatus(e.target.value)}
            size='small'
            sx={{
              fontSize: '0.78rem', height: 34, borderRadius: '8px', minWidth: 110,
              '& fieldset': { borderColor: '#eeeeee' }
            }}
          >
            <MenuItem value='all' sx={{ fontSize: '0.78rem' }}>All status</MenuItem>
            <MenuItem value='scheduled' sx={{ fontSize: '0.78rem' }}>Schedule</MenuItem>
            <MenuItem value='live' sx={{ fontSize: '0.78rem' }}>Live</MenuItem>
            <MenuItem value='ended' sx={{ fontSize: '0.78rem' }}>End</MenuItem>
            <MenuItem value='canceled' sx={{ fontSize: '0.78rem' }}>Cancel</MenuItem>
          </Select>
        </Box>}
    >
      <TableContainer>
        <Table size='small'>
          <TableHead>
            <TableRow
              sx={{
                '& th': {
                  fontSize: '0.72rem',
                  fontWeight: 700,
                  color: '#94a3b8',
                  textTransform: 'uppercase',
                  letterSpacing: '0.05em',
                  borderBottom: '1px solid #eeeeee',
                  py: 1.5,
                  whiteSpace: 'nowrap'
                }
              }}
            >
              {HEADER_CELLS.map((h, i) => (
                <TableCell key={i} align={h.align}>{h.label}</TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {sessions.length === 0
              ? (
                <TableRow>
                  <TableCell
                    colSpan={7}
                    sx={{ textAlign: 'center', py: 5, color: '#9ca3af', fontSize: '0.85rem' }}
                  >
                    Chưa có phiên live nào trong lịch sử
                  </TableCell>
                </TableRow>
              )
              : sessions.map((s) => <SessionRow key={s._id} session={s} />)
            }
          </TableBody>
        </Table>
      </TableContainer>

      <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3, mb: 2 }}>
        <Pagination
          count={totalPages}
          page={page}
          onChange={handlePageChange}
          color="primary"
        />
      </Box>

    </SectionCard >
  )
}

export default LiveHistoryTable
