using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectFlow.Application.DTOs;
using ProjectStatus = ProjectFlow.Domain.Enums.ProjectStatus;
using TaskStatus = ProjectFlow.Domain.Enums.TaskStatus;
using DependencyType = ProjectFlow.Domain.Enums.DependencyType;
using NotificationType = ProjectFlow.Domain.Enums.NotificationType;
using DelayCategory = ProjectFlow.Domain.Enums.DelayCategory;
using TransactionType = ProjectFlow.Domain.Enums.TransactionType;
using ResourceType = ProjectFlow.Domain.Enums.ResourceType;
using ResourceStatus = ProjectFlow.Domain.Enums.ResourceStatus;

namespace ProjectFlow.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RegisterAsync(CreateUserDto dto);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto);
    Task RevokeTokenAsync(Guid userId);
    Task<UserDto> GetCurrentUserAsync(Guid userId);
}

public interface IUserService
{
    Task<PaginatedResultDto<UserDto>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null);
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto> CreateWithRoleAsync(CreateUserWithRoleDto dto);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto);
    Task<UserDto> UpdateUserRoleAsync(Guid userId, Guid roleId);
    Task DeleteAsync(Guid id);
    Task AssignRolesAsync(Guid userId, List<Guid> roleIds);
    Task<List<ProjectMemberDto>> GetUserProjectsAsync(Guid userId);
    Task ActivateAsync(Guid id);
    Task DeactivateAsync(Guid id);
}

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetAllAsync();
    Task<RoleDto> GetByIdAsync(Guid id);
    Task<RoleDto> CreateAsync(CreateRoleDto dto);
    Task<RoleDto> UpdateAsync(Guid id, CreateRoleDto dto);
    Task DeleteAsync(Guid id);
    Task AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds);
    Task InitializeDefaultRolesAsync();
    Task InitializeDefaultAdminAsync();
}

public interface IPermissionService
{
    Task<IEnumerable<PermissionDto>> GetAllAsync();
    Task<IEnumerable<PermissionDto>> GetByModuleAsync(string module);
    Task InitializeDefaultPermissionsAsync();
    Task<bool> HasPermissionAsync(Guid userId, string permission);
}

public interface IProjectService
{
    Task<PaginatedResultDto<ProjectDto>> GetAllAsync(int page = 1, int pageSize = 20, ProjectStatus? status = null, Guid? userId = null);
    Task<ProjectDto> GetByIdAsync(Guid id);
    Task<ProjectDto> CreateAsync(CreateProjectDto dto, Guid ownerId);
    Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto dto);
    Task DeleteAsync(Guid id);
    Task AddMemberAsync(Guid projectId, Guid userId, string? role);
    Task RemoveMemberAsync(Guid projectId, Guid userId);
    Task<List<ProjectMemberDto>> GetMembersAsync(Guid projectId);
    Task UpdateProgressAsync(Guid projectId);
}

public interface ITaskService
{
    Task<PaginatedResultDto<TaskDto>> GetAllAsync(int page = 1, int pageSize = 20, Guid? projectId = null, TaskStatus? status = null, Guid? assignedToId = null, Guid? filterUserId = null);
    Task<TaskDto> GetByIdAsync(Guid id);
    Task<TaskDto> CreateAsync(CreateTaskDto dto, Guid createdById);
    Task<TaskDto> UpdateAsync(Guid id, UpdateTaskDto dto);
    Task DeleteAsync(Guid id);
    Task UpdateStatusAsync(Guid id, TaskStatus status, Guid userId);
    Task AddDependencyAsync(Guid taskId, Guid predecessorId, DependencyType type);
    Task RemoveDependencyAsync(Guid taskId, Guid predecessorId);
    Task<List<TaskDto>> GetSubtasksAsync(Guid parentTaskId);
    Task<int> GetOverdueCountAsync(Guid userId);
}

public interface ITimeEntryService
{
    Task<PaginatedResultDto<TimeEntryDto>> GetAllAsync(int page = 1, int pageSize = 20, Guid? taskId = null, Guid? userId = null);
    Task<TimeEntryDto> GetByIdAsync(Guid id);
    Task<TimeEntryDto> CreateAsync(CreateTimeEntryDto dto, Guid userId);
    Task<TimeEntryDto> UpdateAsync(Guid id, UpdateTimeEntryDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<decimal> GetTotalHoursAsync(Guid userId, DateTime? start = null, DateTime? end = null);
}

public interface ICommentService
{
    Task<List<CommentDto>> GetByTaskAsync(Guid taskId);
    Task<CommentDto> CreateAsync(CreateCommentDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}

public interface INotificationService
{
    Task<List<NotificationDto>> GetByUserAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid id, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task CreateNotificationAsync(Guid userId, string title, string? message, NotificationType type, Guid? referenceId = null);
}

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllAsync();
    Task<TagDto> CreateAsync(CreateTagDto dto);
    Task<TagDto> UpdateAsync(Guid id, CreateTagDto dto);
    Task DeleteAsync(Guid id);
    Task AddToTaskAsync(Guid taskId, Guid tagId);
    Task RemoveFromTaskAsync(Guid taskId, Guid tagId);
}

public interface IWorkflowService
{
    Task<IEnumerable<WorkflowDto>> GetByProjectAsync(Guid projectId);
    Task<WorkflowDto> GetByIdAsync(Guid id);
    Task<WorkflowDto> CreateAsync(CreateWorkflowDto dto);
    Task<WorkflowDto> UpdateAsync(Guid id, CreateWorkflowDto dto);
    Task DeleteAsync(Guid id);
    Task AddTransitionAsync(Guid workflowId, CreateWorkflowTransitionDto dto);
    Task RemoveTransitionAsync(Guid id);
    Task<bool> CanTransitionAsync(Guid projectId, TaskStatus from, TaskStatus to, Guid userId);
}

public interface IDelayService
{
    Task<List<DelayDto>> GetByTaskAsync(Guid taskId);
    Task<DelayDto> CreateAsync(CreateDelayDto dto, Guid createdById);
    Task<List<DelayDto>> GetAllAsync(int page = 1, int pageSize = 20, DelayCategory? category = null);
}

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(Guid userId);
}

