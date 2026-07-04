namespace SmartDocHub.Service.Exceptions;

public class BusinessException : Exception
{
    public ResponseCode Code { get; private set; }

    public BusinessException(string message, ResponseCode code = ResponseCode.BusinessError)
        : base(message)
    {
        Code = code;
    }
}