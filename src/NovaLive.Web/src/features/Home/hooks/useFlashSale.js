import { useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { getActiveCampaignAPI, getFlashSaleItemsAPI } from '~/common/apis/services/flashSaleService'

export const useFlashSale = () => {
  const { data: campaignData, isLoading: campaignLoading } = useQuery({
    queryKey: ['flash-sale', 'active'],
    queryFn: getActiveCampaignAPI,
    staleTime: 30 * 1000,
    retry: false
  })

  const activeCampaign = useMemo(() => {
    if (!campaignData) return null
    if (Array.isArray(campaignData)) return campaignData[0] || null
    if (Array.isArray(campaignData?.items)) return campaignData.items[0] || null
    return campaignData
  }, [campaignData])

  const campaignId = activeCampaign?.id || activeCampaign?._id

  const { data: itemsRaw, isLoading: itemsLoading } = useQuery({
    queryKey: ['flash-sale', 'items', campaignId],
    queryFn: () => getFlashSaleItemsAPI(campaignId),
    enabled: !!campaignId,
    staleTime: 30 * 1000
  })

  // Map flash sale items to the shape HomeProductCard expects
  const flashSaleProducts = useMemo(() => {
    const list = Array.isArray(itemsRaw) ? itemsRaw : (itemsRaw?.items ?? [])
    return list.map(item => ({
      _id: item.spuId || item.spu_id || item.id || item._id,
      id: item.spuId || item.spu_id || item.id || item._id,
      spu_id: item.spuId || item.spu_id || item.id,
      sku_id: item.skuId || item.sku_id,
      spu_name: item.spuName || item.spu_name || item.name,
      name: item.spuName || item.spu_name || item.name,
      spu_thumb: item.thumbnailUrl || item.spu_thumb || '',
      thumbnailUrl: item.thumbnailUrl || item.spu_thumb || '',
      spu_price: item.originalPrice ?? item.spu_price ?? item.price ?? 0,
      original_price: item.originalPrice ?? item.spu_price ?? 0,
      flash_price: item.flashPrice ?? item.flashsale_price ?? 0,
      flashsale_price: item.flashPrice ?? item.flashsale_price ?? 0,
      spu_code: item.skuCode || item.spu_code || '',
      is_flash_sale: true,
      total_sold: item.soldQty ?? item.flashsale_sold ?? 0,
      flashsale_sold: item.soldQty ?? item.flashsale_sold ?? 0,
      flashsale_stock: item.quantity ?? item.flashsale_stock ?? 0,
      quantity: item.quantity ?? item.flashsale_stock ?? 0
    }))
  }, [itemsRaw])

  return {
    campaign: activeCampaign,
    campaigns: Array.isArray(campaignData) ? campaignData : (activeCampaign ? [activeCampaign] : []),
    flashSaleProducts,
    isLoading: campaignLoading || (!!campaignId && itemsLoading),
    hasActiveCampaign: !!campaignId
  }
}
