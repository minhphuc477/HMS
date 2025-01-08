using BusinessLL;
using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace HMS_CHANGE.Dashboard
{
    public partial class ForgetPass : Form
    {
        private readonly UserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private readonly EmailService _emailService;

        public ForgetPass(UserService userService, IServiceProvider serviceProvider, EmailService emailService)
        {
            InitializeComponent();
            _userService = userService;
            _serviceProvider = serviceProvider;
            _emailService = emailService;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text;

            // Check if the email exists in the database
            var user = await _userService.FindUserByEmailAsync(email);
            if (user == null)
            {
                MessageBox.Show("Email does not exist in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Generate reset token
            var resetToken = await _userService.GenerateResetTokenAsync(user.UserId);

            // Send reset token via email
            await _emailService.SendResetTokenEmailAsync(email, resetToken.Token);

            MessageBox.Show("A reset token has been sent to your email.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Navigate to VerifyToken form
            var verifyTokenForm = _serviceProvider.GetRequiredService<TypeInVerifyToken>();
            verifyTokenForm.SetUserId(user.UserId);
            verifyTokenForm.Show();
            this.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        // this should lead user to the account dashboard: using stack to navigate back to the previous form
        private void label3_Click(object sender, EventArgs e)
        {
            if (MainBoard.NavigationHistory.Count > 0)
            {
                var previousForm = MainBoard.NavigationHistory.Pop();
                previousForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("No previous form to go back to.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ForgetPass_Load(object sender, EventArgs e)
        {

        }
    }
}
