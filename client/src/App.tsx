import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { Box, Typography } from '@mui/material';
import { useAppSelector } from './hooks/useRedux';
import MainLayout from './components/layout/MainLayout';
import AuthPage from './pages/AuthPage';
import DashboardPage from './pages/DashboardPage';
import ProjectsPage from './pages/ProjectsPage';
import TasksPage from './pages/TasksPage';
import CalendarPage from './pages/CalendarPage';
import FinancialPage from './pages/FinancialPage';
import ResourcesPage from './pages/ResourcesPage';
import AnalyticsPage from './pages/AnalyticsPage';
import ProfilePage from './pages/ProfilePage';
import UsersPage from './pages/UsersPage';
import SettingsPage from './pages/SettingsPage';
import RolesPermissionsPage from './pages/RolesPermissionsPage';
import ReportsPage from './pages/ReportsPage';
import AuthInitializer from './components/AuthInitializer';
import { useTranslation } from 'react-i18next';

function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const isAuthenticated = useAppSelector((state) => state.auth.isAuthenticated);
  return isAuthenticated ? <>{children}</> : <Navigate to="/auth" />;
}

function AppRoutes() {
  const { t } = useTranslation();
  
  return (
    <AuthInitializer>
      <Routes>
        <Route path="/auth" element={<AuthPage />} />
        <Route path="/" element={<ProtectedRoute><MainLayout /></ProtectedRoute>}>
          <Route index element={<DashboardPage />} />
          <Route path="projects" element={<ProjectsPage />} />
          <Route path="tasks" element={<TasksPage />} />
          <Route path="calendar" element={<CalendarPage />} />
          <Route path="financial" element={<FinancialPage />} />
          <Route path="resources" element={<ResourcesPage />} />
          <Route path="analytics" element={<AnalyticsPage />} />
          <Route path="users" element={<UsersPage />} />
          <Route path="reports" element={<ReportsPage />} />
          <Route path="settings" element={<SettingsPage />} />
          <Route path="roles-permissions" element={<RolesPermissionsPage />} />
          <Route path="notifications" element={<Box sx={{ p: 3 }}><Typography variant="h4">{t('nav.notifications')}</Typography></Box>} />
          <Route path="profile" element={<ProfilePage />} />
        </Route>
      </Routes>
    </AuthInitializer>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <AppRoutes />
    </BrowserRouter>
  );
}
