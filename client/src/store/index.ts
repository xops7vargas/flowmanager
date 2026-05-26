import { configureStore } from '@reduxjs/toolkit';
import { api } from '../api';
import authSlice from '../features/auth/authSlice';
import themeSlice from '../features/theme/themeSlice';

export const store = configureStore({
  reducer: {
    [api.reducerPath]: api.reducer,
    auth: authSlice,
    theme: themeSlice,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(api.middleware),
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
