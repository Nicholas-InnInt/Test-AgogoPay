namespace Neptune.NsPay.Web.PayMonitorApi.Models
{
    public class BaseReponse
    {
        public StatusCodeEnum Code { get; set; }
        public string Message { get; set; }
    }

    public enum StatusCodeEnum
    {
        OK = 200,
        ERROR = 400
    }
    public class ApiResult<T> : BaseReponse
    {
        /// <summary>
        /// 接口返回值
        /// </summary>
        public T Data { get; set; }

    }
}
