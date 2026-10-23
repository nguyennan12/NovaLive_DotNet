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
  Switch,
  FormControlLabel,
  MenuItem
} from '@mui/material'
import CategoryRoundedIcon from '@mui/icons-material/CategoryRounded'
import AddRoundedIcon from '@mui/icons-material/AddRounded'
import SearchRoundedIcon from '@mui/icons-material/SearchRounded'
import EditRoundedIcon from '@mui/icons-material/EditRounded'
import { toast } from 'react-toastify'
import { adminBtnStyles } from '../../theme/adminButtonStyles'

const INITIAL_CATEGORIES = [
  { id: 'cat-1', name: 'Thời Trang & Phụ Kiện', parent: 'Gốc (Root)', slug: 'thoi-trang', productCount: 3420, commission: 5.0, isVisible: true },
  { id: 'cat-1-1', name: '— Thời Trang Nam', parent: 'Thời Trang & Phụ Kiện', slug: 'thoi-trang-nam', productCount: 1850, commission: 5.0, isVisible: true },
  { id: 'cat-1-2', name: '— Thời Trang Nữ', parent: 'Thời Trang & Phụ Kiện', slug: 'thoi-trang-nu', productCount: 1570, commission: 5.0, isVisible: true },
  { id: 'cat-2', name: 'Điện Thoại & Thiết Bị Điện Tử', parent: 'Gốc (Root)', slug: 'dien-tu', productCount: 2150, commission: 3.0, isVisible: true },
  { id: 'cat-2-1', name: '— Điện Thoại Di Động', parent: 'Điện Thoại & Thiết Bị Điện Tử', slug: 'dien-thoai', productCount: 980, commission: 3.0, isVisible: true },
  { id: 'cat-2-2', name: '— Tai Nghe & Loa Bluetooth', parent: 'Điện Thoại & Thiết Bị Điện Tử', slug: 'tai-nghe-loa', productCount: 1170, commission: 4.0, isVisible: true },
  { id: 'cat-3', name: 'Sức Khỏe & Sắc Đẹp (Mỹ Phẩm)', parent: 'Gốc (Root)', slug: 'my-pham-suc-khoe', productCount: 1890, commission: 6.0, isVisible: true },
  { id: 'cat-4', name: 'Nhà Cửa & Đời Sống', parent: 'Gốc (Root)', slug: 'nha-cua-doi-song', productCount: 1420, commission: 4.5, isVisible: true },
  { id: 'cat-5', name: 'Mẹ & Bé', parent: 'Gốc (Root)', slug: 'me-va-be', productCount: 860, commission: 5.0, isVisible: false }
]

