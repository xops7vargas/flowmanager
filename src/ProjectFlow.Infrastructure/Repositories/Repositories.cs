using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;
using ProjectFlow.Domain.Interfaces;
using ProjectFlow.Infrastructure.Data;
using TaskStatus = ProjectFlow.Domain.Enums.TaskStatus;
using TransactionType = ProjectFlow.Domain.Enums.TransactionType;
using ResourceType = ProjectFlow.Domain.Enums.ResourceType;
using ResourceStatus = ProjectFlow.Domain.Enums.ResourceStatus;

namespace ProjectFlow.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ProjectFlowDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ProjectFlowDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public virtual Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }
}

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ProjectFlowDbContext context) : base(context) { }

    public override async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .ToListAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbSet.Where(u => u.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(Guid roleId)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
            .ToListAsync();
    }

    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.ProjectMembers)
                .ThenInclude(pm => pm.Project)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}

public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<IEnumerable<Role>> GetSystemRolesAsync()
    {
        return await _dbSet.Where(r => r.IsSystem).ToListAsync();
    }

    public override async Task<Role?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Permission>> GetByModuleAsync(string module)
    {
        return await _dbSet.Where(p => p.Module == module).ToListAsync();
    }
}

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Project>> GetByOwnerAsync(Guid ownerId)
    {
        return await _dbSet.Where(p => p.OwnerId == ownerId).ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByMemberAsync(Guid userId)
    {
        return await _dbSet
            .Include(p => p.Members)
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetByStatusAsync(ProjectStatus status)
    {
        return await _dbSet.Where(p => p.Status == status).ToListAsync();
    }

    public override async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Owner)
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .Include(p => p.Workflows)
                .ThenInclude(w => w.Transitions)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<TaskItem>> GetByProjectAsync(Guid projectId)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Subtasks)
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByAssigneeAsync(Guid userId)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Where(t => t.AssignedToId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetByStatusAsync(TaskStatus status)
    {
        return await _dbSet.Where(t => t.Status == status).ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(t => t.Project)
            .Where(t => t.DueDate < now && t.Status != TaskStatus.Completed)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetSubtasksAsync(Guid parentTaskId)
    {
        return await _dbSet
            .Include(t => t.Subtasks)
            .Where(t => t.ParentTaskId == parentTaskId)
            .ToListAsync();
    }

    public override async Task<TaskItem?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.ParentTask)
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Subtasks)
            .Include(t => t.TaskTags)
                .ThenInclude(tt => tt.Tag)
            .Include(t => t.PredecessorDependencies)
                .ThenInclude(d => d.PredecessorTask)
            .Include(t => t.SuccessorDependencies)
                .ThenInclude(d => d.SuccessorTask)
            .Include(t => t.TimeEntries)
            .Include(t => t.Comments)
                .ThenInclude(c => c.User)
            .Include(t => t.Attachments)
            .Include(t => t.Delays)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}

public class TimeEntryRepository : Repository<TimeEntry>, ITimeEntryRepository
{
    public TimeEntryRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<TimeEntry>> GetByTaskAsync(Guid taskId)
    {
        return await _dbSet
            .Include(te => te.User)
            .Where(te => te.TaskId == taskId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeEntry>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(te => te.Task)
                .ThenInclude(t => t.Project)
            .Where(te => te.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeEntry>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _dbSet
            .Include(te => te.User)
            .Include(te => te.Task)
            .Where(te => te.Date >= start && te.Date <= end)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalHoursByUserAsync(Guid userId, DateTime? start = null, DateTime? end = null)
    {
        var query = _dbSet.Where(te => te.UserId == userId);
        
        if (start.HasValue)
            query = query.Where(te => te.Date >= start.Value);
        if (end.HasValue)
            query = query.Where(te => te.Date <= end.Value);

        return await query.SumAsync(te => te.Hours);
    }

    public override async Task<TimeEntry?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(te => te.Task)
            .Include(te => te.User)
            .FirstOrDefaultAsync(te => te.Id == id);
    }
}

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Notification>> GetByUserAsync(Guid userId, bool unreadOnly = false)
    {
        var query = _dbSet.Where(n => n.UserId == userId);
        
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead);
    }
}

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Comment>> GetByTaskAsync(Guid taskId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.TaskId == taskId)
            .ToListAsync();
    }
}

public class TagRepository : Repository<Tag>, ITagRepository
{
    public TagRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<Tag?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Name == name);
    }
}

