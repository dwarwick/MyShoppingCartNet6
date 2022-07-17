using MyShoppingCart.Data.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class Product : IEntityBase
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }

        [Range(0.0, 100000000.0)]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]        
        public decimal Price { get; set; }

        [Required]
        public bool Enabled { get; set; } = true;

        
        public ApplicationUser applicationUser { get; set; }

        public List<ProductImage> productImages { get; set; }
        public double Rating { get; set; } = 0;
        public int NumberOfReviews { get; set; } = 0;

        public IList<ProductCategory> productCategory { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "Length must be at least 0 inches")]
        public decimal length { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "width must be at least 0 inches")]
        public decimal width { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "height must be at least 0 inches")]
        public decimal height { get; set; } = 0;
                
    }
}
