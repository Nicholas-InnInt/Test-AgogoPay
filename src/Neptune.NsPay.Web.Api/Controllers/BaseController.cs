using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Web.Api.Models;

namespace Neptune.NsPay.Web.Api.Controllers
{
    public class BaseController : Controller
    {
        #region 统一返回封装

        /// <summary>
        /// 返回封装
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static JsonResult toResponse(StatusCodeType statusCode)
        {
            ApiResult response = new ApiResult();
            response.StatusCode = (int)statusCode;
            response.Message = statusCode.GetEnumText();

            return new JsonResult(response);
        }

        /// <summary>
        /// 返回封装
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="retMessage"></param>
        /// <returns></returns>
        public static JsonResult toResponse(StatusCodeType statusCode, string retMessage)
        {
            ApiResult response = new ApiResult();
            response.StatusCode = (int)statusCode;
            response.Message = retMessage;

            return new JsonResult(response);
        }

        /// <summary>
        /// 返回封装
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="retMessage"></param>
        /// <returns></returns>
        public static JsonResult toResponseWithErrorCode(StatusCodeType statusCode , string retMessage, ProcessErrorCodeType? errorCode = null)
        {
            ApiResultNew response = new ApiResultNew();
            response.StatusCode = (int)statusCode;
            response.Message = retMessage;
            response.ErrorCode = errorCode.HasValue? (int)errorCode.Value : null;
            response.ErrorMessage = errorCode.HasValue ? errorCode.Value.GetEnumText() : null;
            return new JsonResult(response);
        }

        public static JsonResult toResponseError(StatusCodeType statusCode, string retMessage)
        {
            ApiResult<PayApiResult> response = new ApiResult<PayApiResult>();
            response.StatusCode = (int)statusCode;
            response.Message = retMessage;
            response.Data = null;

            return new JsonResult(response);
        }

        /// <summary>
        /// 返回封装
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static JsonResult toResponse<T>(T data)
        {
            ApiResult<T> response = new ApiResult<T>();
            response.StatusCode = (int)StatusCodeType.Success;
            response.Message = StatusCodeType.Success.GetEnumText();
            response.Data = data;
            return new JsonResult(response);
        }

        #endregion
    }
}
