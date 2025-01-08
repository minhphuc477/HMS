namespace HMS_CHANGE.Dashboard
{
    partial class SignUp
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
            checkShowPass = new CheckBox();
            textBox3 = new TextBox();
            label8 = new Label();
            AdminKEY = new TextBox();
            key = new Label();
            RoleCb = new ComboBox();
            label7 = new Label();
            label6 = new Label();
            dateTimePicker1 = new DateTimePicker();
            textBox2 = new TextBox();
            textBox1 = new TextBox();
            button1 = new Button();
            label5 = new Label();
            radioButton2 = new RadioButton();
            radioButton1 = new RadioButton();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            label1 = new Label();
            textBox4 = new TextBox();
            label9 = new Label();
            SuspendLayout();
            // 
            // checkShowPass
            // 
            checkShowPass.AutoSize = true;
            checkShowPass.Location = new Point(166, 381);
            checkShowPass.Margin = new Padding(4, 3, 4, 3);
            checkShowPass.Name = "checkShowPass";
            checkShowPass.Size = new Size(175, 27);
            checkShowPass.TabIndex = 40;
            checkShowPass.Text = "Show Password";
            checkShowPass.UseVisualStyleBackColor = true;
            checkShowPass.CheckedChanged += checkShowPass_CheckedChanged;
            // 
            // textBox3
            // 
            textBox3.Location = new Point(166, 340);
            textBox3.Margin = new Padding(4, 3, 4, 3);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(350, 30);
            textBox3.TabIndex = 39;
            textBox3.TextChanged += textBox3_TextChanged;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(166, 310);
            label8.Margin = new Padding(4, 0, 4, 0);
            label8.Name = "label8";
            label8.Size = new Size(98, 23);
            label8.TabIndex = 38;
            label8.Text = "Password";
            // 
            // AdminKEY
            // 
            AdminKEY.Location = new Point(158, 664);
            AdminKEY.Margin = new Padding(4, 3, 4, 3);
            AdminKEY.Name = "AdminKEY";
            AdminKEY.Size = new Size(350, 30);
            AdminKEY.TabIndex = 37;
            AdminKEY.TextChanged += AdminKEY_TextChanged;
            // 
            // key
            // 
            key.AutoSize = true;
            key.Location = new Point(81, 667);
            key.Margin = new Padding(4, 0, 4, 0);
            key.Name = "key";
            key.Size = new Size(50, 23);
            key.TabIndex = 36;
            key.Text = "KEY";
            key.Click += key_Click;
            // 
            // RoleCb
            // 
            RoleCb.FormattingEnabled = true;
            RoleCb.Items.AddRange(new object[] { "Patient", "Doctor", "Admin" });
            RoleCb.Location = new Point(253, 605);
            RoleCb.Margin = new Padding(4, 3, 4, 3);
            RoleCb.Name = "RoleCb";
            RoleCb.Size = new Size(206, 31);
            RoleCb.TabIndex = 35;
            RoleCb.SelectedIndexChanged += RoleCb_SelectedIndexChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(181, 777);
            label7.Margin = new Padding(4, 0, 4, 0);
            label7.Name = "label7";
            label7.Size = new Size(297, 23);
            label7.TabIndex = 34;
            label7.Text = "Already have an account? Log In";
            label7.Click += label7_Click;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(166, 609);
            label6.Margin = new Padding(4, 0, 4, 0);
            label6.Name = "label6";
            label6.Size = new Size(50, 23);
            label6.TabIndex = 33;
            label6.Text = "Role";
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Location = new Point(158, 472);
            dateTimePicker1.Margin = new Padding(4, 3, 4, 3);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(350, 30);
            dateTimePicker1.TabIndex = 32;
            dateTimePicker1.ValueChanged += dateTimePicker1_ValueChanged;
            // 
            // textBox2
            // 
            textBox2.Location = new Point(166, 183);
            textBox2.Margin = new Padding(4, 3, 4, 3);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(350, 30);
            textBox2.TabIndex = 31;
            textBox2.TextChanged += textBox2_TextChanged;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(166, 98);
            textBox1.Margin = new Padding(4, 3, 4, 3);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(350, 30);
            textBox1.TabIndex = 30;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // button1
            // 
            button1.Location = new Point(181, 710);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(301, 40);
            button1.TabIndex = 29;
            button1.Text = "Sign Up";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(158, 524);
            label5.Margin = new Padding(4, 0, 4, 0);
            label5.Name = "label5";
            label5.Size = new Size(76, 23);
            label5.TabIndex = 28;
            label5.Text = "Gender";
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Location = new Point(381, 554);
            radioButton2.Margin = new Padding(4, 3, 4, 3);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new Size(74, 27);
            radioButton2.TabIndex = 27;
            radioButton2.TabStop = true;
            radioButton2.Text = "Male";
            radioButton2.UseVisualStyleBackColor = true;
            radioButton2.CheckedChanged += radioButton2_CheckedChanged;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new Point(181, 554);
            radioButton1.Margin = new Padding(4, 3, 4, 3);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new Size(96, 27);
            radioButton1.TabIndex = 26;
            radioButton1.TabStop = true;
            radioButton1.Text = "Female";
            radioButton1.UseVisualStyleBackColor = true;
            radioButton1.CheckedChanged += radioButton1_CheckedChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(158, 445);
            label4.Margin = new Padding(4, 0, 4, 0);
            label4.Name = "label4";
            label4.Size = new Size(121, 23);
            label4.TabIndex = 25;
            label4.Text = "Date of Birth";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(166, 153);
            label3.Margin = new Padding(4, 0, 4, 0);
            label3.Name = "label3";
            label3.Size = new Size(58, 23);
            label3.TabIndex = 24;
            label3.Text = "Email";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(159, 68);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(61, 23);
            label2.TabIndex = 23;
            label2.Text = "Name";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Arial", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(241, 9);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(200, 33);
            label1.TabIndex = 22;
            label1.Text = "Sign Up Form";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(166, 261);
            textBox4.Margin = new Padding(4, 3, 4, 3);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(350, 30);
            textBox4.TabIndex = 42;
            textBox4.TextChanged += textBox4_TextChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(166, 232);
            label9.Margin = new Padding(4, 0, 4, 0);
            label9.Name = "label9";
            label9.Size = new Size(133, 23);
            label9.TabIndex = 41;
            label9.Text = "PhoneNumber";
            // 
            // SignUp
            // 
            AutoScaleDimensions = new SizeF(11F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.GradientInactiveCaption;
            ClientSize = new Size(701, 825);
            Controls.Add(textBox4);
            Controls.Add(label9);
            Controls.Add(checkShowPass);
            Controls.Add(textBox3);
            Controls.Add(label8);
            Controls.Add(AdminKEY);
            Controls.Add(key);
            Controls.Add(RoleCb);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(dateTimePicker1);
            Controls.Add(textBox2);
            Controls.Add(textBox1);
            Controls.Add(button1);
            Controls.Add(label5);
            Controls.Add(radioButton2);
            Controls.Add(radioButton1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(4, 3, 4, 3);
            Name = "SignUp";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SignUp";
            Load += SignUp_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox checkShowPass;
        private TextBox textBox3;
        private Label label8;
        private TextBox AdminKEY;
        private Label key;
        private ComboBox RoleCb;
        private Label label7;
        private Label label6;
        private DateTimePicker dateTimePicker1;
        private TextBox textBox2;
        private TextBox textBox1;
        private Button button1;
        private Label label5;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label label1;
        private TextBox textBox4;
        private Label label9;
    }
}