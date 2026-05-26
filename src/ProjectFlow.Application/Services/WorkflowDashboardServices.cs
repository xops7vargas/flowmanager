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

namespace ProjectFlow.Application.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkflowService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<WorkflowDto>> GetByProjectAsync(Guid projectId)
    {
        var workflows = await _unitOfWork.Workflows.GetByProjectAsync(projectId);
        return workflows.Select(MapToDto);
    }

    public async Task<WorkflowDto> GetByIdAsync(Guid id)
    {
        var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
        if (workflow == null) throw new KeyNotFoundException("Workflow not found");
        return MapToDto(workflow);
    }

    public async Task<WorkflowDto> CreateAsync(CreateWorkflowDto dto)
    {
        if (dto.IsDefault)
        {
            var existing = await _unitOfWork.Workflows.GetDefaultAsync(dto.ProjectId);
            if (existing != null)
            {
                existing.IsDefault = false;
                await _unitOfWork.Workflows.UpdateAsync(existing);
            }
        }

        var workflow = new Workflow
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            Name = dto.Name,
            Description = dto.Description,
            IsDefault = dto.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Workflows.AddAsync(workflow);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(workflow);
    }

    public async Task<WorkflowDto> UpdateAsync(Guid id, CreateWorkflowDto dto)
    {
        var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
        if (workflow == null) throw new KeyNotFoundException("Workflow not found");

        if (dto.IsDefault && !workflow.IsDefault)
        {
            var existing = await _unitOfWork.Workflows.GetDefaultAsync(workflow.ProjectId);
            if (existing != null && existing.Id != id)
            {
                existing.IsDefault = false;
                await _unitOfWork.Workflows.UpdateAsync(existing);
            }
        }

        workflow.Name = dto.Name;
        workflow.Description = dto.Description;
        workflow.IsDefault = dto.IsDefault;

        await _unitOfWork.Workflows.UpdateAsync(workflow);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(workflow);
    }

    public async Task DeleteAsync(Guid id)
    {
        var workflow = await _unitOfWork.Workflows.GetByIdAsync(id);
        if (workflow == null) throw new KeyNotFoundException("Workflow not found");
        if (workflow.IsDefault) throw new InvalidOperationException("Cannot delete default workflow");

        await _unitOfWork.Workflows.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddTransitionAsync(Guid workflowId, CreateWorkflowTransitionDto dto)
    {
        var workflow = await _unitOfWork.Workflows.GetByIdAsync(workflowId);
        if (workflow == null) throw new KeyNotFoundException("Workflow not found");

        var transition = new WorkflowTransition
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
            FromStatus = dto.FromStatus,
            ToStatus = dto.ToStatus,
            RequiredRoleId = dto.RequiredRoleId
        };

        workflow.Transitions.Add(transition);
        await _unitOfWork.Workflows.UpdateAsync(workflow);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveTransitionAsync(Guid id)
    {
        var workflows = await _unitOfWork.Workflows.GetAllAsync();
        foreach (var w in workflows)
        {
            var transition = w.Transitions.FirstOrDefault(t => t.Id == id);
            if (transition != null)
            {
                w.Transitions.Remove(transition);
                await _unitOfWork.Workflows.UpdateAsync(w);
                await _unitOfWork.SaveChangesAsync();
                return;
            }
        }
        throw new KeyNotFoundException("Transition not found");
    }

    public async Task<bool> CanTransitionAsync(Guid projectId, TaskStatus from, TaskStatus to, Guid userId)
    {
        var workflow = await _unitOfWork.Workflows.GetDefaultAsync(projectId);
        if (workflow == null) return true;

        var transition = workflow.Transitions.FirstOrDefault(t => t.FromStatus == from && t.ToStatus == to);
        if (transition == null) return false;

        if (transition.RequiredRoleId == null) return true;

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        return user?.UserRoles.Any(ur => ur.RoleId == transition.RequiredRoleId) ?? false;
    }

    private static WorkflowDto MapToDto(Workflow w)
    {
        return new WorkflowDto
        {
            Id = w.Id,
            ProjectId = w.ProjectId,
            Name = w.Name,
            Description = w.Description,
            IsDefault = w.IsDefault,
            Transitions = w.Transitions.Select(t => new WorkflowTransitionDto
            {
                Id = t.Id,
                FromStatus = t.FromStatus,
                ToStatus = t.ToStatus,
                RequiredRoleId = t.RequiredRoleId,
                RequiredRoleName = t.RequiredRole?.Name
            }).ToList()
        };
    }
}

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            return new DashboardDto();
        }

        var userRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var isAdmin = userRoles.Contains("Administrator");

        var allProjects = await _unitOfWork.Projects.GetAllAsync();
        var allTasks = await _unitOfWork.Tasks.GetAllAsync();
        var allTimeEntries = await _unitOfWork.TimeEntries.GetAllAsync();
        
        List<Project> projects;
        List<TaskItem> tasks;
        
        if (isAdmin)
        {
            projects = allProjects.ToList();
            tasks = allTasks.ToList();
        }
        else
        {
            var userProjectMembers = await _unitOfWork.Users.GetByIdAsync(userId);
            var userProjectIds = userProjectMembers?.ProjectMembers?.Select(pm => pm.ProjectId).ToList() ?? new List<Guid>();
            
            var ownedProjectIds = allProjects.Where(p => p.OwnerId == userId).Select(p => p.Id).ToList();
            
            var allAccessibleProjectIds = userProjectIds.Concat(ownedProjectIds).Distinct().ToList();
            
            projects = allProjects.Where(p => allAccessibleProjectIds.Contains(p.Id)).ToList();
            
            var projectIds = projects.Select(p => p.Id).ToList();
            tasks = allTasks.Where(t => t.AssignedToId == userId || projectIds.Contains(t.ProjectId)).ToList();
        }

        var userTasks = tasks.Where(t => t.AssignedToId == userId).ToList();
        var userTimeEntries = allTimeEntries.Where(te => te.UserId == userId).ToList();

        return new DashboardDto
        {
            TotalProjects = projects.Count,
            ActiveProjects = projects.Count(p => p.Status == ProjectStatus.InProgress),
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
            OverdueTasks = tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != TaskStatus.Completed),
            TotalHoursWorked = userTimeEntries.Sum(te => te.Hours),
            PendingTasks = userTasks.Count(t => t.Status == TaskStatus.Todo),
            InProgressTasks = userTasks.Count(t => t.Status == TaskStatus.InProgress),
            ProjectProgress = projects.Select(p => new ProjectProgressDto
            {
                ProjectId = p.Id,
                ProjectName = p.Name,
                Progress = p.Progress,
                TotalTasks = p.Tasks.Count,
                CompletedTasks = p.Tasks.Count(t => t.Status == TaskStatus.Completed)
            }).ToList(),
            TasksByPriority = tasks.GroupBy(t => t.Priority).Select(g => new TaskByPriorityDto
            {
                Priority = g.Key,
                Count = g.Count()
            }).ToList()
        };
    }
}

