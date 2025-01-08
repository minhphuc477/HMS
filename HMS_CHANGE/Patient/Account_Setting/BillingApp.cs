using BusinessLL;
using DataTransferO;
using DevExpress.CodeParser;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMS_CHANGE.Patient.Account_Setting
{
    public partial class BillingApp : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly PaymentAndBillingService _paymentAndBillingService;
        private readonly UserService _userService;
        private readonly DoctorService _doctorService;
        private readonly AppointmentValidationService _appointmentValidationService;
        private readonly SePayService _sePayService;
        private UserDTO? _currentUser;

        public BillingApp(IServiceProvider serviceProvider, PaymentAndBillingService paymentAndBillingService, UserService userService, DoctorService doctorService, AppointmentValidationService appointmentValidationService, SePayService sePayService)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _paymentAndBillingService = paymentAndBillingService;
            _userService = userService;
            _doctorService = doctorService;
            _appointmentValidationService = appointmentValidationService;
            _sePayService = sePayService;

            // Initialize the SearchLookUpEdit controls
            InitializeSearchLookUpEditPayments();
            InitializeSearchLookUpEditBillings();
            InitializeSearchLookUpEditMedicalRecords();
            InitializeSearchLookUpEditPrescriptions();

            _currentUser = UserSession.CurrentUser;
            label20.Text = _currentUser.Name;
        }

        public async void SetUserData(UserDTO user)
        {
            _currentUser = user;
            await LoadPayments();
            await LoadBillings();
            await LoadMedicalRecords();
            await LoadPrescriptions();
        }

        private void InitializeSearchLookUpEditPayments()
        {
            searchLookUpEdit1.Properties.DisplayMember = "PaymentDate";
            searchLookUpEdit1.Properties.ValueMember = "PaymentId";

            GridView view = searchLookUpEdit1.Properties.View;
            view.Columns.Clear();
            view.Columns.AddVisible("PaymentDate", "Payment Date");
            view.Columns.AddVisible("Amount", "Amount");
            view.Columns.AddVisible("PaymentMethod", "Payment Method");

            searchLookUpEdit1.EditValueChanged += searchLookUpEdit1_EditValueChanged;
        }

        private void InitializeSearchLookUpEditBillings()
        {
            searchLookUpEdit2.Properties.DisplayMember = "IssuedDate";
            searchLookUpEdit2.Properties.ValueMember = "BillId";

            GridView view = searchLookUpEdit2.Properties.View;
            view.Columns.Clear();
            view.Columns.AddVisible("IssuedDate", "Issued Date");
            view.Columns.AddVisible("TotalAmount", "Total Amount");
            view.Columns.AddVisible("Status", "Status");

            searchLookUpEdit2.EditValueChanged += searchLookUpEdit2_EditValueChanged;
        }

        private void InitializeSearchLookUpEditMedicalRecords()
        {
            searchLookUpEdit3.Properties.DisplayMember = "CreatedAt";
            searchLookUpEdit3.Properties.ValueMember = "MedicalRecordId";

            GridView view = searchLookUpEdit3.Properties.View;
            view.Columns.Clear();
            view.Columns.AddVisible("CreatedAt", "Created At");
            view.Columns.AddVisible("Diagnosis", "Diagnosis");

            searchLookUpEdit3.EditValueChanged += searchLookUpEdit3_EditValueChangedAsync;
        }

        private void InitializeSearchLookUpEditPrescriptions()
        {
            searchLookUpEdit3.EditValueChanged += async (sender, e) => searchLookUpEdit3_EditValueChangedAsync(sender, e);
            searchLookUpEdit4.Properties.DisplayMember = "Product.Name";
            searchLookUpEdit4.Properties.ValueMember = "PrescriptionDetailId";

            GridView view = searchLookUpEdit4.Properties.View;
            view.Columns.Clear();
            view.Columns.AddVisible("Product.Name", "Product Name");
            view.Columns.AddVisible("Quantity", "Quantity");
            view.Columns.AddVisible("DurationDays", "Duration Days");

            searchLookUpEdit4.EditValueChanged += searchLookUpEdit4_EditValueChanged;
        }

        private async Task LoadPayments()
        {
            try
            {
                var payments = await _paymentAndBillingService.GetPaymentsAsync();
                searchLookUpEdit1.Properties.DataSource = payments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load payments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadBillings()
        {
            try
            {
                var billings = await _paymentAndBillingService.GetBillingsAsync();
                searchLookUpEdit2.Properties.DataSource = billings;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load billings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadMedicalRecords()
        {
            try
            {
                var medicalRecords = await _paymentAndBillingService.GetMedicalRecordsAsync();
                searchLookUpEdit3.Properties.DataSource = medicalRecords;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load medical records: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadPrescriptions()
        {
            try
            {
                var prescriptions = await _paymentAndBillingService.GetPrescriptionDetailsAsync();
                searchLookUpEdit4.Properties.DataSource = prescriptions;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load prescriptions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void searchLookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {
            if (searchLookUpEdit1.EditValue == null)
            {
                MessageBox.Show("Please select a payment from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Guid selectedPaymentId = (Guid)searchLookUpEdit1.EditValue;
            await LoadPaymentDetails(selectedPaymentId);
        }

        private async Task LoadPaymentDetails(Guid paymentId)
        {
            try
            {
                var payment = await _paymentAndBillingService.GetPaymentByIdAsync(paymentId);
                if (payment != null)
                {
                    if (payment.OrderId.HasValue)
                    {
                        var order = await _paymentAndBillingService.GetOrderByIdAsync(payment.OrderId.Value);
                        if (order != null)
                        {
                            textBox2.Text = $"Amount: {payment.Amount}";
                            textBoxAppointmentDetails_past.Text = $"Payment Method: {payment.PaymentMethod}{Environment.NewLine}" +
                                                                  $"Payment Date: {payment.PaymentDate}{Environment.NewLine}" +
                                                                  $"Doctor Fee: {order.DoctorFee}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load payment details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void searchLookUpEdit2_EditValueChanged(object sender, EventArgs e)
        {
            if (searchLookUpEdit2.EditValue == null)
            {
                MessageBox.Show("Please select a billing from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Guid selectedBillingId = (Guid)searchLookUpEdit2.EditValue;
            LoadBillingDetails(selectedBillingId);
        }

        private void LoadBillingDetails(Guid billingId)
        {
            var selectedBilling = ((List<BillingDTO>)searchLookUpEdit2.Properties.DataSource)
                .FirstOrDefault(b => b.BillId == billingId);

            if (selectedBilling != null)
            {
                textBox1.Text = $"Issued Date: {selectedBilling.IssuedDate}{Environment.NewLine}" +
                                $"Total Amount: {selectedBilling.TotalAmount}{Environment.NewLine}" +
                                $"Status: {selectedBilling.Status}{Environment.NewLine}" +
                                $"Due Date: {selectedBilling.DueDate}";
            }
        }

        private async void searchLookUpEdit3_EditValueChangedAsync(object sender, EventArgs e)
        {
            if (searchLookUpEdit3.EditValue == null)
            {
                MessageBox.Show("Please select a medical record from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Guid selectedMedicalRecordId = (Guid)searchLookUpEdit3.EditValue;
            await LoadMedicalRecordDetails(selectedMedicalRecordId);
        }

        private async Task LoadMedicalRecordDetails(Guid medicalRecordId)
        {
            var selectedMedicalRecord = ((List<MedicalRecordDTO>)searchLookUpEdit3.Properties.DataSource)
                .FirstOrDefault(mr => mr.MedicalRecordId == medicalRecordId);

            if (selectedMedicalRecord != null)
            {
                var doctor = await _paymentAndBillingService.GetUserByIdAsync(selectedMedicalRecord.DoctorId);
                var patient = await _paymentAndBillingService.GetUserByIdAsync(selectedMedicalRecord.PatientId);

                textBox4.Text = $"Created At: {selectedMedicalRecord.CreatedAt}{Environment.NewLine}" +
                                $"Diagnosis: {selectedMedicalRecord.Diagnosis}{Environment.NewLine}" +
                                $"Doctor: {doctor?.Name ?? "Unknown"}{Environment.NewLine}" +
                                $"Patient: {patient?.Name ?? "Unknown"}";
            }
        }

        private void searchLookUpEdit4_EditValueChanged(object sender, EventArgs e)
        {
            if (searchLookUpEdit4.EditValue == null)
            {
                MessageBox.Show("Please select a prescription from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Guid selectedPrescriptionDetailId = (Guid)searchLookUpEdit4.EditValue;
            LoadPrescriptionDetailDetails(selectedPrescriptionDetailId);
        }

        private void LoadPrescriptionDetailDetails(Guid prescriptionDetailId)
        {
            var selectedPrescriptionDetail = ((List<PrescriptionDetailDTO>)searchLookUpEdit4.Properties.DataSource)
                .FirstOrDefault(pd => pd.PrescriptionDetailId == prescriptionDetailId);

            if (selectedPrescriptionDetail != null)
            {
                textBox3.Text = $"Product Name: {selectedPrescriptionDetail.Product.Name}{Environment.NewLine}" +
                                $"Quantity: {selectedPrescriptionDetail.Quantity}{Environment.NewLine}" +
                                $"Duration Days: {selectedPrescriptionDetail.DurationDays}{Environment.NewLine}" +
                                $"Notes: {selectedPrescriptionDetail.Notes}";
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // This is the textbox for the searchLookUpEdit1_EditValueChanged method
            // This will hold the amount to be paid (the total amount that includes the doctor fee)
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // This combobox is for the payment method
            // Include the 3 payment methods "CreditCard", "Cash", "Internet Banking"
        }

        private void textBoxAppointmentDetails_past_TextChanged(object sender, EventArgs e)
        {
            // This is where the past Payment details will be displayed
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // This is where the billing details will be displayed
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (searchLookUpEdit1.EditValue == null)
            {
                MessageBox.Show("Please select a payment from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Guid selectedPaymentId = (Guid)searchLookUpEdit1.EditValue;
            var payment = await _paymentAndBillingService.GetPaymentByIdAsync(selectedPaymentId);

            if (payment == null)
            {
                MessageBox.Show("Payment not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (payment.PaymentStatus == "Paid")
            {
                MessageBox.Show("This payment is already marked as paid.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (payment.PaymentStatus == "Failed" || payment.PaymentStatus == "Pending")
            {
                // Proceed with the payment process
                try
                {
                    string selectedPaymentMethod = comboBox1.SelectedItem.ToString();

                    if (selectedPaymentMethod == "CreditCard" || selectedPaymentMethod == "Cash")
                    {
                        payment.PaymentStatus = "Paid";
                        await _paymentAndBillingService.UpdatePaymentAsync(payment);

                        // Update the billing status as well
                        var billing = await _paymentAndBillingService.GetBillingByIdAsync(payment.OrderId.Value);
                        if (billing != null)
                        {
                            billing.Status = "Paid";
                            await _paymentAndBillingService.UpdateBillingAsync(billing);
                        }

                        MessageBox.Show("Payment successful.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (selectedPaymentMethod == "Internet Banking")
                    {
                        // Forward to PaymentByApp form with payment details
                        PaymentByApp paymentByAppForm = new PaymentByApp(selectedPaymentId, payment.Amount, _sePayService);
                        paymentByAppForm.Show();
                    }
                    else
                    {
                        MessageBox.Show("Payment failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while processing the payment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }
    }
}
