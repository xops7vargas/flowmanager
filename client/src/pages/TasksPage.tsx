import React, { useState } from 'react';
import { Box, Typography, Button, Card, CardContent, Grid, TextField, Chip, IconButton, Menu, MenuItem, Dialog, DialogTitle, DialogContent, DialogActions, Select, FormControl, InputLabel, InputAdornment, Pagination } from '@mui/material';
import { Add, MoreVert, Edit, Delete } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useGetTasksQuery, useCreateTaskMutation, useDeleteTaskMutation, useUpdateTaskMutation, useGetProjectsQuery, useGetUsersQuery } from '../api';
import { TaskStatus, TaskPriority } from '../types';

const statusColors: Record<TaskStatus, string> = {
  [TaskStatus.Todo]: '#9e9e9e',
  [TaskStatus.InProgress]: '#2196f3',
  [TaskStatus.InReview]: '#ff9800',
  [TaskStatus.Completed]: '#4caf50',
  [TaskStatus.Blocked]: '#f44336',
};

const priorityColors: Record<TaskPriority, string> = {
  [TaskPriority.Low]: '#4caf50',
  [TaskPriority.Medium]: '#2196f3',
  [TaskPriority.High]: '#ff9800',
  [TaskPriority.Critical]: '#f44336',
};

