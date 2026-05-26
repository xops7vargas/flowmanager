using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;
using ProjectFlow.Domain.Interfaces;
using TaskStatus = ProjectFlow.Domain.Enums.TaskStatus;
using DependencyType = ProjectFlow.Domain.Enums.DependencyType;
using ProjectStatus = ProjectFlow.Domain.Enums.ProjectStatus;
using System.Linq;

namespace ProjectFlow.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public ProjectService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<PaginatedResultDto<ProjectDto>> GetAllAsync(int page = 1, int pageSize = 20, ProjectStatus? status = null, Guid? userId = null)
    {
        var query = await _unitOfWork.Projects.GetAllAsync();
        
        if (userId.HasValue)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            var userRoles = user?.UserRoles.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var isAdmin = userRoles.Contains("Administrator");
            
            if (!isAdmin)
            {
                var userProjectMembers = user?.ProjectMembers?.Select(pm => pm.ProjectId).ToList() ?? new List<Guid>();
                var ownedProjectIds = query.Where(p => p.OwnerId == userId.Value).Select(p => p.Id).ToList();
                var accessibleProjectIds = userProjectMembers.Concat(ownedProjectIds).Distinct().ToList();
                
                query = query.Where(p => accessibleProjectIds.Contains(p.Id));
            }
        }
        
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var totalCount = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResultDto<ProjectDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProjectDto> GetByIdAsync(Guid id)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null) throw new KeyNotFoundException("Project not found");
        return MapToDto(project);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto, Guid ownerId)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Status = ProjectStatus.Planning,
            StartDate = dto.StartDate.HasValue ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc) : null,
            EndDate = dto.EndDate.HasValue ? DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc) : null,
            Budget = dto.Budget,
            OwnerId = ownerId,
            Progress = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Projects.AddAsync(project);
        
        var member = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = ownerId,
            RoleInProject = "Owner",
            JoinedAt = DateTime.UtcNow
        };
        
        var defaultWorkflow = new Workflow
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Name = "Default Workflow",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow
        };
        
        await _unitOfWork.Workflows.AddAsync(defaultWorkflow);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(project);
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectDto dto)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null) throw new KeyNotFoundException("Project not found");

        if (!string.IsNullOrEmpty(dto.Name)) project.Name = dto.Name;
        if (dto.Description != null) project.Description = dto.Description;
        if (dto.Status.HasValue) project.Status = dto.Status.Value;
        if (dto.StartDate.HasValue) project.StartDate = DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc);
        if (dto.EndDate.HasValue) project.EndDate = DateTime.SpecifyKind(dto.EndDate.Value, DateTimeKind.Utc);
        if (dto.Budget.HasValue) project.Budget = dto.Budget;
        if (dto.Progress.HasValue) project.Progress = dto.Progress.Value;
        
        project.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Projects.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(project);
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id);
        if (project == null) throw new KeyNotFoundException("Project not found");

        await _unitOfWork.Projects.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddMemberAsync(Guid projectId, Guid userId, string? role)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null) throw new KeyNotFoundException("Project not found");

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            RoleInProject = role,
            JoinedAt = DateTime.UtcNow
        };

        project.Members.Add(member);
        await _unitOfWork.Projects.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(userId, "Nuevo proyecto", $"Has sido agregado al proyecto {project.Name}", NotificationType.ProjectUpdated, projectId);
    }

    public async Task RemoveMemberAsync(Guid projectId, Guid userId)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null) throw new KeyNotFoundException("Project not found");

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            project.Members.Remove(member);
            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<List<ProjectMemberDto>> GetMembersAsync(Guid projectId)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null) throw new KeyNotFoundException("Project not found");

        return project.Members.Select(m => new ProjectMemberDto
        {
            UserId = m.UserId,
            UserName = $"{m.User.FirstName} {m.User.LastName}",
            Avatar = m.User.Avatar,
            RoleInProject = m.RoleInProject,
            JoinedAt = m.JoinedAt
        }).ToList();
    }

    public async Task UpdateProgressAsync(Guid projectId)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null) return;

        var tasks = await Task.Run(() => project.Tasks.ToList());
        if (tasks.Any())
        {
            var completed = tasks.Count(t => t.Status == TaskStatus.Completed);
            project.Progress = (int)((completed * 100.0) / tasks.Count);
        }

        await _unitOfWork.Projects.UpdateAsync(project);
        await _unitOfWork.SaveChangesAsync();
    }

    private static ProjectDto MapToDto(Project p)
    {
        return new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Status = p.Status,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            Budget = p.Budget,
            Progress = p.Progress,
            OwnerId = p.OwnerId,
            OwnerName = p.Owner != null ? $"{p.Owner.FirstName} {p.Owner.LastName}" : "Unknown",
            TaskCount = p.Tasks?.Count ?? 0,
            CompletedTaskCount = p.Tasks?.Count(t => t.Status == TaskStatus.Completed) ?? 0,
            CreatedAt = p.CreatedAt
        };
    }
}

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IWorkflowService _workflowService;

    public TaskService(IUnitOfWork unitOfWork, INotificationService notificationService, IWorkflowService workflowService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _workflowService = workflowService;
    }

    public async Task<PaginatedResultDto<TaskDto>> GetAllAsync(int page = 1, int pageSize = 20, Guid? projectId = null, TaskStatus? status = null, Guid? assignedToId = null, Guid? filterUserId = null)
    {
        var query = await _unitOfWork.Tasks.GetAllAsync();

        if (filterUserId.HasValue)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(filterUserId.Value);
            var userRoles = user?.UserRoles.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            var isAdmin = userRoles.Contains("Administrator");
            
            if (!isAdmin)
            {
                var userProjectMembers = user?.ProjectMembers?.Select(pm => pm.ProjectId).ToList() ?? new List<Guid>();
                var allProjects = await _unitOfWork.Projects.GetAllAsync();
                var ownedProjectIds = allProjects.Where(p => p.OwnerId == filterUserId).Select(p => p.Id).ToList();
                var accessibleProjectIds = userProjectMembers.Concat(ownedProjectIds).Distinct().ToList();
                
                query = query.Where(t => t.AssignedToId == filterUserId || accessibleProjectIds.Contains(t.ProjectId));
            }
        }

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);
        if (assignedToId.HasValue)
            query = query.Where(t => t.AssignedToId == assignedToId.Value);

        var totalCount = query.Count();
        var items = query.OrderBy(t => t.Order).Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResultDto<TaskDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TaskDto> GetByIdAsync(Guid id)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (task == null) throw new KeyNotFoundException("Task not found");
        return MapToDto(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto, Guid createdById)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(dto.ProjectId);
        if (project == null) throw new KeyNotFoundException("Project not found");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            ParentTaskId = dto.ParentTaskId,
            Title = dto.Title,
            Description = dto.Description,
            Status = TaskStatus.Todo,
            Priority = dto.Priority,
            StartDate = dto.StartDate.HasValue ? DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc) : null,
            DueDate = dto.DueDate.HasValue ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc) : null,
            EstimatedHours = dto.EstimatedHours,
            ActualHours = 0,
            CreatedById = createdById,
            AssignedToId = dto.AssignedToId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var maxOrder = project.Tasks.Any() ? project.Tasks.Max(t => t.Order) : 0;
        task.Order = maxOrder + 1;

        await _unitOfWork.Tasks.AddAsync(task);
        
        if (dto.AssignedToId.HasValue)
        {
            await _notificationService.CreateNotificationAsync(dto.AssignedToId.Value, "Nueva tarea asignada", $"Se te ha asignado la tarea: {task.Title}", NotificationType.TaskAssigned, task.Id);
        }

        await _unitOfWork.SaveChangesAsync();
        return MapToDto(task);
    }

    public async Task<TaskDto> UpdateAsync(Guid id, UpdateTaskDto dto)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (task == null) throw new KeyNotFoundException("Task not found");

        if (!string.IsNullOrEmpty(dto.Title)) task.Title = dto.Title;
        if (dto.Description != null) task.Description = dto.Description;
        if (dto.Status.HasValue) task.Status = dto.Status.Value;
        if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
        if (dto.StartDate.HasValue) task.StartDate = DateTime.SpecifyKind(dto.StartDate.Value, DateTimeKind.Utc);
        if (dto.DueDate.HasValue) task.DueDate = DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc);
        if (dto.EstimatedHours.HasValue) task.EstimatedHours = dto.EstimatedHours.Value;
        if (dto.AssignedToId.HasValue) task.AssignedToId = dto.AssignedToId;
        if (dto.Order.HasValue) task.Order = dto.Order.Value;
        
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (task == null) throw new KeyNotFoundException("Task not found");

        await _unitOfWork.Tasks.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(Guid id, TaskStatus status, Guid userId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (task == null) throw new KeyNotFoundException("Task not found");

        var canTransition = await _workflowService.CanTransitionAsync(task.ProjectId, task.Status, status, userId);
        if (!canTransition) throw new InvalidOperationException("Invalid status transition");

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task);
        
        if (task.AssignedToId.HasValue && task.AssignedToId != userId)
        {
            await _notificationService.CreateNotificationAsync(task.AssignedToId.Value, "Tarea actualizada", $"La tarea '{task.Title}' cambió a {status}", NotificationType.TaskUpdated, task.Id);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddDependencyAsync(Guid taskId, Guid predecessorId, DependencyType type)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
        var predecessor = await _unitOfWork.Tasks.GetByIdAsync(predecessorId);
        
        if (task == null || predecessor == null) throw new KeyNotFoundException("Task not found");

        var dependency = new TaskDependency
        {
            PredecessorTaskId = predecessorId,
            SuccessorTaskId = taskId,
            Type = type
        };

        task.PredecessorDependencies.Add(dependency);
        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveDependencyAsync(Guid taskId, Guid predecessorId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        var dep = task.PredecessorDependencies.FirstOrDefault(d => d.PredecessorTaskId == predecessorId);
        if (dep != null)
        {
            task.PredecessorDependencies.Remove(dep);
            await _unitOfWork.Tasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<List<TaskDto>> GetSubtasksAsync(Guid parentTaskId)
    {
        var parent = await _unitOfWork.Tasks.GetByIdAsync(parentTaskId);
        if (parent == null) throw new KeyNotFoundException("Task not found");

        return parent.Subtasks.Select(MapToDto).ToList();
    }

    public async Task<int> GetOverdueCountAsync(Guid userId)
    {
        var tasks = await _unitOfWork.Tasks.GetByAssigneeAsync(userId);
        return tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Completed);
    }

    private static TaskDto MapToDto(TaskItem t)
    {
        return new TaskDto
        {
            Id = t.Id,
            ProjectId = t.ProjectId,
            ProjectName = t.Project?.Name ?? "Unknown",
            ParentTaskId = t.ParentTaskId,
            ParentTaskTitle = t.ParentTask?.Title,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status,
            Priority = t.Priority,
            StartDate = t.StartDate,
            DueDate = t.DueDate,
            EstimatedHours = t.EstimatedHours,
            ActualHours = t.ActualHours,
            Order = t.Order,
            CreatedById = t.CreatedById,
            CreatedByName = t.CreatedBy != null ? $"{t.CreatedBy.FirstName} {t.CreatedBy.LastName}" : "Unknown",
            AssignedToId = t.AssignedToId,
            AssignedToName = t.AssignedTo != null ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}" : null,
            Tags = t.TaskTags?.Select(tt => new TagDto { Id = tt.TagId, Name = tt.Tag?.Name ?? "", Color = tt.Tag?.Color ?? "#000" }).ToList() ?? new List<TagDto>(),
            SubtaskCount = t.Subtasks?.Count ?? 0,
            CompletedSubtaskCount = t.Subtasks?.Count(st => st.Status == TaskStatus.Completed) ?? 0,
            IsOverdue = t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Completed,
            CreatedAt = t.CreatedAt
        };
    }
}