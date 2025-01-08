using BusinessLL;
using DataTransferO;
using HMS_CHANGE.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMS_CHANGE.Doctor
{
    public partial class DetailsAppointment : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppointmentService _appointmentService;
        private readonly CheckUpService _checkUpService;
        private UserDTO? _currentUser;
        private Guid _doctorId;
        private List<Guid> _appointmentIds;
        private UserDTO? _appointmentUser; // Store the patient/user details

        public DetailsAppointment(IServiceProvider serviceProvider, AppointmentService appointmentService, CheckUpService checkUpService)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _appointmentService = appointmentService;
            _checkUpService = checkUpService;

            // Initialize the SearchLookUpEdit controls
            InitializeSearchLookUpEdit();
            InitializeSearchLookUpEdit2();
            InitializeSearchLookUpEdit3();

            // Initialize DataGridView columns
            InitializeDataGridViewColumns();

            // Add event handlers for auto-calculation
            textBox2.TextChanged += (s, e) => CalculateTotalAmount();
            textBox3.TextChanged += (s, e) => CalculateTotalAmount();

            _currentUser = UserSession.CurrentUser;
            label2.Text = _currentUser.Name;
        }

        private void InitializeDataGridViewColumns()
        {
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProductName",
                HeaderText = "Product Name",
                DataPropertyName = "ProductName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Quantity",
                HeaderText = "Quantity",
                DataPropertyName = "Quantity",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Price",
                HeaderText = "Price",
                DataPropertyName = "Price",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private async void InitializeSearchLookUpEdit()
        {
            // Initialize the SearchLookUpEdit control
            searchLookUpEdit1.Properties.DisplayMember = "FormattedAppointmentDate";
            searchLookUpEdit1.Properties.ValueMember = "AppointmentId";

            // Configure the view
            var view = searchLookUpEdit1.Properties.View;
            view.Columns.Clear(); // Clear existing columns if any
            view.Columns.AddVisible("FormattedAppointmentDate", "Appointment Date");
            view.Columns.AddVisible("Doctor.DoctorName", "Doctor Name");
            view.Columns.AddVisible("Department.DepartmentName", "Department");

            // Load confirmed appointments
            await LoadConfirmedAppointmentsAsync();

            // Handle the EditValueChanged event
            searchLookUpEdit1.EditValueChanged += searchLookUpEdit1_EditValueChanged;
        }

        private async void InitializeSearchLookUpEdit2()
        {
            // Initialize the SearchLookUpEdit control for diseases
            searchLookUpEdit2.Properties.DisplayMember = "DiseaseName";
            searchLookUpEdit2.Properties.ValueMember = "DiseaseId";

            // Configure the view
            var view = searchLookUpEdit2.Properties.View;
            view.Columns.Clear(); // Clear existing columns if any
            view.Columns.AddVisible("DiseaseName", "Disease Name");

            // Load diseases
            await LoadDiseasesAsync();

            // Handle the EditValueChanged event
            searchLookUpEdit2.EditValueChanged += searchLookUpEdit2_EditValueChanged;
        }

        private async void InitializeSearchLookUpEdit3()
        {
            // Initialize the SearchLookUpEdit control for products
            searchLookUpEdit3.Properties.DisplayMember = "Name";
            searchLookUpEdit3.Properties.ValueMember = "ProductId";

            // Configure the view
            var view = searchLookUpEdit3.Properties.View;
            view.Columns.Clear(); // Clear existing columns if any
            view.Columns.AddVisible("Name", "Product Name");
            view.Columns.AddVisible("Price", "Price");

            // Load products
            await LoadProductsAsync();

            // Handle the EditValueChanged event
            searchLookUpEdit3.EditValueChanged += searchLookUpEdit3_EditValueChanged;
        }

        private async Task LoadConfirmedAppointmentsAsync()
        {
            try
            {
                var confirmedAppointments = await _appointmentService.GetConfirmedAppointmentsAsync();
                searchLookUpEdit1.Properties.DataSource = confirmedAppointments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load confirmed appointments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadDiseasesAsync()
        {
            var diseases = await _serviceProvider.GetRequiredService<DoctorService>().GetAllDiseasesAsync();
            searchLookUpEdit2.Properties.DataSource = diseases;
        }

        private async Task LoadProductsAsync()
        {
            var products = await _serviceProvider.GetRequiredService<PaymentAndBillingService>().GetAllProductsAsync();
            searchLookUpEdit3.Properties.DataSource = products;
        }

        private async void searchLookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {
            if (searchLookUpEdit1.EditValue == null)
            {
                MessageBox.Show("Please select an appointment from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Handle the case when an appointment is selected
            Guid selectedAppointmentId = (Guid)searchLookUpEdit1.EditValue;
            await LoadAppointmentDetailsAsync(selectedAppointmentId);
        }

        private async void searchLookUpEdit2_EditValueChanged(object sender, EventArgs e)
        {
            if (searchLookUpEdit2.EditValue == null)
            {
                return;
            }

            // Call the disease to select that belongs to the department of the doctor in current log in
            var selectedDiseaseId = (Guid)searchLookUpEdit2.EditValue;
            await LoadDiseaseDetails(selectedDiseaseId);
        }

        private async void searchLookUpEdit3_EditValueChanged(object sender, EventArgs e)
        {
            if (searchLookUpEdit3.EditValue == null)
            {
                return;
            }

            // Call the product details
            var selectedProductId = (Guid)searchLookUpEdit3.EditValue;
            await LoadProductDetails(selectedProductId);
        }

        private async Task LoadAppointmentDetailsAsync(Guid appointmentId)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);

            if (appointment != null)
            {
                // Store the patient/user details
                _appointmentUser = appointment.Patient;

                // Display the appointment details in the textBoxAppointmentDetails_past
                textBoxAppointmentDetails_past.Text = $"Appointment Date: {appointment.AppointmentDate}{Environment.NewLine}" +
                                                      $"Doctor: {appointment.Doctor?.User?.Name ?? "Unknown"}{Environment.NewLine}" +
                                                      $"Department: {appointment.Department?.DepartmentName ?? "Unknown"}{Environment.NewLine}" +
                                                      $"Notes: {appointment.Notes ?? string.Empty}";

                // Display the user details who booked the confirmed appointment in textBox1
                textBox1.Text = $"User Name: {appointment.Patient?.Name ?? "Unknown"}{Environment.NewLine}" +
                                $"Email: {appointment.Patient?.Email ?? "Unknown"}{Environment.NewLine}" +
                                $"Phone: {appointment.Patient?.PhoneNumber ?? "Unknown"}{Environment.NewLine}" +
                                $"Gender: {appointment.Patient?.Gender ?? "Unknown"}";
            }
            else
            {
                MessageBox.Show("Appointment not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async void SetAppointmentDetails(Guid doctorId, List<Guid> appointmentIds, UserDTO currentUser)
        {
            _doctorId = doctorId;
            _appointmentIds = appointmentIds;
            _currentUser = currentUser;

            // Load the details of the first appointment as an example
            if (_appointmentIds.Any())
            {
                await LoadAppointmentDetailsAsync(_appointmentIds.First());
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void textBoxAppointmentDetails_past_TextChanged(object sender, EventArgs e)
        {
            // Intentionally left empty
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Intentionally left empty
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // Quantity that the product will have
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            // See the disease name
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // Doctor diagnosis
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // Doctor fee for the appointment
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            // Calculate the total amount of the appointment
            // The total amount will be the sum of the product price and the doctor fee
            CalculateTotalAmount();
        }

        private async Task LoadProductDetails(Guid productId)
        {
            var product = await _serviceProvider.GetRequiredService<PaymentAndBillingService>().GetPrescriptionDetailByIdAsync(productId);
            if (product != null)
            {
                textBox2.Text = product.Quantity.ToString();
                textBox6.Text = product.Product.Name;
                textBox4.Text = product.MedicalRecord.Diagnosis;
                textBox3.Text = product.Product.Price.ToString("C");
            }
        }

        private async Task LoadDiseaseDetails(Guid diseaseId)
        {
            var disease = await _serviceProvider.GetRequiredService<DoctorService>().GetDiseaseByIdAsync(diseaseId);
            if (disease != null)
            {
                textBox6.Text = disease.DiseaseName;
            }
        }

        private void CalculateTotalAmount()
        {
            decimal totalAmount = 0;

            // Sum the total amount from the DataGridView
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["Quantity"].Value != null && row.Cells["Price"].Value != null)
                {
                    if (int.TryParse(row.Cells["Quantity"].Value.ToString(), out int quantity) &&
                        decimal.TryParse(row.Cells["Price"].Value.ToString(),
                                         System.Globalization.NumberStyles.Currency,
                                         System.Globalization.CultureInfo.CurrentCulture,
                                         out decimal price))
                    {
                        totalAmount += quantity * price;
                    }
                }
            }

            // Add the doctor fee
            if (decimal.TryParse(textBox3.Text, out var doctorFee))
            {
                totalAmount += doctorFee;
            }

            textBox5.Text = totalAmount.ToString("C");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //show the product details
            // name, quantity, price

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (searchLookUpEdit3.EditValue == null)
            {
                MessageBox.Show("Please select a product from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedProductId = (Guid)searchLookUpEdit3.EditValue;
            var products = (List<PharmacyProductDTO>)searchLookUpEdit3.Properties.DataSource;
            var product = products.FirstOrDefault(p => p.ProductId == selectedProductId);

            if (product != null)
            {
                if (int.TryParse(textBox2.Text, out int quantity))
                {
                    dataGridView1.Rows.Add(product.Name, quantity, product.Price.ToString("C"));
                    CalculateTotalAmount(); // Recalculate total amount after adding a new product
                }
                else
                {
                    MessageBox.Show("Please enter a valid quantity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Selected product not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    dataGridView1.Rows.Remove(row);
                }
                CalculateTotalAmount(); // Recalculate total amount after removing a product
            }
            else
            {
                MessageBox.Show("Please select a product to remove.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Clear all text boxes
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            textBoxAppointmentDetails_past.Clear();

            // Clear DataGridView
            dataGridView1.Rows.Clear();
        }

        private async void button4_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (_appointmentUser == null)
                {
                    MessageBox.Show("No user information available for the selected appointment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get the selected appointment
                if (searchLookUpEdit1.EditValue == null)
                {
                    MessageBox.Show("Please select an appointment from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Guid selectedAppointmentId = (Guid)searchLookUpEdit1.EditValue;
                var appointment = await _appointmentService.GetAppointmentByIdAsync(selectedAppointmentId);

                if (appointment == null)
                {
                    MessageBox.Show("Appointment not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get the selected disease
                if (searchLookUpEdit2.EditValue == null)
                {
                    MessageBox.Show("Please select a disease from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Guid selectedDiseaseId = (Guid)searchLookUpEdit2.EditValue;

                // Create prescription details from DataGridView
                var prescriptions = new List<PrescriptionDetailDTO>();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["ProductName"].Value != null && row.Cells["Quantity"].Value != null && row.Cells["Price"].Value != null)
                    {
                        var product = ((List<PharmacyProductDTO>)searchLookUpEdit3.Properties.DataSource)
                            .FirstOrDefault(p => p.Name == row.Cells["ProductName"].Value.ToString());

                        if (product != null)
                        {
                            if (!int.TryParse(row.Cells["Quantity"].Value.ToString(), out int quantity))
                            {
                                MessageBox.Show("Invalid quantity value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            // Check if the quantity is sufficient
                            if (quantity > product.Stock)
                            {
                                MessageBox.Show($"Insufficient stock for product: {product.Name}. Available stock: {product.Stock}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            if (!int.TryParse(textBox8.Text, out int durationDays))
                            {
                                MessageBox.Show("Invalid duration days value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            prescriptions.Add(new PrescriptionDetailDTO
                            {
                                PrescriptionDetailId = Guid.NewGuid(),
                                ProductId = product.ProductId,
                                Quantity = quantity,
                                DurationDays = durationDays, // Use the duration from textBox8
                                Notes = textBox7.Text // Use the note from textBox7
                            });
                        }
                    }
                }

                // Create a new medical record
                var medicalRecordDto = await _checkUpService.PerformCheckUpAsync(
                    appointment.PatientId,
                    _doctorId,
                    textBox4.Text, // Diagnosis
                    selectedDiseaseId,
                    prescriptions,
                    selectedAppointmentId); // Pass the appointmentId

                // Create order details from DataGridView
                var orderDetails = new List<OrderDetailDTO>();
                foreach (var prescription in prescriptions)
                {
                    var product = ((List<PharmacyProductDTO>)searchLookUpEdit3.Properties.DataSource)
                        .FirstOrDefault(p => p.ProductId == prescription.ProductId);

                    if (product != null)
                    {
                        orderDetails.Add(new OrderDetailDTO
                        {
                            OrderDetailId = Guid.NewGuid(),
                            ProductId = prescription.ProductId,
                            Quantity = prescription.Quantity,
                            Price = product.Price
                        });
                    }
                }

                // Create a new order
                var orderDto = await _checkUpService.CreateOrderAsync(
                    appointment.PatientId,
                    orderDetails,
                    decimal.Parse(textBox3.Text), // Doctor fee
                    medicalRecordDto.MedicalRecordId, // PrescriptionId
                    selectedAppointmentId); // Pass the appointmentId

                // Create a new payment
                var paymentDto = await _checkUpService.CreatePaymentAsync(
                    _appointmentUser.UserId, // Use the stored patient/user ID
                    orderDto.OrderId,
                    "CreditCard", // Payment method
                    selectedAppointmentId); // Pass the appointmentId

                MessageBox.Show("Order placed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to place order: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Log the exception
                Log.Error(ex, "Failed to place order");
            }
        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void label25_Click(object sender, EventArgs e)
        {
            // Forward data to OrderAppointment form
            var orderAppointmentForm = _serviceProvider.GetRequiredService<OrderAppointment>();
            if (_currentUser != null)
            {
                orderAppointmentForm.SetOrderDetails(_doctorId, _appointmentIds, _currentUser, _appointmentUser);
                orderAppointmentForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Current user information is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task SetOrderDetailsAsync(Guid doctorId, List<Guid> appointmentIds, UserDTO currentUser, UserDTO? appointmentUser)
        {
            _doctorId = doctorId;
            _appointmentIds = appointmentIds;
            _currentUser = currentUser;
            _appointmentUser = appointmentUser;

            // Load the details of the first appointment as an example
            if (_appointmentIds.Any())
            {
                await LoadAppointmentDetailsAsync(_appointmentIds.First());
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Current user information is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var doctorService = _serviceProvider.GetRequiredService<DoctorService>();
            var doctorDetail = doctorService.GetDoctorByUserIdAsync(_currentUser.UserId).Result;

            if (doctorDetail == null)
            {
                MessageBox.Show("Doctor details not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var accountDocForm = _serviceProvider.GetRequiredService<AccountDoc>();
            accountDocForm.SetUserData(_currentUser, doctorDetail);
            accountDocForm.Show();
            this.Hide();
        }

        private void label14_Click(object sender, EventArgs e)
        {
            var dashBoardDoctorForm = _serviceProvider.GetRequiredService<DashBoardDoctor>();
            dashBoardDoctorForm.SetDoctorId(_doctorId);
            dashBoardDoctorForm.Show();
            this.Hide();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
