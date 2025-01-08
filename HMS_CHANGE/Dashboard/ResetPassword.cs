using System;
using System.Windows.Forms;
using BusinessLL;
using Microsoft.Extensions.DependencyInjection;

namespace HMS_CHANGE.Dashboard
{
    public partial class ResetPassword : Form
    {
        private readonly UserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private Guid _userId;

        public ResetPassword(UserService userService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _userService = userService;
            _serviceProvider = serviceProvider;
        }

        public void SetUserId(Guid userId)
        {
            _userId = userId;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string newPassword = textBox1.Text;
            string confirmPassword = textBox2.Text;

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please enter and confirm your new password.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.");
                return;
            }

            var user = await _userService.GetUserByIdAsync(_userId);
            if (user == null)
            {
                MessageBox.Show("User not found.");
                return;
            }

            user.PasswordHash = _userService.HashPassword(newPassword);

            try
            {
                await _userService.UpdateUserAsync(user);
                MessageBox.Show("Password reset successful. Please log in with your new password.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to reset password: {ex.Message}");
                Console.WriteLine(ex.ToString());
            }

            var mainBoard = _serviceProvider.GetRequiredService<MainBoard>();
            mainBoard.Show();
            this.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void ResetPassword_Load(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {
            
            var forgetPass = _serviceProvider.GetRequiredService<ForgetPass>();
            forgetPass.Show();
            this.Hide();
           
        }
    }
}

