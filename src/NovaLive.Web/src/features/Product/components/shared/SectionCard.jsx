import { Paper } from '@mui/material'

const SectionCard = (props) => {
  return (
    <Paper
      elevation={0}
      {...props}
      sx={{
        borderRadius: 4,
        border: '1px solid',
        borderColor: 'var(--mui-palette-divider)',
        p: 2.5,
        ...props.sx
      }}
    />
  )
}

export default SectionCard