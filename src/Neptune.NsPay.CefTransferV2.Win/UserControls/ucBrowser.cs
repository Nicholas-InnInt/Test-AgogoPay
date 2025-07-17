using Neptune.NsPay.CefTransferV2.Win.Classes;

namespace Neptune.NsPay.CefTransferV2.Win
{
    public partial class ucBrowser : UserControl
    {
        public MyBrowser Browser { get; set; }
        public ucBrowser()
        {
            InitializeComponent();
        }

        public void LaunchBrowser(MyBrowser browser)
        {
            Browser = browser;
            Browser.DoSetUrl += tstxtUrl_SetUrl;

            panel1.Controls.Clear();
            panel1.Controls.Add(browser.CefBrowser);
        }

        private void toolStrip1_Layout_1(object sender, LayoutEventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => SetToolStripLayout());
        }

        private void tstxtUrl_SetUrl(object? sender, string url)
        {
            this.InvokeOnUiThreadIfRequired(() => SetUrl(url));
        }

        private void SetToolStripLayout()
        {
            tstxtUrl.Width = toolStrip1.Width - 10;

            //var width = toolStrip1.Width;
            //foreach (ToolStripItem item in toolStrip1.Items)
            //{
            //    if (item != tstxtUrl)
            //    {
            //        width -= item.Width - item.Margin.Horizontal;
            //    }
            //}
            //tstxtUrl.Width = Math.Max(100, width - tstxtUrl.Margin.Horizontal);
        }

        private void SetUrl(string url)
        {
            tstxtUrl.Text = url;
        }
    }
}
