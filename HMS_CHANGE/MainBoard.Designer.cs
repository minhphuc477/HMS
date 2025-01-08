namespace HMS_CHANGE
{
    partial class MainBoard
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainBoard));
            label5 = new Label();
            label3 = new Label();
            pictureBox1 = new PictureBox();
            panel1 = new Panel();
            label1 = new Label();
            checkShowPass = new CheckBox();
            LogInPassTb = new TextBox();
            userGmailTb = new TextBox();
            ForgotPassLb = new Label();
            ToSignUpLb = new Label();
            LogInBt = new Button();
            label2 = new Label();
            label4 = new Label();
            pictureBox2 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Arial", 16.2F);
            label5.Location = new Point(41, 46);
            label5.Margin = new Padding(6, 0, 6, 0);
            label5.Name = "label5";
            label5.Size = new Size(479, 32);
            label5.TabIndex = 11;
            label5.Text = "HOSPITAL MANAGEMENT SYSTEM";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Arial", 16.2F);
            label3.Location = new Point(548, 107);
            label3.Margin = new Padding(6, 0, 6, 0);
            label3.Name = "label3";
            label3.Size = new Size(161, 32);
            label3.TabIndex = 10;
            label3.Text = "WELCOME";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(13, 178);
            pictureBox1.Margin = new Padding(4, 3, 4, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(696, 616);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 12;
            pictureBox1.TabStop = false;
            // 
            // panel1
            // 
            panel1.Controls.Add(label1);
            panel1.Controls.Add(checkShowPass);
            panel1.Controls.Add(LogInPassTb);
            panel1.Controls.Add(userGmailTb);
            panel1.Controls.Add(ForgotPassLb);
            panel1.Controls.Add(ToSignUpLb);
            panel1.Controls.Add(LogInBt);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label4);
            panel1.Location = new Point(646, 178);
            panel1.Name = "panel1";
            panel1.Size = new Size(550, 616);
            panel1.TabIndex = 28;
            panel1.Paint += panel1_Paint;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(97, 266);
            label1.Name = "label1";
            label1.Size = new Size(98, 23);
            label1.TabIndex = 36;
            label1.Text = "Password";
            // 
            // checkShowPass
            // 
            checkShowPass.AutoSize = true;
            checkShowPass.Location = new Point(97, 328);
            checkShowPass.Name = "checkShowPass";
            checkShowPass.Size = new Size(175, 27);
            checkShowPass.TabIndex = 35;
            checkShowPass.Text = "Show Password";
            checkShowPass.UseVisualStyleBackColor = true;
            checkShowPass.CheckedChanged += checkShowPass_CheckedChanged;
            // 
            // LogInPassTb
            // 
            LogInPassTb.Location = new Point(97, 292);
            LogInPassTb.Name = "LogInPassTb";
            LogInPassTb.Size = new Size(354, 30);
            LogInPassTb.TabIndex = 34;
            LogInPassTb.TextChanged += LogInPassTb_TextChanged;
            // 
            // userGmailTb
            // 
            userGmailTb.Location = new Point(97, 206);
            userGmailTb.Name = "userGmailTb";
            userGmailTb.Size = new Size(354, 30);
            userGmailTb.TabIndex = 33;
            userGmailTb.TextChanged += userGmailTb_TextChanged;
            // 
            // ForgotPassLb
            // 
            ForgotPassLb.AutoSize = true;
            ForgotPassLb.Location = new Point(204, 473);
            ForgotPassLb.Name = "ForgotPassLb";
            ForgotPassLb.Size = new Size(173, 23);
            ForgotPassLb.TabIndex = 32;
            ForgotPassLb.Text = "Forgot Password?";
            ForgotPassLb.Click += ForgotPassLb_Click;
            // 
            // ToSignUpLb
            // 
            ToSignUpLb.AutoSize = true;
            ToSignUpLb.Location = new Point(146, 440);
            ToSignUpLb.Name = "ToSignUpLb";
            ToSignUpLb.Size = new Size(288, 23);
            ToSignUpLb.TabIndex = 31;
            ToSignUpLb.Text = "Don't have an account? Sign Up";
            ToSignUpLb.Click += ToSignUpLb_Click;
            // 
            // LogInBt
            // 
            LogInBt.Location = new Point(233, 378);
            LogInBt.Name = "LogInBt";
            LogInBt.Size = new Size(96, 34);
            LogInBt.TabIndex = 30;
            LogInBt.Text = "Log In";
            LogInBt.UseVisualStyleBackColor = true;
            LogInBt.Click += LogInBt_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(97, 180);
            label2.Name = "label2";
            label2.Size = new Size(175, 23);
            label2.TabIndex = 29;
            label2.Text = "Username or gmail";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Arial", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(218, 77);
            label4.Name = "label4";
            label4.Size = new Size(111, 33);
            label4.TabIndex = 28;
            label4.Text = "LOG IN";
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(1104, 35);
            pictureBox2.Margin = new Padding(4, 3, 4, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(35, 43);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 29;
            pictureBox2.TabStop = false;
            pictureBox2.Click += pictureBox2_Click;
            // 
            // MainBoard
            // 
            AutoScaleDimensions = new SizeF(11F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = SystemColors.GradientInactiveCaption;
            ClientSize = new Size(1208, 806);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(label5);
            Controls.Add(label3);
            Controls.Add(panel1);
            Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 3, 4, 3);
            Name = "MainBoard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Hospital";
            Load += MainBoard_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label5;
        private Label label3;
        private PictureBox pictureBox1;
        private Panel panel1;
        private Label label1;
        private CheckBox checkShowPass;
        private TextBox LogInPassTb;
        private TextBox userGmailTb;
        private Label ForgotPassLb;
        private Label ToSignUpLb;
        private Button LogInBt;
        private Label label2;
        private Label label4;
        private PictureBox pictureBox2;
    }
}
