using Neptune.NsPay.CefTransferV2.Win.Classes;
using System.Globalization;

namespace Neptune.NsPay.CefTransferV2.Win.Forms
{
    public partial class fBrowser : Form
    {
        #region Variable
        #endregion

        #region Property
        public MyBrowser Browser { get; set; }
        public bool CanClose { get; set; } = false;
        #endregion
        public fBrowser()
        {
            InitializeComponent();
        }

        #region Event
        private async void fBrowser_Load(object sender, EventArgs e)
        {
            try
            {
                WriteMessage("Loading ...");
                Init();

                var browser = new ucBrowser();
                browser.Dock = DockStyle.Fill;

                toolStripContainer1.ContentPanel.Controls.Clear();
                toolStripContainer1.ContentPanel.Controls.Add(browser);

                toolStripContainer1.ContentPanel.Enabled = false;

                Browser = new MyBrowser();
                await Browser.Init();
                Browser.DoWriteUiMessage += tsslblMsg_WriteMessage;
                Browser.DoSetBalance += tsslblBalance_SetBalance;
                Browser.DoLockBrowser += tsbtnLock_LockBrowser;

                browser.LaunchBrowser(Browser);

                this.Text = $"{Browser.BankType} - {Browser.Phone}";

                while (true)
                {
                    if (Browser.CanStart)
                    {
                        await Browser.StartAsync();
                        break;
                    }
                    else
                    {
                        await Task.Delay(500);
                    }
                }
            }
            catch (Exception ex)
            {
                var result = AutoClosingMessageBox.Show(
                    text: ex.Message,
                    caption: $"Error {this.Text}",
                    timeout: 5000,
                    buttons: MessageBoxButtons.OK,
                    defaultResult: DialogResult.OK);

                this.InvokeOnUiThreadIfRequired(() => CloseProgram());
            }
        }

        private void fBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CanClose)
            {
                var result = AutoClosingMessageBox.Show(
                    text: $"Stopping, please wait ...\r\n{this.Text} will auto close after all processes are stopped.",
                    caption: $"Close {this.Text}",
                    timeout: 5000,
                    buttons: MessageBoxButtons.OK,
                    defaultResult: DialogResult.OK);

                e.Cancel = true;
                this.InvokeOnUiThreadIfRequired(() => CloseProgram());
            }
        }

        private void tsslblMsg_WriteMessage(object? sender, string message)
        {
            this.InvokeOnUiThreadIfRequired(() => WriteMessage(message));
        }

        private void tsslblBalance_SetBalance(object? sender, decimal balance)
        {
            this.InvokeOnUiThreadIfRequired(() => SetBalance(balance));
        }

        private void tsbtnLock_LockBrowser(object? sender, bool isLock)
        {
            this.InvokeOnUiThreadIfRequired(() => LockBrowser(isLock));
        }

        private void tsbtnLock_Click(object sender, EventArgs e)
        {
            this.InvokeOnUiThreadIfRequired(() => LockUnlockBrowser());
        }
        #endregion

        #region Public
        #endregion

        #region Private

        private void Init()
        {
            this.Text = "NsPay";
        }

        private void WriteMessage(string message)
        {
            this.tsslblMsg.Text = message;
        }

        private void SetBalance(decimal balance)
        {
            this.tsslblBalance.Text = double.Parse(balance.ToString()).ToString("C0", CultureInfo.GetCultureInfo("vi-VN"));
        }

        private void LockBrowser(bool isLock)
        {
            this.toolStripContainer1.ContentPanel.Enabled = !isLock;
        }

        private void LockUnlockBrowser()
        {
            this.toolStripContainer1.ContentPanel.Enabled = !toolStripContainer1.ContentPanel.Enabled;
        }

        private async void CloseProgram()
        {
            while (Browser.IsRunning)
            {
                await Task.Delay(500);
            }
            CanClose = true;
            this.Close();
        }

        #endregion
    }
}
