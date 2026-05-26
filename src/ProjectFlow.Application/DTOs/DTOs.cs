using System;
using System.Collections.Generic;
using ProjectFlow.Domain.Enums;
using TaskStatus = ProjectFlow.Domain.Enums.TaskStatus;
using TransactionType = ProjectFlow.Domain.Enums.TransactionType;
using ResourceType = ProjectFlow.Domain.Enums.ResourceType;
using ResourceStatus = ProjectFlow.Domain.Enums.ResourceStatus;
using MovementType = ProjectFlow.Domain.Enums.MovementType;
using ConversationType = ProjectFlow.Domain.Enums.ConversationType;
using MessageType = ProjectFlow.Domain.Enums.MessageType;
using SettingType = ProjectFlow.Domain.Enums.SettingType;
using TaskPriority = ProjectFlow.Domain.Enums.TaskPriority;

namespace ProjectFlow.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class CreateUserWithRoleDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
}

public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public bool IsActive { get; set; }
    public Guid? RoleId { get; set; }
}

public class UpdateUserProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? Avatar { get; set; }
}

public class UpdateUserRoleDto
{
    public Guid RoleId { get; set; }
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public List<PermissionDto> Permissions { get; set; } = new();
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public int Progress { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public int TaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
}

public class UpdateProjectDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public int? Progress { get; set; }
}

public class ProjectMemberDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? RoleInProject { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class TaskDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid? ParentTaskId { get; set; }
    public string? ParentTaskTitle { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public int Order { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public List<TagDto> Tags { get; set; } = new();
    public int SubtaskCount { get; set; }
    public int CompletedSubtaskCount { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTaskDto
{
    public Guid ProjectId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal EstimatedHours { get; set; }
    public Guid? AssignedToId { get; set; }
}

public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public Guid? AssignedToId { get; set; }
    public int? Order { get; set; }
}

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class CreateTagDto
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#2196f3";
}

public class TimeEntryDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTimeEntryDto
{
    public Guid TaskId { get; set; }
    public decimal Hours { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}

public class UpdateTimeEntryDto
{
    public decimal Hours { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentDto
{
    public Guid TaskId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public NotificationType Type { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DelayDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DelayCategory Category { get; set; }
    public int DaysDelayed { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateDelayDto
{
    public Guid TaskId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DelayCategory Category { get; set; }
    public int DaysDelayed { get; set; }
}

public class WorkflowDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public List<WorkflowTransitionDto> Transitions { get; set; } = new();
}

public class WorkflowTransitionDto
{
    public Guid Id { get; set; }
    public TaskStatus FromStatus { get; set; }
    public TaskStatus ToStatus { get; set; }
    public Guid? RequiredRoleId { get; set; }
    public string? RequiredRoleName { get; set; }
}

public class CreateWorkflowDto
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateWorkflowTransitionDto
{
    public TaskStatus FromStatus { get; set; }
    public TaskStatus ToStatus { get; set; }
    public Guid? RequiredRoleId { get; set; }
}

public class DashboardDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public List<ProjectProgressDto> ProjectProgress { get; set; } = new();
    public List<TaskByPriorityDto> TasksByPriority { get; set; } = new();
}

public class ProjectProgressDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int Progress { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
}

public class TaskByPriorityDto
{
    public TaskPriority Priority { get; set; }
    public int Count { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class CalendarEventDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
}

public class PaginatedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class ExpenseCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool IsIncome { get; set; }
    public Guid? ParentId { get; set; }
}

public class CreateExpenseCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#2196f3";
    public bool IsIncome { get; set; }
    public Guid? ParentId { get; set; }
}

public class FinancialTransactionDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public string? Reference { get; set; }
}

public class CreateFinancialTransactionDto
{
    public Guid ProjectId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public string? Reference { get; set; }
}

public class FinancialReportDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance { get; set; }
    public List<FinancialTransactionDto> Transactions { get; set; } = new();
    public Dictionary<string, decimal> ByCategory { get; set; } = new();
    public Dictionary<string, decimal> ByMonth { get; set; } = new();
}

public class ResourceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public ResourceStatus Status { get; set; }
    public int Quantity { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal UnitValue { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public string? Location { get; set; }
    public DateTime? PurchaseDate { get; set; }
}

public class CreateResourceDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public ResourceType Type { get; set; }
    public int Quantity { get; set; }
    public decimal UnitValue { get; set; }
    public string? Location { get; set; }
    public DateTime? PurchaseDate { get; set; }
}

public class UpdateResourceDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ResourceStatus? Status { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? Location { get; set; }
}

public class ResourceMovementDto
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateResourceMovementDto
{
    public Guid ResourceId { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Notes { get; set; }
}

public class ConversationDto
{
    public Guid Id { get; set; }
    public ConversationType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ConversationParticipantDto> Participants { get; set; } = new();
    public MessageDto? LastMessage { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public class ConversationParticipantDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public bool IsOnline { get; set; }
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public Guid? ReplyToId { get; set; }
    public string? ReplyToContent { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateMessageDto
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public Guid? ReplyToId { get; set; }
}

public class CreateConversationDto
{
    public ConversationType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Guid> ParticipantIds { get; set; } = new();
}

public class SystemSettingDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SettingType Type { get; set; }
}

public class UpdateSystemSettingDto
{
    public string Value { get; set; } = string.Empty;
}

public class AnalyticsDto
{
    public ComplianceMetricsDto Compliance { get; set; } = new();
    public List<UserPerformanceDto> UserPerformance { get; set; } = new();
    public List<ProjectMetricsDto> ProjectMetrics { get; set; } = new();
    public List<MonthlyDataDto> MonthlyData { get; set; } = new();
    public List<PriorityDistributionDto> PriorityDistribution { get; set; } = new();
}

public class ComplianceMetricsDto
{
    public double CompletionRate { get; set; }
    public double ComplianceRate { get; set; }
    public double OverdueRate { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
}

public class UserPerformanceDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public int TasksCompleted { get; set; }
    public int TasksInProgress { get; set; }
    public int OverdueTasks { get; set; }
    public decimal HoursWorked { get; set; }
    public double CompletionRate { get; set; }
}

public class ProjectMetricsDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double Progress { get; set; }
    public decimal Budget { get; set; }
    public decimal Spent { get; set; }
}

public class MonthlyDataDto
{
    public string Month { get; set; } = string.Empty;
    public int TasksCreated { get; set; }
    public int TasksCompleted { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
}

public class PriorityDistributionDto
{
    public TaskPriority Priority { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}