using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;
using ProjectFlow.Domain.Interfaces;
using TransactionType = ProjectFlow.Domain.Enums.TransactionType;
using ResourceType = ProjectFlow.Domain.Enums.ResourceType;
using ResourceStatus = ProjectFlow.Domain.Enums.ResourceStatus;
using MovementType = ProjectFlow.Domain.Enums.MovementType;
using ConversationType = ProjectFlow.Domain.Enums.ConversationType;
using MessageType = ProjectFlow.Domain.Enums.MessageType;

namespace ProjectFlow.Application.Services;

public class FinancialService : IFinancialService
{
    private readonly IUnitOfWork _unitOfWork;

    public FinancialService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ExpenseCategoryDto>> GetCategoriesAsync(bool? isIncome = null)
    {
        var categories = await _unitOfWork.ExpenseCategories.GetAllAsync();
        if (isIncome.HasValue)
            categories = categories.Where(c => c.IsIncome == isIncome.Value);
        
        return categories.Select(c => new ExpenseCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Color = c.Color,
            IsIncome = c.IsIncome,
            ParentId = c.ParentId
        });
    }

    public async Task<ExpenseCategoryDto> CreateCategoryAsync(CreateExpenseCategoryDto dto)
    {
        var category = new ExpenseCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            IsIncome = dto.IsIncome,
            ParentId = dto.ParentId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ExpenseCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return new ExpenseCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            IsIncome = category.IsIncome,
            ParentId = category.ParentId
        };
    }

    public async Task<ExpenseCategoryDto> UpdateCategoryAsync(Guid id, CreateExpenseCategoryDto dto)
    {
        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(id);
        if (category == null) throw new KeyNotFoundException("Category not found");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.Color = dto.Color;
        category.IsIncome = dto.IsIncome;
        category.ParentId = dto.ParentId;

        await _unitOfWork.ExpenseCategories.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return new ExpenseCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            IsIncome = category.IsIncome,
            ParentId = category.ParentId
        };
    }

    public async Task DeleteCategoryAsync(Guid id)
    {
        await _unitOfWork.ExpenseCategories.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PaginatedResultDto<FinancialTransactionDto>> GetTransactionsAsync(
        int page = 1, int pageSize = 20, Guid? projectId = null, 
        DateTime? startDate = null, DateTime? endDate = null, 
        TransactionType? type = null, Guid? categoryId = null)
    {
        var query = await _unitOfWork.FinancialTransactions.GetAllAsync();

        if (projectId.HasValue) query = query.Where(t => t.ProjectId == projectId.Value);
        if (startDate.HasValue) query = query.Where(t => t.Date >= startDate.Value);
        if (endDate.HasValue) query = query.Where(t => t.Date <= endDate.Value);
        if (type.HasValue) query = query.Where(t => t.Type == type.Value);
        if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId.Value);

        var totalCount = query.Count();
        var items = query.OrderByDescending(t => t.Date).Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResultDto<FinancialTransactionDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<FinancialTransactionDto> CreateTransactionAsync(CreateFinancialTransactionDto dto, Guid userId)
    {
        var transaction = new FinancialTransaction
        {
            Id = Guid.NewGuid(),
            ProjectId = dto.ProjectId,
            CategoryId = dto.CategoryId,
            UserId = userId,
            Amount = dto.Amount,
            Description = dto.Description,
            Date = dto.Date,
            Type = dto.Type,
            Reference = dto.Reference,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.FinancialTransactions.AddAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(transaction);
    }

    public async Task<FinancialTransactionDto> UpdateTransactionAsync(Guid id, CreateFinancialTransactionDto dto)
    {
        var transaction = await _unitOfWork.FinancialTransactions.GetByIdAsync(id);
        if (transaction == null) throw new KeyNotFoundException("Transaction not found");

        transaction.ProjectId = dto.ProjectId;
        transaction.CategoryId = dto.CategoryId;
        transaction.Amount = dto.Amount;
        transaction.Description = dto.Description;
        transaction.Date = dto.Date;
        transaction.Type = dto.Type;
        transaction.Reference = dto.Reference;

        await _unitOfWork.FinancialTransactions.UpdateAsync(transaction);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(transaction);
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        await _unitOfWork.FinancialTransactions.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<FinancialReportDto> GetReportAsync(Guid? projectId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = await _unitOfWork.FinancialTransactions.GetAllAsync();

        if (projectId.HasValue) query = query.Where(t => t.ProjectId == projectId.Value);
        if (startDate.HasValue) query = query.Where(t => t.Date >= startDate.Value);
        if (endDate.HasValue) query = query.Where(t => t.Date <= endDate.Value);

        var transactions = query.ToList();

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        var byCategory = transactions
            .GroupBy(t => t.Category.Name)
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        var byMonth = transactions
            .GroupBy(t => t.Date.ToString("MMM yyyy"))
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Type == TransactionType.Income ? t.Amount : -t.Amount));

        return new FinancialReportDto
        {
            TotalIncome = totalIncome,
            TotalExpenses = totalExpenses,
            Balance = totalIncome - totalExpenses,
            Transactions = transactions.OrderByDescending(t => t.Date).Take(100).Select(MapToDto).ToList(),
            ByCategory = byCategory,
            ByMonth = byMonth
        };
    }

    private static FinancialTransactionDto MapToDto(FinancialTransaction t)
    {
        return new FinancialTransactionDto
        {
            Id = t.Id,
            ProjectId = t.ProjectId,
            ProjectName = t.Project?.Name ?? "",
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name ?? "",
            UserId = t.UserId,
            UserName = t.User != null ? $"{t.User.FirstName} {t.User.LastName}" : null,
            Amount = t.Amount,
            Description = t.Description,
            Date = t.Date,
            Type = t.Type,
            Reference = t.Reference
        };
    }
}

