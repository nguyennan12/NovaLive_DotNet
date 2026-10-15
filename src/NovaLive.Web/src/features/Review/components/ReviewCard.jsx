import MoreHorizRoundedIcon from '@mui/icons-material/MoreHorizRounded'
import ThumbUpOutlinedIcon from '@mui/icons-material/ThumbUpOutlined'
import ThumbUpRoundedIcon from '@mui/icons-material/ThumbUpRounded'
import { Avatar, Box, IconButton, Menu, MenuItem, Rating, Typography } from '@mui/material'
import { useState } from 'react'
import { formatDate } from '~/common/utils/formatters'

const AVATAR_COLORS = ['#3485f7', '#8b5cf6', '#f59e0b', '#10b981', '#ef4444', '#06b6d4']

const avatarColor = (name = '') =>
  AVATAR_COLORS[name.charCodeAt(0) % AVATAR_COLORS.length]

const timeAgo = (isoStr) => {
  const diff = Math.floor((Date.now() - new Date(isoStr)) / 1000)
  if (diff < 60) return 'Vừa xong'
  if (diff < 3600) return `${Math.floor(diff / 60)} phút trước`
  if (diff < 86400) return `${Math.floor(diff / 3600)} giờ trước`
  if (diff < 86400 * 30) return `${Math.floor(diff / 86400)} ngày trước`
  return formatDate(isoStr)
}

const ReviewMenu = () => {
  const [anchor, setAnchor] = useState(null)
  return (
    <>
      <IconButton
        size="small"
        onClick={(e) => setAnchor(e.currentTarget)}
        sx={{ color: '#bbb', '&:hover': { color: '#555' } }}
      >
        <MoreHorizRoundedIcon sx={{ fontSize: 18 }} />
      </IconButton>
      <Menu
        anchorEl={anchor}
        open={Boolean(anchor)}
        onClose={() => setAnchor(null)}
        slotProps={{ paper: { elevation: 2, sx: { borderRadius: '10px', minWidth: 160 } } }}
      >
        <MenuItem onClick={() => setAnchor(null)} sx={{ fontSize: '0.82rem' }}>
          Sao chép liên kết
        </MenuItem>
        <MenuItem onClick={() => setAnchor(null)} sx={{ fontSize: '0.82rem', color: 'error.main' }}>
          Báo cáo
        </MenuItem>
      </Menu>
    </>
  )
}

const ReviewCard = ({ review }) => {
  const [liked, setLiked] = useState(false)
  const [likeCount, setLikeCount] = useState(review.likes ?? 0)

  const toggleLike = () => {
    setLiked(p => {
      setLikeCount(c => p ? c - 1 : c + 1)
      return !p
    })
  }

  return (
    <Box sx={{ py: 2 }}>
      <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1.5 }}>
        <Avatar
          src={review.user?.avatar}
          sx={{
            width: 38, height: 38, flexShrink: 0,
            bgcolor: avatarColor(review.user?.name),
            fontSize: '0.9rem', fontWeight: 800
          }}
        >
          {review.user?.name?.charAt(0)}
        </Avatar>

        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
              <Typography sx={{ fontSize: '0.82rem', fontWeight: 700, color: 'primary.contrastText' }}>
                {review.user?.name}
              </Typography>
              <Rating
                value={review.rating}
                readOnly size="small"
                sx={{ color: '#f59e0b', '& .MuiRating-icon': { fontSize: 13 } }}
              />
            </Box>
            <ReviewMenu />
          </Box>

          <Typography sx={{ fontSize: '0.72rem', color: '#bbb', mt: 0.3, mb: 1 }}>
            {timeAgo(review.createdAt)}
          </Typography>

          <Typography sx={{ fontSize: '0.84rem', color: '#444', lineHeight: 1.6 }}>
            {review.content}
          </Typography>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 1.25 }}>
            <IconButton
              size="small"
              onClick={toggleLike}
              sx={{ p: 0.5, color: liked ? 'secondary.main' : '#bbb', transition: 'color 0.18s' }}
            >
              {liked
                ? <ThumbUpRoundedIcon sx={{ fontSize: 15 }} />
                : <ThumbUpOutlinedIcon sx={{ fontSize: 15 }} />
              }
            </IconButton>
            {likeCount > 0 && (
              <Typography sx={{ fontSize: '0.72rem', color: liked ? 'secondary.main' : '#bbb', fontWeight: 600, transition: 'color 0.18s' }}>
                {likeCount}
              </Typography>
            )}
          </Box>
        </Box>
      </Box>
    </Box>
  )
}

export default ReviewCard
