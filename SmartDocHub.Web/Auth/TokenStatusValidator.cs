using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using SmartDocHub.Domain.UserPermission;
using SmartDocHub.Infrastructure;
using SmartDocHub.Service.Common;

using System.Security.Claims;

namespace SmartDocHub.Web.Auth;

/// <summary>
/// JWT 自定义声明名。
/// </summary>
public static class JwtClaimNames
{
    /// <summary>令牌类型：access / refresh</summary>
    public const string TokenType = "token_type";

    /// <summary>签发时用户的安全戳，用于令牌失效判定</summary>
    public const string SecurityStamp = "sstamp";

    public const string AccessToken = "access";
    public const string RefreshToken = "refresh";
}

/// <summary>
/// 令牌有效性校验：令牌类型、用户是否仍可用、SecurityStamp 是否已变更。
/// </summary>
public interface ITokenStatusValidator
{
    /// <summary>
    /// 校验当前令牌是否仍然有效。
    /// </summary>
    Task<bool> IsValidAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清除指定用户的令牌状态缓存，使状态变更立即生效。
    /// </summary>
    void Invalidate(long userId);
}

/// <summary>
/// 默认实现：基于 30 秒短缓存读取用户状态，避免每个请求都查库。
/// </summary>
public class TokenStatusValidator(IMemoryCache cache, SmartDocHubDbContext dbContext) : ITokenStatusValidator
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public async Task<bool> IsValidAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default)
    {
        if (!long.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return false;
        }

        var state = await cache.GetOrCreateAsync(CacheKeys.UserTokenState(userId), async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var snapshot = await dbContext.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new { u.SecurityStamp, u.Status, u.IsDeleted })
                .FirstOrDefaultAsync(cancellationToken);

            return snapshot is null
                ? UserTokenState.Invalid
                : new UserTokenState(snapshot.SecurityStamp, !snapshot.IsDeleted && snapshot.Status == UserStatus.Normal);
        });

        if (state is null || !state.IsActive)
        {
            return false;
        }

        var tokenStamp = principal.FindFirstValue(JwtClaimNames.SecurityStamp) ?? string.Empty;
        return string.Equals(tokenStamp, state.SecurityStamp ?? string.Empty, StringComparison.Ordinal);
    }

    public void Invalidate(long userId) => cache.Remove(CacheKeys.UserTokenState(userId));

    private sealed record UserTokenState(string? SecurityStamp, bool IsActive)
    {
        public static readonly UserTokenState Invalid = new(null, false);
    }
}
