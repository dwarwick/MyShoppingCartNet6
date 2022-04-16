using MyShoppingCart.Data.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class ProductRating : IEntityBase
    {
        public int Id { get; set; }
        [Required]
        [Range(1,5,ErrorMessage = "Rating must be an integer between 1 and 5.")]
        public int Rating { get; set; }
        public Product product { get; set; }
        public DateTime Date { get; set; }
        public ApplicationUser applicationUser { get; set; }
        
        [Required]
        [MinLength(10,ErrorMessage = " The Review must be at least 10 characters in length.")]
        [Display(Name = "Review Text")]
        
        public string ReviewText { get; set; }
    }
}
