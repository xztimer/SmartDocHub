using System.ComponentModel.DataAnnotations;

using SmartDocHub.Domain.Doc;

namespace SmartDocHub.Service.CategoryApp.Dto;

/// <summary>
/// 更新分类。ParentId 为空或 0 表示移动到根层级。
/// </summary>
public class CategoryUpdateDto
{
    [Required(ErrorMessage = "分类名称不能为空")]
    [MaxLength(100, ErrorMessage = "分类名称不能超过 100 个字符")]
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>父级分类 Id，null 或 0 表示根分类</summary>
    public long? ParentId { get; set; }

    /// <summary>分类状态，不传表示保持原状态</summary>
    public CategoryStatus? CategoryStatus { get; set; }
}
