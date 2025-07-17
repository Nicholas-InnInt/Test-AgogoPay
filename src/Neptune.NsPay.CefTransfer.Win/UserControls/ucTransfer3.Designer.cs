namespace Neptune.NsPay.CefTransfer.Win.UserControls
{
    partial class ucTransfer3
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
            btnOTP = new Button();
            txtOTP = new TextBox();
            lblOTP = new Label();
            txtTransCode = new TextBox();
            lblTransCode = new Label();
            lblTitle = new Label();
            lblCountdown = new Label();
            SuspendLayout();
            // 
            // btnOTP
            // 
            btnOTP.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point);
            btnOTP.Location = new Point(7, 141);
            btnOTP.Name = "btnOTP";
            btnOTP.Size = new Size(204, 26);
            btnOTP.TabIndex = 20;
            btnOTP.Text = "Enter OTP";
            btnOTP.UseVisualStyleBackColor = true;
            btnOTP.Click += btnOTP_Click;
            // 
            // txtOTP
            // 
            txtOTP.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtOTP.Location = new Point(8, 109);
            txtOTP.Name = "txtOTP";
            txtOTP.Size = new Size(203, 26);
            txtOTP.TabIndex = 10;
            // 
            // lblOTP
            // 
            lblOTP.AutoSize = true;
            lblOTP.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point);
            lblOTP.Location = new Point(8, 86);
            lblOTP.Name = "lblOTP";
            lblOTP.Size = new Size(35, 20);
            lblOTP.TabIndex = 14;
            lblOTP.Text = "OTP";
            // 
            // txtTransCode
            // 
            txtTransCode.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtTransCode.Location = new Point(8, 52);
            txtTransCode.Name = "txtTransCode";
            txtTransCode.ReadOnly = true;
            txtTransCode.Size = new Size(289, 26);
            txtTransCode.TabIndex = 0;
            txtTransCode.TabStop = false;
            // 
            // lblTransCode
            // 
            lblTransCode.AutoSize = true;
            lblTransCode.Font = new Font("Arial Narrow", 12F, FontStyle.Regular, GraphicsUnit.Point);
            lblTransCode.Location = new Point(8, 29);
            lblTransCode.Name = "lblTransCode";
            lblTransCode.Size = new Size(113, 20);
            lblTransCode.TabIndex = 11;
            lblTransCode.Text = "Transaction Code";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Arial", 12F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Location = new Point(8, 5);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(143, 19);
            lblTitle.TabIndex = 21;
            lblTitle.Text = "Transfer Screen 3";
            // 
            // lblCountdown
            // 
            lblCountdown.Font = new Font("Arial", 18F, FontStyle.Bold, GraphicsUnit.Point);
            lblCountdown.Location = new Point(217, 86);
            lblCountdown.Name = "lblCountdown";
            lblCountdown.Size = new Size(80, 81);
            lblCountdown.TabIndex = 22;
            lblCountdown.Text = "??";
            lblCountdown.TextAlign = ContentAlignment.MiddleRight;
            // 
            // ucTransfer3
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblCountdown);
            Controls.Add(lblTitle);
            Controls.Add(btnOTP);
            Controls.Add(txtOTP);
            Controls.Add(lblOTP);
            Controls.Add(txtTransCode);
            Controls.Add(lblTransCode);
            Name = "ucTransfer3";
            Size = new Size(300, 180);
            Load += ucTransfer3_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnOTP;
        private TextBox txtOTP;
        private Label lblOTP;
        private TextBox txtTransCode;
        private Label lblTransCode;
        private Label lblTitle;
        private Label lblCountdown;
    }
}
