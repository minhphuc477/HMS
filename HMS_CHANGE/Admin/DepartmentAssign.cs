using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BusinessLL;
using DataTransferO;
using DevExpress.XtraCharts;
using Microsoft.Extensions.DependencyInjection;

namespace HMS_CHANGE.Admin
{
    public partial class DepartmentAssign : Form
    {
        private readonly DoctorService _doctorService;
        private readonly UserService _userService;
        private readonly AppointmentService _appointmentService;
        private readonly IServiceProvider _serviceProvider;
        private UserDTO _currentUser;

        public DepartmentAssign(DoctorService doctorService, UserService userService, AppointmentService appointmentService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _doctorService = doctorService;
            _userService = userService;
            _appointmentService = appointmentService;
            InitializeDataGridView1();
            InitializeDataGridView2();
            InitializeChartControl();
            InitializeSearchLookUpEdit();
            InitializeSearchLookUpEdit2();
            LoadDepartmentData();
            LoadDoctorData();
            LoadChartData();
            _serviceProvider = serviceProvider;
        }

        public void SetUserData(UserDTO user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            label2.Text = $"Welcome, {_currentUser.Name}";
        }

        private void InitializeDataGridView1()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.AutoGenerateColumns = false;

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DepartmentName",
                HeaderText = "Department Name",
                DataPropertyName = "DepartmentName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DepartmentCode",
                HeaderText = "Department Code",
                DataPropertyName = "DepartmentCode",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                HeaderText = "Description",
                DataPropertyName = "Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Diseases",
                HeaderText = "Diseases",
                DataPropertyName = "Diseases",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridView1.SelectionChanged += dataGridView1_SelectionChanged;
        }

