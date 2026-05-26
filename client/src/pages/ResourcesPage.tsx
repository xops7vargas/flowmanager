import React, { useState } from 'react';
import {
  Box, Card, CardContent, Typography, Grid, TextField, Button,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Chip, Dialog, DialogTitle, DialogContent, DialogActions,
  FormControl, InputLabel, Select, MenuItem, IconButton, alpha,
  Pagination, Avatar
} from '@mui/material';
import { Add, Inventory, Person, LocationOn, Edit } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useGetResourcesQuery, useCreateResourceMutation } from '../api';
import { ResourceType, ResourceStatus } from '../types';

const resourceTypeLabels: Record<ResourceType, string> = {
  [ResourceType.Equipment]: 'equipment',
  [ResourceType.Furniture]: 'furniture',
  [ResourceType.Electronics]: 'electronics',
  [ResourceType.Vehicles]: 'vehicles',
  [ResourceType.Tools]: 'tools',
  [ResourceType.OfficeSupplies]: 'officeSupplies',
  [ResourceType.Other]: 'other'
};

const resourceStatusColors: Record<ResourceStatus, string> = {
  [ResourceStatus.Available]: '#4caf50',
  [ResourceStatus.InUse]: '#2196f3',
  [ResourceStatus.Damaged]: '#f44336',
  [ResourceStatus.UnderMaintenance]: '#ff9800',
  [ResourceStatus.Retired]: '#9e9e9e'
};

export default function ResourcesPage() {
  const { t } = useTranslation();
  const [page, setPage] = useState(1);
  const [type, setType] = useState<number | undefined>();
  const [status, setStatus] = useState<number | undefined>();
  const [search, setSearch] = useState('');
  const [openDialog, setOpenDialog] = useState(false);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    code: '',
    type: ResourceType.Equipment,
    quantity: 1,
    unitValue: 0,
    location: ''
  });

  const { data: resources, isLoading } = useGetResourcesQuery({ page, pageSize: 10, type, status, search: search || undefined });
  const [createResource] = useCreateResourceMutation();

  const handleSubmit = async () => {
    await createResource(formData);
    setOpenDialog(false);
    setFormData({
      name: '',
      description: '',
      code: '',
      type: ResourceType.Equipment,
      quantity: 1,
      unitValue: 0,
      location: ''
    });
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">{t('resources.title')}</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => setOpenDialog(true)}>
          {t('resources.newResource')}
        </Button>
      </Box>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                size="small"
                placeholder={t('common.search')}
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth size="small">
                <InputLabel>{t('resources.type')}</InputLabel>
                <Select value={type ?? ''} label={t('resources.type')} onChange={(e) => setType(e.target.value === '' ? undefined : Number(e.target.value))}>
                  <MenuItem value="">{t('common.all')}</MenuItem>
                  {Object.entries(ResourceType).filter(([k]) => isNaN(Number(k))).map(([key, value]) => (
                    <MenuItem key={value} value={value}>{t(`resources.types.${resourceTypeLabels[value as ResourceType]}`)}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth size="small">
                <InputLabel>{t('common.status')}</InputLabel>
                <Select value={status ?? ''} label={t('common.status')} onChange={(e) => setStatus(e.target.value === '' ? undefined : Number(e.target.value))}>
                  <MenuItem value="">{t('common.all')}</MenuItem>
                  {Object.entries(ResourceStatus).filter(([k]) => isNaN(Number(k))).map(([key, value]) => (
                    <MenuItem key={value} value={value}>{t(`resources.statuses.${key}`)}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Grid container spacing={3}>
        {resources?.items.map((resource) => (
          <Grid item xs={12} md={6} lg={4} key={resource.id}>
            <Card sx={{ height: '100%' }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
                  <Avatar sx={{ bgcolor: resourceStatusColors[resource.status], width: 56, height: 56 }}>
                    <Inventory />
                  </Avatar>
                  <Box sx={{ flex: 1 }}>
                    <Typography variant="h6">{resource.name}</Typography>
                    <Typography variant="body2" color="text.secondary">{resource.code}</Typography>
                    <Box sx={{ mt: 1, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                      <Chip 
                        label={t(`resources.types.${resourceTypeLabels[resource.type]}`)}
                        size="small" 
                        variant="outlined"
                      />
                      <Chip 
                        label={t(`resources.statuses.${ResourceStatus[resource.status]}`)}
                        size="small"
                        sx={{ bgcolor: alpha(resourceStatusColors[resource.status], 0.1), color: resourceStatusColors[resource.status] }}
                      />
                    </Box>
                  </Box>
                </Box>
                <Box sx={{ mt: 2 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                    <Typography variant="body2" color="text.secondary">
                      {t('resources.available')}: {resource.availableQuantity} / {resource.quantity}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      ${resource.unitValue}
                    </Typography>
                  </Box>
                  {resource.assignedToName && (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}>
                      <Person fontSize="small" color="action" />
                      <Typography variant="body2">{resource.assignedToName}</Typography>
                    </Box>
                  )}
                  {resource.location && (
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}>
                      <LocationOn fontSize="small" color="action" />
                      <Typography variant="body2">{resource.location}</Typography>
                    </Box>
                  )}
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Box sx={{ mt: 3, display: 'flex', justifyContent: 'center' }}>
        <Pagination 
          count={resources?.totalPages || 1} 
          page={page} 
          onChange={(_, p) => setPage(p)}
        />
      </Box>

      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{t('resources.newResource')}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <TextField fullWidth label={t('resources.name')} value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField fullWidth label={t('resources.code')} value={formData.code} onChange={(e) => setFormData({ ...formData, code: e.target.value })} />
            </Grid>
            <Grid item xs={12}>
              <TextField fullWidth label={t('resources.description')} multiline rows={2} value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <FormControl fullWidth>
                <InputLabel>{t('resources.type')}</InputLabel>
                <Select value={formData.type} label={t('resources.type')} onChange={(e) => setFormData({ ...formData, type: e.target.value as ResourceType })}>
                  {Object.entries(ResourceType).filter(([k]) => isNaN(Number(k))).map(([key, value]) => (
                    <MenuItem key={value} value={value}>{t(`resources.types.${resourceTypeLabels[value as ResourceType]}`)}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField fullWidth label={t('resources.location')} value={formData.location} onChange={(e) => setFormData({ ...formData, location: e.target.value })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField fullWidth label={t('resources.quantity')} type="number" value={formData.quantity} onChange={(e) => setFormData({ ...formData, quantity: parseInt(e.target.value) })} />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField fullWidth label={t('resources.unitValue')} type="number" value={formData.unitValue} onChange={(e) => setFormData({ ...formData, unitValue: parseFloat(e.target.value) })} />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDialog(false)}>{t('common.cancel')}</Button>
          <Button variant="contained" onClick={handleSubmit}>{t('common.save')}</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
