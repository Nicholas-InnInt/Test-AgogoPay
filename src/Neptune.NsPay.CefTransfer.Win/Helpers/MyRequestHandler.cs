using CefSharp.Handler;
using CefSharp;
using Neptune.NsPay.CefTransfer.Win.Classes;

namespace Neptune.NsPay.CefTransfer.Win.Helpers
{
    public class MyRequestHandler : RequestHandler
    {
        private int _nrOfCalls;
        private MyBrowser _browser;

        public int NrOfCalls { get => _nrOfCalls; }

        public MyRequestHandler(MyBrowser browser)
        {
            _browser = browser;
        }

        protected override IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            return new MyResourceRequestHandler(_browser);
        }

        protected override bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            _nrOfCalls++;
            //_browser.LogInfo($"OnBeforeBrowse: {_nrOfCalls}, {request.Url}");
            return false;
        }

        //protected override bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        //{
        //    if (isProxy == true)
        //    {
        //        if (_browser.ProxyEnable)
        //            callback.Continue(_browser.ProxyUser, _browser.ProxyPassword);
        //    }
        //    return true;
        //}
    }
}
