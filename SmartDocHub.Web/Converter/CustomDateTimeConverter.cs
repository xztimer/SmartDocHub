using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartDocHub.Web.Converter
{
    /// <summary>
    /// 日期转换器
    /// </summary>
    /// <param name="format"></param>
    public class CustomDateTimeConverter(string format = "yyyy-MM-dd HH:mm:ss") : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(format));
        }
    }
}
