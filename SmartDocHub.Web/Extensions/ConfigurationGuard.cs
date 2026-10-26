namespace SmartDocHub.Web.Extensions;

/// <summary>
/// 启动期配置安全检查。
/// 开发环境放行，非开发环境发现弱密钥/默认口令直接拒绝启动，避免把仓库里的示例配置带到线上。
/// </summary>
public static class ConfigurationGuard
{
    private static readonly string[] KnownWeakSecurityKeys =
    [
        "nasidpvuhqouwenfoqnfqononpnvqwoejpfgmpajowe"
    ];

    private static readonly string[] KnownWeakPasswords =
    [
        "xtimer1234"
    ];

    public static void EnsureProductionSecrets(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            return;
        }

        var securityKey = builder.Configuration["Authentication:JwtBearer:SecurityKey"];
        if (string.IsNullOrWhiteSpace(securityKey) || securityKey.Length < 32)
        {
            throw new InvalidOperationException(
                "缺少 JWT 签名密钥。请通过环境变量 Authentication__JwtBearer__SecurityKey 配置至少 32 个字符的独立密钥。");
        }

        if (KnownWeakSecurityKeys.Contains(securityKey, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "检测到仓库示例中的 JWT 签名密钥，该密钥已泄漏。请立即轮换后通过环境变量注入。");
        }

        var connectionString = builder.Configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "缺少数据库连接串。请通过环境变量 ConnectionStrings__Default 配置。");
        }

        if (KnownWeakPasswords.Any(p => connectionString.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "数据库连接串仍在使用开发环境默认口令。请轮换数据库密码后通过环境变量注入。");
        }
    }
}
