using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;
using ProjectFlow.Domain.Interfaces;
using NotificationType = ProjectFlow.Domain.Enums.NotificationType;
using DelayCategory = ProjectFlow.Domain.Enums.DelayCategory;

namespace ProjectFlow.Application.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITaskService _taskService;

    public TimeEntryService(IUnitOfWork unitOfWork, ITaskService taskService)
    {
        _unitOfWork = unitOfWork;
        _taskService = taskService;
    }

    public async Task<PaginatedResultDto<TimeEntryDto>> GetAllAsync(int page = 1, int pageSize = 20, Guid? taskId = null, Guid? userId = null)
    {
        var query = await _unitOfWork.TimeEntries.GetAllAsync();

        if (taskId.HasValue) query = query.Where(te => te.TaskId == taskId.Value);
        if (userId.HasValue) query = query.Where(te => te.UserId == userId.Value);

        var totalCount = query.Count();
        var items = query.OrderByDescending(te => te.Date).Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResultDto<TimeEntryDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TimeEntryDto> GetByIdAsync(Guid id)
    {
        var entry = await _unitOfWork.TimeEntries.GetByIdAsync(id);
        if (entry == null) throw new KeyNotFoundException("Time entry not found");
        return MapToDto(entry);
    }

    public async Task<TimeEntryDto> CreateAsync(CreateTimeEntryDto dto, Guid userId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(dto.TaskId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        var entry = new TimeEntry
        {
            Id = Guid.NewGuid(),
            TaskId = dto.TaskId,
            UserId = userId,
            Hours = dto.Hours,
            Date = dto.Date,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        task.ActualHours += dto.Hours;

        await _unitOfWork.TimeEntries.AddAsync(entry);
        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(entry);
    }

    public async Task<TimeEntryDto> UpdateAsync(Guid id, UpdateTimeEntryDto dto, Guid userId)
    {
        var entry = await _unitOfWork.TimeEntries.GetByIdAsync(id);
        if (entry == null) throw new KeyNotFoundException("Time entry not found");
        if (entry.UserId != userId) throw new UnauthorizedAccessException("Not authorized");

        var task = await _unitOfWork.Tasks.GetByIdAsync(entry.TaskId);
        if (task != null)
        {
            task.ActualHours = task.ActualHours - entry.Hours + dto.Hours;
            await _unitOfWork.Tasks.UpdateAsync(task);
        }

        entry.Hours = dto.Hours;
        entry.Date = dto.Date;
        entry.Description = dto.Description;

        await _unitOfWork.TimeEntries.UpdateAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(entry);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var entry = await _unitOfWork.TimeEntries.GetByIdAsync(id);
        if (entry == null) throw new KeyNotFoundException("Time entry not found");
        if (entry.UserId != userId) throw new UnauthorizedAccessException("Not authorized");

        var task = await _unitOfWork.Tasks.GetByIdAsync(entry.TaskId);
        if (task != null)
        {
            task.ActualHours -= entry.Hours;
            await _unitOfWork.Tasks.UpdateAsync(task);
        }

        await _unitOfWork.TimeEntries.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalHoursAsync(Guid userId, DateTime? start = null, DateTime? end = null)
    {
        return await _unitOfWork.TimeEntries.GetTotalHoursByUserAsync(userId, start, end);
    }

    private static TimeEntryDto MapToDto(TimeEntry e)
    {
        return new TimeEntryDto
        {
            Id = e.Id,
            TaskId = e.TaskId,
            TaskTitle = e.Task.Title,
            UserId = e.UserId,
            UserName = $"{e.User.FirstName} {e.User.LastName}",
            Hours = e.Hours,
            Date = e.Date,
            Description = e.Description,
            CreatedAt = e.CreatedAt
        };
    }
}

public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public CommentService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<List<CommentDto>> GetByTaskAsync(Guid taskId)
    {
        var comments = await _unitOfWork.Comments.GetByTaskAsync(taskId);
        var rootComments = comments.Where(c => c.ParentId == null).ToList();
        return rootComments.Select(MapToDtoWithReplies).ToList();
    }

    public async Task<CommentDto> CreateAsync(CreateCommentDto dto, Guid userId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(dto.TaskId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            TaskId = dto.TaskId,
            UserId = userId,
            Content = dto.Content,
            ParentId = dto.ParentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Comments.AddAsync(comment);

        if (task.AssignedToId.HasValue && task.AssignedToId != userId)
        {
            await _notificationService.CreateNotificationAsync(task.AssignedToId.Value, "Nuevo comentario", $"Nuevo comentario en: {task.Title}", NotificationType.CommentAdded, task.Id);
        }

        await _unitOfWork.SaveChangesAsync();
        return MapToDto(comment);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(id);
        if (comment == null) throw new KeyNotFoundException("Comment not found");
        if (comment.UserId != userId) throw new UnauthorizedAccessException("Not authorized");

        await _unitOfWork.Comments.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    private static CommentDto MapToDtoWithReplies(Comment c)
    {
        var dto = new CommentDto
        {
            Id = c.Id,
            TaskId = c.TaskId,
            UserId = c.UserId,
            UserName = $"{c.User.FirstName} {c.User.LastName}",
            UserAvatar = c.User.Avatar,
            Content = c.Content,
            ParentId = c.ParentId,
            CreatedAt = c.CreatedAt,
            Replies = c.Replies.Select(MapToDto).ToList()
        };
        return dto;
    }

    private static CommentDto MapToDto(Comment c)
    {
        return new CommentDto
        {
            Id = c.Id,
            TaskId = c.TaskId,
            UserId = c.UserId,
            UserName = $"{c.User.FirstName} {c.User.LastName}",
            UserAvatar = c.User.Avatar,
            Content = c.Content,
            ParentId = c.ParentId,
            CreatedAt = c.CreatedAt
        };
    }
}

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<NotificationDto>> GetByUserAsync(Guid userId, bool unreadOnly = false)
    {
        var notifications = await _unitOfWork.Notifications.GetByUserAsync(userId, unreadOnly);
        return notifications.Select(MapToDto).ToList();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
    }

    public async Task MarkAsReadAsync(Guid id, Guid userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
        if (notification == null || notification.UserId != userId) throw new KeyNotFoundException("Notification not found");

        notification.IsRead = true;
        await _unitOfWork.Notifications.UpdateAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var notifications = await _unitOfWork.Notifications.GetByUserAsync(userId, false);
        foreach (var n in notifications.Where(n => !n.IsRead))
        {
            n.IsRead = true;
            await _unitOfWork.Notifications.UpdateAsync(n);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
        if (notification == null || notification.UserId != userId) throw new KeyNotFoundException("Notification not found");

        await _unitOfWork.Notifications.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CreateNotificationAsync(Guid userId, string title, string? message, NotificationType type, Guid? referenceId = null)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ReferenceId = referenceId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Notifications.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }

    private static NotificationDto MapToDto(Notification n)
    {
        return new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            ReferenceId = n.ReferenceId,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}

public class TagService : ITagService
{
    private readonly IUnitOfWork _unitOfWork;

    public TagService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TagDto>> GetAllAsync()
    {
        var tags = await _unitOfWork.Tags.GetAllAsync();
        return tags.Select(t => new TagDto { Id = t.Id, Name = t.Name, Color = t.Color });
    }

    public async Task<TagDto> CreateAsync(CreateTagDto dto)
    {
        var existing = await _unitOfWork.Tags.GetByNameAsync(dto.Name);
        if (existing != null) throw new InvalidOperationException("Tag already exists");

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Color = dto.Color
        };

        await _unitOfWork.Tags.AddAsync(tag);
        await _unitOfWork.SaveChangesAsync();

        return new TagDto { Id = tag.Id, Name = tag.Name, Color = tag.Color };
    }

    public async Task<TagDto> UpdateAsync(Guid id, CreateTagDto dto)
    {
        var tag = await _unitOfWork.Tags.GetByIdAsync(id);
        if (tag == null) throw new KeyNotFoundException("Tag not found");

        tag.Name = dto.Name;
        tag.Color = dto.Color;

        await _unitOfWork.Tags.UpdateAsync(tag);
        await _unitOfWork.SaveChangesAsync();

        return new TagDto { Id = tag.Id, Name = tag.Name, Color = tag.Color };
    }

    public async Task DeleteAsync(Guid id)
    {
        await _unitOfWork.Tags.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AddToTaskAsync(Guid taskId, Guid tagId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        var tag = await _unitOfWork.Tags.GetByIdAsync(tagId);
        if (tag == null) throw new KeyNotFoundException("Tag not found");

        task.TaskTags.Add(new TaskTag { TaskId = taskId, TagId = tagId });
        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFromTaskAsync(Guid taskId, Guid tagId)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        var taskTag = task.TaskTags.FirstOrDefault(tt => tt.TagId == tagId);
        if (taskTag != null)
        {
            task.TaskTags.Remove(taskTag);
            await _unitOfWork.Tasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

public class DelayService : IDelayService
{
    private readonly IUnitOfWork _unitOfWork;

    public DelayService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DelayDto>> GetByTaskAsync(Guid taskId)
    {
        var delays = await _unitOfWork.Delays.GetByTaskAsync(taskId);
        return delays.Select(MapToDto).ToList();
    }

    public async Task<DelayDto> CreateAsync(CreateDelayDto dto, Guid createdById)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(dto.TaskId);
        if (task == null) throw new KeyNotFoundException("Task not found");

        var delay = new Delay
        {
            Id = Guid.NewGuid(),
            TaskId = dto.TaskId,
            Reason = dto.Reason,
            Category = dto.Category,
            DaysDelayed = dto.DaysDelayed,
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        task.DueDate = task.DueDate?.AddDays(dto.DaysDelayed);

        await _unitOfWork.Delays.AddAsync(delay);
        await _unitOfWork.Tasks.UpdateAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(delay);
    }

    public async Task<List<DelayDto>> GetAllAsync(int page = 1, int pageSize = 20, DelayCategory? category = null)
    {
        var query = await _unitOfWork.Delays.GetAllAsync();
        if (category.HasValue) query = query.Where(d => d.Category == category.Value);

        var delays = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return delays.Select(MapToDto).ToList();
    }

    private static DelayDto MapToDto(Delay d)
    {
        return new DelayDto
        {
            Id = d.Id,
            TaskId = d.TaskId,
            TaskTitle = d.Task.Title,
            Reason = d.Reason,
            Category = d.Category,
            DaysDelayed = d.DaysDelayed,
            CreatedById = d.CreatedById,
            CreatedByName = $"{d.CreatedBy.FirstName} {d.CreatedBy.LastName}",
            CreatedAt = d.CreatedAt
        };
    }
}