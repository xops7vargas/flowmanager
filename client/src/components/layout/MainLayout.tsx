import React, { useEffect, useState } from 'react';
import { Outlet, useNavigate, useLocation } from 'react-router-dom';
import {
  Box,
  Drawer,
  AppBar,
  Toolbar,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Typography,
  IconButton,
  Badge,
  Avatar,
  Menu,
  MenuItem,
  Divider,
  useTheme,
  useMediaQuery,
  alpha,
  Tooltip,
  Collapse,
  Snackbar,
  Alert,
} from '@mui/material';
import {
  Dashboard,
  Folder,
  Assignment,
  CalendarMonth,
  People,
  BarChart,
  Settings,
  Notifications,
  Menu as MenuIcon,
  Logout,
  Person,
  AttachMoney,
  Inventory,
  Analytics,
  ChevronLeft,
  ChevronRight,
  ExpandLess,
  ExpandMore,
  Security,
  Chat as ChatIcon,
} from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useAppSelector, useAppDispatch } from '../../hooks/useRedux';
import { logout } from '../../features/auth/authSlice';
import { useGetUnreadCountQuery, useGetConversationsQuery } from '../../api';
import { LanguageSwitcher } from '../LanguageSwitcher';
import { ThemeToggle } from '../ThemeToggle';
import ChatWidget from '../Chat/ChatWidget';

const drawerWidth = 280;
const drawerCollapsedWidth = 72;

interface MenuItem {
  text: string;
  icon: React.ReactNode;
  path: string;
  roles?: string[];
  permission?: string;
}

const menuItems: MenuItem[] = [
  { text: 'nav.dashboard', icon: <Dashboard />, path: '/', permission: 'dashboard.view' },
  { text: 'nav.projects', icon: <Folder />, path: '/projects', permission: 'projects.read' },
  { text: 'nav.tasks', icon: <Assignment />, path: '/tasks', permission: 'tasks.read' },
  { text: 'nav.calendar', icon: <CalendarMonth />, path: '/calendar', permission: 'calendar.view' },
  { text: 'financial.title', icon: <AttachMoney />, path: '/financial', permission: 'reports.view' },
  { text: 'resources.title', icon: <Inventory />, path: '/resources', permission: 'projects.read' },
  { text: 'analytics.title', icon: <Analytics />, path: '/analytics', permission: 'reports.view' },
  { text: 'nav.users', icon: <People />, path: '/users', roles: ['Administrator', 'ProjectManager'], permission: 'users.read' },
  { text: 'roles.title', icon: <Security />, path: '/roles-permissions', roles: ['Administrator'] },
  { text: 'nav.reports', icon: <BarChart />, path: '/reports', permission: 'reports.view' },
  { text: 'nav.settings', icon: <Settings />, path: '/settings', roles: ['Administrator'], permission: 'settings.manage' },
];

