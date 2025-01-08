namespace HMS_CHANGE.Dashboard
{
    partial class ResetPassword
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
            label1 = new Label();
            label2 = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            button1 = new Button();
            label3 = new Label();
            label4 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(137, 33);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(231, 33);
            label1.TabIndex = 0;
            label1.Text = "Reset Password";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(93, 122);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(143, 23);
            label2.TabIndex = 1;
            label2.Text = "New Password";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(93, 148);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(316, 30);
            textBox1.TabIndex = 2;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(93, 242);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(316, 30);
            textBox2.TabIndex = 3;
            textBox2.TextChanged += textBox2_TextChanged;
            // 
            // button1
            // 
            button1.Location = new Point(185, 300);
            button1.Name = "button1";
            button1.Size = new Size(123, 52);
            button1.TabIndex = 4;
            button1.Text = "Change";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(93, 216);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(197, 23);
            label3.TabIndex = 5;
            label3.Text = "New Password Again";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(145, 367);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(227, 23);
            label4.TabIndex = 6;
            label4.Text = "Turn back to ForgetPass";
            label4.Click += label4_Click;
            // 
            // ResetPassword
            // 
            AutoScaleDimensions = new SizeF(11F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.GradientInactiveCaption;
            ClientSize = new Size(506, 428);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(button1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(4, 3, 4, 3);
            Name = "ResetPassword";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ResetPassword";
            Load += ResetPassword_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox textBox1;
        private TextBox textBox2;
        private Button button1;
        private Label label3;
        private Label label4;
    }
}