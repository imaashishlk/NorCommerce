using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NorCommerce.Business.Emails;
using NorCommerce.Business.Services.IServices;
using NorCommerce.DataAccess.Data;
using NorCommerce.Models;
using NorCommerce.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NorCommerce.Business.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly IAccountingClient _accountingClient;
        private readonly ApplicationDbContext _context;
        private readonly IAttachmentEmailSender _email;
        private readonly IConfiguration _config;

        public AccountingService(IAccountingClient client, ApplicationDbContext context, IAttachmentEmailSender email, IConfiguration config)
        {
            _accountingClient = client;
            _context = context;
            _email = email;
            _config = config;
        }

        public async Task<int> GetOrCreateCustomerAsync(ApplicationUser user)
        {
            // Checking the user is already populated with tripletex 
            if (user.TripletexCustomerId != null)
                return user.TripletexCustomerId.Value;

            // Find or create
            var id = await _accountingClient.FindCustomerIdByEmailAsync(user.Email!)
                     ?? await _accountingClient.CreateCustomerAsync(user.Name, user.Email!);

            // Save the response id
            user.TripletexCustomerId = id;
            await _context.SaveChangesAsync();

            return id;
        }

        public async Task<List<int>> GetPendingOrderIdsAsync()
        {
            var syncFrom = DateTime.Parse(_config["Accounting:SyncFromDate"]!);

            return await _context.OrderHeaders
                .Where(o => o.PaymentStatus == StaticDetails.PaymentStatusApproved)   // paid only
                .Where(o => o.OrderDate >= syncFrom)                                  // skip old orders
                .Where(o => o.AccountBookInvoiceId == null || !o.InvoiceEmailSent)    // not finished
                .OrderBy(o => o.Id)
                .Select(o => o.Id)
                .Take(20)                                                             // max 20 per round
                .ToListAsync();
        }

        public async Task SyncOrderAsync(int orderHeaderId)
        {
            // Load the order + its user
            var order = await _context.OrderHeaders
                .Include(o => o.ApplicationUser)
                .FirstAsync(o => o.Id == orderHeaderId);

            // Already invoiced? Nothing to do.
            if (order.AccountBookInvoiceId != null && order.InvoiceEmailSent)
                return;

            // Fetching Customer
            var customerId = await GetOrCreateCustomerAsync(order.ApplicationUser);

            // Order (only if not created yet)
            if (order.AccountBookOrderId == null)
            {
                var lines = await _context.OrderDetails
                    .Where(d => d.OrderHeaderId == order.Id)
                    .Select(d => new AccountingOrderLine(
                        d.Product.Title,
                        d.Count,
                        (decimal)d.Price))
                    .ToListAsync();

                var orderDate = DateOnly.FromDateTime(order.OrderDate);

                order.AccountBookOrderId = await _accountingClient.CreateOrderAsync(customerId, orderDate, lines);
                await _context.SaveChangesAsync(); // save right away
            }

            // Invoice
            if (order.AccountBookInvoiceId == null)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var invoice = await _accountingClient.InvoiceOrderAsync(order.AccountBookOrderId.Value, today);

                order.AccountBookInvoiceId = invoice.Id;
                order.InvoiceNumber = invoice.InvoiceNumber;
                await _context.SaveChangesAsync();
            }

            // Email 
            if (!order.InvoiceEmailSent)
            {
                var pdf = await _accountingClient.GetInvoicePdfAsync(order.AccountBookInvoiceId.Value);
                var invoiceNo = order.InvoiceNumber!.Value;

                await _email.SendEmailAsync(
                    order.ApplicationUser.Email!,
                    $"Faktura #{invoiceNo} fra NorCommerce",
                    EmailTemplates.InvoiceReady(order),
                    $"Faktura-{invoiceNo}.pdf",
                    pdf);

                order.InvoiceEmailSent = true;
                await _context.SaveChangesAsync();
            }
        }


    }
}

