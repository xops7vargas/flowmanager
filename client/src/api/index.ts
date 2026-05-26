import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { 
  AuthResponse, LoginRequest, RegisterRequest, User, Project, Task, 
  PaginatedResult, TimeEntry, Comment, Notification, Tag, Dashboard, 
  CalendarEvent, Role, Permission, Workflow, Delay, ProjectMember,
  ExpenseCategory, FinancialTransaction, FinancialReport, Resource, ResourceMovement,
  Conversation, Message, Analytics, SystemSetting, TransactionType
} from '../types';

const baseQuery = fetchBaseQuery({
  baseUrl: '/api',
  prepareHeaders: (headers) => {
    const token = localStorage.getItem('token');
    if (token) {
      headers.set('authorization', `Bearer ${token}`);
    }
    return headers;
  },
});

export const api = createApi({
  reducerPath: 'api',
  baseQuery,
  tagTypes: ['User', 'Project', 'Task', 'TimeEntry', 'Comment', 'Notification', 'Tag', 'Dashboard', 'Workflow', 'Delay', 'Financial', 'Resource', 'Chat', 'Analytics', 'Settings'],
  endpoints: (builder) => ({
    login: builder.mutation<AuthResponse, LoginRequest>({
      query: (body) => ({ url: '/auth/login', method: 'POST', body }),
    }),
    register: builder.mutation<AuthResponse, RegisterRequest>({
      query: (body) => ({ url: '/auth/register', method: 'POST', body }),
      invalidatesTags: ['User'],
    }),
    getCurrentUser: builder.query<User, void>({
      query: () => '/auth/me',
      providesTags: ['User'],
    }),
    updateUser: builder.mutation<User, { id: string; firstName?: string; lastName?: string; phone?: string; bio?: string; avatar?: string; roleId?: string; isActive?: boolean }>({
      query: ({ id, ...body }) => ({ url: `/users/${id}`, method: 'PUT', body }),
      invalidatesTags: ['User'],
    }),
    updateUserRole: builder.mutation<User, { id: string; roleId: string }>({
      query: ({ id, roleId }) => ({ url: `/user-roles/${id}`, method: 'PUT', body: { roleId } }),
      invalidatesTags: ['User'],
    }),
    getUsers: builder.query<PaginatedResult<User>, { page?: number; pageSize?: number; search?: string }>({
      query: ({ page = 1, pageSize = 20, search }) => `/users?page=${page}&pageSize=${pageSize}${search ? `&search=${search}` : ''}`,
      providesTags: ['User'],
    }),
    activateUser: builder.mutation<void, string>({
      query: (id) => ({ url: `/users/${id}/activate`, method: 'POST' }),
      invalidatesTags: ['User'],
    }),
    deactivateUser: builder.mutation<void, string>({
      query: (id) => ({ url: `/users/${id}/deactivate`, method: 'POST' }),
      invalidatesTags: ['User'],
    }),
    getProjects: builder.query<PaginatedResult<Project>, { page?: number; pageSize?: number; status?: number }>({
      query: ({ page = 1, pageSize = 20, status }) => `/projects?page=${page}&pageSize=${pageSize}${status !== undefined ? `&status=${status}` : ''}`,
      providesTags: ['Project'],
    }),
    getProjectById: builder.query<Project, string>({
      query: (id) => `/projects/${id}`,
      providesTags: (result, error, id) => [{ type: 'Project', id }],
    }),
    createProject: builder.mutation<Project, Partial<Project>>({
      query: (body) => ({ url: '/projects', method: 'POST', body }),
      invalidatesTags: ['Project'],
    }),
    updateProject: builder.mutation<Project, { id: string; body: Partial<Project> }>({
      query: ({ id, body }) => ({ url: `/projects/${id}`, method: 'PUT', body }),
      invalidatesTags: (result, error, { id }) => [{ type: 'Project', id }],
    }),
    deleteProject: builder.mutation<void, string>({
      query: (id) => ({ url: `/projects/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Project'],
    }),
    getProjectMembers: builder.query<ProjectMember[], string>({
      query: (id) => `/projects/${id}/members`,
    }),
    addProjectMember: builder.mutation<void, { projectId: string; userId: string; role?: string }>({
      query: ({ projectId, userId, role }) => ({ url: `/projects/${projectId}/members`, method: 'POST', body: { userId, role } }),
      invalidatesTags: (result, error, { projectId }) => [{ type: 'Project', id: projectId }],
    }),
    getTasks: builder.query<PaginatedResult<Task>, { page?: number; pageSize?: number; projectId?: string; status?: number; assignedToId?: string }>({
      query: ({ page = 1, pageSize = 20, projectId, status, assignedToId }) => {
        let url = `/tasks?page=${page}&pageSize=${pageSize}`;
        if (projectId) url += `&projectId=${projectId}`;
        if (status !== undefined) url += `&status=${status}`;
        if (assignedToId) url += `&assignedToId=${assignedToId}`;
        return url;
      },
      providesTags: ['Task'],
    }),
    getTaskById: builder.query<Task, string>({
      query: (id) => `/tasks/${id}`,
      providesTags: (result, error, id) => [{ type: 'Task', id }],
    }),
    createTask: builder.mutation<Task, Partial<Task>>({
      query: (body) => ({ url: '/tasks', method: 'POST', body }),
      invalidatesTags: ['Task', 'Dashboard'],
    }),
    updateTask: builder.mutation<Task, { id: string; body: Partial<Task> }>({
      query: ({ id, body }) => ({ url: `/tasks/${id}`, method: 'PUT', body }),
      invalidatesTags: (result, error, { id }) => [{ type: 'Task', id }],
    }),
    deleteTask: builder.mutation<void, string>({
      query: (id) => ({ url: `/tasks/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Task'],
    }),
    updateTaskStatus: builder.mutation<void, { id: string; status: number }>({
      query: ({ id, status }) => ({ url: `/tasks/${id}/status`, method: 'PUT', body: { status } }),
      invalidatesTags: ['Task', 'Dashboard'],
    }),
    getTimeEntries: builder.query<PaginatedResult<TimeEntry>, { page?: number; pageSize?: number; taskId?: string; userId?: string }>({
      query: ({ page = 1, pageSize = 20, taskId, userId }) => {
        let url = `/time-entries?page=${page}&pageSize=${pageSize}`;
        if (taskId) url += `&taskId=${taskId}`;
        if (userId) url += `&userId=${userId}`;
        return url;
      },
      providesTags: ['TimeEntry'],
    }),
    createTimeEntry: builder.mutation<TimeEntry, Partial<TimeEntry>>({
      query: (body) => ({ url: '/time-entries', method: 'POST', body }),
      invalidatesTags: ['TimeEntry', 'Dashboard'],
    }),
    getComments: builder.query<Comment[], string>({
      query: (taskId) => `/comments/task/${taskId}`,
      providesTags: ['Comment'],
    }),
    createComment: builder.mutation<Comment, Partial<Comment>>({
      query: (body) => ({ url: '/comments', method: 'POST', body }),
      invalidatesTags: ['Comment'],
    }),
    deleteComment: builder.mutation<void, string>({
      query: (id) => ({ url: `/comments/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Comment'],
    }),
    getNotifications: builder.query<Notification[], boolean>({
      query: (unreadOnly) => `/notifications?unreadOnly=${unreadOnly}`,
      providesTags: ['Notification'],
    }),
    getUnreadCount: builder.query<number, void>({
      query: () => '/notifications/unread-count',
      providesTags: ['Notification'],
    }),
    markNotificationRead: builder.mutation<void, string>({
      query: (id) => ({ url: `/notifications/${id}/read`, method: 'PUT' }),
      invalidatesTags: ['Notification'],
    }),
    markAllNotificationsRead: builder.mutation<void, void>({
      query: () => ({ url: '/notifications/read-all', method: 'PUT' }),
      invalidatesTags: ['Notification'],
    }),
    getTags: builder.query<Tag[], void>({
      query: () => '/tags',
      providesTags: ['Tag'],
    }),
    createTag: builder.mutation<Tag, Partial<Tag>>({
      query: (body) => ({ url: '/tags', method: 'POST', body }),
      invalidatesTags: ['Tag'],
    }),
    deleteTag: builder.mutation<void, string>({
      query: (id) => ({ url: `/tags/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Tag'],
    }),
    getDashboard: builder.query<Dashboard, void>({
      query: () => '/dashboard',
      providesTags: ['Dashboard'],
    }),
    getCalendarEvents: builder.query<CalendarEvent[], { start: string; end: string }>({
      query: ({ start, end }) => `/calendar/events?start=${start}&end=${end}`,
    }),
    getRoles: builder.query<Role[], void>({
      query: () => '/roles',
    }),
    getPermissions: builder.query<Permission[], void>({
      query: () => '/permissions',
    }),
    updateRolePermissions: builder.mutation<void, { roleId: string; permissions: string[] }>({
      query: ({ roleId, permissions }) => ({
        url: `/roles/${roleId}/permissions`,
        method: 'POST',
        body: permissions,
      }),
    }),
    getDelays: builder.query<Delay[], { taskId?: string; page?: number; pageSize?: number; category?: number }>({
      query: ({ taskId, page = 1, pageSize = 20, category }) => {
        let url = `/delays?page=${page}&pageSize=${pageSize}`;
        if (taskId) url += `&taskId=${taskId}`;
        if (category !== undefined) url += `&category=${category}`;
        return url;
      },
      providesTags: ['Delay'],
    }),
    createDelay: builder.mutation<Delay, Partial<Delay>>({
      query: (body) => ({ url: '/delays', method: 'POST', body }),
      invalidatesTags: ['Delay'],
    }),
    getExpenseCategories: builder.query<ExpenseCategory[], boolean | null>({
      query: (isIncome) => `/financial/categories${isIncome !== null ? `?isIncome=${isIncome}` : ''}`,
      providesTags: ['Financial'],
    }),
    createExpenseCategory: builder.mutation<ExpenseCategory, Partial<ExpenseCategory>>({
      query: (body) => ({ url: '/financial/categories', method: 'POST', body }),
      invalidatesTags: ['Financial'],
    }),
    deleteExpenseCategory: builder.mutation<void, string>({
      query: (id) => ({ url: `/financial/categories/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Financial'],
    }),
    getFinancialTransactions: builder.query<PaginatedResult<FinancialTransaction>, { page?: number; pageSize?: number; projectId?: string; startDate?: string; endDate?: string; type?: TransactionType; categoryId?: string }>({
      query: ({ page = 1, pageSize = 20, projectId, startDate, endDate, type, categoryId }) => {
        let url = `/financial/transactions?page=${page}&pageSize=${pageSize}`;
        if (projectId) url += `&projectId=${projectId}`;
        if (startDate) url += `&startDate=${startDate}`;
        if (endDate) url += `&endDate=${endDate}`;
        if (type !== undefined) url += `&type=${type}`;
        if (categoryId) url += `&categoryId=${categoryId}`;
        return url;
      },
      providesTags: ['Financial'],
    }),
    createFinancialTransaction: builder.mutation<FinancialTransaction, Partial<FinancialTransaction>>({
      query: (body) => ({ url: '/financial/transactions', method: 'POST', body }),
      invalidatesTags: ['Financial'],
    }),
    deleteFinancialTransaction: builder.mutation<void, string>({
      query: (id) => ({ url: `/financial/transactions/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Financial'],
    }),
    getFinancialReport: builder.query<FinancialReport, { projectId?: string; startDate?: string; endDate?: string }>({
      query: ({ projectId, startDate, endDate }) => {
        let url = '/financial/report?';
        if (projectId) url += `projectId=${projectId}&`;
        if (startDate) url += `startDate=${startDate}&`;
        if (endDate) url += `endDate=${endDate}`;
        return url;
      },
      providesTags: ['Financial'],
    }),
    getResources: builder.query<PaginatedResult<Resource>, { page?: number; pageSize?: number; type?: number; status?: number; search?: string }>({
      query: ({ page = 1, pageSize = 20, type, status, search }) => {
        let url = `/resources?page=${page}&pageSize=${pageSize}`;
        if (type !== undefined) url += `&type=${type}`;
        if (status !== undefined) url += `&status=${status}`;
        if (search) url += `&search=${search}`;
        return url;
      },
      providesTags: ['Resource'],
    }),
    getResourceById: builder.query<Resource, string>({
      query: (id) => `/resources/${id}`,
      providesTags: (result, error, id) => [{ type: 'Resource', id }],
    }),
    createResource: builder.mutation<Resource, Partial<Resource>>({
      query: (body) => ({ url: '/resources', method: 'POST', body }),
      invalidatesTags: ['Resource'],
    }),
    updateResource: builder.mutation<Resource, { id: string; body: Partial<Resource> }>({
      query: ({ id, body }) => ({ url: `/resources/${id}`, method: 'PUT', body }),
      invalidatesTags: (result, error, { id }) => [{ type: 'Resource', id }],
    }),
    deleteResource: builder.mutation<void, string>({
      query: (id) => ({ url: `/resources/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Resource'],
    }),
    createResourceMovement: builder.mutation<ResourceMovement, Partial<ResourceMovement>>({
      query: (body) => ({ url: '/resources/movements', method: 'POST', body }),
      invalidatesTags: ['Resource'],
    }),
    getResourceMovements: builder.query<ResourceMovement[], string>({
      query: (resourceId) => `/resources/${resourceId}/movements`,
      providesTags: ['Resource'],
    }),
    getConversations: builder.query<Conversation[], void>({
      query: () => '/chat/conversations',
      providesTags: ['Chat'],
    }),
    getConversationMessages: builder.query<Message[], { conversationId: string; page?: number; pageSize?: number }>({
      query: ({ conversationId, page = 1, pageSize = 50 }) => `/chat/conversations/${conversationId}/messages?page=${page}&pageSize=${pageSize}`,
      providesTags: ['Chat'],
    }),
    createConversation: builder.mutation<Conversation, Partial<Conversation>>({
      query: (body) => ({ url: '/chat/conversations', method: 'POST', body }),
      invalidatesTags: ['Chat'],
    }),
    sendMessage: builder.mutation<Message, Partial<Message>>({
      query: (body) => ({ url: '/chat/messages', method: 'POST', body }),
      invalidatesTags: ['Chat'],
    }),
    markConversationRead: builder.mutation<void, string>({
      query: (conversationId) => ({ url: `/chat/conversations/${conversationId}/read`, method: 'POST' }),
      invalidatesTags: ['Chat'],
    }),
    getAnalytics: builder.query<Analytics, { userId?: string; startDate?: string; endDate?: string }>({
      query: ({ userId, startDate, endDate }) => {
        let url = '/analytics?';
        if (userId) url += `userId=${userId}&`;
        if (startDate) url += `startDate=${startDate}&`;
        if (endDate) url += `endDate=${endDate}`;
        return url;
      },
      providesTags: ['Analytics'],
    }),
    getSettings: builder.query<SystemSetting[], void>({
      query: () => '/settings',
      providesTags: ['Settings'],
    }),
    updateSetting: builder.mutation<SystemSetting, { key: string; value: string }>({
      query: ({ key, value }) => ({ url: `/settings/${key}`, method: 'PUT', body: { value } }),
      invalidatesTags: ['Settings'],
    }),
  }),
});

export const {
  useLoginMutation,
  useRegisterMutation,
  useGetCurrentUserQuery,
  useUpdateUserMutation,
  useUpdateUserRoleMutation,
  useGetUsersQuery,
  useActivateUserMutation,
  useDeactivateUserMutation,
  useGetProjectsQuery,
  useGetProjectByIdQuery,
  useCreateProjectMutation,
  useUpdateProjectMutation,
  useDeleteProjectMutation,
  useGetProjectMembersQuery,
  useAddProjectMemberMutation,
  useGetTasksQuery,
  useGetTaskByIdQuery,
  useCreateTaskMutation,
  useUpdateTaskMutation,
  useDeleteTaskMutation,
  useUpdateTaskStatusMutation,
  useGetTimeEntriesQuery,
  useCreateTimeEntryMutation,
  useGetCommentsQuery,
  useCreateCommentMutation,
  useDeleteCommentMutation,
  useGetNotificationsQuery,
  useGetUnreadCountQuery,
  useMarkNotificationReadMutation,
  useMarkAllNotificationsReadMutation,
  useGetTagsQuery,
  useCreateTagMutation,
  useDeleteTagMutation,
  useGetDashboardQuery,
  useGetCalendarEventsQuery,
  useGetRolesQuery,
  useGetPermissionsQuery,
  useGetDelaysQuery,
  useCreateDelayMutation,
  useGetExpenseCategoriesQuery,
  useCreateExpenseCategoryMutation,
  useDeleteExpenseCategoryMutation,
  useGetFinancialTransactionsQuery,
  useCreateFinancialTransactionMutation,
  useDeleteFinancialTransactionMutation,
  useGetFinancialReportQuery,
  useGetResourcesQuery,
  useGetResourceByIdQuery,
  useCreateResourceMutation,
  useUpdateResourceMutation,
  useDeleteResourceMutation,
  useCreateResourceMovementMutation,
  useGetResourceMovementsQuery,
  useGetConversationsQuery,
  useGetConversationMessagesQuery,
  useCreateConversationMutation,
  useSendMessageMutation,
  useMarkConversationReadMutation,
  useGetAnalyticsQuery,
  useGetSettingsQuery,
  useUpdateSettingMutation,
  useUpdateRolePermissionsMutation,
} = api;