using AutoMapper;
using BusinessLL;
using BusinessLL.AdminBLL;
using DataTransferO;
using HMS_CHANGE.Admin;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMS_CHANGE.Dashboard
{
    public partial class DashBoardAdmin : Form
    {
        private readonly DoctorManagementService _doctorManagementService;
        private readonly DoctorService _doctorService;
        private readonly UserService _userService;
        private readonly IMapper _mapper;
        private readonly IServiceProvider _serviceProvider;
        private UserDTO _currentUser;

        public DashBoardAdmin(DoctorService doctorService, UserService userService, IMapper mapper, DoctorManagementService doctorManagementService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _doctorService = doctorService;
            _userService = userService;
            _mapper = mapper;
            _doctorManagementService = doctorManagementService;
            _serviceProvider = serviceProvider;
            InitializeDataGridView();
            LoadDoctorData();
            LoadInactiveDoctorData();
            LoadDepartments();

            dataGridView1.CellClick += dataGridView1_CellClick;
            dataGridView2.CellClick += dataGridView2_CellClick;

            _currentUser = UserSession.CurrentUser;
            label2.Text = _currentUser.Name;
        }

        private void InitializeDataGridView()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", DataPropertyName = "Name" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", DataPropertyName = "Email" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "PhoneNumber", HeaderText = "Phone Number", DataPropertyName = "PhoneNumber" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "DateOfBirth", HeaderText = "Date of Birth", DataPropertyName = "DateOfBirth" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "Gender", HeaderText = "Gender", DataPropertyName = "Gender" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "Specialization", HeaderText = "Specialization", DataPropertyName = "Specialization" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "Qualification", HeaderText = "Qualification", DataPropertyName = "Qualification" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "ExperienceYears", HeaderText = "Experience Years", DataPropertyName = "ExperienceYears" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "Description", HeaderText = "Description", DataPropertyName = "Description" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "Department", HeaderText = "Department", DataPropertyName = "DepartmentName" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsActive", HeaderText = "Is Active", DataPropertyName = "IsActive" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsDeleted", HeaderText = "Is Deleted", DataPropertyName = "IsDeleted" });

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView1.ScrollBars = ScrollBars.Both;

            dataGridView2.AutoGenerateColumns = false;
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", DataPropertyName = "Name", HeaderText = "Name" });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", DataPropertyName = "Email", HeaderText = "Email" });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn { Name = "PhoneNumber", DataPropertyName = "PhoneNumber", HeaderText = "Phone Number" });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsActive", DataPropertyName = "IsActive", HeaderText = "Is Active" });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsDeleted", DataPropertyName = "IsDeleted", HeaderText = "Is Deleted" });
        }

        private async void LoadDoctorData()
        {
            try
            {
                var doctors = await _doctorService.GetAllActiveDoctorsAsync();
                var doctorDetails = _mapper.Map<List<DoctorGridViewDTO>>(doctors);
                dataGridView1.DataSource = new BindingList<DoctorGridViewDTO>(doctorDetails);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load doctor data");
                MessageBox.Show($"Failed to load doctor data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadDepartments()
        {
            try
            {
                var departments = await _doctorService.GetDepartmentsAsync();
                comboBox1.DataSource = departments;
                comboBox1.DisplayMember = "DepartmentName";
                comboBox1.ValueMember = "DepartmentId";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load departments");
                MessageBox.Show($"Failed to load departments: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedRow = dataGridView1.Rows[e.RowIndex].DataBoundItem as DoctorGridViewDTO;
                if (selectedRow != null)
                {
                    textBox1.Text = selectedRow.Name;
                    textBox3.Text = selectedRow.PhoneNumber;
                    textBox2.Text = selectedRow.Email ?? string.Empty;
                    dateTimePicker1.Value = selectedRow.DateOfBirth.ToDateTime(TimeOnly.MinValue);
                    textBox8.Text = selectedRow.Specialization;
                    textBox4.Text = selectedRow.Qualification;
                    textBox9.Text = selectedRow.ExperienceYears?.ToString() ?? "0";
                    textBox10.Text = selectedRow.Description;
                    comboBox1.SelectedValue = selectedRow.DepartmentId;

                    if (selectedRow.Gender == "Female")
                    {
                        radioButton1.Checked = true;
                    }
                    else if (selectedRow.Gender == "Male")
                    {
                        radioButton2.Checked = true;
                    }
                    else
                    {
                        radioButton1.Checked = false;
                        radioButton2.Checked = false;
                    }
                }
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show("Email cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var userDto = new UserDTO
                {
                    Name = textBox1.Text,
                    Email = textBox2.Text,
                    PhoneNumber = textBox3.Text,
                    DateOfBirth = DateOnly.FromDateTime(dateTimePicker1.Value),
                    Gender = radioButton1.Checked ? "Female" : radioButton2.Checked ? "Male" : null,
                    RoleId = _userService.GetRoleIdByName("Doctor"),
                    IsActive = true,
                    IsDeleted = false,
                    PasswordHash = _userService.HashPassword("123be ")
                };

                Log.Information("UserDTO before adding doctor: {@UserDto}", userDto);

                var doctorDto = new DoctorDetailDTO
                {
                    Specialization = textBox8.Text,
                    Qualification = textBox4.Text,
                    ExperienceYears = int.TryParse(textBox9.Text, out int experienceYears) ? experienceYears : (int?)null,
                    Description = textBox10.Text,
                    DepartmentId = comboBox1.SelectedValue as Guid?,
                    UserId = userDto.UserId,
                    Phone = userDto.PhoneNumber
                };

                await _doctorManagementService.AddDoctorAsync(userDto, doctorDto);
                MessageBox.Show("Doctor added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDoctorData();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to add doctor");
                MessageBox.Show($"Failed to add doctor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    var selectedRow = dataGridView1.SelectedRows[0].DataBoundItem as DoctorGridViewDTO;
                    if (selectedRow != null)
                    {
                        var doctorDto = new DoctorDetailDTO
                        {
                            DoctorId = selectedRow.DoctorId,
                            UserId = selectedRow.UserId,
                            User = new UserDTO
                            {
                                UserId = selectedRow.UserId,
                                Name = textBox1.Text,
                                Email = textBox2.Text,
                                PhoneNumber = textBox3.Text,
                                DateOfBirth = DateOnly.FromDateTime(dateTimePicker1.Value),
                                Gender = radioButton1.Checked ? "Female" : radioButton2.Checked ? "Male" : null,
                                RoleId = _userService.GetRoleIdByName("Doctor")
                            },
                            Specialization = textBox8.Text,
                            Qualification = textBox4.Text,
                            ExperienceYears = int.TryParse(textBox9.Text, out int experienceYears) ? experienceYears : (int?)null,
                            Description = textBox10.Text,
                            DepartmentId = comboBox1.SelectedValue as Guid?
                        };

                        await _doctorManagementService.UpdateDoctorAsync(doctorDto);
                        MessageBox.Show("Doctor updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDoctorData();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a doctor to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to update doctor");
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner exception details");
                }
                MessageBox.Show($"Failed to update doctor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox8.Clear();
            textBox9.Clear();
            textBox10.Clear();
            dateTimePicker1.Value = DateTime.Now;
            comboBox1.SelectedIndex = -1;
            radioButton1.Checked = false;
            radioButton2.Checked = false;
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    var selectedRow = dataGridView1.SelectedRows[0].DataBoundItem as DoctorGridViewDTO;
                    if (selectedRow != null)
                    {
                        await _doctorService.DeleteDoctorAsync(selectedRow.DoctorId);
                        MessageBox.Show("Doctor deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDoctorData();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a doctor to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete doctor");
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner exception details");
                }
                MessageBox.Show($"Failed to delete doctor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView2.SelectedRows.Count > 0)
                {
                    var selectedRow = dataGridView2.SelectedRows[0].DataBoundItem as DoctorGridViewDTO;
                    if (selectedRow != null)
                    {
                        await _userService.ReactivateDoctorAsync(selectedRow.UserId);
                        MessageBox.Show("Doctor reactivated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadInactiveDoctorData();
                        LoadDoctorData();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a doctor to reactivate.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to reactivate doctor");
                MessageBox.Show($"Failed to reactivate doctor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadInactiveDoctorData()
        {
            try
            {
                var inactiveDoctors = await _userService.GetInactiveDoctorsAsync();
                Log.Information("Fetched inactive doctors: {@InactiveDoctors}", inactiveDoctors);

                var doctorDetails = _mapper.Map<List<DoctorGridViewDTO>>(inactiveDoctors);
                Log.Information("Mapped inactive doctors to DoctorGridViewDTO: {@DoctorDetails}", doctorDetails);

                dataGridView2.DataSource = new BindingList<DoctorGridViewDTO>(doctorDetails);
                Log.Information("Bound inactive doctors to dataGridView2");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load inactive doctor data");
                MessageBox.Show($"Failed to load inactive doctor data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Name of the doctor
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // Email of the doctor
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // Phone number of the doctor
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            // Specialization of the doctor
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // Qualification of the doctor
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            // Experience years of the doctor
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            // Description of the doctor
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Department of the doctor
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // Date of birth of the doctor
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            // Female radio button checked
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            // Male radio button checked
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is missing. Please log in again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var departmentAssignForm = _serviceProvider.GetRequiredService<DepartmentAssign>();
            departmentAssignForm.SetUserData(_currentUser);
            departmentAssignForm.Show();
            this.Hide();
        }

        private void label3_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show("User information is missing. Please log in again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var appointmentStatsForm = _serviceProvider.GetRequiredService<AppointmentStats>();
            appointmentStatsForm.SetUserData(_currentUser);
            appointmentStatsForm.Show();
            this.Hide();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
