using Neptune.NsPay.CefTransfer.Win.Classes;

namespace Neptune.NsPay.CefTransfer.Win.UserControls
{
    public partial class ucTransfer3 : UserControl
    {
        private Color COLOR_GREEN = Color.FromArgb(192, 255, 192);
        private Color COLOR_DEFAULT = SystemColors.Control;
        private Color COLOR_BLACK = SystemColors.ControlText;
        private Color COLOR_RED = Color.Red;

        private MyBrowser _browser;
        public ucTransfer3(MyBrowser browser)
        {
            _browser = browser;
            _browser.GetTransferTransactionCode += browser_GetTransferTransCode;
            _browser.EditingTransferPage3 += browser_EditingTransferPage3;
            _browser.SetCountdown += browser_SetCountdown;

            InitializeComponent();
        }

        private void ucTransfer3_Load(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => this.lblCountdown.Text = "");
            this.InvokeOnUiThreadIfRequired(() => SetTransferTransCode(_browser.TransferTransCode));
            this.InvokeOnUiThreadIfRequired(() => SetEditing(true));
        }

        private void btnOTP_Click(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => SetEditing(false));
            _browser.TransferOTP = this.txtOTP.Text.Trim();
            _browser.EnteringTransOtp = false;
        }

        private void browser_EditingTransferPage3(object? sender, bool editing)
        {
            this.InvokeOnUiThreadIfRequired(() => SetEditing(editing));
        }

        private void browser_GetTransferTransCode(object? sender, string transferTransCode)
        {
            this.InvokeOnUiThreadIfRequired(() => SetTransferTransCode(transferTransCode));
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

        private void SetCountdown(int countdown)
        {
            if (countdown >= 10)
                this.lblCountdown.ForeColor = COLOR_BLACK;
            else
                this.lblCountdown.ForeColor = COLOR_RED;
            this.lblCountdown.Text = countdown.ToString();
        }

        private void SetTransferTransCode(string transferTransCode)
        {
            this.txtTransCode.Text = transferTransCode;
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
