namespace Neptune.NsPay.CefTransfer.Win.UserControls
{
    partial class ucLogin1
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
            pbVeriCode = new PictureBox();
            btnCaptcha = new Button();
            lblCaptcha = new Label();
            txtCaptcha = new TextBox();
            lblTitle = new Label();
            lblCountdown = new Label();
            ((System.ComponentModel.ISupportInitialize)pbVeriCode).BeginInit();
            SuspendLayout();
            // 
            // pbVeriCode
            // 
            pbVeriCode.Location = new Point(8, 27);
            pbVeriCode.Name = "pbVeriCode";
            pbVeriCode.Size = new Size(289, 90);
            pbVeriCode.SizeMode = PictureBoxSizeMode.Zoom;
            pbVeriCode.TabIndex = 2;
            pbVeriCode.TabStop = false;
            // 
            // btnCaptcha
            // 
            btnCaptcha.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point);
            btnCaptcha.Location = new Point(8, 175);
            btnCaptcha.Name = "btnCaptcha";
            btnCaptcha.Size = new Size(202, 26);
            btnCaptcha.TabIndex = 20;
            btnCaptcha.Text = "Enter Captcha";
            btnCaptcha.UseVisualStyleBackColor = true;
            btnCaptcha.Click += btnCaptcha_Click;
            // 
            // lblCaptcha
            // 
            lblCaptcha.AutoSize = true;
            lblCaptcha.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point);
            lblCaptcha.Location = new Point(8, 120);
            lblCaptcha.Name = "lblCaptcha";
            lblCaptcha.Size = new Size(57, 20);
            lblCaptcha.TabIndex = 4;
            lblCaptcha.Text = "Captcha";
            // 
            // txtCaptcha
            // 
            txtCaptcha.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtCaptcha.Location = new Point(8, 143);
            txtCaptcha.Name = "txtCaptcha";
            txtCaptcha.Size = new Size(202, 26);
            txtCaptcha.TabIndex = 10;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Location = new Point(8, 5);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(124, 19);
            lblTitle.TabIndex = 21;
            lblTitle.Text = "Login Screen 1";
            // 
            // lblCountdown
            // 
            lblCountdown.Font = new Font("Arial", 18F, FontStyle.Bold, GraphicsUnit.Point);
            lblCountdown.ForeColor = SystemColors.ControlText;
            lblCountdown.Location = new Point(217, 120);
            lblCountdown.Name = "lblCountdown";
            lblCountdown.Size = new Size(80, 81);
            lblCountdown.TabIndex = 22;
            lblCountdown.Text = "??";
            lblCountdown.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ucLogin1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblCountdown);
            Controls.Add(lblTitle);
            Controls.Add(pbVeriCode);
            Controls.Add(btnCaptcha);
            Controls.Add(lblCaptcha);
            Controls.Add(txtCaptcha);
            Name = "ucLogin1";
            Size = new Size(300, 215);
            Load += ucLogin1_Load;
            ((System.ComponentModel.ISupportInitialize)pbVeriCode).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pbVeriCode;
        private Button btnCaptcha;
        private Label lblCaptcha;
        private TextBox txtCaptcha;
        private Label lblTitle;
        private Label lblCountdown;
    }
}
