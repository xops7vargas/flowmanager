import React from 'react';
import ReactDOM from 'react-dom/client';
import { Provider } from 'react-redux';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { store } from './store';
import { createAppTheme } from './theme';
import { useAppSelector } from './hooks/useRedux';
import './i18n';
import App from './App';

const ThemedApp = () => {
  const mode = useAppSelector((state) => state.theme.mode);
  const theme = React.useMemo(() => createAppTheme(mode), [mode]);
  
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <App />
    </ThemeProvider>
  );
};

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <Provider store={store}>
      <ThemedApp />
    </Provider>
  </React.StrictMode>
);
