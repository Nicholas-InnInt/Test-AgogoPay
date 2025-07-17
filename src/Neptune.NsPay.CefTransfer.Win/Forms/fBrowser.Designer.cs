namespace Neptune.NsPay.CefTransfer.Win.Forms
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
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            pbScrnshot = new PictureBox();
            btnSave = new Button();
            txtScrnTime = new TextBox();
            toolStrip1 = new ToolStrip();
            tstxtUrl = new ToolStripTextBox();
            menuStrip1 = new MenuStrip();
            tsmiScrnshot = new ToolStripMenuItem();
            statusStrip2 = new StatusStrip();
            tsslblBalance = new ToolStripStatusLabel();
            toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbScrnshot).BeginInit();
            toolStrip1.SuspendLayout();
            menuStrip1.SuspendLayout();
            statusStrip2.SuspendLayout();
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
            toolStripContainer1.ContentPanel.AutoScroll = true;
            toolStripContainer1.ContentPanel.Controls.Add(splitContainer1);
            toolStripContainer1.ContentPanel.Size = new Size(784, 393);
            toolStripContainer1.Dock = DockStyle.Fill;
            toolStripContainer1.Location = new Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(784, 509);
            toolStripContainer1.TabIndex = 0;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(statusStrip2);
            toolStripContainer1.TopToolStripPanel.Controls.Add(menuStrip1);
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
            // 
            // statusStrip1
            // 
            statusStrip1.Dock = DockStyle.None;
            statusStrip1.Items.AddRange(new ToolStripItem[] { tsslblMsg });
            statusStrip1.Location = new Point(0, 0);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(784, 25);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // tsslblMsg
            // 
            tsslblMsg.Font = new Font("Arial Narrow", 12F);
            tsslblMsg.Name = "tsslblMsg";
            tsslblMsg.Size = new Size(769, 20);
            tsslblMsg.Spring = true;
            tsslblMsg.Text = "Ready";
            tsslblMsg.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.IsSplitterFixed = true;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Font = new Font("Arial", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            splitContainer1.Panel1.ForeColor = Color.White;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(784, 393);
            splitContainer1.SplitterDistance = 400;
            splitContainer1.TabIndex = 0;
            splitContainer1.TabStop = false;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.FixedPanel = FixedPanel.Panel2;
            splitContainer2.IsSplitterFixed = true;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(pbScrnshot);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(btnSave);
            splitContainer2.Panel2.Controls.Add(txtScrnTime);
            splitContainer2.Size = new Size(380, 393);
            splitContainer2.SplitterDistance = 353;
            splitContainer2.TabIndex = 0;
            splitContainer2.TabStop = false;
            // 
            // pbScrnshot
            // 
            pbScrnshot.Dock = DockStyle.Fill;
            pbScrnshot.Location = new Point(0, 0);
            pbScrnshot.Name = "pbScrnshot";
            pbScrnshot.Size = new Size(380, 353);
            pbScrnshot.SizeMode = PictureBoxSizeMode.Zoom;
            pbScrnshot.TabIndex = 0;
            pbScrnshot.TabStop = false;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Location = new Point(302, 3);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 30);
            btnSave.TabIndex = 0;
            btnSave.TabStop = false;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // txtScrnTime
            // 
            txtScrnTime.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtScrnTime.BorderStyle = BorderStyle.None;
            txtScrnTime.Location = new Point(3, 8);
            txtScrnTime.Name = "txtScrnTime";
            txtScrnTime.ReadOnly = true;
            txtScrnTime.Size = new Size(298, 19);
            txtScrnTime.TabIndex = 0;
            txtScrnTime.TabStop = false;
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = DockStyle.Bottom;
            toolStrip1.Font = new Font("Arial Narrow", 12F);
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new ToolStripItem[] { tstxtUrl });
            toolStrip1.Location = new Point(0, 65);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(784, 26);
            toolStrip1.Stretch = true;
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            toolStrip1.Layout += toolStrip1_Layout;
            // 
            // tstxtUrl
            // 
            tstxtUrl.AutoSize = false;
            tstxtUrl.Font = new Font("Arial Narrow", 12F);
            tstxtUrl.Name = "tstxtUrl";
            tstxtUrl.ReadOnly = true;
            tstxtUrl.Size = new Size(100, 26);
            // 
            // menuStrip1
            // 
            menuStrip1.Dock = DockStyle.None;
            menuStrip1.Font = new Font("Arial Narrow", 12F);
            menuStrip1.Items.AddRange(new ToolStripItem[] { tsmiScrnshot });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(784, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // tsmiScrnshot
            // 
            tsmiScrnshot.Name = "tsmiScrnshot";
            tsmiScrnshot.Size = new Size(89, 24);
            tsmiScrnshot.Text = "Screenshot";
            tsmiScrnshot.Click += tsmiScrnshot_Click;
            // 
            // statusStrip2
            // 
            statusStrip2.Dock = DockStyle.None;
            statusStrip2.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            statusStrip2.Items.AddRange(new ToolStripItem[] { tsslblBalance });
            statusStrip2.Location = new Point(0, 28);
            statusStrip2.Name = "statusStrip2";
            statusStrip2.Size = new Size(784, 37);
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
            tsslblBalance.Size = new Size(182, 32);
            tsslblBalance.Text = "Balance: $$$";
            // 
            // fBrowser
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 509);
            Controls.Add(toolStripContainer1);
            Font = new Font("Arial Narrow", 12F);
            MainMenuStrip = menuStrip1;
            Margin = new Padding(3, 4, 3, 4);
            Name = "fBrowser";
            Text = "fBrowser";
            FormClosing += fBrowser_FormClosing;
            Load += fBrowser_Load;
            SizeChanged += fBrowser_SizeChanged;
            toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            toolStripContainer1.BottomToolStripPanel.PerformLayout();
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pbScrnshot).EndInit();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip2.ResumeLayout(false);
            statusStrip2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private ToolStripContainer toolStripContainer1;
        private MenuStrip menuStrip1;
        private ToolStrip toolStrip1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tsslblMsg;
        private ToolStripTextBox tstxtUrl;
        private SplitContainer splitContainer1;
        private PictureBox pbScrnshot;
        private ToolStripMenuItem tsmiScrnshot;
        private Button btnSave;
        private SplitContainer splitContainer2;
        private TextBox txtScrnTime;
        private StatusStrip statusStrip2;
        private ToolStripStatusLabel tsslblBalance;
    }
}