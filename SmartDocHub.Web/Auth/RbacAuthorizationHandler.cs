using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Hybrid;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.Common;
using SmartDocHub.Service.PermissionApp;

using System.Security.Claims;

namespace SmartDocHub.Web.Auth;

public class RbacAuthorizationHandler(
    HybridCache cache,
    IRolePermissionService rolePermissionService) : AuthorizationHandler<RbacRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RbacRequirement requirement)
    {
        var requiredCode = requirement.Code;
        if (string.IsNullOrEmpty(requiredCode)) return;
        var userRoles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        if (!userRoles.Any()) return;

        if (userRoles.Contains(SystemRoles.SuperAdmin))
        {
            context.Succeed(requirement);
        }

        var permissionTasks = userRoles.Select(async roleName =>
        {
            return await cache.GetOrCreateAsync(
                CacheKeys.RolePermissionCodes(roleName),
                async token =>
                {
                    var perms = await rolePermissionService.GetRolePermissionsAsync(roleName);
                    return perms
                        .Where(t => !string.IsNullOrEmpty(t.Code))
                        .Select(t => t.Code)
                        .ToList();
                },
                options: new HybridCacheEntryOptions
                {
                    LocalCacheExpiration = TimeSpan.FromMinutes(20) // L1 内存缓存过期时间
                }
            );
        });

        var rolePermissionsResults = await Task.WhenAll(permissionTasks);
        var userOwnedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var permissions in rolePermissionsResults)
        {
            if (permissions != null)
            {
                foreach (var perm in permissions)
                {
                    userOwnedCodes.Add(perm);
                }
            }
        }

        if (userOwnedCodes.Contains(requiredCode))
        {
            context.Succeed(requirement);
        }

    }
}
