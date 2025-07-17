using Neptune.NsPay.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.PlatformWithdraw.Helper
{
    public class HttpHelper
    {
        public static string ReqUrl(string reqUrl, string method, string paramData, string token)
        {
            try
            {
                var host = AppSettings.Configuration["PlatfromHost"];
                HttpWebRequest request = WebRequest.Create(reqUrl) as HttpWebRequest;
                request.Method = method.ToUpperInvariant();

                if (!string.IsNullOrEmpty(token) && token.Length > 1)
                {
                    //token = token.Replace("\"", "");
                    request.Headers.Add("authorization", "Bearer " + token);
                }
                request.Headers.Add("x-requested-with", "XMLHttpRequest");
                request.Headers.Add("origin", "http://" + host + "");
                request.Headers.Add("referer", "http://" + host + "/");
                request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36");
                if (request.Method.ToString() != "GET" && !string.IsNullOrEmpty(paramData) && paramData.Length > 0)
                {
                    request.ContentType = "application/json";
                    request.Accept = "application/json";
                    byte[] buffer = Encoding.UTF8.GetBytes(paramData);
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }

                using (HttpWebResponse resp = request.GetResponse() as HttpWebResponse)
                {
                    using (StreamReader stream = new StreamReader(resp.GetResponseStream(), Encoding.UTF8))
                    {
                        string result = stream.ReadToEnd();
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
