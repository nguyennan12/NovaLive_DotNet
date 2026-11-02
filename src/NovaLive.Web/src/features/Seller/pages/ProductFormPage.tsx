import { useParams } from 'react-router-dom'
import { useProduct } from '../hooks/useProducts'
import { QueryState } from '../components/shared/QueryState'
import { ProductEditor } from '../components/products/ProductEditor'

export default function ProductFormPage() {
  const { id } = useParams()
  const query = useProduct(id)
  if (id) {
    return (
      <QueryState loading={query.isLoading} error={query.error} onRetry={query.refetch}>
        {query.data && <ProductEditor key={query.data.id} initial={query.data} />}
      </QueryState>
    )
  }
  return <ProductEditor />
}
