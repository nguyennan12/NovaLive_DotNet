import { useState, useEffect } from 'react'
import {
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  MenuItem,
  Stack,
  TextField,
  Typography
} from '@mui/material'
import AddRoundedIcon from '@mui/icons-material/AddRounded'
import EditRoundedIcon from '@mui/icons-material/EditRounded'
import DeleteRoundedIcon from '@mui/icons-material/DeleteRounded'
import RefreshRoundedIcon from '@mui/icons-material/RefreshRounded'
import CategoryRoundedIcon from '@mui/icons-material/CategoryRounded'
import { toast } from 'react-toastify'
import {
  getAdminCategoriesAPI,
  createAdminCategoryAPI,
  updateAdminCategoryAPI,
  deleteAdminCategoryAPI
} from '~/common/apis/services/adminService'
import { PageHeader } from '~/features/Seller/components/shared/PageHeader'
import { SellerCard } from '~/features/Seller/components/shared/SellerCard'
import { DataTable } from '~/features/Seller/components/shared/DataTable'

export const AdminCategoriesPage = () => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [categories, setCategories] = useState([])

  // Modal State
  const [modalOpen, setModalOpen] = useState(false)
  const [editingCategory, setEditingCategory] = useState(null)
  const [deleteConfirmCat, setDeleteConfirmCat] = useState(null)

  const [name, setName] = useState('')
  const [parentId, setParentId] = useState('')
  const [iconUrl, setIconUrl] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  const loadCategories = async () => {
    setLoading(true)
    setError(null)
    try {
      const res = await getAdminCategoriesAPI()
      const list = Array.isArray(res) ? res : res?.items || res?.data || []
      setCategories(list)
    } catch (err) {
      setError(err)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadCategories()
  }, [])

  const handleOpenCreate = () => {
    setEditingCategory(null)
    setName('')
    setParentId('')
    setIconUrl('')
    setModalOpen(true)
  }

  const handleOpenEdit = (cat) => {
    setEditingCategory(cat)
    setName(cat.name || '')
    setParentId(cat.parentId || cat.parent_id || '')
    setIconUrl(cat.iconUrl || cat.icon_url || '')
    setModalOpen(true)
  }

  const handleSave = async (e) => {
    e.preventDefault()
    if (!name.trim()) {
      toast.error('Vui lòng nhập tên danh mục')
      return
    }

    setIsSubmitting(true)
    try {
      const payload = {
        name: name.trim(),
        parentId: parentId || null,
        iconUrl: iconUrl.trim() || null
      }

      if (editingCategory) {
        await updateAdminCategoryAPI(editingCategory.id, payload)
        toast.success(`Đã cập nhật danh mục: ${name}`)
      } else {
        await createAdminCategoryAPI(payload)
        toast.success(`Đã thêm mới danh mục: ${name}`)
      }
      setModalOpen(false)
      loadCategories()
    } catch (err) {
      toast.error(err?.response?.data?.detail || 'Thao tác thất bại')
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDelete = async () => {
    if (!deleteConfirmCat) return
    setIsSubmitting(true)
    try {
      await deleteAdminCategoryAPI(deleteConfirmCat.id)
      toast.success(`Đã xóa danh mục: ${deleteConfirmCat.name}`)
      setDeleteConfirmCat(null)
      loadCategories()
    } catch (err) {
      toast.error(err?.response?.data?.detail || 'Xóa danh mục thất bại')
    } finally {
      setIsSubmitting(false)
    }
  }

  const rootCategories = categories.filter((c) => !c.parentId && !c.parent_id)

  return (
    <>
      <PageHeader
        title="Quản lý Cây danh mục"
        description="Thiết lập phân cấp ngành hàng và danh mục sản phẩm hiển thị trên sàn."
        action={
          <Stack direction="row" spacing={1}>
            <Button
              variant="outlined"
              size="small"
              startIcon={<RefreshRoundedIcon fontSize="small" />}
              onClick={loadCategories}
              disabled={loading}
              sx={{
                borderColor: 'divider',
                color: 'text.secondary',
                textTransform: 'none',
                borderRadius: 2,
                '&:hover': { bgcolor: 'rgba(0,0,0,0.04)' }
              }}
            >
              Làm mới
            </Button>
            <Button
              variant="contained"
              size="small"
              color="secondary"
              startIcon={<AddRoundedIcon fontSize="small" />}
              onClick={handleOpenCreate}
              sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
            >
              Thêm danh mục
            </Button>
          </Stack>
        }
      />

      <SellerCard>
        <DataTable
          rows={categories}
          rowKey={(row) => row.id}
          loading={loading}
          error={error}
          onRetry={loadCategories}
          emptyText="Chưa có danh mục nào trên sàn."
          columns={[
            {
              key: 'name',
              label: 'Tên ngành hàng',
              render: (row) => (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                  <CategoryRoundedIcon fontSize="small" color="secondary" />
                  <Typography variant="body2" sx={{ fontWeight: 700, color: '#0f172a' }}>
                    {row.name}
                  </Typography>
                </Box>
              )
            },
            {
              key: 'slug',
              label: 'Đường dẫn (Slug)',
              render: (row) => (
                <Typography variant="body2" color="text.secondary">
                  {row.slug || '—'}
                </Typography>
              )
            },
            {
              key: 'parent',
              label: 'Phân cấp cha',
              render: (row) => {
                const parent = categories.find((c) => c.id === (row.parentId || row.parent_id))
                return (
                  <Chip
                    label={parent ? parent.name : 'Danh mục gốc'}
                    size="small"
                    variant={parent ? 'outlined' : 'filled'}
                    sx={{ borderRadius: 1.5, fontSize: '0.75rem' }}
                  />
                )
              }
            },
            {
              key: 'actions',
              label: 'Thao tác',
              align: 'right',
              render: (row) => (
                <Stack direction="row" spacing={1} justifyContent="flex-end">
                  <IconButton size="small" onClick={() => handleOpenEdit(row)}>
                    <EditRoundedIcon fontSize="small" />
                  </IconButton>
                  <IconButton size="small" color="error" onClick={() => setDeleteConfirmCat(row)}>
                    <DeleteRoundedIcon fontSize="small" />
                  </IconButton>
                </Stack>
              )
            }
          ]}
        />
      </SellerCard>

      {/* Add / Edit Dialog */}
      <Dialog
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        maxWidth="xs"
        fullWidth
        slotProps={{ paper: { sx: { borderRadius: 3 } } }}
      >
        <form onSubmit={handleSave}>
          <DialogTitle sx={{ fontWeight: 800 }}>
            {editingCategory ? 'Chỉnh sửa ngành hàng' : 'Thêm ngành hàng mới'}
          </DialogTitle>
          <DialogContent dividers>
            <Stack spacing={2} sx={{ pt: 1 }}>
              <TextField
                required
                fullWidth
                size="small"
                label="Tên danh mục"
                placeholder="VD: Đồ gia dụng"
                value={name}
                onChange={(e) => setName(e.target.value)}
              />
              <TextField
                select
                fullWidth
                size="small"
                label="Danh mục cha"
                value={parentId}
                onChange={(e) => setParentId(e.target.value)}
              >
                <MenuItem value="">— Là danh mục gốc (Root) —</MenuItem>
                {rootCategories
                  .filter((c) => c.id !== editingCategory?.id)
                  .map((c) => (
                    <MenuItem key={c.id} value={c.id}>
                      {c.name}
                    </MenuItem>
                  ))}
              </TextField>
              <TextField
                fullWidth
                size="small"
                label="URL Icon (tùy chọn)"
                placeholder="https://..."
                value={iconUrl}
                onChange={(e) => setIconUrl(e.target.value)}
              />
            </Stack>
          </DialogContent>
          <DialogActions sx={{ p: 2 }}>
            <Button onClick={() => setModalOpen(false)} color="inherit" sx={{ textTransform: 'none' }}>
              Hủy
            </Button>
            <Button
              type="submit"
              variant="contained"
              color="secondary"
              disabled={isSubmitting || !name.trim()}
              sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
            >
              {editingCategory ? 'Lưu thay đổi' : 'Tạo danh mục'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>

      {/* Delete Confirm Dialog */}
      <Dialog
        open={Boolean(deleteConfirmCat)}
        onClose={() => setDeleteConfirmCat(null)}
        maxWidth="xs"
        fullWidth
        slotProps={{ paper: { sx: { borderRadius: 3 } } }}
      >
        <DialogTitle sx={{ fontWeight: 800 }}>Xác nhận xóa danh mục</DialogTitle>
        <DialogContent>
          <Typography variant="body2" color="text.secondary">
            Bạn có chắc chắn muốn xóa ngành hàng: <strong>{deleteConfirmCat?.name}</strong>? Thao tác này không thể hoàn tác.
          </Typography>
        </DialogContent>
        <DialogActions sx={{ p: 2 }}>
          <Button onClick={() => setDeleteConfirmCat(null)} color="inherit" sx={{ textTransform: 'none' }}>
            Hủy
          </Button>
          <Button
            variant="contained"
            color="error"
            onClick={handleDelete}
            disabled={isSubmitting}
            sx={{ textTransform: 'none', borderRadius: 2, fontWeight: 700 }}
          >
            Xác nhận xóa
          </Button>
        </DialogActions>
      </Dialog>
    </>
  )
}

export default AdminCategoriesPage