public class WorkflowRepository : Repository<Workflow>, IWorkflowRepository
{
    public WorkflowRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Workflow>> GetByProjectAsync(Guid projectId)
    {
        return await _dbSet
            .Include(w => w.Transitions)
                .ThenInclude(t => t.RequiredRole)
            .Where(w => w.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<Workflow?> GetDefaultAsync(Guid projectId)
    {
        return await _dbSet
            .Include(w => w.Transitions)
                .ThenInclude(t => t.RequiredRole)
            .FirstOrDefaultAsync(w => w.ProjectId == projectId && w.IsDefault);
    }

    public override async Task<Workflow?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(w => w.Transitions)
                .ThenInclude(t => t.RequiredRole)
            .FirstOrDefaultAsync(w => w.Id == id);
    }
}

public class DelayRepository : Repository<Delay>, IDelayRepository
{
    public DelayRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Delay>> GetByTaskAsync(Guid taskId)
    {
        return await _dbSet
            .Include(d => d.CreatedBy)
            .Where(d => d.TaskId == taskId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Delay>> GetByCategoryAsync(DelayCategory category)
    {
        return await _dbSet
            .Include(d => d.Task)
            .Include(d => d.CreatedBy)
            .Where(d => d.Category == category)
            .ToListAsync();
    }

    public override async Task<Delay?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(d => d.Task)
            .Include(d => d.CreatedBy)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
}

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId)
    {
        return await _dbSet
            .Include(a => a.User)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }
}

public class ExpenseCategoryRepository : Repository<ExpenseCategory>, IExpenseCategoryRepository
{
    public ExpenseCategoryRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<ExpenseCategory>> GetByTypeAsync(bool isIncome)
    {
        return await _dbSet.Where(c => c.IsIncome == isIncome).ToListAsync();
    }
}

public class FinancialTransactionRepository : Repository<FinancialTransaction>, IFinancialTransactionRepository
{
    public FinancialTransactionRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<FinancialTransaction>> GetByProjectAsync(Guid projectId)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Category)
            .Include(t => t.User)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<IEnumerable<FinancialTransaction>> GetByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Category)
            .Include(t => t.User)
            .Where(t => t.Date >= start && t.Date <= end)
            .ToListAsync();
    }

    public async Task<IEnumerable<FinancialTransaction>> GetByTypeAsync(TransactionType type)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Category)
            .Include(t => t.User)
            .Where(t => t.Type == type)
            .ToListAsync();
    }

    public async Task<IEnumerable<FinancialTransaction>> GetByCategoryAsync(Guid categoryId)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Category)
            .Include(t => t.User)
            .Where(t => t.CategoryId == categoryId)
            .ToListAsync();
    }

    public override async Task<FinancialTransaction?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Category)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}

public class ResourceRepository : Repository<Resource>, IResourceRepository
{
    public ResourceRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Resource>> GetByTypeAsync(ResourceType type)
    {
        return await _dbSet
            .Include(r => r.AssignedTo)
            .Where(r => r.Type == type)
            .ToListAsync();
    }

    public async Task<IEnumerable<Resource>> GetByStatusAsync(ResourceStatus status)
    {
        return await _dbSet
            .Include(r => r.AssignedTo)
            .Where(r => r.Status == status)
            .ToListAsync();
    }

    public async Task<IEnumerable<Resource>> GetAvailableAsync()
    {
        return await _dbSet
            .Include(r => r.AssignedTo)
            .Where(r => r.Status == ResourceStatus.Available && r.AvailableQuantity > 0)
            .ToListAsync();
    }

    public override async Task<Resource?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(r => r.AssignedTo)
            .Include(r => r.Movements)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
}

