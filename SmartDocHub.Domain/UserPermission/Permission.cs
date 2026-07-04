using System;
using System.Collections.Generic;
using System.Text;

namespace SmartDocHub.Domain.UserPermission;

public class Permission
{
    public long Id { get; set; }
    public string Name { get; set; }
    public PermissionType Type { get; set; }
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
    /// API接口
    /// </summary>
    API
}
