namespace Neptune.NsPay.Transfer.Win.Forms
{
    partial class fBrowser
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(fBrowser));
            toolStripContainer1 = new ToolStripContainer();
            statusStrip1 = new StatusStrip();
            tsslblMsg = new ToolStripStatusLabel();
            toolStrip1 = new ToolStrip();
            tsbtnLock = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            tsbtnBalance = new ToolStripButton();
            tsbtnCountDown = new ToolStripButton();
            tslblPointer = new ToolStripLabel();
            toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            statusStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            toolStripContainer1.BottomToolStripPanel.Controls.Add(statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Size = new Size(984, 487);
            toolStripContainer1.Dock = DockStyle.Fill;
            toolStripContainer1.Location = new Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(984, 561);
            toolStripContainer1.TabIndex = 1;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
            // 
            // statusStrip1
            // 
            statusStrip1.Dock = DockStyle.None;
            statusStrip1.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            statusStrip1.Items.AddRange(new ToolStripItem[] { tsslblMsg });
            statusStrip1.Location = new Point(0, 0);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(984, 25);
            statusStrip1.SizingGrip = false;
            statusStrip1.TabIndex = 0;
            // 
            // tsslblMsg
            // 
            tsslblMsg.Name = "tsslblMsg";
            tsslblMsg.Size = new Size(48, 20);
            tsslblMsg.Text = "Ready";
            // 
            // toolStrip1
            // 
            toolStrip1.BackColor = SystemColors.Control;
            toolStrip1.Dock = DockStyle.None;
            toolStrip1.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new ToolStripItem[] { tsbtnLock, toolStripSeparator1, tsbtnBalance, tsbtnCountDown, tslblPointer });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(984, 49);
            toolStrip1.Stretch = true;
            toolStrip1.TabIndex = 1;
            // 
            // tsbtnLock
            // 
            tsbtnLock.BackColor = Color.FromArgb(192, 255, 192);
            tsbtnLock.DisplayStyle = ToolStripItemDisplayStyle.Image;
            tsbtnLock.Font = new Font("Arial Narrow", 20.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tsbtnLock.ForeColor = SystemColors.ControlText;
            tsbtnLock.Image = Properties.Resources.Unlock_32;
            tsbtnLock.ImageScaling = ToolStripItemImageScaling.None;
            tsbtnLock.ImageTransparentColor = Color.Magenta;
            tsbtnLock.Name = "tsbtnLock";
            tsbtnLock.Padding = new Padding(5);
            tsbtnLock.Size = new Size(46, 46);
            tsbtnLock.Text = "Lock / Unlock";
            tsbtnLock.Click += tsbtnLock_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Margin = new Padding(5);
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 39);
            // 
            // tsbtnBalance
            // 
            tsbtnBalance.BackColor = Color.Red;
            tsbtnBalance.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbtnBalance.ForeColor = Color.White;
            tsbtnBalance.Image = (Image)resources.GetObject("tsbtnBalance.Image");
            tsbtnBalance.ImageTransparentColor = Color.Magenta;
            tsbtnBalance.Name = "tsbtnBalance";
            tsbtnBalance.Padding = new Padding(5);
            tsbtnBalance.Size = new Size(196, 46);
            tsbtnBalance.Text = "Balance: $$$";
            // 
            // tsbtnCountDown
            // 
            tsbtnCountDown.BackColor = Color.White;
            tsbtnCountDown.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbtnCountDown.Image = (Image)resources.GetObject("tsbtnCountDown.Image");
            tsbtnCountDown.ImageTransparentColor = Color.Magenta;
            tsbtnCountDown.Name = "tsbtnCountDown";
            tsbtnCountDown.Padding = new Padding(5);
            tsbtnCountDown.Size = new Size(55, 46);
            tsbtnCountDown.Text = "---";
            // 
            // tslblPointer
            // 
            tslblPointer.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tslblPointer.Name = "tslblPointer";
            tslblPointer.Size = new Size(50, 46);
            tslblPointer.Text = "pointer";
            // 
            // fBrowser
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 561);
            Controls.Add(toolStripContainer1);
            Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(1000, 500);
            Name = "fBrowser";
            Text = "fBrowser";
            FormClosing += fBrowser_FormClosing;
            Load += fBrowser_Load;
            toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            toolStripContainer1.BottomToolStripPanel.PerformLayout();
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private ToolStripContainer toolStripContainer1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tsslblMsg;
        private ToolStrip toolStrip1;
        private ToolStripButton tsbtnLock;
        private ToolStripButton tsbtnBalance;
        private ToolStripButton tsbtnCountDown;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripLabel tslblPointer;
    }
}