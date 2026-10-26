using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using SmartDocHub.Web.Auth;

using System.Security.Claims;
using System.Text;

namespace SmartDocHub.Web.Extensions;

public static class JwtExtension
{
    public static void AddJwt(this WebApplicationBuilder builder)
    {
        var jwtBearer = builder.Configuration.GetSection("Authentication").GetSection("JwtBearer");
        builder.Services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,//是否验证Issuer
                ValidIssuer = jwtBearer.GetValue<string>("Issuer"),

                ValidateAudience = true,//是否验证Audience
                ValidAudience = jwtBearer.GetValue<string>("Audience"),

                ValidateIssuerSigningKey = true,//是否验证SecurityKey
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtBearer.GetValue<string>("SecurityKey")!)),

                ValidateLifetime = true,//是否验证失效时间
                ClockSkew = TimeSpan.FromSeconds(5)
            };

            opt.Events = new JwtBearerEvents
            {
                // 签名校验通过后，继续校验令牌类型、账号状态与安全戳
                OnTokenValidated = async context =>
                {
                    var tokenType = context.Principal?.FindFirstValue(JwtClaimNames.TokenType);
                    if (!string.Equals(tokenType, JwtClaimNames.AccessToken, StringComparison.Ordinal))
                    {
                        // 刷新令牌不得作为访问令牌使用
                        context.Fail("令牌类型不正确");
                        return;
                    }

                    var validator = context.HttpContext.RequestServices
                        .GetRequiredService<ITokenStatusValidator>();

                    if (!await validator.IsValidAsync(context.Principal!, context.HttpContext.RequestAborted))
                    {
                        context.Fail("令牌已失效，请重新登录");
                    }
                },
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsJsonAsync(new { code = 401, message = "未登录或登录已过期" });
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsJsonAsync(new { code = 403, message = "没有访问该资源的权限" });
                }
            };
        });
    }
}
