import ArrowUpwardRoundedIcon from '@mui/icons-material/ArrowUpwardRounded'
import FavoriteRoundedIcon from '@mui/icons-material/FavoriteRounded'
import ShareRoundedIcon from '@mui/icons-material/ShareRounded'
import { Avatar, Box, IconButton, InputBase, Typography } from '@mui/material'
import { useEffect, useRef, useState } from 'react'
import PushPinRoundedIcon from '@mui/icons-material/PushPinRounded'
import { glassSx } from '~/theme'

const CommentItem = ({ comment }) => {
  const isPinned = comment.isPinned

  return (
    <Box
      sx={{
        display: 'inline-flex',
        alignItems: 'center',
        ...glassSx,
        gap: 1.2,
        pl: 1,
        py: 1, // Giảm padding trên dưới
        bgcolor: 'rgba(255, 255, 255, 0.15)', // Trắng trong suốt
        backdropFilter: 'blur(12px)',
        WebkitBackdropFilter: 'blur(12px)', // Hỗ trợ Safari
        borderRadius: '24px',
        maxWidth: '80%',
        minWidth: '80%',
        position: 'relative',
        transition: 'transform 0.2s',
        '&:active': { transform: 'scale(0.98)' }
      }}
    >
      <Avatar
        src={comment.avatar}
        sx={{
          width: 28, // Chỉnh avatar gọn lại chút để bubble thanh thoát hơn
          height: 28,
          fontSize: '0.8rem',
          bgcolor: 'rgba(255,255,255,0.2)'
        }}
      >
        {comment.userName?.[0]}
      </Avatar>

      <Box sx={{ flex: 1, overflow: 'hidden' }}>
        <Typography
          variant="subtitle2"
          sx={{
            color: 'white',
            fontWeight: 700,
            fontSize: '0.75rem',
            lineHeight: 1.2,
            display: 'flex',
            alignItems: 'center',
            gap: 0.5
          }}
        >
          {comment.userName}
          {isPinned && <PushPinRoundedIcon sx={{ fontSize: 12, transform: 'rotate(45deg)', opacity: 0.8 }} />}
        </Typography>

        <Typography
          variant="body2"
          sx={{
            color: 'rgba(255, 255, 255, 0.9)',
            fontSize: '0.75rem',
            lineHeight: 1.3,
            whiteSpace: 'nowrap',
            overflow: 'hidden',
            textOverflow: 'ellipsis'
          }}
        >
          {comment.message}
        </Typography>
      </Box>

      {isPinned && (
        <Box sx={{ ml: 1, opacity: 0.6 }}>
          <PushPinRoundedIcon sx={{ fontSize: 16, color: 'white' }} />
        </Box>
      )}
    </Box>
  )
}

const CIRCLE_BTN = {
  width: 38, height: 38, borderRadius: '50%',
  bgcolor: 'rgba(255,255,255,0.18)',
  backdropFilter: 'blur(8px)',
  WebkitBackdropFilter: 'blur(8px)',
  border: '1px solid rgba(255,255,255,0.15)',
  pointerEvents: 'auto',
  flexShrink: 0
}

const LiveComments = ({ comments, onSend, commentError, likes, onLike }) => {
  const [text, setText] = useState('')
  const listRef = useRef(null)

  useEffect(() => {
    if (listRef.current) listRef.current.scrollTop = listRef.current.scrollHeight
  }, [comments.length])

  const handleSend = () => {
    const msg = text.trim()
    if (!msg) return
    onSend(msg)
    setText('')
  }

  return (
    <Box sx={{
      position: 'absolute', bottom: 8, left: 0, right: 0,
      background: 'linear-gradient(to top, rgba(0,0,0,0.7) 35%, rgba(0,0,0,0.1) 75%, transparent 100%)',
      px: 2, pb: 1.25, pt: 4,
      pointerEvents: 'none'
    }}>
      {/* Comment list */}
      <Box
        ref={listRef}
        sx={{
          maxHeight: 180,
          overflowY: 'auto',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'flex-start',
          gap: 1,
          mb: 1.5,
          '&::-webkit-scrollbar': { display: 'none' },
          scrollbarWidth: 'none'
        }}
      >
        {comments.length === 0 && (
          <Typography sx={{
            color: 'rgba(255,255,255,0.5)', fontSize: '0.72rem',
            textShadow: '0 1px 3px rgba(0,0,0,0.8)'
          }}>
            Hãy là người đầu tiên bình luận...
          </Typography>
        )}
        {comments.map(c => <CommentItem key={c.id} comment={c} />)}
      </Box>

      {commentError && (
        <Typography sx={{ color: '#fca5a5', fontSize: '0.65rem', mb: 0.5 }}>
          {commentError}
        </Typography>
      )}

      {/* Bottom action row: */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
        {/* Input pill — send arrow inside */}
        <Box sx={{
          flex: 1, display: 'flex', alignItems: 'center',
          bgcolor: 'rgba(255,255,255,0.14)',
          backdropFilter: 'blur(12px)',
          WebkitBackdropFilter: 'blur(12px)',
          borderRadius: '24px',
          border: '1px solid rgba(255,255,255,0.2)',
          px: 1.25, py: 0.45,
          pointerEvents: 'auto'
        }}>
          <InputBase
            value={text}
            onChange={e => setText(e.target.value)}
            onKeyDown={e => { if (e.key === 'Enter') { e.preventDefault(); handleSend() } }}
            placeholder="Comment..."
            sx={{
              flex: 1, color: 'white', fontSize: '0.8rem',
              '& input::placeholder': { color: 'rgba(255,255,255,0.5)' }
            }}
            slotProps={{ maxLength: 200 }}
          />
          <IconButton
            onClick={handleSend}
            size="small"
            disabled={!text.trim()}
            sx={{
              p: 0.4, ml: 0.25,
              bgcolor: text.trim() ? '#4a7fff' : 'rgba(255,255,255,0.15)',
              borderRadius: '50%',
              transition: 'background 0.2s',
              '&:hover': { bgcolor: '#3a6fe0' },
              '&.Mui-disabled': { bgcolor: 'rgba(255,255,255,0.1)' }
            }}
          >
            <ArrowUpwardRoundedIcon sx={{ color: 'white', fontSize: 16 }} />
          </IconButton>
        </Box>

        {/* Share */}
        <IconButton sx={CIRCLE_BTN}>
          <ShareRoundedIcon sx={{ color: 'white', fontSize: 19 }} />
        </IconButton>

        {/* Like */}
        <Box sx={{ position: 'relative', display: 'flex', alignItems: 'center' }}>
          <Typography
            sx={{
              position: 'absolute',
              bottom: '105%',
              left: '50%',
              transform: 'translateX(-50%)',
              color: 'white',
              fontSize: '0.7rem',
              fontWeight: 700,
              textShadow: '0 1px 4px rgba(0,0,0,0.8)',

              pointerEvents: 'none',
              whiteSpace: 'nowrap'
            }}
          >
            {likes > 0 ? likes : ''}
          </Typography>

          {/* Nút Like */}
          <IconButton
            onClick={onLike}
            sx={{
              ...CIRCLE_BTN,
              '&:hover': { bgcolor: 'rgba(239,68,68,0.35)' },
              '&:active': { transform: 'scale(1.2)', bgcolor: 'rgba(239,68,68,0.2)' }, transition: 'transform 0.1s'
            }}
          >
            <FavoriteRoundedIcon sx={{ color: '#ffffff', fontSize: 19, '&:active': { color: '#ff4f4fff' } }} />
          </IconButton>
        </Box>
      </Box>
    </Box>
  )
}

export default LiveComments
