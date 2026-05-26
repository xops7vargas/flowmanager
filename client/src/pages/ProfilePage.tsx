import { useState, useRef } from 'react';
import { Box, Card, CardContent, Typography, TextField, Button, Avatar, IconButton, Grid, Divider, Alert, Snackbar } from '@mui/material';
import { PhotoCamera, Save, Lock } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useGetCurrentUserQuery, useUpdateUserMutation } from '../api';

export default function ProfilePage() {
  const { t } = useTranslation();
  const { data: user, isLoading } = useGetCurrentUserQuery();
  const [updateUser] = useUpdateUserMutation();
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    phone: '',
    bio: '',
  });
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  if (user && formData.email === '') {
    setFormData({
      firstName: user.firstName || '',
      lastName: user.lastName || '',
      email: user.email || '',
      phone: (user as any).phone || '',
      bio: (user as any).bio || '',
    });
    setAvatarPreview(user.avatar || null);
  }

  const handleAvatarClick = () => {
    fileInputRef.current?.click();
  };

  const handleAvatarChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatarPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSubmit = async () => {
    try {
      await updateUser({
        id: user?.id,
        firstName: formData.firstName,
        lastName: formData.lastName,
        phone: formData.phone,
        bio: formData.bio,
        avatar: avatarPreview || undefined,
      } as any).unwrap();
      setSnackbar({ open: true, message: t('profile.updateSuccess'), severity: 'success' });
    } catch {
      setSnackbar({ open: true, message: t('profile.updateError'), severity: 'error' });
    }
  };

  if (isLoading) {
    return <Box sx={{ p: 3 }}>{t('common.loading')}</Box>;
  }

  return (
    <Box sx={{ p: 3, maxWidth: 800, mx: 'auto' }}>
      <Typography variant="h4" gutterBottom>{t('profile.title')}</Typography>
      
      <Card sx={{ mt: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
            <Box sx={{ position: 'relative' }}>
              <Avatar
                src={avatarPreview || undefined}
                sx={{ width: 120, height: 120, fontSize: 48, bgcolor: 'primary.main' }}
              >
                {formData.firstName?.[0]}{formData.lastName?.[0]}
              </Avatar>
              <IconButton
                sx={{
                  position: 'absolute',
                  bottom: 0,
                  right: 0,
                  bgcolor: 'primary.main',
                  color: 'white',
                  '&:hover': { bgcolor: 'primary.dark' },
                }}
                onClick={handleAvatarClick}
              >
                <PhotoCamera />
              </IconButton>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                style={{ display: 'none' }}
                onChange={handleAvatarChange}
              />
            </Box>
            <Box sx={{ ml: 3 }}>
              <Typography variant="h5">{formData.firstName} {formData.lastName}</Typography>
              <Typography variant="body2" color="text.secondary">{formData.email}</Typography>
            </Box>
          </Box>

          <Divider sx={{ my: 2 }} />

          <Grid container spacing={3}>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label={t('profile.firstName')}
                value={formData.firstName}
                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label={t('profile.lastName')}
                value={formData.lastName}
                onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label={t('profile.email')}
                value={formData.email}
                disabled
                helperText={t('profile.emailCannotChange')}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label={t('profile.phone')}
                value={formData.phone}
                onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label={t('profile.bio')}
                value={formData.bio}
                onChange={(e) => setFormData({ ...formData, bio: e.target.value })}
                multiline
                rows={3}
                placeholder={t('profile.bioPlaceholder')}
              />
            </Grid>
          </Grid>

          <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
            <Button
              variant="contained"
              startIcon={<Save />}
              onClick={handleSubmit}
            >
              {t('common.save')}
            </Button>
            <Button
              variant="outlined"
              startIcon={<Lock />}
              onClick={() => {}}
            >
              {t('profile.changePassword')}
            </Button>
          </Box>
        </CardContent>
      </Card>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar({ ...snackbar, open: false })}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
