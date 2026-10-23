import { useState } from 'react'
import {
  Box,
  Card,
  Typography,
  Chip,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Stack,
  TextField,
  InputAdornment,
  Avatar,
  MenuItem
} from '@mui/material'
import PeopleAltRoundedIcon from '@mui/icons-material/PeopleAltRounded'
import AdminPanelSettingsRoundedIcon from '@mui/icons-material/AdminPanelSettingsRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import BlockRoundedIcon from '@mui/icons-material/BlockRounded'
import LockOpenRoundedIcon from '@mui/icons-material/LockOpenRounded'
import SecurityRoundedIcon from '@mui/icons-material/SecurityRounded'
import { toast } from 'react-toastify'
import { adminBtnStyles } from '../../theme/adminButtonStyles'

const INITIAL_USERS = [
  {
    id: 'u-1',
    name: 'Rei Admin',
    email: 'admin@novalive.online',
    phone: '0988776655',
    role: 'Admin',
    status: 'Active',
    createdAt: '01/01/2026',
    lastLogin: '23/10/2026 21:05',
    avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=100&auto=format&fit=crop&q=80'
  },
  {
    id: 'u-2',
    name: 'Nguyễn Văn Mod',
    email: 'mod.nguyen@novalive.online',
    phone: '0912345679',
    role: 'Moderator',
    status: 'Active',
    createdAt: '15/02/2026',
    lastLogin: '23/10/2026 19:40',
    avatar: 'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=100&auto=format&fit=crop&q=80'
  },
  {
    id: 'u-3',
    name: 'Trần Thị Mai (Glow & Care)',
    email: 'mai.tran@glowcare.vn',
    phone: '0987654321',
    role: 'Seller',
    status: 'Active',
    createdAt: '10/05/2026',
    lastLogin: '23/10/2026 20:15',
    avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=100&auto=format&fit=crop&q=80'
  },
  {
    id: 'u-4',
    name: 'testuser@novalive.online',
    email: 'testuser@novalive.online',
    phone: '0912345678',
    role: 'Buyer',
    status: 'Active',
    createdAt: '23/10/2026',
    lastLogin: '23/10/2026 19:15',
    avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=100&auto=format&fit=crop&q=80'
  },
  {
    id: 'u-5',
    name: 'Hoàng Văn B (Gian lận)',
    email: 'fake.warning@gmail.com',
    phone: '0977889900',
    role: 'Seller',
    status: 'Suspended',
    createdAt: '12/03/2026',
    lastLogin: '15/10/2026 10:00',
    avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100&auto=format&fit=crop&q=80'
  }
]

