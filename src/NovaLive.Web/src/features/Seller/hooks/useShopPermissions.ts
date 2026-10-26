import { createContext, useContext } from 'react'

export const SellerPermissionsContext = createContext({ canOperate: false })
export const useShopPermissions = () => useContext(SellerPermissionsContext)
