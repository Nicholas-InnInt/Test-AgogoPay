namespace Neptune.NsPay.Web.Api.Models
{
    public class ApiResult
    {
        public ApiResult()
        {
            StatusCode = 0;
        }

        /// <summary>
        /// 请求状态
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回时间戳
        /// </summary>
        public string Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

    }

    public class ApiResultNew
    {
        public ApiResultNew()
        {
            StatusCode = 0;
        }

        /// <summary>
        /// 请求状态
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// 返回信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 错误码
        /// </summary>
        public int? ErrorCode { get; set; }


        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 返回时间戳
        /// </summary>
        public string Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();

    }

    public class ApiResult<T> : ApiResult
    {
        /// <summary>
        /// 接口返回值
        /// </summary>
        public T Data { get; set; }

    }
}
