using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLL;
using DataTransferO;
using HMS_CHANGE.Patient.Account_Setting;
using HMS_CHANGE.Patient.Appointment;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace HMS_CHANGE.Patient
{
    public partial class BookAppointment : Form
    {
        private readonly DoctorService _doctorService;
        private readonly AppointmentService _appointmentService;
        private readonly UserService _userService;
        private readonly EmailService _emailService;
        private readonly ForwardService _forwardService;
        private readonly IServiceProvider _serviceProvider;
        private UserDTO? _currentUser;
        private Guid? _followUpAppointmentId;

        public BookAppointment(DoctorService doctorService, AppointmentService appointmentService, UserService userService, EmailService emailService, ForwardService forwardService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _doctorService = doctorService ?? throw new ArgumentNullException(nameof(doctorService));
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _forwardService = forwardService ?? throw new ArgumentNullException(nameof(forwardService));
            _serviceProvider = serviceProvider;

            Load += BookAppointment_Load; // Subscribe to the Load event
            LoadAppointmentTypes();
            comboBox4.Visible = false;
            label24.Visible = false;

            // Set the current user from UserSession
            _currentUser = UserSession.CurrentUser;
            if (_currentUser != null)
            {
                SetWelcomeMessage(_currentUser);
            }
        }

        private async void BookAppointment_Load(object? sender, EventArgs e)
        {
            try
            {
                if (IsHandleCreated) // Ensure the form handle exists
                {
                    await LoadDepartmentsAsync();
                }
            }
            catch (Exception ex)
            {
                if (IsHandleCreated)
                {
                    MessageBox.Show($"Failed to load departments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            // Intentionally left empty for designer reference
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            // Intentionally left empty for designer reference
        }

        public void SetWelcomeMessage(UserDTO user)
        {
            label20.Text = $"{user.Name}";
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void label20_Click(object sender, EventArgs e)
        {
            // Intentionally left empty for designer reference
        }

        private async void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.SelectedValue is Guid departmentId)
            {
                await LoadDoctorsAsync(departmentId).ConfigureAwait(false);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Intentionally left empty for designer reference
        }

        private async void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedItem?.ToString() == "Follow Up Appointment")
            {
                // Show the previous appointments combo box and label
                comboBox4.Visible = true;
                label24.Visible = true;
                await LoadPreviousAppointmentsAsync().ConfigureAwait(false);
            }
            else
            {
                // Hide the previous appointments combo box and label
                comboBox4.Visible = false;
                label24.Visible = false;
                _followUpAppointmentId = null;
            }
        }

        private void ComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedValue is Guid appointmentId)
            {
                _followUpAppointmentId = appointmentId;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (Availabletime.SelectedItem == null)
            {
                MessageBox.Show("Please select a time slot.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedTime = Availabletime.SelectedItem?.ToString() ?? string.Empty;
            if (!DateTime.TryParseExact(
                    $"{dateTimePicker1.Value:yyyy-MM-dd} {selectedTime}",
                    "yyyy-MM-dd HH:mm",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime localAppointmentDate))
            {
                MessageBox.Show("Invalid date or time format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Define the UTC offset for the local time zone
            var utcOffset = TimeSpan.Zero; // Vietnam time zone (UTC+7)

            // Convert local time to UTC
            var appointmentDateUtc = TimeZoneHelper.ConvertLocalToUtc(localAppointmentDate, utcOffset);

            if (comboBox1.SelectedValue is Guid doctorId && comboBox2.SelectedValue is Guid departmentId)
            {
                try
                {
                    var appointment = await _appointmentService.CreateAppointmentAsync(
                        doctorId,
                        _currentUser.UserId,
                        departmentId,
                        appointmentDateUtc,
                        utcOffset,
                        textBox1.Text,
                        _followUpAppointmentId).ConfigureAwait(false);

                    if (!string.IsNullOrEmpty(_currentUser.Email) && !string.IsNullOrEmpty(_currentUser.Name))
                    {
                        await _emailService.SendAppointmentConfirmationEmailAsync(_currentUser.Email, _currentUser.Name, localAppointmentDate).ConfigureAwait(false);
                    }
                    else
                    {
                        MessageBox.Show("User email or username is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    Log.Information("Appointment booked successfully for {UserName} at {AppointmentDateLocal}", _currentUser.Name, localAppointmentDate);

                    MessageBox.Show("Appointment booked successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to book appointment");
                    MessageBox.Show($"Failed to book appointment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _doctorService.GetDepartmentsAsync().ConfigureAwait(false);
                // Update UI safely
                if (comboBox2.InvokeRequired)
                {
                    comboBox2.Invoke(new Action(() =>
                    {
                        if (IsHandleCreated)
                        {
                            comboBox2.DataSource = departments;
                            comboBox2.DisplayMember = "DepartmentName";
                            comboBox2.ValueMember = "DepartmentId";
                        }
                    }));
                }
                else
                {
                    comboBox2.DataSource = departments;
                    comboBox2.DisplayMember = "DepartmentName";
                    comboBox2.ValueMember = "DepartmentId";
                }
            }
            catch (Exception ex)
            {
                if (IsHandleCreated)
                {
                    MessageBox.Show($"Failed to load departments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task LoadDoctorsAsync(Guid departmentId)
        {
            try
            {
                var doctors = await _doctorService.GetDoctorsByDepartmentAsync(departmentId).ConfigureAwait(false);
                // Update UI safely
                if (comboBox1.InvokeRequired)
                {
                    comboBox1.Invoke(new Action(() =>
                    {
                        if (IsHandleCreated)
                        {
                            comboBox1.DataSource = doctors;
                            comboBox1.DisplayMember = "DoctorName";
                            comboBox1.ValueMember = "DoctorId";
                        }
                    }));
                }
                else
                {
                    comboBox1.DataSource = doctors;
                    comboBox1.DisplayMember = "DoctorName";
                    comboBox1.ValueMember = "DoctorId";
                }
            }
            catch (Exception ex)
            {
                if (IsHandleCreated)
                {
                    MessageBox.Show($"Failed to load doctors: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task LoadPreviousAppointmentsAsync()
        {
            try
            {
                List<AppointmentDTO> previousAppointments = (await _appointmentService.GetPastAppointmentsAsync(_currentUser.UserId).ConfigureAwait(false))
                    .Where(a => a.Status == "Confirmed" || a.Status == "Completed")
                    .ToList();

                // Use Invoke to update the UI from the main thread
                if (comboBox4.InvokeRequired)
                {
                    comboBox4.Invoke(new Action(() =>
                    {
                        if (IsHandleCreated)
                        {
                            comboBox4.DataSource = previousAppointments;
                            comboBox4.DisplayMember = "FormattedAppointmentDate";
                            comboBox4.ValueMember = "AppointmentId";
                        }
                    }));
                }
                else
                {
                    comboBox4.DataSource = previousAppointments;
                    comboBox4.DisplayMember = "FormattedAppointmentDate";
                    comboBox4.ValueMember = "AppointmentId";
                }
            }
            catch (Exception ex)
            {
                if (IsHandleCreated)
                {
                    MessageBox.Show($"Failed to load previous appointments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadAppointmentTypes()
        {
            comboBox3.Items.Add("New Appointment");
            comboBox3.Items.Add("Follow Up Appointment");
            comboBox3.SelectedIndex = 0; // Default to "New Appointment"
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Intentionally left empty for designer reference
        }

        private void label24_Click(object sender, EventArgs e)
        {
            // Intentionally left empty for designer reference
        }

        private async void dateTimePicker1_ValueChanged_1(object sender, EventArgs e)
        {
            if (comboBox1.SelectedValue is not Guid doctorId)
            {
                MessageBox.Show("Please select a doctor.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Retrieve available slots (assumed to be in local time)
                DateTime selectedDate = dateTimePicker1.Value.Date;
                var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(doctorId, selectedDate).ConfigureAwait(false);

                if (availableSlots == null || !availableSlots.Any())
                {
                    MessageBox.Show("No available slots for the selected doctor on this date.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Sort slots and add to the listbox
                var sortedSlots = availableSlots.OrderBy(slot => slot).ToList();

                // Update the UI on the main thread
                if (Availabletime.InvokeRequired)
                {
                    Availabletime.Invoke(new Action(() =>
                    {
                        if (IsHandleCreated)
                        {
                            Availabletime.Items.Clear();
                            // Display slots in local time
                            foreach (var slot in sortedSlots)
                            {
                                Availabletime.Items.Add(slot.ToString("HH:mm")); // Display in 24-hour format
                            }
                        }
                    }));
                }
                else
                {
                    Availabletime.Items.Clear();
                    // Display slots in local time
                    foreach (var slot in sortedSlots)
                    {
                        Availabletime.Items.Add(slot.ToString("HH:mm")); // Display in 24-hour format
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsHandleCreated)
                {
                    MessageBox.Show($"Failed to load available time slots: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void Availabletime_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handle selection if needed
        }

        private void label14_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is missing. Please log in again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var home = _serviceProvider.GetRequiredService<Dashboard.DashBoardPatient_GetStart>();
            home.SetWelcomeMessage(_currentUser);
            home.Show();
            this.Hide();
        }

        private async void label9_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is missing. Please log in again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var (userName, userEmail, patientId, upcomingAppointments, pastAppointments) = await _forwardService.GetWelcomeMessageAsync(_currentUser.Email);

                var upcomingAppointment = _serviceProvider.GetRequiredService<UpcomingApp>();
                upcomingAppointment.SetWelcomeMessage(new UserDTO
                {
                    UserId = patientId,
                    Name = userName,
                    Email = userEmail,
                    DateOfBirth = _currentUser.DateOfBirth,
                    PhoneNumber = _currentUser.PhoneNumber,
                    Gender = _currentUser.Gender
                }, patientId, upcomingAppointments, pastAppointments); // Pass user and appointments
                upcomingAppointment.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to navigate to UpcomingApp: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void label10_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is missing. Please log in again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Retrieve the appointment ID for the currently logged-in user
            var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(_currentUser.UserId);
            var appointment = appointments.FirstOrDefault();

            if (appointment == null)
            {
                MessageBox.Show("No upcoming appointment found for the user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var reschedule = _serviceProvider.GetRequiredService<RescheduleAppointment>();
            await reschedule.SetAppointmentDetailsAsync(appointment.AppointmentId, _currentUser.Name, _currentUser.Email, _currentUser.DateOfBirth, _currentUser.PhoneNumber, _currentUser.Gender, _currentUser.UserId);
            reschedule.Show();
            this.Hide();
        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is missing. Please log in again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var accountSettingsForm = _serviceProvider.GetRequiredService<AccountSettingDashboard>();
            accountSettingsForm.SetUserData(_currentUser);
            accountSettingsForm.Show();
            this.Hide();
        }
    }
}
