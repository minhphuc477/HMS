using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLL; // Reference to Business Logic Layer
using DataTransferO;
using HMS_CHANGE.Dashboard;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS_CHANGE
{
    public partial class MainBoard : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        public MainBoard(IServiceProvider serviceProvider, UserService userService, IConfiguration configuration)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _userService = userService;
            _configuration = configuration;
        }

        public static class NavigationHistory
        {
            private static Stack<Form> history = new Stack<Form>();

            public static void Push(Form form)
            {
                history.Push(form);
            }

            public static Form Pop()
            {
                return history.Pop();
            }

            public static int Count => history.Count;
        }

        private void MainBoard_Load(object sender, EventArgs e)
        {
        }

        private void SignUpLb_Click(object sender, EventArgs e)
        {
            // Navigate to Sign Up form
            var signUpForm = _serviceProvider.GetRequiredService<HMS_CHANGE.Dashboard.SignUp>();
            signUpForm.Show();
            this.Hide();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ToSignUpLb_Click(object sender, EventArgs e)
        {
            var signUpForm = _serviceProvider.GetRequiredService<HMS_CHANGE.Dashboard.SignUp>();
            signUpForm.Show();
            this.Hide();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }

        private void ForgotPassLb_Click(object sender, EventArgs e)
        {
            // Debugging: Check the current form and stack count
            Console.WriteLine("Current form: MainBoard");
            Console.WriteLine("Stack count before push: " + NavigationHistory.Count);

            NavigationHistory.Push(this);

            // Debugging: Check the stack count after push
            Console.WriteLine("Stack count after push: " + NavigationHistory.Count);

            var forgetPassForm = _serviceProvider.GetRequiredService<ForgetPass>();
            forgetPassForm.Show();
            this.Hide();
        }

        private void RedirectedUserByRole(string? roleName)
        {
            switch (roleName?.Trim().ToLower())
            {
                case "admin":
                    var adminDashboard = _serviceProvider.GetRequiredService<DashBoardAdmin>();
                    adminDashboard.Show();
                    break;
                case "doctor":
                    var doctorDashboard = _serviceProvider.GetRequiredService<DashBoardDoctor>();
                    doctorDashboard.SetDoctorId(UserSession.CurrentUser.UserId); // Pass the doctor ID
                    doctorDashboard.Show();
                    break;
                case "patient":
                    var patientDashboard = _serviceProvider.GetRequiredService<DashBoardPatient_GetStart>();
                    patientDashboard.Show();
                    break;
                default:
                    MessageBox.Show("Role not recognized. Unable to redirect.");
                    break;
            }
        }

        private async void LogInBt_Click(object sender, EventArgs e)
        {
            try
            {
                string emailorUserName = userGmailTb.Text;
                string password = LogInPassTb.Text;

                if (string.IsNullOrEmpty(emailorUserName) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please fill all the fields");
                    return;
                }

                UserDTO user;
                try
                {
                    user = await _userService.LoginAsync(emailorUserName, password);
                    _userService.SetCurrentUserId(user.UserId); // Set current user ID here
                    UserSession.CurrentUser = user; // Update UserSession
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                MessageBox.Show("Login Successful");

                var roleName = _userService.GetRoleNameById(user.RoleId);
                if (string.IsNullOrEmpty(roleName))
                {
                    MessageBox.Show("User role not recognized");
                    return;
                }

                RedirectedUserByRole(roleName);
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void userGmailTb_TextChanged(object sender, EventArgs e)
        {

        }

        private void checkShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if (checkShowPass.Checked)
            {
                LogInPassTb.UseSystemPasswordChar = false; // Show password
            }
            else
            {
                LogInPassTb.UseSystemPasswordChar = true; // Hide password
            }
        }

        private void LogInPassTb_TextChanged(object sender, EventArgs e)
        {
            if (!checkShowPass.Checked)
            {
                LogInPassTb.UseSystemPasswordChar = true; // Ensure it's hidden
            }
        }
    }
}
