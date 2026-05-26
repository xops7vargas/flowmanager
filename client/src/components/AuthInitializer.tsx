import { useEffect, useState } from 'react';
import { Box, CircularProgress } from '@mui/material';
import { useAppDispatch, useAppSelector } from '../hooks/useRedux';
import { setUser, logout } from '../features/auth/authSlice';
import { useGetCurrentUserQuery } from '../api';

export default function AuthInitializer({ children }: { children: React.ReactNode }) {
  const dispatch = useAppDispatch();
  const token = useAppSelector((state) => state.auth.token);
  const { data: user, isLoading, error } = useGetCurrentUserQuery(undefined, {
    skip: !token,
  });

  useEffect(() => {
    if (token && user) {
      dispatch(setUser(user));
    }
    if (token && error) {
      dispatch(logout());
    }
  }, [token, user, error, dispatch]);

  if (!token) {
    return <>{children}</>;
  }

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
        <CircularProgress />
      </Box>
    );
  }

  return <>{children}</>;
}
