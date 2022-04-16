using MyShoppingCart.Data.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class ProductImage : IEntityBase
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageURL { get; set; }
        public string ImageName { get; set; }

        [Display(Name = "Image Description")]
        [StringLength(25, ErrorMessage = "Maximum length of description: 25 charcters")]
        public string ImageDescription { get; set; }

        [ForeignKey("ProductId")]
        public Product product { get; set; }

    }
}
