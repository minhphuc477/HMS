using BusinessLL;
using DataAL.Models;
using DataTransferO;
using DevExpress.XtraCharts;
using DevExpress.XtraGantt.Scheduling;
using iText.Forms.Form.Element;
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
    public partial class AppointmentStats : Form
    {
        private readonly AppointmentService _appointmentService;
        private readonly UserService _userService;
        private readonly PaymentAndBillingService _paymentAndBillingService;
        private readonly DoctorService _doctorService;
        private UserDTO _currentUser;
        private IServiceProvider _serviceProvider;

        public AppointmentStats(AppointmentService appointmentService, UserService userService, PaymentAndBillingService paymentAndBillingService, DoctorService doctorService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _userService = userService;
            _paymentAndBillingService = paymentAndBillingService;
            _doctorService = doctorService;
            this.StartPosition = FormStartPosition.CenterScreen; // Center the form on the screen
            _serviceProvider = serviceProvider;
        }

        public void SetUserData(UserDTO currentUser)
        {
            _currentUser = currentUser;
            label2.Text = currentUser.Name;
            // Use _currentUser as needed within this form
        }

        private void panel6_Paint(object sender, PaintEventArgs e)
        {
            // Intentionally left empty
        }

        // ChartControl1: Appointment Volume by Date
        private async void chartControl1_Click(object sender, EventArgs e)
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                var appointmentGroups = appointments
                    .GroupBy(a => a.AppointmentDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Date)
                    .ToList();

                if (chartControl1.Series.Count == 0)
                {
                    chartControl1.Series.Add(new Series("Appointments", ViewType.Line));
                }

                chartControl1.Series[0].Points.Clear();
                foreach (var group in appointmentGroups)
                {
                    chartControl1.Series[0].Points.Add(new SeriesPoint(group.Date, group.Count));
                }

                // Set chart title and axis labels
                chartControl1.Titles.Clear();
                chartControl1.Titles.Add(new ChartTitle { Text = "Appointment Volume by Date" });
                ((XYDiagram)chartControl1.Diagram).AxisX.Title.Text = "Date";
                ((XYDiagram)chartControl1.Diagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
                ((XYDiagram)chartControl1.Diagram).AxisY.Title.Text = "Number of Appointments";
                ((XYDiagram)chartControl1.Diagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;

                // Enable tooltips
                chartControl1.ToolTipEnabled = DevExpress.Utils.DefaultBoolean.True;
                chartControl1.Series[0].ToolTipPointPattern = "{A}: {V}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load appointment volume data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ChartControl2: Appointment Status Distribution
        private async void chartControl2_Click(object sender, EventArgs e)
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                var statusGroups = appointments
                    .GroupBy(a => a.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToList();

                if (chartControl2.Series.Count == 0)
                {
                    chartControl2.Series.Add(new Series("Status", ViewType.Pie));
                }

                chartControl2.Series[0].Points.Clear();
                foreach (var group in statusGroups)
                {
                    chartControl2.Series[0].Points.Add(new SeriesPoint(group.Status, group.Count));
                }

                // Set chart title
                chartControl2.Titles.Clear();
                chartControl2.Titles.Add(new ChartTitle { Text = "Appointment Status Distribution" });
                chartControl2.Titles[0].Dock = ChartTitleDockStyle.Left; // Move title to the left side

                // Enable tooltips
                chartControl2.ToolTipEnabled = DevExpress.Utils.DefaultBoolean.True;
                chartControl2.Series[0].ToolTipPointPattern = "{A}: {V}";

                // Customize pie chart labels
                PieSeriesLabel seriesLabel = (PieSeriesLabel)chartControl2.Series[0].Label;
                seriesLabel.TextPattern = "{A}: {VP:P0}";
                seriesLabel.TextColor = Color.Black;
                seriesLabel.Font = new Font("Tahoma", 10, FontStyle.Bold);
                seriesLabel.Border.Visibility = DevExpress.Utils.DefaultBoolean.False;

                // Customize pie chart size and position
                PieSeriesView seriesView = (PieSeriesView)chartControl2.Series[0].View;
                seriesView.RuntimeExploding = true;
                seriesView.ExplodedDistancePercentage = 5;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load appointment status data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ChartControl3: Age Distribution of Patients
        private async void chartControl3_Click(object sender, EventArgs e)
        {
            try
            {
                var patients = await _userService.GetUsersByRoleAsync("Patient");
                var ageGroups = patients
                    .Where(p => p.DateOfBirth != null)
                    .GroupBy(p => DateTime.Now.Year - p.DateOfBirth.Year)
                    .Select(g => new { Age = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Age)
                    .ToList();

                if (chartControl3.Series.Count == 0)
                {
                    chartControl3.Series.Add(new Series("Age Distribution", ViewType.Bar));
                }

                chartControl3.Series[0].Points.Clear();
                foreach (var group in ageGroups)
                {
                    chartControl3.Series[0].Points.Add(new SeriesPoint(group.Age, group.Count));
                }

                // Set chart title and axis labels
                chartControl3.Titles.Clear();
                chartControl3.Titles.Add(new ChartTitle { Text = "Age Distribution of Patients" });
                ((XYDiagram)chartControl3.Diagram).AxisX.Title.Text = "Age";
                ((XYDiagram)chartControl3.Diagram).AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
                ((XYDiagram)chartControl3.Diagram).AxisY.Title.Text = "Number of Patients";
                ((XYDiagram)chartControl3.Diagram).AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;

                // Enable tooltips
                chartControl3.ToolTipEnabled = DevExpress.Utils.DefaultBoolean.True;
                chartControl3.Series[0].ToolTipPointPattern = "{A}: {V}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load age distribution data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ChartControl4: Gender Distribution of Patients
        private async void chartControl4_Click(object sender, EventArgs e)
        {
            try
            {
                var patients = await _userService.GetUsersByRoleAsync("Patient");
                var genderGroups = patients
                    .Where(p => !string.IsNullOrEmpty(p.Gender))
                    .GroupBy(p => p.Gender)
                    .Select(g => new { Gender = g.Key, Count = g.Count() })
                    .ToList();

                if (chartControl4.Series.Count == 0)
                {
                    chartControl4.Series.Add(new Series("Gender Distribution", ViewType.Pie));
                }

                chartControl4.Series[0].Points.Clear();
                foreach (var group in genderGroups)
                {
                    chartControl4.Series[0].Points.Add(new SeriesPoint(group.Gender, group.Count));
                }

                // Set chart title
                chartControl4.Titles.Clear();
                chartControl4.Titles.Add(new ChartTitle { Text = "Gender Distribution of Patients" });
                chartControl4.Titles[0].Dock = ChartTitleDockStyle.Left; // Move title to the left side

                // Enable tooltips
                chartControl4.ToolTipEnabled = DevExpress.Utils.DefaultBoolean.True;
                chartControl4.Series[0].ToolTipPointPattern = "{A}: {V}";

                // Customize pie chart labels
                PieSeriesLabel seriesLabel = (PieSeriesLabel)chartControl4.Series[0].Label;
                seriesLabel.TextPattern = "{A}: {VP:P0}";
                seriesLabel.TextColor = Color.Black;
                seriesLabel.Font = new Font("Tahoma", 10, FontStyle.Bold);
                seriesLabel.Border.Visibility = DevExpress.Utils.DefaultBoolean.False;

                // Customize pie chart size and position
                PieSeriesView seriesView = (PieSeriesView)chartControl4.Series[0].View;
                seriesView.RuntimeExploding = true;
                seriesView.ExplodedDistancePercentage = 5;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load gender distribution data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void label9_Click(object sender, EventArgs e)
        {
            var revenuStatsForm = new RevenuStats(
                _paymentAndBillingService,
                _doctorService,
                _userService,
                _appointmentService,
                _serviceProvider
            );

            revenuStatsForm.SetUserData(_currentUser);
            revenuStatsForm.Show();
            this.Hide();
        }

        private void label2_Click(object sender, EventArgs e)
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
