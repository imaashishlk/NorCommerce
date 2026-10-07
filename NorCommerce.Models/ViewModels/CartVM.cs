using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NorCommerce.Models.ViewModels
{
    public class CartVM
    {
        public IEnumerable<ShoppingCart> CartItems { get; set; }

        public OrderHeader OrderHeader { get; set; }

        [NotMapped]
        public decimal CartTotal { get; set; }
    }
}
