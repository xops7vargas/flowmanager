import React, { useState } from 'react';
import {
  Box, Typography, Button, Card, CardContent, Grid, TextField,
  Dialog, DialogTitle, DialogContent, DialogActions, Chip,
  IconButton, Menu, MenuItem, InputAdornment, Pagination
} from '@mui/material';
import { Add, Search, MoreVert, Edit, Delete } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useGetProjectsQuery, useCreateProjectMutation, useDeleteProjectMutation, useUpdateProjectMutation } from '../api';
import { ProjectStatus } from '../types';

const statusColors: Record<ProjectStatus, string> = {
  [ProjectStatus.Planning]: '#9e9e9e',
  [ProjectStatus.InProgress]: '#2196f3',
  [ProjectStatus.OnHold]: '#ff9800',
  [ProjectStatus.AtRisk]: '#f44336',
  [ProjectStatus.Delayed]: '#f44336',
  [ProjectStatus.Completed]: '#4caf50',
  [ProjectStatus.Cancelled]: '#757575',
};

export default function ProjectsPage() {
  const { t } = useTranslation();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [open, setOpen] = useState(false);
  const [editProject, setEditProject] = useState<any>(null);
  const [formData, setFormData] = useState({ name: '', description: '', startDate: '', endDate: '', budget: '' });
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [selectedProject, setSelectedProject] = useState<any>(null);

  const { data, isLoading } = useGetProjectsQuery({ page, pageSize: 12, status: undefined });
  const [createProject] = useCreateProjectMutation();
  const [updateProject] = useUpdateProjectMutation();
  const [deleteProject] = useDeleteProjectMutation();

  const handleSubmit = async () => {
    try {
      const projectData = {
        ...formData,
        budget: formData.budget ? parseFloat(formData.budget) : undefined,
        startDate: formData.startDate ? formData.startDate : undefined,
        endDate: formData.endDate ? formData.endDate : undefined,
      };
      if (editProject) {
        await updateProject({ id: editProject.id, body: projectData });
      } else {
        await createProject(projectData);
      }
      setOpen(false);
      setEditProject(null);
      setFormData({ name: '', description: '', startDate: '', endDate: '', budget: '' });
    } catch (error) {
      console.error(error);
    }
  };

  const handleDelete = async (id: string) => {
    if (confirm(t('projects.confirmDelete'))) {
      await deleteProject(id);
    }
    setAnchorEl(null);
  };

  const getStatusLabel = (status: ProjectStatus) => {
    const labels: Record<ProjectStatus, string> = {
      [ProjectStatus.Planning]: t('projects.statuses.active'),
      [ProjectStatus.InProgress]: t('projects.statuses.active'),
      [ProjectStatus.OnHold]: t('projects.statuses.onHold'),
      [ProjectStatus.AtRisk]: t('tasks.priorities.urgent'),
      [ProjectStatus.Delayed]: t('dashboard.delayedTasks'),
      [ProjectStatus.Completed]: t('projects.statuses.completed'),
      [ProjectStatus.Cancelled]: t('projects.statuses.cancelled'),
    };
    return labels[status] || status.toString();
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">{t('projects.title')}</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => setOpen(true)}>
          {t('projects.newProject')}
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
          <Grid container spacing={3}>
            {data?.items.map((project: any) => (
              <Grid item xs={12} sm={6} md={4} key={project.id}>
                <Card>
                  <CardContent>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                      <Typography variant="h6" noWrap sx={{ flexGrow: 1 }}>{project.name}</Typography>
                      <IconButton size="small" onClick={(e) => { setAnchorEl(e.currentTarget); setSelectedProject(project); }}>
                        <MoreVert />
                      </IconButton>
                    </Box>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 2, minHeight: 40 }}>
                      {project.description || t('common.noData')}
                    </Typography>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <Chip label={getStatusLabel(project.status)} size="small" sx={{ bgcolor: statusColors[project.status], color: 'white' }} />
                      <Typography variant="body2" color="text.secondary">
                        {project.completedTaskCount}/{project.taskCount} {t('nav.tasks').toLowerCase()}
                      </Typography>
                    </Box>
                    <Box sx={{ mt: 2, display: 'flex', justifyContent: 'space-between' }}>
                      <Typography variant="caption" color="text.secondary">
                        {project.startDate ? new Date(project.startDate).toLocaleDateString() : ''} - {project.endDate ? new Date(project.endDate).toLocaleDateString() : ''}
                      </Typography>
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>

          {data?.totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
              <Pagination count={data.totalPages} page={page} onChange={(e, p) => setPage(p)} />
            </Box>
          )}
        </>
      )}

      <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={() => setAnchorEl(null)}>
        <MenuItem onClick={() => { setEditProject(selectedProject); setFormData({ name: selectedProject.name, description: selectedProject.description || '', startDate: selectedProject.startDate?.split('T')[0] || '', endDate: selectedProject.endDate?.split('T')[0] || '', budget: selectedProject.budget || '' }); setOpen(true); setAnchorEl(null); }}>
          <Edit fontSize="small" sx={{ mr: 1 }} /> {t('common.edit')}
        </MenuItem>
        <MenuItem onClick={() => handleDelete(selectedProject?.id)}>
          <Delete fontSize="small" sx={{ mr: 1 }} /> {t('common.delete')}
        </MenuItem>
      </Menu>

      <Dialog open={open} onClose={() => { setOpen(false); setEditProject(null); }} maxWidth="sm" fullWidth>
        <DialogTitle>{editProject ? t('projects.editProject') : t('projects.newProject')}</DialogTitle>
        <DialogContent>
          <TextField fullWidth label={t('projects.projectName')} value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} sx={{ mt: 2, mb: 2 }} />
          <TextField fullWidth label={t('projects.description')} value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} multiline rows={3} sx={{ mb: 2 }} />
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <TextField fullWidth label={t('projects.startDate')} type="date" value={formData.startDate} onChange={(e) => setFormData({ ...formData, startDate: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid item xs={6}>
              <TextField fullWidth label={t('projects.endDate')} type="date" value={formData.endDate} onChange={(e) => setFormData({ ...formData, endDate: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
          </Grid>
          <TextField fullWidth label={t('projects.budget')} type="number" value={formData.budget} onChange={(e) => setFormData({ ...formData, budget: e.target.value })} sx={{ mt: 2 }} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { setOpen(false); setEditProject(null); }}>{t('common.cancel')}</Button>
          <Button onClick={handleSubmit} variant="contained">{editProject ? t('common.save') : t('common.create')}</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
