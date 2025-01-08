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
using DataTransferO; // Reference to Data Transfer Objects
using BCrypt.Net; // Reference to BCrypt.Net-Next for password hashing
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions; // For configuration

namespace HMS_CHANGE.Dashboard
{
    public partial class SignUp : Form
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly EmailService _emailService;

        public SignUp(UserService userService, IConfiguration configuration, IServiceProvider serviceProvider, EmailService emailService)
        {
            InitializeComponent();
            _userService = userService;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _emailService = emailService;

            this.FormClosed += new FormClosedEventHandler(SignUp_FormClosed); // Subscribe to FormClosed event
        }

        // FormClosed event handler
        private void SignUp_FormClosed(object? sender, FormClosedEventArgs e)
        {
            // Show the mainboard (main dashboard) form
            var mainboardForm = _serviceProvider.GetRequiredService<MainBoard>();
            mainboardForm.Show();
        }

        private void label7_Click(object sender, EventArgs e)
        {
            var loginForm = _serviceProvider.GetRequiredService<MainBoard>();
            loginForm.Show();
            this.Hide();
        }

        private void SignUpLb_Click(object sender, EventArgs e)
        {
            // Navigate to Sign Up form
            var signUpForm = _serviceProvider.GetRequiredService<SignUp>();
            signUpForm.Show();
            this.Hide();
        }

        // Name 
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        // Email 
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        //Phone number
        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        //Password
        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        //a checkbox to show password and hide password
        private void checkShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if (checkShowPass.Checked)
            {
                textBox3.PasswordChar = '\0'; // Show password
            }
            else
            {
                textBox3.PasswordChar = '*'; // Hide password
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        //Female radio button
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        // Male radio button 
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        // SELECT ROLE User Doctor Admin 
        private void RoleCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check if a role is selected
            if (RoleCb.SelectedItem != null)
            {
                // Show Admin Key text box and label if Admin or Doctor is selected
                if (RoleCb.SelectedItem.ToString() == "Admin" || RoleCb.SelectedItem.ToString() == "Doctor")
                {
                    AdminKEY.Visible = true;
                    key.Visible = true; // Ensure key is your label's name
                }
                else
                {
                    AdminKEY.Visible = false;
                    key.Visible = false;
                }
            }
        }

        // If choose Admin or doctor, a textbox will appear to enter the Admin Key
        private void AdminKEY_TextChanged(object sender, EventArgs e)
        {

        }

        // Sign up button
        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Collect data from form
                string name = textBox1.Text;
                string email = textBox2.Text;
                string phoneNumber = textBox4.Text;
                string password = textBox3.Text;
                DateOnly dateOfBirth = DateOnly.FromDateTime(dateTimePicker1.Value);
                string gender = radioButton1.Checked ? "Female" : radioButton2.Checked ? "Male" : "Other";
                string? selectedRole = RoleCb.SelectedItem?.ToString();

                // Validate data
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phoneNumber) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Please fill all required fields.");
                    return;
                }

                if (!_userService.IsValidEmail(email))
                {
                    MessageBox.Show("Invalid email format.");
                    return;
                }

                if (string.IsNullOrEmpty(selectedRole))
                {
                    MessageBox.Show("Please select a role.");
                    return;
                }

                Guid roleId = _userService.GetRoleIdByName(selectedRole);

                // Check for Admin/Doctor Key
                if (selectedRole == "Admin" || selectedRole == "Doctor")
                {
                    string adminKey = AdminKEY.Text;
                    string? configKey = _configuration["KeyToAdminDoc:Key"];
                    if (string.IsNullOrEmpty(configKey) || adminKey != configKey)
                    {
                        MessageBox.Show("Invalid Admin/Doctor key.");
                        return;
                    }
                }

                // Hash password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                // Create UserDTO
                var userDto = new UserDTO(Guid.NewGuid(), name, dateOfBirth, email, phoneNumber, gender, roleId, hashedPassword)
                {
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Call UserService to create user
                await _userService.CreateUserAsync(userDto);

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(email, name);

                MessageBox.Show("Sign up successful!");
                // Optionally, redirect to another form, such as the login form
                var loginForm = _serviceProvider.GetRequiredService<MainBoard>();
                loginForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void SignUp_Load(object sender, EventArgs e)
        {
            // Auto-select "User" if no role is selected
            if (RoleCb.Items.Count > 0 && RoleCb.SelectedIndex == -1)
            {
                RoleCb.SelectedIndex = RoleCb.Items.IndexOf("Patient");
            }

            // Hide password by default
            textBox3.PasswordChar = '*';
            AdminKEY.Visible = false;
            key.Visible = false;
        }

        private void key_Click(object sender, EventArgs e)
        {

        }
    }
}
