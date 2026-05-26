import React, { useState, useEffect } from 'react';
import { Box, Grid, Card, CardContent, Typography, Stack, LinearProgress, List, ListItem, ListItemIcon, ListItemText, Chip, IconButton, Badge, Divider } from '@mui/material';
import { 
  Folder, Assignment, CheckCircle, Warning, AccessTime, Notifications as NotificationsIcon, Info, Error as ErrorIcon, CheckCircleOutline, Message
} from '@mui/icons-material';
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { useTranslation } from 'react-i18next';
import { useGetDashboardQuery, useGetNotificationsQuery, useGetConversationsQuery, useGetTasksQuery } from '../api';
import { TaskStatus, TaskPriority } from '../types';
import dayjs from 'dayjs';

export default function DashboardPage() {
  const { t } = useTranslation();
  const { data: dashboard, isLoading } = useGetDashboardQuery();
  const { data: notifications } = useGetNotificationsQuery(false);
  const { data: conversations } = useGetConversationsQuery();
  const { data: tasks } = useGetTasksQuery({ page: 1, pageSize: 100 });
  const [alerts, setAlerts] = useState<any[]>([]);

  const priorityColors: Record<TaskPriority, string> = {
    [TaskPriority.Low]: '#4caf50',
    [TaskPriority.Medium]: '#2196f3',
    [TaskPriority.High]: '#ff9800',
    [TaskPriority.Critical]: '#f44336',
  };

  const getPriorityLabel = (priority: TaskPriority) => {
    const labels: Record<TaskPriority, string> = {
      [TaskPriority.Low]: 'Baja',
      [TaskPriority.Medium]: 'Media',
      [TaskPriority.High]: 'Alta',
      [TaskPriority.Critical]: 'Urgente',
    };
    return labels[priority] || priority.toString();
  };

  useEffect(() => {
    const newAlerts: any[] = [];
    
    if (notifications) {
      notifications.slice(0, 5).forEach((notif: any) => {
        newAlerts.push({
          id: notif.id,
          type: notif.type,
          title: notif.title,
          message: notif.message,
          createdAt: notif.createdAt,
          icon: notif.type === 0 ? <Info color="info" /> : <Warning color="warning" />,
          color: 'info'
        });
      });
    }

    if (tasks?.items) {
      const now = dayjs();
      tasks.items.forEach((task: any) => {
        if (task.status === TaskStatus.Completed) {
          const wasNotified = notifications?.some((n: any) => n.title.includes(task.title));
          if (!wasNotified) {
            newAlerts.push({
              id: `completed-${task.id}`,
              type: 'completed',
              title: 'Tarea Completada',
              message: `"${task.title}" ha sido completada`,
              createdAt: task.updatedAt,
              icon: <CheckCircleOutline color="success" />,
              color: 'success'
            });
          }
        }
        
        if (task.dueDate && dayjs(task.dueDate).isBefore(now) && task.status !== TaskStatus.Completed) {
          const wasNotified = notifications?.some((n: any) => n.title.includes(task.title));
          if (!wasNotified) {
            newAlerts.push({
              id: `overdue-${task.id}`,
              type: 'overdue',
              title: 'Tarea Vencida',
              message: `"${task.title}" ha vencido`,
              createdAt: task.dueDate,
              icon: <ErrorIcon color="error" />,
              color: 'error'
            });
          }
        }

        const dueSoon = task.dueDate && dayjs(task.dueDate).diff(now, 'day') <= 3 && dayjs(task.dueDate).isAfter(now) && task.status !== TaskStatus.Completed;
        if (dueSoon) {
          newAlerts.push({
            id: `duesoon-${task.id}`,
            type: 'dueSoon',
            title: 'Tarea por Vencer',
            message: `"${task.title}" vence en ${dayjs(task.dueDate).diff(now, 'day')} día(s)`,
            createdAt: task.dueDate,
            icon: <Warning color="warning" />,
            color: 'warning'
          });
        }
      });
    }

    if (conversations) {
      const totalUnread = conversations.reduce((sum: number, c: any) => sum + (c.unreadCount || 0), 0);
      if (totalUnread > 0) {
        newAlerts.push({
          id: 'messages',
          type: 'messages',
          title: 'Nuevos Mensajes',
          message: `Tienes ${totalUnread} mensaje(s) sin leer`,
          createdAt: new Date().toISOString(),
          icon: <Message color="info" />,
          color: 'info'
        });
      }
    }

    setAlerts(newAlerts.slice(0, 8));
  }, [notifications, tasks, conversations]);

  if (isLoading) {
    return <Box sx={{ p: 3 }}>{t('common.loading')}</Box>;
  }

  if (!dashboard) {
    return <Box sx={{ p: 3 }}>{t('common.noData')}</Box>;
  }

  const statusData = [
    { name: 'Por Hacer', value: dashboard.pendingTasks, color: '#9e9e9e' },
    { name: 'En Progreso', value: dashboard.inProgressTasks, color: '#2196f3' },
    { name: 'Completadas', value: dashboard.completedTasks, color: '#4caf50' },
    { name: 'Retrasadas', value: dashboard.overdueTasks, color: '#f44336' },
  ];

  const priorityData = dashboard.tasksByPriority.map(tp => ({
    name: getPriorityLabel(tp.priority),
    value: tp.count,
    color: priorityColors[tp.priority],
  }));

  return (
    <Box>
      <Typography variant="h4" gutterBottom>{t('dashboard.title')}</Typography>
      
      {alerts.length > 0 && (
        <Card sx={{ mb: 3, bgcolor: '#fff3e0' }}>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
              <NotificationsIcon color="warning" />
              <Typography variant="h6">Alertas Recientes</Typography>
              <Chip label={alerts.length} color="warning" size="small" />
            </Box>
            <Grid container spacing={1}>
              {alerts.slice(0, 4).map((alert: any) => (
                <Grid item xs={12} sm={6} key={alert.id}>
                  <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 1, p: 1, bgcolor: 'background.paper', borderRadius: 1 }}>
                    {alert.icon}
                    <Box sx={{ flex: 1 }}>
                      <Typography variant="subtitle2" color={`${alert.color}.main`}>{alert.title}</Typography>
                      <Typography variant="caption" color="text.secondary">{alert.message}</Typography>
                    </Box>
                  </Box>
                </Grid>
              ))}
            </Grid>
          </CardContent>
        </Card>
      )}
      
      <Grid container spacing={3}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" spacing={2}>
                <Box sx={{ p: 1.5, bgcolor: 'primary.light', borderRadius: 1 }}>
                  <Folder sx={{ color: 'white' }} />
                </Box>
                <Box>
                  <Typography variant="h5">{dashboard.totalProjects}</Typography>
                  <Typography variant="body2" color="text.secondary">{t('dashboard.totalProjects')}</Typography>
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" spacing={2}>
                <Box sx={{ p: 1.5, bgcolor: 'info.light', borderRadius: 1 }}>
                  <Assignment sx={{ color: 'white' }} />
                </Box>
                <Box>
                  <Typography variant="h5">{dashboard.totalTasks}</Typography>
                  <Typography variant="body2" color="text.secondary">{t('dashboard.totalTasks')}</Typography>
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" spacing={2}>
                <Box sx={{ p: 1.5, bgcolor: 'success.light', borderRadius: 1 }}>
                  <CheckCircle sx={{ color: 'white' }} />
                </Box>
                <Box>
                  <Typography variant="h5">{dashboard.completedTasks}</Typography>
                  <Typography variant="body2" color="text.secondary">{t('dashboard.completedTasks')}</Typography>
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              <Stack direction="row" alignItems="center" spacing={2}>
                <Box sx={{ p: 1.5, bgcolor: 'warning.light', borderRadius: 1 }}>
                  <Warning sx={{ color: 'white' }} />
                </Box>
                <Box>
                  <Typography variant="h5">{dashboard.overdueTasks}</Typography>
                  <Typography variant="body2" color="text.secondary">{t('dashboard.delayedTasks')}</Typography>
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card sx={{ height: 320 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Estado de Tareas</Typography>
              <ResponsiveContainer width="100%" height={250}>
                <PieChart>
                  <Pie
                    data={statusData}
                    cx="50%"
                    cy="50%"
                    innerRadius={50}
                    outerRadius={80}
                    paddingAngle={3}
                    dataKey="value"
                  >
                    {statusData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value) => `${value} tareas`} />
                  <Legend wrapperStyle={{ fontSize: '12px' }} layout="vertical" align="right" verticalAlign="middle" />
                </PieChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card sx={{ height: 320 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>Prioridades</Typography>
              <ResponsiveContainer width="100%" height={250}>
                <BarChart data={priorityData} layout="vertical" margin={{ left: 40 }}>
                  <XAxis type="number" tick={{ fontSize: 11 }} />
                  <YAxis dataKey="name" type="category" tick={{ fontSize: 11 }} width={50} />
                  <Tooltip formatter={(value) => `${value} tareas`} />
                  <Bar dataKey="value" radius={[0, 4, 4, 0]}>
                    {priorityData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>{t('projects.progress')}</Typography>
              <Stack spacing={2}>
                {dashboard.projectProgress.map((project) => (
                  <Box key={project.projectId}>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                      <Typography variant="body2" noWrap sx={{ maxWidth: '60%' }}>{project.projectName}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {project.completedTasks}/{project.totalTasks} tareas
                      </Typography>
                    </Box>
                    <LinearProgress 
                      variant="determinate" 
                      value={project.progress} 
                      sx={{ height: 8, borderRadius: 4 }}
                    />
                  </Box>
                ))}
                {dashboard.projectProgress.length === 0 && (
                  <Typography variant="body2" color="text.secondary">{t('dashboard.noProjects')}</Typography>
                )}
              </Stack>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
