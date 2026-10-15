import AddIcon from '@mui/icons-material/Add'
import CloseIcon from '@mui/icons-material/Close'
import InventoryIcon from '@mui/icons-material/Inventory'
import SaveOutlinedIcon from '@mui/icons-material/SaveOutlined'
import { Box, Breadcrumbs, Button, Link, Typography } from '@mui/material'
import { useState } from 'react'
import { useSelector } from 'react-redux'
import { Link as RouterLink } from 'react-router-dom'
import { toast } from 'react-toastify'
import { selectCurrentUser } from '~/store/user/userSlice'
import { DiscountForm } from '../components/DiscountPage/DiscountForm'
import { DiscountList } from '../components/DiscountPage/DiscountList'
import { DiscountStat } from '../components/DiscountPage/DiscountStat'
import { DeleteDialog } from '../components/shared/DeleteDialog'
import { DiscountFilterBar } from '../components/shared/DiscountFilterBar'
import { useDiscounts } from '../hook/useDiscounts'


export const DiscountPage = () => {
  const user = useSelector(selectCurrentUser)
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('all')
  const [type, setType] = useState('all')
  const [category, setCategory] = useState('all')
  const filters = { search, status, type, category, shopId: user.user_shop }

  const { filtered, stats, addDiscount, updateDiscount, deleteDiscount, isLoading, isFetching, isMutating } =
    useDiscounts(filters)

  const [editData, setEditData] = useState(null)

  const handleEditClick = (discount) => {
    setEditData(discount)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const handleFormSubmit = async (data) => {
    const getErrorMessage = (error) => error?.response?.data?.message || error?.message || 'Request failed'

    try {
      const actionPromise = editData
        ? updateDiscount(editData, data)
        : addDiscount(data)

      await toast.promise(actionPromise, {
        pending: editData ? 'Đang cập nhật discount...' : 'Đang tạo discount...',
        success: editData ? 'Đã cập nhật discount!' : 'Tạo discount thành công!',
        error: {
          render({ data: error }) {
            return getErrorMessage(error)
          }
        }
      })

      setEditData(null)
    } catch {
      // Error toast is already handled by toast.promise
    }
  }

  const [deleteTarget, setDeleteTarget] = useState(null)
  const handleDeleteConfirm = async () => {
    const getErrorMessage = (error) => error?.response?.data?.message || error?.message || 'Request failed'

    try {
      await toast.promise(deleteDiscount(deleteTarget), {
        pending: 'Đang xoá discount...',
        success: 'Đã xoá discount.',
        error: {
          render({ data: error }) {
            return getErrorMessage(error)
          }
        }
      })

      setDeleteTarget(null)
    } catch {
      // Error toast is already handled by toast.promise
    }
  }

  const hasFilter = !!search || status !== 'all' || type !== 'all' || category !== 'all'
  const handleCancel = () => setEditData(null)

  return (
    <Box sx={{
      display: 'flex',
      flexDirection: 'column',
      gap: 2.5,
      background: 'linear-gradient(135deg, #f8faff 0%, #f0f4ff 50%, #f5f8ff 100%)',
      p: { xs: 2, md: 3 },
      minHeight: '100vh'
    }}>

      {/* ── HEADER ── */}
      <Box>
        <Breadcrumbs sx={{ mb: 1 }}>
          <Link
            component={RouterLink}
            to="/dashboard/shop/products"
            underline="hover"
            sx={{ display: 'flex', alignItems: 'center', gap: 0.5, fontSize: '0.8rem', color: 'primary.contrastText' }}
          >
            <InventoryIcon sx={{ fontSize: 13 }} /> Discounts
          </Link>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, fontSize: '0.8rem', fontWeight: 600 }}>
            <AddIcon sx={{ fontSize: 13 }} /> Manager
          </Box>
        </Breadcrumbs>
        <Typography variant="h5" fontWeight={800} letterSpacing="-0.5px" sx={{ color: 'primary.contrastText' }}>
          Discount Management
        </Typography>
        <Typography variant="body2" color="primary.contrastText" mt={0.25}>
          Manage discount codes, promotional campaigns, and customer offers.
        </Typography>
      </Box>

      {/* ── STATS ROW ── */}
      <DiscountStat stats={stats} />

      {/* ── FORM SECTION ── */}
      <Box sx={{
        bgcolor: '#fff',
        border: '1px solid',
        borderColor: 'divider',
        borderRadius: '16px',
        p: { xs: 1.5, md: 2 }
      }}>
        {/* Form header + actions */}
        <Box sx={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          mb: 2,
          flexWrap: 'wrap',
          gap: 1
        }}>
          <Typography sx={{
            fontSize: '0.72rem', fontWeight: 800, letterSpacing: '0.09em',
            textTransform: 'uppercase', color: 'text.secondary'
          }}>
            {editData ? 'Edit discount' : 'Create new discount'}
          </Typography>
          <Box sx={{ display: 'flex', gap: 1 }}>
            <Button
              variant="outlined"
              size="small"
              startIcon={<CloseIcon sx={{ fontSize: '14px !important' }} />}
              onClick={handleCancel}
              disabled={isMutating}
              sx={{
                borderRadius: 2, fontSize: '0.78rem', fontWeight: 600,
                borderColor: 'divider', color: 'text.secondary',
                '&:hover': { borderColor: '#ccc', bgcolor: '#f9f9f9' }
              }}
            >
              Huỷ
            </Button>
            <Button
              type="submit"
              form="discount-form"
              variant="contained"
              size="small"
              startIcon={<SaveOutlinedIcon sx={{ fontSize: '14px !important' }} />}
              disabled={isMutating}
              disableElevation
              sx={{
                borderRadius: 2, fontSize: '0.78rem', fontWeight: 700,
                background: 'linear-gradient(90deg, #4c83f0, #69aedc)',
                color: '#fff',
                boxShadow: '0 3px 8px rgba(76,131,240,0.25)',
                '&:hover': { opacity: 0.9 }
              }}
            >
              {editData ? 'Lưu thay đổi' : 'Hoàn tất'}
            </Button>
          </Box>
        </Box>

        <DiscountForm editData={editData} onSubmit={handleFormSubmit} />
      </Box>

      {/* ── LIST SECTION ── */}
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        <DiscountFilterBar
          search={search}
          status={status}
          type={type}
          category={category}
          onSearchChange={setSearch}
          onStatusChange={setStatus}
          onTypeChange={setType}
          onCategoryChange={setCategory}
        />
        <DiscountList
          discounts={filtered}
          loading={isLoading || isFetching}
          hasFilter={hasFilter}
          onEdit={handleEditClick}
          onDelete={setDeleteTarget}
        />
      </Box>

      {/* ── DIALOGS ── */}
      <DeleteDialog
        open={Boolean(deleteTarget)}
        discount={deleteTarget}
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteTarget(null)}
      />
    </Box>
  )
}