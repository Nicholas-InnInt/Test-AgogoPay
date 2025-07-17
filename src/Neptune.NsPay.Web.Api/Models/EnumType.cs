using System.Reflection;

namespace Neptune.NsPay.Web.Api.Models
{
    /// <summary>
    /// 枚举扩展属性
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// 获得枚举提示文本
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetEnumText(this Enum obj)
        {
            Type type = obj.GetType();
            FieldInfo field = type.GetField(obj.ToString());
            TextAttribute attribute = (TextAttribute)field.GetCustomAttribute(typeof(TextAttribute));
            return attribute.Value;
        }
    }

    public class TextAttribute : Attribute
    {
        public TextAttribute(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }

    public enum ProcessErrorCodeType
    {
        [Text("订单失败")]
        OrderFailed = 101,

        [Text("订单超时")]
        OrderExpired = 102,

        [Text("订单已完成")]
        OrderCompleted = 103,

        [Text("等待结果")]
        WaitingOrderResult = 104,


        [Text("金额不匹配")]
        OrderAmountNotMatch = 105,

        [Text("设备错误")]
        DeviceError = 200,


        [Text("参数错误")]
        ParamError = 500,


        [Text("未知错误")]
        UnknownError = 999,

    }

    public enum StatusCodeType
    {
        [Text("请求成功")]
        Success = 200,

        [Text("内部请求出错")]
        Error = 500,

        [Text("请求失败")]
        Faild = 600,

        [Text("访问请求未授权! 当前 SESSION 失效, 请重新登陆")]
        Unauthorized = 401,

        [Text("请求参数不完整或不正确")]
        ParameterError = 4444,

        [Text("您无权进行此操作，请求执行已拒绝")]
        Forbidden = 403,

        [Text("找不到与请求匹配的 HTTP 资源")]
        NotFound = 404,

        [Text("HTTP请求类型不合法")]
        HttpMehtodError = 405,

        [Text("HTTP请求不合法,请求参数可能被篡改")]
        HttpRequestError = 406,

        [Text("该URL已经失效")]
        URLExpireError = 407,
    }
}
