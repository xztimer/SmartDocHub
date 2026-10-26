namespace SmartDocHub.Service.Common;

/// <summary>
/// 服务层操作结果。
/// 控制器据此映射 HTTP 状态码，避免用错误消息字符串做流程判断。
/// </summary>
public readonly record struct ServiceResult(bool IsSuccess, string Message, ServiceResultStatus Status)
{
    public static ServiceResult Ok() => new(true, string.Empty, ServiceResultStatus.Success);

    public static ServiceResult Fail(string message, ServiceResultStatus status = ServiceResultStatus.Invalid)
        => new(false, message, status);

    public static ServiceResult NotFound(string message) => new(false, message, ServiceResultStatus.NotFound);

    public static ServiceResult Conflict(string message) => new(false, message, ServiceResultStatus.Conflict);
}

/// <summary>
/// 带返回值的服务层操作结果，用于新增类操作回传主键。
/// </summary>
public readonly record struct ServiceResult<T>(bool IsSuccess, string Message, ServiceResultStatus Status, T? Value)
{
    public static ServiceResult<T> Ok(T value) => new(true, string.Empty, ServiceResultStatus.Success, value);

    public static ServiceResult<T> Fail(string message, ServiceResultStatus status = ServiceResultStatus.Invalid)
        => new(false, message, status, default);

    public static ServiceResult<T> NotFound(string message) => new(false, message, ServiceResultStatus.NotFound, default);

    public static ServiceResult<T> Conflict(string message) => new(false, message, ServiceResultStatus.Conflict, default);
}

public enum ServiceResultStatus
{
    Success = 0,
    Invalid = 1,
    NotFound = 2,
    Conflict = 3
}
