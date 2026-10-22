using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Business.Services.IServices
{
    public interface IAccountingClient
    {
        Task<string> PingAsync();
        Task<int?> FindCustomerIdByEmailAsync(string email);
        Task<int> CreateCustomerAsync(string name, string email);
        Task<int> GetStandardVatTypeIdAsync();
        Task<int> CreateOrderAsync(int customerId, DateOnly orderDate, List<AccountingOrderLine> lines);
        Task<AccountingInvoice> InvoiceOrderAsync(int orderId, DateOnly invoiceDate);
        Task<byte[]> GetInvoicePdfAsync(int invoiceId);
    }

    public record AccountingOrderLine(string Description, decimal Quantity, decimal UnitPriceInclVat);
    public record AccountingInvoice(int Id, int InvoiceNumber);
}
