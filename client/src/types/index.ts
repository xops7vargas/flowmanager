export enum ProjectStatus {
  Planning = 0,
  InProgress = 1,
  OnHold = 2,
  AtRisk = 3,
  Delayed = 4,
  Completed = 5,
  Cancelled = 6
}

export enum TaskStatus {
  Todo = 0,
  InProgress = 1,
  InReview = 2,
  Completed = 3,
  Blocked = 4
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export enum DelayCategory {
  Technical = 0,
  Organizational = 1,
  External = 2,
  Requirements = 3,
  Resources = 4
}

export enum NotificationType {
  TaskAssigned = 0,
  TaskUpdated = 1,
  TaskCompleted = 2,
  TaskDueSoon = 3,
  TaskOverdue = 4,
  CommentAdded = 5,
  ProjectUpdated = 6,
  Mention = 7
}

export enum DependencyType {
  FinishToStart = 0,
  StartToStart = 1,
  FinishToFinish = 2,
  StartToFinish = 3
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  avatar?: string;
  phone?: string;
  bio?: string;
  isActive: boolean;
  roles: string[];
  permissions: string[];
  createdAt: string;
}

export interface Role {
  id: string;
  name: string;
  description?: string;
  isSystem: boolean;
  permissions: Permission[];
}

export interface Permission {
  id: string;
  name: string;
  module: string;
  description?: string;
}

export interface Project {
  id: string;
  name: string;
  description?: string;
  status: ProjectStatus;
  startDate?: string;
  endDate?: string;
  budget?: number;
  progress: number;
  ownerId: string;
  ownerName: string;
  taskCount: number;
  completedTaskCount: number;
  createdAt: string;
}

export interface ProjectMember {
  userId: string;
  userName: string;
  avatar?: string;
  roleInProject?: string;
  joinedAt: string;
}

export interface Task {
  id: string;
  projectId: string;
  projectName: string;
  parentTaskId?: string;
  parentTaskTitle?: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  startDate?: string;
  dueDate?: string;
  estimatedHours: number;
  actualHours: number;
  order: number;
  createdById: string;
  createdByName: string;
  assignedToId?: string;
  assignedToName?: string;
  tags: Tag[];
  subtaskCount: number;
  completedSubtaskCount: number;
  isOverdue: boolean;
  createdAt: string;
}

export interface Tag {
  id: string;
  name: string;
  color: string;
}

export interface TimeEntry {
  id: string;
  taskId: string;
  taskTitle: string;
  userId: string;
  userName: string;
  hours: number;
  date: string;
  description?: string;
  createdAt: string;
}

export interface Comment {
  id: string;
  taskId: string;
  userId: string;
  userName: string;
  userAvatar?: string;
  content: string;
  parentId?: string;
  replies: Comment[];
  createdAt: string;
}

export interface Notification {
  id: string;
  title: string;
  message?: string;
  type: NotificationType;
  referenceId?: string;
  isRead: boolean;
  createdAt: string;
}

export interface Delay {
  id: string;
  taskId: string;
  taskTitle: string;
  reason: string;
  category: DelayCategory;
  daysDelayed: number;
  createdById: string;
  createdByName: string;
  createdAt: string;
}

export interface Workflow {
  id: string;
  projectId: string;
  name: string;
  description?: string;
  isDefault: boolean;
  transitions: WorkflowTransition[];
}

export interface WorkflowTransition {
  id: string;
  fromStatus: TaskStatus;
  toStatus: TaskStatus;
  requiredRoleId?: string;
  requiredRoleName?: string;
}

export interface Dashboard {
  totalProjects: number;
  activeProjects: number;
  totalTasks: number;
  completedTasks: number;
  overdueTasks: number;
  totalHoursWorked: number;
  pendingTasks: number;
  inProgressTasks: number;
  projectProgress: ProjectProgress[];
  tasksByPriority: TaskByPriority[];
}

export interface ProjectProgress {
  projectId: string;
  projectName: string;
  progress: number;
  totalTasks: number;
  completedTasks: number;
}

export interface TaskByPriority {
  priority: TaskPriority;
  count: number;
}

export interface CalendarEvent {
  id: string;
  title: string;
  start: string;
  end: string;
  color: string;
  description?: string;
  status: TaskStatus;
  projectId: string;
  projectName: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role?: string;
}

export enum TransactionType {
  Income = 0,
  Expense = 1
}

export enum ResourceType {
  Equipment = 0,
  Furniture = 1,
  Electronics = 2,
  Vehicles = 3,
  Tools = 4,
  OfficeSupplies = 5,
  Other = 6
}

export enum ResourceStatus {
  Available = 0,
  InUse = 1,
  Damaged = 2,
  UnderMaintenance = 3,
  Retired = 4
}

export enum MovementType {
  Entry = 0,
  Exit = 1,
  Assignment = 2,
  Return = 3,
  Maintenance = 4,
  Retirement = 5
}

export enum ConversationType {
  Direct = 0,
  Group = 1
}

export enum MessageType {
  Text = 0,
  File = 1,
  Image = 2
}

export interface ExpenseCategory {
  id: string;
  name: string;
  description: string;
  color: string;
  isIncome: boolean;
  parentId?: string;
}

export interface FinancialTransaction {
  id: string;
  projectId: string;
  projectName: string;
  categoryId: string;
  categoryName: string;
  userId?: string;
  userName?: string;
  amount: number;
  description: string;
  date: string;
  type: TransactionType;
  reference?: string;
}

export interface FinancialReport {
  totalIncome: number;
  totalExpenses: number;
  balance: number;
  transactions: FinancialTransaction[];
  byCategory: Record<string, number>;
  byMonth: Record<string, number>;
}

export interface Resource {
  id: string;
  name: string;
  description: string;
  code: string;
  type: ResourceType;
  status: ResourceStatus;
  quantity: number;
  availableQuantity: number;
  unitValue: number;
  assignedToId?: string;
  assignedToName?: string;
  location?: string;
  purchaseDate?: string;
}

export interface ResourceMovement {
  id: string;
  resourceId: string;
  resourceName: string;
  userId: string;
  userName: string;
  projectId?: string;
  projectName?: string;
  type: MovementType;
  quantity: number;
  notes?: string;
  createdAt: string;
}

export interface Conversation {
  id: string;
  type: ConversationType;
  name: string;
  participants: ConversationParticipant[];
  lastMessage?: Message;
  lastMessageAt: string;
  unreadCount: number;
  participantIds?: string[];
}

export interface ConversationParticipant {
  userId: string;
  userName: string;
  avatar?: string;
  isOnline: boolean;
}

export interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderAvatar?: string;
  content: string;
  type: MessageType;
  replyToId?: string;
  replyToContent?: string;
  createdAt: string;
}

export interface ComplianceMetrics {
  completionRate: number;
  complianceRate: number;
  overdueRate: number;
  totalTasks: number;
  completedTasks: number;
  overdueTasks: number;
}

export interface UserPerformance {
  userId: string;
  userName: string;
  avatar?: string;
  tasksCompleted: number;
  tasksInProgress: number;
  overdueTasks: number;
  hoursWorked: number;
  completionRate: number;
}

export interface ProjectMetrics {
  projectId: string;
  projectName: string;
  totalTasks: number;
  completedTasks: number;
  overdueTasks: number;
  progress: number;
  budget: number;
  spent: number;
}

export interface MonthlyData {
  month: string;
  tasksCreated: number;
  tasksCompleted: number;
  hoursWorked: number;
  income: number;
  expenses: number;
}

export interface Analytics {
  compliance: ComplianceMetrics;
  userPerformance: UserPerformance[];
  projectMetrics: ProjectMetrics[];
  monthlyData: MonthlyData[];
  priorityDistribution: { priority: TaskPriority; count: number; percentage: number }[];
}

export interface SystemSetting {
  id: string;
  key: string;
  value: string;
  description: string;
  type: 'String' | 'Boolean' | 'Number' | 'Json';
}