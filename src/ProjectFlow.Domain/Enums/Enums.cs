namespace ProjectFlow.Domain.Enums;

public enum ProjectStatus
{
    Planning = 0,
    InProgress = 1,
    OnHold = 2,
    AtRisk = 3,
    Delayed = 4,
    Completed = 5,
    Cancelled = 6
}

public enum TaskStatus
{
    Todo = 0,
    InProgress = 1,
    InReview = 2,
    Completed = 3,
    Blocked = 4
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum DelayCategory
{
    Technical = 0,
    Organizational = 1,
    External = 2,
    Requirements = 3,
    Resources = 4
}

public enum NotificationType
{
    TaskAssigned = 0,
    TaskUpdated = 1,
    TaskCompleted = 2,
    TaskDueSoon = 3,
    TaskOverdue = 4,
    CommentAdded = 5,
    ProjectUpdated = 6,
    Mention = 7
}

public enum DependencyType
{
    FinishToStart = 0,
    StartToStart = 1,
    FinishToFinish = 2,
    StartToFinish = 3
}

public enum TransactionType
{
    Income = 0,
    Expense = 1
}

public enum ResourceType
{
    Equipment = 0,
    Furniture = 1,
    Electronics = 2,
    Vehicles = 3,
    Tools = 4,
    OfficeSupplies = 5,
    Other = 6
}

public enum ResourceStatus
{
    Available = 0,
    InUse = 1,
    Damaged = 2,
    UnderMaintenance = 3,
    Retired = 4
}

public enum MovementType
{
    Entry = 0,
    Exit = 1,
    Assignment = 2,
    Return = 3,
    Maintenance = 4,
    Retirement = 5
}

public enum ConversationType
{
    Direct = 0,
    Group = 1
}

public enum MessageType
{
    Text = 0,
    File = 1,
    Image = 2
}

public enum SettingType
{
    String = 0,
    Boolean = 1,
    Number = 2,
    Json = 3
}