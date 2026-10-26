using System.ComponentModel.DataAnnotations;

using SmartDocHub.Domain.Doc;

namespace SmartDocHub.Service.CategoryApp.Dto;

/// <summary>
/// 新建分类。
/// ParentId 为空或 0 表示根分类——原实现强制校验父级存在，导致根分类无法创建。
/// </summary>
public class CategoryCreateDto
{
    [Required(ErrorMessage = "分类名称不能为空")]
    [MaxLength(100, ErrorMessage = "分类名称不能超过 100 个字符")]
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>父级分类 Id，null 或 0 表示根分类</summary>
    public long? ParentId { get; set; }

    /// <summary>分类状态，不传默认启用（枚举默认值 0 是禁用，不能直接依赖默认值）</summary>
    public CategoryStatus? CategoryStatus { get; set; }
}
