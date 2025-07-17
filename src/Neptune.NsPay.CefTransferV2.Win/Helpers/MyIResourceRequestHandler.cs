using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.CefTransferV2.Win.Helpers
{
    public class MyIResourceRequestHandler : IResourceRequestHandler
    {
        ICookieAccessFilter IResourceRequestHandler.GetCookieAccessFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return GetCookieAccessFilter(chromiumWebBrowser, browser, frame, request);
        }

        protected virtual ICookieAccessFilter GetCookieAccessFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return null;
        }

        IResourceHandler IResourceRequestHandler.GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return GetResourceHandler(chromiumWebBrowser, browser, frame, request);
        }

        protected virtual IResourceHandler GetResourceHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return null;
        }

        IResponseFilter IResourceRequestHandler.GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return GetResourceResponseFilter(chromiumWebBrowser, browser, frame, request, response);
        }

        protected virtual IResponseFilter GetResourceResponseFilter(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return null;
        }

        CefReturnValue IResourceRequestHandler.OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return OnBeforeResourceLoad(chromiumWebBrowser, browser, frame, request, callback);
        }

        protected virtual CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            return CefReturnValue.Continue;
        }

        bool IResourceRequestHandler.OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return OnProtocolExecution(chromiumWebBrowser, browser, frame, request);
        }

        protected virtual bool OnProtocolExecution(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request)
        {
            return false;
        }

        void IResourceRequestHandler.OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {
            OnResourceLoadComplete(chromiumWebBrowser, browser, frame, request, response, status, receivedContentLength);
        }

        protected virtual void OnResourceLoadComplete(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {

        }

        void IResourceRequestHandler.OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {
            OnResourceRedirect(chromiumWebBrowser, browser, frame, request, response, ref newUrl);
        }

        protected virtual void OnResourceRedirect(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response, ref string newUrl)
        {

        }

        bool IResourceRequestHandler.OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return OnResourceResponse(chromiumWebBrowser, browser, frame, request, response);
        }

        protected virtual bool OnResourceResponse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            return false;
        }

        protected virtual void Dispose()
        {

        }

        void IDisposable.Dispose()
        {
            Dispose();
        }
    }
}