using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayoutsJobConsoleApp.Models
{
    public class Product 
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

        [Required]
        public ApplicationUser applicationUser { get; set; }

        public List<ProductImage> productImages { get; set; }
        public double Rating { get; set; } = 0;
        public int NumberOfReviews { get; set; } = 0;
    }
}
