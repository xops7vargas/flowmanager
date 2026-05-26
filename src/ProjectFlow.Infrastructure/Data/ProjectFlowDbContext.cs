using Microsoft.EntityFrameworkCore;
using ProjectFlow.Domain.Entities;

namespace ProjectFlow.Infrastructure.Data;

public class ProjectFlowDbContext : DbContext
{
    public ProjectFlowDbContext(DbContextOptions<ProjectFlowDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskDependency> TaskDependencies => Set<TaskDependency>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<Delay> Delays => Set<Delay>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<FinancialTransaction> FinancialTransactions => Set<FinancialTransaction>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceMovement> ResourceMovements => Set<ResourceMovement>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Bio).HasColumnType("text");
            entity.Property(e => e.Avatar).HasColumnType("text");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Module).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.HasIndex(e => new { e.Module, e.Name }).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.HasOne(e => e.User).WithMany(u => u.UserRoles).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Role).WithMany(r => r.UserRoles).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            entity.HasOne(e => e.Role).WithMany(r => r.RolePermissions).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Permission).WithMany(p => p.RolePermissions).HasForeignKey(e => e.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Budget).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Owner).WithMany(u => u.OwnedProjects).HasForeignKey(e => e.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.ToTable("ProjectMembers");
            entity.HasKey(e => new { e.ProjectId, e.UserId });
            entity.Property(e => e.RoleInProject).HasMaxLength(50);
            entity.HasOne(e => e.Project).WithMany(p => p.Members).HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany(u => u.ProjectMembers).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.EstimatedHours).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ActualHours).HasColumnType("decimal(18,2)");
            entity.HasOne(e => e.Project).WithMany(p => p.Tasks).HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ParentTask).WithMany(t => t.Subtasks).HasForeignKey(e => e.ParentTaskId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CreatedBy).WithMany(u => u.CreatedTasks).HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AssignedTo).WithMany(u => u.AssignedTasks).HasForeignKey(e => e.AssignedToId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TaskDependency>(entity =>
        {
            entity.ToTable("TaskDependencies");
            entity.HasKey(e => new { e.PredecessorTaskId, e.SuccessorTaskId });
            entity.HasOne(e => e.PredecessorTask).WithMany(t => t.SuccessorDependencies).HasForeignKey(e => e.PredecessorTaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.SuccessorTask).WithMany(t => t.PredecessorDependencies).HasForeignKey(e => e.SuccessorTaskId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.ToTable("TimeEntries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Hours).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.Task).WithMany(t => t.TimeEntries).HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany(u => u.TimeEntries).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).HasColumnType("text").IsRequired();
            entity.HasOne(e => e.Task).WithMany(t => t.Comments).HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany(u => u.Comments).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Parent).WithMany(c => c.Replies).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.ToTable("Attachments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.HasOne(e => e.Task).WithMany(t => t.Attachments).HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(7).HasDefaultValue("#2196f3");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<TaskTag>(entity =>
        {
            entity.ToTable("TaskTags");
            entity.HasKey(e => new { e.TaskId, e.TagId });
            entity.HasOne(e => e.Task).WithMany(t => t.TaskTags).HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Tag).WithMany(t => t.TaskTags).HasForeignKey(e => e.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).HasColumnType("text");
            entity.HasOne(e => e.User).WithMany(u => u.Notifications).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.OldValues).HasColumnType("jsonb");
            entity.Property(e => e.NewValues).HasColumnType("jsonb");
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.HasOne(e => e.User).WithMany(u => u.AuditLogs).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Workflow>(entity =>
        {
            entity.ToTable("Workflows");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.HasOne(e => e.Project).WithMany(p => p.Workflows).HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkflowTransition>(entity =>
        {
            entity.ToTable("WorkflowTransitions");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Workflow).WithMany(w => w.Transitions).HasForeignKey(e => e.WorkflowId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.RequiredRole).WithMany().HasForeignKey(e => e.RequiredRoleId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Delay>(entity =>
        {
            entity.ToTable("Delays");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasColumnType("text").IsRequired();
            entity.HasOne(e => e.Task).WithMany(t => t.Delays).HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.CreatedBy).WithMany(u => u.Delays).HasForeignKey(e => e.CreatedById).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ExpenseCategory>(entity =>
        {
            entity.ToTable("ExpenseCategories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Color).HasMaxLength(7).HasDefaultValue("#2196f3");
            entity.HasOne(e => e.Parent).WithMany(c => c.Children).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.ToTable("FinancialTransactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Category).WithMany(c => c.Transactions).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.ToTable("Resources");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.UnitValue).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.HasOne(e => e.AssignedTo).WithMany().HasForeignKey(e => e.AssignedToId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ResourceMovement>(entity =>
        {
            entity.ToTable("ResourceMovements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.HasOne(e => e.Resource).WithMany(r => r.Movements).HasForeignKey(e => e.ResourceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("Conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<ConversationParticipant>(entity =>
        {
            entity.ToTable("ConversationParticipants");
            entity.HasKey(e => new { e.ConversationId, e.UserId });
            entity.HasOne(e => e.Conversation).WithMany(c => c.Participants).HasForeignKey(e => e.ConversationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).HasColumnType("text").IsRequired();
            entity.HasOne(e => e.Conversation).WithMany(c => c.Messages).HasForeignKey(e => e.ConversationId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Sender).WithMany().HasForeignKey(e => e.SenderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ReplyTo).WithMany().HasForeignKey(e => e.ReplyToId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.ToTable("SystemSettings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Value).HasColumnType("text");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.HasIndex(e => e.Key).IsUnique();
        });
    }
}