using BusinessLL;
using DataTransferO;
using HMS_CHANGE.Doctor;
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

namespace HMS_CHANGE.Dashboard
{
    public partial class DashBoardDoctor : Form
    {
        private readonly DoctorService _doctorService;
        private readonly AppointmentService _appointmentService;
        private readonly EmailService _emailService;
        private readonly HospitalStatisticService _hospitalStatisticService;
        private readonly IServiceProvider _serviceProvider;
        private Guid _doctorId;
        private Guid _selectedAppointmentId;
        private bool _isRescheduling = false;
        private UserDTO _currentUser;
        private DoctorDetailDTO _currentDoctorDetail;

        public DashBoardDoctor(DoctorService doctorService, AppointmentService appointmentService, EmailService emailService, HospitalStatisticService hospitalStatisticService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _doctorService = doctorService ?? throw new ArgumentNullException(nameof(doctorService));
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _hospitalStatisticService = hospitalStatisticService ?? throw new ArgumentNullException(nameof(hospitalStatisticService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            InitializeDataGridView();
            Load += DashBoardDoctor_Load;
            HideRescheduleControls();
        }

        public void SetDoctorId(Guid doctorId)
        {
            _doctorId = doctorId;
        }

        private void InitializeDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;

            // Add AppointmentId column with Name property set
            var appointmentIdColumn = new DataGridViewTextBoxColumn
            {
                Name = "AppointmentId", // Set the Name property
                DataPropertyName = "AppointmentId",
                HeaderText = "AppointmentId",
                Visible = false // Hide the AppointmentId column
            };
            dataGridView1.Columns.Add(appointmentIdColumn);

            // Add other columns with Name property set
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                DataPropertyName = "Name",
                HeaderText = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Email",
                DataPropertyName = "Email",
                HeaderText = "Email",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Gender",
                DataPropertyName = "Gender",
                HeaderText = "Gender",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AppointmentDate",
                DataPropertyName = "AppointmentDate",
                HeaderText = "Appointment Date",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                DefaultCellStyle = { Format = "MM/dd/yyyy HH:mm" }
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                DataPropertyName = "Status",
                HeaderText = "Status",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private async void DashBoardDoctor_Load(object? sender, EventArgs e)
        {
            if (UserSession.CurrentUser == null)
            {
                MessageBox.Show("User is not logged in.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                _currentUser = UserSession.CurrentUser;
                _currentDoctorDetail = await _doctorService.GetDoctorByUserIdAsync(_currentUser.UserId);
                if (_currentDoctorDetail == null)
                {
                    MessageBox.Show("Doctor details not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _doctorId = _currentDoctorDetail.DoctorId;

                var appointments = await _doctorService.GetAppointmentsForDoctorAsync(_doctorId);
                var patientWithAppointments = appointments.Select(a => new
                {
                    AppointmentId = a.AppointmentId,
                    Name = a.Patient?.Name ?? string.Empty,
                    Email = a.Patient?.Email ?? string.Empty,
                    Gender = a.Patient?.Gender ?? string.Empty,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                }).ToList();

                dataGridView1.DataSource = patientWithAppointments;

                // Load statistics
                await LoadStatisticsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            label2.Text = $"Dr. {UserSession.CurrentUser.Name}";
        }

        private async Task LoadStatisticsAsync()
        {
            if (_doctorId == Guid.Empty)
            {
                MessageBox.Show("Doctor ID is not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var statistics = await _hospitalStatisticService.CalculateStatisticsAsync(_doctorId);
                textBox1.Text = statistics.TotalAppointments.ToString();
                textBox2.Text = statistics.CanceledAppointments.ToString();
                var ratio = statistics.ReturningPatients == 0 ? 0 : (double)statistics.NewPatients / statistics.ReturningPatients;
                textBox3.Text = ratio.ToString("F2");

                Log.Information("Loaded statistics for DoctorId: {DoctorId}, TotalAppointments: {TotalAppointments}, CanceledAppointments: {CanceledAppointments}, NewPatients: {NewPatients}, ReturningPatients: {ReturningPatients}, Ratio: {Ratio}",
                    _doctorId, statistics.TotalAppointments, statistics.CanceledAppointments, statistics.NewPatients, statistics.ReturningPatients, ratio);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load statistics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = dataGridView1.Rows[e.RowIndex];
                if (row.Cells["AppointmentId"].Value is Guid appointmentId)
                {
                    _selectedAppointmentId = appointmentId;
                }
                else
                {
                    MessageBox.Show("Appointment ID not found for the selected row.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (_selectedAppointmentId == Guid.Empty)
            {
                MessageBox.Show("Please select an appointment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isRescheduling)
            {
                if (Availabletime.SelectedItem == null)
                {
                    MessageBox.Show("Please select a time slot.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string? selectedTime = Availabletime.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedTime) || !DateTime.TryParseExact($"{dateTimePicker1.Value:yyyy-MM-dd} {selectedTime}", "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime newLocalAppointmentDate))
                {
                    MessageBox.Show("Invalid date or time format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                try
                {
                    var rescheduledAppointment = await _appointmentService.RescheduleAppointmentAsync(_selectedAppointmentId, _doctorId, newLocalAppointmentDate);
                    var doctorDetail = await _doctorService.GetDoctorByIdAsync(_doctorId);

                    if (rescheduledAppointment.Patient == null || string.IsNullOrEmpty(rescheduledAppointment.Patient.Email) || string.IsNullOrEmpty(rescheduledAppointment.Patient.Name))
                    {
                        MessageBox.Show("Patient details are missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (doctorDetail == null || string.IsNullOrEmpty(doctorDetail.DoctorName))
                    {
                        MessageBox.Show("Doctor details are missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    await _emailService.SendAppointmentRescheduledEmailAsync(rescheduledAppointment.Patient.Email, rescheduledAppointment.Patient.Name, doctorDetail.DoctorName, newLocalAppointmentDate);
                    MessageBox.Show("Appointment rescheduled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadAppointmentsAsync();
                    HideRescheduleControls();
                    _isRescheduling = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to reschedule appointment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                try
                {
                    var appointment = await _appointmentService.UpdateAppointmentStatusAsync(_selectedAppointmentId, "Confirmed");

                    if (appointment.Patient == null || string.IsNullOrEmpty(appointment.Patient.Email) || string.IsNullOrEmpty(appointment.Patient.Name))
                    {
                        MessageBox.Show("Patient details are missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    await _emailService.SendAppointmentStatusChangedEmailAsync(appointment.Patient.Email, appointment.Patient.Name, appointment.AppointmentDate, "Confirmed");
                    MessageBox.Show("Appointment confirmed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadAppointmentsAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to confirm appointment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            // Toggle the visibility of the reschedule controls
            if (dateTimePicker1.Visible)
            {
                HideRescheduleControls();
                _isRescheduling = false;
            }
            else
            {
                // Reschedule the appointment
                if (_selectedAppointmentId == Guid.Empty)
                {
                    MessageBox.Show("Please select an appointment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Show the date and time pickers
                ShowRescheduleControls();
                _isRescheduling = true;

                // Load available time slots for the selected date
                await LoadAvailableTimeSlotsAsync();
            }
        }

        private async Task LoadAvailableTimeSlotsAsync()
        {
            try
            {
                DateTime selectedDate = dateTimePicker1.Value.Date;
                var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(_doctorId, selectedDate);

                if (availableSlots == null || !availableSlots.Any())
                {
                    MessageBox.Show("No available slots for the selected doctor on this date.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var sortedSlots = availableSlots.OrderBy(slot => slot).ToList();

                Availabletime.Items.Clear();
                foreach (var slot in sortedSlots)
                {
                    Availabletime.Items.Add(slot.ToString("HH:mm"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load available time slots: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            // Cancel the appointment
            if (_selectedAppointmentId == Guid.Empty)
            {
                MessageBox.Show("Please select an appointment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var appointment = await _appointmentService.UpdateAppointmentStatusAsync(_selectedAppointmentId, "Canceled");

                if (appointment.Patient == null || string.IsNullOrEmpty(appointment.Patient.Email) || string.IsNullOrEmpty(appointment.Patient.Name))
                {
                    MessageBox.Show("Patient details are missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                await _emailService.SendAppointmentCanceledEmailAsync(appointment.Patient.Email, appointment.Patient.Name, appointment.AppointmentDate);
                MessageBox.Show("Appointment canceled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAppointmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to cancel appointment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadAppointmentsAsync()
        {
            try
            {
                var appointments = await _doctorService.GetAppointmentsForDoctorAsync(_doctorId);
                var patientWithAppointments = appointments.Select(a => new
                {
                    AppointmentId = a.AppointmentId,
                    Name = a.Patient?.Name ?? string.Empty,
                    Email = a.Patient?.Email ?? string.Empty,
                    Gender = a.Patient?.Gender ?? string.Empty,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                }).ToList();

                dataGridView1.DataSource = patientWithAppointments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Availabletime_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Intentionally left empty
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // Intentionally left empty
        }

        private void label15_Click(object sender, EventArgs e)
        {
            // Intentionally left empty
        }

        private void HideRescheduleControls()
        {
            dateTimePicker1.Visible = false;
            Availabletime.Visible = false;
            label15.Visible = false;
        }

        private void ShowRescheduleControls()
        {
            dateTimePicker1.Visible = true;
            Availabletime.Visible = true;
            label15.Visible = true;
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private async void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Total appointment
            if (_doctorId == Guid.Empty)
            {
                MessageBox.Show("Doctor ID is not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var statistics = await _hospitalStatisticService.CalculateStatisticsAsync(_doctorId);
                textBox1.Text = statistics.TotalAppointments.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate total appointments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void textBox2_TextChanged(object sender, EventArgs e)
        {
            // Total canceled appointment
            if (_doctorId == Guid.Empty)
            {
                MessageBox.Show("Doctor ID is not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var statistics = await _hospitalStatisticService.CalculateStatisticsAsync(_doctorId);
                textBox2.Text = statistics.CanceledAppointments.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate canceled appointments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void textBox3_TextChanged(object sender, EventArgs e)
        {
            // New vs follow-up appointment ratio
            if (_doctorId == Guid.Empty)
            {
                MessageBox.Show("Doctor ID is not set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var statistics = await _hospitalStatisticService.CalculateStatisticsAsync(_doctorId);
                var ratio = statistics.ReturningPatients == 0 ? 0 : (double)statistics.NewPatients / statistics.ReturningPatients;
                textBox3.Text = ratio.ToString("F2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to calculate new vs follow-up appointment ratio: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            var detailsAppointmentForm = _serviceProvider.GetRequiredService<DetailsAppointment>();
            var allAppointmentIds = dataGridView1.Rows
                .Cast<DataGridViewRow>()
                .Select(row => (Guid)row.Cells["AppointmentId"].Value)
                .ToList();
            detailsAppointmentForm.SetAppointmentDetails(_doctorId, allAppointmentIds, UserSession.CurrentUser);
            detailsAppointmentForm.Show();
            this.Hide();
        }

        private void label6_Click(object sender, EventArgs e)
        {
            var accountDocForm = _serviceProvider.GetRequiredService<AccountDoc>();
            accountDocForm.SetUserData(_currentUser, _currentDoctorDetail);
            accountDocForm.Show();
            this.Hide();
        }

        private void Availabletime_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }
    }
}
