using CefSharp.Handler;
using CefSharp;
using Neptune.NsPay.CefTransferV2.Win.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.CefTransferV2.Win.Helpers
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
            //_browser.ReqTable.AddRow(request.Url, DateTime.Now);
            _nrOfCalls++;
            return false;
        }
    }
}
