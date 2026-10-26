namespace SmartDocHub.Web.AuditLog
{
    public class AuditLogAttribute : Attribute
    {
        public bool IsOpen { get; set; } = true;
    }
}
