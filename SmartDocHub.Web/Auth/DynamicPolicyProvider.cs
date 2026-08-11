using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SmartDocHub.Web.Auth;

/// <summary>
/// 动态策略供应器
/// </summary>
public class DynamicPolicyProvider : DefaultAuthorizationPolicyProvider
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options"></param>
    public DynamicPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    /// <summary>
    /// 重写GetPolicyAsync方法
    /// </summary>
    /// <param name="policyName"></param>
    /// <returns></returns>
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);
        if (policy != null) return policy;
        var newPolicy = new AuthorizationPolicyBuilder();
        newPolicy.RequireAuthenticatedUser();
        newPolicy.AddRequirements(new RbacRequirement(policyName));
        return newPolicy.Build();
    }
}
