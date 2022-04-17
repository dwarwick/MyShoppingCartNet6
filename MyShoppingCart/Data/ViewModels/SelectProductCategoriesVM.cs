using MyShoppingCart.Models;
using System.Collections.Generic;

namespace MyShoppingCart.Data.ViewModels
{
    public class SelectProductCategoriesVM
    {
        public EditCategoriesVM editCategoriesVM { get; set; }

        public List<ProductCategory> lstProductCategories { get; set; }

		public Product product { get; set; }
	}
}
