using System.ComponentModel.DataAnnotations;

namespace NorCommerce.Models
{
    public class Category
    {
        public int  Id { get; set; }

        [Display(Name = "Category Name")]
        [Required(ErrorMessage = "Name field is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Display(Name = "Display Order")]
        [Required(ErrorMessage = "Display order is required.")]
        [Range(0, 100, ErrorMessage = "Display Order must be between 0 and 100.")]
        public int DisplayOrder { get; set; }
    }
}
