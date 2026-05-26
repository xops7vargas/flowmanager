import React, { useState } from 'react';
import { Box, Typography, Card, CardContent, Button, IconButton, Dialog, DialogTitle, DialogContent, Chip, Grid, List, ListItem, ListItemText } from '@mui/material';
import { ChevronLeft, ChevronRight, Event, Schedule, Person, Flag } from '@mui/icons-material';
import { useGetCalendarEventsQuery, useGetTasksQuery } from '../api';
import { TaskStatus, TaskPriority } from '../types';
import dayjs from 'dayjs';

const statusColors: Record<TaskStatus, string> = {
  [TaskStatus.Todo]: '#9e9e9e',
  [TaskStatus.InProgress]: '#2196f3',
  [TaskStatus.InReview]: '#ff9800',
  [TaskStatus.Completed]: '#4caf50',
  [TaskStatus.Blocked]: '#f44336',
};

const statusLabels: Record<TaskStatus, string> = {
  [TaskStatus.Todo]: 'Por Hacer',
  [TaskStatus.InProgress]: 'En Progreso',
  [TaskStatus.InReview]: 'En Revisión',
  [TaskStatus.Completed]: 'Completado',
  [TaskStatus.Blocked]: 'Bloqueado',
};

const priorityLabels: Record<TaskPriority, string> = {
  [TaskPriority.Low]: 'Baja',
  [TaskPriority.Medium]: 'Media',
  [TaskPriority.High]: 'Alta',
  [TaskPriority.Critical]: 'Crítica',
};

