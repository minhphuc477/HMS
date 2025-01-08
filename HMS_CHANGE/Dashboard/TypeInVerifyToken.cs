using BusinessLL;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;

namespace HMS_CHANGE.Dashboard
{
    public partial class TypeInVerifyToken : Form
    {
        private readonly UserService _userService;
        private readonly IServiceProvider _serviceProvider;
        private Guid _userId;

        public TypeInVerifyToken(UserService userService, IServiceProvider serviceProvider)
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
            string token = textBox1.Text;

            // Verify the token
            var resetToken = await _userService.VerifyResetTokenAsync(token);
            if (resetToken == null || resetToken.UserId != _userId)
            {
                MessageBox.Show("Invalid or expired token.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Token verified successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Navigate to ResetPass form
            var resetPassForm = _serviceProvider.GetRequiredService<ResetPassword>();
            resetPassForm.SetUserId(_userId);
            resetPassForm.Show();
            this.Hide();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {
            // Transfer back to the ForgetPass form
            var forgetPassForm = _serviceProvider.GetRequiredService<ForgetPass>();
            forgetPassForm.Show();
            this.Close();
        }
    }
}
