using SmartDocHub.Service.Exceptions;

namespace SmartDocHub.Web.Reponse
{
    public class ApiResult<T>
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public T Data { get; set; }

        public ApiResult(ResponseCode code, string msg, T data = default)
        {
            Code = (int)code;
            Msg = msg;
            Data = data;
        }
    }
}
