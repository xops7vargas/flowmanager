using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Domain.Entities;
using ProjectStatus = ProjectFlow.Domain.Enums.ProjectStatus;
using TaskStatus = ProjectFlow.Domain.Enums.TaskStatus;
using DelayCategory = ProjectFlow.Domain.Enums.DelayCategory;
using TransactionType = ProjectFlow.Domain.Enums.TransactionType;
using ResourceType = ProjectFlow.Domain.Enums.ResourceType;
using ResourceStatus = ProjectFlow.Domain.Enums.ResourceStatus;
using MovementType = ProjectFlow.Domain.Enums.MovementType;

namespace ProjectFlow.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetUsersByRoleAsync(Guid roleId);
}

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name);
    Task<IEnumerable<Role>> GetSystemRolesAsync();
}

public interface IPermissionRepository : IRepository<Permission>
{
    Task<IEnumerable<Permission>> GetByModuleAsync(string module);
}

public interface IUserRoleRepository : IRepository<UserRole>
{
    Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId);
}

public interface IRolePermissionRepository : IRepository<RolePermission>
{
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId);
    Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(Guid permissionId);
}

public interface IProjectRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetByOwnerAsync(Guid ownerId);
    Task<IEnumerable<Project>> GetByMemberAsync(Guid userId);
    Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status);
}

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<IEnumerable<TaskItem>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<TaskItem>> GetByAssigneeAsync(Guid userId);
    Task<IEnumerable<TaskItem>> GetByStatusAsync(TaskStatus status);
    Task<IEnumerable<TaskItem>> GetOverdueTasksAsync();
    Task<IEnumerable<TaskItem>> GetSubtasksAsync(Guid parentTaskId);
}

public interface ITimeEntryRepository : IRepository<TimeEntry>
{
    Task<IEnumerable<TimeEntry>> GetByTaskAsync(Guid taskId);
    Task<IEnumerable<TimeEntry>> GetByUserAsync(Guid userId);
    Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<decimal> GetTotalHoursByUserAsync(Guid userId, DateTime? start = null, DateTime? end = null);
}

public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserAsync(Guid userId, bool unreadOnly = false);
    Task<int> GetUnreadCountAsync(Guid userId);
}

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByTaskAsync(Guid taskId);
}

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name);
}

public interface IWorkflowRepository : IRepository<Workflow>
{
    Task<IEnumerable<Workflow>> GetByProjectAsync(Guid projectId);
    Task<Workflow?> GetDefaultAsync(Guid projectId);
}

public interface IDelayRepository : IRepository<Delay>
{
    Task<IEnumerable<Delay>> GetByTaskAsync(Guid taskId);
    Task<IEnumerable<Delay>> GetByCategoryAsync(DelayCategory category);
}

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId);
    Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId);
}

public interface IExpenseCategoryRepository : IRepository<ExpenseCategory>
{
    Task<IEnumerable<ExpenseCategory>> GetByTypeAsync(bool isIncome);
}

public interface IFinancialTransactionRepository : IRepository<FinancialTransaction>
{
    Task<IEnumerable<FinancialTransaction>> GetByProjectAsync(Guid projectId);
    Task<IEnumerable<FinancialTransaction>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<FinancialTransaction>> GetByTypeAsync(TransactionType type);
    Task<IEnumerable<FinancialTransaction>> GetByCategoryAsync(Guid categoryId);
}

public interface IResourceRepository : IRepository<Resource>
{
    Task<IEnumerable<Resource>> GetByTypeAsync(ResourceType type);
    Task<IEnumerable<Resource>> GetByStatusAsync(ResourceStatus status);
    Task<IEnumerable<Resource>> GetAvailableAsync();
}

public interface IResourceMovementRepository : IRepository<ResourceMovement>
{
    Task<IEnumerable<ResourceMovement>> GetByResourceAsync(Guid resourceId);
    Task<IEnumerable<ResourceMovement>> GetByUserAsync(Guid userId);
    Task<IEnumerable<ResourceMovement>> GetByProjectAsync(Guid projectId);
}

public interface IConversationRepository : IRepository<Conversation>
{
    Task<Conversation?> GetDirectConversationAsync(Guid userId1, Guid userId2);
    Task<IEnumerable<Conversation>> GetByUserAsync(Guid userId);
}

public interface IConversationParticipantRepository : IRepository<ConversationParticipant>
{
    Task<ConversationParticipant?> GetAsync(Guid conversationId, Guid userId);
}

public interface IMessageRepository : IRepository<Message>
{
    Task<IEnumerable<Message>> GetByConversationAsync(Guid conversationId, int page = 1, int pageSize = 50);
    Task<Message?> GetLastMessageAsync(Guid conversationId);
}

public interface ISystemSettingRepository : IRepository<SystemSetting>
{
    Task<SystemSetting?> GetByKeyAsync(string key);
}

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IPermissionRepository Permissions { get; }
    IUserRoleRepository UserRoles { get; }
    IRolePermissionRepository RolePermissions { get; }
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    ITimeEntryRepository TimeEntries { get; }
    INotificationRepository Notifications { get; }
    ICommentRepository Comments { get; }
    ITagRepository Tags { get; }
    IWorkflowRepository Workflows { get; }
    IDelayRepository Delays { get; }
    IAuditLogRepository AuditLogs { get; }
    IExpenseCategoryRepository ExpenseCategories { get; }
    IFinancialTransactionRepository FinancialTransactions { get; }
    IResourceRepository Resources { get; }
    IResourceMovementRepository ResourceMovements { get; }
    IConversationRepository Conversations { get; }
    IConversationParticipantRepository ConversationParticipants { get; }
    IMessageRepository Messages { get; }
    ISystemSettingRepository SystemSettings { get; }

    DbContext Context { get; }

    Task<int> SaveChangesAsync();
}