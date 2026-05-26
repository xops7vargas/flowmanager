import React, { useState } from 'react';
import {
  Box, Card, CardContent, Typography, Grid, FormControl, InputLabel, 
  Select, MenuItem, alpha
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import { 
  BarChart, Bar, LineChart, Line, PieChart, Pie, Cell, 
  XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend 
} from 'recharts';
import { 
  CheckCircle, Warning, Schedule, TrendingUp 
} from '@mui/icons-material';
import { useGetAnalyticsQuery } from '../api';
import { TaskPriority } from '../types';

const COLORS = ['#4caf50', '#2196f3', '#ff9800', '#f44336'];

export default function AnalyticsPage() {
  const { t } = useTranslation();
  const [period, setPeriod] = useState('12');
  const { data: analytics, isLoading } = useGetAnalyticsQuery({});

  if (isLoading) {
    return <Box sx={{ p: 3 }}>{t('common.loading')}</Box>;
  }

  const compliance = analytics?.compliance;
  const monthlyData = analytics?.monthlyData || [];
  const userPerformance = analytics?.userPerformance || [];
  const projectMetrics = analytics?.projectMetrics || [];
  const priorityData = analytics?.priorityDistribution?.map(p => ({
    name: t(`tasks.priorities.${Object.keys(TaskPriority)[p.priority].toLowerCase()}`),
    value: p.count,
    percentage: p.percentage
  })) || [];

  const priorityLabels = ['Baja', 'Media', 'Alta', 'Crítica'];
  const priorityDataFormatted = analytics?.priorityDistribution?.map((p, i) => ({
    name: priorityLabels[p.priority] || `Prioridad ${p.priority}`,
    value: p.count,
    percentage: p.percentage
  })) || [];

  const taskStatusData = [
    { name: 'Completadas', value: compliance?.completedTasks || 0, color: '#4caf50' },
    { name: 'En Progreso', value: compliance ? Math.max(0, compliance.totalTasks - compliance.completedTasks - compliance.overdueTasks) : 0, color: '#2196f3' },
    { name: 'Retrasadas', value: compliance?.overdueTasks || 0, color: '#f44336' }
  ];

  const renderLabel = (entry: any) => {
    return `${entry.name}: ${entry.value}`;
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">{t('analytics.title')}</Typography>
        <FormControl size="small" sx={{ minWidth: 150 }}>
          <InputLabel>{t('analytics.period')}</InputLabel>
          <Select value={period} label={t('analytics.period')} onChange={(e) => setPeriod(e.target.value)}>
            <MenuItem value="3">{t('analytics.last3months')}</MenuItem>
            <MenuItem value="6">{t('analytics.last6months')}</MenuItem>
            <MenuItem value="12">{t('analytics.last12months')}</MenuItem>
          </Select>
        </FormControl>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} sm={6} md={3}>
          <Card sx={{ bgcolor: alpha('#4caf50', 0.1), height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <CheckCircle sx={{ fontSize: 40, color: '#4caf50' }} />
                <Box>
                  <Typography variant="h4">{compliance?.completionRate?.toFixed(1)}%</Typography>
                  <Typography variant="body2" color="text.secondary">{t('analytics.completionRate')}</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card sx={{ bgcolor: alpha('#2196f3', 0.1), height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <TrendingUp sx={{ fontSize: 40, color: '#2196f3' }} />
                <Box>
                  <Typography variant="h4">{compliance?.complianceRate?.toFixed(1)}%</Typography>
                  <Typography variant="body2" color="text.secondary">{t('analytics.complianceRate')}</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card sx={{ bgcolor: alpha('#f44336', 0.1), height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Warning sx={{ fontSize: 40, color: '#f44336' }} />
                <Box>
                  <Typography variant="h4">{compliance?.overdueRate?.toFixed(1)}%</Typography>
                  <Typography variant="body2" color="text.secondary">{t('analytics.overdueRate')}</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={3}>
          <Card sx={{ bgcolor: alpha('#ff9800', 0.1), height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Schedule sx={{ fontSize: 40, color: '#ff9800' }} />
                <Box>
                  <Typography variant="h4">{compliance?.totalTasks || 0}</Typography>
                  <Typography variant="body2" color="text.secondary">{t('analytics.totalTasks')}</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={8}>
          <Card sx={{ minHeight: 350 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>{t('analytics.taskEvolution')}</Typography>
              <ResponsiveContainer width="100%" height={280}>
                <LineChart data={monthlyData} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                  <YAxis tick={{ fontSize: 11 }} />
                  <Tooltip contentStyle={{ fontSize: 12 }} />
                  <Legend wrapperStyle={{ fontSize: 11 }} />
                  <Line type="monotone" dataKey="tasksCreated" name={t('analytics.tasksCreated')} stroke="#2196f3" strokeWidth={2} dot={{ r: 4 }} />
                  <Line type="monotone" dataKey="tasksCompleted" name={t('analytics.tasksCompleted')} stroke="#4caf50" strokeWidth={2} dot={{ r: 4 }} />
                </LineChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Card sx={{ minHeight: 350 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>{t('analytics.taskDistribution')}</Typography>
              <ResponsiveContainer width="100%" height={280}>
                <PieChart margin={{ top: 5, right: 5, left: 5, bottom: 5 }}>
                  <Pie
                    data={taskStatusData}
                    cx="50%"
                    cy="50%"
                    innerRadius={50}
                    outerRadius={90}
                    paddingAngle={3}
                    dataKey="value"
                    labelLine={false}
                    label={({ name, percent }) => `${(percent * 100).toFixed(0)}%`}
                  >
                    {taskStatusData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value) => `${value} tareas`} contentStyle={{ fontSize: 12 }} />
                  <Legend wrapperStyle={{ fontSize: 11 }} />
                </PieChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card sx={{ minHeight: 350 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>{t('analytics.userPerformance')}</Typography>
              <ResponsiveContainer width="100%" height={280}>
                <BarChart data={userPerformance.slice(0, 5)} layout="vertical" margin={{ top: 5, right: 20, left: 10, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis type="number" tick={{ fontSize: 11 }} />
                  <YAxis dataKey="userName" type="category" width={80} tick={{ fontSize: 11 }} />
                  <Tooltip contentStyle={{ fontSize: 12 }} />
                  <Legend wrapperStyle={{ fontSize: 11 }} />
                  <Bar dataKey="tasksCompleted" name={t('tasks.statuses.done')} fill="#4caf50" />
                  <Bar dataKey="overdueTasks" name={t('dashboard.delayedTasks')} fill="#f44336" />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card sx={{ minHeight: 350 }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>{t('analytics.priorityDistribution')}</Typography>
              <ResponsiveContainer width="100%" height={280}>
                <PieChart margin={{ top: 5, right: 5, left: 5, bottom: 5 }}>
                  <Pie
                    data={priorityDataFormatted}
                    cx="50%"
                    cy="50%"
                    outerRadius={90}
                    paddingAngle={3}
                    dataKey="value"
                    labelLine={false}
                    label={({ name, percent }) => `${(percent * 100).toFixed(0)}%`}
                  >
                    {priorityDataFormatted.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(value) => `${value} tareas`} contentStyle={{ fontSize: 12 }} />
                  <Legend wrapperStyle={{ fontSize: 11 }} />
                </PieChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>{t('analytics.projectMetrics')}</Typography>
              <Grid container spacing={2}>
                {projectMetrics.slice(0, 6).map((project) => (
                  <Grid item xs={12} sm={6} md={4} key={project.projectId}>
                    <Box sx={{ p: 2, border: '1px solid', borderColor: 'divider', borderRadius: 2, height: '100%' }}>
                      <Typography variant="subtitle1" fontWeight={600} noWrap>{project.projectName || 'Sin Nombre'}</Typography>
                      <Box sx={{ mt: 1, display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="text.secondary">
                          {t('tasks.statuses.done')}: {project.completedTasks}/{project.totalTasks}
                        </Typography>
                        <Typography variant="body2" fontWeight={600}>
                          {project.progress.toFixed(0)}%
                        </Typography>
                      </Box>
                      <Box sx={{ mt: 1, display: 'flex', justifyContent: 'space-between' }}>
                        <Typography variant="body2" color="text.secondary">
                          {t('projects.budget')}: ${project.budget?.toFixed(2) || '0'}
                        </Typography>
                        <Typography variant="body2" color={project.spent > project.budget ? 'error' : 'success'}>
                          ${project.spent?.toFixed(2) || '0'}
                        </Typography>
                      </Box>
                    </Box>
                  </Grid>
                ))}
                {projectMetrics.length === 0 && (
                  <Grid item xs={12}>
                    <Typography color="text.secondary" align="center">{t('common.noData')}</Typography>
                  </Grid>
                )}
              </Grid>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
}