export const UsersManagementTab = () => {
  const [users, setUsers] = useState(INITIAL_USERS)
  const [searchTerm, setSearchTerm] = useState('')
  const [roleFilter, setRoleFilter] = useState('ALL')
  const [editingRoleUser, setEditingRoleUser] = useState(null)
  const [selectedRole, setSelectedRole] = useState('Buyer')

  const handleOpenRoleModal = (u) => {
    setEditingRoleUser(u)
    setSelectedRole(u.role)
  }

  const handleSaveRole = () => {
    if (!editingRoleUser) return
    setUsers(prev => prev.map(u => u.id === editingRoleUser.id ? { ...u, role: selectedRole } : u))
    toast.success(`Đã cập nhật vai trò RBAC cho ${editingRoleUser.name} thành [${selectedRole}]`)
    setEditingRoleUser(null)
  }

  const handleToggleBan = (u) => {
    const nextStatus = u.status === 'Active' ? 'Suspended' : 'Active'
    setUsers(prev => prev.map(item => item.id === u.id ? { ...item, status: nextStatus } : item))
    if (nextStatus === 'Suspended') {
      toast.error(`Đã khóa tài khoản người dùng: ${u.name}`)
    } else {
      toast.success(`Đã mở khóa tài khoản cho: ${u.name}`)
    }
  }

  const filteredUsers = users.filter(u => {
    if (roleFilter !== 'ALL' && u.role !== roleFilter) return false
    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase()
      return u.name.toLowerCase().includes(q) || u.email.toLowerCase().includes(q) || u.phone.includes(q)
    }
    return true
  })

  return (
    <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)' }}>
      {/* Header & Filter */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2.5, flexWrap: 'wrap', gap: 1.5 }}>
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <PeopleAltRoundedIcon sx={{ color: '#0095ff', fontSize: 24 }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
              Quản Trị Người Dùng & Phân Quyền (RBAC Management)
            </Typography>
          </Box>
          <Typography variant="caption" sx={{ color: '#64748b' }}>
            Quản lý tài khoản toàn hệ thống, gán quyền Admin / Moderator / Seller và xử lý khóa vi phạm
          </Typography>
        </Box>
        <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap sx={{ alignItems: 'center' }}>
          {[
            { key: 'ALL', label: `Tất cả (${users.length})` },
            { key: 'Admin', label: 'Admin' },
            { key: 'Moderator', label: 'Moderator' },
            { key: 'Seller', label: 'Seller' },
            { key: 'Buyer', label: 'Buyer' }
          ].map(f => {
            const isActive = roleFilter === f.key
            return (
              <Chip
                key={f.key}
                label={f.label}
                onClick={() => setRoleFilter(f.key)}
                sx={{
                  fontWeight: 700,
                  fontSize: '0.78rem',
                  whiteSpace: 'nowrap',
                  bgcolor: isActive ? '#0095ff' : '#f1f5f9',
                  color: isActive ? '#ffffff' : '#475569',
                  '&:hover': {
                    bgcolor: isActive ? '#0080db' : '#e2e8f0'
                  }
                }}
              />
            )
          })}
        </Stack>
      </Box>

      {/* Search Bar */}
      <Box sx={{ mb: 2 }}>
        <TextField
          size="small"
          placeholder="Tìm kiếm người dùng theo tên, email, số điện thoại..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          sx={{
            width: { xs: '100%', sm: 380 },
            '& .MuiOutlinedInput-root': {
              borderRadius: '10px'
            }
          }}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchRoundedIcon sx={{ color: '#94a3b8', fontSize: 20 }} />
              </InputAdornment>
            )
          }}
        />
      </Box>

      <TableContainer sx={{ overflowX: 'auto' }}>
        <Table size="small">
          <TableHead sx={{ bgcolor: '#f8fafc' }}>
            <TableRow>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Người Dùng</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Số Điện Thoại</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Vai Trò (Role)</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Trạng Thái</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Ngày Đăng Ký</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Lần Cuối Đăng Nhập</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Hành Động</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredUsers.map((u) => {
              const roleStyles = {
                Admin: { bgcolor: '#fef2f2', color: '#ef4444', border: '1px solid #fecaca' },
                Moderator: { bgcolor: '#fffbeb', color: '#d97706', border: '1px solid #fde68a' },
                Seller: { bgcolor: '#f5f3ff', color: '#8b5cf6', border: '1px solid #ddd6fe' },
                Buyer: { bgcolor: '#f0fdf4', color: '#16a34a', border: '1px solid #bbf7d0' }
              }[u.role] || { bgcolor: '#f1f5f9', color: '#64748b', border: '1px solid #e2e8f0' }

              return (
                <TableRow key={u.id} hover>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                      <Avatar src={u.avatar} sx={{ width: 36, height: 36, border: '1px solid #e2e8f0' }} />
                      <Box>
                        <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{u.name}</Typography>
                        <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap', display: 'block' }}>{u.email}</Typography>
                      </Box>
                    </Box>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ color: '#334155', fontWeight: 500, whiteSpace: 'nowrap' }}>{u.phone}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Chip
                      label={u.role}
                      size="small"
                      sx={{
                        fontWeight: 700,
                        height: 24,
                        fontSize: '0.72rem',
                        whiteSpace: 'nowrap',
                        ...roleStyles
                      }}
                    />
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Chip
                      label={u.status === 'Active' ? 'Hoạt Động' : 'Bị Khóa'}
                      size="small"
                      sx={{
                        fontWeight: 700,
                        height: 24,
                        fontSize: '0.72rem',
                        whiteSpace: 'nowrap',
                        bgcolor: u.status === 'Active' ? '#f0fdf4' : '#fef2f2',
                        color: u.status === 'Active' ? '#16a34a' : '#dc2626',
                        border: u.status === 'Active' ? '1px solid #bbf7d0' : '1px solid #fecaca'
                      }}
                    />
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="body2" sx={{ color: '#64748b', fontSize: '0.8rem', whiteSpace: 'nowrap' }}>{u.createdAt}</Typography>
                  </TableCell>
                  <TableCell sx={{ whiteSpace: 'nowrap' }}>
                    <Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>{u.lastLogin}</Typography>
                  </TableCell>
                  <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                    <Stack direction="row" spacing={1} justifyContent="flex-end">
                      <Button
                        size="small"
                        variant="outlined"
                        startIcon={<SecurityRoundedIcon sx={{ fontSize: '16px !important' }} />}
                        onClick={() => handleOpenRoleModal(u)}
                        sx={{
                          ...adminBtnStyles.transSecondary,
                          textTransform: 'none',
                          whiteSpace: 'nowrap'
                        }}
                      >
                        Phân Quyền
                      </Button>
                      <Button
                        size="small"
                        variant={u.status === 'Active' ? 'outlined' : 'contained'}
                        startIcon={u.status === 'Active' ? <BlockRoundedIcon sx={{ fontSize: '16px !important' }} /> : <LockOpenRoundedIcon sx={{ fontSize: '16px !important' }} />}
                        onClick={() => handleToggleBan(u)}
                        sx={{
                          ...(u.status === 'Active' ? adminBtnStyles.transDanger : adminBtnStyles.success),
                          textTransform: 'none',
                          whiteSpace: 'nowrap'
                        }}
                      >
                        {u.status === 'Active' ? 'Khóa' : 'Mở'}
                      </Button>
                    </Stack>
                  </TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      </TableContainer>

      {/* MODAL: GÁN ROLE RBAC */}
      <Dialog
        open={Boolean(editingRoleUser)}
        onClose={() => setEditingRoleUser(null)}
        maxWidth="xs"
        fullWidth
        PaperProps={{ sx: { borderRadius: '16px', p: 1 } }}
      >
        <DialogTitle sx={{ fontWeight: 800, display: 'flex', alignItems: 'center', gap: 1 }}>
          <AdminPanelSettingsRoundedIcon sx={{ color: '#0095ff' }} />
          <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
            Gán Quyền RBAC: {editingRoleUser?.name}
          </Typography>
        </DialogTitle>
        <DialogContent dividers>
          <Box sx={{ pt: 1 }}>
            <Typography variant="body2" sx={{ color: '#64748b', mb: 2 }}>
              Chọn vai trò phân quyền cho tài khoản <strong>{editingRoleUser?.email}</strong>:
            </Typography>
            <TextField
              select
              fullWidth
              size="small"
              label="Vai trò (Role)"
              value={selectedRole}
              onChange={(e) => setSelectedRole(e.target.value)}
              sx={{
                '& .MuiOutlinedInput-root': {
                  borderRadius: '10px'
                }
              }}
            >
              <MenuItem value="Buyer">Buyer (Người mua hàng phổ thông)</MenuItem>
              <MenuItem value="Seller">Seller (Chủ gian hàng bán hàng & Livestream)</MenuItem>
              <MenuItem value="Moderator">Moderator (Kiểm duyệt viên nội dung, live & tranh chấp)</MenuItem>
              <MenuItem value="Admin">Platform Administrator (Toàn quyền quản trị sàn)</MenuItem>
            </TextField>
          </Box>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setEditingRoleUser(null)} sx={{ textTransform: 'none', color: '#64748b', fontWeight: 600 }}>Hủy</Button>
          <Button
            variant="contained"
            onClick={handleSaveRole}
            sx={{
              ...adminBtnStyles.primary,
              textTransform: 'none',
              whiteSpace: 'nowrap'
            }}
          >
            Lưu Quyền Hạn
          </Button>
        </DialogActions>
      </Dialog>
    </Card>
  )
}

export default UsersManagementTab
