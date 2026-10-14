using NorCommerce.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NorCommerce.Business.Emails
{
    public class EmailTemplates
    {
        private const string Navy = "#13294B";
        private const string DarkNavy = "#0C1C33";
        private const string Coral = "#FF6B6B";
        private const string Red = "#C8102E";
        private const string Background = "#F4F6F9";
        private const string Text = "#111827";
        private const string Muted = "#6b7280";
        private const string Border = "#e5e7eb";


        // Common Email Templates
        private static string Layout(string title, string body)
        {
            return $@"
            <div style=""background:{Background};padding:24px;font-family:Arial,sans-serif;"">
              <table width=""100%"" style=""max-width:600px;margin:auto;background:#ffffff;border-radius:8px;border-collapse:collapse;overflow:hidden;"">
                <tr>
                  <td style=""background:{Navy};padding:20px 24px;font-size:22px;font-weight:bold;color:#ffffff;"">
                    Nor<span style=""color:{Coral};"">Commerce</span>
                  </td>
                </tr>
                <tr>
                  <td style=""padding:24px;color:{Text};font-size:15px;line-height:1.6;"">
                    <h2 style=""margin:0 0 16px;font-size:20px;color:{Navy};"">{title}</h2>
                    {body}
                  </td>
                </tr>
                <tr>
                  <td style=""background:{DarkNavy};padding:16px 24px;color:#9ca3af;font-size:12px;"">
                    <strong style=""color:#ffffff;"">NorCommerce</strong> &middot; Alt du trenger<br/>
                    This is an automatic email. Please do not reply.
                  </td>
                </tr>
              </table>
            </div>";
        }

        public static string OrderConfirmation(OrderHeader order, IEnumerable<OrderDetails> items)
        {
            var rows = new StringBuilder();

            foreach (var item in items)
            {
                rows.Append($@"
                    <tr>
                      <td style=""padding:8px 0;border-bottom:1px solid {Border};"">{WebUtility.HtmlEncode(item.Product.Title)} x {item.Count}</td>
                      <td style=""padding:8px 0;border-bottom:1px solid {Border};text-align:right;"">{(item.Price * item.Count):C}</td>
                    </tr>");
            }

            var body = $@"
                <p>Hi {WebUtility.HtmlEncode(order.Name)},</p>
                <p>Thank you for your order. We have received it and will start processing it soon.</p>

                <p style=""margin:16px 0 8px;""><strong>Order #{order.Id}</strong><br/>
                <span style=""color:{Muted};"">{order.OrderDate:dd.MM.yyyy HH:mm}</span></p>

                <table width=""100%"" style=""border-collapse:collapse;font-size:14px;"">
                  {rows}
                  <tr>
                    <td style=""padding:12px 0;font-weight:bold;"">Total</td>
                    <td style=""padding:12px 0;font-weight:bold;text-align:right;color:{Red};"">{order.OrderTotal:C}</td>
                  </tr>
                </table>

                <p style=""margin-top:16px;"">We will send you another email when your order ships.</p>";

            return Layout("Order confirmed", body);
        }

        public static string OrderCancelledRefunded(OrderHeader order, IEnumerable<OrderDetails> items)
        {
            var rows = new StringBuilder();

            foreach (var item in items)
            {
                rows.Append($@"
                    <tr>
                      <td style=""padding:8px 0;border-bottom:1px solid {Border};"">{WebUtility.HtmlEncode(item.Product.Title)} x {item.Count}</td>
                      <td style=""padding:8px 0;border-bottom:1px solid {Border};text-align:right;"">{(item.Price * item.Count):C}</td>
                    </tr>");
            }

            var body = $@"
                <p>Hi {WebUtility.HtmlEncode(order.Name)},</p>
                <p>Your order has been cancelled and a refund has been initiated. Thank you for shopping with us.</p>

                <p style=""margin:16px 0 8px;""><strong>Order #{order.Id}</strong><br/>
                <span style=""color:{Muted};"">{order.OrderDate:dd.MM.yyyy HH:mm}</span></p>

                <table width=""100%"" style=""border-collapse:collapse;font-size:14px;"">
                  {rows}
                  <tr>
                    <td style=""padding:12px 0;font-weight:bold;"">Total</td>
                    <td style=""padding:12px 0;font-weight:bold;text-align:right;color:{Red};"">{order.OrderTotal:C}</td>
                  </tr>
                </table>

                <p style=""margin-top:16px;"">We will send you another email when your order ships.</p>";

            return Layout("Order confirmed", body);
        }

        public static string OrderShipped(OrderHeader order)
        {
            var body = $@"
                <p>Hi {WebUtility.HtmlEncode(order.Name)},</p>
                <p>Your order <strong>#{order.Id}</strong> is on its way.</p>
                <table style=""font-size:14px;margin-top:8px;"">
                  <tr><td style=""color:{Muted};padding-right:16px;"">Carrier</td><td>{WebUtility.HtmlEncode(order.Carrier)}</td></tr>
                  <tr><td style=""color:{Muted};padding-right:16px;"">Tracking</td><td>{WebUtility.HtmlEncode(order.TrackingNumber)}</td></tr>
                </table>";

            return Layout("Your order has shipped", body);
        }
    }
}