public class ResourceService : IResourceService
{
    private readonly IUnitOfWork _unitOfWork;

    public ResourceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResultDto<ResourceDto>> GetAllAsync(
        int page = 1, int pageSize = 20, ResourceType? type = null, 
        ResourceStatus? status = null, string? search = null)
    {
        var query = await _unitOfWork.Resources.GetAllAsync();

        if (type.HasValue) query = query.Where(r => r.Type == type.Value);
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(r => r.Name.Contains(search) || r.Code.Contains(search));

        var totalCount = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResultDto<ResourceDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ResourceDto> GetByIdAsync(Guid id)
    {
        var resource = await _unitOfWork.Resources.GetByIdAsync(id);
        if (resource == null) throw new KeyNotFoundException("Resource not found");
        return MapToDto(resource);
    }

    public async Task<ResourceDto> CreateAsync(CreateResourceDto dto)
    {
        var resource = new Resource
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Code = dto.Code,
            Type = dto.Type,
            Status = ResourceStatus.Available,
            Quantity = dto.Quantity,
            AvailableQuantity = dto.Quantity,
            UnitValue = dto.UnitValue,
            Location = dto.Location,
            PurchaseDate = dto.PurchaseDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Resources.AddAsync(resource);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(resource);
    }

    public async Task<ResourceDto> UpdateAsync(Guid id, UpdateResourceDto dto)
    {
        var resource = await _unitOfWork.Resources.GetByIdAsync(id);
        if (resource == null) throw new KeyNotFoundException("Resource not found");

        resource.Name = dto.Name;
        resource.Description = dto.Description;
        if (dto.Status.HasValue) resource.Status = dto.Status.Value;
        resource.AssignedToId = dto.AssignedToId;
        resource.Location = dto.Location;

        await _unitOfWork.Resources.UpdateAsync(resource);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(resource);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _unitOfWork.Resources.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ResourceMovementDto> CreateMovementAsync(CreateResourceMovementDto dto, Guid userId)
    {
        var resource = await _unitOfWork.Resources.GetByIdAsync(dto.ResourceId);
        if (resource == null) throw new KeyNotFoundException("Resource not found");

        if (dto.Type == MovementType.Entry)
        {
            resource.Quantity += dto.Quantity;
            resource.AvailableQuantity += dto.Quantity;
        }
        else if (dto.Type == MovementType.Exit)
        {
            if (resource.AvailableQuantity < dto.Quantity)
                throw new InvalidOperationException("Not enough available quantity");
            resource.AvailableQuantity -= dto.Quantity;
        }
        else if (dto.Type == MovementType.Assignment)
        {
            if (resource.AvailableQuantity < dto.Quantity)
                throw new InvalidOperationException("Not enough available quantity");
            resource.AvailableQuantity -= dto.Quantity;
            resource.Status = ResourceStatus.InUse;
            resource.AssignedToId = userId;
        }
        else if (dto.Type == MovementType.Return)
        {
            resource.AvailableQuantity += dto.Quantity;
            resource.Status = ResourceStatus.Available;
            resource.AssignedToId = null;
        }

        var movement = new ResourceMovement
        {
            Id = Guid.NewGuid(),
            ResourceId = dto.ResourceId,
            UserId = userId,
            ProjectId = dto.ProjectId,
            Type = dto.Type,
            Quantity = dto.Quantity,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ResourceMovements.AddAsync(movement);
        await _unitOfWork.Resources.UpdateAsync(resource);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(movement);
    }

    public async Task<List<ResourceMovementDto>> GetMovementsAsync(Guid resourceId)
    {
        var movements = await _unitOfWork.ResourceMovements.GetByResourceAsync(resourceId);
        return movements.Select(MapToDto).ToList();
    }

    public async Task AssignToUserAsync(Guid resourceId, Guid userId)
    {
        var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId);
        if (resource == null) throw new KeyNotFoundException("Resource not found");

        resource.AssignedToId = userId;
        resource.Status = ResourceStatus.InUse;

        await _unitOfWork.Resources.UpdateAsync(resource);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReturnFromUserAsync(Guid resourceId)
    {
        var resource = await _unitOfWork.Resources.GetByIdAsync(resourceId);
        if (resource == null) throw new KeyNotFoundException("Resource not found");

        resource.AssignedToId = null;
        resource.Status = ResourceStatus.Available;

        await _unitOfWork.Resources.UpdateAsync(resource);
        await _unitOfWork.SaveChangesAsync();
    }

    private static ResourceDto MapToDto(Resource r)
    {
        return new ResourceDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Code = r.Code,
            Type = r.Type,
            Status = r.Status,
            Quantity = r.Quantity,
            AvailableQuantity = r.AvailableQuantity,
            UnitValue = r.UnitValue,
            AssignedToId = r.AssignedToId,
            AssignedToName = r.AssignedTo != null ? $"{r.AssignedTo.FirstName} {r.AssignedTo.LastName}" : null,
            Location = r.Location,
            PurchaseDate = r.PurchaseDate
        };
    }

    private static ResourceMovementDto MapToDto(ResourceMovement m)
    {
        return new ResourceMovementDto
        {
            Id = m.Id,
            ResourceId = m.ResourceId,
            ResourceName = m.Resource?.Name ?? "",
            UserId = m.UserId,
            UserName = m.User != null ? $"{m.User.FirstName} {m.User.LastName}" : "",
            ProjectId = m.ProjectId,
            ProjectName = m.Project?.Name,
            Type = m.Type,
            Quantity = m.Quantity,
            Notes = m.Notes,
            CreatedAt = m.CreatedAt
        };
    }
}

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChatService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ConversationDto>> GetConversationsAsync(Guid userId)
    {
        var conversations = await _unitOfWork.Conversations.GetByUserAsync(userId);
        return conversations.Select(c => MapToDto(c, userId)).ToList();
    }

    public async Task<ConversationDto> GetConversationAsync(Guid conversationId, Guid userId)
    {
        var conversation = await _unitOfWork.Conversations.GetByIdAsync(conversationId);
        if (conversation == null) throw new KeyNotFoundException("Conversation not found");
        
        if (!conversation.Participants.Any(p => p.UserId == userId))
            throw new UnauthorizedAccessException("Not authorized");

        return MapToDto(conversation, userId);
    }

    public async Task<ConversationDto> CreateConversationAsync(CreateConversationDto dto, Guid userId)
    {
        Conversation conversation;

        if (dto.Type == ConversationType.Direct && dto.ParticipantIds.Count == 1)
        {
            var existing = await _unitOfWork.Conversations.GetDirectConversationAsync(userId, dto.ParticipantIds[0]);
            if (existing != null) return MapToDto(existing, userId);
        }

        conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Type = dto.Type,
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow
        };

        var participantIds = dto.ParticipantIds.ToList();
        if (!participantIds.Contains(userId))
        {
            participantIds.Add(userId);
        }

        foreach (var participantId in participantIds.Distinct())
        {
            conversation.Participants.Add(new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = participantId,
                JoinedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.Conversations.AddAsync(conversation);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(conversation, userId);
    }

    public async Task<List<MessageDto>> GetMessagesAsync(Guid conversationId, int page = 1, int pageSize = 50)
    {
        var messages = await _unitOfWork.Messages.GetByConversationAsync(conversationId, page, pageSize);
        return messages.Select(MapToDto).ToList();
    }

    public async Task<MessageDto> SendMessageAsync(CreateMessageDto dto, Guid senderId)
    {
        var conversation = await _unitOfWork.Conversations.GetByIdAsync(dto.ConversationId);
        if (conversation == null) throw new KeyNotFoundException("Conversation not found");

        if (!conversation.Participants.Any(p => p.UserId == senderId))
            throw new UnauthorizedAccessException("Not authorized");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = dto.ConversationId,
            SenderId = senderId,
            Content = dto.Content,
            Type = dto.Type,
            ReplyToId = dto.ReplyToId,
            CreatedAt = DateTime.UtcNow
        };

        conversation.LastMessageAt = DateTime.UtcNow;

        await _unitOfWork.Messages.AddAsync(message);
        await _unitOfWork.Conversations.UpdateAsync(conversation);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(message);
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid userId)
    {
        var participant = await _unitOfWork.ConversationParticipants.GetAsync(conversationId, userId);
        if (participant != null)
        {
            participant.LastReadAt = DateTime.UtcNow;
            await _unitOfWork.ConversationParticipants.UpdateAsync(participant);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task<List<ConversationDto>> GetDirectMessagesAsync(Guid userId, Guid otherUserId)
    {
        var conversation = await _unitOfWork.Conversations.GetDirectConversationAsync(userId, otherUserId);
        if (conversation == null) return new List<ConversationDto>();
        return new List<ConversationDto> { MapToDto(conversation, userId) };
    }

    private static ConversationDto MapToDto(Conversation c, Guid currentUserId)
    {
        var lastMessage = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
        
        return new ConversationDto
        {
            Id = c.Id,
            Type = c.Type,
            Name = c.Name,
            Participants = c.Participants.Select(p => new ConversationParticipantDto
            {
                UserId = p.UserId,
                UserName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : "",
                Avatar = p.User?.Avatar,
                IsOnline = p.User?.IsActive ?? false
            }).ToList(),
            LastMessage = lastMessage != null ? MapToDto(lastMessage) : null,
            LastMessageAt = c.LastMessageAt,
            UnreadCount = c.Participants
                .Where(p => p.UserId == currentUserId && p.LastReadAt.HasValue)
                .Select(p => c.Messages.Count(m => m.SenderId != currentUserId && m.CreatedAt > p.LastReadAt))
                .FirstOrDefault()
        };
    }

    private static MessageDto MapToDto(Message m)
    {
        return new MessageDto
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            SenderId = m.SenderId,
            SenderName = m.Sender != null ? $"{m.Sender.FirstName} {m.Sender.LastName}" : "",
            SenderAvatar = m.Sender?.Avatar,
            Content = m.Content,
            Type = m.Type,
            ReplyToId = m.ReplyToId,
            ReplyToContent = m.ReplyTo?.Content,
            CreatedAt = m.CreatedAt
        };
    }
}

public class SettingsService : ISettingsService
{
    private readonly IUnitOfWork _unitOfWork;

    public SettingsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<SystemSettingDto>> GetAllAsync()
    {
        var settings = await _unitOfWork.SystemSettings.GetAllAsync();
        return settings.Select(MapToDto);
    }

    public async Task<SystemSettingDto> GetByKeyAsync(string key)
    {
        var setting = await _unitOfWork.SystemSettings.GetByKeyAsync(key);
        if (setting == null) throw new KeyNotFoundException("Setting not found");
        return MapToDto(setting);
    }

    public async Task<SystemSettingDto> UpdateAsync(string key, UpdateSystemSettingDto dto)
    {
        var setting = await _unitOfWork.SystemSettings.GetByKeyAsync(key);
        if (setting == null)
        {
            setting = new SystemSetting
            {
                Id = Guid.NewGuid(),
                Key = key,
                Value = dto.Value,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.SystemSettings.AddAsync(setting);
        }
        else
        {
            setting.Value = dto.Value;
            setting.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SystemSettings.UpdateAsync(setting);
        }
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(setting);
    }

    public async Task InitializeDefaultsAsync()
    {
        var defaults = new Dictionary<string, string>
        {
            { "AllowRegistration", "true" },
            { "RequireEmailVerification", "false" },
            { "DefaultUserRole", "User" },
            { "CompanyName", "Sonyi-Flow" },
            { "Language", "es" },
            { "Theme", "light" }
        };

        foreach (var (key, value) in defaults)
        {
            var existing = await _unitOfWork.SystemSettings.GetByKeyAsync(key);
            if (existing == null)
            {
                await _unitOfWork.SystemSettings.AddAsync(new SystemSetting
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> GetBoolValueAsync(string key)
    {
        var setting = await _unitOfWork.SystemSettings.GetByKeyAsync(key);
        return setting?.Value == "true";
    }

    public async Task<string> GetStringValueAsync(string key)
    {
        var setting = await _unitOfWork.SystemSettings.GetByKeyAsync(key);
        return setting?.Value ?? "";
    }

    private static SystemSettingDto MapToDto(SystemSetting s)
    {
        return new SystemSettingDto
        {
            Id = s.Id,
            Key = s.Key,
            Value = s.Value,
            Description = s.Description,
            Type = s.Type
        };
    }
}

public class AnalyticsService : IAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public AnalyticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AnalyticsDto> GetAnalyticsAsync(Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var compliance = await GetComplianceMetricsAsync(userId, startDate, endDate);
        var userPerformance = await GetUserPerformanceAsync(null, startDate, endDate);
        var projectMetrics = await GetProjectMetricsAsync(startDate, endDate);
        var monthlyData = await GetMonthlyDataAsync(12);
        var priorityDistribution = await GetPriorityDistributionAsync(startDate, endDate);

        return new AnalyticsDto
        {
            Compliance = compliance,
            UserPerformance = userPerformance,
            ProjectMetrics = projectMetrics,
            MonthlyData = monthlyData,
            PriorityDistribution = priorityDistribution
        };
    }

    public async Task<ComplianceMetricsDto> GetComplianceMetricsAsync(Guid? userId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync();
        
        if (userId.HasValue)
            tasks = tasks.Where(t => t.AssignedToId == userId.Value);
        
        var taskList = tasks.ToList();

        var total = taskList.Count;
        var completed = taskList.Count(t => t.Status == Domain.Enums.TaskStatus.Completed);
        var overdue = taskList.Count(t => t.DueDate < DateTime.UtcNow && t.Status != Domain.Enums.TaskStatus.Completed);

        var complianceRate = total > 0 ? (double)completed / total * 100 : 0;
        var overdueRate = total > 0 ? (double)overdue / total * 100 : 0;

        return new ComplianceMetricsDto
        {
            CompletionRate = complianceRate,
            ComplianceRate = 100 - overdueRate,
            OverdueRate = overdueRate,
            TotalTasks = total,
            CompletedTasks = completed,
            OverdueTasks = overdue
        };
    }

    public async Task<List<UserPerformanceDto>> GetUserPerformanceAsync(Guid? projectId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var tasks = await _unitOfWork.Tasks.GetAllAsync();
        var timeEntries = await _unitOfWork.TimeEntries.GetAllAsync();

        if (projectId.HasValue)
            tasks = tasks.Where(t => t.ProjectId == projectId.Value);

        var taskList = tasks.ToList();
        var timeList = timeEntries.ToList();

        return users.Select(u => {
            var userTasks = taskList.Where(t => t.AssignedToId == u.Id).ToList();
            return new UserPerformanceDto
            {
                UserId = u.Id,
                UserName = $"{u.FirstName} {u.LastName}",
                Avatar = u.Avatar,
                TasksCompleted = userTasks.Count(t => t.Status == Domain.Enums.TaskStatus.Completed),
                TasksInProgress = userTasks.Count(t => t.Status == Domain.Enums.TaskStatus.InProgress),
                OverdueTasks = userTasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != Domain.Enums.TaskStatus.Completed),
                HoursWorked = timeList.Where(te => te.UserId == u.Id).Sum(te => te.Hours),
                CompletionRate = userTasks.Count > 0 ? (double)userTasks.Count(t => t.Status == Domain.Enums.TaskStatus.Completed) / userTasks.Count * 100 : 0
            };
        }).ToList();
    }

    public async Task<List<ProjectMetricsDto>> GetProjectMetricsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var projects = await _unitOfWork.Projects.GetAllAsync();
        var tasks = await _unitOfWork.Tasks.GetAllAsync();
        var transactions = await _unitOfWork.FinancialTransactions.GetAllAsync();

        var taskList = tasks.ToList();
        var transactionList = transactions.ToList();

        return projects.Select(p => {
            var projectTasks = taskList.Where(t => t.ProjectId == p.Id).ToList();
            return new ProjectMetricsDto
            {
                ProjectId = p.Id,
                ProjectName = p.Name,
                TotalTasks = projectTasks.Count,
                CompletedTasks = projectTasks.Count(t => t.Status == Domain.Enums.TaskStatus.Completed),
                OverdueTasks = projectTasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != Domain.Enums.TaskStatus.Completed),
                Progress = p.Progress,
                Budget = p.Budget ?? 0,
                Spent = transactionList.Where(t => t.ProjectId == p.Id && t.Type == TransactionType.Expense).Sum(t => t.Amount)
            };
        }).ToList();
    }

    public async Task<List<MonthlyDataDto>> GetMonthlyDataAsync(int months = 12)
    {
        var startDate = DateTime.UtcNow.AddMonths(-months);
        var tasks = await _unitOfWork.Tasks.GetAllAsync();
        var timeEntries = await _unitOfWork.TimeEntries.GetAllAsync();
        var transactions = await _unitOfWork.FinancialTransactions.GetAllAsync();

        var taskList = tasks.Where(t => t.CreatedAt >= startDate).ToList();
        var timeList = timeEntries.Where(t => t.Date >= startDate).ToList();
        var transactionList = transactions.Where(t => t.Date >= startDate).ToList();

        var result = new List<MonthlyDataDto>();
        for (int i = months - 1; i >= 0; i--)
        {
            var month = DateTime.UtcNow.AddMonths(-i);
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthTasks = taskList.Where(t => t.CreatedAt >= monthStart && t.CreatedAt <= monthEnd).ToList();
            var monthTime = timeList.Where(t => t.Date >= monthStart && t.Date <= monthEnd).ToList();
            var monthTransactions = transactionList.Where(t => t.Date >= monthStart && t.Date <= monthEnd).ToList();

            result.Add(new MonthlyDataDto
            {
                Month = monthStart.ToString("MMM yyyy"),
                TasksCreated = monthTasks.Count,
                TasksCompleted = monthTasks.Count(t => t.Status == Domain.Enums.TaskStatus.Completed),
                HoursWorked = monthTime.Sum(t => t.Hours),
                Income = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                Expenses = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
            });
        }

        return result;
    }

    private async Task<List<PriorityDistributionDto>> GetPriorityDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var tasks = await _unitOfWork.Tasks.GetAllAsync();
        var taskList = tasks.ToList();
        var total = taskList.Count;

        return Enum.GetValues<Domain.Enums.TaskPriority>()
            .Select(p => new PriorityDistributionDto
            {
                Priority = p,
                Count = taskList.Count(t => t.Priority == p),
                Percentage = total > 0 ? (double)taskList.Count(t => t.Priority == p) / total * 100 : 0
            }).ToList();
    }

    public async Task<object> GetProjectReportAsync(Guid projectId, DateTime? startDate, DateTime? endDate)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null) throw new KeyNotFoundException("Proyecto no encontrado");

        var tasks = (await _unitOfWork.Tasks.GetAllAsync()).Where(t => t.ProjectId == projectId).ToList();
        var transactions = (await _unitOfWork.FinancialTransactions.GetAllAsync())
            .Where(t => t.ProjectId == projectId).ToList();

        return new
        {
            project = new { project.Id, project.Name, project.Status, project.Budget, project.Progress },
            tasks = new
            {
                total = tasks.Count,
                completed = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Completed),
                inProgress = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.InProgress),
                overdue = tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != Domain.Enums.TaskStatus.Completed)
            },
            financials = new
            {
                totalIncome = transactions.Where(t => t.Type == Domain.Enums.TransactionType.Income).Sum(t => t.Amount),
                totalExpenses = transactions.Where(t => t.Type == Domain.Enums.TransactionType.Expense).Sum(t => t.Amount),
                balance = transactions.Where(t => t.Type == Domain.Enums.TransactionType.Income).Sum(t => t.Amount) -
                         transactions.Where(t => t.Type == Domain.Enums.TransactionType.Expense).Sum(t => t.Amount)
            },
            generatedAt = DateTime.UtcNow
        };
    }

    public async Task<object> GetUserReportAsync(Guid userId, DateTime? startDate, DateTime? endDate)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("Usuario no encontrado");

        var tasks = (await _unitOfWork.Tasks.GetAllAsync())
            .Where(t => t.AssignedToId == userId).ToList();
        var timeEntries = (await _unitOfWork.TimeEntries.GetAllAsync())
            .Where(t => t.UserId == userId).ToList();

        return new
        {
            user = new { user.Id, user.FirstName, user.LastName, user.Email },
            tasks = new
            {
                assigned = tasks.Count,
                completed = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.Completed),
                inProgress = tasks.Count(t => t.Status == Domain.Enums.TaskStatus.InProgress),
                overdue = tasks.Count(t => t.DueDate < DateTime.UtcNow && t.Status != Domain.Enums.TaskStatus.Completed)
            },
            timeTracking = new
            {
                totalHours = timeEntries.Sum(t => t.Hours),
                entries = timeEntries.Count
            },
            generatedAt = DateTime.UtcNow
        };
    }

    public async Task<object> GetFinancialReportAsync(Guid? projectId, DateTime? startDate, DateTime? endDate)
    {
        var transactions = await _unitOfWork.FinancialTransactions.GetAllAsync();
        
        if (projectId.HasValue)
            transactions = transactions.Where(t => t.ProjectId == projectId.Value).ToList();
        
        if (startDate.HasValue)
            transactions = transactions.Where(t => t.Date >= startDate.Value).ToList();
        if (endDate.HasValue)
            transactions = transactions.Where(t => t.Date <= endDate.Value).ToList();

        var transactionList = transactions.ToList();

        return new
        {
            summary = new
            {
                totalIncome = transactionList.Where(t => t.Type == Domain.Enums.TransactionType.Income).Sum(t => t.Amount),
                totalExpenses = transactionList.Where(t => t.Type == Domain.Enums.TransactionType.Expense).Sum(t => t.Amount),
                balance = transactionList.Where(t => t.Type == Domain.Enums.TransactionType.Income).Sum(t => t.Amount) -
                         transactionList.Where(t => t.Type == Domain.Enums.TransactionType.Expense).Sum(t => t.Amount)
            },
            transactions = transactionList.Select(t => new
            {
                t.Id,
                t.Amount,
                t.Description,
                t.Type,
                t.Date,
                t.Reference
            }),
            generatedAt = DateTime.UtcNow
        };
    }
}
