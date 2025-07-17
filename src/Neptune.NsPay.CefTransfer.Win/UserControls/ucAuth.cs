using Neptune.NsPay.CefTransfer.Win.Classes;

namespace Neptune.NsPay.CefTransfer.Win.UserControls
{
    public partial class ucAuth : UserControl
    {
        private Color COLOR_BLACK = SystemColors.ControlText;
        private Color COLOR_RED = Color.Red;
        private MyBrowser _browser;
        public ucAuth(MyBrowser browser)
        {
            _browser = browser;
            _browser.SetCountdown += browser_SetCountdown;

            InitializeComponent();
        }

        private void ucAuth_Load(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => this.lblCountdown.Text = "");
        }

        private void browser_SetCountdown(object? sender, int countdown)
        {
            this.InvokeOnUiThreadIfRequired(() => SetCountdown(countdown));
        }

        private void SetCountdown(int countdown)
        {
            if (countdown >= 10)
                this.lblCountdown.ForeColor = COLOR_BLACK;
            else
                this.lblCountdown.ForeColor = COLOR_RED;
            this.lblCountdown.Text = countdown.ToString();
        }
    }
}
