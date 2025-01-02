using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BusinessLL
{
    public interface ISePayService
    {
        Task<TransactionDetails?> GetTransactionDetailsAsync(string transactionId);
        Task<List<TransactionDetails>> GetTransactionListAsync(string? accountNumber = null, DateTime? transactionDateMin = null, DateTime? transactionDateMax = null, int? limit = null);
        Task<int> GetTransactionCountAsync(string? accountNumber = null, DateTime? transactionDateMin = null, DateTime? transactionDateMax = null);
        Task<List<TransactionDetails>> FindMatchingTransactionsAsync(string userName, decimal amount, string note, string ttCode);
    }

    public class SePayService : ISePayService
    {
        private static readonly HttpClient client = new HttpClient();

        static SePayService()
        {
            client.BaseAddress = new Uri("https://my.sepay.vn/userapi/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_ACCESS_TOKEN"); // Replace with your actual token
        }

        public async Task<TransactionDetails?> GetTransactionDetailsAsync(string transactionId)
        {
            var response = await client.GetAsync($"transactions/details/{transactionId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var transactionDetails = JsonConvert.DeserializeObject<TransactionDetailsResponse>(content);
                return transactionDetails?.Transaction;
            }
            return null;
        }

        public async Task<List<TransactionDetails>> GetTransactionListAsync(string? accountNumber = null, DateTime? transactionDateMin = null, DateTime? transactionDateMax = null, int? limit = null)
        {
            var queryParameters = new List<string>();
            if (!string.IsNullOrEmpty(accountNumber))
                queryParameters.Add($"account_number={accountNumber}");
            if (transactionDateMin.HasValue)
                queryParameters.Add($"transaction_date_min={transactionDateMin.Value:yyyy-MM-dd HH:mm:ss}");
            if (transactionDateMax.HasValue)
                queryParameters.Add($"transaction_date_max={transactionDateMax.Value:yyyy-MM-dd HH:mm:ss}");
            if (limit.HasValue)
                queryParameters.Add($"limit={limit.Value}");

            var queryString = queryParameters.Count > 0 ? "?" + string.Join("&", queryParameters) : string.Empty;
            var response = await client.GetAsync($"transactions/list{queryString}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var transactionListResponse = JsonConvert.DeserializeObject<TransactionListResponse>(content);
                return transactionListResponse?.Transactions ?? new List<TransactionDetails>();
            }
            return new List<TransactionDetails>();
        }

        public async Task<int> GetTransactionCountAsync(string? accountNumber = null, DateTime? transactionDateMin = null, DateTime? transactionDateMax = null)
        {
            var queryParameters = new List<string>();
            if (!string.IsNullOrEmpty(accountNumber))
                queryParameters.Add($"account_number={accountNumber}");
            if (transactionDateMin.HasValue)
                queryParameters.Add($"transaction_date_min={transactionDateMin.Value:yyyy-MM-dd}");
            if (transactionDateMax.HasValue)
                queryParameters.Add($"transaction_date_max={transactionDateMax.Value:yyyy-MM-dd}");

            var queryString = queryParameters.Count > 0 ? "?" + string.Join("&", queryParameters) : string.Empty;
            var response = await client.GetAsync($"transactions/count{queryString}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var countResponse = JsonConvert.DeserializeObject<CountTransactionResponse>(content);
                return countResponse?.CountTransactions ?? 0;
            }
            return 0;
        }

        /// <summary>
        /// Finds transactions matching the specified criteria.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="amount">Amount of the transaction.</param>
        /// <param name="note">Note associated with the transaction.</param>
        /// <param name="ttCode">TT code of the transaction.</param>
        /// <returns>List of matching transactions.</returns>
        public async Task<List<TransactionDetails>> FindMatchingTransactionsAsync(string userName, decimal amount, string note, string ttCode)
        {
            var transactions = await GetTransactionListAsync();
            var matchingTransactions = new List<TransactionDetails>();

            foreach (var transaction in transactions)
            {
                bool isMatch =
                    (transaction.TransactionContent.Contains(userName, StringComparison.OrdinalIgnoreCase) ||
                    transaction.AmountIn == amount.ToString("F2") ||
                    transaction.TransactionContent.Contains(note, StringComparison.OrdinalIgnoreCase) ||
                    transaction.ReferenceNumber.Equals(ttCode, StringComparison.OrdinalIgnoreCase));

                if (isMatch)
                {
                    matchingTransactions.Add(transaction);
                }
            }

            return matchingTransactions;
        }

        /// <summary>
        /// Continuously monitors transactions and triggers an action when a matching transaction is found.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="amount">Amount to match.</param>
        /// <param name="note">Note to match.</param>
        /// <param name="ttCode">TT code to match.</param>
        /// <param name="cancellationToken">Cancellation token to stop monitoring.</param>
        /// <returns>Matching transaction if found; otherwise, null.</returns>
        public async Task<TransactionDetails?> MonitorForTransactionAsync(string userName, decimal amount, string note, string ttCode, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var matches = await FindMatchingTransactionsAsync(userName, amount, note, ttCode);
                if (matches.Count > 0)
                {
                    return matches.First();
                }

                // Wait for a specified interval before checking again
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            }

            return null;
        }
    }

    // DTO Classes
    public class TransactionListResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("messages")]
        public Messages Messages { get; set; }

        [JsonProperty("transactions")]
        public List<TransactionDetails> Transactions { get; set; }
    }

    public class CountTransactionResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("messages")]
        public Messages Messages { get; set; }

        [JsonProperty("count_transactions")]
        public int CountTransactions { get; set; }
    }

    public class TransactionDetailsResponse
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("error")]
        public string? Error { get; set; }

        [JsonProperty("messages")]
        public Messages Messages { get; set; }

        [JsonProperty("transaction")]
        public TransactionDetails? Transaction { get; set; }
    }

    public class Messages
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
    }

    public class TransactionDetails
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("transaction_date")]
        public string TransactionDate { get; set; }

        [JsonProperty("account_number")]
        public string AccountNumber { get; set; }

        [JsonProperty("sub_account")]
        public string SubAccount { get; set; }

        [JsonProperty("amount_in")]
        public string AmountIn { get; set; }

        [JsonProperty("amount_out")]
        public string AmountOut { get; set; }

        [JsonProperty("accumulated")]
        public string Accumulated { get; set; }

        [JsonProperty("transaction_content")]
        public string TransactionContent { get; set; }

        [JsonProperty("reference_number")]
        public string ReferenceNumber { get; set; }

        [JsonProperty("bank_brand_name")]
        public string BankBrandName { get; set; }

        [JsonProperty("bank_account_id")]
        public string BankAccountId { get; set; }
    }
}
