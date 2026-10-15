import { useQuery } from '@tanstack/react-query'
import { getDistrictsAPI, getProvincesAPI, getWardsAPI } from '~/common/apis/services/shippingService'

export const useProvinces = () => {
  const { data: provinces = [], isLoading: isLoadingProvinces } = useQuery({
    queryKey: ['ghn_provinces'],
    queryFn: getProvincesAPI,
    staleTime: Infinity
  })
  return { provinces, isLoadingProvinces }
}

export const useDistricts = (provinceId) => {
  const { data: districts = [], isLoading: isLoadingDistricts } = useQuery({
    queryKey: ['ghn_districts', provinceId],
    queryFn: () => getDistrictsAPI(provinceId),
    enabled: !!provinceId,
    staleTime: Infinity
  })
  return { districts, isLoadingDistricts }
}

export const useWards = (districtId) => {
  const { data: wards = [], isLoading: isLoadingWards } = useQuery({
    queryKey: ['ghn_wards', districtId],
    queryFn: () => getWardsAPI(districtId),
    enabled: !!districtId,
    staleTime: Infinity
  })
  return { wards, isLoadingWards }
}
