using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocHub.Domain.UserPermission;

public class Permission
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public PermissionType Type { get; set; }
    public string? Path { get; set; }
    public string ApiMethod { get; set; }
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
