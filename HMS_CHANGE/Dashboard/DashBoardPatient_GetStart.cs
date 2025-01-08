using BusinessLL;
using DataTransferO;
using HMS_CHANGE.Patient;
using HMS_CHANGE.Patient.Account_Setting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMS_CHANGE.Dashboard
{
    public partial class DashBoardPatient_GetStart : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly EmailService _emailService;
        private readonly DoctorService _doctorService;
        private readonly ForwardService _forwardService;
        private readonly UserService _userService;
        private UserDTO? _currentUser;

        public DashBoardPatient_GetStart(EmailService emailService, DoctorService doctorService, ForwardService forwardService, UserService userService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _emailService = emailService;
            _doctorService = doctorService;
            _forwardService = forwardService;
            _userService = userService;
            _serviceProvider = serviceProvider;

            richTextBox1.ReadOnly = true;
            SetNavigationTutorial(richTextBox1);

            // Set the current user from UserSession
            _currentUser = UserSession.CurrentUser;
            if (_currentUser != null)
            {
                SetWelcomeMessage(_currentUser);
            }
        }

        // Update the SetWelcomeMessage method to accept a UserDTO object
        public void SetWelcomeMessage(UserDTO user)
        {
            label20.Text = $"{user.Name}";
            label10.Text = $"Hello, and Welcome {user.Name}!";
            label2.Text = $"If you did not receive our greeting in your {user.Email} yet, please check your email to ensure it is usable.";
            label12.Text = $"Please click on this link to send another greeting to {user.Email} to make sure that it works!";
        }

        private async void label12_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is not set. Please provide valid user information.");
                return;
            }

            try
            {
                await _emailService.SendWelcomeEmailAsync(_currentUser.Email, _currentUser.Name);
                MessageBox.Show("Welcome email sent successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while sending the email: {ex.Message}");
            }
        }

        // Empty event handlers can be removed if not needed
        private void label4_Click(object sender, EventArgs e) { }
        private void label10_Click(object sender, EventArgs e) { }
        private void panel4_PPaint(object sender, PaintEventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void SetNavigationTutorial(RichTextBox richTextBox1)
        {
            richTextBox1.Rtf = @"{\rtf1\ansi {\fonttbl\f0\fswiss Helvetica;} 
                                                          {\colortbl;\red0\green0\blue0;\red0\green0\blue255;} 
                                                          \viewkind4\uc1\pard\lang1033\f0\fs24\cf1 Welcome to the Patient Dashboard! \cf0\fs20\par \par 

                                                           \pard\sa200\sl276\slmult1\fs20\b Getting Started:\b0\par 
                                                            - \b\ul Home Page/ DashBoard\b0\ulnone: This is where you'll find your overview. It includes your upcoming appointments, recent notifications, and quick links to important sections.\par 
                                                            - \b\ul Appointments\b0\ulnone: Manage your appointments easily. You can schedule new appointments, view upcoming ones, and see past appointment history.\par 
                                                            - \b\ul Medical Records\b0\ulnone: Access your medical history, including test results, treatment plans, and visit summaries. Your records are just a click away.\par 
                                                            - \b\ul Prescriptions\b0\ulnone: View your current prescriptions and request refills directly from your dashboard.\par 
                                                            - \b\ul Billing\b0\ulnone: Stay on top of your payments. View billing statements and make payments securely through this section.\par 
                                                            - \b\ul Notifications\b0\ulnone: Important alerts and messages about your healthcare will appear here. Make sure to check it regularly.\par 

                                                            \pard\sa200\sl276\slmult1\b Navigation Tips:\b0\par 
                                                             - \b\ul Navigation Bar\b0\ulnone: Use the navigation bar at the top of the screen to quickly move between different sections of the dashboard.\par 
                                                             - \b\ul Profile and Account \b0\ulnone: Click on your profile icon to update your personal information, insurance details, and preferences.\par \par 

                                                             \pard\sa200\sl276\slmult1\cf2\b Thank you for using our System Booking! Your health is our top priority.\b0\cf0\par }";
        }

        private async void label17_Click(object sender, EventArgs e)
        {
            await NavigateToBookAppointmentAsync();
        }

        private async void label19_Click(object sender, EventArgs e)
        {
            await NavigateToBookAppointmentAsync();
        }

        private async Task NavigateToBookAppointmentAsync()
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is not set. Please provide valid user information.");
                return;
            }

            try
            {
                var appointment = _serviceProvider.GetRequiredService<Patient.BookAppointment>();
                appointment.SetWelcomeMessage(_currentUser);

                // Show the new form and hide the current one
                appointment.Show();
                this.Hide();

                // Dispose of the current form to release resources
                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to navigate to BookAppointment: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label18_Click(object sender, EventArgs e)
        {
        }

        private void label20_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void label21_Click(object sender, EventArgs e)
        {
        }

        private void label16_Click(object sender, EventArgs e)
        {
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
        }

        private void label14_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is missing. Please log in again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var home = _serviceProvider.GetRequiredService<DashBoardPatient_GetStart>();
            home.SetWelcomeMessage(_currentUser);
            home.Show();
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
