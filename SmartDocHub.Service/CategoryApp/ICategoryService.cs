using SmartDocHub.Service.CategoryApp.Dto;
using SmartDocHub.Service.Common;

namespace SmartDocHub.Service.CategoryApp;

public interface ICategoryService
{
    /// <summary>分类列表，默认排除已删除</summary>
    /// <param name="includeDisabled">是否包含已禁用分类（管理页需要，文档选择器不需要）</param>
    Task<List<CategoryDto>> GetListAsync(bool includeDisabled = true);

    Task<ServiceResult<long>> CreateAsync(CategoryCreateDto dto);

    Task<ServiceResult> UpdateAsync(long id, CategoryUpdateDto dto);

    /// <summary>软删除。物理删除会让历史文档的 CategoryId 变成悬空引用。</summary>
    Task<ServiceResult> DeleteAsync(long id);
}
