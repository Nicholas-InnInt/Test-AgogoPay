namespace Neptune.NsPay.CefTransferV2.Win.Forms
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
            toolStripContainer1 = new ToolStripContainer();
            statusStrip1 = new StatusStrip();
            tsslblMsg = new ToolStripStatusLabel();
            statusStrip2 = new StatusStrip();
            tsslblBalance = new ToolStripStatusLabel();
            toolStrip1 = new ToolStrip();
            tsbtnLock = new ToolStripButton();
            toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            statusStrip1.SuspendLayout();
            statusStrip2.SuspendLayout();
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
            toolStripContainer1.ContentPanel.Size = new Size(984, 472);
            toolStripContainer1.Dock = DockStyle.Fill;
            toolStripContainer1.Location = new Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(984, 561);
            toolStripContainer1.TabIndex = 1;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(statusStrip2);
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
            // statusStrip2
            // 
            statusStrip2.Dock = DockStyle.None;
            statusStrip2.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            statusStrip2.Items.AddRange(new ToolStripItem[] { tsslblBalance });
            statusStrip2.Location = new Point(0, 0);
            statusStrip2.Name = "statusStrip2";
            statusStrip2.Size = new Size(984, 37);
            statusStrip2.SizingGrip = false;
            statusStrip2.TabIndex = 0;
            statusStrip2.Text = "statusStrip2";
            // 
            // tsslblBalance
            // 
            tsslblBalance.BackColor = Color.Red;
            tsslblBalance.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            tsslblBalance.ForeColor = Color.White;
            tsslblBalance.Name = "tsslblBalance";
            tsslblBalance.RightToLeft = RightToLeft.No;
            tsslblBalance.Size = new Size(182, 32);
            tsslblBalance.Text = "Balance: $$$";
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = DockStyle.None;
            toolStrip1.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new ToolStripItem[] { tsbtnLock });
            toolStrip1.Location = new Point(0, 37);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(984, 27);
            toolStrip1.Stretch = true;
            toolStrip1.TabIndex = 1;
            // 
            // tsbtnLock
            // 
            tsbtnLock.BackColor = SystemColors.ControlLight;
            tsbtnLock.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tsbtnLock.ForeColor = SystemColors.ControlText;
            tsbtnLock.ImageTransparentColor = Color.Magenta;
            tsbtnLock.Name = "tsbtnLock";
            tsbtnLock.Size = new Size(95, 24);
            tsbtnLock.Text = "Lock / Unlock";
            tsbtnLock.Click += tsbtnLock_Click;
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
            statusStrip2.ResumeLayout(false);
            statusStrip2.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private ToolStripContainer toolStripContainer1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tsslblMsg;
        private StatusStrip statusStrip2;
        private ToolStripStatusLabel tsslblBalance;
        private ToolStrip toolStrip1;
        private ToolStripButton tsbtnLock;
    }
}