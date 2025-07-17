using Neptune.NsPay.CefTransfer.Win.Classes;

namespace Neptune.NsPay.CefTransfer.Win.UserControls
{
    public partial class ucLogin3 : UserControl
    {
        private Color COLOR_GREEN = Color.FromArgb(192, 255, 192);
        private Color COLOR_DEFAULT = SystemColors.Control;
        private Color COLOR_BLACK = SystemColors.ControlText;
        private Color COLOR_RED = Color.Red;

        private MyBrowser _browser;
        public ucLogin3(MyBrowser browser)
        {
            _browser = browser;
            _browser.GetTransactionCode += browser_GetTransactionCode;
            _browser.EditingLoginPage3 += browser_EditingLoginPage3;
            _browser.SetCountdown += browser_SetCountdown;

            InitializeComponent();
        }

        private void ucLogin3_Load(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => this.lblCountdown.Text = "");
            this.InvokeOnUiThreadIfRequired(() => SetTransactionCode(_browser.TransactionCode));
            this.InvokeOnUiThreadIfRequired(() => SetEditing(true));
        }

        private void btnOTP_Click(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => SetEditing(false));
            _browser.OTP = this.txtOTP.Text.Trim();
            _browser.EnteringOtp = false;
        }

        private void browser_EditingLoginPage3(object? sender, bool editing)
        {
            this.InvokeOnUiThreadIfRequired(() => SetEditing(editing));
        }

        private void browser_GetTransactionCode(object? sender, string transactionCode)
        {
            this.InvokeOnUiThreadIfRequired(() => SetTransactionCode(transactionCode));
        }

        private void browser_SetCountdown(object? sender, int countdown)
        {
            this.InvokeOnUiThreadIfRequired(() => SetCountdown(countdown));
        }

        private void SetEditing(bool editing)
        {
            this.Enabled = editing;
            this.BackColor = editing ? COLOR_GREEN : COLOR_DEFAULT;
            if (editing)
                this.txtOTP.Focus();
        }

        private void SetTransactionCode(string transactionCode)
        {
            this.txtTransCode.Text = transactionCode;
        }

        private void SetCountdown(int countdown)
        {
            if (countdown >= 10)
                this.lblCountdown.ForeColor = COLOR_BLACK;
            else
                this.lblCountdown.ForeColor = COLOR_RED;
            this.lblCountdown.Text = countdown.ToString();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                btnOTP.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
