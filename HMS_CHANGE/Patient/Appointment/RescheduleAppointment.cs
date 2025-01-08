using BusinessLL;
using DataTransferO;
using DevExpress.CodeParser;
using DevExpress.XtraGrid.Views.Grid;
using HMS_CHANGE.Patient.Account_Setting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMS_CHANGE.Patient.Appointment
{
    public partial class RescheduleAppointment : Form
    {
        private readonly AppointmentService _appointmentService;
        private readonly DoctorService _doctorService;
        private readonly EmailService _emailService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ForwardService _forwardService;
        private Guid _appointmentId;
        private Guid _doctorId;
        private string? _userName;
        private string? _userEmail;
        private Guid _patientId;
        private DateOnly _dateOfBirth;
        private string? _phoneNumber;
        private string? _gender;
        private UserDTO? _currentUser;

        public RescheduleAppointment(AppointmentService appointmentService, DoctorService doctorService, EmailService emailService, IServiceProvider serviceProvider, ForwardService forwardService)
        {
            InitializeComponent();
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _doctorService = doctorService ?? throw new ArgumentNullException(nameof(doctorService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _serviceProvider = serviceProvider;
            _forwardService = forwardService ?? throw new ArgumentNullException(nameof(forwardService));

            Load += RescheduleAppointment_Load;
        }

        private void RescheduleAppointment_Load(object? sender, EventArgs e)
        {
            InitializeSearchLookUpEdit();
        }

        public async Task SetAppointmentDetailsAsync(Guid appointmentId, string userName, string userEmail, DateOnly dateOfBirth, string? phoneNumber, string? gender, Guid patientId)
        {
            if (string.IsNullOrEmpty(userName))
            {
                throw new ArgumentNullException(nameof(userName), "User name cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new ArgumentNullException(nameof(userEmail), "User email cannot be null or empty.");
            }

            _appointmentId = appointmentId;
            _userName = userName;
            _userEmail = userEmail;
            _dateOfBirth = dateOfBirth;
            _phoneNumber = phoneNumber;
            _gender = gender;
            _patientId = patientId;

            // Set the current user
            _currentUser = new UserDTO
            {
                UserId = patientId,
                Name = userName,
                Email = userEmail,
                DateOfBirth = dateOfBirth,
                PhoneNumber = phoneNumber,
                Gender = gender
            };

            // Load all appointments for the user
            var appointments = await _appointmentService.GetUpcomingAppointmentsAsync(patientId);
            searchLookUpEdit1.Properties.DataSource = appointments.ToList();
            searchLookUpEdit1.Properties.DisplayMember = "FormattedAppointmentDate";
            searchLookUpEdit1.Properties.ValueMember = "AppointmentId";

            // Set the selected appointment details
            await SetSelectedAppointmentDetailsAsync(appointmentId);
        }

        private async Task SetSelectedAppointmentDetailsAsync(Guid appointmentId)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(appointmentId);
            if (appointment != null)
            {
                _doctorId = appointment.DoctorId;
                var utcOffset = TimeSpan.Zero; // Vietnam time zone (UTC+7)
                dateTimePicker1.Value = TimeZoneHelper.ConvertUtcToLocal(appointment.AppointmentDate, utcOffset);
                textBox1.Text = appointment.Notes;
                textBox2.Text = appointment.Department.DepartmentName;
                textBox3.Text = appointment.Doctor.User.Name;
                textBox4.Text = appointment.FollowUpAppointmentId.HasValue ? "Follow Up Appointment" : "New Appointment";
            }
            else
            {
                MessageBox.Show("Appointment not found. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void InitializeSearchLookUpEdit()
        {
            // Initialize the SearchLookUpEdit control
            searchLookUpEdit1.Properties.DisplayMember = "FormattedAppointmentDate";
            searchLookUpEdit1.Properties.ValueMember = "AppointmentId";

            // Configure the view
            GridView view = searchLookUpEdit1.Properties.View;
            view.Columns.Clear(); // Clear existing columns if any
            view.Columns.AddVisible("FormattedAppointmentDate", "Appointment Date");
            view.Columns.AddVisible("Doctor.DoctorName", "Doctor Name");
            view.Columns.AddVisible("Department.DepartmentName", "Department");

            // Handle the EditValueChanged event
            searchLookUpEdit1.EditValueChanged += searchLookUpEdit1_EditValueChanged;
        }

        private async void searchLookUpEdit1_EditValueChanged(object? sender, EventArgs e)
        {
            if (searchLookUpEdit1.EditValue == null)
            {
                MessageBox.Show("Please select an appointment from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(_userEmail) || string.IsNullOrEmpty(_userName))
            {
                MessageBox.Show("User email is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Handle the case when an appointment is selected
            Guid selectedAppointmentId = (Guid)searchLookUpEdit1.EditValue;
            await SetSelectedAppointmentDetailsAsync(selectedAppointmentId);
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

        private async void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            await LoadAvailableTimeSlotsAsync();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (Availabletime.SelectedItem == null)
            {
                MessageBox.Show("Please select a time slot.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string? selectedTime = Availabletime.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedTime) || !DateTime.TryParseExact($"{dateTimePicker1.Value:yyyy-MM-dd} {selectedTime}", "yyyy-MM-dd HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime localAppointmentDate))
            {
                MessageBox.Show("Invalid date or time format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_userEmail))
            {
                MessageBox.Show("User email is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(_userName))
            {
                MessageBox.Show("User name is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Define the UTC offset for the local time zone
                var utcOffset = TimeSpan.FromHours(0); // Vietnam time zone (UTC+7)

                // Convert local time to UTC
                var appointmentDateUtc = TimeZoneHelper.ConvertLocalToUtc(localAppointmentDate, utcOffset);

                var rescheduledAppointment = await _appointmentService.RescheduleAppointmentAsync(_appointmentId, _doctorId, appointmentDateUtc);
                DateTime appointmentDateLocal = TimeZoneHelper.ConvertUtcToLocal(rescheduledAppointment.AppointmentDate, utcOffset);
                await _emailService.SendAppointmentConfirmationEmailAsync(_userEmail, _userName, appointmentDateLocal);

                MessageBox.Show("Appointment rescheduled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to reschedule appointment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label14_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_userName))
            {
                MessageBox.Show("User name is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(_userEmail))
            {
                MessageBox.Show("User email is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var home = _serviceProvider.GetRequiredService<Dashboard.DashBoardPatient_GetStart>();
            home.SetWelcomeMessage(new UserDTO
            {
                Name = _userName,
                Email = _userEmail,
                DateOfBirth = _dateOfBirth,
                PhoneNumber = _phoneNumber,
                Gender = _gender
            });
            home.Show();
            this.Hide();
        }

        private async void label3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_userName) || string.IsNullOrEmpty(_userEmail))
            {
                MessageBox.Show("User information is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var scheduleAppointment = _serviceProvider.GetRequiredService<BookAppointment>();
            scheduleAppointment.SetWelcomeMessage(new UserDTO
            {
                Name = _userName,
                Email = _userEmail,
                DateOfBirth = _dateOfBirth,
                PhoneNumber = _phoneNumber,
                Gender = _gender
            });
            scheduleAppointment.Show();
            this.Hide();
        }

        private async void label7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_userName) || string.IsNullOrEmpty(_userEmail))
            {
                MessageBox.Show("User information is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var scheduleAppointment = _serviceProvider.GetRequiredService<BookAppointment>();
            scheduleAppointment.SetWelcomeMessage(new UserDTO
            {
                Name = _userName,
                Email = _userEmail,
                DateOfBirth = _dateOfBirth,
                PhoneNumber = _phoneNumber,
                Gender = _gender
            });
            scheduleAppointment.Show();
            this.Hide();
        }

        private async void label9_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_userEmail) || string.IsNullOrEmpty(_userName))
            {
                MessageBox.Show("User name is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                (string userName, string userEmail, Guid patientId, List<AppointmentDTO> upcomingAppointments, List<AppointmentDTO> pastAppointments) = await _forwardService.GetWelcomeMessageAsync(_userEmail);

                var upcomingAppointment = _serviceProvider.GetRequiredService<UpcomingApp>();
                upcomingAppointment.SetWelcomeMessage(new UserDTO
                {
                    Name = userName,
                    Email = userEmail,
                    DateOfBirth = _dateOfBirth,
                    PhoneNumber = _phoneNumber,
                    Gender = _gender
                }, patientId, upcomingAppointments, pastAppointments); // Pass patientId and appointments
                upcomingAppointment.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to navigate to UpcomingApp: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void label17_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_userEmail) || string.IsNullOrEmpty(_userName))
            {
                MessageBox.Show("User name is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var bookAppointment = _serviceProvider.GetRequiredService<BookAppointment>();
            bookAppointment.SetWelcomeMessage(new UserDTO
            {
                Name = _userName,
                Email = _userEmail,
                DateOfBirth = _dateOfBirth,
                PhoneNumber = _phoneNumber,
                Gender = _gender
            });
            bookAppointment.Show();
            this.Hide();
        }

        private void Availabletime_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Intentionally left empty
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // note textbox
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // department textbox
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // doctor textbox
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // appointment type textbox
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Set the current user in the UserSession
            UserSession.CurrentUser = _currentUser;

            var accountSettingsForm = _serviceProvider.GetRequiredService<AccountSettingDashboard>();
            accountSettingsForm.Show();
            this.Hide();
        }
    }
}
