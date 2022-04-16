using MyShoppingCart.Models;
using System.Collections.Generic;

namespace MyShoppingCart.Data.ViewModels
{
    public class EditCategoriesVM
    {
        public List<ProductCategoryLookup> lstProductCategoryLookup { get; set; }
        public ProductCategoryLookup ProductCategoryLookup { get; set; }
    }
}
