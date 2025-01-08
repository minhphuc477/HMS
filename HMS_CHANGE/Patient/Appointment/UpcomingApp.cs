using BusinessLL;
using DataTransferO;
using HMS_CHANGE.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.XtraEditors;
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
using System.IO;
using HMS_CHANGE.Patient.Account_Setting;

namespace HMS_CHANGE.Patient.Appointment
{
    public partial class UpcomingApp : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AppointmentService _appointmentService;
        private readonly PdfGenerationService _pdfGenerationService;
        private readonly ForwardService _forwardService;
        private UserDTO? _currentUser;

        public UpcomingApp(IServiceProvider serviceProvider, AppointmentService appointmentService, PdfGenerationService pdfGenerationService, ForwardService forwardService)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _appointmentService = appointmentService;
            _pdfGenerationService = pdfGenerationService;
            _forwardService = forwardService;

            // Initialize the SearchLookUpEdit controls
            InitializeSearchLookUpEdit();
            InitializeSearchLookUpEditPast();

            // Set the current user from UserSession
            _currentUser = UserSession.CurrentUser;
            if (_currentUser != null)
            {
                label20.Text = $"{_currentUser.Name}";
                LoadUpcomingAppointments();
                LoadPastAppointments();
            }
        }

        public void SetWelcomeMessage(UserDTO user, Guid patientId, List<AppointmentDTO> upcomingAppointments, List<AppointmentDTO> pastAppointments)
        {
            label20.Text = $"{user.Name}";
            // Additional logic to handle the welcome message and appointments

            searchLookUpEdit1.Properties.DataSource = upcomingAppointments;
            searchLookUpEdit2_Past.Properties.DataSource = pastAppointments;
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

        private void InitializeSearchLookUpEditPast()
        {
            // Initialize the SearchLookUpEdit control for past appointments
            searchLookUpEdit2_Past.Properties.DisplayMember = "FormattedAppointmentDate";
            searchLookUpEdit2_Past.Properties.ValueMember = "AppointmentId";

            // Configure the view
            GridView view = searchLookUpEdit2_Past.Properties.View;
            view.Columns.Clear(); // Clear existing columns if any
            view.Columns.AddVisible("FormattedAppointmentDate", "Appointment Date");
            view.Columns.AddVisible("Doctor.DoctorName", "Doctor Name");
            view.Columns.AddVisible("Department.DepartmentName", "Department");

            // Handle the EditValueChanged event
            searchLookUpEdit2_Past.EditValueChanged += searchLookUpEdit2_Past_EditValueChanged;
        }

        private async void LoadUpcomingAppointments()
        {
            try
            {
                if (_appointmentService == null)
                {
                    throw new NullReferenceException("_appointmentService is not initialized.");
                }
                if (_currentUser == null || _currentUser.UserId == Guid.Empty)
                {
                    throw new NullReferenceException("_currentUser is not initialized.");
                }

                // Fetch upcoming appointments
                List<AppointmentDTO> upcomingAppointments = (await _appointmentService.GetUpcomingAppointmentsAsync(_currentUser.UserId)).ToList();

                // Log the fetched data
                Console.WriteLine("Fetched upcoming appointments:");
                foreach (var appointment in upcomingAppointments)
                {
                    string doctorName = appointment.Doctor?.User?.Name ?? "Unknown";
                    string departmentName = appointment.Department?.DepartmentName ?? "Unknown";
                    Console.WriteLine($"Appointment Date: {appointment.AppointmentDate}, Doctor: {doctorName}, Department: {departmentName}");
                }

                // Check if searchLookUpEdit1 is initialized
                if (searchLookUpEdit1 == null)
                {
                    throw new NullReferenceException("searchLookUpEdit1 is not initialized.");
                }

                // Check if upcomingAppointments is null or empty
                if (upcomingAppointments == null || !upcomingAppointments.Any())
                {
                    MessageBox.Show("No upcoming appointments found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Bind the data to the SearchLookUpEdit
                searchLookUpEdit1.Properties.DataSource = upcomingAppointments;

                // Update DisplayMember to use the formatted date
                searchLookUpEdit1.Properties.DisplayMember = "FormattedAppointmentDate";

                // Check if data source is set correctly
                if (searchLookUpEdit1.Properties.DataSource == null)
                {
                    Console.WriteLine("DataSource is null");
                }
                else
                {
                    Console.WriteLine("DataSource is set");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load upcoming appointments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadPastAppointments()
        {
            try
            {
                if (_appointmentService == null)
                {
                    throw new NullReferenceException("_appointmentService is not initialized.");
                }
                if (_currentUser == null || _currentUser.UserId == Guid.Empty)
                {
                    throw new NullReferenceException("_currentUser is not initialized.");
                }

                // Fetch past appointments
                List<AppointmentDTO> pastAppointments = (await _appointmentService.GetPastAppointmentsAsync(_currentUser.UserId)).ToList();

                // Log the fetched data
                Console.WriteLine("Fetched past appointments:");
                foreach (var appointment in pastAppointments)
                {
                    string doctorName = appointment.Doctor?.User?.Name ?? "Unknown";
                    string departmentName = appointment.Department?.DepartmentName ?? "Unknown";
                    Console.WriteLine($"Appointment Date: {appointment.AppointmentDate}, Doctor: {doctorName}, Department: {departmentName}");
                }

                if (searchLookUpEdit2_Past == null)
                {
                    throw new NullReferenceException("searchLookUpEdit2_Past is not initialized.");
                }

                if (pastAppointments == null || !pastAppointments.Any())
                {
                    MessageBox.Show("No past appointments found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Bind the data to the SearchLookUpEdit
                searchLookUpEdit2_Past.Properties.DataSource = pastAppointments;

                // Update DisplayMember to use the formatted date
                searchLookUpEdit2_Past.Properties.DisplayMember = "FormattedAppointmentDate";

                // Log the data source
                if (searchLookUpEdit2_Past.Properties.DataSource == null)
                {
                    Console.WriteLine("DataSource is null");
                }
                else
                {
                    var dataSource = (List<AppointmentDTO>)searchLookUpEdit2_Past.Properties.DataSource;
                    Console.WriteLine("DataSource is set with the following appointments:");
                    foreach (var appointment in dataSource)
                    {
                        Console.WriteLine($"AppointmentId: {appointment.AppointmentId}, FormattedDate: {appointment.FormattedAppointmentDate}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load past appointments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void searchLookUpEdit1_EditValueChanged(object? sender, EventArgs e)
        {
            if (searchLookUpEdit1.EditValue == null)
            {
                MessageBox.Show("Please select an appointment from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Handle the case when an appointment is selected
            Guid selectedAppointmentId = (Guid)searchLookUpEdit1.EditValue;
            LoadAppointmentDetails(selectedAppointmentId);
        }

        private void LoadAppointmentDetails(Guid appointmentId)
        {
            // Fetch the selected appointment details
            var selectedAppointment = ((List<AppointmentDTO>)searchLookUpEdit1.Properties.DataSource)
                .FirstOrDefault(a => a.AppointmentId == appointmentId);

            if (selectedAppointment != null)
            {
                // Display the appointment details in the grid or other UI elements
                textBoxAppointmentDetails_past.Text = $"Appointment Date: {selectedAppointment.AppointmentDate}{Environment.NewLine}" +
                                                 $"Doctor: {selectedAppointment.Doctor?.User?.Name ?? "Unknown"}{Environment.NewLine}" +
                                                 $"Department: {selectedAppointment.Department?.DepartmentName ?? "Unknown"}{Environment.NewLine}" +
                                                 $"Notes: {selectedAppointment.Notes ?? string.Empty}";
            }
        }

        private void textBoxAppointmentDetails_TextChanged(object sender, EventArgs e)
        {
            // Intentionally left empty for designer reference
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void label20_Click(object sender, EventArgs e)
        {
            // Intentionally left empty for designer reference
        }

        private void label14_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var home = _serviceProvider.GetRequiredService<DashBoardPatient_GetStart>();
            home.SetWelcomeMessage(_currentUser);
            home.Show();
            this.Hide();
        }

        private async void label7_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var scheduleAppointment = _serviceProvider.GetRequiredService<BookAppointment>();
            scheduleAppointment.SetWelcomeMessage(_currentUser);
            scheduleAppointment.Show();
            this.Hide();
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            // Intentionally left empty for designer reference
        }

        //text box 1 is for the past appointment details
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void searchLookUpEdit2_Past_EditValueChanged(object? sender, EventArgs e)
        {
            if (searchLookUpEdit2_Past.EditValue == null)
            {
                MessageBox.Show("Please select an appointment from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Handle the case when a past appointment is selected
            Guid selectedAppointmentId = (Guid)searchLookUpEdit2_Past.EditValue;
            LoadPastAppointmentDetails(selectedAppointmentId);
        }

        private void LoadPastAppointmentDetails(Guid appointmentId)
        {
            var selectedAppointment = ((List<AppointmentDTO>)searchLookUpEdit2_Past.Properties.DataSource)
                .FirstOrDefault(a => a.AppointmentId == appointmentId);

            if (selectedAppointment != null)
            {
                Console.WriteLine($"Selected Appointment - ID: {selectedAppointment.AppointmentId}, Date: {selectedAppointment.AppointmentDate}");
                textBox1.Text = $"Appointment Date: {selectedAppointment.AppointmentDate}{Environment.NewLine}" +
                                                      $"Doctor: {selectedAppointment.Doctor?.User?.Name ?? "Unknown"}{Environment.NewLine}" +
                                                      $"Department: {selectedAppointment.Department?.DepartmentName ?? "Unknown"}{Environment.NewLine}" +
                                                      $"Notes: {selectedAppointment.Notes ?? string.Empty}";
            }
            else
            {
                Console.WriteLine("Selected appointment not found.");
            }
        }

        private async void label3_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var scheduleAppointment = _serviceProvider.GetRequiredService<BookAppointment>();
            scheduleAppointment.SetWelcomeMessage(_currentUser);
            scheduleAppointment.Show();
            this.Hide();
        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private async void label19_Click_1(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var email = _currentUser.Email ?? throw new ArgumentNullException(nameof(_currentUser.Email));

                var (userName, userEmail, patientId, upcomingAppointments, pastAppointments) = await _forwardService.GetWelcomeMessageAsync(email);

                var appointment = upcomingAppointments.FirstOrDefault(a => a.Status == "Confirmed" || a.Status == "Pending");

                if (appointment == null)
                {
                    MessageBox.Show("No upcoming appointment found for the user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var reschedule = _serviceProvider.GetRequiredService<RescheduleAppointment>();
                await reschedule.SetAppointmentDetailsAsync(appointment.AppointmentId, userName, userEmail, _currentUser.DateOfBirth, _currentUser.PhoneNumber ?? string.Empty, _currentUser.Gender ?? string.Empty, patientId);
                reschedule.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to navigate to RescheduleAppointment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Check if a value is selected in the searchLookUpEdit1 control
            if (searchLookUpEdit1.EditValue == null)
            {
                MessageBox.Show("Please select an appointment from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the selected appointment ID
            Guid selectedAppointmentId = (Guid)searchLookUpEdit1.EditValue;

            // Find the selected appointment in the data source
            var selectedAppointment = ((List<AppointmentDTO>)searchLookUpEdit1.Properties.DataSource)
                .FirstOrDefault(a => a.AppointmentId == selectedAppointmentId);

            if (selectedAppointment != null)
            {
                // Create a memory stream for the PDF
                MemoryStream stream = new MemoryStream();

                try
                {
                    // Retrieve user information using ForwardService
                    var email = _currentUser.Email ?? throw new ArgumentNullException(nameof(_currentUser.Email));

                    var (userName, userEmail, patientId, upcomingAppointments, pastAppointments) = await _forwardService.GetWelcomeMessageAsync(email);

                    // Generate the PDF using the PdfGenerationService
                    _pdfGenerationService.GenerateAppointmentPdf(selectedAppointment, stream, new UserDTO
                    {
                        UserId = _currentUser.UserId,
                        Name = _currentUser.Name,
                        Email = _currentUser.Email,
                        // Add other user details as needed
                    });

                    // Reset the stream position to the beginning
                    stream.Position = 0;

                    // Wrap the stream with NonClosingStream
                    var nonClosingStream = new PdfGenerationService.NonClosingStream(stream);

                    // Get an instance of PdfViewerForm from the service provider
                    var pdfViewerForm = _serviceProvider.GetRequiredService<PdfViewerForm>();

                    // Load the generated PDF into the PdfViewerForm
                    pdfViewerForm.LoadPdf(nonClosingStream, new UserDTO
                    {
                        UserId = _currentUser.UserId,
                        Name = _currentUser.Name,
                        Email = _currentUser.Email,
                        // Add other user details as needed
                    });

                    // Show the PdfViewerForm
                    pdfViewerForm.Show();

                    // Attach an event handler to dispose of the stream when the PdfViewerForm is closed
                    pdfViewerForm.FormClosed += (s, args) => stream.Dispose();
                }
                catch (Exception ex)
                {
                    // Show an error message if an exception occurs
                    MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    // Ensure the stream is disposed of in case of an error
                    stream.Dispose();
                }
            }
            else
            {
                // Show an error message if the selected appointment is not found
                MessageBox.Show("Selected appointment not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (searchLookUpEdit1.EditValue == null)
            {
                MessageBox.Show("Please select an appointment from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the selected appointment ID
            Guid selectedAppointmentId = (Guid)searchLookUpEdit1.EditValue;

            // Confirm cancellation
            var confirmResult = MessageBox.Show("Are you sure you want to cancel this appointment?", "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    // Cancel the appointment
                    await _appointmentService.DeleteAppointmentAsync(selectedAppointmentId);
                    MessageBox.Show("Appointment canceled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh the upcoming appointments list
                    var upcomingAppointments = (await _appointmentService.GetUpcomingAppointmentsAsync(_currentUser.UserId)).ToList();
                    searchLookUpEdit1.Properties.DataSource = upcomingAppointments;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to cancel appointment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void label6_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is not set. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var accountSettingsForm = _serviceProvider.GetRequiredService<AccountSettingDashboard>();
            accountSettingsForm.SetUserData(new UserDTO
            {
                UserId = _currentUser.UserId,
                Name = _currentUser.Name,
                Email = _currentUser.Email,
                DateOfBirth = _currentUser.DateOfBirth,
                PhoneNumber = _currentUser.PhoneNumber,
                Gender = _currentUser.Gender
            });
            accountSettingsForm.Show();
            this.Hide();
        }
    }
}