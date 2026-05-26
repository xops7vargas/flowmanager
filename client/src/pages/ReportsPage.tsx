import { useState } from 'react';
import { Box, Typography, Card, CardContent, Grid, FormControl, InputLabel, Select, MenuItem, Button, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, TextField, Chip, alpha } from '@mui/material';
import { Download, Assessment, People, AttachMoney } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useGetProjectsQuery, useGetFinancialReportQuery, useGetAnalyticsQuery, useGetFinancialTransactionsQuery } from '../api';
import type { FinancialReport } from '../types';

export default function ReportsPage() {
  const { t } = useTranslation();
  const [selectedProject, setSelectedProject] = useState('');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [reportType, setReportType] = useState('project');

  const { data: projects } = useGetProjectsQuery({ page: 1, pageSize: 100 });
  const { data: analytics } = useGetAnalyticsQuery({});
  const { data: transactions } = useGetFinancialTransactionsQuery({ 
    page: 1,
    pageSize: 100,
    projectId: selectedProject || undefined,
    startDate: startDate || undefined,
    endDate: endDate || undefined,
  });

  const handleExport = () => {
    alert('Exportando reporte...');
  };

  const getStatusLabel = (status: number) => {
    const labels = ['Planificación', 'Activo', 'Completado', 'En Pausa', 'Cancelado'];
    return labels[status] || 'Desconocido';
  };

  const getStatusColor = (status: number): 'default' | 'primary' | 'success' | 'warning' | 'error' => {
    const colors: ('default' | 'primary' | 'success' | 'warning' | 'error')[] = ['default', 'primary', 'success', 'warning', 'error'];
    return colors[status] || 'default';
  };

  const calculateTotals = () => {
    if (!transactions?.items) return { income: 0, expenses: 0, balance: 0 };
    const income = transactions.items.filter((t: any) => t.type === 0).reduce((sum: number, t: any) => sum + (t.amount || 0), 0);
    const expenses = transactions.items.filter((t: any) => t.type === 1).reduce((sum: number, t: any) => sum + (t.amount || 0), 0);
    return { income, expenses, balance: income - expenses };
  };

  const totals = calculateTotals();

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">{t('nav.reports')}</Typography>
        <Button variant="contained" startIcon={<Download />} onClick={handleExport}>
          {t('common.download')}
        </Button>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={3}>
          <FormControl fullWidth>
            <InputLabel>Tipo de Reporte</InputLabel>
            <Select value={reportType} label="Tipo de Reporte" onChange={(e) => setReportType(e.target.value)}>
              <MenuItem value="project">Reporte de Proyecto</MenuItem>
              <MenuItem value="financial">Reporte Financiero</MenuItem>
              <MenuItem value="user">Reporte de Usuario</MenuItem>
            </Select>
          </FormControl>
        </Grid>

        {reportType === 'project' && (
          <Grid item xs={12} md={3}>
            <FormControl fullWidth>
              <InputLabel>Proyecto</InputLabel>
              <Select value={selectedProject} label="Proyecto" onChange={(e) => setSelectedProject(e.target.value)}>
                <MenuItem value="">Todos los proyectos</MenuItem>
                {projects?.items.map((p: any) => (
                  <MenuItem key={p.id} value={p.id}>{p.name}</MenuItem>
                ))}
              </Select>
            </FormControl>
          </Grid>
        )}

        <Grid item xs={12} md={3}>
          <TextField
            fullWidth
            label="Fecha Inicio"
            type="date"
            value={startDate}
            onChange={(e) => setStartDate(e.target.value)}
            InputLabelProps={{ shrink: true }}
          />
        </Grid>
        <Grid item xs={12} md={3}>
          <TextField
            fullWidth
            label="Fecha Fin"
            type="date"
            value={endDate}
            onChange={(e) => setEndDate(e.target.value)}
            InputLabelProps={{ shrink: true }}
          />
        </Grid>
      </Grid>

      {reportType === 'project' && (
        <Grid container spacing={3} sx={{ mt: 2 }}>
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  <Assessment sx={{ mr: 1, verticalAlign: 'middle' }} />
                  Resumen de Proyectos
                </Typography>
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Proyecto</TableCell>
                        <TableCell>Estado</TableCell>
                        <TableCell align="right">Presupuesto</TableCell>
                        <TableCell align="right">Tareas Totales</TableCell>
                        <TableCell align="right">Completadas</TableCell>
                        <TableCell align="right">En Progreso</TableCell>
                        <TableCell align="right">Retrasadas</TableCell>
                        <TableCell align="right">Progreso</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {analytics?.projectMetrics?.map((project: any) => (
                        <TableRow key={project.projectId}>
                          <TableCell>{project.projectName}</TableCell>
                          <TableCell>
                            <Chip label={getStatusLabel(project.status)} color={getStatusColor(project.status)} size="small" />
                          </TableCell>
                          <TableCell align="right">${project.budget?.toLocaleString() || 0}</TableCell>
                          <TableCell align="right">{project.totalTasks}</TableCell>
                          <TableCell align="right">{project.completedTasks}</TableCell>
                          <TableCell align="right">{project.inProgressTasks || 0}</TableCell>
                          <TableCell align="right">{project.overdueTasks || 0}</TableCell>
                          <TableCell align="right">{project.progress?.toFixed(0)}%</TableCell>
                        </TableRow>
                      ))}
                      {(!analytics?.projectMetrics || analytics.projectMetrics.length === 0) && (
                        <TableRow>
                          <TableCell colSpan={8} align="center">
                            <Typography color="text.secondary">No hay datos de proyectos</Typography>
                          </TableCell>
                        </TableRow>
                      )}
                    </TableBody>
                  </Table>
                </TableContainer>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {reportType === 'financial' && (
        <Grid container spacing={3} sx={{ mt: 2 }}>
          <Grid item xs={12} md={4}>
            <Card sx={{ bgcolor: alpha('#4caf50', 0.1) }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <AttachMoney sx={{ fontSize: 40, color: '#4caf50' }} />
                  <Box>
                    <Typography variant="h5">${totals.income.toLocaleString()}</Typography>
                    <Typography variant="body2" color="text.secondary">Total Ingresos</Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={4}>
            <Card sx={{ bgcolor: alpha('#f44336', 0.1) }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <AttachMoney sx={{ fontSize: 40, color: '#f44336' }} />
                  <Box>
                    <Typography variant="h5">${totals.expenses.toLocaleString()}</Typography>
                    <Typography variant="body2" color="text.secondary">Total Gastos</Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={4}>
            <Card sx={{ bgcolor: alpha('#2196f3', 0.1) }}>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                  <AttachMoney sx={{ fontSize: 40, color: '#2196f3' }} />
                  <Box>
                    <Typography variant="h5">${totals.balance.toLocaleString()}</Typography>
                    <Typography variant="body2" color="text.secondary">Balance</Typography>
                  </Box>
                </Box>
              </CardContent>
            </Card>
          </Grid>

          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>Transacciones</Typography>
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Fecha</TableCell>
                        <TableCell>Descripción</TableCell>
                        <TableCell>Tipo</TableCell>
                        <TableCell align="right">Monto</TableCell>
                        <TableCell>Referencia</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {transactions?.items?.slice(0, 20).map((tx: any) => (
                        <TableRow key={tx.id}>
                          <TableCell>{new Date(tx.date).toLocaleDateString()}</TableCell>
                          <TableCell>{tx.description || '-'}</TableCell>
                          <TableCell>
                            <Chip 
                              label={tx.type === 0 ? 'Ingreso' : 'Gasto'} 
                              color={tx.type === 0 ? 'success' : 'error'} 
                              size="small" 
                            />
                          </TableCell>
                          <TableCell align="right">${tx.amount?.toLocaleString() || 0}</TableCell>
                          <TableCell>{tx.reference || '-'}</TableCell>
                        </TableRow>
                      ))}
                      {(!transactions?.items || transactions.items.length === 0) && (
                        <TableRow>
                          <TableCell colSpan={5} align="center">
                            <Typography color="text.secondary">No hay transacciones</Typography>
                          </TableCell>
                        </TableRow>
                      )}
                    </TableBody>
                  </Table>
                </TableContainer>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {reportType === 'user' && (
        <Grid container spacing={3} sx={{ mt: 2 }}>
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  <People sx={{ mr: 1, verticalAlign: 'middle' }} />
                  Rendimiento de Usuarios
                </Typography>
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Usuario</TableCell>
                        <TableCell align="right">Tareas Asignadas</TableCell>
                        <TableCell align="right">Completadas</TableCell>
                        <TableCell align="right">En Progreso</TableCell>
                        <TableCell align="right">Retrasadas</TableCell>
                        <TableCell align="right">Horas Registradas</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {analytics?.userPerformance?.map((user: any) => (
                        <TableRow key={user.userId}>
                          <TableCell>{user.userName}</TableCell>
                          <TableCell align="right">{user.tasksCompleted + user.tasksInProgress + user.overdueTasks}</TableCell>
                          <TableCell align="right">{user.tasksCompleted}</TableCell>
                          <TableCell align="right">{user.tasksInProgress}</TableCell>
                          <TableCell align="right">{user.overdueTasks}</TableCell>
                          <TableCell align="right">{user.hoursWorked?.toFixed(1) || 0}</TableCell>
                        </TableRow>
                      ))}
                      {(!analytics?.userPerformance || analytics.userPerformance.length === 0) && (
                        <TableRow>
                          <TableCell colSpan={6} align="center">
                            <Typography color="text.secondary">No hay datos de usuarios</Typography>
                          </TableCell>
                        </TableRow>
                      )}
                    </TableBody>
                  </Table>
                </TableContainer>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}
    </Box>
  );
}
