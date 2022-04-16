using Microsoft.AspNetCore.Http;
using MyShoppingCart.Data;
using MyShoppingCart.Data.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class NewProductVM
    {
        public int Id { get; set; }

        [Display(Name = "Product Name")]
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Display(Name = "Product Description")]
        [Required(ErrorMessage = "Product Description is Required")]
        public string Description { get; set; }

        [Display(Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:C0}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Price is required")]        
        public decimal Price { get; set; }

        public bool Enabled { get; set; }

        public IFormFile File { get; set; }
        public List<IFormFile> Files { get; set; }

        public List<ProductImage> ProductImages { get; set; }

        [Required]
        public ApplicationUser applicationUser { get; set; }
    }
}
