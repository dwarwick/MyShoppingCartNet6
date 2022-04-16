using MyShoppingCart.Data.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class ProductCategory : IEntityBase
    {
        [Key]
        public int Id { get; set ; }
        
        public int ProductId { get; set; }
        public Product product { get; set; }
        public int ProductCategoryLookupId { get; set; }
        public ProductCategoryLookup productCategoryLookup { get; set; }
    }
}
