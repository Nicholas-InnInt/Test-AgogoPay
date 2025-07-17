using Neptune.NsPay.CefTransfer.Common.Classes;
using Neptune.NsPay.CefTransfer.Common.Models;
using Neptune.NsPay.CefTransfer.Win.Classes;

namespace Neptune.NsPay.CefTransfer.Win.UserControls
{
    public partial class ucLogin1 : UserControl
    {
        private Color COLOR_GREEN = Color.FromArgb(192, 255, 192);
        private Color COLOR_DEFAULT = SystemColors.Control;
        private Color COLOR_BLACK = SystemColors.ControlText;
        private Color COLOR_RED = Color.Red;

        private MyBrowser _browser;
        public ucLogin1(MyBrowser browser)
        {
            _browser = browser;
            _browser.VcbGetCaptcha += browser_VcbGetCaptcha;
            _browser.EditingLoginPage1 += browser_EditingLoginPage1;
            _browser.SetCountdown += browser_SetCountdown;

            InitializeComponent();
        }

        private void browser_EditingLoginPage1(object? sender, bool editing)
        {
            this.InvokeOnUiThreadIfRequired(() => SetEditing(editing));
        }

        private void ucLogin1_Load(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => this.lblCountdown.Text = "");
            if (_browser.VcbVerificationCode != null)
            {
                this.InvokeOnUiThreadIfRequired(() => SetCaptchaImage(_browser.VcbVerificationCode));
            }
            this.InvokeOnUiThreadIfRequired(() => SetEditing(true));
        }

        private void btnCaptcha_Click(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => SetEditing(false));
            _browser.Captcha = this.txtCaptcha.Text.Trim();
            _browser.EnteringCaptcha = false;
        }

        private void browser_VcbGetCaptcha(object? sender, VcbVerificationCodeModel model)
        {
            this.InvokeOnUiThreadIfRequired(() => SetCaptchaImage(model));
        }

        private void browser_SetCountdown(object? sender, int countdown)
        {
            this.InvokeOnUiThreadIfRequired(() => SetCountdown(countdown));
        }

        private void SetCaptchaImage(VcbVerificationCodeModel model)
        {
            this.pbVeriCode.Image?.Dispose();
            this.pbVeriCode.Image = model.CaptchaImage.ToBitmap();
            this.txtCaptcha.Text = "";
        }

        private void SetEditing(bool editing)
        {
            this.Enabled = editing;
            this.pbVeriCode.Visible = editing;
            this.BackColor = editing ? COLOR_GREEN : COLOR_DEFAULT;
            if (editing)
                this.txtCaptcha.Focus();
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
                btnCaptcha.PerformClick();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
