namespace Neptune.NsPay.CefTransfer.Win.UserControls
{
    partial class ucAuth
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblCountdown = new Label();
            SuspendLayout();
            // 
            // lblCountdown
            // 
            lblCountdown.Font = new Font("Arial", 18F, FontStyle.Bold, GraphicsUnit.Point);
            lblCountdown.ForeColor = SystemColors.ControlText;
            lblCountdown.Location = new Point(3, 0);
            lblCountdown.Name = "lblCountdown";
            lblCountdown.Size = new Size(80, 81);
            lblCountdown.TabIndex = 23;
            lblCountdown.Text = "??";
            lblCountdown.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ucAuth
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblCountdown);
            Name = "ucAuth";
            Size = new Size(300, 150);
            Load += ucAuth_Load;
            ResumeLayout(false);
        }

        #endregion

        private Label lblCountdown;
    }
}
