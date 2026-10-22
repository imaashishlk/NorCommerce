using NorCommerce.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Business.Services.IServices
{
    public interface IAttachmentEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage, string? fileName, byte[]? file);
    }
}
