import { useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { getActiveCampaignAPI, getFlashSaleItemsAPI } from '~/common/apis/services/flashSaleService'

export const useFlashSale = () => {
  const { data: campaign, isLoading: campaignLoading } = useQuery({
    queryKey: ['flash-sale', 'active'],
    queryFn: getActiveCampaignAPI,
    staleTime: 30 * 1000,
    retry: false
  })

  const campaignId = campaign?._id
  const { data: itemsRaw, isLoading: itemsLoading } = useQuery({
    queryKey: ['flash-sale', 'items', campaignId],
    queryFn: () => getFlashSaleItemsAPI(campaignId),
    enabled: !!campaignId,
    staleTime: 30 * 1000
  })

  // Map flash sale items to the shape HomeProductCard expects
  const flashSaleProducts = useMemo(() => {
    const list = itemsRaw?.items ?? []
    return list.map(item => ({
      _id: item.spu_id,
      spu_name: item.spu_name,
      spu_thumb: item.spu_thumb,
      spu_price: item.original_price,
      flash_price: item.flashsale_price,
      spu_code: item.spu_code,
      is_flash_sale: true,
      total_sold: item.flashsale_sold,
      flashsale_stock: item.flashsale_stock,
      flashsale_sold: item.flashsale_sold
    }))
  }, [itemsRaw])

  return {
    campaign,
    flashSaleProducts,
    isLoading: campaignLoading || (!!campaignId && itemsLoading),
    hasActiveCampaign: !!campaign
  }
}
