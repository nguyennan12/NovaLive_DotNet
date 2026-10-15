import { combineReducers, configureStore } from '@reduxjs/toolkit'
import { persistReducer } from 'redux-persist'
import createWebStorage from 'redux-persist/es/storage/createWebStorage'
import { CartReducer } from './cart/cartSlice'
import { CategoryReducer } from './product/categorySlice'
import { DiscountReducer } from './discount/discountSlice'
import { UserReducer } from './user/userSlice'

const storage = createWebStorage('local')
const rootPersistConfig = {
  key: 'root',
  storage,
  whitelist: ['user', 'category', 'cart', 'discount']
}

const reducers = combineReducers({
  user: UserReducer,
  category: CategoryReducer,
  cart: CartReducer,
  discount: DiscountReducer
})

const persistedReducer = persistReducer(rootPersistConfig, reducers)

export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) => getDefaultMiddleware({ serializableCheck: false })
})
