using CefSharp;
using Neptune.NsPay.CefTransferV2.Win.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.CefTransferV2.Win.Helpers
{
    public class MyResourceRequestHandler : MyIResourceRequestHandler
    {
        public MyBrowser MyBrowser { get; set; }    

        public MyResourceRequestHandler(MyBrowser browser)
        {
            MyBrowser = browser;
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
                MyBrowser.NumOfSuccess++;
                //_browser.LogInfo($"Success {_browser.NumOfSuccess}, {request.Url}");
            }
            else
            {
                MyBrowser.NumOfFailure++;
                //_browser.LogInfo($"Failed {_browser.NumOfFailure}, {request.Url}");
            }
            MyBrowser.OnResourceLoadComplete(request);
        }

        protected override void Dispose()
        {
            MyFilterManager.Dispose();
            base.Dispose();
        }
    }
}
