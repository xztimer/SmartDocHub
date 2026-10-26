using System.Globalization;

namespace SmartDocHub.Service.Common;

/// <summary>
/// 时间统一口径：数据库与 API 一律 UTC，展示层负责转本地。
/// </summary>
public static class DateTimeUtc
{
    /// <summary>
    /// 归一化为 UTC。Kind 为 Unspecified 时按 UTC 解释——
    /// 数据库 timestamptz 读出的值语义上就是 UTC，若走 ToUniversalTime 会被当成本地时间再减 8 小时。
    /// </summary>
    public static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    /// <summary>输出带 Z 的 ISO8601，前端可直接 new Date(...) 解析</summary>
    public static string ToIsoString(DateTime value)
        => AsUtc(value).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
}
