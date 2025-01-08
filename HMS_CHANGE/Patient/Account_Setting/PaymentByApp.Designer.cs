namespace HMS_CHANGE.Patient.Account_Setting
{
    partial class PaymentByApp
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaymentByApp));
            billingBindingSource = new BindingSource(components);
            pictureBox2 = new PictureBox();
            paymentBindingSource = new BindingSource(components);
            medicalRecordBindingSource = new BindingSource(components);
            panel1 = new Panel();
            panel4 = new Panel();
            label3 = new Label();
            textBox3 = new TextBox();
            label2 = new Label();
            labelPaymentId = new TextBox();
            label1 = new Label();
            button2 = new Button();
            labelAmount = new TextBox();
            label22 = new Label();
            label4 = new Label();
            prescriptionDetailBindingSource = new BindingSource(components);
            ((System.ComponentModel.ISupportInitialize)billingBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)paymentBindingSource).BeginInit();
            ((System.ComponentModel.ISupportInitialize)medicalRecordBindingSource).BeginInit();
            panel1.SuspendLayout();
            panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)prescriptionDetailBindingSource).BeginInit();
            SuspendLayout();
            // 
            // billingBindingSource
            // 
            billingBindingSource.DataSource = typeof(DataAL.Models.Billing);
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(1517, 12);
            pictureBox2.Margin = new Padding(4, 3, 4, 3);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(35, 43);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 30;
            pictureBox2.TabStop = false;
            // 
            // paymentBindingSource
            // 
            paymentBindingSource.DataSource = typeof(DataAL.Models.Payment);
            // 
            // medicalRecordBindingSource
            // 
            medicalRecordBindingSource.DataSource = typeof(DataAL.Models.MedicalRecord);
            // 
            // panel1
            // 
            panel1.BackColor = SystemColors.GradientInactiveCaption;
            panel1.Controls.Add(pictureBox2);
            panel1.Controls.Add(panel4);
            panel1.Dock = DockStyle.Fill;
            panel1.Font = new Font("Arial", 12F);
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1058, 515);
            panel1.TabIndex = 6;
            panel1.Paint += panel1_Paint;
            // 
            // panel4
            // 
            panel4.BackColor = SystemColors.Control;
            panel4.Controls.Add(label3);
            panel4.Controls.Add(textBox3);
            panel4.Controls.Add(label2);
            panel4.Controls.Add(labelPaymentId);
            panel4.Controls.Add(label1);
            panel4.Controls.Add(button2);
            panel4.Controls.Add(labelAmount);
            panel4.Controls.Add(label22);
            panel4.Controls.Add(label4);
            panel4.Font = new Font("Arial", 12F);
            panel4.Location = new Point(0, 0);
            panel4.Margin = new Padding(4, 3, 4, 3);
            panel4.Name = "panel4";
            panel4.Size = new Size(657, 515);
            panel4.TabIndex = 2;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(21, 250);
            label3.Name = "label3";
            label3.Size = new Size(66, 23);
            label3.TabIndex = 28;
            label3.Text = "Status";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(21, 276);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(255, 30);
            textBox3.TabIndex = 27;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(21, 83);
            label2.Name = "label2";
            label2.Size = new Size(139, 23);
            label2.TabIndex = 26;
            label2.Text = "AppointmentID";
            // 
            // labelPaymentId
            // 
            labelPaymentId.Location = new Point(21, 109);
            labelPaymentId.Name = "labelPaymentId";
            labelPaymentId.Size = new Size(255, 30);
            labelPaymentId.TabIndex = 25;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(21, 169);
            label1.Name = "label1";
            label1.Size = new Size(82, 23);
            label1.TabIndex = 24;
            label1.Text = "Amount:";
            // 
            // button2
            // 
            button2.BackColor = Color.Aquamarine;
            button2.Location = new Point(235, 422);
            button2.Name = "button2";
            button2.Size = new Size(76, 42);
            button2.TabIndex = 23;
            button2.Text = "Pay";
            button2.UseVisualStyleBackColor = false;
            // 
            // labelAmount
            // 
            labelAmount.Location = new Point(21, 195);
            labelAmount.Name = "labelAmount";
            labelAmount.Size = new Size(255, 30);
            labelAmount.TabIndex = 21;
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(151, 467);
            label22.Name = "label22";
            label22.Size = new Size(254, 23);
            label22.TabIndex = 19;
            label22.Text = "Back to Payment and Billing";
            label22.Click += label22_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Arial", 16.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.Location = new Point(221, 27);
            label4.Name = "label4";
            label4.Size = new Size(132, 32);
            label4.TabIndex = 0;
            label4.Text = "Payment ";
            // 
            // prescriptionDetailBindingSource
            // 
            prescriptionDetailBindingSource.DataSource = typeof(DataAL.Models.PrescriptionDetail);
            // 
            // PaymentByApp
            // 
            AutoScaleDimensions = new SizeF(11F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1058, 515);
            Controls.Add(panel1);
            Font = new Font("Arial", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4, 3, 4, 3);
            Name = "PaymentByApp";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PaymentByApp";
            ((System.ComponentModel.ISupportInitialize)billingBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)paymentBindingSource).EndInit();
            ((System.ComponentModel.ISupportInitialize)medicalRecordBindingSource).EndInit();
            panel1.ResumeLayout(false);
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)prescriptionDetailBindingSource).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private BindingSource billingBindingSource;
        private PictureBox pictureBox2;
        private BindingSource paymentBindingSource;
        private BindingSource medicalRecordBindingSource;
        private Panel panel1;
        private BindingSource prescriptionDetailBindingSource;
        private Panel panel4;
        private Button button2;
        private TextBox labelAmount;
        private Label label22;
        private Label label4;
        private Label label3;
        private TextBox textBox3;
        private Label label2;
        private TextBox labelPaymentId;
        private Label label1;
    }
}