public class ResourceMovementRepository : Repository<ResourceMovement>, IResourceMovementRepository
{
    public ResourceMovementRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<ResourceMovement>> GetByResourceAsync(Guid resourceId)
    {
        return await _dbSet
            .Include(m => m.User)
            .Include(m => m.Project)
            .Where(m => m.ResourceId == resourceId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ResourceMovement>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(m => m.Resource)
            .Include(m => m.Project)
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ResourceMovement>> GetByProjectAsync(Guid projectId)
    {
        return await _dbSet
            .Include(m => m.Resource)
            .Include(m => m.User)
            .Where(m => m.ProjectId == projectId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public override async Task<ResourceMovement?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(m => m.Resource)
            .Include(m => m.User)
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}

public class ConversationRepository : Repository<Conversation>, IConversationRepository
{
    public ConversationRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<Conversation?> GetDirectConversationAsync(Guid userId1, Guid userId2)
    {
        return await _dbSet
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Include(c => c.Messages)
            .Where(c => c.Type == ConversationType.Direct && 
                c.Participants.Any(p => p.UserId == userId1) && 
                c.Participants.Any(p => p.UserId == userId2))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Conversation>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();
    }

    public override async Task<Conversation?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(50))
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}

public class ConversationParticipantRepository : Repository<ConversationParticipant>, IConversationParticipantRepository
{
    public ConversationParticipantRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<ConversationParticipant?> GetAsync(Guid conversationId, Guid userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId == userId);
    }
}

public class MessageRepository : Repository<Message>, IMessageRepository
{
    public MessageRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<IEnumerable<Message>> GetByConversationAsync(Guid conversationId, int page = 1, int pageSize = 50)
    {
        return await _dbSet
            .Include(m => m.Sender)
            .Include(m => m.ReplyTo)
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Message?> GetLastMessageAsync(Guid conversationId)
    {
        return await _dbSet
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public override async Task<Message?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(m => m.Sender)
            .Include(m => m.ReplyTo)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}

public class SystemSettingRepository : Repository<SystemSetting>, ISystemSettingRepository
{
    public SystemSettingRepository(ProjectFlowDbContext context) : base(context) { }

    public async Task<SystemSetting?> GetByKeyAsync(string key)
    {
        return await _dbSet.FirstOrDefaultAsync(s => s.Key == key);
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ProjectFlowDbContext _context;

    public UnitOfWork(ProjectFlowDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Roles = new RoleRepository(context);
        Permissions = new PermissionRepository(context);
        UserRoles = new UserRoleRepository(context);
        RolePermissions = new RolePermissionRepository(context);
        Projects = new ProjectRepository(context);
        Tasks = new TaskRepository(context);
        TimeEntries = new TimeEntryRepository(context);
        Notifications = new NotificationRepository(context);
        Comments = new CommentRepository(context);
        Tags = new TagRepository(context);
        Workflows = new WorkflowRepository(context);
        Delays = new DelayRepository(context);
        AuditLogs = new AuditLogRepository(context);
        ExpenseCategories = new ExpenseCategoryRepository(context);
        FinancialTransactions = new FinancialTransactionRepository(context);
        Resources = new ResourceRepository(context);
        ResourceMovements = new ResourceMovementRepository(context);
        Conversations = new ConversationRepository(context);
        ConversationParticipants = new ConversationParticipantRepository(context);
        Messages = new MessageRepository(context);
        SystemSettings = new SystemSettingRepository(context);
    }

    public IUserRepository Users { get; }
    public IRoleRepository Roles { get; }
    public IPermissionRepository Permissions { get; }
    public IUserRoleRepository UserRoles { get; }
    public IRolePermissionRepository RolePermissions { get; }
    public IProjectRepository Projects { get; }
    public ITaskRepository Tasks { get; }
    public ITimeEntryRepository TimeEntries { get; }
    public INotificationRepository Notifications { get; }
    public ICommentRepository Comments { get; }
    public ITagRepository Tags { get; }
    public IWorkflowRepository Workflows { get; }
    public IDelayRepository Delays { get; }
    public IAuditLogRepository AuditLogs { get; }
    public IExpenseCategoryRepository ExpenseCategories { get; }
    public IFinancialTransactionRepository FinancialTransactions { get; }
    public IResourceRepository Resources { get; }
    public IResourceMovementRepository ResourceMovements { get; }
    public IConversationRepository Conversations { get; }
    public IConversationParticipantRepository ConversationParticipants { get; }
    public IMessageRepository Messages { get; }
    public ISystemSettingRepository SystemSettings { get; }

    public DbContext Context => _context;

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

public class UserRoleRepository : IUserRoleRepository
{
    private readonly ProjectFlowDbContext _context;
    private readonly DbSet<UserRole> _dbSet;

    public UserRoleRepository(ProjectFlowDbContext context)
    {
        _context = context;
        _dbSet = context.Set<UserRole>();
    }

    public async Task<UserRole?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<UserRole>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task<UserRole> AddAsync(UserRole entity) => (await _dbSet.AddAsync(entity)).Entity;
    public Task UpdateAsync(UserRole entity) { _dbSet.Update(entity); return Task.CompletedTask; }
    public Task DeleteAsync(Guid id)
    {
        var entity = _dbSet.FindAsync(id).Result;
        if (entity != null) _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(Guid userId) =>
        await _dbSet.Where(ur => ur.UserId == userId).ToListAsync();

    public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(Guid roleId) =>
        await _dbSet.Where(ur => ur.RoleId == roleId).ToListAsync();
}

public class RolePermissionRepository : IRolePermissionRepository
{
    private readonly ProjectFlowDbContext _context;
    private readonly DbSet<RolePermission> _dbSet;

    public RolePermissionRepository(ProjectFlowDbContext context)
    {
        _context = context;
        _dbSet = context.Set<RolePermission>();
    }

    public async Task<RolePermission?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);
    public async Task<IEnumerable<RolePermission>> GetAllAsync() => await _dbSet.ToListAsync();
    public async Task<RolePermission> AddAsync(RolePermission entity) => (await _dbSet.AddAsync(entity)).Entity;
    public Task UpdateAsync(RolePermission entity) { _dbSet.Update(entity); return Task.CompletedTask; }
    public Task DeleteAsync(Guid id) { return Task.CompletedTask; }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId) =>
        await _dbSet.Where(rp => rp.RoleId == roleId).ToListAsync();

    public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(Guid permissionId) =>
        await _dbSet.Where(rp => rp.PermissionId == permissionId).ToListAsync();
}