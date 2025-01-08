using BusinessLL;
using DataTransferO;
using HMS_CHANGE.Dashboard;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMS_CHANGE.Doctor
{
    public partial class AccountDoc : Form
    {
        private readonly UserService _userService;
        private readonly DoctorService _doctorService;
        private readonly ImageService _imageService;
        private readonly IServiceProvider _serviceProvider;
        private UserDTO _currentUser;
        private DoctorDetailDTO _currentDoctorDetail;

        public AccountDoc(UserService userService, DoctorService doctorService, ImageService imageService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _userService = userService;
            _doctorService = doctorService;
            _imageService = imageService;
            _serviceProvider = serviceProvider;
            _currentUser = new UserDTO();
            _currentDoctorDetail = new DoctorDetailDTO();

            LoadUserInfo();
        }

        public void SetUserData(UserDTO currentUser, DoctorDetailDTO currentDoctorDetail)
        {
            _currentUser = currentUser;
            _currentDoctorDetail = currentDoctorDetail;
            PopulateUserInfo();
            label20.Text = currentUser.Name;
        }

        private async void LoadUserInfo()
        {
            try
            {
                var currentUser = UserSession.CurrentUser;
                if (currentUser != null)
                {
                    _currentUser = currentUser;
                    _currentDoctorDetail = await _doctorService.GetDoctorByUserIdAsync(_currentUser.UserId);
                    PopulateUserInfo();
                }
                else
                {
                    MessageBox.Show("User information could not be loaded.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load user information: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateUserInfo()
        {
            textBox1.Text = _currentUser.Name ?? string.Empty;
            textBox2.Text = _currentUser.Email;
            textBox3.Text = _currentUser.PhoneNumber ?? string.Empty;
            dateTimePicker1.Value = _currentUser.DateOfBirth.ToDateTime(TimeOnly.MinValue);
            if (_currentUser.Gender == "Female")
            {
                radioButton1.Checked = true;
            }
            else if (_currentUser.Gender == "Male")
            {
                radioButton2.Checked = true;
            }

            textBox4.Text = _currentDoctorDetail.Qualification ?? string.Empty;
            textBox8.Text = _currentDoctorDetail.Specialization ?? string.Empty;
            textBox9.Text = _currentDoctorDetail.ExperienceYears?.ToString() ?? string.Empty;
            textBox10.Text = _currentDoctorDetail.Description ?? string.Empty;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Update user information from form fields
                _currentUser.Name = textBox1.Text;
                _currentUser.Email = textBox2.Text;
                _currentUser.PhoneNumber = textBox3.Text;
                _currentUser.DateOfBirth = DateOnly.FromDateTime(dateTimePicker1.Value);
                _currentUser.Gender = radioButton1.Checked ? "Female" : "Male";

                // Validate user data
                ValidateUserData(_currentUser);

                // Update user in the database
                await _userService.UpdateUserInforAsync(_currentUser);

                // Update doctor details from form fields
                _currentDoctorDetail.Qualification = textBox4.Text;
                _currentDoctorDetail.Specialization = textBox8.Text;
                _currentDoctorDetail.ExperienceYears = int.TryParse(textBox9.Text, out int experienceYears) ? experienceYears : (int?)null;
                _currentDoctorDetail.Description = textBox10.Text;

                // Update doctor details in the database
                await _doctorService.UpdateDoctorAsync(_currentDoctorDetail);

                // Update UserSession
                UserSession.CurrentUser = await _userService.GetUserByIdAsync(_currentUser.UserId);

                MessageBox.Show("Your information has been updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update user information: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ValidateUserData(UserDTO userDto)
        {
            if (string.IsNullOrWhiteSpace(userDto.Name))
                throw new ArgumentException("Name cannot be empty.");

            if (!_userService.IsValidEmail(userDto.Email))
                throw new ArgumentException("Invalid email format.");

            // Add more validations as needed
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Upload image button for doctor
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                    openFileDialog.Title = "Select a Profile Picture";

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;

                        // Validate the selected file
                        if (!_imageService.IsValidImageFile(filePath))
                        {
                            MessageBox.Show("Invalid image file format. Please select a valid image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Convert the image to a byte array
                        byte[] imageData = await File.ReadAllBytesAsync(filePath);

                        // Upload the image and update the doctor's profile picture
                        Guid imageId = await _imageService.AddOrUpdateDoctorImageAsync(_currentUser.UserId, imageData);

                        // Load the updated profile picture
                        await LoadUserProfilePicture();

                        MessageBox.Show("Profile picture updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the inner exception details for better debugging
                var innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                MessageBox.Show($"Failed to update profile picture: {ex.Message}. Inner exception: {innerExceptionMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task LoadUserProfilePicture()
        {
            try
            {
                var imageDto = await _imageService.GetImageByEntityIdAsync(_currentUser.UserId);
                if (imageDto?.ImageData != null)
                {
                    using (var ms = new MemoryStream(imageDto.ImageData))
                    {
                        pictureBox3.Image = Image.FromStream(ms);
                        pictureBox1.Image = Image.FromStream(ms);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load profile picture: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void label10_Click(object sender, EventArgs e)
        {
            // Set account to deactivate isActive = false; isDeleted = true
            try
            {
                var confirmResult = MessageBox.Show("Are you sure you want to deactivate your account?", "Confirm Deactivation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirmResult == DialogResult.Yes)
                {
                    await _userService.DeactivateUserAsync(_currentUser.UserId);

                    // Update UserSession
                    UserSession.CurrentUser = null;

                    MessageBox.Show("Your account has been deactivated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Optionally, navigate to the login screen or close the application
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to deactivate account: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            // Change password button
            try
            {
                string currentPassword = textBox6.Text;
                string newPassword = textBox5.Text;
                string confirmPassword = textBox7.Text;

                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("New password and confirmation password do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                await _userService.ChangeUserPasswordAsync(_currentUser.UserId, currentPassword, newPassword);

                MessageBox.Show("Your password has been changed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to change password: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ForgotPassLb_Click(object sender, EventArgs e)
        {
            // Forward to forgot password
            // Use stack to navigate back
            MainBoard.NavigationHistory.Push(this);

            // Navigate to the ForgetPass form
            var forgetPassForm = _serviceProvider.GetRequiredService<ForgetPass>();
            forgetPassForm.Show();
            this.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            var orderAppointmentForm = _serviceProvider.GetRequiredService<OrderAppointment>();
            orderAppointmentForm.SetOrderDetails(_currentDoctorDetail.DoctorId, new List<Guid>(), _currentUser, null);
            orderAppointmentForm.Show();
            this.Hide();
        }

        private void label14_Click(object sender, EventArgs e)
        {
            var dashBoardDoctorForm = _serviceProvider.GetRequiredService<DashBoardDoctor>();
            dashBoardDoctorForm.SetDoctorId(_currentDoctorDetail.DoctorId);
            dashBoardDoctorForm.Show();
            this.Hide();
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // qualification
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            // specializations 
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            //experience years
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            //description
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //name
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            //email
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // phone
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // date of birth
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //radio button female
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            //radio button male
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }
    }
}
