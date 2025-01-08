using BusinessLL;
using DataTransferO;
using HMS_CHANGE.Dashboard;
using Microsoft.Extensions.DependencyInjection;
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
    public partial class OrderAppointment : Form
    {
        private readonly CheckUpService _checkUpService;
        private readonly PaymentAndBillingService _paymentAndBillingService;
        private readonly IServiceProvider _serviceProvider;
        private Guid _doctorId;
        private List<Guid> _appointmentIds;
        private UserDTO? _currentUser;
        private UserDTO? _appointmentUser;
        private DoctorDetailDTO _currentDoctorDetail;

        public OrderAppointment(CheckUpService checkUpService, PaymentAndBillingService paymentAndBillingService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _checkUpService = checkUpService;
            _paymentAndBillingService = paymentAndBillingService;
            _serviceProvider = serviceProvider;

            // Initialize DataGridView columns
            InitializeOrderDataGridView();
            InitializePaymentDataGridView();

            // Load data into DataGridViews
            LoadOrders();
            LoadPayments();

            _currentUser = UserSession.CurrentUser;
            label2.Text = _currentUser.Name;
        }

        public void SetOrderDetails(Guid doctorId, List<Guid> appointmentIds, UserDTO currentUser, UserDTO? appointmentUser)
        {
            _doctorId = doctorId;
            _appointmentIds = appointmentIds;
            _currentUser = currentUser;
            _appointmentUser = appointmentUser;

            // Load doctor details
            LoadDoctorDetails();

            // Load data into DataGridViews
            LoadOrders();
            LoadPayments();
        }

        private async void LoadDoctorDetails()
        {
            if (_currentUser != null)
            {
                _currentDoctorDetail = await _serviceProvider.GetRequiredService<DoctorService>().GetDoctorByUserIdAsync(_currentUser.UserId);
            }
            else
            {
                MessageBox.Show("Current user is not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeOrderDataGridView()
        {
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OrderId",
                HeaderText = "Order ID",
                DataPropertyName = "OrderId",
                Visible = false // Hide the column if you don't want it to be visible
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "OrderDate",
                HeaderText = "Order Date",
                DataPropertyName = "OrderDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "TotalAmount",
                HeaderText = "Total Amount",
                DataPropertyName = "TotalAmount",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                DataPropertyName = "Status",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UserName",
                HeaderText = "User Name",
                DataPropertyName = "UserName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void InitializePaymentDataGridView()
        {
            dataGridView2.Columns.Clear();

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentId",
                HeaderText = "Payment ID",
                DataPropertyName = "PaymentId",
                Visible = false // Hide the column if you don't want it to be visible
            });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentDate",
                HeaderText = "Payment Date",
                DataPropertyName = "PaymentDate",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Amount",
                HeaderText = "Amount",
                DataPropertyName = "Amount",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentMethod",
                HeaderText = "Payment Method",
                DataPropertyName = "PaymentMethod",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PaymentStatus",
                HeaderText = "Payment Status",
                DataPropertyName = "PaymentStatus",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UserName",
                HeaderText = "User Name",
                DataPropertyName = "UserName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private async void LoadOrders()
        {
            try
            {
                var orders = await _paymentAndBillingService.GetOrdersAsync();
                var ordersWithUserNames = new List<dynamic>();

                foreach (var order in orders)
                {
                    var user = await _paymentAndBillingService.GetUserByIdAsync(order.PatientId);
                    ordersWithUserNames.Add(new
                    {
                        order.OrderId,
                        order.OrderDate,
                        order.TotalAmount,
                        order.Status,
                        UserName = user?.Name
                    });
                }

                dataGridView1.DataSource = ordersWithUserNames;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load orders: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadPayments()
        {
            try
            {
                var payments = await _paymentAndBillingService.GetPaymentsAsync();
                var paymentsWithUserNames = new List<dynamic>();

                foreach (var payment in payments)
                {
                    var user = await _paymentAndBillingService.GetUserByIdAsync(payment.UserId);
                    paymentsWithUserNames.Add(new
                    {
                        payment.PaymentId,
                        payment.PaymentDate,
                        payment.Amount,
                        payment.PaymentMethod,
                        payment.PaymentStatus,
                        UserName = user?.Name
                    });
                }

                dataGridView2.DataSource = paymentsWithUserNames;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load payments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Handle cell content click for payments DataGridView
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Handle cell content click for orders DataGridView
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Order appointment change status to "Delivered"
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0];
                if (selectedRow != null)
                {
                    var orderIdCell = selectedRow.Cells["OrderId"];
                    if (orderIdCell != null && orderIdCell.Value != null)
                    {
                        var orderId = (Guid)orderIdCell.Value;

                        var order = await _paymentAndBillingService.GetOrderByIdAsync(orderId);
                        if (order != null)
                        {
                            order.Status = "Delivered";
                            await _paymentAndBillingService.UpdateOrderAsync(order);
                            MessageBox.Show("Order status updated to Delivered.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadOrders();
                        }
                        else
                        {
                            MessageBox.Show("Order not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Order ID is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No row selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No row selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            // Order appointment change status to "Cancelled"
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0];
                if (selectedRow != null)
                {
                    var orderIdCell = selectedRow.Cells["OrderId"];
                    if (orderIdCell != null && orderIdCell.Value != null)
                    {
                        var orderId = (Guid)orderIdCell.Value;

                        var order = await _paymentAndBillingService.GetOrderByIdAsync(orderId);
                        if (order != null)
                        {
                            order.Status = "Canceled";
                            await _paymentAndBillingService.UpdateOrderAsync(order);
                            MessageBox.Show("Order status updated to Cancelled.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadOrders();
                        }
                        else
                        {
                            MessageBox.Show("Order not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Order ID is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No row selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No row selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            // payment appointment change status to "Paid" -> the billing status of the order should also be Paid
            if (dataGridView2.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView2.SelectedRows[0];
                if (selectedRow != null)
                {
                    var paymentIdCell = selectedRow.Cells["PaymentId"];
                    if (paymentIdCell != null && paymentIdCell.Value != null)
                    {
                        var paymentId = (Guid)paymentIdCell.Value;

                        var payment = await _paymentAndBillingService.GetPaymentByIdAsync(paymentId);
                        if (payment != null)
                        {
                            payment.PaymentStatus = "Paid";
                            await _paymentAndBillingService.UpdatePaymentAsync(payment);

                            if (payment.OrderId.HasValue)
                            {
                                var billing = await _paymentAndBillingService.GetBillingByIdAsync(payment.OrderId.Value);
                                if (billing != null)
                                {
                                    billing.Status = "Paid";
                                    await _paymentAndBillingService.UpdateBillingAsync(billing);
                                }
                            }

                            MessageBox.Show("Payment status updated to Paid.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadPayments();
                        }
                        else
                        {
                            MessageBox.Show("Payment not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Payment ID is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No row selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No row selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            // payment appointment change status to "Failed" --> the billing status of the order should also be Unpaid
            if (dataGridView2.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView2.SelectedRows[0];
                if (selectedRow != null)
                {
                    var paymentIdCell = selectedRow.Cells["PaymentId"];
                    if (paymentIdCell != null && paymentIdCell.Value != null)
                    {
                        var paymentId = (Guid)paymentIdCell.Value;

                        var payment = await _paymentAndBillingService.GetPaymentByIdAsync(paymentId);
                        if (payment != null)
                        {
                            payment.PaymentStatus = "Failed";
                            await _paymentAndBillingService.UpdatePaymentAsync(payment);

                            if (payment.OrderId.HasValue)
                            {
                                var billing = await _paymentAndBillingService.GetBillingByIdAsync(payment.OrderId.Value);
                                if (billing != null)
                                {
                                    billing.Status = "Unpaid";
                                    await _paymentAndBillingService.UpdateBillingAsync(billing);
                                }
                            }

                            MessageBox.Show("Payment status updated to Failed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadPayments();
                        }
                        else
                        {
                            MessageBox.Show("Payment not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Payment ID is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No row selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No row selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void label24_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Current user is not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var detailsAppointmentForm = _serviceProvider.GetRequiredService<DetailsAppointment>();
            await detailsAppointmentForm.SetOrderDetailsAsync(_doctorId, _appointmentIds, _currentUser, _appointmentUser);
            detailsAppointmentForm.Show();
            this.Hide();
        }

        private void label14_Click(object sender, EventArgs e)
        {
            var dashBoardDoctorForm = _serviceProvider.GetRequiredService<DashBoardDoctor>();
            dashBoardDoctorForm.SetDoctorId(_doctorId);
            dashBoardDoctorForm.Show();
            this.Hide();
        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("Current user is not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var accountDocForm = _serviceProvider.GetRequiredService<AccountDoc>();
            accountDocForm.SetUserData(_currentUser, _currentDoctorDetail);
            accountDocForm.Show();
            this.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
