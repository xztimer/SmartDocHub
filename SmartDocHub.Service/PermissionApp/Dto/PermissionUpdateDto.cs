using System.ComponentModel.DataAnnotations;

using SmartDocHub.Domain.UserPermission;

namespace SmartDocHub.Service.PermissionApp.Dto;

/// <summary>
/// 更新权限。
/// 刻意不包含 Code：权限码是 [HasPermission("...")] 与角色缓存的契约，
/// 允许随意修改会导致代码与数据不一致、缓存里的旧码继续放行。
/// 需要改码请删除后重建。
/// </summary>
public class PermissionUpdateDto
{
    public long? ParentId { get; set; }

    [Required(ErrorMessage = "权限名称不能为空")]
    [MaxLength(100, ErrorMessage = "权限名称不能超过 100 个字符")]
    public string Name { get; set; } = string.Empty;

    public PermissionType Type { get; set; }

    [MaxLength(200)]
    public string? Path { get; set; }

    [MaxLength(200)]
    public string? ApiMethod { get; set; }

    [MaxLength(100)]
    public string? Icon { get; set; }

    /// <summary>不传表示保持原状态</summary>
    public PermissionStatus? Status { get; set; }
}
