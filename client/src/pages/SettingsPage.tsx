import { useState, useEffect } from 'react';
import { Box, Typography, Card, CardContent, TextField, Button, Switch, FormControlLabel, Grid, Snackbar, Alert } from '@mui/material';
import { Save } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useGetSettingsQuery, useUpdateSettingMutation } from '../api';

export default function SettingsPage() {
  const { t } = useTranslation();
  const { data: settings, isLoading, refetch } = useGetSettingsQuery();
  const [updateSetting] = useUpdateSettingMutation();
  const [localSettings, setLocalSettings] = useState<Record<string, string>>({});
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  useEffect(() => {
    if (settings) {
      const obj: Record<string, string> = {};
      settings.forEach((s: any) => {
        obj[s.key] = s.value;
      });
      setLocalSettings(obj);
    }
  }, [settings]);

  const handleChange = (key: string, value: string) => {
    setLocalSettings({ ...localSettings, [key]: value });
  };

  const handleSave = async (key: string) => {
    try {
      await updateSetting({ key, value: localSettings[key] }).unwrap();
      setSnackbar({ open: true, message: t('settings.saved'), severity: 'success' });
      refetch();
    } catch {
      setSnackbar({ open: true, message: t('settings.error'), severity: 'error' });
    }
  };

  if (isLoading) {
    return <Box sx={{ p: 3 }}>{t('common.loading')}</Box>;
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>{t('nav.settings')}</Typography>

      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>{t('settings.general')}</Typography>
              
              <TextField
                fullWidth
                label={t('settings.companyName')}
                value={localSettings['CompanyName'] || ''}
                onChange={(e) => handleChange('CompanyName', e.target.value)}
                sx={{ mb: 2 }}
              />
              
              <TextField
                fullWidth
                label={t('settings.language')}
                select
                SelectProps={{ native: true }}
                value={localSettings['Language'] || 'es'}
                onChange={(e) => handleChange('Language', e.target.value)}
                sx={{ mb: 2 }}
              >
                <option value="es">Español</option>
                <option value="en">English</option>
              </TextField>

              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings['AllowRegistration'] === 'true'}
                    onChange={(e) => handleChange('AllowRegistration', e.target.checked ? 'true' : 'false')}
                  />
                }
                label={t('settings.allowRegistration')}
              />
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>{t('settings.security')}</Typography>
              
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings['RequireEmailVerification'] === 'true'}
                    onChange={(e) => handleChange('RequireEmailVerification', e.target.checked ? 'true' : 'false')}
                  />
                }
                label={t('settings.requireEmailVerification')}
              />

              <Box sx={{ mt: 2 }}>
                <TextField
                  fullWidth
                  label={t('settings.defaultUserRole')}
                  select
                  SelectProps={{ native: true }}
                  value={localSettings['DefaultUserRole'] || 'User'}
                  onChange={(e) => handleChange('DefaultUserRole', e.target.value)}
                >
                  <option value="User">User</option>
                  <option value="Developer">Developer</option>
                  <option value="ProjectManager">Project Manager</option>
                </TextField>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>{t('settings.appearance')}</Typography>
              
              <FormControlLabel
                control={
                  <Switch
                    checked={localSettings['Theme'] === 'dark'}
                    onChange={(e) => handleChange('Theme', e.target.checked ? 'dark' : 'light')}
                  />
                }
                label={t('settings.darkMode')}
              />
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Snackbar open={snackbar.open} autoHideDuration={3000} onClose={() => setSnackbar({ ...snackbar, open: false })}>
        <Alert severity={snackbar.severity}>{snackbar.message}</Alert>
      </Snackbar>
    </Box>
  );
}
