using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Infrastructure.Tripletex
{
    public class TripletexOptions
    {
        public string BaseUrl { get; set; } = null!;
        public string ConsumerToken { get; set; } = null!;
        public string EmployeeToken { get; set; } = null!;
    }
}
