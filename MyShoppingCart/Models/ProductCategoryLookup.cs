using MyShoppingCart.Data.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class ProductCategoryLookup : IEntityBase
    {
        [Key]
        public int Id { get; set; }

        public int ParentCategoryId { get; set; }

        [Display(Name = "New Category Name")]
        public string CategoryName { get; set; }
        public IList<ProductCategory> productCategory { get; set; }
    }
}
