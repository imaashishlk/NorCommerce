using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace NorCommerce.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        [Required]
        public int ProductID { get; set; }

        [ValidateNever]
        [ForeignKey("ProductID")]
        public Product Product { get; set; }

        [Range(1, 100, ErrorMessage = "Please enter a value between 1 and 100")]
        public int Count { get; set; }

        [Required]
        public string ApplicationUserId { get; set; }

        [ValidateNever]
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser ApplicationUser { get; set; }

        [NotMapped]
        public double Price { 
            get
            {
                if (Product == null)
                {
                    return 0;
                }

                if (Count < 50)
                {
                    return Product.Price;
                }
                else if (Count < 100)
                {
                    return Product.Price50;
                }
                else
                {
                    return Product.Price100;
                }
            }
        }
    }
}