public class CalendarService : ICalendarService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkflowService _workflowService;

    public CalendarService(IUnitOfWork unitOfWork, IWorkflowService workflowService)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
    }

    public async Task<List<CalendarEventDto>> GetEventsAsync(Guid userId, DateTime start, DateTime end)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return new List<CalendarEventDto>();

        var userRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var isAdmin = userRoles.Contains("Administrator");

        var allTasks = await _unitOfWork.Tasks.GetAllAsync();
        
        List<TaskItem> tasks;
        
        if (isAdmin)
        {
            tasks = allTasks
                .Where(t => ((t.StartDate.HasValue && t.StartDate >= start && t.StartDate <= end) || 
                            (t.DueDate.HasValue && t.DueDate >= start && t.DueDate <= end)))
                .ToList();
        }
        else
        {
            var userProjectMembers = await _unitOfWork.Users.GetByIdAsync(userId);
            var userProjectIds = userProjectMembers?.ProjectMembers?.Select(pm => pm.ProjectId).ToList() ?? new List<Guid>();
            
            var ownedProjects = await _unitOfWork.Projects.GetAllAsync();
            var ownedProjectIds = ownedProjects.Where(p => p.OwnerId == userId).Select(p => p.Id).ToList();
            
            var allAccessibleProjectIds = userProjectIds.Concat(ownedProjectIds).Distinct().ToList();
            
            tasks = allTasks
                .Where(t => (t.AssignedToId == userId || allAccessibleProjectIds.Contains(t.ProjectId)) &&
                           ((t.StartDate.HasValue && t.StartDate >= start && t.StartDate <= end) || 
                            (t.DueDate.HasValue && t.DueDate >= start && t.DueDate <= end)))
                .ToList();
        }

        return tasks.Select(t => new CalendarEventDto
        {
            Id = t.Id,
            Title = t.Title,
            Start = t.StartDate ?? t.DueDate ?? DateTime.UtcNow,
            End = t.DueDate ?? t.StartDate ?? DateTime.UtcNow,
            Color = GetColorByStatus(t.Status),
            Description = t.Description,
            Status = t.Status,
            ProjectId = t.ProjectId,
            ProjectName = t.Project?.Name ?? "Sin Proyecto"
        }).ToList();
    }

    public async Task MoveTaskAsync(Guid taskId, DateTime newStartDate, DateTime newEndDate, Guid userId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        task.StartDate = newStartDate;
        task.DueDate = newEndDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();
    }

    private static string GetColorByStatus(TaskStatus status)
    {
        return status switch
        {
            TaskStatus.Todo => "#9e9e9e",
            TaskStatus.InProgress => "#2196f3",
            TaskStatus.InReview => "#ff9800",
            TaskStatus.Completed => "#4caf50",
            TaskStatus.Blocked => "#f44336",
            _ => "#9e9e9e"
        };
    }
}