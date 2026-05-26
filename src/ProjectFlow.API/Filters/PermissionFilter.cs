using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjectFlow.Application.Interfaces;

namespace ProjectFlow.API.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class PermissionAttribute : Attribute
{
    public string Permission { get; }

    public PermissionAttribute(string permission)
    {
        Permission = permission;
    }
}

public class PermissionFilter : IAsyncAuthorizationFilter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly string _permission;

    public PermissionFilter(IServiceProvider serviceProvider, string permission)
    {
        _serviceProvider = serviceProvider;
        _permission = permission;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userIdClaim = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        
        var hasPermission = await permissionService.HasPermissionAsync(userId, _permission);
        
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}

public static class PermissionFilterExtensions
{
    public static void AddPermissionFilter(this MvcOptions options, IServiceProvider serviceProvider)
    {
        options.Filters.AddService<PermissionFilter>();
    }
}
