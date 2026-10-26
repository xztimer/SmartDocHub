using SmartDocHub.Domain.Doc;

namespace SmartDocHub.Service.CategoryApp.Dto;

/// <summary>
/// 分类输出模型。字段与实体保持一致，避免前端适配。
/// </summary>
public class CategoryDto
{
    public long Id { get; set; }
    public long ParentId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public CategoryStatus CategoryStatus { get; set; }
    public bool IsDeleted { get; set; }
}
