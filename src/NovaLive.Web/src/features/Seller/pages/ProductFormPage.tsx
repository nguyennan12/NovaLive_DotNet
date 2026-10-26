import { useParams } from 'react-router-dom'
import { useProduct } from '../hooks/useProducts'
import { PageHeader } from '../components/shared/PageHeader'
import { QueryState } from '../components/shared/QueryState'
import { ProductEditor } from '../components/products/ProductEditor'

export default function ProductFormPage() {
  const { id } = useParams()
  const query = useProduct(id)
  return <><PageHeader title={id ? 'Chỉnh sửa sản phẩm' : 'Thêm sản phẩm'} description='Thông tin SPU và ma trận SKU của cửa hàng.' />{id ? <QueryState loading={query.isLoading} error={query.error} onRetry={query.refetch}>{query.data && <ProductEditor key={query.data.id} initial={query.data} />}</QueryState> : <ProductEditor />}</>
}