public interface ICalendarService
{
    Task<List<CalendarEventDto>> GetEventsAsync(Guid userId, DateTime start, DateTime end);
    Task MoveTaskAsync(Guid taskId, DateTime newStartDate, DateTime newEndDate, Guid userId);
}

public interface IFinancialService
{
    Task<IEnumerable<ExpenseCategoryDto>> GetCategoriesAsync(bool? isIncome = null);
    Task<ExpenseCategoryDto> CreateCategoryAsync(CreateExpenseCategoryDto dto);
    Task<ExpenseCategoryDto> UpdateCategoryAsync(Guid id, CreateExpenseCategoryDto dto);
    Task DeleteCategoryAsync(Guid id);
    Task<PaginatedResultDto<FinancialTransactionDto>> GetTransactionsAsync(int page = 1, int pageSize = 20, Guid? projectId = null, DateTime? startDate = null, DateTime? endDate = null, TransactionType? type = null, Guid? categoryId = null);
    Task<FinancialTransactionDto> CreateTransactionAsync(CreateFinancialTransactionDto dto, Guid userId);
    Task<FinancialTransactionDto> UpdateTransactionAsync(Guid id, CreateFinancialTransactionDto dto);
    Task DeleteTransactionAsync(Guid id);
    Task<FinancialReportDto> GetReportAsync(Guid? projectId = null, DateTime? startDate = null, DateTime? endDate = null);
}

public interface IResourceService
{
    Task<PaginatedResultDto<ResourceDto>> GetAllAsync(int page = 1, int pageSize = 20, ResourceType? type = null, ResourceStatus? status = null, string? search = null);
    Task<ResourceDto> GetByIdAsync(Guid id);
    Task<ResourceDto> CreateAsync(CreateResourceDto dto);
    Task<ResourceDto> UpdateAsync(Guid id, UpdateResourceDto dto);
    Task DeleteAsync(Guid id);
    Task<ResourceMovementDto> CreateMovementAsync(CreateResourceMovementDto dto, Guid userId);
    Task<List<ResourceMovementDto>> GetMovementsAsync(Guid resourceId);
    Task AssignToUserAsync(Guid resourceId, Guid userId);
    Task ReturnFromUserAsync(Guid resourceId);
}

public interface IChatService
{
    Task<List<ConversationDto>> GetConversationsAsync(Guid userId);
    Task<ConversationDto> GetConversationAsync(Guid conversationId, Guid userId);
    Task<ConversationDto> CreateConversationAsync(CreateConversationDto dto, Guid userId);
    Task<List<MessageDto>> GetMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50);
    Task<MessageDto> SendMessageAsync(CreateMessageDto dto, Guid senderId);
    Task MarkAsReadAsync(Guid conversationId, Guid userId);
    Task<List<ConversationDto>> GetDirectMessagesAsync(Guid userId, Guid otherUserId);
}

public interface ISettingsService
{
    Task<IEnumerable<SystemSettingDto>> GetAllAsync();
    Task<SystemSettingDto> GetByKeyAsync(string key);
    Task<SystemSettingDto> UpdateAsync(string key, UpdateSystemSettingDto dto);
    Task InitializeDefaultsAsync();
    Task<bool> GetBoolValueAsync(string key);
    Task<string> GetStringValueAsync(string key);
}

public interface IAnalyticsService
{
    Task<AnalyticsDto> GetAnalyticsAsync(Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<ComplianceMetricsDto> GetComplianceMetricsAsync(Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<UserPerformanceDto>> GetUserPerformanceAsync(Guid? projectId = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<ProjectMetricsDto>> GetProjectMetricsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<MonthlyDataDto>> GetMonthlyDataAsync(int months = 12);
    Task<object> GetProjectReportAsync(Guid projectId, DateTime? startDate, DateTime? endDate);
    Task<object> GetUserReportAsync(Guid userId, DateTime? startDate, DateTime? endDate);
    Task<object> GetFinancialReportAsync(Guid? projectId, DateTime? startDate, DateTime? endDate);
}