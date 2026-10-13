using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace NorCommerce.Business.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("NorCommerce", _config["Email:From"]!));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlMessage };

            using var client = new SmtpClient();
            await client.ConnectAsync(_config["Email:Host"]!, int.Parse(_config["Email:Port"]!), SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config["Email:ApiKey"]!, _config["Email:SecretKey"]!);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
