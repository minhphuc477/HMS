using BusinessLL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HMS_CHANGE.Patient.Account_Setting
{
    public partial class PaymentByApp : Form
    {
        private readonly Guid _paymentId;
        private readonly decimal _amount;
        private readonly SePayService _sePayService;

        public PaymentByApp(Guid paymentId, decimal amount, SePayService sePayService)
        {
            InitializeComponent();
            _paymentId = paymentId;
            _amount = amount;
            _sePayService = sePayService;
        }

        private async void PaymentByApp_Load(object sender, EventArgs e)
        {
            // Display payment details
            labelAmount.Text = $"Amount: {_amount}";
            labelPaymentId.Text = $"Payment ID: {_paymentId}";

            // Process payment
            bool paymentSuccess = await ProcessPaymentAsync();

            if (paymentSuccess)
            {
                MessageBox.Show("Payment successful.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Update payment status in the database
                // You can call a method to update the payment status here
            }
            else
            {
                MessageBox.Show("Payment failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<bool> ProcessPaymentAsync()
        {
            // Simulate payment processing delay
            await Task.Delay(2000);

            // Get transaction details from SePay API
            var transactionDetails = await _sePayService.GetTransactionDetailsAsync(_paymentId.ToString());

            if (transactionDetails != null && transactionDetails.AmountIn == _amount.ToString())
            {
                // Payment was successful
                return true;
            }

            // Payment failed
            return false;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }
    }
    public interface ISePayService
    {
        Task<TransactionDetails?> GetTransactionDetailsAsync(string transactionId);
        Task<List<TransactionDetails>> GetTransactionListAsync(string? accountNumber = null, DateTime? transactionDateMin = null, DateTime? transactionDateMax = null, int? limit = null);
        Task<int> GetTransactionCountAsync(string? accountNumber = null, DateTime? transactionDateMin = null, DateTime? transactionDateMax = null);
    }

  
}
