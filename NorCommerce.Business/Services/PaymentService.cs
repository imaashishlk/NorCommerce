using NorCommerce.Business.Services.IServices;
using NorCommerce.Models;
using Stripe.Checkout;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Business.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly SessionService _sessionService;
        private readonly RefundService _refundService;

        public PaymentService(SessionService sessionService, RefundService refundService)
        {
            _sessionService = sessionService;
            _refundService = refundService;
        }
        public async Task<PaymentSessionResult> CreateCheckoutSessionAsync(OrderHeader order, string successUrl, string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                ClientReferenceId = order.Id.ToString(),
                LineItems = order.OrderDetails.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)Math.Round(item.Price * 100),   // double kr to øre
                        Currency = "nok",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                }).ToList()
            };

            var session = await _sessionService.CreateAsync(options);
            return new PaymentSessionResult { SessionId = session.Id, Url = session.Url };
        }

        public async Task<(bool IsPaid, string? PaymentIntentId)> GetPaymentStatusAsync(string sessionId)
        {
            var session = await _sessionService.GetAsync(sessionId);
            return (session.PaymentStatus == "paid", session.PaymentIntentId);
        }

        public async Task<(string RefundId, string Status)> RefundAsync(string paymentIntentId)
        {
            var refund = await _refundService.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Reason = RefundReasons.RequestedByCustomer
            });

            return (refund.Id, refund.Status);
        }
    }
}
