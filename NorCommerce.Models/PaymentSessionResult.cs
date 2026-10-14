using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Models
{
    public class PaymentSessionResult
    {
        public string SessionId { get; set; } = null!;
        public string Url { get; set; } = null!;
    }
}
