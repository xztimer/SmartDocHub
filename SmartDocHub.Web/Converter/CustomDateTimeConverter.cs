using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

using SmartDocHub.Service.Common;

namespace SmartDocHub.Web.Converter
{
    /// <summary>
    /// 日期转换器。
    /// 输出：一律 UTC 的 ISO8601（带 Z），前端用 new Date(...) 即可正确本地化。
    /// 输入：兼容带偏移的 ISO8601；无偏移的旧格式 "yyyy-MM-dd HH:mm:ss" 按 UTC 解释，
    ///       与旧版接口输出保持同一口径，避免升级后时间整体偏移。
    /// </summary>
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private const string LegacyFormat = "yyyy-MM-dd HH:mm:ss";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var text = reader.GetString()?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                throw new JsonException("时间不能为空");
            }

            if (HasOffset(text))
            {
                if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var offsetValue))
                {
                    return offsetValue.UtcDateTime;
                }
            }
            else if (DateTime.TryParseExact(text, LegacyFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var legacy))
            {
                return DateTime.SpecifyKind(legacy, DateTimeKind.Utc);
            }
            else if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var assumed))
            {
                // 例如 "2026-10-26T00:08:24"：无偏移即按 UTC
                return assumed.UtcDateTime;
            }

            throw new JsonException($"无法解析的时间格式：{text}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(DateTimeUtc.ToIsoString(value));

        /// <summary>判断字符串是否携带时区偏移（Z 或 ±HH:mm）</summary>
        private static bool HasOffset(string text)
        {
            if (text.EndsWith('Z') || text.EndsWith('z')) return true;

            // 日期部分自身含 '-'，偏移只可能出现在时间部分之后
            var timeSeparatorIndex = text.IndexOf('T');
            if (timeSeparatorIndex < 0) return false;

            var offsetIndex = text.IndexOfAny(['+', '-'], timeSeparatorIndex);
            return offsetIndex > 0;
        }
    }
}
