using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NorCommerce.Business.Services.IServices;
using Stripe;
using Stripe.Climate;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorCommerce.Infrastructure.Tripletex
{
    public class TripletexClient : IAccountingClient
    {
        private readonly HttpClient _http;
        private readonly TripletexOptions _txOptions;
        private TripletexSessionStore _session;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public TripletexClient(HttpClient http, IOptions<TripletexOptions> txOptions, TripletexSessionStore session)
        {
            _http = http;
            _txOptions = txOptions.Value;
            _http.BaseAddress = new Uri(_txOptions.BaseUrl);
            _session = session;
        }

        public async Task IsLoggedInAsync()
        {
            if (!_session.IsValid()) await LoginAsync();

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"0:{_session.Token}"));
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        }

        // Getting session token
        private async Task LoginAsync()
        {
            var consumer = Uri.EscapeDataString(_txOptions.ConsumerToken);
            var employee = Uri.EscapeDataString(_txOptions.EmployeeToken);
            var expires = DateTime.Today.AddDays(2).ToString("yyyy-MM-dd");

            var url = $"token/session/:create?consumerToken={consumer}&employeeToken={employee}&expirationDate={expires}";

            var response = await _http.PutAsync(url, null);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TxSingle<SessionToken>>();
            _session.Token = result!.Value.Token;
            // Setting expiration to 2 days, but asking new token well in advance, before expiration of a day
            _session.ExpiresAt = result!.Value.ExpirationDate.AddDays(-1);
        }

        // Health check <Whoami>
        public async Task<string> PingAsync()  
        {
            if (!_session.IsValid()) await LoginAsync();

            var response = await _http.GetAsync("token/session/>whoAmI");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<int?> FindCustomerIdByEmailAsync(string email)
        {
            await IsLoggedInAsync();

            var url = $"customer?email={Uri.EscapeDataString(email)}&fields=id,name,email";
            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TxList<TxCustomer>>();
            return result!.Values.FirstOrDefault()?.Id;
        }

        public async Task<int> CreateCustomerAsync(string name, string email)
        {
            await IsLoggedInAsync();

            var customer = new TxCustomer(null, name, email, email, true);

            var response = await _http.PostAsJsonAsync("customer", customer, JsonOptions);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TxSingle<TxCustomer>>();
            return result!.Value.Id!.Value;
        }

        // VAT
        public async Task<int> GetStandardVatTypeIdAsync()
        {
            // Already looked up? Use it.
            if (_session.StandardVatTypeId != null)
                return _session.StandardVatTypeId.Value;

            await IsLoggedInAsync();

            var response = await _http.GetAsync("ledger/vatType?number=3&fields=id,number,name,percentage");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TxList<TxVatType>>();
            var vat = result!.Values.FirstOrDefault()
                      ?? throw new InvalidOperationException("Tripletex VAT type 3 (25%) not found.");

            _session.StandardVatTypeId = vat.Id;
            return vat.Id;
        }

        public async Task<int> CreateOrderAsync(int customerId, DateOnly orderDate, List<AccountingOrderLine> lines)
        {
            await IsLoggedInAsync();

            var vatId = await GetStandardVatTypeIdAsync();

            var txLines = lines
                .Select(l => new TxOrderLine(l.Description, l.Quantity, l.UnitPriceInclVat, new TxRef(vatId)))
                .ToList();

            var order = new TxOrder(
                Id: null,
                Customer: new TxRef(customerId),
                OrderDate: orderDate,
                DeliveryDate: orderDate,
                IsPrioritizeAmountsIncludingVat: true,
                OrderLines: txLines
            );

            var response = await _http.PostAsJsonAsync("order", order, JsonOptions);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TxSingle<TxOrder>>();
            return result!.Value.Id!.Value;
        }

        public async Task<AccountingInvoice> InvoiceOrderAsync(int orderId, DateOnly invoiceDate)
        {
            await IsLoggedInAsync();

            var date = invoiceDate.ToString("yyyy-MM-dd");
            var url = $"order/{orderId}/:invoice?invoiceDate={date}&sendToCustomer=false";

            var response = await _http.PutAsync(url, null);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<TxSingle<TxInvoice>>();
            return new AccountingInvoice(result!.Value.Id, result.Value.InvoiceNumber);
        }

        public async Task<byte[]> GetInvoicePdfAsync(int invoiceId)
        {
            await IsLoggedInAsync();

            var response = await _http.GetAsync($"invoice/{invoiceId}/pdf");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}
