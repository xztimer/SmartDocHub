using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Service.PermissionApp;

using System.Security.Claims;

namespace SmartDocHub.Web.Auth;

public class RbacAuthorizationHandler(
    IMemoryCache cache,
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

        var userOwnedCodes = new HashSet<string>();

        foreach (var roleName in userRoles)
        {
            var rolePermissionCode = await cache.GetOrCreateAsync($"ROLE_CODES_{roleName}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                var perms = await rolePermissionService.GetRolePermissionsAsync(roleName);
                return perms
                .Where(t => !string.IsNullOrEmpty(t.Code))
                .Select(t => t.Code)
                .ToList();
            });
            if(rolePermissionCode != null)
            {
                foreach(var role in rolePermissionCode)
                {
                    userOwnedCodes.Add(role);
                }
            }
        }
        if (userRoles.Contains("管理员") || userOwnedCodes.Contains(requiredCode))
        {
            context.Succeed(requirement);
        }

    }
}
