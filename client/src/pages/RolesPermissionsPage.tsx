import { useState, useEffect } from 'react';
import { Box, Typography, Card, CardContent, Grid, FormControl, InputLabel, Select, MenuItem, Chip, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Button, Snackbar, Alert } from '@mui/material';
import { Save } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useGetRolesQuery, useGetPermissionsQuery, useUpdateRolePermissionsMutation } from '../api';

interface RolePermission {
  permissionId: string;
  granted: boolean;
}

export default function RolesPermissionsPage() {
  const { t } = useTranslation();
  const [selectedRole, setSelectedRole] = useState<string>('');
  const [selectedPermissions, setSelectedPermissions] = useState<RolePermission[]>([]);
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const { data: roles, isLoading: rolesLoading } = useGetRolesQuery();
  const { data: permissions, isLoading: permissionsLoading } = useGetPermissionsQuery();
  const [updatePermissions] = useUpdateRolePermissionsMutation();

  const rolePermissionsData = roles?.find(r => r.id === selectedRole)?.permissions || [];

  useEffect(() => {
    if (selectedRole && permissions && roles) {
      const role = roles.find(r => r.id === selectedRole);
      const rolePerms = role?.permissions || [];
      
      const perms: RolePermission[] = permissions.map(p => ({
        permissionId: p.id,
        granted: rolePerms.some(rp => rp.id === p.id)
      }));
      setSelectedPermissions(perms);
    }
  }, [selectedRole, permissions, roles]);

  const handleTogglePermission = (permissionId: string) => {
    setSelectedPermissions(prev => 
      prev.map(p => 
        p.permissionId === permissionId 
          ? { ...p, granted: !p.granted } 
          : p
      )
    );
  };

  const handleSave = async () => {
    try {
      const grantedPermissions = selectedPermissions
        .filter(p => p.granted)
        .map(p => {
          const perm = permissions?.find(perm => perm.id === p.permissionId);
          return perm?.name || '';
        })
        .filter(name => name !== '');
      
      await updatePermissions({ 
        roleId: selectedRole, 
        permissions: grantedPermissions 
      }).unwrap();
      
      setSnackbar({ open: true, message: t('users.permissionsSaved'), severity: 'success' });
    } catch (error) {
      setSnackbar({ open: true, message: t('users.permissionsError'), severity: 'error' });
    }
  };

  const modules = permissions?.reduce((acc, p) => {
    if (!acc[p.module]) acc[p.module] = [];
    acc[p.module].push(p);
    return acc;
  }, {} as Record<string, typeof permissions>) || {};

  const moduleLabels: Record<string, string> = {
    User: 'Usuarios',
    Project: 'Proyectos',
    Task: 'Tareas',
    Financial: 'Financiero',
    Resource: 'Recursos',
    Chat: 'Chat',
    Settings: 'Configuración',
    Report: 'Reportes',
    Analytics: 'Analíticas',
    Dashboard: 'Panel',
    Notification: 'Notificaciones',
    Workflow: 'Flujo de Trabajo',
    Role: 'Roles',
    Permission: 'Permisos'
  };

  if (rolesLoading || permissionsLoading) {
    return <Box sx={{ p: 3 }}>{t('common.loading')}</Box>;
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>Administración de Roles y Permisos</Typography>

      <Grid container spacing={3}>
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>Seleccionar Rol</Typography>
              <FormControl fullWidth>
                <InputLabel>Rol</InputLabel>
                <Select
                  value={selectedRole}
                  label="Rol"
                  onChange={(e) => setSelectedRole(e.target.value)}
                >
                  {roles?.map(role => (
                    <MenuItem key={role.id} value={role.id}>
                      {role.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              {selectedRole && (
                <Box sx={{ mt: 2 }}>
                  <Typography variant="body2" color="text.secondary">
                    {roles?.find(r => r.id === selectedRole)?.description || 'Sin descripción'}
                  </Typography>
                  <Box sx={{ mt: 2 }}>
                    <Chip 
                      label={roles?.find(r => r.id === selectedRole)?.isSystem ? 'Rol del Sistema' : 'Rol Personalizado'} 
                      color={roles?.find(r => r.id === selectedRole)?.isSystem ? 'primary' : 'default'} 
                      size="small" 
                    />
                  </Box>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={8}>
          {selectedRole ? (
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                  <Typography variant="h6">Permisos</Typography>
                  <Button 
                    variant="contained" 
                    startIcon={<Save />} 
                    onClick={handleSave}
                  >
                    {t('common.save')}
                  </Button>
                </Box>

                {Object.entries(modules).map(([module, perms]) => (
                  <Box key={module} sx={{ mb: 3 }}>
                    <Typography variant="subtitle1" fontWeight="bold" sx={{ mb: 1, color: 'primary.main' }}>
                      {moduleLabels[module] || module}
                    </Typography>
                    <TableContainer component={Paper} variant="outlined">
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Permiso</TableCell>
                            <TableCell>Descripción</TableCell>
                            <TableCell align="center">Estado</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {perms.map(perm => (
                            <TableRow key={perm.id}>
                              <TableCell>{perm.name}</TableCell>
                              <TableCell>{perm.description || '-'}</TableCell>
                              <TableCell align="center">
                                <Chip 
                                  label={selectedPermissions.find(p => p.permissionId === perm.id)?.granted ? '✓' : '✗'}
                                  color={selectedPermissions.find(p => p.permissionId === perm.id)?.granted ? 'success' : 'default'}
                                  onClick={() => handleTogglePermission(perm.id)}
                                  clickable
                                  size="small"
                                />
                              </TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  </Box>
                ))}
              </CardContent>
            </Card>
          ) : (
            <Card>
              <CardContent>
                <Typography variant="body1" color="text.secondary" align="center">
                  Selecciona un rol para ver y configurar sus permisos
                </Typography>
              </CardContent>
            </Card>
          )}
        </Grid>
      </Grid>

      <Snackbar open={snackbar.open} autoHideDuration={3000} onClose={() => setSnackbar({ ...snackbar, open: false })}>
        <Alert severity={snackbar.severity}>{snackbar.message}</Alert>
      </Snackbar>
    </Box>
  );
}