export default function MainLayout() {
  const { t } = useTranslation();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [mobileOpen, setMobileOpen] = React.useState(false);
  const [collapsed, setCollapsed] = React.useState(false);
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  
  const navigate = useNavigate();
  const location = useLocation();
  const dispatch = useAppDispatch();
  const user = useAppSelector((state) => state.auth.user);
  const isAuthenticated = useAppSelector((state) => state.auth.isAuthenticated);
  
  const { data: unreadCount } = useGetUnreadCountQuery();
  const [conversationsData, setConversationsData] = useState<any[]>([]);
  const { data: conversations, refetch } = useGetConversationsQuery();
  
  const handleRefetch = () => {
    refetch();
  };

  useEffect(() => {
    const interval = setInterval(() => {
      refetch();
    }, 5000);
    return () => clearInterval(interval);
  }, [refetch]);

  useEffect(() => {
    if (conversations) {
      setConversationsData(conversations);
    }
  }, [conversations]);
  
  const totalUnreadMessages = conversationsData?.reduce((sum, c) => sum + (c.unreadCount || 0), 0) || 0;
  
  const [hasInitialized, setHasInitialized] = useState(false);
  const [lastMessageTime, setLastMessageTime] = useState<string>('');
  const [newMessageNotification, setNewMessageNotification] = useState<{ sender: string; preview: string } | null>(null);
  const [notificationOpen, setNotificationOpen] = useState(false);

  // Get the latest message across all conversations
  const latestMessage = conversationsData?.[0]?.lastMessage;
  const latestMessageTime = latestMessage?.createdAt || '';

  useEffect(() => {
    if (!latestMessage) return;
    
    if (!hasInitialized) {
      setLastMessageTime(latestMessageTime);
      setHasInitialized(true);
      return;
    }
    
    // Detect new message by comparing timestamps
    if (latestMessageTime > lastMessageTime) {
      const senderName = latestMessage.senderName || 'Nuevo mensaje';
      setNewMessageNotification({
        sender: senderName,
        preview: latestMessage.content?.substring(0, 50) || 'Te ha enviado un mensaje'
      });
      setNotificationOpen(true);
      
      // Try native notification
      if (Notification.permission === 'granted') {
        new Notification('Nuevo mensaje de ' + senderName, {
          body: latestMessage.content?.substring(0, 100) || 'Tienes un nuevo mensaje'
        });
      } else if (Notification.permission !== 'denied') {
        Notification.requestPermission().then(permission => {
          if (permission === 'granted') {
            new Notification('Nuevo mensaje de ' + senderName, {
              body: latestMessage.content?.substring(0, 100) || 'Tienes un nuevo mensaje'
            });
          }
        });
      }
    }
    setLastMessageTime(latestMessageTime);
  }, [latestMessageTime, hasInitialized, latestMessage]);

  const handleCloseNotification = () => {
    setNotificationOpen(false);
  };

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  const handleCollapseToggle = () => {
    setCollapsed(!collapsed);
  };

  const currentWidth = collapsed ? drawerCollapsedWidth : drawerWidth;

  const hasAnyRole = user?.roles && user.roles.length > 0;
  const hasAnyPermission = user?.permissions && user.permissions.length > 0;

  const filteredMenuItems = menuItems.filter(item => {
    if (item.roles) {
      const hasRole = user?.roles?.some(role => item.roles?.includes(role));
      if (!hasRole) return false;
    }
    if (item.permission) {
      const hasPermission = user?.permissions?.includes(item.permission);
      if (!hasPermission) return false;
    }
    if (!hasAnyRole && !hasAnyPermission) {
      return false;
    }
    return true;
  });

  const drawer = (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Box sx={{ 
        p: collapsed ? 1 : 2, 
        display: 'flex', 
        alignItems: 'center', 
        justifyContent: collapsed ? 'center' : 'space-between',
        minHeight: 64,
        borderBottom: `1px solid ${theme.palette.divider}`
      }}>
        {!collapsed && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Box sx={{ width: 32, height: 32, borderRadius: 1, bgcolor: 'primary.main', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Typography variant="h6" sx={{ color: 'white', fontWeight: 'bold' }}>S</Typography>
            </Box>
            <Typography variant="subtitle1" fontWeight="bold" noWrap>SonyiFlow</Typography>
          </Box>
        )}
        <IconButton onClick={handleCollapseToggle} size="small">
          {collapsed ? <ChevronRight /> : <ChevronLeft />}
        </IconButton>
      </Box>

      <List sx={{ flex: 1, px: 1, py: 2 }}>
        {filteredMenuItems.map((item) => (
          <Tooltip title={collapsed ? t(item.text) : ''} placement="right" key={item.path}>
            <ListItemButton
              onClick={() => navigate(item.path)}
              selected={location.pathname === item.path}
              sx={{
                borderRadius: 1,
                mb: 0.5,
                px: collapsed ? 1 : 2,
                justifyContent: collapsed ? 'center' : 'flex-start',
                minHeight: 44,
                '&.Mui-selected': {
                  bgcolor: alpha(theme.palette.primary.main, 0.15),
                  '&:hover': {
                    bgcolor: alpha(theme.palette.primary.main, 0.25),
                  },
                },
              }}
            >
              <ListItemIcon
                sx={{
                  minWidth: collapsed ? 0 : 40,
                  color: location.pathname === item.path ? 'primary.main' : 'inherit',
                  justifyContent: 'center',
                }}
              >
                {item.icon}
              </ListItemIcon>
              {!collapsed && (
                <ListItemText 
                  primary={t(item.text)} 
                  primaryTypographyProps={{ 
                    fontSize: '0.875rem',
                    fontWeight: location.pathname === item.path ? 600 : 400
                  }} 
                />
              )}
            </ListItemButton>
          </Tooltip>
        ))}
      </List>

      <Divider />
      
      <Box sx={{ p: 1 }}>
        <ListItemButton
          onClick={() => navigate('/profile')}
          sx={{ borderRadius: 1, px: collapsed ? 1 : 2, justifyContent: collapsed ? 'center' : 'flex-start' }}
        >
          <Avatar 
            src={user?.avatar || undefined} 
            sx={{ width: 32, height: 32, mr: collapsed ? 0 : 1 }}
          >
            {user?.firstName?.[0]}{user?.lastName?.[0]}
          </Avatar>
          {!collapsed && (
            <ListItemText 
              primary={`${user?.firstName || ''} ${user?.lastName || ''}`}
              secondary={user?.roles?.[0] || 'User'}
              primaryTypographyProps={{ fontSize: '0.8rem', fontWeight: 500 }}
              secondaryTypographyProps={{ fontSize: '0.7rem' }}
            />
          )}
        </ListItemButton>
      </Box>
    </Box>
  );

  return (
    <Box sx={{ display: 'flex' }}>
      <AppBar
        position="fixed"
        sx={{
          width: { md: `calc(100% - ${currentWidth}px)` },
          ml: { md: `${currentWidth}px` },
          boxShadow: 'none',
          borderBottom: `1px solid ${theme.palette.divider}`,
          bgcolor: theme.palette.background.paper,
          color: theme.palette.text.primary,
        }}
      >
        <Toolbar sx={{ justifyContent: 'space-between' }}>
          <IconButton
            color="inherit"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { md: 'none' } }}
          >
            <MenuIcon />
          </IconButton>
          
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <LanguageSwitcher />
            <ThemeToggle />
            <IconButton color="inherit" onClick={() => navigate('/notifications')}>
              <Badge 
                badgeContent={typeof unreadCount === 'number' ? unreadCount : Number(unreadCount) || 0} 
                color="error"
                showZero={false}
              >
                <Notifications />
              </Badge>
            </IconButton>
            {isAuthenticated && (
              <IconButton color="inherit" onClick={() => handleRefetch()}>
                <Badge 
                  badgeContent={totalUnreadMessages} 
                  color="error"
                  showZero={false}
                >
                  <ChatIcon />
                </Badge>
              </IconButton>
            )}
            <IconButton
              color="inherit"
              onClick={(e) => setAnchorEl(e.currentTarget)}
            >
              <Avatar src={user?.avatar || undefined} sx={{ width: 32, height: 32 }}>
                {user?.firstName?.[0]}{user?.lastName?.[0]}
              </Avatar>
            </IconButton>
          </Box>
          
          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={() => setAnchorEl(null)}
            transformOrigin={{ horizontal: 'right', vertical: 'top' }}
            anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
          >
            <MenuItem onClick={() => { navigate('/profile'); setAnchorEl(null); }}>
              <Person sx={{ mr: 1 }} /> {t('nav.profile')}
            </MenuItem>
            <Divider />
            <MenuItem onClick={() => dispatch(logout())}>
              <Logout sx={{ mr: 1 }} /> {t('common.logout')}
            </MenuItem>
          </Menu>
        </Toolbar>
      </AppBar>

      <Box
        component="nav"
        sx={{ width: { md: currentWidth }, flexShrink: { md: 0 } }}
      >
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: 'block', md: 'none' },
            '& .MuiDrawer-paper': { boxSizing: 'border-box', width: drawerWidth },
          }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', md: 'block' },
            '& .MuiDrawer-paper': { 
              boxSizing: 'border-box', 
              width: currentWidth,
              transition: theme.transitions.create('width', {
                easing: theme.transitions.easing.sharp,
                duration: theme.transitions.duration.enteringScreen,
              }),
              borderRight: 'none',
              bgcolor: 'transparent',
            },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>

      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 2,
          width: { md: `calc(100% - ${currentWidth}px)` },
          mt: '64px',
          bgcolor: theme.palette.background.default,
          minHeight: 'calc(100vh - 64px)',
        }}
      >
        <Outlet />
      </Box>

      <Snackbar
        open={notificationOpen}
        autoHideDuration={6000}
        onClose={handleCloseNotification}
        anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
        sx={{ mt: 7 }}
      >
        <Alert 
          onClose={handleCloseNotification} 
          severity="info" 
          sx={{ width: '100%' }}
        >
          <Typography variant="body2" fontWeight="bold">
            {newMessageNotification?.sender}
          </Typography>
          <Typography variant="caption">
            {newMessageNotification?.preview}
          </Typography>
        </Alert>
      </Snackbar>

      {isAuthenticated && (
        <ChatWidget 
          conversations={conversationsData || []} 
          onRefetch={handleRefetch} 
        />
      )}
    </Box>
  );
}
