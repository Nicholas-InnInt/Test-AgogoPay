using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.AccountChecker
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log the Request
            await LogRequestAsync(context.Request);

            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;

            using (var newBodyStream = new MemoryStream())
            {
                context.Response.Body = newBodyStream;

                // Proceed with the request
                await _next(context);

                // Log the Response
                await LogResponseAsync(context.Response);

                // Copy the response to the original stream
                await newBodyStream.CopyToAsync(originalBodyStream);
            }
        }

        private async Task LogRequestAsync(HttpRequest request)
        {
            // Log Request Method and URL
            _logger.LogInformation($"Request: {request.Method} {request.Scheme}://{request.Host}{request.Path}{request.QueryString}");

            // Log Request Body if possible
            if (request.ContentLength > 0)
            {
                request.EnableBuffering(); // Allow reading the request stream
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    var body = await reader.ReadToEndAsync();
                    _logger.LogInformation($"Request Body: {body}");
                    request.Body.Seek(0, SeekOrigin.Begin); // Reset the stream position for further processing
                }
            }

            // Log Request Headers
            foreach (var header in request.Headers)
            {
                _logger.LogInformation($"Request Header: {header.Key}: {header.Value}");
            }
        }

        private async Task LogResponseAsync(HttpResponse response)
        {
            // Log Response Status Code
            _logger.LogInformation($"Response Status: {response.StatusCode}");

            // Log Response Body if possible
            if (response.Body.CanRead)
            {
                response.Body.Seek(0, SeekOrigin.Begin); // Move to the beginning of the response body
                using (var reader = new StreamReader(response.Body))
                {
                    var body = await reader.ReadToEndAsync();
                    _logger.LogInformation($"Response Body: {body}");
                    response.Body.Seek(0, SeekOrigin.Begin); // Reset the stream position
                }
            }
        }
    }

}



