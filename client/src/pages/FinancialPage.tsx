import React, { useState } from 'react';
import {
  Box, Card, CardContent, Typography, Grid, TextField, Button,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow,
  Chip, Dialog, DialogTitle, DialogContent, DialogActions,
  FormControl, InputLabel, Select, MenuItem, IconButton, Tabs, Tab,
  Pagination, alpha
} from '@mui/material';
import { Add, Delete, TrendingUp, TrendingDown, AttachMoney } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useGetFinancialTransactionsQuery, useGetFinancialReportQuery, 
  useGetExpenseCategoriesQuery, useCreateFinancialTransactionMutation,
  useGetProjectsQuery } from '../api';
import { TransactionType, Project } from '../types';

export default function FinancialPage() {
  const { t } = useTranslation();
  const [tab, setTab] = useState(0);
  const [page, setPage] = useState(1);
  const [openDialog, setOpenDialog] = useState(false);
  const [formData, setFormData] = useState({
    projectId: '',
    categoryId: '',
    amount: '',
    description: '',
    date: new Date().toISOString().split('T')[0],
    type: TransactionType.Expense
  });

  const { data: transactions, isLoading } = useGetFinancialTransactionsQuery({
    page,
    pageSize: 10,
    type: tab === 0 ? undefined : tab === 1 ? TransactionType.Income : TransactionType.Expense
  });
  const { data: report } = useGetFinancialReportQuery({});
  const { data: categories } = useGetExpenseCategoriesQuery(null);
  const { data: projects } = useGetProjectsQuery({ pageSize: 100 });
  const [createTransaction] = useCreateFinancialTransactionMutation();

  const filteredCategories = categories?.filter(c => 
    (formData.type === TransactionType.Income ? c.isIncome : !c.isIncome)
  );

  const handleSubmit = async () => {
    await createTransaction({
      projectId: formData.projectId,
      categoryId: formData.categoryId,
      amount: parseFloat(formData.amount),
      description: formData.description,
      date: formData.date,
      type: formData.type
    });
    setOpenDialog(false);
    setFormData({
      projectId: '',
      categoryId: '',
      amount: '',
      description: '',
      date: new Date().toISOString().split('T')[0],
      type: TransactionType.Expense
    });
  };

  const formatCurrency = (value: number) => 
    new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'USD' }).format(value);

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">{t('financial.title')}</Typography>
        <Button variant="contained" startIcon={<Add />} onClick={() => setOpenDialog(true)}>
          {t('financial.newTransaction')}
        </Button>
      </Box>

      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} md={4}>
          <Card sx={{ bgcolor: alpha('#4caf50', 0.1) }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Box sx={{ p: 1.5, bgcolor: '#4caf50', borderRadius: 1 }}>
                  <TrendingUp sx={{ color: 'white' }} />
                </Box>
                <Box>
                  <Typography variant="body2" color="text.secondary">{t('financial.income')}</Typography>
                  <Typography variant="h5">{formatCurrency(report?.totalIncome || 0)}</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={4}>
          <Card sx={{ bgcolor: alpha('#f44336', 0.1) }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Box sx={{ p: 1.5, bgcolor: '#f44336', borderRadius: 1 }}>
                  <TrendingDown sx={{ color: 'white' }} />
                </Box>
                <Box>
                  <Typography variant="body2" color="text.secondary">{t('financial.expenses')}</Typography>
                  <Typography variant="h5">{formatCurrency(report?.totalExpenses || 0)}</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={4}>
          <Card sx={{ bgcolor: alpha('#2196f3', 0.1) }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                <Box sx={{ p: 1.5, bgcolor: '#2196f3', borderRadius: 1 }}>
                  <AttachMoney sx={{ color: 'white' }} />
                </Box>
                <Box>
                  <Typography variant="body2" color="text.secondary">{t('financial.balance')}</Typography>
                  <Typography variant="h5">{formatCurrency(report?.balance || 0)}</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Card>
        <Tabs value={tab} onChange={(_, v) => setTab(v)} sx={{ px: 2 }}>
          <Tab label={t('common.all')} />
          <Tab label={t('financial.income')} />
          <Tab label={t('financial.expenses')} />
        </Tabs>
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>{t('financial.date')}</TableCell>
                <TableCell>{t('financial.description')}</TableCell>
                <TableCell>{t('projects.title')}</TableCell>
                <TableCell>{t('financial.category')}</TableCell>
                <TableCell align="right">{t('financial.amount')}</TableCell>
                <TableCell>{t('common.status')}</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {transactions?.items.map((tx) => (
                <TableRow key={tx.id} hover>
                  <TableCell>{new Date(tx.date).toLocaleDateString()}</TableCell>
                  <TableCell>{tx.description}</TableCell>
                  <TableCell>{tx.projectName}</TableCell>
                  <TableCell>{tx.categoryName}</TableCell>
                  <TableCell align="right" sx={{ 
                    color: tx.type === TransactionType.Income ? '#4caf50' : '#f44336',
                    fontWeight: 600
                  }}>
                    {tx.type === TransactionType.Income ? '+' : '-'}{formatCurrency(tx.amount)}
                  </TableCell>
                  <TableCell>
                    <Chip 
                      label={tx.type === TransactionType.Income ? t('financial.income') : t('financial.expenses')}
                      size="small"
                      color={tx.type === TransactionType.Income ? 'success' : 'error'}
                    />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
        <Box sx={{ p: 2, display: 'flex', justifyContent: 'center' }}>
          <Pagination 
            count={transactions?.totalPages || 1} 
            page={page} 
            onChange={(_, p) => setPage(p)}
          />
        </Box>
      </Card>

      <Dialog open={openDialog} onClose={() => setOpenDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{t('financial.newTransaction')}</DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>{t('financial.type')}</InputLabel>
                <Select
                  value={formData.type}
                  label={t('financial.type')}
                  onChange={(e) => setFormData({ ...formData, type: e.target.value as TransactionType, categoryId: '' })}
                >
                  <MenuItem value={TransactionType.Income}>{t('financial.income')}</MenuItem>
                  <MenuItem value={TransactionType.Expense}>{t('financial.expenses')}</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label={t('financial.amount')}
                type="number"
                value={formData.amount}
                onChange={(e) => setFormData({ ...formData, amount: e.target.value })}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                fullWidth
                label={t('financial.date')}
                type="date"
                value={formData.date}
                onChange={(e) => setFormData({ ...formData, date: e.target.value })}
                InputLabelProps={{ shrink: true }}
              />
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>{t('projects.title')}</InputLabel>
                <Select
                  value={formData.projectId}
                  label={t('projects.title')}
                  onChange={(e) => setFormData({ ...formData, projectId: e.target.value })}
                >
                  <MenuItem value="">-</MenuItem>
                  {projects?.items.map((p) => (
                    <MenuItem key={p.id} value={p.id}>{p.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <FormControl fullWidth>
                <InputLabel>{t('financial.category')}</InputLabel>
                <Select
                  value={formData.categoryId}
                  label={t('financial.category')}
                  onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                >
                  {filteredCategories?.map((c) => (
                    <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label={t('financial.description')}
                multiline
                rows={3}
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              />
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
