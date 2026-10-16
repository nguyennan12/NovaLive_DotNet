import PersonIcon from '@mui/icons-material/Person'
import EditIcon from '@mui/icons-material/Edit'
import { Box, Paper, IconButton, Tooltip } from '@mui/material'
import { useMemo, useState } from 'react'
import { formatDate } from '~/common/utils/formatters'
import SectionTitle from '../shared/SectionTitle'
import InfoRow from '../shared/InfoRow'
import FullEditProfileDialog from './FullEditProfileDialog'
import { glassSx } from '~/theme'

const GENDER_LABEL = { male: 'Nam', female: 'Nữ', other: 'Khác' }

const buildRows = (profile) => [
  { label: 'Họ và tên', value: profile?.user_name, field: 'user_name', editLabel: 'Họ và tên' },
  {
    label: 'Email',
    value: profile?.user_email,
    verified: profile?.isVerified ?? true,
    field: null
  },
  {
    label: 'Số điện thoại',
    value: profile?.user_phone,
    field: 'user_phone',
    editLabel: 'Số điện thoại'
  },
  {
    label: 'Giới tính',
    value: GENDER_LABEL[profile?.user_gender] ?? null,
    field: 'user_gender',
    editLabel: 'Giới tính'
  },
  {
    label: 'Ngày sinh',
    value: profile?.user_birthday ? formatDate(profile.user_birthday) : null,
    field: 'user_birthday',
    editLabel: 'Ngày sinh',
    rawValue: profile?.user_birthday?.slice(0, 10)
  }
]

const PersonalInfoCard = ({ profile, isLoading, onSave, isSaving }) => {
  const [isFullEditOpen, setIsFullEditOpen] = useState(false)

  const rows = useMemo(() => buildRows(profile), [profile])

  const handleSave = (data, options) => onSave(data, options)

  return (
    <>
      <Paper
        elevation={0}
        sx={{ borderRadius: 3, p: { xs: 2, sm: 2.5 }, bgcolor: 'primary.main', border: '1px solid', borderColor: 'divider', minWidth: '360px', height: '100%', ...glassSx }}
      >
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <SectionTitle title="Thông tin cá nhân" icon={PersonIcon} />
          {!isLoading && (
            <Tooltip title="Sửa tất cả">
              <IconButton
                size="small"
                onClick={() => setIsFullEditOpen(true)}
                sx={{
                  color: 'secondary.main',
                  mt: -0.5,
                  '&:hover': { background: 'rgba(83,155,255,0.08)' }
                }}
              >
                <EditIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Box>
        <Box>
          {rows.map(({ label, value, verified }) => (
            <InfoRow
              key={label}
              label={label}
              value={value}
              verified={verified}
              loading={isLoading}
            />
          ))}
        </Box>
      </Paper>

      <FullEditProfileDialog
        open={isFullEditOpen}
        onClose={() => setIsFullEditOpen(false)}
        profile={profile}
        onSave={handleSave}
        isSaving={isSaving}
      />
    </>
  )
}

export default PersonalInfoCard
