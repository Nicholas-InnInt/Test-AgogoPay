using Neptune.NsPay.Transfer.Win.Classes;
using System.Globalization;

namespace Neptune.NsPay.Transfer.Win.Forms
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

                this.InvokeOnUiThreadIfRequired(() => LockBrowser(true));

                Browser = new MyBrowser();
                await Browser.Init();
                Browser.Dpi = this.DeviceDpi;
                Browser.DoWriteUiMessage += tsslblMsg_WriteMessage;
                Browser.DoSetBalance += tsbtnBalance_SetBalance;
                Browser.DoLockBrowser += tsbtnLock_LockBrowser;
                Browser.DoSetCountDown += tsbtnCountDown_SetCountDown;
                Browser.DoResetCountDown += tsbtnCountDown_ResetCountDown;
                Browser.DoSetPointer += tsblbPointer_SetPointer;

                browser.LaunchBrowser(Browser);

                tslblPointer.Visible = Browser.IsDebug;

                this.Text = $"{Browser.BankType} - {Browser.Phone}";
                await Browser.StartAsync();
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

        private void tsbtnBalance_SetBalance(object? sender, decimal balance)
        {
            this.InvokeOnUiThreadIfRequired(() => SetBalance(balance));
        }

        private void tsbtnCountDown_SetCountDown(object? sender, int countDown)
        {
            this.InvokeOnUiThreadIfRequired(() => SetCountDown(countDown));
        }

        private void tsbtnCountDown_ResetCountDown(object? sender, bool reset)
        {
            this.InvokeOnUiThreadIfRequired(() => ResetCountDown());
        }

        private void tsblbPointer_SetPointer(object? sender, string pointer)
        {
            if (Browser.IsDebug)
                this.InvokeOnUiThreadIfRequired(() => SetPointer(pointer));
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
            this.tsbtnBalance.Text = double.Parse(balance.ToString()).ToString("C0", CultureInfo.GetCultureInfo("vi-VN"));
        }

        private void LockBrowser(bool isLock)
        {
            this.toolStripContainer1.ContentPanel.Enabled = !isLock;
            this.tsbtnLock.Image = this.toolStripContainer1.ContentPanel.Enabled ?
               Properties.Resources.Unlock_32 : Properties.Resources.Lock_32;
            this.tsbtnLock.BackColor = this.toolStripContainer1.ContentPanel.Enabled ?
                Color.FromArgb(192, 255, 192) : Color.FromArgb(255, 192, 192);

            if (this.toolStripContainer1.ContentPanel.Enabled)
            {
                this.toolStripContainer1.ContentPanel.Focus();
            }
        }

        private void LockUnlockBrowser()
        {
            LockBrowser(this.toolStripContainer1.ContentPanel.Enabled);
        }

        private void SetCountDown(int countDown)
        {
            if (countDown <= 5)
            { this.tsbtnCountDown.ForeColor = Color.Red; }
            else
            { this.tsbtnCountDown.ForeColor = SystemColors.ControlText; }
            this.tsbtnCountDown.Text = countDown.ToString();
        }

        private void ResetCountDown()
        {
            this.tsbtnCountDown.ForeColor = SystemColors.ControlText;
            this.tsbtnCountDown.Text = "---";
        }

        private void SetPointer(string pointer)
        {
            this.tslblPointer.Text = pointer;
        }

        private async void CloseProgram()
        {
            while (Browser.IsRunning)
            {
                await Task.Delay(500);
            }
            Browser.Dispose();
            CanClose = true;
            this.Close();
        }

        #endregion
    }
}
