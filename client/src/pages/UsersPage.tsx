import { useState } from 'react';
import { Box, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, IconButton, Chip, Avatar, TextField, InputAdornment, Pagination, Button, Dialog, DialogTitle, DialogContent, DialogActions, FormControl, InputLabel, Select, MenuItem, FormControlLabel, Checkbox, Snackbar, Alert } from '@mui/material';
import { Search, Add, Edit, Delete, Block, CheckCircle } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useGetUsersQuery, useActivateUserMutation, useDeactivateUserMutation, useRegisterMutation, useGetRolesQuery, useUpdateUserMutation, useUpdateUserRoleMutation } from '../api';
import { User } from '../types';

export default function UsersPage() {
  const { t } = useTranslation();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const { data, isLoading } = useGetUsersQuery({ page, pageSize: 10, search: search || undefined });
  const { data: roles } = useGetRolesQuery();
  const [activateUser] = useActivateUserMutation();
  const [deactivateUser] = useDeactivateUserMutation();
  const [register] = useRegisterMutation();
  const [updateUser] = useUpdateUserMutation();
  const [updateUserRole] = useUpdateUserRoleMutation();

  const [openDialog, setOpenDialog] = useState(false);
  const [editUser, setEditUser] = useState<User | null>(null);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({ open: false, message: '', severity: 'success' });
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    roleId: '',
    isActive: true
  });

  const handleToggleStatus = async (user: User) => {
    try {
      if (user.isActive) {
        await deactivateUser(user.id).unwrap();
      } else {
        await activateUser(user.id).unwrap();
      }
    } catch (error) {
      console.error(error);
    }
  };

  const handleOpenDialog = (user?: User) => {
    if (user) {
      setEditUser(user);
      const currentRoleName = user.roles?.[0];
      const existingRole = roles?.find(r => r.name === currentRoleName);
      setFormData({
        email: user.email,
        password: '',
        firstName: user.firstName,
        lastName: user.lastName,
        roleId: existingRole?.id || currentRoleName || roles?.[0]?.id || '',
        isActive: user.isActive
      });
    } else {
      setEditUser(null);
      setFormData({ email: '', password: '', firstName: '', lastName: '', roleId: roles?.[0]?.id || '', isActive: true });
    }
    setOpenDialog(true);
  };

  const handleSave = async () => {
    try {
      if (editUser) {
        await updateUser({
          id: editUser.id,
          firstName: formData.firstName,
          lastName: formData.lastName,
          isActive: formData.isActive
        }).unwrap();
        
        if (formData.roleId && formData.roleId !== '') {
          await updateUserRole({
            id: editUser.id,
            roleId: formData.roleId
          }).unwrap();
        }
        
        setOpenDialog(false);
        setSnackbar({ open: true, message: t('users.updateSuccess'), severity: 'success' });
      } else {
        const selectedRole = roles?.find(r => r.id === formData.roleId);
        await register({
          email: formData.email,
          password: formData.password,
          firstName: formData.firstName,
          lastName: formData.lastName,
          role: selectedRole?.name
        }).unwrap();
        setOpenDialog(false);
        setPage(1);
        setSnackbar({ open: true, message: t('users.createSuccess'), severity: 'success' });
      }
    } catch (error: any) {
      console.error(error);
      setSnackbar({ open: true, message: error?.data?.message || t('common.error'), severity: 'error' });
    }
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">{t('nav.users')}</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => handleOpenDialog()}>
          {t('users.create')}
        </Button>
      </Box>

      <TextField
        placeholder={t('common.search') + '...'}
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        sx={{ mb: 3, width: 300 }}
        InputProps={{
          startAdornment: <InputAdornment position="start"><Search /></InputAdornment>,
        }}
      />

      {isLoading ? (
        <Typography>{t('common.loading')}</Typography>
      ) : (
        <>
          <TableContainer component={Paper}>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>{t('users.avatar')}</TableCell>
                  <TableCell>{t('users.name')}</TableCell>
                  <TableCell>{t('users.email')}</TableCell>
                  <TableCell>{t('users.status')}</TableCell>
                  <TableCell>{t('users.createdAt')}</TableCell>
                  <TableCell align="right">{t('common.actions')}</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data?.items.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell>
                      <Avatar src={user.avatar || undefined}>
                        {user.firstName?.[0]}{user.lastName?.[0]}
                      </Avatar>
                    </TableCell>
                    <TableCell>{user.firstName} {user.lastName}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>
                      <Chip 
                        label={user.isActive ? t('users.active') : t('users.inactive')} 
                        color={user.isActive ? 'success' : 'error'} 
                        size="small" 
                      />
                    </TableCell>
                    <TableCell>
                      {user.roles?.map(role => (
                        <Chip key={role} label={role} size="small" sx={{ mr: 0.5 }} />
                      ))}
                    </TableCell>
                    <TableCell>{new Date(user.createdAt).toLocaleDateString()}</TableCell>
                    <TableCell align="right">
                      <IconButton size="small" onClick={() => handleOpenDialog(user)}>
                        <Edit />
                      </IconButton>
                      <IconButton size="small" onClick={() => handleToggleStatus(user)}>
                        {user.isActive ? <Block /> : <CheckCircle />}
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>

          {data && data.totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
              <Pagination count={data.totalPages} page={page} onChange={(_, p) => setPage(p)} />
            </Box>
          )}
        </>
      )}

      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editUser ? t('users.edit') : t('users.create')}</DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            label={t('users.firstName')}
            value={formData.firstName}
            onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
            sx={{ mt: 2, mb: 2 }}
          />
          <TextField
            fullWidth
            label={t('users.lastName')}
            value={formData.lastName}
            onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
            sx={{ mb: 2 }}
          />
          <TextField
            fullWidth
            label={t('users.email')}
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            disabled={!!editUser}
            sx={{ mb: 2 }}
          />
          {!editUser && (
            <TextField
              fullWidth
              label={t('users.password')}
              type="password"
              value={formData.password}
              onChange={(e) => setFormData({ ...formData, password: e.target.value })}
              sx={{ mb: 2 }}
            />
          )}
          <FormControl fullWidth sx={{ mb: 2 }}>
            <InputLabel>{t('users.role')}</InputLabel>
            <Select
              value={formData.roleId}
              label={t('users.role')}
              onChange={(e) => setFormData({ ...formData, roleId: e.target.value })}
            >
              {roles?.map(role => (
                <MenuItem key={role.id} value={role.id}>{role.name}</MenuItem>
              ))}
            </Select>
          </FormControl>
          {editUser && (
            <FormControlLabel
              control={
                <Checkbox
                  checked={formData.isActive}
                  onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                />
              }
              label={t('users.active')}
            />
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>{t('common.cancel')}</Button>
          <Button onClick={handleSave} variant="contained">{t('common.save')}</Button>
        </DialogActions>
      </Dialog>
      <Snackbar open={snackbar.open} autoHideDuration={6000} onClose={() => setSnackbar({ ...snackbar, open: false })}>
        <Alert onClose={() => setSnackbar({ ...snackbar, open: false })} severity={snackbar.severity} sx={{ width: '100%' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
