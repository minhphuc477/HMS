using BusinessLL;
using DataTransferO;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMS_CHANGE.Admin
{
    public partial class AccountAdmin : Form
    {
        private readonly PaymentAndBillingService _paymentAndBillingService;
        private readonly IServiceProvider _serviceProvider;
        private UserDTO _currentUser;

        public AccountAdmin(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _paymentAndBillingService = _serviceProvider.GetRequiredService<PaymentAndBillingService>();
        }

        public void SetUserData(UserDTO user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            label2.Text = $"Welcome, {_currentUser.Name}";

            // Initialize DataGridView columns
            InitializeProductDataGridView();

            // Load data into DataGridView
            LoadProducts();

            // Load data into searchLookUpEdit
            LoadSearchLookUpEdit();
        }

        private void InitializeProductDataGridView()
        {
            dataGridView2.Columns.Clear();

            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ProductId",
                HeaderText = "Product ID",
                DataPropertyName = "ProductId",
                Visible = false // Hide the column if you don't want it to be visible
            });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Name",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Description",
                HeaderText = "Description",
                DataPropertyName = "Description",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Price",
                HeaderText = "Price",
                DataPropertyName = "Price",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView2.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Stock",
                HeaderText = "Stock",
                DataPropertyName = "Stock",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView2.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "RequiresPrescription",
                HeaderText = "Requires Prescription",
                DataPropertyName = "RequiresPrescription",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        private async void LoadProducts()
        {
            try
            {
                var products = await _paymentAndBillingService.GetAllProductsAsync();
                dataGridView2.DataSource = products.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load products: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadSearchLookUpEdit()
        {
            try
            {
                var products = await _paymentAndBillingService.GetAllProductsAsync();
                searchLookUpEdit1.Properties.DataSource = products.ToList();
                searchLookUpEdit1.Properties.DisplayMember = "Name";
                searchLookUpEdit1.Properties.ValueMember = "ProductId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load products for search: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedRow = dataGridView2.Rows[e.RowIndex].DataBoundItem as PharmacyProductDTO;
                if (selectedRow != null)
                {
                    textBox6.Text = selectedRow.Name;
                    textBox5.Text = selectedRow.Description;
                    textBox7.Text = selectedRow.Price.ToString();
                    textBox1.Text = selectedRow.Stock.ToString();
                    comboBox1.SelectedItem = selectedRow.RequiresPrescription == true ? "Yes" : "No";
                }
            }
        }

        private void panel8_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            // name of product
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            // description of product
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            //price of product
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //stock of product
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // require prescription of product
        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (searchLookUpEdit1.EditValue != null)
            {
                // Add stock to the selected product
                Guid selectedProductId = (Guid)searchLookUpEdit1.EditValue;
                var product = await _paymentAndBillingService.GetProductByIdAsync(selectedProductId);

                if (product != null)
                {
                    if (int.TryParse(textBox2.Text, out int additionalStock))
                    {
                        product.Stock += additionalStock;

                        try
                        {
                            await _paymentAndBillingService.UpdatePharmacyProductAsync(product);
                            MessageBox.Show("Stock updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadProducts();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to update stock: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid stock quantity.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Selected product not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Create a new pharmacy product
                var productDto = new PharmacyProductDTO
                {
                    Name = textBox6.Text,
                    Description = textBox5.Text,
                    Price = decimal.Parse(textBox7.Text),
                    Stock = int.Parse(textBox1.Text),
                    RequiresPrescription = comboBox1.SelectedItem.ToString() == "Yes"
                };

                try
                {
                    await _paymentAndBillingService.CreatePharmacyProductAsync(productDto);
                    MessageBox.Show("Product created successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            // Edit the selected pharmacy product
            if (dataGridView2.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView2.SelectedRows[0].DataBoundItem as PharmacyProductDTO;
                if (selectedRow != null)
                {
                    selectedRow.Name = textBox6.Text;
                    selectedRow.Description = textBox5.Text;
                    selectedRow.Price = decimal.Parse(textBox7.Text);
                    selectedRow.Stock = int.Parse(textBox1.Text);
                    selectedRow.RequiresPrescription = comboBox1.SelectedItem.ToString() == "Yes";

                    try
                    {
                        await _paymentAndBillingService.UpdatePharmacyProductAsync(selectedRow);
                        MessageBox.Show("Product updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadProducts();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to update product: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a product to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void searchLookUpEdit1_EditValueChanged(object sender, EventArgs e)
        {
            if (searchLookUpEdit1.EditValue == null)
            {
                MessageBox.Show("Please select a product from the list.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Guid selectedProductId = (Guid)searchLookUpEdit1.EditValue;
            var product = await _paymentAndBillingService.GetProductByIdAsync(selectedProductId);

            if (product != null)
            {
                textBox6.Text = product.Name;
                textBox5.Text = product.Description;
                textBox7.Text = product.Price.ToString();
                textBox1.Text = product.Stock.ToString();
                comboBox1.SelectedItem = product.RequiresPrescription == true ? "Yes" : "No";
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            // add stock into product
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // clear all of the text boxes and combo box
            textBox6.Clear();
            textBox5.Clear();
            textBox7.Clear();
            textBox1.Clear();
            textBox2.Clear();
            comboBox1.SelectedIndex = -1;
        }
    }
}