export const CategoriesManagementTab = () => {
  const [categories, setCategories] = useState(INITIAL_CATEGORIES)
  const [searchTerm, setSearchTerm] = useState('')
  const [createModalOpen, setCreateModalOpen] = useState(false)
  const [editingCategory, setEditingCategory] = useState(null)

  // Form State
  const [formData, setFormData] = useState({
    name: '',
    parent: 'Gốc (Root)',
    slug: '',
    commission: 5.0,
    isVisible: true
  })

  const handleOpenCreate = () => {
    setEditingCategory(null)
    setFormData({ name: '', parent: 'Gốc (Root)', slug: '', commission: 5.0, isVisible: true })
    setCreateModalOpen(true)
  }

  const handleOpenEdit = (cat) => {
    setEditingCategory(cat)
    setFormData({
      name: cat.name.replace('— ', ''),
      parent: cat.parent,
      slug: cat.slug,
      commission: cat.commission,
      isVisible: cat.isVisible
    })
    setCreateModalOpen(true)
  }

  const handleSaveCategory = () => {
    if (!formData.name.trim()) {
      toast.error('Vui lòng nhập tên ngành hàng')
      return
    }

    if (editingCategory) {
      setCategories(prev => prev.map(c => c.id === editingCategory.id ? {
        ...c,
        name: formData.parent === 'Gốc (Root)' ? formData.name : `— ${formData.name}`,
        parent: formData.parent,
        slug: formData.slug || formData.name.toLowerCase().replace(/\s+/g, '-'),
        commission: Number(formData.commission),
        isVisible: formData.isVisible
      } : c))
      toast.success(`Đã cập nhật danh mục: ${formData.name}`)
    } else {
      const newCat = {
        id: `cat-${Date.now()}`,
        name: formData.parent === 'Gốc (Root)' ? formData.name : `— ${formData.name}`,
        parent: formData.parent,
        slug: formData.slug || formData.name.toLowerCase().replace(/\s+/g, '-'),
        productCount: 0,
        commission: Number(formData.commission),
        isVisible: formData.isVisible
      }
      setCategories(prev => [...prev, newCat])
      toast.success(`Đã thêm danh mục mới: ${formData.name}`)
    }

    setCreateModalOpen(false)
  }

  const handleToggleVisibility = (id) => {
    setCategories(prev => prev.map(c => {
      if (c.id === id) {
        const next = !c.isVisible
        if (next) toast.success(`Đã hiển thị danh mục trên menu`)
        else toast.info(`Đã ẩn danh mục khỏi menu`)
        return { ...c, isVisible: next }
      }
      return c
    }))
  }

  const filteredCategories = categories.filter(c => {
    if (searchTerm.trim()) {
      const q = searchTerm.toLowerCase()
      return c.name.toLowerCase().includes(q) || c.slug.toLowerCase().includes(q)
    }
    return true
  })

  return (
    <Card sx={{ p: 2.5, borderRadius: '16px', border: '1px solid #e2e8f0', bgcolor: '#ffffff', boxShadow: '0 2px 10px rgba(0,0,0,0.02)', width: '100%' }}>
      {/* Header & Button */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2.5, flexWrap: 'wrap', gap: 1.5 }}>
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <CategoryRoundedIcon sx={{ color: '#0095ff', fontSize: 24 }} />
            <Typography variant="h6" sx={{ fontWeight: 800, color: '#0f172a', whiteSpace: 'nowrap' }}>
              Quản Lý Cây Danh Mục Toàn Sàn (Category Hierarchy & Commission)
            </Typography>
          </Box>
          <Typography variant="caption" sx={{ color: '#64748b' }}>
            Định hình phân cấp sản phẩm và cấu hình tỷ lệ % phí hoa hồng sàn theo từng ngành hàng
          </Typography>
        </Box>
        <Button
          startIcon={<AddRoundedIcon />}
          onClick={handleOpenCreate}
          sx={adminBtnStyles.primary}
        >
          Thêm Danh Mục Mới
        </Button>
      </Box>

      {/* Search Bar */}
      <Box sx={{ mb: 2 }}>
        <TextField
          size="small"
          placeholder="Tìm tên danh mục, đường dẫn slug..."
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
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Tên Danh Mục</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Danh Mục Cha</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Slug URL</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Số Sản Phẩm Đang Bán</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Phí Hoa Hồng Sàn (%)</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Hiển Thị Menu</TableCell>
              <TableCell align="right" sx={{ fontWeight: 700, color: '#475569', whiteSpace: 'nowrap' }}>Thao Tác</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredCategories.map((c) => (
              <TableRow key={c.id} hover>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <CategoryRoundedIcon sx={{ color: c.parent === 'Gốc (Root)' ? '#0095ff' : '#94a3b8', fontSize: '1.2rem', flexShrink: 0 }} />
                    <Typography variant="body2" sx={{ fontWeight: c.parent === 'Gốc (Root)' ? 800 : 500, color: '#0f172a', whiteSpace: 'nowrap' }}>
                      {c.name}
                    </Typography>
                  </Box>
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}><Typography variant="caption" sx={{ color: '#64748b', whiteSpace: 'nowrap' }}>{c.parent}</Typography></TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}><code style={{ color: '#0095ff', fontWeight: 600 }}>/{c.slug}</code></TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}><Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a', whiteSpace: 'nowrap' }}>{c.productCount.toLocaleString()}</Typography></TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Chip
                    label={`${c.commission}%`}
                    size="small"
                    sx={{
                      fontWeight: 700,
                      height: 24,
                      fontSize: '0.75rem',
                      whiteSpace: 'nowrap',
                      bgcolor: '#eff6ff',
                      color: '#0095ff',
                      border: '1px solid #bfdbfe'
                    }}
                  />
                </TableCell>
                <TableCell sx={{ whiteSpace: 'nowrap' }}>
                  <Switch
                    size="small"
                    checked={c.isVisible}
                    onChange={() => handleToggleVisibility(c.id)}
                    sx={{
                      '& .MuiSwitch-switchBase.Mui-checked': {
                        color: '#0095ff'
                      },
                      '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                        backgroundColor: '#0095ff'
                      }
                    }}
                  />
                </TableCell>
                <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                  <Button
                    size="small"
                    startIcon={<EditRoundedIcon sx={{ fontSize: '16px !important' }} />}
                    onClick={() => handleOpenEdit(c)}
                    sx={adminBtnStyles.transSecondary}
                  >
                    Sửa
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* MODAL: THÊM / SỬA DANH MỤC */}
      <Dialog
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        maxWidth="xs"
        fullWidth
        PaperProps={{ sx: { borderRadius: '16px', p: 1 } }}
      >
        <DialogTitle sx={{ fontWeight: 800, whiteSpace: 'nowrap' }}>
          {editingCategory ? 'Chỉnh Sửa Danh Mục' : 'Thêm Danh Mục Mới'}
        </DialogTitle>
        <DialogContent dividers>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <TextField
              label="Tên danh mục"
              fullWidth
              size="small"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              sx={{ '& .MuiOutlinedInput-root': { borderRadius: '10px' } }}
            />

            <TextField
              select
              label="Danh mục cha"
              fullWidth
              size="small"
              value={formData.parent}
              onChange={(e) => setFormData({ ...formData, parent: e.target.value })}
              sx={{ '& .MuiOutlinedInput-root': { borderRadius: '10px' } }}
            >
              <MenuItem value="Gốc (Root)">Gốc (Root - Danh mục chính)</MenuItem>
              <MenuItem value="Thời Trang & Phụ Kiện">Thời Trang & Phụ Kiện</MenuItem>
              <MenuItem value="Điện Thoại & Thiết Bị Điện Tử">Điện Thoại & Thiết Bị Điện Tử</MenuItem>
              <MenuItem value="Sức Khỏe & Sắc Đẹp (Mỹ Phẩm)">Sức Khỏe & Sắc Đẹp (Mỹ Phẩm)</MenuItem>
              <MenuItem value="Nhà Cửa & Đời Sống">Nhà Cửa & Đời Sống</MenuItem>
            </TextField>

            <TextField
              label="Slug (Đường dẫn tĩnh)"
              fullWidth
              size="small"
              placeholder="VD: thoi-trang-nam"
              value={formData.slug}
              onChange={(e) => setFormData({ ...formData, slug: e.target.value })}
              sx={{ '& .MuiOutlinedInput-root': { borderRadius: '10px' } }}
            />

            <TextField
              label="Tỷ lệ phí hoa hồng sàn (%)"
              type="number"
              fullWidth
              size="small"
              value={formData.commission}
              onChange={(e) => setFormData({ ...formData, commission: e.target.value })}
              sx={{ '& .MuiOutlinedInput-root': { borderRadius: '10px' } }}
              InputProps={{
                endAdornment: <InputAdornment position="end">%</InputAdornment>
              }}
            />

            <FormControlLabel
              control={
                <Switch
                  checked={formData.isVisible}
                  onChange={(e) => setFormData({ ...formData, isVisible: e.target.checked })}
                  sx={{
                    '& .MuiSwitch-switchBase.Mui-checked': {
                      color: '#0095ff'
                    },
                    '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': {
                      backgroundColor: '#0095ff'
                    }
                  }}
                />
              }
              label={<Typography variant="body2" sx={{ fontWeight: 600 }}>Hiển thị trên Menu chính của sàn</Typography>}
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setCreateModalOpen(false)} sx={{ textTransform: 'none', color: '#64748b', fontWeight: 600, whiteSpace: 'nowrap' }}>Hủy</Button>
          <Button
            onClick={handleSaveCategory}
            sx={adminBtnStyles.primary}
          >
            Lưu Danh Mục
          </Button>
        </DialogActions>
      </Dialog>
    </Card>
  )
}

export default CategoriesManagementTab
