import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Alert,
  CircularProgress,
  InputAdornment,
  IconButton,
  Tabs,
  Tab,
  alpha,
} from '@mui/material';
import { Visibility, VisibilityOff } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useLoginMutation, useRegisterMutation } from '../api';
import { useAppDispatch } from '../hooks/useRedux';
import { setCredentials } from '../features/auth/authSlice';
import { Logo } from '../components/Logo';
import { LanguageSwitcher } from '../components/LanguageSwitcher';

export default function AuthPage() {
  const { t } = useTranslation();
  const [isLogin, setIsLogin] = useState(true);
  const [showPassword, setShowPassword] = useState(false);
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
  });
  const [error, setError] = useState('');

  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const [login, { isLoading: loginLoading }] = useLoginMutation();
  const [register, { isLoading: registerLoading }] = useRegisterMutation();

  const isLoading = loginLoading || registerLoading;

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      if (isLogin) {
        const result = await login({ email: formData.email, password: formData.password }).unwrap();
        dispatch(setCredentials({ user: result.user, token: result.token }));
        navigate('/');
      } else {
        const result = await register({
          email: formData.email,
          password: formData.password,
          firstName: formData.firstName,
          lastName: formData.lastName,
        }).unwrap();
        dispatch(setCredentials({ user: result.user, token: result.token }));
        navigate('/');
      }
    } catch (err: any) {
      const errorMessage = err.data?.message || err.data?.error || t('auth.registerError');
      
      if (err.status === 403 || (err.data && err.data.message && err.data.message.toLowerCase().includes('activate'))) {
        setError(t('auth.accountNotActivated'));
      } else {
        setError(errorMessage);
      }
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: (theme) => 
          theme.palette.mode === 'dark'
            ? 'linear-gradient(135deg, #0f172a 0%, #1e1b4b 100%)'
            : 'linear-gradient(135deg, #f8fafc 0%, #e0e7ff 100%)',
        padding: 2,
        position: 'relative',
        overflow: 'hidden',
        '&::before': {
          content: '""',
          position: 'absolute',
          top: '-50%',
          left: '-50%',
          width: '200%',
          height: '200%',
          background: (theme) => 
            theme.palette.mode === 'dark'
              ? 'radial-gradient(circle at 30% 30%, rgba(37, 99, 235, 0.15) 0%, transparent 50%), radial-gradient(circle at 70% 70%, rgba(59, 130, 246, 0.15) 0%, transparent 50%)'
              : 'radial-gradient(circle at 30% 30%, rgba(37, 99, 235, 0.2) 0%, transparent 50%), radial-gradient(circle at 70% 70%, rgba(59, 130, 246, 0.15) 0%, transparent 50%)',
          animation: 'float 20s ease-in-out infinite',
          '@keyframes float': {
            '0%, 100%': { transform: 'translate(0, 0)' },
            '50%': { transform: 'translate(-2%, -2%)' },
          },
        },
      }}
    >
      <Box sx={{ position: 'absolute', top: 16, right: 16, zIndex: 1 }}>
        <LanguageSwitcher />
      </Box>
      
      <Card 
        sx={{ 
          maxWidth: 480, 
          width: '100%', 
          position: 'relative',
          overflow: 'visible',
          boxShadow: 'none',
          bgcolor: 'background.paper',
          '&:hover': {
            boxShadow: 'none',
          },
          '& .MuiTextField-root': {
            '& .MuiOutlinedInput-root': {
              '&:hover': {
                '& .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'rgba(0, 0, 0, 0.23)',
                },
              },
              '&.Mui-focused': {
                '& .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'primary.main',
                },
              },
            },
          },
        }}
      >
        <CardContent sx={{ 
          p: 4,
          boxShadow: 'none',
          '&:hover': {
            boxShadow: 'none',
          },
          '& .MuiTextField-root': {
            '& .MuiOutlinedInput-root': {
              '&:hover': {
                backgroundColor: 'transparent',
                '& .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'rgba(0, 0, 0, 0.23)',
                },
              },
              '&.Mui-focused': {
                backgroundColor: 'transparent',
                '& .MuiOutlinedInput-notchedOutline': {
                  borderColor: 'primary.main',
                },
              },
            },
          },
          '& .MuiButtonBase-root': {
            '&:hover': {
              backgroundColor: 'transparent',
              boxShadow: 'none',
            },
            '&.MuiButtonBase-root:hover': {
              backgroundColor: 'transparent',
              boxShadow: 'none',
            },
          },
          '& .MuiTab-root': {
            '&:hover': {
              backgroundColor: 'transparent',
            },
          },
        }}>
          <Box sx={{ textAlign: 'center', mb: 4 }}>
            <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2 }}>
              <Logo />
            </Box>
            <Typography 
              variant="h5" 
              component="h1" 
              fontWeight="700"
              sx={{ 
                background: (theme) => 
                  theme.palette.mode === 'dark'
                    ? 'linear-gradient(135deg, #60a5fa 0%, #3b82f6 100%)'
                    : 'linear-gradient(135deg, #3b82f6 0%, #2563eb 100%)',
                WebkitBackgroundClip: 'text',
                WebkitTextFillColor: 'transparent',
              }}
            >
              {t('common.appName')}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {isLogin ? t('auth.loginSubtitle') : t('auth.registerSubtitle')}
            </Typography>
          </Box>

          <Tabs 
            value={isLogin ? 0 : 1} 
            onChange={(_, v) => { setIsLogin(v === 0); setError(''); }}
            variant="fullWidth"
            sx={{
              mb: 3,
              '& .MuiTab-root': {
                fontWeight: 600,
                textTransform: 'none',
              },
            }}
          >
            <Tab label={t('auth.signIn')} />
            <Tab label={t('auth.signUp')} />
          </Tabs>

          {error && (
            <Alert 
              severity="error" 
              sx={{ 
                mb: 3,
                '& .MuiAlert-message': { fontWeight: 500 }
              }}
            >
              {error}
            </Alert>
          )}

          <form onSubmit={handleSubmit}>
            {!isLogin && (
              <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
                <TextField
                  fullWidth
                  label={t('auth.firstName')}
                  name="firstName"
                  value={formData.firstName}
                  onChange={handleChange}
                  required={!isLogin}
                />
                <TextField
                  fullWidth
                  label={t('auth.lastName')}
                  name="lastName"
                  value={formData.lastName}
                  onChange={handleChange}
                  required={!isLogin}
                />
              </Box>
            )}

            <TextField
              fullWidth
              label={t('auth.email')}
              name="email"
              type="email"
              value={formData.email}
              onChange={handleChange}
              required
              sx={{ mb: 2 }}
            />

            <TextField
              fullWidth
              label={t('auth.password')}
              name="password"
              type={showPassword ? 'text' : 'password'}
              value={formData.password}
              onChange={handleChange}
              required
              sx={{ mb: 3 }}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                      {showPassword ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />

            <Button
              type="submit"
              fullWidth
              variant="contained"
              size="large"
              disabled={isLoading}
              sx={{
                mb: 2,
                py: 1.5,
                background: 'linear-gradient(135deg, #2563eb 0%, #3b82f6 100%)',
                '&:hover': {
                  background: 'linear-gradient(135deg, #2563eb 0%, #3b82f6 100%)',
                },
                '&.MuiButtonBase-root:hover': {
                  bgcolor: 'transparent',
                },
              }}
            >
              {isLoading ? <CircularProgress size={24} color="inherit" /> : isLogin ? t('auth.login') : t('auth.register')}
            </Button>

            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="body2" color="text.secondary">
                {isLogin ? t('auth.noAccount') : t('auth.hasAccount')}
                <Button
                  variant="text"
                  size="small"
                  onClick={() => {
                    setIsLogin(!isLogin);
                    setError('');
                  }}
                  sx={{ fontWeight: 600 }}
                >
                  {isLogin ? t('auth.signUp') : t('auth.signIn')}
                </Button>
              </Typography>
            </Box>
          </form>
        </CardContent>
      </Card>
    </Box>
  );
}
