using CefSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptune.NsPay.CefTransferV2.Win.Helpers
{
    public class MyDownloadHandler : IDownloadHandler
    {
        public event EventHandler<DownloadItem> OnBeforeDownloadFired;

        public event EventHandler<DownloadItem> OnDownloadUpdatedFired;

        private int _downloadPercent;
        private bool _isDownloading;

        public event EventHandler<int> OnDownloading;
        public event EventHandler<bool> CanDownload;

        public int DownloadPercent { get => _downloadPercent; }

        public bool OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (downloadItem.IsValid)
            {
                Debug.WriteLine("== File information ========================");
                Debug.WriteLine(" File URL: {0}", downloadItem.Url);
                Debug.WriteLine(" Suggested FileName: {0}", downloadItem.SuggestedFileName);
                Debug.WriteLine(" MimeType: {0}", downloadItem.MimeType);
                Debug.WriteLine(" Content Disposition: {0}", downloadItem.ContentDisposition);
                Debug.WriteLine(" Total Size: {0}", downloadItem.TotalBytes);
                Debug.WriteLine("============================================");
            }

            OnBeforeDownloadFired?.Invoke(this, downloadItem);

            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Continue(
                        downloadItem.SuggestedFileName,
                        showDialog: true
                    );
                }
            }

            return true;
        }

        /// https://cefsharp.github.io/api/51.0.0/html/T_CefSharp_DownloadItem.htm
        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            OnDownloadUpdatedFired?.Invoke(this, downloadItem);

            if (downloadItem.IsValid)
            {
                // Show progress of the download
                if (downloadItem.IsInProgress && (downloadItem.PercentComplete != 0))
                {
                    Debug.WriteLine(
                        "Current Download Speed: {0} bytes ({1}%)",
                        downloadItem.CurrentSpeed,
                        downloadItem.PercentComplete
                    );

                    _downloadPercent = downloadItem.PercentComplete;
                    _isDownloading = true;
                }

                if (downloadItem.IsComplete)
                {
                    Debug.WriteLine("The download has been finished !");

                    _downloadPercent = 100;
                    _isDownloading = false;
                }
            }

            OnDownloading?.Invoke(this, _downloadPercent);
        }

        bool IDownloadHandler.CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
        {
            var canDownload = !_isDownloading;
            CanDownload?.Invoke(this, canDownload);
            return canDownload;
        }
    }
}