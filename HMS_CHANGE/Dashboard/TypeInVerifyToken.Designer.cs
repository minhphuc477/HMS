namespace HMS_CHANGE.Dashboard
{
    partial class TypeInVerifyToken
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
            button1 = new Button();
            label3 = new Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(134, 28);
            label1.Name = "label1";
            label1.Size = new Size(182, 33);
            label1.TabIndex = 0;
            label1.Text = "Verify Token";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(61, 87);
            label2.Name = "label2";
            label2.Size = new Size(62, 23);
            label2.TabIndex = 1;
            label2.Text = "Token";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(61, 113);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(286, 30);
            textBox1.TabIndex = 2;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // button1
            // 
            button1.Location = new Point(156, 180);
            button1.Name = "button1";
            button1.Size = new Size(96, 35);
            button1.TabIndex = 3;
            button1.Text = "Check";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(70, 230);
            label3.Name = "label3";
            label3.Size = new Size(277, 23);
            label3.TabIndex = 4;
            label3.Text = "Turn back to Forget Password";
            label3.Click += label3_Click;
            // 
            // TypeInVerifyToken
            // 
            AutoScaleDimensions = new SizeF(11F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.GradientInactiveCaption;
            ClientSize = new Size(437, 299);
            Controls.Add(label3);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(4, 3, 4, 3);
            Name = "TypeInVerifyToken";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "TypeInVerifyToken";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox textBox1;
        private Button button1;
        private Label label3;
    }
}