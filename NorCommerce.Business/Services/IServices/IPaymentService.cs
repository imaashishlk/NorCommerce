using NorCommerce.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Business.Services.IServices
{
    public interface IPaymentService
    {
        Task<PaymentSessionResult> CreateCheckoutSessionAsync(OrderHeader order, string successUrl, string cancelUrl);
        Task<(bool IsPaid, string? PaymentIntentId)> GetPaymentStatusAsync(string sessionId);
        Task<(string RefundId, string Status)> RefundAsync(string paymentIntentId);
    }
}
