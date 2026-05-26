using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectFlow.Application.DTOs;
using ProjectFlow.Application.Interfaces;
using ProjectFlow.Domain.Entities;
using ProjectFlow.Domain.Enums;
using ProjectFlow.Domain.Interfaces;

namespace ProjectFlow.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResultDto<UserDto>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        var query = await _unitOfWork.Users.GetAllAsync();
        
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));
        }

        var totalCount = query.Count();
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResultDto<UserDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        return MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var existing = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existing != null) throw new InvalidOperationException("Email already exists");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);

        var defaultRole = await _unitOfWork.Roles.GetByNameAsync("Developer");
        if (defaultRole == null)
            defaultRole = await _unitOfWork.Roles.GetByNameAsync("ProjectManager");
        
        if (defaultRole != null)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = defaultRole.Id });
        }

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> CreateWithRoleAsync(CreateUserWithRoleDto dto)
    {
        var existing = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existing != null) throw new InvalidOperationException("Email already exists");

        var role = await _unitOfWork.Roles.GetByIdAsync(dto.RoleId);
        if (role == null) throw new InvalidOperationException("Role not found");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        
        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = dto.RoleId });

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Avatar = dto.Avatar;
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        if (dto.RoleId.HasValue && dto.RoleId != Guid.Empty)
        {
            var roleId = dto.RoleId.Value;
            
            var existingUserRoles = await _unitOfWork.UserRoles.GetByUserIdAsync(id);
            foreach (var ur in existingUserRoles)
            {
                _unitOfWork.UserRoles.DeleteAsync(ur.UserId).Wait();
            }

            var newUserRole = new UserRole { UserId = id, RoleId = roleId };
            await _unitOfWork.UserRoles.AddAsync(newUserRole);
        }

        await _unitOfWork.SaveChangesAsync();

        user = await _unitOfWork.Users.GetByIdAsync(id);
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateUserProfileDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
        if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
        if (dto.Phone != null) user.Phone = dto.Phone;
        if (dto.Bio != null) user.Bio = dto.Bio;
        if (dto.Avatar != null) user.Avatar = dto.Avatar;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserRoleAsync(Guid userId, Guid roleId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null) throw new KeyNotFoundException("Role not found");

        user.UserRoles.Clear();
        
        var userRole = new UserRole { UserId = userId, RoleId = roleId };
        await _unitOfWork.UserRoles.AddAsync(userRole);
        
        await _unitOfWork.SaveChangesAsync();

        user = await _unitOfWork.Users.GetByIdAsync(userId);
        return MapToDto(user!);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException("User not found");
        
        user.IsActive = false;
        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AssignRolesAsync(Guid userId, List<Guid> roleIds)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.UserRoles.Clear();
        foreach (var roleId in roleIds)
        {
            var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
            if (role != null)
            {
                user.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
            }
        }

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<ProjectMemberDto>> GetUserProjectsAsync(Guid userId)
    {
        var members = await Task.Run(() => _unitOfWork.Projects.GetAllAsync()
            .Result.SelectMany(p => p.Members)
            .Where(m => m.UserId == userId)
            .ToList());

        return members.Select(m => new ProjectMemberDto
        {
            UserId = m.UserId,
            UserName = $"{m.User.FirstName} {m.User.LastName}",
            Avatar = m.User.Avatar,
            RoleInProject = m.RoleInProject,
            JoinedAt = m.JoinedAt
        }).ToList();
    }

    public async Task ActivateAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeactivateAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    private UserDto MapToDto(User user)
    {
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var permissions = new List<string>();
        
        if (user.UserRoles.Any())
        {
            var rolePermissions = _unitOfWork.RolePermissions.GetAllAsync().Result
                .Where(rp => roleIds.Contains(rp.RoleId))
                .ToList();
            var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();
            permissions = _unitOfWork.Permissions.GetAllAsync().Result
                .Where(p => permissionIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToList();
        }
        
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Avatar = user.Avatar,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            Permissions = permissions,
            CreatedAt = user.CreatedAt
        };
    }
}

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;

    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<RoleDto>> GetAllAsync()
    {
        var allRoles = await _unitOfWork.Roles.GetAllAsync();
        var allRolePerms = await _unitOfWork.RolePermissions.GetAllAsync();
        var allPerms = await _unitOfWork.Permissions.GetAllAsync();
        
        var rolesWithPermissions = allRoles.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            IsSystem = r.IsSystem,
            Permissions = allRolePerms
                .Where(rp => rp.RoleId == r.Id)
                .Select(rp => {
                    var perm = allPerms.FirstOrDefault(p => p.Id == rp.PermissionId);
                    return perm != null ? new PermissionDto
                    {
                        Id = perm.Id,
                        Name = perm.Name,
                        Module = perm.Module,
                        Description = perm.Description
                    } : null;
                }).Where(p => p != null).Cast<PermissionDto>().ToList()
        }).ToList();
        
        return rolesWithPermissions;
    }

    public async Task<RoleDto> GetByIdAsync(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null) throw new KeyNotFoundException("Role not found");
        return MapToDto(role);
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto dto)
    {
        var existing = await _unitOfWork.Roles.GetByNameAsync(dto.Name);
        if (existing != null) throw new InvalidOperationException("Role already exists");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Roles.AddAsync(role);
        
        foreach (var permId in dto.PermissionIds)
        {
            var perm = await _unitOfWork.Permissions.GetByIdAsync(permId);
            if (perm != null)
            {
                role.RolePermissions.Add(new RolePermission { RoleId = role.Id, PermissionId = permId });
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return MapToDto(role);
    }

    public async Task<RoleDto> UpdateAsync(Guid id, CreateRoleDto dto)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null) throw new KeyNotFoundException("Role not found");
        if (role.IsSystem) throw new InvalidOperationException("Cannot update system role");

        role.Name = dto.Name;
        role.Description = dto.Description;
        role.RolePermissions.Clear();

        foreach (var permId in dto.PermissionIds)
        {
            role.RolePermissions.Add(new RolePermission { RoleId = id, PermissionId = permId });
        }

        await _unitOfWork.Roles.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(role);
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(id);
        if (role == null) throw new KeyNotFoundException("Role not found");
        if (role.IsSystem) throw new InvalidOperationException("Cannot delete system role");

        await _unitOfWork.Roles.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds)
    {
        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
        if (role == null) throw new KeyNotFoundException("Role not found");

        role.RolePermissions.Clear();
        foreach (var permId in permissionIds)
        {
            role.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permId });
        }

        await _unitOfWork.Roles.UpdateAsync(role);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task InitializeDefaultRolesAsync()
    {
        var existing = await _unitOfWork.Roles.GetAllAsync();
        if (existing.Any()) return;

        var roles = new List<Role>
        {
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Administrator", Description = "System Administrator", IsSystem = true },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "ProjectManager", Description = "Project Manager", IsSystem = true },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Developer", Description = "Developer", IsSystem = true },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Programmer", Description = "Programmer", IsSystem = true },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "QA", Description = "Quality Assurance Tester", IsSystem = true }
        };

        foreach (var role in roles)
        {
            await _unitOfWork.Roles.AddAsync(role);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task InitializeDefaultAdminAsync()
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync("admin@projectflow.com");
        if (existingUser != null) return;

        var adminRole = await _unitOfWork.Roles.GetByNameAsync("Administrator");
        if (adminRole == null) return;

        var allPermissions = await _unitOfWork.Permissions.GetAllAsync();
        foreach (var perm in allPermissions)
        {
            adminRole.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
        }
        await _unitOfWork.SaveChangesAsync();

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@projectflow.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            FirstName = "Admin",
            LastName = "System",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(adminUser);
        adminUser.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });

        await _unitOfWork.SaveChangesAsync();
    }

    private static RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            Permissions = role.RolePermissions.Select(rp => new PermissionDto
            {
                Id = rp.PermissionId,
                Name = rp.Permission.Name,
                Module = rp.Permission.Module,
                Description = rp.Permission.Description
            }).ToList()
        };
    }
}

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;

    public PermissionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<PermissionDto>> GetAllAsync()
    {
        var permissions = await _unitOfWork.Permissions.GetAllAsync();
        return permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Name = p.Name,
            Module = p.Module,
            Description = p.Description
        });
    }

    public async Task<IEnumerable<PermissionDto>> GetByModuleAsync(string module)
    {
        return await _unitOfWork.Permissions.GetByModuleAsync(module)
            .ContinueWith(t => t.Result.Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Module = p.Module,
                Description = p.Description
            }));
    }

    public async Task InitializeDefaultPermissionsAsync()
    {
        var existing = await _unitOfWork.Permissions.GetAllAsync();
        if (existing.Any()) return;

        var permissions = new List<Permission>
        {
            new() { Id = Guid.NewGuid(), Name = "dashboard.view", Module = "Dashboard", Description = "View dashboard" },
            new() { Id = Guid.NewGuid(), Name = "calendar.view", Module = "Calendar", Description = "View calendar" },
            
            new() { Id = Guid.NewGuid(), Name = "users.create", Module = "Users", Description = "Create users" },
            new() { Id = Guid.NewGuid(), Name = "users.read", Module = "Users", Description = "Read users" },
            new() { Id = Guid.NewGuid(), Name = "users.update", Module = "Users", Description = "Update users" },
            new() { Id = Guid.NewGuid(), Name = "users.delete", Module = "Users", Description = "Delete users" },
            new() { Id = Guid.NewGuid(), Name = "users.assign_role", Module = "Users", Description = "Assign roles to users" },
            
            new() { Id = Guid.NewGuid(), Name = "projects.create", Module = "Projects", Description = "Create projects" },
            new() { Id = Guid.NewGuid(), Name = "projects.read", Module = "Projects", Description = "Read projects" },
            new() { Id = Guid.NewGuid(), Name = "projects.update", Module = "Projects", Description = "Update projects" },
            new() { Id = Guid.NewGuid(), Name = "projects.delete", Module = "Projects", Description = "Delete projects" },
            new() { Id = Guid.NewGuid(), Name = "projects.manage_members", Module = "Projects", Description = "Manage project members" },

            new() { Id = Guid.NewGuid(), Name = "tasks.create", Module = "Tasks", Description = "Create tasks" },
            new() { Id = Guid.NewGuid(), Name = "tasks.read", Module = "Tasks", Description = "Read tasks" },
            new() { Id = Guid.NewGuid(), Name = "tasks.update", Module = "Tasks", Description = "Update tasks" },
            new() { Id = Guid.NewGuid(), Name = "tasks.delete", Module = "Tasks", Description = "Delete tasks" },
            new() { Id = Guid.NewGuid(), Name = "tasks.assign", Module = "Tasks", Description = "Assign tasks" },
            new() { Id = Guid.NewGuid(), Name = "tasks.approve", Module = "Tasks", Description = "Approve tasks" },

            new() { Id = Guid.NewGuid(), Name = "timeentries.create", Module = "TimeEntries", Description = "Create time entries" },
            new() { Id = Guid.NewGuid(), Name = "timeentries.read", Module = "TimeEntries", Description = "Read time entries" },
            new() { Id = Guid.NewGuid(), Name = "timeentries.update", Module = "TimeEntries", Description = "Update time entries" },
            new() { Id = Guid.NewGuid(), Name = "timeentries.delete", Module = "TimeEntries", Description = "Delete time entries" },
            new() { Id = Guid.NewGuid(), Name = "timeentries.approve", Module = "TimeEntries", Description = "Approve time entries" },

            new() { Id = Guid.NewGuid(), Name = "reports.view", Module = "Reports", Description = "View reports" },
            new() { Id = Guid.NewGuid(), Name = "reports.export", Module = "Reports", Description = "Export reports" },

            new() { Id = Guid.NewGuid(), Name = "settings.manage", Module = "Settings", Description = "Manage settings" },
            new() { Id = Guid.NewGuid(), Name = "settings.workflows", Module = "Settings", Description = "Manage workflows" }
        };

        foreach (var perm in permissions)
        {
            await _unitOfWork.Permissions.AddAsync(perm);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null) return false;

        var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
        var roleIds = userRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToList();

        var rolePermissions = await _unitOfWork.RolePermissions.GetAllAsync();
        var permissionEntities = await _unitOfWork.Permissions.GetAllAsync();

        var permId = permissionEntities.FirstOrDefault(p => p.Name == permission)?.Id;
        if (permId == null) return false;

        return rolePermissions.Any(rp => roleIds.Contains(rp.RoleId) && rp.PermissionId == permId);
    }
}