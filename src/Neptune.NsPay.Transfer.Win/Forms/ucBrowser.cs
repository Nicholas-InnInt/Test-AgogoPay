using Neptune.NsPay.Transfer.Win.Classes;

namespace Neptune.NsPay.Transfer.Win.Forms
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
            panel1.Controls.Add(browser.BrowserView);
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
        }

        private void SetUrl(string url)
        {
            tstxtUrl.Text = url;
        }
    }
}

