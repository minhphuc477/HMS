using BusinessLL;
using DataAL.Models;
using DataTransferO;
using DevExpress.PivotGrid.OLAP.Mdx;
using DevExpress.XtraCharts;
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

namespace HMS_CHANGE.Admin
{
    public partial class RevenuStats : Form
    {
        private readonly PaymentAndBillingService _paymentAndBillingService;
        private readonly DoctorService _doctorService;
        private readonly UserService _userService;
        private readonly AppointmentService _appointmentService;
        private UserDTO _currentUser;
        private IServiceProvider _serviceProvider;

        public RevenuStats(PaymentAndBillingService paymentAndBillingService, DoctorService doctorService, UserService userService, AppointmentService appointmentService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _paymentAndBillingService = paymentAndBillingService;
            _doctorService = doctorService;
            _userService = userService;
            _appointmentService = appointmentService;
            _serviceProvider = serviceProvider;
        }

        public void SetUserData(UserDTO currentUser)
        {
            _currentUser = currentUser;
            label2.Text = currentUser.Name;
            // Use _currentUser as needed within this form
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Intentionally left empty
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private async void chartControl3_Click(object sender, EventArgs e)
        {
            // Revenue Trend Chart: A line chart showing the revenue generated over time.
            try
            {
                var revenueData = await GetRevenueTrendDataAsync();
                if (chartControl3.Series.Count == 0)
                {
                    chartControl3.Series.Add(new Series("Revenue", ViewType.Line));
                }

                chartControl3.Series[0].Points.Clear();
                foreach (var data in revenueData)
                {
                    chartControl3.Series[0].Points.Add(new SeriesPoint(data.Date, data.Revenue));
                }

                // Set chart title and axis labels
                chartControl3.Titles.Clear();
                chartControl3.Titles.Add(new ChartTitle { Text = "Revenue Trend Over Time" });
                ((XYDiagram)chartControl3.Diagram).AxisX.Title.Text = "Date";
                ((XYDiagram)chartControl3.Diagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
                ((XYDiagram)chartControl3.Diagram).AxisY.Title.Text = "Revenue";
                ((XYDiagram)chartControl3.Diagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;

                // Enable tooltips
                chartControl3.ToolTipEnabled = DevExpress.Utils.DefaultBoolean.True;
                chartControl3.Series[0].ToolTipPointPattern = "{A}: {V:C}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load revenue trend data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private async void chartControl1_Click(object sender, EventArgs e)
        {
            // Medication Inventory Chart: A bar chart showing the stock levels of various medications.
            try
            {
                var inventoryData = await GetMedicationInventoryDataAsync();
                if (chartControl1.Series.Count == 0)
                {
                    chartControl1.Series.Add(new Series("Stock Levels", ViewType.Bar));
                }

                chartControl1.Series[0].Points.Clear();
                foreach (var data in inventoryData)
                {
                    chartControl1.Series[0].Points.Add(new SeriesPoint(data.MedicationName, data.StockLevel));
                }

                // Set chart title and axis labels
                chartControl1.Titles.Clear();
                chartControl1.Titles.Add(new ChartTitle { Text = "Medication Inventory Levels" });
                ((XYDiagram)chartControl1.Diagram).AxisX.Title.Text = "Medication";
                ((XYDiagram)chartControl1.Diagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
                ((XYDiagram)chartControl1.Diagram).AxisY.Title.Text = "Stock Level";
                ((XYDiagram)chartControl1.Diagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;

                // Enable tooltips
                chartControl1.ToolTipEnabled = DevExpress.Utils.DefaultBoolean.True;
                chartControl1.Series[0].ToolTipPointPattern = "{A}: {V}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load medication inventory data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void chartControl2_Click(object sender, EventArgs e)
        {
            // Revenue from Pharmacy Sales Chart: A bar chart showing revenue generated from pharmacy sales.
            try
            {
                var salesData = await GetPharmacySalesDataAsync();
                if (chartControl2.Series.Count == 0)
                {
                    chartControl2.Series.Add(new Series("Pharmacy Sales", ViewType.Bar));
                }

                chartControl2.Series[0].Points.Clear();
                foreach (var data in salesData)
                {
                    chartControl2.Series[0].Points.Add(new SeriesPoint(data.MedicationName, data.Revenue));
                }

                // Set chart title and axis labels
                chartControl2.Titles.Clear();
                chartControl2.Titles.Add(new ChartTitle { Text = "Revenue from Pharmacy Sales" });
                ((XYDiagram)chartControl2.Diagram).AxisX.Title.Text = "Medication";
                ((XYDiagram)chartControl2.Diagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
                ((XYDiagram)chartControl2.Diagram).AxisY.Title.Text = "Revenue";
                ((XYDiagram)chartControl2.Diagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;

                // Enable tooltips
                chartControl2.ToolTipEnabled = DevExpress.Utils.DefaultBoolean.True;
                chartControl2.Series[0].ToolTipPointPattern = "{A}: {V:C}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load pharmacy sales data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<List<(DateTime Date, decimal Revenue)>> GetRevenueTrendDataAsync()
        {
            var payments = await _paymentAndBillingService.GetPaymentsAsync();
            var revenueData = payments
                .GroupBy(p => p.PaymentDate?.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(p => p.Amount) })
                .OrderBy(g => g.Date)
                .ToList();

            return revenueData.Select(r => (r.Date.Value, r.Revenue)).ToList();
        }

        private async Task<List<(string DepartmentName, decimal Revenue)>> GetRevenueByDepartmentDataAsync()
        {
            var billings = await _paymentAndBillingService.GetBillingsAsync();
            var revenueData = billings
                .Where(b => b.Order != null && b.Order.Department != null)
                .GroupBy(b => b.Order.Department.DepartmentName)
                .Select(g => new { DepartmentName = g.Key, Revenue = g.Sum(b => b.TotalAmount) })
                .OrderBy(g => g.DepartmentName)
                .ToList();

            return revenueData.Select(r => (r.DepartmentName, r.Revenue)).ToList();
        }

        private async Task<List<(string MedicationName, int StockLevel)>> GetMedicationInventoryDataAsync()
        {
            var products = await _paymentAndBillingService.GetAllProductsAsync();
            return products.Select(p => (p.Name, p.Stock)).ToList();
        }

        private async Task<List<(string MedicationName, decimal Revenue)>> GetPharmacySalesDataAsync()
        {
            var orders = await _paymentAndBillingService.GetOrdersAsync();
            var salesData = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => od.Product.Name)
                .Select(g => new { MedicationName = g.Key, Revenue = g.Sum(od => od.Price * od.Quantity) })
                .OrderBy(g => g.MedicationName)
                .ToList();

            return salesData.Select(r => (r.MedicationName, r.Revenue)).ToList();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label25_Click(object sender, EventArgs e)
        {
            var accountAdminForm = _serviceProvider.GetRequiredService<AccountAdmin>();
            accountAdminForm.SetUserData(_currentUser);
            accountAdminForm.Show();
            this.Hide();
        }
    }
}
