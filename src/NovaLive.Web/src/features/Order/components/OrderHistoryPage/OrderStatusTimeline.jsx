import CheckCircleRoundedIcon from '@mui/icons-material/CheckCircleRounded'
import RadioButtonUncheckedRoundedIcon from '@mui/icons-material/RadioButtonUncheckedRounded'
import CancelRoundedIcon from '@mui/icons-material/CancelRounded'
import { Box, Typography } from '@mui/material'
import { STATUS_TIMELINE_STEPS } from '../../constants/orderStatus'
import { formatDate } from '~/common/utils/formatters'

function OrderStatusTimeline({ order }) {
  const { order_status, createdAt, deliveredAt, cancelledAt, updatedAt } = order

  if (order_status === 'cancelled') {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
        <TimelineStep
          done
          isCancelled
          label="Đặt hàng thành công"
          time={createdAt}
        />
        <TimelineStep
          done
          isCancelled
          label="Đơn hàng đã bị hủy"
          time={cancelledAt || updatedAt}
          isLast
        />
      </Box>
    )
  }

  const statusOrder = ['pending', 'processing', 'confirmed', 'shipped', 'delivered']
  const currentIdx = statusOrder.indexOf(order_status)

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
      {STATUS_TIMELINE_STEPS.map((step, idx) => {
        const done = idx <= currentIdx
        const isCurrent = idx === currentIdx
        const time = idx === 0
          ? createdAt
          : (idx === STATUS_TIMELINE_STEPS.length - 1 && deliveredAt)
            ? deliveredAt
            : (isCurrent ? updatedAt : null)

        return (
          <TimelineStep
            key={step.status}
            done={done}
            isCurrent={isCurrent}
            label={step.label}
            time={done ? time : null}
            isLast={idx === STATUS_TIMELINE_STEPS.length - 1}
          />
        )
      })}
    </Box>
  )
}

function TimelineStep({ done, isCurrent, isCancelled, label, time, isLast }) {
  const iconColor = isCancelled
    ? '#dc2626'
    : done
      ? (isCurrent ? '#3485f7' : '#16a34a')
      : 'rgba(0,0,0,0.2)'

  return (
    <Box sx={{ display: 'flex', gap: 1.5, position: 'relative' }}>
      {/* Connector line */}
      {!isLast && (
        <Box sx={{
          position: 'absolute',
          left: 10, top: 22,
          width: 2, height: 'calc(100% + 6px)',
          bgcolor: done ? 'rgba(22,163,74,0.3)' : 'rgba(0,0,0,0.08)',
          zIndex: 0
        }} />
      )}

      {/* Icon */}
      <Box sx={{ position: 'relative', zIndex: 1, flexShrink: 0, mt: 0.25 }}>
        {isCancelled ? (
          <CancelRoundedIcon sx={{ fontSize: 22, color: iconColor }} />
        ) : done ? (
          <CheckCircleRoundedIcon sx={{ fontSize: 22, color: iconColor }} />
        ) : (
          <RadioButtonUncheckedRoundedIcon sx={{ fontSize: 22, color: iconColor }} />
        )}
      </Box>

      {/* Label + time */}
      <Box sx={{ flex: 1, pb: isLast ? 0 : 0.5 }}>
        <Typography sx={{
          fontSize: '0.82rem',
          fontWeight: done ? 600 : 400,
          color: done ? 'primary.contrastText' : 'rgba(45,45,45,0.4)'
        }}>
          {label}
        </Typography>
        {time && (
          <Typography sx={{ fontSize: '0.72rem', color: 'rgba(45,45,45,0.45)', mt: 0.25 }}>
            {formatDate(time)}
          </Typography>
        )}
      </Box>
    </Box>
  )
}

export default OrderStatusTimeline