export default function TasksPage() {
  const { t } = useTranslation();
  const [page, setPage] = useState(1);
  const [open, setOpen] = useState(false);
  const [editTask, setEditTask] = useState<any>(null);
  const [formData, setFormData] = useState<{ projectId: string; title: string; description: string; priority: TaskPriority; startDate: string; dueDate: string; estimatedHours: number | string; assignedToId: string }>({ projectId: '', title: '', description: '', priority: TaskPriority.Medium, startDate: '', dueDate: '', estimatedHours: '', assignedToId: '' });
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [selectedTask, setSelectedTask] = useState<any>(null);

  const { data, isLoading } = useGetTasksQuery({ page, pageSize: 12, projectId: undefined, status: undefined, assignedToId: undefined });
  const { data: projectsData } = useGetProjectsQuery({ page: 1, pageSize: 100 });
  const { data: usersData } = useGetUsersQuery({ page: 1, pageSize: 100 });
  const [createTask] = useCreateTaskMutation();
  const [updateTask] = useUpdateTaskMutation();
  const [deleteTask] = useDeleteTaskMutation();

  const handleSubmit = async () => {
    try {
      const taskData: any = {
        projectId: formData.projectId,
        title: formData.title,
        description: formData.description,
        priority: formData.priority,
        startDate: formData.startDate || undefined,
        dueDate: formData.dueDate || undefined,
        estimatedHours: formData.estimatedHours ? parseFloat(formData.estimatedHours as string) : undefined,
      };
      if (formData.assignedToId) {
        taskData.assignedToId = formData.assignedToId;
      }
      if (editTask) {
        await updateTask({ id: editTask.id, body: taskData });
      } else {
        await createTask(taskData);
      }
      setOpen(false);
      setEditTask(null);
      setFormData({ projectId: '', title: '', description: '', priority: TaskPriority.Medium, startDate: '', dueDate: '', estimatedHours: '', assignedToId: '' });
    } catch (error) {
      console.error(error);
    }
  };

  const handleDelete = async (id: string) => {
    if (confirm(t('tasks.confirmDelete'))) {
      await deleteTask(id);
    }
    setAnchorEl(null);
  };

  const getStatusLabel = (status: TaskStatus) => {
    const labels: Record<TaskStatus, string> = {
      [TaskStatus.Todo]: t('tasks.statuses.todo'),
      [TaskStatus.InProgress]: t('tasks.statuses.inProgress'),
      [TaskStatus.InReview]: t('tasks.statuses.review'),
      [TaskStatus.Completed]: t('tasks.statuses.done'),
      [TaskStatus.Blocked]: t('tasks.statuses.cancelled'),
    };
    return labels[status] || status.toString();
  };

  const getPriorityLabel = (priority: TaskPriority) => {
    const labels: Record<TaskPriority, string> = {
      [TaskPriority.Low]: t('tasks.priorities.low'),
      [TaskPriority.Medium]: t('tasks.priorities.medium'),
      [TaskPriority.High]: t('tasks.priorities.high'),
      [TaskPriority.Critical]: t('tasks.priorities.urgent'),
    };
    return labels[priority] || priority.toString();
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">{t('tasks.title')}</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => setOpen(true)}>
          {t('tasks.newTask')}
        </Button>
      </Box>

      {isLoading ? (
        <Typography>{t('common.loading')}</Typography>
      ) : (
        <>
          <Grid container spacing={2}>
            {data?.items.map((task: any) => (
              <Grid item xs={12} sm={6} md={4} key={task.id}>
                <Card sx={{ borderLeft: 4, borderColor: statusColors[task.status] }}>
                  <CardContent>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                      <Typography variant="subtitle1" noWrap sx={{ flexGrow: 1, fontWeight: 500 }}>{task.title}</Typography>
                      <IconButton size="small" onClick={(e) => { setAnchorEl(e.currentTarget); setSelectedTask(task); }}>
                        <MoreVert fontSize="small" />
                      </IconButton>
                    </Box>
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>{task.projectName}</Typography>
                    <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                      <Chip label={getStatusLabel(task.status)} size="small" sx={{ bgcolor: statusColors[task.status], color: 'white', fontSize: '0.7rem' }} />
                      <Chip label={getPriorityLabel(task.priority)} size="small" sx={{ bgcolor: priorityColors[task.priority], color: 'white', fontSize: '0.7rem' }} />
                    </Box>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 2 }}>
                      <Typography variant="caption" color="text.secondary">
                        {task.assignedToName || t('tasks.assignedTo') + ': -'}
                      </Typography>
                      {task.dueDate && (
                        <Typography variant="caption" color={task.isOverdue ? 'error' : 'text.secondary'}>
                          {t('tasks.dueDate')}: {new Date(task.dueDate).toLocaleDateString()}
                        </Typography>
                      )}
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>

          {data?.totalPages > 1 && (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
              <Pagination count={data.totalPages} page={page} onChange={(_, p) => setPage(p)} />
            </Box>
          )}
        </>
      )}

      <Menu anchorEl={anchorEl} open={Boolean(anchorEl)} onClose={() => setAnchorEl(null)}>
        <MenuItem onClick={() => { setEditTask(selectedTask); setFormData({ projectId: selectedTask.projectId, title: selectedTask.title, description: selectedTask.description || '', priority: selectedTask.priority, startDate: selectedTask.startDate?.split('T')[0] || '', dueDate: selectedTask.dueDate?.split('T')[0] || '', estimatedHours: selectedTask.estimatedHours || '', assignedToId: selectedTask.assignedToId || '' }); setOpen(true); setAnchorEl(null); }}>
          <Edit fontSize="small" sx={{ mr: 1 }} /> {t('common.edit')}
        </MenuItem>
        <MenuItem onClick={() => handleDelete(selectedTask?.id)}>
          <Delete fontSize="small" sx={{ mr: 1 }} /> {t('common.delete')}
        </MenuItem>
      </Menu>

      <Dialog open={open} onClose={() => { setOpen(false); setEditTask(null); }} maxWidth="sm" fullWidth>
        <DialogTitle>{editTask ? t('tasks.editTask') : t('tasks.newTask')}</DialogTitle>
        <DialogContent>
          <FormControl fullWidth sx={{ mt: 2, mb: 2 }}>
            <InputLabel>{t('tasks.project')}</InputLabel>
            <Select value={formData.projectId} onChange={(e) => setFormData({ ...formData, projectId: e.target.value })} label={t('tasks.project')}>
              {projectsData?.items.map((p: any) => <MenuItem key={p.id} value={p.id}>{p.name}</MenuItem>)}
            </Select>
          </FormControl>
          <FormControl fullWidth sx={{ mb: 2 }}>
            <InputLabel>{t('tasks.assignedTo')}</InputLabel>
            <Select value={formData.assignedToId} onChange={(e) => setFormData({ ...formData, assignedToId: e.target.value })} label={t('tasks.assignedTo')}>
              <MenuItem value="">-- {t('common.select')} --</MenuItem>
              {usersData?.items.map((u: any) => <MenuItem key={u.id} value={u.id}>{u.firstName} {u.lastName}</MenuItem>)}
            </Select>
          </FormControl>
          <TextField fullWidth label={t('tasks.taskTitle')} value={formData.title} onChange={(e) => setFormData({ ...formData, title: e.target.value })} sx={{ mb: 2 }} />
          <TextField fullWidth label={t('tasks.taskDescription')} value={formData.description} onChange={(e) => setFormData({ ...formData, description: e.target.value })} multiline rows={3} sx={{ mb: 2 }} />
          <FormControl fullWidth sx={{ mb: 2 }}>
            <InputLabel>{t('tasks.priority')}</InputLabel>
            <Select value={formData.priority} onChange={(e) => setFormData({ ...formData, priority: e.target.value as TaskPriority })} label={t('tasks.priority')}>
              <MenuItem value={TaskPriority.Low}>{t('tasks.priorities.low')}</MenuItem>
              <MenuItem value={TaskPriority.Medium}>{t('tasks.priorities.medium')}</MenuItem>
              <MenuItem value={TaskPriority.High}>{t('tasks.priorities.high')}</MenuItem>
              <MenuItem value={TaskPriority.Critical}>{t('tasks.priorities.urgent')}</MenuItem>
            </Select>
          </FormControl>
          <Grid container spacing={2}>
            <Grid item xs={6}>
              <TextField fullWidth label={t('tasks.startDate')} type="date" value={formData.startDate} onChange={(e) => setFormData({ ...formData, startDate: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid item xs={6}>
              <TextField fullWidth label={t('tasks.dueDate')} type="date" value={formData.dueDate} onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })} InputLabelProps={{ shrink: true }} />
            </Grid>
          </Grid>
          <TextField fullWidth label={t('tasks.estimatedHours')} type="number" value={formData.estimatedHours} onChange={(e) => setFormData({ ...formData, estimatedHours: e.target.value })} sx={{ mt: 2 }} InputProps={{ startAdornment: <InputAdornment position="start">h</InputAdornment> }} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => { setOpen(false); setEditTask(null); }}>{t('common.cancel')}</Button>
          <Button onClick={handleSubmit} variant="contained">{editTask ? t('common.save') : t('common.create')}</Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
