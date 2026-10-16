import CssBaseline from '@mui/material/CssBaseline'
import { ThemeProvider } from '@mui/material/styles'
import { ConfirmProvider } from 'material-ui-confirm'
import ReactDOM from 'react-dom/client'
import { Provider } from 'react-redux'
import { BrowserRouter } from 'react-router-dom'
import { ToastContainer } from 'react-toastify'
import { persistStore } from 'redux-persist'
import { PersistGate } from 'redux-persist/integration/react'
import App from './App'
import { store } from './store/store'
import theme from './theme'
import { QueryClientProvider, QueryClient } from '@tanstack/react-query'
import ScrollToTop from './common/hooks/useScroll'


const persitor = persistStore(store)
const queryClient = new QueryClient()

ReactDOM.createRoot(document.getElementById('root')).render(

  <BrowserRouter basename='/'>
    <Provider store={store}>
      <QueryClientProvider client={queryClient}>
        <PersistGate loading={null} persistor={persitor}>
          <ThemeProvider theme={theme}>
            <ConfirmProvider>
              <CssBaseline />
              <ScrollToTop />
              <App />
              <ToastContainer autoClose={2000} theme="colored" />
            </ConfirmProvider>
          </ThemeProvider >
        </PersistGate>
      </QueryClientProvider>
    </Provider>
  </BrowserRouter>
)
