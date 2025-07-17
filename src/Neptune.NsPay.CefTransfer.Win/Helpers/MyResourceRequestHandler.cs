using CefSharp;
using Neptune.NsPay.CefTransfer.Win.Classes;

namespace Neptune.NsPay.CefTransfer.Win.Helpers
{
    public class MyResourceRequestHandler : MyIResourceRequestHandler
    {
        private MyBrowser _browser;
        public MyResourceRequestHandler(MyBrowser browser)
        {
            _browser = browser;
        }

        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Continue;
        }

        protected override IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            var filter = MyFilterManager.CreateFilter(request.Identifier.ToString());
            return filter;
        }

        protected override void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame,
            IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            if (status == UrlRequestStatus.Success)
            {
                _browser.NumOfSuccess++;
                //_browser.LogInfo($"Success {_browser.NumOfSuccess}, {request.Url}");
            }
            else
            {
                _browser.NumOfFailure++;
                //_browser.LogInfo($"Failed {_browser.NumOfFailure}, {request.Url}");
            }
            //Debug.WriteLine($"{request.Url},{response.StatusCode}, {response.ErrorCode}, {status}");
            _browser.browser_OnResourceLoadComplete(request);
        }

        protected override void Dispose()
        {
            MyFilterManager.Dispose();
            base.Dispose();
        }
    }
}
