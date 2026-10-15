import { Box, Table, TableBody, TableCell, TableRow } from '@mui/material'
import { SectionHeader } from '../shared/SectionHeader'

const ProductAttributesTable = ({ attributes }) => {
  if (!attributes?.length) return null

  return (
    <Box>
      <SectionHeader title="Chi tiết sản phẩm" />
      <Table size="small" sx={{
        '& .MuiTableCell-root': { borderColor: '#f3f4f6', py: 1.25 }
      }}>
        <TableBody>
          {attributes.map((attr, i) => (
            <TableRow key={attr.attr_id ?? i} sx={{
              bgcolor: i % 2 === 0 ? 'rgba(52,133,247,0.04)' : 'transparent'
            }}>
              <TableCell sx={{
                width: '40%',
                fontSize: '0.8rem',
                color: '#6f97ddff',
                fontWeight: 600
              }}>
                {attr.attr_name}
              </TableCell>
              <TableCell sx={{
                fontSize: '0.85rem',
                color: 'primary.contrastText',
                fontWeight: 500
              }}>
                {attr.attr_value}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Box>
  )
}

export default ProductAttributesTable
