namespace SmartDocHub.Domain.UserPermission;

public class Permission
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public PermissionType Type { get; set; }
    public string? Path { get; set; }
    public string? ApiMethod { get; set; }
    public string? Icon { get; set; }
    public PermissionStatus Status { get; set; }

}

public enum PermissionType
{
    /// <summary>
    /// 菜单
    /// </summary>
    Menu,

    /// <summary>
    /// 按钮/功能
    /// </summary>
    Button,

    /// <summary>
    /// API
    /// </summary>
    API
}

public enum PermissionStatus
{
    Forbidden = 0,

    Normal = 1
}