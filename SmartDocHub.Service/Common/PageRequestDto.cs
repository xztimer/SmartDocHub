using System.ComponentModel.DataAnnotations;

namespace SmartDocHub.Service.Common;

/// <summary>
/// 分页请求基类。
/// 页码与每页条数必须受约束：PageIndex 小于 1 会产生负数 Skip 直接 500，
/// PageSize 不设上限则等价于开放全表导出，是廉价的资源耗尽入口。
/// </summary>
public class PageRequestDto
{
    public const int MaxPageSize = 200;

    /// <summary>页码，从 1 开始</summary>
    [Range(1, int.MaxValue, ErrorMessage = "页码必须大于 0")]
    public int PageIndex { get; set; } = 1;

    /// <summary>每页条数，上限 200</summary>
    [Range(1, MaxPageSize, ErrorMessage = "每页条数必须在 1-200 之间")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 计算跳过条数。控制器已由模型校验兜底，这里再钳制一次，
    /// 防止服务层被其他入口（定时任务、内部调用）以非法值调用。
    /// </summary>
    public int GetSkipCount() => (Math.Max(1, PageIndex) - 1) * GetPageSize();

    /// <summary>取钳制后的每页条数</summary>
    public int GetPageSize() => Math.Clamp(PageSize, 1, MaxPageSize);
}
