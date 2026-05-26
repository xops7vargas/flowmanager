using System;
using System.Collections.Generic;
using ProjectFlow.Domain.Enums;
using TaskStatus = ProjectFlow.Domain.Enums.TaskStatus;
using TaskPriority = ProjectFlow.Domain.Enums.TaskPriority;

namespace ProjectFlow.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public int Progress { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public virtual ICollection<Workflow> Workflows { get; set; } = new List<Workflow>();
}

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string? RoleInProject { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public virtual Project Project { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public class TaskItem
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public int Order { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? AssignedToId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Project Project { get; set; } = null!;
    public virtual TaskItem? ParentTask { get; set; }
    public virtual ICollection<TaskItem> Subtasks { get; set; } = new List<TaskItem>();
    public virtual User CreatedBy { get; set; } = null!;
    public virtual User? AssignedTo { get; set; }
    public virtual ICollection<TaskDependency> PredecessorDependencies { get; set; } = new List<TaskDependency>();
    public virtual ICollection<TaskDependency> SuccessorDependencies { get; set; } = new List<TaskDependency>();
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public virtual ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    public virtual ICollection<Delay> Delays { get; set; } = new List<Delay>();
}

public class TaskDependency
{
    public Guid PredecessorTaskId { get; set; }
    public Guid SuccessorTaskId { get; set; }
    public DependencyType Type { get; set; } = DependencyType.FinishToStart;

    public virtual TaskItem PredecessorTask { get; set; } = null!;
    public virtual TaskItem SuccessorTask { get; set; } = null!;
}

public class TimeEntry
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public decimal Hours { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual TaskItem Task { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public class Comment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual TaskItem Task { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Comment? Parent { get; set; }
    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
}

public class Attachment
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual TaskItem Task { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#2196f3";

    public virtual ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}

public class TaskTag
{
    public Guid TaskId { get; set; }
    public Guid TagId { get; set; }

    public virtual TaskItem Task { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public NotificationType Type { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual User User { get; set; } = null!;
}

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public virtual User? User { get; set; }
}

public class Workflow
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Project Project { get; set; } = null!;
    public virtual ICollection<WorkflowTransition> Transitions { get; set; } = new List<WorkflowTransition>();
}

public class WorkflowTransition
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public TaskStatus FromStatus { get; set; }
    public TaskStatus ToStatus { get; set; }
    public Guid? RequiredRoleId { get; set; }

    public virtual Workflow Workflow { get; set; } = null!;
    public virtual Role? RequiredRole { get; set; } = null!;
}

public class Delay
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DelayCategory Category { get; set; }
    public int DaysDelayed { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual TaskItem Task { get; set; } = null!;
    public virtual User CreatedBy { get; set; } = null!;
}

public class ExpenseCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#2196f3";
    public bool IsIncome { get; set; }
    public Guid? ParentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ExpenseCategory? Parent { get; set; }
    public virtual ICollection<ExpenseCategory> Children { get; set; } = new List<ExpenseCategory>();
    public virtual ICollection<FinancialTransaction> Transactions { get; set; } = new List<FinancialTransaction>();
}

public class FinancialTransaction
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? UserId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public string? Reference { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Project Project { get; set; } = null!;
    public virtual ExpenseCategory Category { get; set; } = null!;
    public virtual User? User { get; set; } = null!;
}

public class Resource
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public ResourceStatus Status { get; set; } = ResourceStatus.Available;
    public int Quantity { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal UnitValue { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? Location { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual User? AssignedTo { get; set; }
    public virtual ICollection<ResourceMovement> Movements { get; set; } = new List<ResourceMovement>();
}

public class ResourceMovement
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ProjectId { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Resource Resource { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Project? Project { get; set; }
}

public class Conversation
{
    public Guid Id { get; set; }
    public ConversationType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}

public class ConversationParticipant
{
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastReadAt { get; set; }

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public Guid? ReplyToId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Conversation Conversation { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
    public virtual Message? ReplyTo { get; set; }
}

public class SystemSetting
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SettingType Type { get; set; } = SettingType.String;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}