        private void InitializeDataGridView2()
        {
            dataGridView2.Columns.Clear();
            dataGridView2.AutoGenerateColumns = false;

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DoctorName",
                HeaderText = "Doctor Name",
                DataPropertyName = "DoctorName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Specialization",
                HeaderText = "Specialization",
                DataPropertyName = "Specialization",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Phone",
                HeaderText = "Phone",
                DataPropertyName = "Phone",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "DepartmentName",
                HeaderText = "Department",
                DataPropertyName = "DepartmentName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private void InitializeChartControl()
        {
            chartControl1.Series.Clear();

            Series barSeries = new Series("Patients", ViewType.Bar);
            barSeries.LabelsVisibility = DevExpress.Utils.DefaultBoolean.True;
            barSeries.View.Color = Color.Blue;

            chartControl1.Series.Add(barSeries);

            XYDiagram diagram = (XYDiagram)chartControl1.Diagram;
            diagram.AxisX.Title.Text = "Departments";
            diagram.AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
            diagram.AxisY.Title.Text = "Number of Patients";
            diagram.AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
        }

        private async void LoadDepartmentData()
        {
            try
            {
                var departments = await _doctorService.GetDepartmentsDesAsync();
                var departmentDetails = departments.Select(d => new
                {
                    d.DepartmentId,
                    d.DepartmentName,
                    d.DepartmentCode,
                    d.Description,
                    Diseases = string.Join(", ", d.Diseases.Select(ds => ds.DiseaseName))
                }).ToList();

                // Log the fetched data
                foreach (var dept in departmentDetails)
                {
                    Console.WriteLine($"Department: {dept.DepartmentName}, Diseases: {dept.Diseases}");
                }

                dataGridView1.DataSource = new BindingList<object>(departmentDetails.Cast<object>().ToList());

                // Bind departments to SearchLookUpEdit controls
                searchLookUpEdit1.Properties.DataSource = departments;
                searchLookUpEdit2.Properties.DataSource = departments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load department data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadDoctorData()
        {
            try
            {
                var doctors = await _doctorService.GetAllDoctorsAsync();
                var doctorDetails = doctors.Select(d => new
                {
                    DoctorName = d.User.Name,
                    d.Specialization,
                    d.Phone,
                    DepartmentName = d.Department?.DepartmentName
                }).ToList();

                dataGridView2.DataSource = new BindingList<object>(doctorDetails.Cast<object>().ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load doctor data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadChartData()
        {
            try
            {
                var departments = await _doctorService.GetDepartmentsDesAsync();
                var departmentPatientCounts = new Dictionary<string, int>();

                foreach (var department in departments)
                {
                    int patientCount = await _doctorService.GetPatientCountForDepartmentAsync(department.DepartmentId);
                    departmentPatientCounts[department.DepartmentName] = patientCount;

                    // Log the patient count for debugging
                    Console.WriteLine($"Department: {department.DepartmentName}, Patient Count: {patientCount}");
                }

                Series barSeries = chartControl1.Series[0];

                barSeries.Points.Clear();

                foreach (var kvp in departmentPatientCounts)
                {
                    barSeries.Points.Add(new SeriesPoint(kvp.Key, kvp.Value));
                }

                chartControl1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load chart data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0].DataBoundItem as dynamic;
                if (selectedRow != null)
                {
                    textBox1.Text = selectedRow.DepartmentName;
                    textBox8.Text = selectedRow.DepartmentCode;
                    textBox4.Text = selectedRow.Description;
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Department Details
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Show doctor in department (only show doctor with their department)
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Department name
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            // Department code
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            // Description of department
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            // Doctor in department that have been selected in searchLookUpEdit1
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // Appointment in department that have been selected in searchLookUpEdit1
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            // disease in each department that have been selected in searchLookUpEdit1
        }

        private void chartControl1_Click(object sender, EventArgs e)
        {
            // View the chart of department of patients in each department currently have
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Check if the department text boxes are filled
            if (!string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox8.Text) && !string.IsNullOrWhiteSpace(textBox4.Text))
            {
                // Add new department
                var departmentDto = new DepartmentDTO
                {
                    DepartmentId = Guid.NewGuid(),
                    DepartmentName = textBox1.Text,
                    DepartmentCode = textBox8.Text,
                    Description = textBox4.Text
                };

                try
                {
                    await _doctorService.AddDepartmentAsync(departmentDto);
                    MessageBox.Show("Department added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox1.Clear();
                    textBox8.Clear();
                    textBox4.Clear();
                    LoadDepartmentData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add department: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (!string.IsNullOrWhiteSpace(textBox6.Text) && searchLookUpEdit2.EditValue != null)
            {
                // Add new disease to the selected department
                var selectedDepartmentId = (Guid)searchLookUpEdit2.EditValue;
                var diseaseName = textBox6.Text;

                try
                {
                    var diseaseDto = new DiseaseDTO
                    {
                        DiseaseId = Guid.NewGuid(),
                        DiseaseName = diseaseName,
                        DepartmentId = selectedDepartmentId
                    };

                    await _doctorService.AddDiseaseToDepartmentAsync(diseaseDto);
                    MessageBox.Show("Disease added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    textBox6.Clear();
                    LoadDepartmentData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add disease: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please fill in the required fields.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedValue != null)
            {
                // Delete disease
                var selectedDiseaseId = (Guid)comboBox2.SelectedValue;

                try
                {
                    await _doctorService.DeleteDiseaseAsync(selectedDiseaseId);
                    MessageBox.Show("Disease deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDepartmentData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete disease: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0].DataBoundItem as dynamic;
                var selectedDepartmentId = selectedRow.DepartmentId;

                // Check if there are doctors in the department
                var doctors = await _doctorService.GetDoctorsByDepartmentAsync(selectedDepartmentId);
                if (doctors.Count > 0) // Check if the list is not empty
                {
                    MessageBox.Show("Cannot delete department with doctors in it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Delete department and associated diseases
                try
                {
                    await _doctorService.DeleteDepartmentAndDiseasesAsync(selectedDepartmentId);
                    MessageBox.Show("Department and associated diseases deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDepartmentData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete department: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a department or disease to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Clear all of the fields
            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            textBox5.Clear();
            textBox6.Clear();
            textBox8.Clear();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            // Disease name
        }

        private void InitializeSearchLookUpEdit()
        {
            searchLookUpEdit1.Properties.DisplayMember = "DepartmentName";
            searchLookUpEdit1.Properties.ValueMember = "DepartmentId";

            var view = searchLookUpEdit1.Properties.View;
            view.Columns.Clear();
            view.Columns.AddVisible("DepartmentName", "Department Name");
            view.Columns.AddVisible("DepartmentCode", "Department Code");

            searchLookUpEdit1.EditValueChanged += searchLookUpEdit1_EditValueChanged;
        }

        private void InitializeSearchLookUpEdit2()
        {
            searchLookUpEdit2.Properties.DisplayMember = "DepartmentName";
            searchLookUpEdit2.Properties.ValueMember = "DepartmentId";

            var view = searchLookUpEdit2.Properties.View;
            view.Columns.Clear();
            view.Columns.AddVisible("DepartmentName", "Department Name");
            view.Columns.AddVisible("DepartmentCode", "Department Code");

            searchLookUpEdit2.EditValueChanged += searchLookUpEdit2_EditValueChanged;
        }

        private async void searchLookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {
            if (searchLookUpEdit1.EditValue == null)
            {
                return;
            }

            var selectedDepartmentId = (Guid)searchLookUpEdit1.EditValue;
            var departments = await _doctorService.GetDepartmentsAsync();
            var selectedDepartment = departments.FirstOrDefault(d => d.DepartmentId == selectedDepartmentId);

            if (selectedDepartment != null)
            {
                var doctors = await _doctorService.GetDoctorsByDepartmentAsync(selectedDepartmentId);
                var appointments = await _appointmentService.GetAppointmentsByDepartmentAsync(selectedDepartmentId);
                var diseases = await _doctorService.GetDiseasesByDepartmentAsync(selectedDepartmentId);

                textBox3.Text = doctors.Count().ToString();
                textBox2.Text = appointments.Count().ToString();
                textBox5.Text = diseases.Count().ToString();
            }
        }

        private async void searchLookUpEdit2_EditValueChanged(object sender, EventArgs e)
        {
            if (searchLookUpEdit2.EditValue == null)
            {
                return;
            }

            var selectedDepartmentId = (Guid)searchLookUpEdit2.EditValue;
            var departments = await _doctorService.GetDepartmentsAsync();
            var selectedDepartment = departments.FirstOrDefault(d => d.DepartmentId == selectedDepartmentId);

            if (selectedDepartment != null)
            {
                var diseases = await _doctorService.GetDiseasesByDepartmentAsync(selectedDepartmentId);
                var diseaseDetails = diseases.Select(d => new
                {
                    d.DiseaseName,
                    d.DiseaseId
                }).ToList();

                comboBox2.DataSource = diseaseDetails;
                comboBox2.DisplayMember = "DiseaseName";
                comboBox2.ValueMember = "DiseaseId";
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

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

        

        private void label20_Click(object sender, EventArgs e)
        {

        }
    }
}