export default function CalendarPage() {
  const [currentDate, setCurrentDate] = useState(dayjs());
  const [selectedDate, setSelectedDate] = useState<string | null>(null);
  const [taskDialogOpen, setTaskDialogOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState<any>(null);

  const startOfMonth = currentDate.startOf('month').format('YYYY-MM-DD');
  const endOfMonth = currentDate.endOf('month').format('YYYY-MM-DD');

  const { data: events } = useGetCalendarEventsQuery({ start: startOfMonth, end: endOfMonth });
  const { data: allTasks } = useGetTasksQuery({ page: 1, pageSize: 500 });

  const daysInMonth = currentDate.daysInMonth();
  const firstDayOfMonth = currentDate.startOf('month').day();

  const days = [];
  for (let i = 0; i < firstDayOfMonth; i++) {
    days.push(null);
  }
  for (let i = 1; i <= daysInMonth; i++) {
    days.push(i);
  }

  const getEventsForDay = (day: number) => {
    if (!events) return [];
    const date = currentDate.date(day).format('YYYY-MM-DD');
    return events.filter((e: any) => dayjs(e.start).format('YYYY-MM-DD') === date);
  };

  const getTasksForDate = (dateStr: string) => {
    if (!allTasks) return [];
    return allTasks.items.filter((task: any) => {
      const dueDate = task.dueDate ? dayjs(task.dueDate).format('YYYY-MM-DD') : null;
      const startDate = task.startDate ? dayjs(task.startDate).format('YYYY-MM-DD') : null;
      return dueDate === dateStr || startDate === dateStr;
    });
  };

  const handleDayClick = (day: number) => {
    const date = currentDate.date(day).format('YYYY-MM-DD');
    setSelectedDate(date);
    setSelectedTask(null);
    setTaskDialogOpen(true);
  };

  const getTaskStatus = (task: any) => {
    if (task.status === TaskStatus.Completed) return { label: 'Completado', color: 'success' };
    if (task.dueDate) {
      const dueDate = dayjs(task.dueDate);
      const now = dayjs();
      if (dueDate.isBefore(now)) return { label: 'Incumplimiento', color: 'error' };
      if (dueDate.diff(now, 'day') <= 3) return { label: 'Por Vencer', color: 'warning' };
    }
    if (task.status === TaskStatus.InProgress) return { label: 'En Curso', color: 'info' };
    return { label: 'Pendiente', color: 'default' };
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Calendario</Typography>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <IconButton onClick={() => setCurrentDate(currentDate.subtract(1, 'month'))}>
            <ChevronLeft />
          </IconButton>
          <Typography variant="h6">{currentDate.format('MMMM YYYY')}</Typography>
          <IconButton onClick={() => setCurrentDate(currentDate.add(1, 'month'))}>
            <ChevronRight />
          </IconButton>
        </Box>
      </Box>

      <Card>
        <CardContent>
          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: 1 }}>
            {['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'].map((day) => (
              <Typography key={day} align="center" fontWeight="bold" sx={{ p: 1 }}>
                {day}
              </Typography>
            ))}
            {days.map((day, index) => {
              const dateStr = day ? currentDate.date(day).format('YYYY-MM-DD') : null;
              const tasksForDay = dateStr ? getTasksForDate(dateStr) : [];
              const hasOverdue = tasksForDay.some((t: any) => t.dueDate && dayjs(t.dueDate).isBefore(dayjs()) && t.status !== TaskStatus.Completed);
              const isToday = day && dayjs().date() === day && currentDate.isSame(dayjs(), 'month');

              return (
                <Box
                  key={index}
                  onClick={() => day && handleDayClick(day)}
                  sx={{
                    minHeight: 100,
                    p: 1,
                    border: 1,
                    borderColor: hasOverdue ? 'error.main' : isToday ? 'primary.main' : 'divider',
                    borderRadius: 1,
                    bgcolor: day === null ? 'transparent' : isToday ? 'action.hover' : 'background.paper',
                    cursor: day ? 'pointer' : 'default',
                    '&:hover': day ? { bgcolor: 'action.selected' } : {},
                  }}
                >
                  {day && (
                    <>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <Typography 
                          variant="body2" 
                          fontWeight={isToday ? 'bold' : 'normal'} 
                          color={isToday ? 'primary' : 'textPrimary'}
                        >
                          {day}
                        </Typography>
                        {hasOverdue && <Chip label="!" color="error" size="small" sx={{ height: 16, fontSize: '0.65rem' }} />}
                      </Box>
                      {tasksForDay.slice(0, 3).map((task: any) => {
                        const taskStatus = getTaskStatus(task);
                        return (
                          <Box key={task.id} sx={{ mt: 0.5 }}>
                            <Typography 
                              variant="caption" 
                              sx={{ 
                                display: 'block', 
                                bgcolor: statusColors[task.status], 
                                color: 'white', 
                                px: 0.5, 
                                borderRadius: 0.5, 
                                overflow: 'hidden', 
                                textOverflow: 'ellipsis', 
                                whiteSpace: 'nowrap' 
                              }}
                            >
                              {task.title}
                            </Typography>
                          </Box>
                        );
                      })}
                      {tasksForDay.length > 3 && (
                        <Typography variant="caption" color="text.secondary">
                          +{tasksForDay.length - 3} más
                        </Typography>
                      )}
                    </>
                  )}
                </Box>
              );
            })}
          </Box>
        </CardContent>
      </Card>

      <Dialog open={taskDialogOpen} onClose={() => setTaskDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          Tareas del {selectedDate ? dayjs(selectedDate).format('DD MMMM YYYY') : ''}
        </DialogTitle>
        <DialogContent>
          {selectedDate && (() => {
            const tasksForDate = getTasksForDate(selectedDate);
            if (tasksForDate.length === 0) {
              return <Typography>No hay tareas para esta fecha</Typography>;
            }
            return (
              <List>
                {tasksForDate.map((task: any) => {
                  const taskStatus = getTaskStatus(task);
                  return (
                    <ListItem key={task.id} divider>
                      <ListItemText
                        primary={
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Typography variant="subtitle1" sx={{ flexGrow: 1 }}>{task.title}</Typography>
                            <Chip label={taskStatus.label} color={taskStatus.color as any} size="small" />
                            <Chip label={statusLabels[task.status]} size="small" sx={{ bgcolor: statusColors[task.status], color: 'white' }} />
                          </Box>
                        }
                        secondary={
                          <Grid container spacing={2} sx={{ mt: 1 }}>
                            <Grid item xs={6}>
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Event fontSize="small" color="action" />
                                <Typography variant="body2">
                                  Inicio: {task.startDate ? dayjs(task.startDate).format('DD/MM/YYYY') : 'Sin fecha'}
                                </Typography>
                              </Box>
                            </Grid>
                            <Grid item xs={6}>
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Schedule fontSize="small" color="action" />
                                <Typography variant="body2">
                                  Vencimiento: {task.dueDate ? dayjs(task.dueDate).format('DD/MM/YYYY') : 'Sin fecha'}
                                </Typography>
                              </Box>
                            </Grid>
                            <Grid item xs={6}>
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Person fontSize="small" color="action" />
                                <Typography variant="body2">
                                  Asignado: {task.assignedToName || 'Sin asignar'}
                                </Typography>
                              </Box>
                            </Grid>
                            <Grid item xs={6}>
                              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Flag fontSize="small" color="action" />
                                <Typography variant="body2">
                                  Prioridad: {priorityLabels[task.priority]}
                                </Typography>
                              </Box>
                            </Grid>
                            {task.projectName && (
                              <Grid item xs={12}>
                                <Typography variant="body2" color="text.secondary">
                                  Proyecto: {task.projectName}
                                </Typography>
                              </Grid>
                            )}
                            {task.description && (
                              <Grid item xs={12}>
                                <Typography variant="body2">
                                  Descripción: {task.description}
                                </Typography>
                              </Grid>
                            )}
                          </Grid>
                        }
                      />
                    </ListItem>
                  );
                })}
              </List>
            );
          })()}
        </DialogContent>
      </Dialog>
    </Box>
  );
}
