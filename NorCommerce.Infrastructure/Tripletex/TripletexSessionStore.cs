using NorCommerce.DataAccess.Seed;
using System;
using System.Collections.Generic;
using System.Text;

namespace NorCommerce.Infrastructure.Tripletex
{
    public class TripletexSessionStore
    {
        public string? Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int? StandardVatTypeId { get; set; }

        public bool IsValid()
        {
            return Token != null && DateTime.Now < ExpiresAt;
        }
    }
}
